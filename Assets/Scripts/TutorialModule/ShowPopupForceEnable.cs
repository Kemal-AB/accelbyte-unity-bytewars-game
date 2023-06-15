using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class ShowPopupForceEnable : EditorWindow
{
    private Vector2 _scrollPos;

    public static void Init()
    {
        ShowPopupForceEnable window = ScriptableObject.CreateInstance<ShowPopupForceEnable>();
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 300, 150);
        window.titleContent = new GUIContent("Override found on TutorialModuleConfig.json");
        window.ShowPopup();
    }
    

    void OnGUI()
    {
        // var modulesName = TutorialModuleOverride.ForcedModules;
        var moduleDependencies = TutorialModuleForceEnable.ListAllModules;
        // var modules = modulesName != null ? String.Join(' ', modulesName) : "Null";
        var modules = moduleDependencies != null ? String.Join(' ', moduleDependencies) : "Null";
        if (TutorialModuleForceEnable.IsError)
        {
            EditorGUILayout.LabelField($"Check your {modules} module, the Asset Config cannot be found ", EditorStyles.wordWrappedLabel);

        }
        else
        {
            //Override found on TutorialModuleConfig.json
            EditorGUILayout.LabelField($"Override found on TutorialModuleConfig.json", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Force enabled modules:", EditorStyles.wordWrappedLabel);
            _scrollPos =
                EditorGUILayout.BeginScrollView(_scrollPos, alwaysShowHorizontal:false, alwaysShowVertical:true);
            EditorGUILayout.LabelField($"{modules}", EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView();

        }
        // GUILayout.Space(70);
        if (GUILayout.Button("Ok!")) this.Close();
    }
}
#endif