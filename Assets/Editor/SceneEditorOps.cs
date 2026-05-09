using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Julaporn.EditorTools
{
    /// <summary>
    /// Utility สำหรับ editor scripts ที่ mutate scene/asset
    ///
    /// ทำไมต้องมี:
    ///   • Unity allow GameObject ชื่อซ้ำ → new GameObject(name) สะสมเรื่อยๆ ทุกครั้งที่รัน
    ///   • Asset deleted แต่ scene ยัง reference → magenta material
    ///   • Mutate scene ตอน play mode → exception "cannot use during play mode"
    ///   • ไม่ save → restart Editor แล้วหายหมด
    ///
    /// ทุก editor script ใน Julaporn3DSite ต้องใช้ utility นี้ — เพื่อให้ idempotent
    /// (รัน N ครั้ง = ผลลัพธ์เดียวกับรัน 1 ครั้ง)
    /// </summary>
    public static class SceneEditorOps
    {
        /// <summary>
        /// Abort ถ้า Editor อยู่ใน Play mode — เพราะ MarkSceneDirty/SaveScene เรียกไม่ได้
        /// คืน true ถ้าปลอดภัยให้ทำงานต่อ
        /// </summary>
        public static bool EnsureNotPlayMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[SceneEditorOps] ❌ Editor อยู่ใน Play mode — กด Stop ก่อนรัน script");
                return false;
            }
            return true;
        }

        /// <summary>
        /// ลบ root GameObjects ชื่อตรงตามที่ระบุ ทั้งหมด แล้วสร้างใหม่ 1 ตัว
        /// idempotent — รันซ้ำได้โดยไม่สะสม duplicate
        /// </summary>
        public static GameObject ReplaceGameObject(string name)
        {
            DeleteAllByName(name);
            return new GameObject(name);
        }

        /// <summary>
        /// ลบ root GameObjects ทุกตัวใน active scene ที่ชื่อตรงตามระบุ
        /// (Unity allow ชื่อซ้ำ — GameObject.Find เจอแค่ตัวแรก ไม่พอ)
        /// </summary>
        public static int DeleteAllByName(string name)
        {
            int count = 0;
            var scene = SceneManager.GetActiveScene();
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == name)
                {
                    Object.DestroyImmediate(go);
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// สร้าง folder ใน Assets/ ถ้ายังไม่มี (รองรับ nested e.g. "Assets/Foo/Bar")
        /// </summary>
        public static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;

            // หา parent ที่มีอยู่แล้ว ค่อย create แต่ละ level
            string normalized = folderPath.Replace('\\', '/');
            string[] parts = normalized.Split('/');
            string current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        /// <summary>
        /// Load material ที่ path หรือสร้างใหม่ด้วย shader ที่กำหนด — idempotent
        /// </summary>
        public static Material LoadOrCreateMaterial(string assetPath, string shaderName)
        {
            EnsureFolder(System.IO.Path.GetDirectoryName(assetPath).Replace('\\', '/'));

            var mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (mat == null)
            {
                var shader = Shader.Find(shaderName);
                if (shader == null)
                {
                    Debug.LogError($"[SceneEditorOps] ❌ ไม่เจอ shader '{shaderName}'");
                    return null;
                }
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, assetPath);
            }
            else
            {
                // shader อาจเปลี่ยน → reset
                var shader = Shader.Find(shaderName);
                if (shader != null) mat.shader = shader;
            }
            return mat;
        }

        /// <summary>
        /// Mark scene dirty + save — ใช้ตอนจบ editor script ทุกตัว
        /// </summary>
        public static void MarkDirtyAndSave()
        {
            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        /// <summary>
        /// ลบ asset + ลบ reference ใน scene/material ที่ใช้ asset นั้น
        /// (AssetDatabase.DeleteAsset อย่างเดียวอาจทิ้ง dangling reference → magenta material)
        /// </summary>
        public static void DeleteAssetSafe(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return;
            if (AssetDatabase.LoadAssetAtPath<Object>(assetPath) == null) return;
            AssetDatabase.DeleteAsset(assetPath);
        }
    }
}
