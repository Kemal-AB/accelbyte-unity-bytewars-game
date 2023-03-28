using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using TextAsset = UnityEngine.TextAsset;

public class MenuManager : MonoBehaviour
{
    private const string BACK_BUTTON = "BackButton";

    private static MenuManager _instance;
    public static MenuManager Instance => _instance;

    private Dictionary<string, GameObject> _menusDictionary = new Dictionary<string, GameObject>();
    private Stack<GameObject> _mainMenusStack = new Stack<GameObject>();
    private Stack<GameObject> _inGameStack = new Stack<GameObject>();
    private GameObject _currentMainMenu;
    private GameObject _inGameMenu;
    private bool _isAGSDKReady = false;

    private Action _onHideAnimateComplete = null;

    
    
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
        
        // Make MenuManager object persistent in all scene
        DontDestroyOnLoad(this.gameObject);
    }
    
    #region Runtime Initialize Functions
    
    /// <summary>
    /// Create MenuManager GameObject in Scene on Runtime
    /// </summary>
    [RuntimeInitializeOnLoadMethod]
    private static void SingletonInstanceChecker()
    {
        if (_instance == null)
        {
            GameObject menuManagerGameObject = new GameObject("MenuManager");
            _instance = menuManagerGameObject.AddComponent<MenuManager>();
        }
    }
    
    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        InitMenu();
    }

    #region Change menu screen

    /// <summary>
    /// Move To Next Menu
    /// </summary>
    /// <param name="menuName"></param>
    public void ChangeToMenu(AssetEnum menuName)
    {
        LeanTween.alpha(_currentMainMenu, 0, 0.4f).setOnComplete(() =>
        {
            OnChangeMenuComplete(menuName.ToString());
        });
    }

    public void ChangeToMenu(string menuName)
    {
        LeanTween.alpha(_currentMainMenu, 0, 0.4f).setOnComplete(() =>
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
        if (_currentMainMenu != null)
        {
            _currentMainMenu.SetActive(false);
        }

        var targetMenu = _menusDictionary[menuName];
            
        targetMenu.gameObject.SetActive(true);
        _currentMainMenu = targetMenu;
        _mainMenusStack.Push(_currentMainMenu);
    }
    
    
    public void ChangeToMenu<T>(AssetEnum menuName, Action<T> onMenuChanged)
    {
        LeanTween.alpha(_currentMainMenu, 0, 0.4f).setOnComplete(() =>
        {
            OnMenuChanged(menuName, onMenuChanged);
        });
    }
    
    private void OnMenuChanged<T>(AssetEnum menuName, Action<T> onComplete)
    {
        if (_currentMainMenu != null)
        {
            _currentMainMenu.SetActive(false);
        }

        var targetMenu = _menusDictionary[menuName.ToString()];
        targetMenu.gameObject.SetActive(true);
        if (onComplete != null)
        {
            var uiController = targetMenu.GetComponent<T>();
            onComplete(uiController);
        }
        _currentMainMenu = targetMenu;
        _mainMenusStack.Push(_currentMainMenu);
    }

    public void HideAnimate(Action onHideAnimateComplete)
    {
        _onHideAnimateComplete = onHideAnimateComplete;
        LeanTween.alpha(_currentMainMenu, 0, 0.4f).setOnComplete(OnHideAnimateTweenComplete);
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
        LeanTween.alpha(_currentMainMenu, 0, 0.4f).setOnComplete(() =>
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
        LeanTween.alpha(_currentMainMenu, 0, 0.4f).setOnComplete(() =>
        {
            if (_currentMainMenu == _mainMenusStack.Peek())
            {
                _mainMenusStack.Pop().SetActive(false);
                _currentMainMenu = _mainMenusStack.Peek();
                _currentMainMenu.SetActive(true);
            }
        });
    }

    // close menu panel
    public void CloseMenuPanel()
    {
        _currentMainMenu.SetActive(false);
    }

    /// <summary>
    /// Go to main menu from another scene
    /// </summary>
    /// <param name="sceneName"></param>
    public void ChangeToMainMenu([CanBeNull] string sceneName)
    {
        AudioManager.Instance.PlayMusic("BGM_MainMenu");
                
        Time.timeScale = 1; //this line added to handle Leantween bug

        LeanTween.alpha(_currentMainMenu, 0, 0.4f).setOnComplete(() =>
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
        if (sceneName != null)
        {
            SceneManager.LoadScene(sceneName,LoadSceneMode.Single);
        }
        var mainMenu = _menusDictionary[AssetEnum.MainMenuCanvas.ToString()];
        _currentMainMenu = mainMenu;
        _currentMainMenu.SetActive(true);
        _mainMenusStack.Clear();
        _mainMenusStack.Push(_currentMainMenu);
    }
    #endregion

    #region Initialize and Instantiate Menu prefab

    private void InitMenu()
    {
        // Play Default Music
        AudioManager.Instance.PlayMusic("BGM_MainMenu");;
        
        // bool isAuthEssentialExist = false; 
        var allActiveModule = TutorialModuleManager.Instance.GetAllActiveModule();

        foreach (var moduleNamePair in allActiveModule)
        {
            if (moduleNamePair.Key.Contains("AuthEssentialTData"))
            {
                InitMenuByModules(moduleNamePair.Value);
            }
        }
        
        InitCoreMenu();
        
        // Check If auth essential active
        if (allActiveModule.TryGetValue("AuthEssentialTData", out TutorialModuleData authEssential))
        {
            _currentMainMenu = _menusDictionary["LoadingMenuCanvas"];
            if (_currentMainMenu.activeSelf == false)
            {            
                _currentMainMenu.SetActive(true);
                _mainMenusStack.Push(_currentMainMenu);
            }

            _isAGSDKReady = TutorialModuleUtil.IsAccelbyteSDKInstalled();
            IEnumerator check = CheckAGSDKReady();

            if (!check.Equals(null))
            {
                _currentMainMenu.SetActive(false);
                _currentMainMenu = _menusDictionary["LoadingMenuCanvas"];;
                _currentMainMenu.SetActive(true);
            }
        }
        else
        {
            _currentMainMenu = _menusDictionary["MainMenuCanvas"];
            if (_currentMainMenu.activeSelf == false)
            {            
                _currentMainMenu.SetActive(true);
                _mainMenusStack.Push(_currentMainMenu);
            }
        }
    }

    private IEnumerator CheckAGSDKReady()
    {
        while (!_isAGSDKReady)
        {
            yield return null;
        }
    }

    private void InitCoreMenu()
    {
        object mainmenuConfigObj = AssetManager.Singleton.GetAsset(AssetEnum.MainMenuUiConfig);
        var mainmenuConfig = mainmenuConfigObj as MainMenuUiConfig;

        if (mainmenuConfig == null)
        {
            return;
        }

        var starterMenu = Instantiate(mainmenuConfig.starterMainUI, transform);
        starterMenu.SetActive(false);
        starterMenu.name = starterMenu.name;
        _menusDictionary[mainmenuConfig.starterMainUI.name] = starterMenu;

        foreach (var menuGameObject in mainmenuConfig.otherMainUI)
        {
            var otherCoreMenu = Instantiate(menuGameObject, transform);
            otherCoreMenu.SetActive(false);
            otherCoreMenu.name = menuGameObject.name;
            _menusDictionary[menuGameObject.name] = otherCoreMenu;
        }
    }

    private void InitMenuByModules(TutorialModuleData moduleData)
    {
        var modulePrefab = moduleData.prefab;
        GameObject menubyModule = Instantiate(modulePrefab, Vector3.zero, Quaternion.identity, _instance.transform);
        menubyModule.name = modulePrefab.name; ;
        _menusDictionary.Add(menubyModule.name, menubyModule);
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
        if (_currentMainMenu != null)
        {
            _currentMainMenu.SetActive(false);
        }

        // // var loginMenu = _menusDictionary[MenuEnum.LoginMenuCanvas.ToString()];
        //
        // loginMenu.SetActive(true);
        // _currentMainMenu = loginMenu;
        // _mainMenusStack.Clear();
    }

    // Go to Main menu from Login menu
    public void LoginToMainMenu()
    {
        ChangeToMainMenu(null);
    }

    #endregion

    public void ShowRetrySkipQuitMenu(UnityAction retryCallback, UnityAction skipCallback, string message=null)
    {
        //TODO call RetrySkipQuitMenuHandler.SetData
        // string menuName = AssetEnum.RetrySkipQuitMenuCanvas.ToString();
        var retryMenu = _menusDictionary["RetrySkipQuitMenuCanvas"];
        var handler = retryMenu.GetComponent<RetrySkipQuitMenuHandler>();
        handler.SetData(retryCallback, skipCallback, message);
        ChangeToMenu("RetrySkipQuitMenuCanvas");
    }
    
}
