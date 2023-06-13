using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class Builder
{
    private static readonly string[] scenes = new[]
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/GalaxyWorld.unity"
    };
    
    [MenuItem("Build/Build Windows64 Client")]
    public static void BuildWindowsClient()
    {
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "../Build/Client/ByteWars.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        
        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[Builder.BuildWindowsClient] Build client successful - Build written to: {options.locationPathName}");
        }
        else if(report.summary.result == BuildResult.Failed)
        {
            Debug.LogError("[Builder.BuildWindowsClient] Build client failed");
        }
    }
    
    [MenuItem("Build/Build Server")]
    public static void BuildLinuxServer()
    {
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.LinuxHeadlessSimulation, BuildTarget.StandaloneLinux64);
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "../Build/Server/ByteWarsServer.x86_64",
            target = BuildTarget.StandaloneLinux64,
            subtarget = (int)StandaloneBuildSubtarget.Server,
            options = BuildOptions.None
        };
        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[Builder.BuildLinuxServer] Build server successful - Build written to: {options.locationPathName}");
        }
        else if(report.summary.result == BuildResult.Failed)
        {
            Debug.LogError("[Builder.BuildLinuxServer] Build server failed");
        }
    }

    public static void UpdateGameVersion()
    {
        string[] cmdArgs = System.Environment.GetCommandLineArgs();
        string gameVersion = "";

        foreach (string arg in cmdArgs)
        {
            if (arg.Contains("-setGameVersion="))
            {
                gameVersion = arg.Replace("-setGameVersion=", "");
                PlayerSettings.bundleVersion = gameVersion;
            }
        }
    }
}
