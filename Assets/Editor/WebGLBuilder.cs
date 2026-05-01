using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public class WebGLBuilder
{
    public static void Execute()
    {
        const string outputPath = @"E:\Julaporn\Unity\Julaporn3DSite\WebBuild";
        const string targetScene = "Assets/MainScene.unity";

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

        // ตั้ง Build Settings ให้ชี้แค่ MainScene
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(targetScene, true)
        };

        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        var options = new BuildPlayerOptions
        {
            scenes           = new[] { targetScene },
            locationPathName = outputPath,
            target           = BuildTarget.WebGL,
            options          = BuildOptions.None,
        };

        Debug.Log("[WebGLBuilder] Build MainScene → WebGL...");
        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
            Debug.Log($"[WebGLBuilder] ✓ Build สำเร็จ → {outputPath}  ({report.summary.totalSize / 1024 / 1024} MB)");
        else
            Debug.LogError($"[WebGLBuilder] ✗ Build ล้มเหลว: {report.summary.result}");
    }
}
