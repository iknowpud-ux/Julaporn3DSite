using UnityEngine;

/// <summary>
/// Migration shim — class นี้ถูกเก็บไว้เพราะ MainScene.unity ยังอ้าง GUID ของมัน
///
/// หน้าที่: ตรวจสอบและ AddComponent ของ SRP-split (PlayerInputReader / GroundChecker / PlayerLocomotion)
/// ตอน Awake — ถ้ายังไม่มี → add ให้แล้ว destroy ตัวเอง
///
/// เป้าหมายระยะยาว: รัน Tools → Julaporn → Refactor Player To SRP (1 ครั้ง) → save scene → ลบไฟล์นี้
///
/// เหตุผลที่ต้องมี shim: ถ้าไม่มี Awake auto-migrate → user กด Play แล้ว WASD ตาย
/// (PlayerInput.OnMove ส่ง message ไม่มีใครรับ เพราะ scene ยังไม่ถูก refactor)
/// </summary>
public class PlayerController : MonoBehaviour
{
    private void Awake()
    {
        bool migrated = false;

        if (GetComponent<PlayerInputReader>() == null)
        {
            gameObject.AddComponent<PlayerInputReader>();
            migrated = true;
        }
        if (GetComponent<GroundChecker>() == null)
        {
            gameObject.AddComponent<GroundChecker>();
            migrated = true;
        }
        if (GetComponent<PlayerLocomotion>() == null)
        {
            gameObject.AddComponent<PlayerLocomotion>();
            migrated = true;
        }

        if (migrated)
        {
            Debug.Log("[PlayerController] Auto-migrated → PlayerInputReader + GroundChecker + PlayerLocomotion. " +
                      "โปรดรัน Tools → Julaporn → Refactor Player To SRP เพื่อ persist scene.");
        }

        // ลบตัวเอง — หน้าที่จบแล้ว ไม่ต้อง update อะไรอีก
        Destroy(this);
    }
}
