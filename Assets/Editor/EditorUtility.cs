#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


public class EditorUtility
{
    private const string GameModeFolder = @"Assets\GameMode";

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
    static void CheckSteamConfiguration()
    {
        DefaultEngineIniReader.ReadConfiguration();
        var strVal = DefaultEngineIniReader.Get("SteamWorks", "appId");
        if (!String.IsNullOrEmpty(strVal))
        {
            if (uint.TryParse(strVal, out var uintVal))
            {
                string strCWDPath = Directory.GetCurrentDirectory();
                string strSteamAppIdPath = Path.Combine(strCWDPath, "steam_appid.txt");
                if (File.Exists(strSteamAppIdPath)) {
                    using var sr = new StreamReader(strSteamAppIdPath);
                    var content = sr.ReadLine();
                    if (!strVal.Equals(content))
                    {
                        File.WriteAllText(strSteamAppIdPath, strVal);
                        Debug.Log("update steam_appid to "+strVal);
                    }
                }
            }
        }
    }


}
#endif