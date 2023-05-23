using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class TutorialModuleOverride
{
    internal const string FIRST_TIME = "FIRST_TIME"; // the key

    public static string ForcedModule
    {
        get => _forcedModule;
        set => _forcedModule = value;
    }

    public static bool IsError
    {
        get => _isError;
        set => _isError = value;
    }
    
    private static TutorialModuleData _overrideModule;
    private static string _forcedModule;
    private static bool _isError;

    static TutorialModuleOverride()
    {
        EditorApplication.update += RunOnce;
        EditorApplication.quitting += Quit;
    }

    private static void Quit()
    {
        EditorPrefs.DeleteKey(FIRST_TIME);
    }

    private static void RunOnce()
    {
        var firstTime = EditorPrefs.GetBool(FIRST_TIME, true);

        if (firstTime)
        {
            if (EditorPrefs.GetBool("FIRST_TIME", true))
            {
                var IsReadJsonConfig = ReadJsonConfig() != null ? ReadJsonConfig() : null;
                if (IsReadJsonConfig != null)
                {
                    Debug.Log($"first time opened");
                    ShowPopupOverride.Init();
                }
            }

            EditorPrefs.SetBool(FIRST_TIME, false);
        }

        EditorApplication.update -= RunOnce;
    }


    private static bool IsTargetModuleCurrentSelectedModule()
    {
        return _overrideModule.name == Selection.activeObject.name ? true : false;
    }

    public static bool IsDependency(string name)
    {
        if (_overrideModule != null)
        {
            Debug.Log(_overrideModule.moduleDependencies.Length);
            if (_overrideModule.moduleDependencies.Length > 0)
            {
                return _overrideModule.moduleDependencies
                    .Any(x => x.name == name 
                              || x.moduleDependencies.Any(y => y.name == name));
            }
        }

        return false;
    }

    private static string ReadJsonConfig()
    {
        var tutorialModuleConfig = (TextAsset)Resources.Load("Modules/TutorialModuleConfig");
        var json = JsonUtility.FromJson<TutorialModuleConfig>(tutorialModuleConfig.text);
        Debug.Log($"override module {json.moduleName}");
        Debug.Log($"override status {json.moduleOverrideStatus}");
        if (!json.moduleOverrideStatus)
        {
            return null;
        }

        _forcedModule = json.moduleName;
        return _forcedModule;
    }

    public static bool OverrideModules()
    {
        var overridesModuleName = ReadJsonConfig();
        var module = GetTutorialModuleDataObject(overridesModuleName);
        if ( module == null)
        {
            return false;
        }

        _overrideModule = module;
        Debug.Log(_overrideModule.name);

        if (IsTargetModuleCurrentSelectedModule())
        {
            _overrideModule.isActive = true;
            return SetDependenciesToActive();
        }
        else
        {
            return false;
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

                    Debug.Log(dependency.value.name);
                    dependency.value.isActive = true;
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
}


#region UnusedCode
//     public static bool OverrideDependencyModules(Object moduleName)
//     {
//         ReadJsonConfig();
//         
//         Debug.Log($"current selected module {moduleName.name}");
//
//         if (moduleName == null)
//         {
//             return false;
//         }
//             
//         var modules = (TutorialModuleData)moduleName;
//         var modulesLength = modules.ModuleDependency.TutorialModuleDatas.Length;
//         Debug.Log(modulesLength);
//         if (modulesLength > 1)
//         {
//             foreach (var dependency in modules.ModuleDependency.TutorialModuleDatas.Select((value, index) => (value, index)))
//             {
//                 if (dependency.value == null)
//                 {
//                     Debug.Log($"this element {dependency.index} is null");
//                     return false;
//                 }
//
//                 
//                 Debug.Log(dependency.value.name);
//                 dependency.value.isActive = true;
//             }
//
//             return true;
//         }
//         else
//         {
//             return false;
//         }
//     }
// }
    

    #endregion
    
public class TutorialModuleConfig
{
    public bool moduleOverrideStatus;
    public string moduleName;

}