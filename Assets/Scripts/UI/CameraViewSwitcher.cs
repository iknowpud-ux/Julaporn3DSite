using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Button handler — สลับ CameraController ระหว่าง Isometric 2.5D ↔ Third-Person
/// แสดง label ตาม view ปัจจุบัน
///
/// SRP: คลาสนี้รู้แค่ "click button → ToggleView + update label"
/// ไม่รู้ logic ของกล้อง (delegate ไปที่ CameraController)
/// </summary>
[RequireComponent(typeof(Button))]
public class CameraViewSwitcher : MonoBehaviour
{
    [Tooltip("CameraController ที่จะสลับ — ถ้าว่างจะ FindFirstObjectByType อัตโนมัติ")]
    [SerializeField] private CameraController cameraController;

    [Tooltip("Text label แสดงโหมดปัจจุบัน")]
    [SerializeField] private Text label;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (cameraController == null)
            cameraController = FindFirstObjectByType<CameraController>();

        _button.onClick.AddListener(OnClick);
        UpdateLabel();
    }

    private void OnClick()
    {
        if (cameraController == null) return;
        cameraController.ToggleView();
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (label == null || cameraController == null) return;
        label.text = cameraController.View == CameraView.Isometric25D ? "🎮 2.5D" : "👤 3rd";
    }
}
