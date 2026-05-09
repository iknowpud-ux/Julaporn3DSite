using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Orbit camera คล้าย Three.js OrbitControls — มุมมองผังเมือง (city overview)
///
/// Mouse:
///   • Left-drag   → orbit (yaw + pitch)
///   • Right-drag  → pan (เลื่อน focus point)
///   • Wheel       → zoom (ปรับระยะ)
/// Touch:
///   • 1-finger    → orbit
///   • 2-finger    → pinch zoom + pan (avg delta)
///
/// ออกแบบเป็น single-class เพราะทุก action โคจรรอบ state เดียวกัน (yaw/pitch/distance/focus)
/// — split ก็ได้แต่จะเปลือง overhead โดยไม่เพิ่ม clarity (SRP = state เดียว)
/// </summary>
[RequireComponent(typeof(Camera))]
public sealed class OrbitCameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("จุด focus เริ่มต้น — กล้องจะโคจรรอบจุดนี้ (world position)")]
    [SerializeField] private Vector3 focusPoint = new Vector3(-118f, 28f, -230f);

    [Header("Initial Pose")]
    [Tooltip("ระยะกล้องเริ่มต้นจาก focus")]
    [SerializeField] private float initialDistance = 2500f;

    [Tooltip("Yaw เริ่มต้น (องศา) — มุมหมุนรอบแกน Y")]
    [SerializeField] private float initialYaw = 45f;

    [Tooltip("Pitch เริ่มต้น (องศา) — 0=ระดับพื้น 90=มองดิ่งลง")]
    [SerializeField, Range(5f, 89f)] private float initialPitch = 45f;

    [Header("Limits")]
    [SerializeField] private float minDistance = 50f;
    [SerializeField] private float maxDistance = 8000f;

    [Tooltip("Pitch ต่ำสุด — กันกล้องอยู่ระดับพื้นจนทะลุ ground")]
    [SerializeField, Range(1f, 30f)] private float minPitch = 10f;

    [Tooltip("Pitch สูงสุด — กัน gimbal flip ที่ขั้ว (อย่าใส่ 90)")]
    [SerializeField, Range(60f, 89f)] private float maxPitch = 85f;

    [Header("Speed")]
    [Tooltip("ความไว orbit (องศา ต่อ pixel mouse delta)")]
    [SerializeField] private float orbitSpeed = 0.3f;

    [Tooltip("ความไว pan — scale อัตโนมัติตามระยะ (กล้องไกลก็ pan ไกลขึ้น)")]
    [SerializeField] private float panSpeed = 0.0015f;

    [Tooltip("เปอร์เซ็นต์ระยะที่ลด/เพิ่มต่อ scroll notch (0.1 = 10%)")]
    [SerializeField, Range(0.02f, 0.3f)] private float zoomStep = 0.1f;

    [Header("Damping")]
    [Tooltip("ค่าน้อย = lerp ลื่นมาก, ค่ามาก = sharp ตามทันที")]
    [SerializeField, Range(1f, 30f)] private float damping = 15f;

    // state เป้าหมาย (input เขียน, damping อ่าน)
    private float _targetYaw, _targetPitch, _targetDistance;
    private Vector3 _targetFocus;

    // state ปัจจุบัน (lerp toward target ทุกเฟรม)
    private float _curYaw, _curPitch, _curDistance;
    private Vector3 _curFocus;

    // drag state
    private bool _orbiting;
    private bool _panning;
    private Vector2 _lastMousePos;

    // pinch state — เก็บระยะ 2 นิ้วของเฟรมก่อน เพื่อคำนวณ delta
    private float _lastPinchDistance;

    private void Awake()
    {
        // sync target = current ตอนเริ่ม กัน jump
        _targetYaw = _curYaw = initialYaw;
        _targetPitch = _curPitch = Mathf.Clamp(initialPitch, minPitch, maxPitch);
        _targetDistance = _curDistance = Mathf.Clamp(initialDistance, minDistance, maxDistance);
        _targetFocus = _curFocus = focusPoint;

        ApplyTransform();
    }

    private void Update()
    {
        ReadMouse();
        ReadTouch();
        ApplyDamping();
        ApplyTransform();
    }

    private void ReadMouse()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 pos = mouse.position.ReadValue();

        if (mouse.leftButton.wasPressedThisFrame)  { _orbiting = true; _lastMousePos = pos; }
        if (mouse.leftButton.wasReleasedThisFrame) { _orbiting = false; }
        if (mouse.rightButton.wasPressedThisFrame) { _panning = true;  _lastMousePos = pos; }
        if (mouse.rightButton.wasReleasedThisFrame){ _panning = false; }

        Vector2 delta = pos - _lastMousePos;
        _lastMousePos = pos;

        if (_orbiting)
        {
            _targetYaw += delta.x * orbitSpeed;
            _targetPitch -= delta.y * orbitSpeed;
            _targetPitch = Mathf.Clamp(_targetPitch, minPitch, maxPitch);
        }
        else if (_panning)
        {
            ApplyPan(delta);
        }

        // wheel zoom — เป็น exponential ให้ความรู้สึก natural ทุกระยะ
        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float sign = Mathf.Sign(scroll);
            _targetDistance *= 1f - sign * zoomStep;
            _targetDistance = Mathf.Clamp(_targetDistance, minDistance, maxDistance);
        }
    }

    private void ReadTouch()
    {
        var ts = Touchscreen.current;
        if (ts == null) return;

        // นับ active touch + เก็บ 2 ตัวแรก
        int active = 0;
        UnityEngine.InputSystem.Controls.TouchControl t0 = null, t1 = null;
        foreach (var t in ts.touches)
        {
            if (!t.press.isPressed) continue;
            if (active == 0) t0 = t;
            else if (active == 1) t1 = t;
            active++;
        }

        if (active == 1 && t0 != null)
        {
            // 1 นิ้ว = orbit
            Vector2 d = t0.delta.ReadValue();
            _targetYaw += d.x * orbitSpeed;
            _targetPitch -= d.y * orbitSpeed;
            _targetPitch = Mathf.Clamp(_targetPitch, minPitch, maxPitch);
            _lastPinchDistance = 0f;
        }
        else if (active >= 2 && t0 != null && t1 != null)
        {
            // 2 นิ้ว = pinch zoom + pan ด้วย average delta
            Vector2 p0 = t0.position.ReadValue();
            Vector2 p1 = t1.position.ReadValue();
            float curPinch = (p0 - p1).magnitude;

            // เฟรมแรกที่ 2 นิ้วลง — ตั้ง baseline กัน jump
            if (_lastPinchDistance <= 0.01f) _lastPinchDistance = curPinch;

            float pinchDelta = curPinch - _lastPinchDistance;
            _lastPinchDistance = curPinch;

            if (Mathf.Abs(pinchDelta) > 0.5f)
            {
                _targetDistance *= 1f - pinchDelta * 0.003f;
                _targetDistance = Mathf.Clamp(_targetDistance, minDistance, maxDistance);
            }

            Vector2 avgDelta = (t0.delta.ReadValue() + t1.delta.ReadValue()) * 0.5f;
            ApplyPan(avgDelta);
        }
        else
        {
            _lastPinchDistance = 0f;
        }
    }

    private void ApplyPan(Vector2 screenDelta)
    {
        // pan scale ตามระยะ — กล้องไกล drag ได้กว้างขึ้น (รู้สึกธรรมชาติ)
        float scale = _targetDistance * panSpeed;
        Vector3 right = transform.right;
        Vector3 up = transform.up;
        _targetFocus += (-screenDelta.x * right + -screenDelta.y * up) * scale;
    }

    private void ApplyDamping()
    {
        // exponential smoothing — frame-rate independent
        float t = 1f - Mathf.Exp(-damping * Time.deltaTime);
        _curYaw = Mathf.Lerp(_curYaw, _targetYaw, t);
        _curPitch = Mathf.Lerp(_curPitch, _targetPitch, t);
        _curDistance = Mathf.Lerp(_curDistance, _targetDistance, t);
        _curFocus = Vector3.Lerp(_curFocus, _targetFocus, t);
    }

    private void ApplyTransform()
    {
        Quaternion rot = Quaternion.Euler(_curPitch, _curYaw, 0f);
        transform.rotation = rot;
        transform.position = _curFocus + rot * Vector3.back * _curDistance;
    }

    /// <summary>API สำหรับโค้ดอื่นเรียก reset/jump กล้องไปจุดใหม่</summary>
    public void FocusOn(Vector3 worldPoint, float distance = -1f)
    {
        _targetFocus = worldPoint;
        if (distance > 0f) _targetDistance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

#if UNITY_EDITOR
    /// <summary>
    /// ดูดค่า transform ปัจจุบัน → ใส่เป็น initial values
    /// ใช้: คลิกขวาที่ header ของ component ใน Inspector → "Capture Pose From Transform"
    /// คำนวณ focus = ตำแหน่งกล้อง + forward × distance (project ลงพื้น y=0 ถ้าเป็นไปได้)
    /// </summary>
    [ContextMenu("Capture Pose From Transform")]
    private void CapturePoseFromTransform()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        initialPitch = euler.x;
        initialYaw = euler.y;

        Vector3 fwd = transform.forward;
        float distance;
        Vector3 focus;

        // พยายาม project ไปบนพื้น y=0 เพื่อ focus ที่สมเหตุสมผล
        if (fwd.y < -0.01f)
        {
            // กล้องก้มลง — หาจุดที่ ray ตัดพื้น
            distance = -transform.position.y / fwd.y;
            focus = transform.position + fwd * distance;
            focus.y = 0f; // snap ให้ flat
        }
        else
        {
            // กล้องมองระดับ/ขึ้น — ใช้ระยะ default แล้วเล็งไปข้างหน้า
            distance = 500f;
            focus = transform.position + fwd * distance;
        }

        initialDistance = Mathf.Clamp(distance, minDistance, maxDistance);
        focusPoint = focus;

        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);

        Debug.Log($"[OrbitCamera] Captured: yaw={initialYaw:F2} pitch={initialPitch:F2} " +
                  $"distance={initialDistance:F1} focus={focusPoint}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(focusPoint, 50f);
        Gizmos.DrawLine(transform.position, focusPoint);
    }
#endif
}
