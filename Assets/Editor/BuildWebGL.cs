using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildWebGL
{
    public static void Execute()
    {
        // Disable compression เพื่อ Vercel compatibility
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes           = new[] { "Assets/MainScene.unity" },
            locationPathName = "WebBuild",
            target           = BuildTarget.WebGL,
            options          = BuildOptions.None
        };

        BuildReport  report  = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
            Debug.Log($"WebGL Build succeeded — size: {summary.totalSize / 1024 / 1024} MB");
        else
            Debug.LogError($"WebGL Build FAILED: {summary.result}");
    }
}
