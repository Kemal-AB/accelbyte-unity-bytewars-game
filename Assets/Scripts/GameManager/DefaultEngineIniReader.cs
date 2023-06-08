using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DefaultEngineIniReader : MonoBehaviour
{
    private static string _defaultEnginePath;
    private static bool _isInitialized;
    private static bool _isInitializing;

    public static DefaultEngineIniReader Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        if (!_isInitialized && !_isInitializing)
        {
            _isInitializing = true;
            ReadConfiguration();
            _isInitializing = false;
            _isInitialized = true;
        }
    }

    private static string GetDefaultEngineIniPath()
    {
        if (String.IsNullOrEmpty(_defaultEnginePath))
        {
            //https://docs.unity3d.com/ScriptReference/Application-dataPath.html
            var s = Path.DirectorySeparatorChar;
            _defaultEnginePath = Application.dataPath + s + "Config" + s + "DefaultEngine.ini";
        }
        return _defaultEnginePath;
    }

    private static Dictionary<string, Dictionary<string, string>> _config =
        new Dictionary<string, Dictionary<string, string>>();
    public static void ReadConfiguration()
    {
        var path = GetDefaultEngineIniPath();
        if (File.Exists(path))
        {
            using var sr = new StreamReader(path);
            string line;
            string theSection = "";
            string theKey = "";
            string theValue = "";
            while (!string.IsNullOrEmpty(line = sr.ReadLine())) {
                line.Trim();
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    theSection = line.Substring(1, line.Length - 2);
                }
                else
                {
                    string[] ln = line.Split(new char[] { '=' });
                    theKey = ln[0].Trim();
                    theValue = ln[1].Trim();
                }
                if (theSection == "" || theKey == "" || theValue == "")
                    continue;
                PopulateIni(theSection, theKey, theValue);
            }
        }
    }
    
    private static void PopulateIni(string section, string key, string value) {
        if (_config.TryGetValue(section, out var dict))
        {
            if (!dict.TryGetValue(key, out var val))
            {
                dict.Add(key, value);
            }
        }
        else
        {
            _config.Add(section, new Dictionary<string, string>()
            {
                {key,value}
            });
        }
    }

    public static string Get(string section, string key)
    {
        if (_config.TryGetValue(section, out var dict))
        {
            if (dict.TryGetValue(key, out var value))
            {
                return value;
            }
        }
        else
        {
            Debug.LogWarning($"can't find section:{section}");
        }
        return "";
    }
}
