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
}
