using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        var modulesName = TutorialModuleOverride.ForcedModules;
        var modules = String.Join(' ', modulesName);
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
