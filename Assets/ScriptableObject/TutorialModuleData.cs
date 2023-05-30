using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class TutorialModuleData : ScriptableObject
{
    public MenuCanvas defaultMenuUIprefab;
    public TextAsset defaultModuleScript;
    public TutorialType type;
    public bool isActive;
    public TextAsset starterScript;
    public MenuCanvas starterMenuUIPrefab;
    public bool isStarterActive;
    public TutorialModuleData[] moduleDependencies;
}
