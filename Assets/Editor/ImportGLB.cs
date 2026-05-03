using UnityEditor;
using UnityEngine;
using System.IO;

public class ImportGLB
{
    public static void Execute()
    {
        string src = @"C:\Users\PUD\Desktop\julaporn.glb";
        string dst = Path.Combine(Application.dataPath, "julaporn.glb");

        if (!File.Exists(src))
        {
            UnityEngine.Debug.LogError($"[ImportGLB] ไม่พบไฟล์ต้นทาง: {src}");
            return;
        }

        File.Copy(src, dst, true);
        AssetDatabase.Refresh();
        UnityEngine.Debug.Log($"[ImportGLB] Import สำเร็จ → {dst}");
    }
}
