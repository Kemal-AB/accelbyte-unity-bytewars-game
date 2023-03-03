using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Directory = System.IO.Directory;
using Object = UnityEngine.Object;

public class TutorialModuleManager : MonoBehaviour
{
    // private instance
    private static TutorialModuleManager _instance;
    // instance getter
    public static TutorialModuleManager Instance => _instance;

    private Dictionary<string, ModuleData> _moduleClassTypes = new Dictionary<string, ModuleData>();
    private Dictionary<string, List<Transform>> _moduleUITransforms = new Dictionary<string, List<Transform>>();
    private bool _isInstantiated = false;

    private void Awake()
    {
        // check if another instance for TutorialModuleManager exists
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        PrepareScriptAssets();
        AddModuleComponents();
    }

    #region Runtime Initialize Functions
    
    /// <summary>
    /// Create TutorialModuleManager GameObject in Scene on Runtime
    /// </summary>
    [RuntimeInitializeOnLoadMethod]
    private static void SingletonInstanceChecker()
    {
        if (_instance == null)
        {
            GameObject moduleManagerGameObject = new GameObject("TutorialModuleManager");
            _instance = moduleManagerGameObject.AddComponent<TutorialModuleManager>();
        }
    }
    
    #endregion

    #region Module References Initialization Functions

    /// <summary>
    /// Prepare Script Asset for Module Class Reference
    /// </summary>
    private void PrepareScriptAssets()
    {
        // temporary added, expected result from AssetManager (need to be added in AssetManager instead)
        if (AssetManager.Singleton == null)
            return;
        Object[] scriptObjects = AssetManager.Singleton.GetTextAssets();
        
        // store the Class Type values in dictionary
        foreach (Object script in scriptObjects)
        {
            Type scriptClassType = TypeBuilder.GetType(script.name);

            if (scriptClassType != null)
            {
                string scriptPath = Directory.GetFiles(Application.dataPath, script.name + ".cs", SearchOption.AllDirectories)[0];
                List<string> pathCategories = scriptPath.Split(new char[] {'\\', '/'}).ToList();
                
                // remove all folders path until "Modules" folder
                pathCategories.RemoveRange(0, pathCategories.IndexOf("Modules") + 1);
                // remove script name
                pathCategories.RemoveAt(pathCategories.Count - 1);
                // remove "Scripts" folder
                if (pathCategories.Contains("Scripts"))
                {
                    pathCategories.RemoveAt(pathCategories.IndexOf("Scripts"));
                }

                _moduleClassTypes.Add(script.name, new ModuleData(scriptClassType, pathCategories.ToArray()));   
            }
        }
    }
    
    /// <summary>
    /// Add all modules loaded as TutorialModuleManager GameObject's components
    /// </summary>
    private void AddModuleComponents()
    {
        foreach (ModuleData module in _moduleClassTypes.Values)
        {
            GameObject moduleGameObject = this.gameObject;
            Transform childTransform = null;
            bool objectCreated = false;
            for (int index = module.categoryPath.Length - 1; index >= 0; index--)
            {
                Transform foundTransform = this.transform.Find(module.categoryPath[index]);
                if (foundTransform != null)
                {
                    objectCreated = true;
                }
                else
                {
                    foundTransform = new GameObject(module.categoryPath[index]).transform;
                }
                
                // check if it's the desired module gameobject
                if (index == module.categoryPath.Length - 1)
                {
                    moduleGameObject = foundTransform.gameObject;
                }
                // if loops ends
                if (index == 0)
                {
                    foundTransform.SetParent(this.transform);
                    objectCreated = true;
                }
                
                // in case, child 
                if (childTransform != null)
                {
                    childTransform.SetParent(foundTransform);
                }

                // break loop if all Module Game Objects created
                if (objectCreated)
                {
                    break;
                }

                // store the newly created foundTransform for the next loop
                childTransform = foundTransform;
            }
            
            moduleGameObject.AddComponent(module.classType);
        }
    }

    #endregion

    #region UI References Initialization Functions
    
    /// <summary>
    /// Prepare UI Assets that will be used from another script
    /// Need to be called where the Prefab instantiated
    /// </summary>
    public void PrepareUIAssets(GameObject viewGameObject)
    {
        List<Transform> childTransformList = new List<Transform>();
        foreach (Transform childTransform in viewGameObject.GetComponentsInChildren<Transform>())
        {
            if (childTransform != viewGameObject.transform)
            {
                childTransformList.Add(childTransform);   
            }
        }

        _moduleUITransforms.Add(viewGameObject.name, childTransformList);

        // set flag to true for later checking
        _isInstantiated = true;
    }

    /// <summary>
    /// Check function that will wait until _isInstantiated set to TRUE
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckUIInstantiated()
    {
        while (!_isInstantiated)
        {
            yield return CheckUIInstantiated();
        }
    }

    #endregion
    
    #region Getter Functions

    /// <summary>
    /// Get specific Module object to get the Module's Class.
    /// Cast the returned value to the desired class for getting the Class references.
    /// Example: <code>ClassName object = (ClassName) TutorialModuleManager.Instance.GetModuleClass("ClassName");</code>
    /// </summary>
    /// <param name="moduleId">Module's Id of the desired Module Class</param>
    /// <returns>object of the desired module component</returns>
    public Object GetModuleClass(string moduleId)
    {
        Type moduleType = _moduleClassTypes[moduleId].classType;
        return gameObject.GetComponentInChildren(moduleType);
    }
    
    public T GetModuleClass<T>()
    {
        return gameObject.GetComponentInChildren<T>();
    }
    
    /// <summary>
    /// Get specific UI Transform 
    /// </summary>
    /// <param name="prefabId">Prefab Id of the desired UI's gameobject</param>
    /// <param name="transformId">Transform Id of the desired UI gameobject in the Prefab</param>
    /// <returns>Transform of the desired UI gameobject inside the Module Prefab</returns>
    public Transform GetUITransform(string prefabId, string transformId)
    {
        IEnumerator check = CheckUIInstantiated();

        if (!check.Equals(null))
        {
            foreach (Transform uiTransform in _moduleUITransforms[prefabId])
            {
                if (uiTransform.name == transformId)
                {
                    return uiTransform;
                }
            }
        }
        
        return null;
    }

    #endregion
}