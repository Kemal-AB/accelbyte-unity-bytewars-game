using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class TutorialModuleData : ScriptableObject
{
    public MenuCanvas prefab;
    public TutorialType type;
    public bool isActive;
}
