using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
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
        var modulesName = TutorialModuleOverride.ForcedModules;
        var modules = modulesName != null ? String.Join(' ', modulesName) : "Test";
        if (TutorialModuleOverride.IsError)
        {
            EditorGUILayout.LabelField($"Check your {modules} module, the Asset Config cannot be found ", EditorStyles.wordWrappedLabel);

        }
        else
        {
            EditorGUILayout.LabelField($"Tutorial Module Is Override {modules}", EditorStyles.wordWrappedLabel);
        }
        GUILayout.Space(70);
        if (GUILayout.Button("Ok!")) this.Close();
    }
}
#endif