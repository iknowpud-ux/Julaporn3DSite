using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Tooltip("ความเร็วเคลื่อนที่ m/s")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("ระยะ Raycast ตรวจพื้น")]
    [SerializeField] private float groundCheckDistance = 0.6f;

    private Rigidbody _rb;
    private Vector2   _moveInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    // PlayerInput (Send Messages) — รับจาก WASD, Gamepad, และ OnScreenStick (mobile)
    private void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        if (_moveInput.sqrMagnitude < 0.01f)
        {
            _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
            return;
        }

        // อิงทิศกล้องเสมอ — W/joystick-up = วิ่งตามที่กล้องมอง
        Transform cam     = Camera.main ? Camera.main.transform : null;
        Vector3   forward = cam ? cam.forward : Vector3.forward;
        Vector3   right   = cam ? cam.right   : Vector3.right;
        forward.y = 0f;
        right.y   = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 dir = (forward * _moveInput.y + right * _moveInput.x).normalized;
        _rb.linearVelocity = new Vector3(
            dir.x * moveSpeed,
            _rb.linearVelocity.y,
            dir.z * moveSpeed
        );
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        bool grounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }
#endif
}
