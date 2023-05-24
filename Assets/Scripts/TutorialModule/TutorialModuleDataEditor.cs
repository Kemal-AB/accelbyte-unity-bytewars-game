using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(TutorialModuleData),true)]
public class TutorialModuleDataEditor : Editor
{
    private SerializedProperty _prefab;
    private SerializedProperty _type;
    private SerializedProperty _isActive;
    private SerializedProperty _entryPoint;
    private SerializedProperty _moduleDependencies;
    private bool _overrideStatus;
    private TutorialModuleData _overrideModule;
    private List<string> _entrypointOptions = new List<string>();
    private bool _isDependencyModule;

    private void OnEnable() 
    {
        _prefab = serializedObject.FindProperty("prefab");
        _type = serializedObject.FindProperty("type");
        _isActive = serializedObject.FindProperty("isActive");
        _moduleDependencies = serializedObject.FindProperty("moduleDependencies");
        // _overrideStatus = TutorialModuleOverride.OverrideDependencyModules(_moduleDependencies.serializedObject.targetObject);
        _overrideStatus = TutorialModuleOverride.OverrideModules();
        _isDependencyModule = TutorialModuleOverride.IsDependency(Selection.activeObject.name);
        Debug.Log($"is this CA dependant {_isDependencyModule}");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_prefab);
        EditorGUILayout.PropertyField(_type);
        EditorGUI.BeginDisabledGroup(_overrideStatus || _isDependencyModule);
        EditorGUILayout.PropertyField(_isActive);
        EditorGUILayout.PropertyField(_moduleDependencies, new GUIContent("Label Text"), true);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.Space();
        serializedObject.ApplyModifiedProperties();

    }
}
#endif
