using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class TutorialModuleData : ScriptableObject
{
    public MenuCanvas prefab;
    public TutorialType type;
    public bool isActive;
    public TutorialModuleData[] moduleDependencies;
}
