using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;

namespace Julaporn.EditorTools
{
    /// <summary>
    /// เปิด URP Bloom เพื่อให้ pin (HDR emission) เกิด halo glow
    ///
    /// ทำไม Bloom (ไม่ใช่ fake billboard halo):
    ///   • ใช้ effect เดียวเปลี่ยนทั้ง scene — ภายหลังจะ glow ไฟถนน/หน้าต่างได้ฟรี
    ///   • Standard URP pipeline — ไม่ต้อง custom shader/texture
    ///   • Threshold = 0.9 → กรองเฉพาะ HDR pixel (pin) ไม่ bloom ทั่วเมือง
    ///
    /// Idempotent — สร้าง/อัปเดต VolumeProfile + GO "Global Volume"
    /// </summary>
    public static class SetupBloom
    {
        private const string ProfilePath = "Assets/Settings/Volume_Bloom.asset";

        [MenuItem("Tools/Julaporn/Setup Bloom")]
        public static void Execute()
        {
            if (!SceneEditorOps.EnsureNotPlayMode()) return;

            // 1. VolumeProfile + Bloom override
            SceneEditorOps.EnsureFolder("Assets/Settings");
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            if (!profile.TryGet<Bloom>(out var bloom))
            {
                bloom = profile.Add<Bloom>(true);
            }
            bloom.active = true;
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.9f; // เกิน 1.0 ตรงนี้ → bloom (pin emission > 1 ผ่าน, fog/ambient < 1 ไม่ผ่าน)
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.6f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.88f; // กว้างขึ้นจากเดิม 0.75 → ทำให้ halo รัศมีกว้างชัด
            bloom.clamp.overrideState = true;
            bloom.clamp.value = 65472f;  // default — กัน firefly
            bloom.tint.overrideState = false;
            bloom.highQualityFiltering.overrideState = true;
            bloom.highQualityFiltering.value = true;
            EditorUtility.SetDirty(profile);

            // 2. Global Volume GameObject
            var volumeGO = SceneEditorOps.ReplaceGameObject("Global Volume");
            var volume = volumeGO.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 0f;
            volume.profile = profile;

            // 3. เปิด post-processing บน Main Camera
            var camGO = GameObject.Find("Main Camera");
            if (camGO != null)
            {
                var camData = camGO.GetComponent<UniversalAdditionalCameraData>();
                if (camData == null) camData = camGO.AddComponent<UniversalAdditionalCameraData>();
                camData.renderPostProcessing = true;
                EditorUtility.SetDirty(camData);
            }
            else
            {
                Debug.LogWarning("[SetupBloom] ⚠ ไม่เจอ 'Main Camera' — post-FX ไม่ได้เปิด");
            }

            SceneEditorOps.MarkDirtyAndSave();
            Debug.Log("[SetupBloom] ✅ Bloom volume + camera post-FX enabled (threshold 0.9, intensity 0.6)");
        }
    }
}
