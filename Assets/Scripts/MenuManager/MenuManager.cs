using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using TMPro;
using Unity.Tutorials.Core.Editor;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using TextAsset = UnityEngine.TextAsset;

public class MenuManager : MonoBehaviour
{
    private const string BACK_BUTTON = "BackButton";

    static MenuManager _instance;
    public static MenuManager Instance => _instance;

    private Dictionary<string, GameObject> menusDictionary = new Dictionary<string, GameObject>();
    private Stack<GameObject> menusStack = new Stack<GameObject>();
    private GameObject currentMenu;
    
    private Dictionary<string, Dictionary<string,Dictionary<string, string>>> buttonDictionary = new Dictionary<string, Dictionary<string,Dictionary<string, string>>>();

    private Action _onHideAnimateComplete = null;
    [SerializeField]
    private StarterMenuCanvas _starterMenuCanvasPrefab;
    //dummy Enum
    public enum MenuEnum
    {
        LoginMenuCanvas,
    }

    /// <summary>
    /// Instantiate Menu manager as singleton
    /// </summary>
    private void Awake()
    {

        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        else
        {
            _instance = this;
        }

        // Load Menu Config
        GenerateMenuConfig();
        LoadConfigFromAssetManager();
        
        // Make MenuManager object persistent in all scene
        DontDestroyOnLoad(this.gameObject);

    }
    
    // Start is called before the first frame update
    void Start()
    {
        // init all menu prefabs
        InitMenuFromAssets();
        SetMenuDirection();
    }

    #region Generate and Load Config files from asset manager
    
    /// <summary>
    /// Load MenuConfig.json from asset manager
    /// </summary>
    private void LoadConfigFromAssetManager()
    {
        object configObj = AssetManager.Singleton.GetAsset(AssetEnum.MenuConfig);
        TextAsset config = configObj as TextAsset;
        if (config != null)
        {
            buttonDictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string,Dictionary<string, string>>>>(config.text);
        }
    }
    
    private void GenerateMenuConfig()
    {
        Dictionary<string, Dictionary<string, string>> menuCanvasDict = new Dictionary<string, Dictionary<string, string>>();

        Object[] menuAssets = AssetManager.Singleton.GetAssetsInFolder("MainMenu");
        
        // Prepare Lists for searching the desired GameObject for the Config JSON file
        List<GameObject> menuCanvasObjects = new List<GameObject>();
        List<string> menuCanvasNames = new List<string>();
        foreach (Object asset in menuAssets)
        {
            if (asset.name.Contains("MenuCanvas"))
            {
                menuCanvasObjects.Add(asset as GameObject);
                menuCanvasNames.Add(asset.name);
            }
        }

        foreach (GameObject menuCanvas in menuCanvasObjects)
        {
            Dictionary<string, string> buttonsDict = new Dictionary<string, string>();
            
            foreach (Button childObject in menuCanvas.GetComponentsInChildren<Button>())
            {
                // Remove 'Button' word in object name
                string objectName = childObject.name.Remove(childObject.name.IndexOf("Button"), 6);
                
                // Find Menu Canvas with the same name as the button
                string canvasName = objectName + "MenuCanvas";
                if (menuCanvasNames.Contains(canvasName))
                {
                    buttonsDict.Add(childObject.name, canvasName);
                }
                
                switch (childObject.name)
                {
                    case "EliminationButton":
                        buttonsDict.Add(childObject.name, "MatchLobbyMenuCanvas");
                        break;
                    case "TeamDeathmatchButton":
                        buttonsDict.Add(childObject.name, "MatchLobbyTeamMenuCanvas");
                        break;
                    case "SingleplayerButton":
                        buttonsDict.Add(childObject.name, "GameDirection");
                        break;
                }
            }
            
            menuCanvasDict.Add(menuCanvas.name, buttonsDict);
        }

        Dictionary<string, Dictionary<string, Dictionary<string, string>>> menuConfig =
            new Dictionary<string, Dictionary<string, Dictionary<string, string>>>()
            {
                {"MenuCanvas", menuCanvasDict}
            };

        string filePath = Application.dataPath + "/Resources/Modules/MenuConfig.json";
        SaveToJsonFile(menuConfig, filePath);
    }

    #endregion
    
    #region Change menu screen

    /// <summary>
    /// Move To Next Menu
    /// </summary>
    /// <param name="menuName"></param>
    public void ChangeToMenu(AssetEnum menuName)
    {
        LeanTween.alpha(currentMenu, 0, 0.4f).setOnComplete(() =>
        {
            OnChangeMenuComplete(menuName.ToString());
        });
    }

    public void ChangeToMenu(string menuName)
    {
        LeanTween.alpha(currentMenu, 0, 0.4f).setOnComplete(() =>
        {
            OnChangeMenuComplete(menuName);
        });
    }
    
    /// <summary>
    /// Change Menu Callback
    /// </summary>
    /// <param name="menuName"></param>
    private void OnChangeMenuComplete(String menuName)
    {
        if (currentMenu != null)
        {
            currentMenu.SetActive(false);
        }

        var targetMenu = menusDictionary[menuName];
            
        targetMenu.gameObject.SetActive(true);
        currentMenu = targetMenu;
        menusStack.Push(currentMenu);
    }
    
    
    public void ChangeToMenu<T>(AssetEnum menuName, Action<T> onMenuChanged)
    {
        LeanTween.alpha(currentMenu, 0, 0.4f).setOnComplete(() =>
        {
            OnMenuChanged(menuName, onMenuChanged);
        });
    }
    
    private void OnMenuChanged<T>(AssetEnum menuName, Action<T> onComplete)
    {
        if (currentMenu != null)
        {
            currentMenu.SetActive(false);
        }

        var targetMenu = menusDictionary[menuName.ToString()];
        targetMenu.gameObject.SetActive(true);
        if (onComplete != null)
        {
            var uiController = targetMenu.GetComponent<T>();
            onComplete(uiController);
        }
        currentMenu = targetMenu;
        menusStack.Push(currentMenu);
    }

    public void HideAnimate(Action onHideAnimateComplete)
    {
        _onHideAnimateComplete = onHideAnimateComplete;
        LeanTween.alpha(currentMenu, 0, 0.4f).setOnComplete(OnHideAnimateTweenComplete);
    }

    private void OnHideAnimateTweenComplete()
    {
        CloseMenuPanel();
        if (_onHideAnimateComplete != null)
        {
            _onHideAnimateComplete();
            _onHideAnimateComplete = null;
        }
    }
    
    /// <summary>
    /// Change Scene
    /// </summary>
    /// <param name="sceneName"></param>
    public void ChangeScene(string sceneName)
    {
        LeanTween.alpha(currentMenu, 0, 0.4f).setOnComplete(() =>
        {
            OnChangeSceneComplete(sceneName);
        });

    }
    /// <summary>
    /// Change scene callback
    /// </summary>
    /// <param name="sceneName"></param>
    private void OnChangeSceneComplete(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        Instance.CloseMenuPanel();
    }
    
    // back to one page before
    public void OnBackPressed()
    {
        LeanTween.alpha(currentMenu, 0, 0.4f).setOnComplete(() =>
        {
            if (currentMenu == menusStack.Peek())
            {
                menusStack.Pop().SetActive(false);
                currentMenu = menusStack.Peek();
                currentMenu.SetActive(true);
            }
        });
    }

    // close menu panel
    public void CloseMenuPanel()
    {
        currentMenu.SetActive(false);
    }

    /// <summary>
    /// Go to main menu from another scene
    /// </summary>
    /// <param name="sceneName"></param>
    public void ChangeToMainMenu([CanBeNull] string sceneName)
    {
                
        Time.timeScale = 1; //this line added to handle Leantween bug

        LeanTween.alpha(currentMenu, 0, 0.4f).setOnComplete(() =>
        {
            OnChangeToMainMenuComplete(sceneName);
        });
        
    }
    
    /// <summary>
    /// Go to main menu callback
    /// </summary>
    /// <param name="sceneName"></param>
    private void OnChangeToMainMenuComplete([CanBeNull] string sceneName)
    {
        if (!sceneName.IsNullOrEmpty())
        {
            SceneManager.LoadScene(sceneName,LoadSceneMode.Single);
        }
        var mainMenu = menusDictionary[AssetEnum.MainMenuCanvas.ToString()];
        currentMenu = mainMenu;
        currentMenu.SetActive(true);
        menusStack.Clear();
        menusStack.Push(currentMenu);
    }
    #endregion

    #region Initialize All Menu prefab from asset manager

    /// <summary>
    /// Init all menu canvas from asset manager
    /// </summary>
    void InitMenuFromAssets()
    {
        var childDictionary = buttonDictionary.GetValueOrDefault("MenuCanvas");
        foreach (var menuCanvas in childDictionary)
        {
            object menuCanvasObj = AssetManager.Singleton.GetAsset(menuCanvas.Key);
            GameObject menuCanvasGameObject = menuCanvasObj as GameObject;
            if (menuCanvasGameObject == null)
            {
                Debug.Log(menuCanvas.Key+" is null");
            }
            GameObject menuGameObject = Instantiate(menuCanvasGameObject, transform);
            menusDictionary[menuCanvas.Key] = menuGameObject;
            menusDictionary[menuCanvas.Key].name = menusDictionary[menuCanvas.Key].name.Replace("(Clone)", "");
            menusDictionary[menuCanvas.Key].SetActive(false);
            
            // Prepare GameObject for UI References
            TutorialModuleManager.Instance.PrepareUIAssets(menuGameObject);
        }
        
        currentMenu = menusDictionary[AssetEnum.LoadingMenuCanvas.ToString()];
        if (currentMenu.activeSelf == false)
        {
            currentMenu.SetActive(true);
            menusStack.Push(currentMenu);
        }

    }

    #endregion

    #region Set Menu direction(canvas) into button)

    /// <summary>
    /// Set button menu direction
    /// </summary>
    void SetMenuDirection()
    {
        var childDictionary = buttonDictionary.GetValueOrDefault("MenuCanvas");
        foreach (var item in menusDictionary)
        {
            var buttons = item.Value.GetComponentsInChildren<Button>();
            // bool isCanvasAvailable = childDictionary.ContainsKey(item.Key);
            var buttonsDictionaryByMenuCanvas = childDictionary.GetValueOrDefault(item.Key);
            
            foreach (var button in buttons)
            {
                //Setup Back button listener into all menu
                if (button.name == BACK_BUTTON)
                {
                    button.onClick.AddListener(() => Instance.OnBackPressed());
                }
                else
                {
                    var canvasTarget = buttonsDictionaryByMenuCanvas.GetValueOrDefault(button.name);

                    if (!canvasTarget.IsNullOrEmpty())
                    {
                        if (canvasTarget == "GameDirection")
                        {
                            continue;
                        }
                        //Debug.Log($"L 251 {canvasTarget}");
                        AssetEnum menuCanvasEnum;
                        Enum.TryParse(canvasTarget, out menuCanvasEnum);
                        //Debug.Log($"L 254 {menuCanvasEnum.ToString()}");
                        button.onClick.AddListener(() => Instance.ChangeToMenu(menuCanvasEnum));
                    }
                    else
                    {
                        DisableButton(button);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Helper Function to disable Button UI
    /// </summary>
    /// <param name="button"></param>
    private void DisableButton(Button button)
    {
        button.enabled = false;
        button.transition = Selectable.Transition.None;
        button.GetComponent<Image>().color = Color.clear;
        button.GetComponentInChildren<TMP_Text>().color = Color.gray;
        button.enabled = false;
    }

    #endregion
    
    #region Action from related to login screen

    // Back To Login From all Menu
    public void BackToLoginMenu()
    {
        if (currentMenu != null)
        {
            currentMenu.SetActive(false);
        }

        var loginMenu = menusDictionary[MenuEnum.LoginMenuCanvas.ToString()];

        loginMenu.SetActive(true);
        currentMenu = loginMenu;
        menusStack.Clear();
    }

    // Go to Main menu from Login menu
    public void LoginToMainMenu()
    {
        ChangeToMainMenu(null);
    }

    #endregion

    #region Utilities Functions
    
    /// <summary>
    /// Store any object to JSON file
    /// </summary>
    /// <param name="desiredObject">any object to write and store in JSON file</param>
    /// <param name="filePath">destination path of the JSON output file</param>
    private void SaveToJsonFile(object desiredObject, string filePath)
    {
        string jsonString = JsonConvert.SerializeObject(desiredObject, Formatting.Indented);
        
        File.WriteAllText(filePath, jsonString);
    }

    #endregion
    
    #region create menu runtime
    public string AddMenu(MenuCanvasData menuCanvasData, string message=null)
    {
        GameObject newMenuGO;
        if (!menusDictionary.TryGetValue(menuCanvasData.name, out newMenuGO))
        {
            var newMenu = Instantiate(_starterMenuCanvasPrefab, transform);
            newMenu.name = menuCanvasData.name;
            newMenu.InstantiateButtons(menuCanvasData.buttons, message);
            GameObject o;
            (o = newMenu.gameObject).SetActive(false);
            menusDictionary[menuCanvasData.name] = o;   
        }
        else
        {
            StarterMenuCanvas starterMenuCanvas = newMenuGO.GetComponent<StarterMenuCanvas>();
            starterMenuCanvas.SetButtonsCallback(menuCanvasData.buttons);
            starterMenuCanvas.SetAdditionalInfo(message);
        }
        return menuCanvasData.name;
    }
    #endregion

    public void ShowRetrySkipQuitMenu(UnityAction retryCallback, UnityAction skipCallback, string message=null)
    {
        var retrySkipMenuData = AssetManager.Singleton.GetAsset(AssetEnum.RetrySkipQuitMenu)
            as TutorialModuleData;
        if (retrySkipMenuData != null)
        {
            RetrySkipQuitMenuHandler handler =
                TutorialModuleManager.Instance.GetModuleClass<RetrySkipQuitMenuHandler>();
            handler.SetData(retrySkipMenuData, retryCallback, skipCallback, message);
            string menuName = AddMenu(retrySkipMenuData.menuCanvasData, message);
            ChangeToMenu(menuName);
        }
    }
    
}
