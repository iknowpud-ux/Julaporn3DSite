using UnityEditor;
using UnityEngine;

public class ApplyDarkCityMaterials
{
    public static void Execute()
    {
        // สีจาก ref: Smart City dark night theme
        Apply("Assets/Materials/Mat_MainBuilding.mat",
            new Color(0.18f, 0.22f, 0.35f),   // dark blue-slate — อาคารหลักสว่างกว่าอาคารอื่นเล็กน้อย
            metallic: 0.4f, smoothness: 0.65f,
            emission: new Color(0.04f, 0.07f, 0.18f));  // glow ฟ้าจาง

        Apply("Assets/Materials/Mat_Buildings.mat",
            new Color(0.10f, 0.12f, 0.18f),   // charcoal navy
            metallic: 0.2f, smoothness: 0.45f);

        Apply("Assets/Materials/Mat_Ground.mat",
            new Color(0.05f, 0.055f, 0.08f),  // เกือบดำ
            metallic: 0.0f, smoothness: 0.2f);

        Apply("Assets/Materials/Mat_Roads.mat",
            new Color(0.08f, 0.09f, 0.12f),   // dark gray เล็กน้อยสว่างกว่า ground
            metallic: 0.0f, smoothness: 0.35f);

        Apply("Assets/Materials/Mat_Water.mat",
            new Color(0.04f, 0.08f, 0.16f),   // dark navy blue
            metallic: 0.0f, smoothness: 0.9f,
            emission: new Color(0.01f, 0.03f, 0.08f));

        Apply("Assets/Materials/Mat_Forest.mat",
            new Color(0.05f, 0.09f, 0.06f),   // dark green muted
            metallic: 0.0f, smoothness: 0.1f);

        AssetDatabase.SaveAssets();
        Debug.Log("[ApplyDarkCityMaterials] อัปเดต material ครบทุกตัว");
    }

    static void Apply(string path, Color baseColor, float metallic = 0f, float smoothness = 0.5f, Color? emission = null)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null) { Debug.LogError("ไม่พบ: " + path); return; }

        mat.SetColor("_BaseColor", baseColor);
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);

        if (emission.HasValue)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission.Value);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
        }

        EditorUtility.SetDirty(mat);
    }
}
