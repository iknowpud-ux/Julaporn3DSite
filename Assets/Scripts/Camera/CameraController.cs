using UnityEngine;
using UnityEngine.Serialization;

/// <summary>view mode สำหรับ CameraController</summary>
public enum CameraView
{
    Isometric25D,
    ThirdPerson
}

/// <summary>
/// Follow camera รองรับ 2 view: Isometric 2.5D (orthographic) และ Third-Person (perspective)
/// แต่ละ view เก็บ preset ของตัวเอง — เปลี่ยนผ่าน ToggleView() / SetView()
///
/// แก้ปัญหา clipping: เปิด nearClip / farClip ให้ปรับได้ — default 0.05 / 500
/// (default ของ Unity คือ 0.3 / 1000 ซึ่ง near 0.3 ตัดของใกล้ตัว)
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Transform ของ Player ที่กล้องจะติดตาม")]
    [SerializeField] private Transform target;

    [Header("Active View")]
    [Tooltip("View mode ที่ใช้ตอนเริ่มเกม")]
    [SerializeField] private CameraView currentView = CameraView.Isometric25D;

    [Header("Isometric 2.5D Preset")]
    [FormerlySerializedAs("orthographic")]
    [SerializeField] private bool isoOrthographic = true;

    [FormerlySerializedAs("orthographicSize")]
    [SerializeField, Range(5f, 80f)] private float isoOrthoSize = 18f;

    [FormerlySerializedAs("pitch")]
    [Tooltip("มุมก้มแนวตั้ง (60° = isometric, 90° = top-down)")]
    [SerializeField, Range(10f, 89f)] private float isoPitch = 60f;

    [FormerlySerializedAs("yaw")]
    [Tooltip("มุมหมุนแนวนอน — ต้อง sync กับ PlayerLocomotion ผ่าน Yaw property")]
    [SerializeField] private float isoYaw = 225f;

    [FormerlySerializedAs("distance")]
    [SerializeField, Range(10f, 200f)] private float isoDistance = 36f;

    [Tooltip("FOV ตอน Iso perspective (orthographic = false) — ต่ำ = telephoto = ดูใกล้ ortho, ลด foreshortening")]
    [SerializeField, Range(10f, 60f)] private float isoFieldOfView = 35f;

    [Header("Third Person Preset")]
    [Tooltip("มุมก้ม TP — ปกติ 15-30°")]
    [SerializeField, Range(0f, 60f)] private float tpPitch = 20f;

    [Tooltip("มุม yaw TP — 0° = หันไปทิศ +Z (north)")]
    [SerializeField] private float tpYaw = 0f;

    [SerializeField, Range(3f, 30f)] private float tpDistance = 8f;

    [Tooltip("FOV สำหรับ perspective TP")]
    [SerializeField, Range(30f, 90f)] private float tpFieldOfView = 60f;

    [Header("Third Person Mouse Look")]
    [Tooltip("ความเร็วการหมุนเมื่อ drag right mouse — สูง = หมุนไว")]
    [SerializeField, Range(0.5f, 10f)] private float mouseSensitivity = 3f;

    [Tooltip("มุม pitch ต่ำสุด (มอง level ขึ้น) — กัน flip")]
    [SerializeField, Range(-30f, 30f)] private float tpPitchMin = -10f;

    [Tooltip("มุม pitch สูงสุด (มอง top-down)")]
    [SerializeField, Range(40f, 89f)] private float tpPitchMax = 70f;

    [Header("Common")]
    [Tooltip("ความลื่นในการตาม player — สูง = ตามเร็ว")]
    [SerializeField, Range(1f, 30f)] private float followSmoothness = 12f;

    [Tooltip("offset ความสูงจุดเล็งตอน Iso — เพิ่มเพื่อเห็นตึกสูง (เดิม lookAtHeightOffset)")]
    [FormerlySerializedAs("lookAtHeightOffset")]
    [SerializeField] private float isoLookAtOffset = 25f;

    [Tooltip("offset ความสูงจุดเล็งตอน TP — ปกติ 1-2 (ระดับอกผู้เล่น)")]
    [SerializeField] private float tpLookAtOffset = 1.5f;

    [Tooltip("ความเร็ว scroll zoom (desktop)")]
    [SerializeField] private float zoomSpeed = 8f;

    [Header("Clipping (กันตึกถูกตัด)")]
    [Tooltip("Near clip plane — เล็ก = เห็นของใกล้ตัวมากขึ้น (default Unity = 0.3)")]
    [SerializeField, Range(0.01f, 1f)] private float nearClip = 0.05f;

    [Tooltip("Far clip plane — ใหญ่ = เห็นของไกลมากขึ้น")]
    [SerializeField, Range(50f, 10000f)] private float farClip = 500f;

    /// <summary>มุม yaw ปัจจุบัน — ให้ PlayerLocomotion sync (รองรับ view switch)</summary>
    public float Yaw => _activeYaw;

    /// <summary>view ปัจจุบัน — ให้ UI button อ่าน label</summary>
    public CameraView View => currentView;

    private Camera _camera;
    private Vector3 _smoothVelocity;
    private Quaternion _fixedRotation;
    private float _activePitch, _activeYaw, _activeDistance;
    private bool _activeOrthographic;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        ApplyViewPreset();
        if (target != null) SnapToTarget();
    }

    private void Update()
    {
        // mouse look เฉพาะ TP — Iso lock มุม
        if (currentView == CameraView.ThirdPerson)
            HandleMouseLook();
    }

    private void LateUpdate()
    {
        if (target == null) return;
        HandleZoom();
        ApplyPosition();
    }

    private void HandleMouseLook()
    {
        // right mouse held + drag → หมุน yaw/pitch
        // ใช้ right click เพื่อไม่ชนกับ left-click ที่ใช้กดปุ่ม UI / drag joystick
        if (!Input.GetMouseButton(1)) return;

        float dx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float dy = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _activeYaw += dx;
        _activePitch = Mathf.Clamp(_activePitch - dy, tpPitchMin, tpPitchMax);

        _fixedRotation = Quaternion.Euler(_activePitch, _activeYaw, 0f);
        transform.rotation = _fixedRotation;
    }

    /// <summary>สลับระหว่าง Iso และ TP — เรียกจาก UI Button</summary>
    public void ToggleView()
    {
        currentView = currentView == CameraView.Isometric25D
            ? CameraView.ThirdPerson
            : CameraView.Isometric25D;
        ApplyViewPreset();
    }

    /// <summary>ตั้ง view เฉพาะค่า — เรียกจากภายนอกเช่น save/load</summary>
    public void SetView(CameraView view)
    {
        currentView = view;
        ApplyViewPreset();
    }

    private void ApplyViewPreset()
    {
        switch (currentView)
        {
            case CameraView.Isometric25D:
                _activeOrthographic = isoOrthographic;
                _activePitch = isoPitch;
                _activeYaw = isoYaw;
                _activeDistance = isoDistance;
                break;
            case CameraView.ThirdPerson:
                _activeOrthographic = false;
                _activePitch = tpPitch;
                _activeYaw = tpYaw;
                _activeDistance = tpDistance;
                break;
        }

        _fixedRotation = Quaternion.Euler(_activePitch, _activeYaw, 0f);
        transform.rotation = _fixedRotation;
        ApplyProjection();
    }

    private void ApplyProjection()
    {
        if (_camera == null) return;
        _camera.orthographic = _activeOrthographic;
        _camera.nearClipPlane = nearClip;
        _camera.farClipPlane = farClip;

        if (_activeOrthographic)
        {
            _camera.orthographicSize = isoOrthoSize;
        }
        else
        {
            // เลือก FOV ตาม view — Iso perspective ใช้ FOV ต่ำ (telephoto) ให้ดูใกล้ ortho
            _camera.fieldOfView = currentView == CameraView.ThirdPerson
                ? tpFieldOfView
                : isoFieldOfView;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;

        if (_activeOrthographic)
        {
            isoOrthoSize = Mathf.Clamp(isoOrthoSize - scroll * zoomSpeed, 5f, 80f);
            _camera.orthographicSize = isoOrthoSize;
        }
        else
        {
            _activeDistance = Mathf.Clamp(_activeDistance - scroll * zoomSpeed, 3f, 30f);
        }
    }

    private void ApplyPosition()
    {
        Vector3 lookAt = target.position + Vector3.up * (currentView == CameraView.ThirdPerson ? tpLookAtOffset : isoLookAtOffset);
        Vector3 desiredPos = lookAt - _fixedRotation * Vector3.forward * _activeDistance;

        transform.position = Vector3.SmoothDamp(
            transform.position, desiredPos, ref _smoothVelocity, 1f / followSmoothness);
    }

    private void SnapToTarget()
    {
        Vector3 lookAt = target.position + Vector3.up * (currentView == CameraView.ThirdPerson ? tpLookAtOffset : isoLookAtOffset);
        transform.position = lookAt - _fixedRotation * Vector3.forward * _activeDistance;
    }

#if UNITY_EDITOR
    // re-apply ทันทีเมื่อปรับค่าใน inspector ตอน play mode
    private void OnValidate()
    {
        if (!Application.isPlaying || _camera == null) return;
        ApplyViewPreset();
    }

    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, target.position + Vector3.up * (currentView == CameraView.ThirdPerson ? tpLookAtOffset : isoLookAtOffset));
        Gizmos.DrawWireSphere(target.position, 0.4f);
    }
#endif
}
