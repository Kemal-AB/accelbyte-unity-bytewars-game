using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class TutorialModuleData : ScriptableObject
{
    public MenuCanvas defaultMenuUIPrefab;
    public MenuCanvas[] defaultAdditionalMenuUIPrefabs;
    public TextAsset defaultModuleScript;
    public TutorialType type;
    public bool isActive;
    public bool additionalScripts;
    public bool additionalPrefab;
    public TextAsset[] defaultHelperScripts;
    public TextAsset[] starterHelperScripts;
    public TextAsset starterScript;
    public MenuCanvas starterMenuUIPrefab;
    public MenuCanvas[] starterAdditionalMenuUIPrefabs;
    public bool isStarterActive;
    public TutorialModuleData[] moduleDependencies;

    #region Tutorial Module Generated Prefabs
    
    public PrefabObjectType instantiatedPrefabType;
    public string instantiatedGameObjectName;
    public PrefabClassType prefabClassType;
    public int associateDefaultPrefabClass;
    public int associateStarterPrefabClass;
    public TutorialType otherTutorialModule;
    public AssetEnum genericPrefabClass;
    public string buttonText;
    public string targetParentGameObjectPath;
    public int spawnOrder;
    
    #endregion
}
