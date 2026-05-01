using UnityEngine;

// [Single Responsibility] แค่ show/hide joystick root ตาม device type
// ไม่รู้จัก input หรือ physics
public class VirtualJoystick : MonoBehaviour
{
    [Tooltip("Root GameObject ของ joystick UI — ซ่อนบน desktop, แสดงบน touch device")]
    [SerializeField] private GameObject joystickRoot;

    private void Awake()
    {
        if (joystickRoot == null) return;

        // Input.touchSupported ทำงานถูกต้องบน WebGL — browser report ผ่าน pointer events
        bool isMobile = Input.touchSupported || SystemInfo.deviceType == DeviceType.Handheld;
        joystickRoot.SetActive(isMobile);
    }
}
