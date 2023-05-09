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
    private static void BuildWindowsClient()
    {
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "../WindowsClient/accelbute-unity-bytewars.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build client successful - Build written to: {options.locationPathName}");
        }
        else if(report.summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build client failed");
        }
    }
    
    [MenuItem("Build/Build Server")]
    private static void BuildLinuxServer()
    {
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.LinuxHeadlessSimulation, BuildTarget.StandaloneLinux64);
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "../LinuxDedicatedServer/accelbyte-unity-bytewars-server.X86_64",
            target = BuildTarget.StandaloneLinux64,
            subtarget = (int)StandaloneBuildSubtarget.Server,
            options = BuildOptions.None
        };
        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build server successful - Build written to: {options.locationPathName}");
        }
        else if(report.summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build server failed");
        }
    }
}
