#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SteamEditorUtility
{
    [InitializeOnLoadMethod]
    private static async void CheckSteamConfiguration()
    {
        var tutorialModuleConfig = (TextAsset)Resources.Load(GConfig.ConfigurationPath);
        var json = JsonUtility.FromJson<TutorialModuleConfig>(tutorialModuleConfig.text);
        if (json.steamConfiguration == null)
            return;
        string newSteamAppId = json.steamConfiguration.steamAppId;
        if (uint.TryParse(newSteamAppId, out var newSteamAppIdUint))
        {
            string strCwdPath = Directory.GetCurrentDirectory();
            string strSteamAppIdPath = Path.Combine(strCwdPath, "steam_appid.txt");
            if (File.Exists(strSteamAppIdPath)) {
                using var sr = new StreamReader(strSteamAppIdPath);
                var content = await sr.ReadLineAsync();
                sr.Close();
                if (!newSteamAppId.Equals(content))
                {
                    try
                    {
                        await File.WriteAllTextAsync(strSteamAppIdPath, newSteamAppId);
                        Debug.Log("update steam_appid to "+newSteamAppId);
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
#endif
