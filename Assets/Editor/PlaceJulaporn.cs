using UnityEditor;
using UnityEngine;

public class PlaceJulaporn
{
    public static void Execute()
    {
        string assetPath = "Assets/julaporn.glb";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

        if (prefab == null)
        {
            Debug.LogError("[PlaceJulaporn] ไม่พบ asset: " + assetPath + " — ลอง AssetDatabase.Refresh() ก่อน");
            AssetDatabase.Refresh();
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = "Julaporn";
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        Undo.RegisterCreatedObjectUndo(instance, "Place Julaporn GLB");
        Selection.activeGameObject = instance;

        Debug.Log("[PlaceJulaporn] วาง Julaporn.glb ที่ (0,0,0) สำเร็จ");
    }
}
