using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Editor tool — refactor Player GameObject จาก monolithic PlayerController
/// ไปเป็น PlayerInputReader + GroundChecker + PlayerLocomotion (SRP-compliant)
///
/// วิธีใช้: Unity menu → Tools → Julaporn → Refactor Player To SRP
/// </summary>
public static class PlayerSetup
{
    [MenuItem("Tools/Julaporn/Refactor Player To SRP")]
    public static void RefactorPlayer()
    {
        var player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("[PlayerSetup] ไม่พบ GameObject ชื่อ 'Player' ใน active scene");
            return;
        }

        // 1. ลบ deprecated PlayerController stub
#pragma warning disable CS0618 // ตั้งใจอ้าง type ที่ Obsolete เพื่อ remove มัน
        var oldCtrl = player.GetComponent<PlayerController>();
        if (oldCtrl != null)
        {
            Object.DestroyImmediate(oldCtrl, true);
            Debug.Log("[PlayerSetup] ลบ PlayerController (deprecated)");
        }
#pragma warning restore CS0618

        // 2. Add 3 components ตาม SRP — idempotent: ถ้ามีแล้วไม่ add ซ้ำ
        if (player.GetComponent<PlayerInputReader>() == null)
        {
            player.AddComponent<PlayerInputReader>();
            Debug.Log("[PlayerSetup] + PlayerInputReader");
        }
        if (player.GetComponent<GroundChecker>() == null)
        {
            player.AddComponent<GroundChecker>();
            Debug.Log("[PlayerSetup] + GroundChecker");
        }
        var locomotion = player.GetComponent<PlayerLocomotion>();
        if (locomotion == null)
        {
            locomotion = player.AddComponent<PlayerLocomotion>();
            Debug.Log("[PlayerSetup] + PlayerLocomotion");
        }

        // 3. หา CameraController ใน scene แล้วลาก reference เข้า PlayerLocomotion.cameraRef
        var cam = Object.FindFirstObjectByType<CameraController>();
        if (cam != null)
        {
            var so = new SerializedObject(locomotion);
            so.FindProperty("cameraRef").objectReferenceValue = cam;
            so.ApplyModifiedProperties();
            Debug.Log($"[PlayerSetup] PlayerLocomotion.cameraRef → {cam.name}");
        }
        else
        {
            Debug.LogWarning("[PlayerSetup] ไม่พบ CameraController — PlayerLocomotion จะใช้ค่า cameraYaw default (225°)");
        }

        // 4. verify PlayerInput (Unity component) → SendMessages
        var pi = player.GetComponent<PlayerInput>();
        if (pi != null)
        {
            if (pi.notificationBehavior != PlayerNotifications.SendMessages)
            {
                pi.notificationBehavior = PlayerNotifications.SendMessages;
                EditorUtility.SetDirty(pi);
                Debug.Log("[PlayerSetup] PlayerInput.notificationBehavior → SendMessages");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerSetup] ไม่มี PlayerInput component — รัน Tools → Julaporn → Setup Joystick ก่อน");
        }

        EditorUtility.SetDirty(player);
        EditorSceneManager.MarkSceneDirty(player.scene);
        Selection.activeGameObject = player;

        Debug.Log("[PlayerSetup] ✅ Done — กด Ctrl+S บันทึก scene แล้วกด Play ทดสอบ");
    }

    [MenuItem("Tools/Julaporn/Setup Joystick (Re-create Canvas)")]
    public static void SetupJoystick() => JoystickSetup.Execute();
}
