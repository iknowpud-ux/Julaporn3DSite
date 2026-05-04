using UnityEngine;

/// <summary>
/// ขยับ Rigidbody ตาม input ที่อ่านได้จาก PlayerInputReader
///
/// SRP: คลาสนี้รู้แค่ "ทำให้ Rigidbody เคลื่อนที่" — ไม่รู้เรื่อง input source หรือ camera
///
/// สำคัญ — ใช้ "fixed yaw rotation" แปลง input → world direction:
/// ไม่อิง Camera.main.forward runtime เพื่อตัด feedback loop
/// (ของเดิมพังเพราะ: กล้อง follow ช้ากว่า player → forward เปลี่ยน → ทิศ input เปลี่ยน → กระตุก)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputReader))]
public class PlayerLocomotion : MonoBehaviour
{
    [Tooltip("ความเร็วเดิน (m/s)")]
    [SerializeField, Range(1f, 30f)] private float moveSpeed = 12f;

    [Tooltip("มุม yaw ของกล้อง — ใช้แปลง input → world direction. ต้องตรงกับ CameraController.yaw")]
    [SerializeField] private float cameraYaw = 225f;

    [Tooltip("ลาก CameraController มา → จะ sync yaw อัตโนมัติ. ปล่อยว่างจะใช้ค่า cameraYaw ด้านบน")]
    [SerializeField] private CameraController cameraRef;

    private Rigidbody _rb;
    private PlayerInputReader _input;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _input = GetComponent<PlayerInputReader>();

        // freeze rotation ทุกแกน — กัน Rigidbody หมุนเองจาก collision/torque
        // (เป็นต้นเหตุของอาการ "เหวี่ยง" ตอน collide กับวัตถุอื่น)
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        // interpolate ลด jitter visual ตอน camera follow rate ต่างจาก physics step
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        Vector2 input = _input.MoveInput;

        // dead zone กัน drift จาก analog stick — หยุดเฉพาะ XZ คง velocity Y (gravity/jump)
        if (input.sqrMagnitude < 0.01f)
        {
            Vector3 v = _rb.linearVelocity;
            _rb.linearVelocity = new Vector3(0f, v.y, 0f);
            return;
        }

        // อ่าน yaw ตอนนี้ — รองรับการสลับ view (Iso/TP) ตอน runtime ผ่าน CameraController.ToggleView
        // ปลอดภัยจาก feedback loop เพราะ Yaw เป็น preset value ไม่ใช่ Camera.main.transform runtime
        float yaw = cameraRef != null ? cameraRef.Yaw : cameraYaw;
        Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);

        Vector3 dir = yawRotation * new Vector3(input.x, 0f, input.y).normalized;

        Vector3 vel = _rb.linearVelocity;
        vel.x = dir.x * moveSpeed;
        vel.z = dir.z * moveSpeed;
        _rb.linearVelocity = vel;
    }
}
