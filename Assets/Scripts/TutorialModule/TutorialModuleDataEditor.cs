using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(TutorialModuleData),true)]
public class TutorialModuleDataEditor : Editor
{
    private SerializedProperty _defaultMenuUIPrefab;
    private SerializedProperty _defaultModuleScript;
    private SerializedProperty _type;
    private SerializedProperty _isActive;
    private SerializedProperty _starterScript;
    private SerializedProperty _starterMenuUIPrefab;
    private SerializedProperty _isStarterActive;

    private SerializedProperty _hasAdditionalScripts;
    private SerializedProperty _defaultHelperFiles;
    private SerializedProperty _starterHelperFiles;
    
    private SerializedProperty _moduleDependencies;
    private bool _forceEnableStatus;
    private bool _forceDisableStatus;
    private TutorialModuleData _overrideModule;
    private List<string> _entrypointOptions = new List<string>();
    private bool _isDependencyModule;

    private SerializedProperty _hasAdditionalPrefab;
    private SerializedProperty _defaultAdditionalMenuUIPrefabs;
    private SerializedProperty _starterAdditionalMenuUIPrefabs;
    

    private bool _defaultFold = true;
    private bool _starterFold = true;
    
    int toolbarInt = 0;
    string[] toolbarStrings = {"Default", "Starter"};

    private void OnEnable() 
    {
        _defaultMenuUIPrefab = serializedObject.FindProperty("defaultMenuUIPrefab");
        _defaultModuleScript = serializedObject.FindProperty("defaultModuleScript");
        _type = serializedObject.FindProperty("type");
        _isActive = serializedObject.FindProperty("isActive");
        _starterScript = serializedObject.FindProperty("starterScript");
        _moduleDependencies = serializedObject.FindProperty("moduleDependencies");
        _starterMenuUIPrefab = serializedObject.FindProperty("starterMenuUIPrefab");
        _isStarterActive = serializedObject.FindProperty("isStarterActive");
        _forceEnableStatus = TutorialModuleForceEnable.ForceEnableModules(Selection.activeObject.name);
        _forceDisableStatus = TutorialModuleForceEnable.IsForceDisable;
        _isDependencyModule = TutorialModuleForceEnable.IsDependency(Selection.activeObject.name);

        _hasAdditionalScripts = serializedObject.FindProperty("additionalScripts");
        _defaultHelperFiles = serializedObject.FindProperty("defaultHelperScripts");
        _starterHelperFiles = serializedObject.FindProperty("starterHelperScripts");
        
        _hasAdditionalPrefab = serializedObject.FindProperty("additionalPrefab");
        _defaultAdditionalMenuUIPrefabs = serializedObject.FindProperty("defaultAdditionalMenuUIPrefabs");
        _starterAdditionalMenuUIPrefabs = serializedObject.FindProperty("starterAdditionalMenuUIPrefabs");
        
        
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        _defaultFold = EditorGUILayout.BeginFoldoutHeaderGroup(_defaultFold, "Tutorial Module");
        if (_defaultFold)
        {

            EditorGUILayout.PropertyField(_defaultMenuUIPrefab);
            EditorGUILayout.PropertyField(_defaultModuleScript);
            
            EditorGUILayout.PropertyField(_type);
            EditorGUI.BeginDisabledGroup(_forceEnableStatus || _isDependencyModule || _forceDisableStatus);
            EditorGUILayout.PropertyField(_isActive);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();


        _starterFold = EditorGUILayout.BeginFoldoutHeaderGroup(_starterFold, "Tutorial Module Starter");
        if (_starterFold)
        {
            EditorGUILayout.PropertyField(_starterMenuUIPrefab);
            EditorGUILayout.PropertyField(_starterScript);
            EditorGUILayout.PropertyField(_isStarterActive);
            EditorGUILayout.Space();

        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUI.BeginDisabledGroup(_forceEnableStatus || _isDependencyModule);
        EditorGUILayout.PropertyField(_moduleDependencies, true);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(_hasAdditionalScripts);
        EditorGUILayout.PropertyField(_hasAdditionalPrefab);
        toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings);

        if (toolbarInt >= 0)
        {
            switch (toolbarStrings[toolbarInt])
            {
                case "Default":
                    DefaultHelperScripts();
                    break;
                case "Starter":
                    StarterHelperScripts();
                    break;
            }
        }

        serializedObject.ApplyModifiedProperties();

    }

    private void DefaultHelperScripts()
    {
        if (_hasAdditionalScripts.boolValue)
        {
            EditorGUI.BeginDisabledGroup(_isStarterActive.boolValue);
            EditorGUILayout.PropertyField(_defaultHelperFiles);
            EditorGUI.EndDisabledGroup();
        }
        
        if (_hasAdditionalPrefab.boolValue)
        {
            EditorGUI.BeginDisabledGroup(_isStarterActive.boolValue);
            EditorGUILayout.PropertyField(_defaultAdditionalMenuUIPrefabs);
            EditorGUI.EndDisabledGroup();
        }
    }

    private void StarterHelperScripts()
    {
        if (_hasAdditionalScripts.boolValue)
        {
            EditorGUI.BeginDisabledGroup(!_isStarterActive.boolValue);
            EditorGUILayout.PropertyField(_starterHelperFiles);
            EditorGUI.EndDisabledGroup();
        }
        
        if (_hasAdditionalPrefab.boolValue)
        {
            EditorGUI.BeginDisabledGroup(!_isStarterActive.boolValue);
            EditorGUILayout.PropertyField(_starterAdditionalMenuUIPrefabs);
            EditorGUI.EndDisabledGroup();
        }
    }
}
#endif
