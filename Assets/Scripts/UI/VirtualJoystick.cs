using UnityEngine;

/// <summary>
/// Show/hide joystick root ตาม device type เท่านั้น
/// การรับ input จริงทำโดย OnScreenStick (Input System) บน Handle child
/// </summary>
public class VirtualJoystick : MonoBehaviour
{
    [Tooltip("JoystickRoot GameObject — แสดงบน touch device, ซ่อนบน desktop")]
    [SerializeField] private GameObject joystickRoot;

    private void Awake()
    {
        if (joystickRoot == null) return;
        joystickRoot.SetActive(Input.touchSupported);
    }
}
