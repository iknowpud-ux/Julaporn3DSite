using UnityEngine;
using UnityEditor;
using Julaporn.Pins;

namespace Julaporn.EditorTools
{
    /// <summary>
    /// Setup pin system (idempotent — รันซ้ำได้ ดู CLAUDE.md §17):
    ///   1. สร้าง/อัปเดต LampDataAsset (5 sample lamps near focus 124,0,417)
    ///   2. สร้าง/อัปเดต Pin prefab (Sphere + PinMarker + SphereCollider + Mat_Pin)
    ///   3. ReplaceGameObject("PinManager") ใน scene + assign refs
    ///
    /// รัน: Tools → Julaporn → Setup Pins (หรือผ่าน Coplay MCP execute_script)
    /// </summary>
    public static class SetupPins
    {
        private const string DataPath   = "Assets/Data/Lamps.asset";
        private const string PinPrefabPath = "Assets/Prefabs/Pin.prefab";
        private const string PinMaterialPath = "Assets/Materials/Mat_Pin.mat";

        [MenuItem("Tools/Julaporn/Setup Pins")]
        public static void Execute()
        {
            if (!SceneEditorOps.EnsureNotPlayMode()) return;

            // 1. data asset
            var data = CreateOrUpdateLampData();

            // 2. material + prefab
            var pinMat = CreatePinMaterial();
            var pinPrefab = CreateOrUpdatePinPrefab(pinMat);

            // 3. scene GameObject
            var managerGO = SceneEditorOps.ReplaceGameObject("PinManager");
            var manager = managerGO.AddComponent<PinManager>();

            // assign refs ผ่าน SerializedObject (กัน private field write ตรงๆ ไม่ persist)
            var so = new SerializedObject(manager);
            so.FindProperty("lampData").objectReferenceValue = data;
            so.FindProperty("pinPrefab").objectReferenceValue = pinPrefab.GetComponent<PinMarker>();

            // หา OrbitCameraController ใน scene ใส่ให้ด้วย
            var orbitCam = Object.FindFirstObjectByType<OrbitCameraController>();
            if (orbitCam != null)
            {
                so.FindProperty("orbitCamera").objectReferenceValue = orbitCam;
                so.FindProperty("raycastCamera").objectReferenceValue = orbitCam.GetComponent<Camera>();
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            SceneEditorOps.MarkDirtyAndSave();
            Debug.Log($"[SetupPins] ✅ Done — {data.lamps.Length} lamps, prefab @ {PinPrefabPath}");
        }

        private static LampDataAsset CreateOrUpdateLampData()
        {
            SceneEditorOps.EnsureFolder("Assets/Data");
            var data = AssetDatabase.LoadAssetAtPath<LampDataAsset>(DataPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<LampDataAsset>();
                AssetDatabase.CreateAsset(data, DataPath);
            }

            // 5 lamp positions — ใกล้ camera focus (124, 0, 417), y=80 (ลอยเหนือตึก)
            // mirror id กับ WebOverlay/src/data.ts
            data.lamps = new[]
            {
                NewLamp("LP-A012423", LampDataAsset.Status.Off,        new Vector3(  50f, 80f, 380f)),
                NewLamp("LP-A012406", LampDataAsset.Status.NeedRepair, new Vector3( 130f, 80f, 420f)),
                NewLamp("LP-A012417", LampDataAsset.Status.On,         new Vector3( 210f, 80f, 400f)),
                NewLamp("LP-A012438", LampDataAsset.Status.Off,        new Vector3(  80f, 80f, 460f)),
                NewLamp("LP-A012414", LampDataAsset.Status.On,         new Vector3( 180f, 80f, 480f)),
            };
            EditorUtility.SetDirty(data);
            return data;
        }

        private static LampDataAsset.Lamp NewLamp(string id, LampDataAsset.Status s, Vector3 pos)
            => new LampDataAsset.Lamp { id = id, status = s, worldPosition = pos };

        private static Material CreatePinMaterial()
        {
            // URP Lit + emission keyword → _EmissionColor (HDR) bloom ได้
            // (เลิกใช้ Unlit เพราะ _EmissionColor ไม่ทำงาน → ไม่มี glow)
            var mat = SceneEditorOps.LoadOrCreateMaterial(PinMaterialPath, "Universal Render Pipeline/Lit");
            if (mat == null) return null;

            var defaultColor = new Color(0.31f, 0.53f, 0.97f);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", defaultColor);
            // เปิด emission keyword + initial color (PinMarker.Configure จะ override ต่อ)
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                mat.SetColor("_EmissionColor", defaultColor * 3f); // baseEmission default
            }
            // ลด specular/smoothness ให้ pin ดูเป็น emissive sphere ล้วน
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.1f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static GameObject CreateOrUpdatePinPrefab(Material pinMat)
        {
            SceneEditorOps.EnsureFolder("Assets/Prefabs");

            // ลบ prefab เก่าทุกครั้งแล้วสร้างใหม่ — กัน orphaned components
            SceneEditorOps.DeleteAssetSafe(PinPrefabPath);

            // สร้าง template GO ใน scene ชั่วคราว
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Pin";
            // scale = 1 (unit sphere) — PinMarker.baseScale (default 9) จะ override ใน Awake
            // เก็บ source of truth ไว้ที่ PinMarker เพื่อ tune ผ่าน inspector ครั้งเดียว
            go.transform.localScale = Vector3.one;

            // primitive Sphere มี SphereCollider อยู่แล้ว — แค่ใช้ต่อ
            var col = go.GetComponent<SphereCollider>();
            col.isTrigger = false; // ให้ Physics.Raycast hit ได้ตรง

            // assign material
            var rend = go.GetComponent<MeshRenderer>();
            rend.sharedMaterial = pinMat;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;

            // attach script
            go.AddComponent<PinMarker>();

            // save as prefab + ลบ scene instance ทิ้ง
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PinPrefabPath);
            Object.DestroyImmediate(go);

            return prefab;
        }
    }
}
