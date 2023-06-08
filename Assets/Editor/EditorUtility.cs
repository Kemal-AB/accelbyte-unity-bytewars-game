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
    
    [InitializeOnLoadMethod]
    static async void CheckSteamConfiguration()
    {
        DefaultEngineIniReader.ReadConfiguration();
        var strVal = DefaultEngineIniReader.Get("SteamWorks", "appId");
        if (!String.IsNullOrEmpty(strVal))
        {
            if (uint.TryParse(strVal, out var uintVal))
            {
                string strCwdPath = Directory.GetCurrentDirectory();
                string strSteamAppIdPath = Path.Combine(strCwdPath, "steam_appid.txt");
                if (File.Exists(strSteamAppIdPath)) {
                    using var sr = new StreamReader(strSteamAppIdPath);
                    var content = await sr.ReadLineAsync();
                    sr.Close();
                    if (!strVal.Equals(content))
                    {
                        try
                        {
                            await File.WriteAllTextAsync(strSteamAppIdPath, strVal);
                            Debug.Log("update steam_appid to "+strVal);
                        }
                        catch (Exception e)
                        {
                            Debug.Log("failed to automatically update steam_appid.txt, " +
                                      "please close the file or update it manually " +
                                      $"error: {e.Message}");
                        }
                    }
                }
            }
        }
    }


}
#endif