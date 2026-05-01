using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Tooltip("Transform ของ Player ที่กล้องจะติดตาม — ลาก Player มาใส่ใน Inspector")]
    [SerializeField] private Transform target;

    [Tooltip("ระยะห่างในแนวลึกจาก Player")]
    [SerializeField] private float distance = 5f;

    [Tooltip("ความสูงที่กล้องลอยเหนือ Player")]
    [SerializeField] private float height = 2f;

    [Tooltip("ความไวหมุนซ้าย-ขวา (Input System ส่งค่า pixel/frame — เล็กกว่า Legacy ~10x)")]
    [SerializeField] private float sensitivityX = 0.15f;

    [Tooltip("ความไวหมุนบน-ล่าง")]
    [SerializeField] private float sensitivityY = 0.15f;

    [Tooltip("มุมก้มต่ำสุด (องศา)")]
    [SerializeField] private float minPitch = -20f;

    [Tooltip("มุมเงยสูงสุด (องศา)")]
    [SerializeField] private float maxPitch = 60f;

    private float _yaw;
    private float _pitch;

    private void Start()
    {
        // WebGL: cursor lock ต้องเรียกหลัง user gesture — canvas click คือ trigger
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        RotateByMouse();
        FollowTarget();
    }

    private void RotateByMouse()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        // delta.ReadValue() คืน pixel/frame — ใหญ่กว่า GetAxis("Mouse X") มาก ต้องใช้ sensitivity ต่ำกว่าเดิม
        Vector2 delta = mouse.delta.ReadValue();
        _yaw   += delta.x * sensitivityX;
        _pitch -= delta.y * sensitivityY;
        _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);
    }

    private void FollowTarget()
    {
        // Spherical coordinate รอบ target — ง่ายกว่า lerp และไม่มี gimbal lock
        Quaternion rot    = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3    offset = rot * new Vector3(0f, height, -distance);

        transform.position = target.position + offset;
        // LookAt จุดกึ่งกลาง body แทน pivot เพื่อให้กล้องดูเป็นธรรมชาติ
        transform.LookAt(target.position + Vector3.up * (height * 0.5f));
    }
}
