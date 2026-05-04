using UnityEngine;

/// <summary>
/// Show/hide joystick root ตาม device type
/// การรับ input จริงทำโดย OnScreenStick (Input System) บน Handle child
///
/// Editor: force show เสมอ — ไว้ทดสอบ touch UI บน desktop
/// Build: แสดงเฉพาะ mobile platform (Application.isMobilePlatform)
/// </summary>
public class VirtualJoystick : MonoBehaviour
{
    [Tooltip("JoystickRoot GameObject — แสดงบน touch/mobile, ซ่อนบน desktop build")]
    [SerializeField] private GameObject joystickRoot;

    [Tooltip("ติ๊กเพื่อ force แสดงเสมอแม้บน desktop build (สำหรับ debug)")]
    [SerializeField] private bool forceShow = false;

    private void Awake()
    {
        if (joystickRoot == null) return;

        bool show = forceShow || Application.isMobilePlatform;
#if UNITY_EDITOR
        // ใน Editor force show — เพื่อทดสอบ joystick บน desktop ผ่าน mouse drag
        show = true;
#endif
        joystickRoot.SetActive(show);
    }
}
