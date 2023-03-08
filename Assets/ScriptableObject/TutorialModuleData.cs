using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class TutorialModuleData : ScriptableObject
{
    public GameObject prefab;
    public string moduleName;
    public bool isActive;
}
