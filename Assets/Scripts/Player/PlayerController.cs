using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Tooltip("ความเร็วเคลื่อนที่ m/s")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("แรงพุ่งขึ้นเมื่อกระโดด")]
    [SerializeField] private float jumpForce = 7f;

    [Tooltip("ระยะ Raycast ตรวจพื้น — ต้องมากกว่า collider radius เล็กน้อย")]
    [SerializeField] private float groundCheckDistance = 0.6f;

    private Rigidbody _rb;
    private bool _isGrounded;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // wasPressedThisFrame = true แค่ frame เดียว เทียบเท่า Input.GetKeyDown
        if (_isGrounded && kb.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("[Player] Jump!");
            Jump();
        }
    }

    private void FixedUpdate()
    {
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
        Move();
    }

    private void Move()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float h = 0f, v = 0f;
        if (kb.dKey.isPressed) h =  1f;
        if (kb.aKey.isPressed) h = -1f;
        if (kb.wKey.isPressed) v =  1f;
        if (kb.sKey.isPressed) v = -1f;

        if (h != 0f) Debug.Log($"[Player] H={h}");
        if (v != 0f) Debug.Log($"[Player] V={v}");

        // อิงทิศทางกับกล้อง เพื่อให้ W = วิ่งตามทิศที่กล้องมองเสมอ
        Transform cam = Camera.main ? Camera.main.transform : null;
        Vector3 forward = cam ? cam.forward : Vector3.forward;
        Vector3 right   = cam ? cam.right   : Vector3.right;
        forward.y = 0f;
        right.y   = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = (forward * v + right * h).normalized;

        // linearVelocity = Unity 6 API แทน .velocity ที่ deprecated
        _rb.linearVelocity = new Vector3(
            moveDir.x * moveSpeed,
            _rb.linearVelocity.y,
            moveDir.z * moveSpeed
        );
    }

    private void Jump()
    {
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, jumpForce, _rb.linearVelocity.z);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // เส้นสีเขียว = อยู่บนพื้น, สีแดง = กำลังลอย
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }
#endif
}
