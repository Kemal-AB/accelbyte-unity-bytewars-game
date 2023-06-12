#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


public static class EditorUtility
{
    private const string GameModeFolder = @"Assets\GameMode";

    // static EditorUtility()
    // {
    //     EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    // }
    //
    // private static void OnPlayModeStateChanged(PlayModeStateChange state)
    // {
    //     Debug.Log($"editor play state: {state}");
    // }

    [MenuItem("AssetDatabase/Force Save Game Mode")]
    static void ForceReserializeAsset()
    {
        List<string> gameModePaths = new List<string>();
        DirectoryInfo directoryInfo = new DirectoryInfo(GameModeFolder);
        FileInfo[] fileInfos = directoryInfo.GetFiles("*.asset");
        foreach (var fileInfo in fileInfos)
        {
            gameModePaths.Add(Path.Combine(GameModeFolder,fileInfo.Name));
        }
        foreach (var gameModePath in gameModePaths)
        {
            Debug.Log(gameModePath);
        }
        AssetDatabase.ForceReserializeAssets(gameModePaths);
    }


}
#endif