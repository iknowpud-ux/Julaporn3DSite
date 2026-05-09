using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;

namespace Julaporn.EditorTools
{
    /// <summary>
    /// Reset scene → ลบ post-FX volume + stress test lights, spawn 20 point lights, ambient flat #232737 ×3
    /// </summary>
    public static class ResetSceneLighting
    {
        public static void Execute()
        {
            if (!SceneEditorOps.EnsureNotPlayMode()) return;

            // 1. ลบ post-fx + stress test (idempotent — ไม่ throw ถ้าไม่มีอยู่แล้ว)
            SceneEditorOps.DeleteAllByName("Global Volume");
            SceneEditorOps.DeleteAssetSafe("Assets/Settings/PostFX_Profile.asset");
            SceneEditorOps.DeleteAllByName("StressTestLights");
            SceneEditorOps.DeleteAssetSafe("Assets/Materials/Mat_BulbEmissive.mat");

            // 2. Camera: ปิด post-processing
            var camGO = GameObject.Find("Main Camera");
            if (camGO != null)
            {
                var camData = camGO.GetComponent<UniversalAdditionalCameraData>();
                if (camData != null) camData.renderPostProcessing = false;
            }

            // 3. Spawn 20 point lights (Replace = ลบเก่า ทุกตัวที่ชื่อซ้ำ + สร้างใหม่)
            var root = SceneEditorOps.ReplaceGameObject("PointLights");
            Vector3 center = new Vector3(124f, 0f, 417f);
            float radius = 300f;

            Random.InitState(7);
            for (int i = 1; i <= 20; i++)
            {
                var go = new GameObject($"PointLight_{i}");
                go.transform.SetParent(root.transform, false);

                Vector2 offset = Random.insideUnitCircle * radius;
                go.transform.position = new Vector3(
                    center.x + offset.x,
                    Random.Range(8f, 25f),
                    center.z + offset.y);

                var l = go.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = Color.white;
                l.range = 1500f;
                l.intensity = 60f;
                l.shadows = LightShadows.None;

                // gizmo sphere — เห็นใน Scene view เท่านั้น (ไม่โผล่ Game view, ไม่ติด build)
                go.AddComponent<LightGizmo>();
            }
            Debug.Log("[Reset] spawned 20 PointLight_1..20 (range 1500, intensity 60)");

            // 4. Ambient = Flat HDR — #232737 × 3
            float ambientMul = 3f;
            RenderSettings.ambientMode = AmbientMode.Flat;
            Color baseColor = HexToColor("#232737");
            RenderSettings.ambientLight = baseColor * ambientMul;
            DynamicGI.UpdateEnvironment();
            Debug.Log($"[Reset] ambient = Flat #232737 ×{ambientMul} = ({baseColor.r * ambientMul:F2}, {baseColor.g * ambientMul:F2}, {baseColor.b * ambientMul:F2})");

            SceneEditorOps.MarkDirtyAndSave();
            Debug.Log("[Reset] ✅ Done");
        }

        private static Color HexToColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }
    }
}
