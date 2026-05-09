using UnityEngine;

namespace Julaporn.EditorTools
{
    /// <summary>
    /// แก้ค่า fog ของ scene — Atmospheric haze สำหรับ city overview
    /// </summary>
    public static class FogTuner
    {
        public static void Execute()
        {
            if (!SceneEditorOps.EnsureNotPlayMode()) return;

            Debug.Log($"[Fog BEFORE] enabled={RenderSettings.fog} mode={RenderSettings.fogMode} " +
                      $"start={RenderSettings.fogStartDistance} end={RenderSettings.fogEndDistance} " +
                      $"density={RenderSettings.fogDensity} color={RenderSettings.fogColor}");

            // Atmospheric haze — start 300 ตึกใกล้ใส, end 2500 ตึกฝั่งไกล fade เต็ม
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 200f;
            RenderSettings.fogEndDistance = 2000f;
            RenderSettings.fogDensity = 0.01f;
            RenderSettings.fogColor = new Color(13f / 255f, 15f / 255f, 20f / 255f, 1f); // #0D0F14

            Debug.Log($"[Fog AFTER] enabled={RenderSettings.fog} mode={RenderSettings.fogMode} " +
                      $"start={RenderSettings.fogStartDistance} end={RenderSettings.fogEndDistance}");

            SceneEditorOps.MarkDirtyAndSave();
        }
    }
}
