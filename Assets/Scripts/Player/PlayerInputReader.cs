using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// อ่าน input จาก WASD / Gamepad / On-Screen Stick แล้ว expose เป็น property ให้ component อื่นใช้
///
/// SRP: คลาสนี้รู้แค่ "input คืออะไร" — ไม่รู้เรื่อง physics, movement, หรือ camera
/// เปลี่ยน input scheme ในอนาคต (เช่น add gyroscope) แก้แค่ไฟล์นี้ — ไม่กระทบ locomotion
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerInput))]
public class PlayerInputReader : MonoBehaviour
{
    /// <summary>ทิศทาง input บนระนาบ XZ (-1..1, -1..1) — readonly สำหรับ subscriber</summary>
    public Vector2 MoveInput { get; private set; }

    // SendMessages handler — Unity Input System จะเรียก method นี้อัตโนมัติเมื่อ action "Move" trigger
    // (PlayerInput component ต้องตั้ง Behavior = "Send Messages")
    private void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }
}
