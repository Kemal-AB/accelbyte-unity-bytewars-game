using System;
using System.Collections.Generic;

public class GConfig
{
    public static bool GetBool(string section, string key, bool defaultValue)
    {
        var strVal = DefaultEngineIniReader.Get(section, key);
        if (!String.IsNullOrEmpty(strVal))
        {
            if (bool.TryParse(strVal, out var boolVal))
            {
                return boolVal;
            }
        }
        return defaultValue;
    }

    public static string GetString(string section, string key, string defaultValue)
    {
        var strVal = DefaultEngineIniReader.Get(section, key);
        if (!String.IsNullOrEmpty(strVal))
        {
            return strVal;
        }
        return defaultValue;
    }
}

public enum GConfigType
{
    AutoLoginSteam
}
