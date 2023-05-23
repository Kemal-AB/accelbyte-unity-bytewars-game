using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ShowPopupOverride : EditorWindow
{
    
    public static void Init()
    {
        ShowPopupOverride window = ScriptableObject.CreateInstance<ShowPopupOverride>();
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
        window.ShowPopup();
    }
    

    void OnGUI()
    {
        var moduleName = TutorialModuleOverride.ForcedModule;
        if (TutorialModuleOverride.IsError)
        {
            EditorGUILayout.LabelField($"Check your {moduleName} module, the Asset Config cannot be found ", EditorStyles.wordWrappedLabel);

        }
        else
        {
            EditorGUILayout.LabelField($"Tutorial Module Is Override {moduleName}", EditorStyles.wordWrappedLabel);
        }
        GUILayout.Space(70);
        if (GUILayout.Button("Ok!")) this.Close();
    }
}
