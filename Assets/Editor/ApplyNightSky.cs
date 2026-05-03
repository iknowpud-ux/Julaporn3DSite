using UnityEditor;
using UnityEngine;

public class ApplyNightSky
{
    public static void Execute()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.03f, 0.05f, 0.10f);
        RenderSettings.fogStartDistance = 150f;
        RenderSettings.fogEndDistance = 500f;

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[ApplyNightSky] Fog 150–500m updated");
    }
}
