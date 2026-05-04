using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class JoystickSetup
{
    public static void Execute()
    {
        SetupPlayerInput();
        EnsureEventSystem();
        CreateJoystickUI();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[JoystickSetup] Done — PlayerInput + EventSystem + Canvas + SwitchView Button สำเร็จ");
    }

    static void EnsureEventSystem()
    {
        // ตรวจหา EventSystem ใน scene — ถ้ามีอยู่แล้วไม่สร้างซ้ำ
        var existing = Object.FindFirstObjectByType<EventSystem>();
        if (existing != null)
        {
            // ถ้ามี EventSystem แต่ยังใช้ StandaloneInputModule (legacy) → swap เป็น InputSystemUIInputModule
            var legacy = existing.GetComponent<StandaloneInputModule>();
            if (legacy != null)
            {
                Object.DestroyImmediate(legacy);
                existing.gameObject.AddComponent<InputSystemUIInputModule>();
                Debug.Log("[JoystickSetup] Swapped legacy StandaloneInputModule → InputSystemUIInputModule");
            }
            return;
        }

        // ไม่มี → สร้างใหม่
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>(); // ใช้ตัวนี้เพราะ project ใช้ Input System (ไม่ใช่ StandaloneInputModule)
        Debug.Log("[JoystickSetup] + EventSystem (with InputSystemUIInputModule)");
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

        var stick               = handleGO.AddComponent<OnScreenStick>();
        stick.controlPath       = "<Gamepad>/leftStick";
        stick.movementRange     = 60f;

        var so = new SerializedObject(vj);
        so.FindProperty("joystickRoot").objectReferenceValue = rootGO;
        so.ApplyModifiedProperties();

        // --- Switch View Button — มุมขวาบน ---
        CreateSwitchViewButton(canvasGO);

        Debug.Log("[JoystickSetup] Canvas + OnScreenStick + SwitchView Button created");
    }

    static void CreateSwitchViewButton(GameObject canvasGO)
    {
        var btnGO  = new GameObject("SwitchViewButton");
        btnGO.transform.SetParent(canvasGO.transform, false);

        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.1f, 0.1f, 0.1f, 0.6f);

        var btnRT              = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin        = new Vector2(1f, 1f);
        btnRT.anchorMax        = new Vector2(1f, 1f);
        btnRT.pivot            = new Vector2(1f, 1f);
        btnRT.anchoredPosition = new Vector2(-40f, -40f);
        btnRT.sizeDelta        = new Vector2(220f, 110f);

        var btn = btnGO.AddComponent<Button>();

        // --- Label child ---
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(btnGO.transform, false);
        var label   = labelGO.AddComponent<Text>();
        label.text  = "🎮 2.5D";
        label.alignment = TextAnchor.MiddleCenter;
        label.color     = Color.white;
        label.fontSize  = 42;
        label.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var labelRT       = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = Vector2.zero;
        labelRT.offsetMax = Vector2.zero;

        // --- CameraViewSwitcher script ---
        var switcher = btnGO.AddComponent<CameraViewSwitcher>();
        var so = new SerializedObject(switcher);
        so.FindProperty("label").objectReferenceValue = label;

        // ลาก CameraController reference
        var cam = Object.FindFirstObjectByType<CameraController>();
        if (cam != null)
            so.FindProperty("cameraController").objectReferenceValue = cam;

        so.ApplyModifiedProperties();
    }
}
