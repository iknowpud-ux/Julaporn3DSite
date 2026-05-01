using UnityEditor;
using UnityEngine;

public class SceneChecker
{
    public static string Execute()
    {
        var scenes = EditorBuildSettings.scenes;
        if (scenes.Length == 0)
            return "NO SCENES IN BUILD SETTINGS";

        var sb = new System.Text.StringBuilder();
        foreach (var s in scenes)
            sb.AppendLine($"{(s.enabled ? "ON" : "OFF")} | {s.path}");

        return sb.ToString();
    }
}
