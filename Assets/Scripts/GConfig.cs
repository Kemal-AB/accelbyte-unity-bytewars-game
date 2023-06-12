using System;
using System.Collections.Generic;

public class GConfig
{
    public const string ConfigurationPath = "Modules/TutorialModuleConfig";
    public static string GetSteamAppId()
    {
        var steamConfig = GetSteamConfiguration();
        if (steamConfig != null)
        {
            return steamConfig.steamAppId;
        }
        return "";
    }

    public static bool GetSteamAutoLogin()
    {
        var steamConfig = GetSteamConfiguration();
        if (steamConfig != null)
        {
            return steamConfig.autoLogin;
        }
        return true;
    }
    private static SteamConfiguration GetSteamConfiguration()
    {
        if (ConfigurationReader.Config != null)
        {
            return ConfigurationReader.Config.steamConfiguration;
        }
        return null;
    }
}