using UnityEngine;

/// <summary>
/// Fixed isometric follow camera — ล็อคมุมคงที่ ติดตาม Player แบบลื่น
/// ไม่มี rotation, ไม่ lock cursor — เข้าเล่นได้ทันที
/// Desktop: scroll wheel zoom เท่านั้น
/// Mobile: ติดตาม Player เฉยๆ ไม่มี input เพิ่มเติม
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Transform ของ Player ที่กล้องจะติดตาม — ลาก Player มาวาง")]
    [SerializeField] private Transform target;

    [Header("Locked Angle")]
    [Tooltip("มุมก้มแนวตั้ง (0=แนวนอน, 90=มองตรงลง) — 50° ได้ผล isometric style")]
    [SerializeField, Range(10f, 89f)] private float pitch = 50f;

    [Tooltip("มุมหมุนแนวนอน — 225° ให้มุม isometric มองมุมซ้าย-หน้า เหมือนในภาพ")]
    [SerializeField] private float yaw = 225f;

    [Header("Distance")]
    [Tooltip("ระยะห่างจาก Player")]
    [SerializeField, Range(3f, 60f)] private float distance = 12f;

    [Tooltip("ระยะ zoom ใกล้สุด")]
    [SerializeField] private float minDistance = 5f;

    [Tooltip("ระยะ zoom ไกลสุด")]
    [SerializeField] private float maxDistance = 25f;

    [Header("Feel")]
    [Tooltip("ความลื่นตาม Player — สูงขึ้น = ตามเร็วขึ้น")]
    [SerializeField, Range(1f, 20f)] private float followSmoothness = 8f;

    [Tooltip("ความเร็ว scroll zoom (desktop)")]
    [SerializeField] private float zoomSpeed = 8f;

    [Tooltip("ความสูง look-at offset เพื่อให้ player อยู่กึ่งกลางจอ")]
    [SerializeField] private float lookAtHeightOffset = 1f;

    private Vector3 _smoothVelocity;

    private void Awake()
    {
        // วาง camera ทันทีที่ตำแหน่งถูกต้อง ป้องกัน pop-in
        if (target != null)
            SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleZoom();
        ApplyPosition();
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
            distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
    }

    private void ApplyPosition()
    {
        // มุม pitch/yaw คงที่ — ไม่มี user input หมุนกล้อง
        Quaternion rotation   = Quaternion.Euler(pitch, yaw, 0f);
        Vector3    desiredPos = target.position + rotation * new Vector3(0f, 0f, -distance);

        transform.position = Vector3.SmoothDamp(
            transform.position, desiredPos, ref _smoothVelocity, 1f / followSmoothness);

        transform.LookAt(target.position + Vector3.up * lookAtHeightOffset);
    }

    private void SnapToTarget()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.position  = target.position + rotation * new Vector3(0f, 0f, -distance);
        transform.LookAt(target.position + Vector3.up * lookAtHeightOffset);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, target.position + Vector3.up * lookAtHeightOffset);
        Gizmos.DrawWireSphere(target.position, 0.4f);
    }
#endif
}
