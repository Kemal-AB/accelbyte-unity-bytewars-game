using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
[InitializeOnLoad]
public static class TutorialModuleOverride
{
    internal const string FIRST_TIME = "FIRST_TIME"; // the key

    public static string[] ListAllModules
    {
        get => _moduleDependencies;
        set => _moduleDependencies = value;
    }
    public static string[] ForcedModules
    {
        get => _forcedModules;
        set => _forcedModules = value;
    }

    public static bool IsError
    {
        get => _isError;
        set => _isError = value;
    }
    
    private static TutorialModuleData _overrideModule;
    private static string[] _forcedModules;
    private static bool _isError;
    private static Dictionary<string, TutorialModuleData> _moduleDictionary = new Dictionary<string, TutorialModuleData>();
    private static string[] _moduleDependencies;

#if UNITY_EDITOR
    static TutorialModuleOverride()
    {
        EditorApplication.update += RunOnce;
        EditorApplication.quitting += Quit;
    }
#endif

    private static void Quit()
    {
        EditorPrefs.DeleteKey(FIRST_TIME);
    }

    /// <summary>
    /// Method to show window at the firs time open editor
    /// </summary>
    private static void RunOnce()
    {
        var firstTime = EditorPrefs.GetBool(FIRST_TIME, true);

        if (firstTime)
        {
            if (EditorPrefs.GetBool("FIRST_TIME", true))
            {
                var isReadJsonConfig = ReadJsonConfig() != null ? ReadJsonConfig() : null;
                if (isReadJsonConfig != null)
                {
                    // GetAllDependencies(isReadJsonConfig);
                    isReadJsonConfig.ToList().ForEach(x => OverrideModules($"{x}AssetConfig", true));
                    _moduleDependencies = _moduleDictionary.Select(x => x.Key.Replace("AssetConfig", "")).ToArray();
                    Debug.Log($"first time opened");
                    ShowPopupOverride.Init();
                }
            }

            EditorPrefs.SetBool(FIRST_TIME, false);
        }

        EditorApplication.update -= RunOnce;
    }


    // private static void GetAllDependencies(string[] isReadJsonConfig)
    // {
    //     var overridesModules = isReadJsonConfig;
    //     var modulesDictionary = new Dictionary<string, bool>();
    //     overridesModules?.ToList().ForEach(x =>
    //     {
    //         var module = GetTutorialModuleDataObject(x);
    //         if (module == null)
    //         {
    //             return;
    //         }
    //         
    //         modulesDictionary.TryAdd(module.name, true);
    //         if (module.moduleDependencies.Length > 0)
    //         {
    //             modulesDictionary.TryAdd(module.name, true);
    //             foreach (var dependency in module.moduleDependencies)
    //             {
    //                 dependency.moduleDependencies
    //             }
    //         }
    //     });
    //     _moduleDependencies = modulesDictionary.Select(x => x.Key).ToArray();
    // }

    private static bool IsTargetModuleCurrentSelectedModule()
    {
        return _overrideModule.name == Selection.activeObject.name ? true : false;
    }

    public static bool IsDependency(string selectedAssetConfig)
    {
        // Check each asset configs registered in _moduleDictionary
        // Debug.Log($"check if it's a dependency module {selectedAssetConfig}");
        _moduleDictionary.TryGetValue(selectedAssetConfig, out _overrideModule);
        if (_overrideModule != null)
        {
            // Debug.Log($"{_overrideModule.name} it's a dependency module ");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Read Json config related to the override modules
    /// </summary>
    /// <param name="readFromInspector"></param>
    /// <param name="startOrChangeConfig"></param>
    /// <returns></returns>
    private static string[] ReadJsonConfig(bool readFromInspector = false)
    {
        var tutorialModuleConfig = (TextAsset)Resources.Load("Modules/TutorialModuleConfig");
        var json = JsonUtility.FromJson<TutorialModuleConfig>(tutorialModuleConfig.text);
        
        // Check if open asset config from inspector
        if (!readFromInspector)
        {
            Debug.Log($"override module {String.Join(" ",json.modulesName)}");
            Debug.Log($"override status {json.enableModulesOverride}");
        }
        if (json.modulesName.Length <= 0)
        {
            Debug.Log($"there are no modules override, check length module {json.modulesName.Length}");
            _forcedModules = null;
            _isError = true;
            return _forcedModules;
        }
        if (!json.enableModulesOverride)
        {
            Debug.Log($"enableModulesOverride status {json.enableModulesOverride}");
            _forcedModules = null;
            return _forcedModules;
        }

        _forcedModules = json.modulesName;
        return _forcedModules;
    }

    public static bool OverrideModules(string moduleName, bool isFirstTime = false)
    {
        if (!moduleName.ToLower().Contains("assetconfig"))
        {
            return false;
        }
        
        var isReadJsonConfig = ReadJsonConfig(readFromInspector:true) != null ? _forcedModules : null;

        if (isReadJsonConfig == null)
        {
            return false;
        }
        
        //TODO: Null checcking on moduleName
        var overrideStatus = false;
        var overridesModules = isReadJsonConfig;
        var modulesDictionary = new Dictionary<string, bool>();
        overridesModules?.ToList().ForEach(x =>
        {
            var module = GetTutorialModuleDataObject(x);
            if (module == null)
            {
                overrideStatus = false;
                return;
            }

            _overrideModule = module;

            if (!isFirstTime)
            {
                if (IsTargetModuleCurrentSelectedModule())
                {
                    _overrideModule.isActive = true;
                    // _overrideModule.isStarterActive = false;
                    overrideStatus = SetDependenciesToActive();
                }
                else
                {
                    overrideStatus = false;
                }
            }
            else
            {
                overrideStatus = true;
            }

            modulesDictionary.Add(_overrideModule.name, overrideStatus);
            _moduleDictionary.TryAdd(_overrideModule.name, _overrideModule);

            CheckDependency(_overrideModule);
        });
        modulesDictionary.TryGetValue(moduleName, out overrideStatus);
        return overrideStatus;
    }

    private static void CheckDependency(TutorialModuleData moduleData)
    {
        foreach (var tutorialModuleData in moduleData.moduleDependencies)
        {
            _moduleDictionary.TryAdd(tutorialModuleData.name, tutorialModuleData);
            if (tutorialModuleData.moduleDependencies != null)
            {
                CheckDependency(tutorialModuleData);
            }
        } 
    }

    private static bool SetDependenciesToActive()
    {
        var modulesLength = _overrideModule.moduleDependencies.Length;
        switch (modulesLength)
        {
            case 0:
                return true;
            case > 0:
            {
                foreach (var dependency in _overrideModule.moduleDependencies.Select((value, index) =>
                             (value, index)))
                {
                    if (dependency.value == null)
                    {
                        Debug.Log($"this element {dependency.index} is null, please add the dependency value");
                        return false;
                    }

                    Debug.Log($"element {dependency.index} {dependency.value.name}");
                    dependency.value.isActive = true;
                    dependency.value.isStarterActive = false;
                }

                return true;
            }
            default:
                return false;
        }
    }

    private static TutorialModuleData GetTutorialModuleDataObject(string moduleName)
    {
        var fileName = $"{moduleName}AssetConfig";
        var assets = AssetDatabase.FindAssets(fileName);
        if (assets.Length == 0)
        {
            Debug.Log($"check your module name, the Asset Config cannot be found");
            _isError = true;
            ShowPopupOverride.Init();
            return null;
        }
        var asset = assets[0] != null
            ? AssetDatabase.GUIDToAssetPath(assets[0])
            : null;
        Debug.Log(asset);
        return AssetDatabase.LoadAssetAtPath<TutorialModuleData>(asset);
    }

    private static void GetAllHelperFiles(Object selectedGameObject, bool isStaterActive = false)
    {
        var assetConfig = (TutorialModuleData)selectedGameObject;
        var fileName = selectedGameObject.name;
        var allModuleFiles = Directory.GetFiles($"{Application.dataPath}/Resources", 
            fileName, SearchOption.AllDirectories ).Where(x => !x.Contains("UI"));

        var moduleFiles = allModuleFiles as string[] ?? allModuleFiles.ToArray();
        
        if (isStaterActive)
        {
            //return all starter helper files
            var starterHelperScripts = moduleFiles.Where(x => x.Contains("starter")).Select(AssetDatabase.LoadAssetAtPath<TextAsset>).ToArray();
            assetConfig.starterHelperScripts = starterHelperScripts;

        }
        //return default helper files
        var defaultHelperScripts = moduleFiles.Where(x => !x.Contains("starter")).Select(AssetDatabase.LoadAssetAtPath<TextAsset>).ToArray();
        assetConfig.defaultHelperScripts = defaultHelperScripts;

    }
}
#endif


public class TutorialModuleConfig
{
    public bool enableModulesOverride;
    public string[] modulesName;

}