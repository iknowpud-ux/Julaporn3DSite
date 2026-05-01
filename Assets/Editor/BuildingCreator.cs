using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class BuildingCreator
{
    public static void Execute()
    {
        // ตั้งค่า Rigidbody constraints บน Player ก่อนสร้าง buildings
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // FreezeRotation ป้องกัน sphere กลิ้งล้มเมื่อชนกับวัตถุ
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                EditorUtility.SetDirty(rb);
            }
        }

        // seed คงที่ให้ผลลัพธ์เหมือนกันทุกครั้ง
        Random.InitState(99);

        for (int i = 0; i < 10; i++)
        {
            float angle  = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(5f, 20f);
            float x      = Mathf.Cos(angle) * radius;
            float z      = Mathf.Sin(angle) * radius;
            float bHeight = Random.Range(5f, 15f);

            GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = "Building_" + (i + 1);

            // วาง pivot ที่ฐาน — position.y = ครึ่งความสูง
            building.transform.position   = new Vector3(x, bHeight * 0.5f, z);
            building.transform.localScale = new Vector3(3f, bHeight, 3f);

            Undo.RegisterCreatedObjectUndo(building, "Create Building");
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[BuildingCreator] Created 10 buildings + set Rigidbody FreezeRotation");
    }
}
