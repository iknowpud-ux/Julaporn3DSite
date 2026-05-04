using UnityEngine;

/// <summary>
/// ตรวจสอบว่า player แตะพื้นหรือไม่ — เปิด API IsGrounded ให้ component อื่นใช้
///
/// SRP: คลาสนี้รู้แค่ "อยู่บนพื้นไหม" — ไม่รู้เรื่อง movement หรือ jump logic
/// อนาคต swap algorithm (Raycast → SphereCast → CapsuleCast) แก้แค่ไฟล์นี้
/// </summary>
[DisallowMultipleComponent]
public class GroundChecker : MonoBehaviour
{
    [Tooltip("ระยะ raycast ลงล่างจากจุดศูนย์กลาง (m)")]
    [SerializeField, Range(0.1f, 3f)] private float distance = 0.6f;

    [Tooltip("layer ของพื้น — ตั้งให้ตรงกับ layer ของ Plane/Terrain")]
    [SerializeField] private LayerMask groundMask = ~0;

    /// <summary>true เมื่อมี collider อยู่ใต้ player ภายในระยะ distance</summary>
    public bool IsGrounded { get; private set; }

    private void FixedUpdate()
    {
        // ใช้ FixedUpdate เพราะ physics-based — sync กับ Rigidbody integration step
        IsGrounded = Physics.Raycast(transform.position, Vector3.down, distance, groundMask);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * distance);
    }
#endif
}
