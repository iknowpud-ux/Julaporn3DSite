using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

public class JoystickSetup
{
    public static void Execute()
    {
        SetupPlayerInput();
        CreateJoystickUI();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[JoystickSetup] Done — save scene + PlayerInput + Canvas สำเร็จ");
    }

    static void SetupPlayerInput()
    {
        var player = GameObject.Find("Player");
        if (player == null) { Debug.LogError("[JoystickSetup] ไม่พบ Player"); return; }

        var pi = player.GetComponent<PlayerInput>() ?? player.AddComponent<PlayerInput>();

        var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
            "Assets/InputSystem_Actions.inputactions");
        if (actions == null) { Debug.LogError("[JoystickSetup] ไม่พบ InputSystem_Actions"); return; }

        pi.actions              = actions;
        pi.notificationBehavior = PlayerNotifications.SendMessages;
        pi.defaultActionMap     = "Player";
        Debug.Log("[JoystickSetup] PlayerInput → SendMessages, ActionMap = Player");
    }

    static void CreateJoystickUI()
    {
        // ลบ Canvas เก่าถ้ามี
        var old = GameObject.Find("MobileJoystickCanvas");
        if (old != null) Object.DestroyImmediate(old);

        // --- Canvas ---
        var canvasGO = new GameObject("MobileJoystickCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // VirtualJoystick script อยู่บน Canvas
        var vj = canvasGO.AddComponent<VirtualJoystick>();

        // --- JoystickRoot (Background) — มุมล่างซ้าย ---
        // AddComponent<Image> สร้าง RectTransform อัตโนมัติ ต้อง add ก่อนจึงจะ GetComponent ได้
        var rootGO  = new GameObject("JoystickRoot");
        rootGO.transform.SetParent(canvasGO.transform, false);
        var bgImg   = rootGO.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.45f);
        var rootRT              = rootGO.GetComponent<RectTransform>();
        rootRT.anchorMin        = Vector2.zero;
        rootRT.anchorMax        = Vector2.zero;
        rootRT.pivot            = new Vector2(0.5f, 0.5f);
        rootRT.anchoredPosition = new Vector2(200f, 200f);
        rootRT.sizeDelta        = new Vector2(250f, 250f);

        // --- Handle (knob) — child ของ Background ---
        var handleGO  = new GameObject("Handle");
        handleGO.transform.SetParent(rootGO.transform, false);
        var handleImg = handleGO.AddComponent<Image>();
        handleImg.color = new Color(0.85f, 0.85f, 0.85f, 0.75f);
        var handleRT              = handleGO.GetComponent<RectTransform>();
        handleRT.anchorMin        = new Vector2(0.5f, 0.5f);
        handleRT.anchorMax        = new Vector2(0.5f, 0.5f);
        handleRT.pivot            = new Vector2(0.5f, 0.5f);
        handleRT.anchoredPosition = Vector2.zero;
        handleRT.sizeDelta        = new Vector2(120f, 120f);

        // OnScreenStick บน Handle — bind กับ <Gamepad>/leftStick
        // Move action ใน InputSystem_Actions มี binding นี้อยู่แล้ว
        var stick               = handleGO.AddComponent<OnScreenStick>();
        stick.controlPath       = "<Gamepad>/leftStick";
        stick.movementRange     = 60f;

        // ส่ง joystickRoot เข้า VirtualJoystick ผ่าน SerializedObject
        var so = new SerializedObject(vj);
        so.FindProperty("joystickRoot").objectReferenceValue = rootGO;
        so.ApplyModifiedProperties();

        Debug.Log("[JoystickSetup] Canvas + OnScreenStick created (250px bg / 120px handle / 60px range)");
    }
}
