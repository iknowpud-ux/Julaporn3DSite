using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Tooltip("Transform ของ Player ที่กล้องจะติดตาม — ลาก Player มาใส่ใน Inspector")]
    [SerializeField] private Transform target;

    [Tooltip("ระยะห่างในแนวลึกจาก Player")]
    [SerializeField] private float distance = 5f;

    [Tooltip("ความสูงที่กล้องลอยเหนือ Player")]
    [SerializeField] private float height = 2f;

    [Tooltip("ความไวหมุนซ้าย-ขวา")]
    [SerializeField] private float sensitivityX = 3f;

    [Tooltip("ความไวหมุนบน-ล่าง")]
    [SerializeField] private float sensitivityY = 2f;

    [Tooltip("มุมก้มต่ำสุด (องศา)")]
    [SerializeField] private float minPitch = -20f;

    [Tooltip("มุมเงยสูงสุด (องศา)")]
    [SerializeField] private float maxPitch = 60f;

    private float _yaw;
    private float _pitch;
    private bool _isLocked;

    private void Update()
    {
        HandleCursorLock();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        if (_isLocked)
            RotateByMouse();

        FollowTarget();
    }

    private void HandleCursorLock()
    {
        // WebGL ต้องการ user gesture ก่อนถึงจะ lock cursor ได้ — click คือ gesture นั้น
        if (!_isLocked && Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
            _isLocked        = true;
        }

        // ESC ปลด lock เพื่อให้ user กลับมาใช้ cursor ปกติได้
        if (_isLocked && Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            _isLocked        = false;
        }
    }

    private void RotateByMouse()
    {
        _yaw   += Input.GetAxis("Mouse X") * sensitivityX;
        _pitch -= Input.GetAxis("Mouse Y") * sensitivityY;
        _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);
    }

    private void FollowTarget()
    {
        // Spherical coordinate รอบ target — ป้องกัน gimbal lock
        Quaternion rot    = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3    offset = rot * new Vector3(0f, height, -distance);

        transform.position = target.position + offset;
        transform.LookAt(target.position + Vector3.up * (height * 0.5f));
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, target.position);
    }
#endif
}
