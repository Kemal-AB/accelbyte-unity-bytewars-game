using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class TutorialModuleData : ScriptableObject
{
    public string moduleName;
    public bool isActive;
    [SerializeField]
    public MenuCanvasData menuCanvasData;
}
