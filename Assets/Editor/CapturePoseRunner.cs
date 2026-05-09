using System.Reflection;
using UnityEngine;

namespace Julaporn.EditorTools
{
    /// <summary>
    /// One-shot: เรียก ContextMenu method "CapturePoseFromTransform" บน Main Camera
    /// ใช้ดูด transform ปัจจุบัน → ใส่เป็น initial pose ของ OrbitCameraController
    /// </summary>
    public static class CapturePoseRunner
    {
        public static void Execute()
        {
            if (!SceneEditorOps.EnsureNotPlayMode()) return;

            var cam = GameObject.Find("Main Camera");
            if (cam == null) { Debug.LogError("[CapturePose] ไม่เจอ Main Camera"); return; }

            var controller = cam.GetComponent("OrbitCameraController") as MonoBehaviour;
            if (controller == null) { Debug.LogError("[CapturePose] ไม่เจอ OrbitCameraController"); return; }

            var method = controller.GetType().GetMethod(
                "CapturePoseFromTransform",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (method == null) { Debug.LogError("[CapturePose] หา method ไม่เจอ"); return; }

            method.Invoke(controller, null);
            SceneEditorOps.MarkDirtyAndSave();
        }
    }
}
