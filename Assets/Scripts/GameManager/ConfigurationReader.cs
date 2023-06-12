using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigurationReader : MonoBehaviour
{
    private static string _defaultEnginePath;
    private static bool _isInitialized;
    private static bool _isInitializing;
    public static ConfigurationReader Instance { get; private set; }
    public static TutorialModuleConfig Config => _config;
    private static TutorialModuleConfig _config;

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

    static void ReadConfiguration()
    {
        var tutorialModuleConfig = (TextAsset)Resources.Load(GConfig.ConfigurationPath);
        _config = JsonUtility.FromJson<TutorialModuleConfig>(tutorialModuleConfig.text);
    }
}
