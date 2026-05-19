using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Julaporn.Pins
{
    /// <summary>
    /// Spawn pins จาก LampDataAsset + handle hover/click raycast + bridge ไป dashboard
    ///
    /// SRP แบ่งกับ <see cref="PinMarker"/>:
    ///   • PinMarker = state ของตัวเอง (scale/emission ตาม hover/selected flag)
    ///   • PinManager = global concern (spawn/raycast/JS bridge/track selection)
    ///
    /// Click vs drag:
    ///   • Press → จำตำแหน่ง
    ///   • Release → ถ้า drag &lt; threshold = click → raycast
    ///   • ถ้า drag เยอะ = หมุนกล้อง (ผู้ใช้ไม่ได้กด pin) → ignore
    ///
    /// Hover:
    ///   • ทุก frame: raycast → pin ที่ hit (หรือ null ถ้าไม่ hit)
    ///   • ถ้าเปลี่ยน → SetHovered(false) ตัวเก่า + SetHovered(true) ตัวใหม่
    ///
    /// JS Bridge:
    ///   • Unity → JS: jslib DashboardSelectPin(id) → window.dashboardSelectPin(id)
    ///   • JS → Unity: SendMessage("PinManager", "FocusOnPin", id) → public method
    /// </summary>
    public sealed class PinManager : MonoBehaviour
    {
        [Header("Data")]
        [Tooltip("ScriptableObject ที่ list lamp ทั้งหมด")]
        [SerializeField] private LampDataAsset lampData;

        [Tooltip("Prefab pin (Sphere + PinMarker + SphereCollider) — scale ของ prefab จะถูก override โดย PinMarker.baseScale")]
        [SerializeField] private PinMarker pinPrefab;

        [Header("Colors")]
        [Tooltip("สี pin ปกติ (On/Off) — ตรงกับ --accent-blue ฝั่ง dashboard")]
        [SerializeField] private Color normalColor = new Color(0.31f, 0.53f, 0.97f);

        [Tooltip("สี pin ตอน NeedRepair — ตรงกับ --accent-red")]
        [SerializeField] private Color repairColor = new Color(0.94f, 0.29f, 0.29f);

        [Header("Click & Hover")]
        [Tooltip("Camera ที่ใช้ raycast (default = Camera.main)")]
        [SerializeField] private Camera raycastCamera;

        [Tooltip("Drag เกินกี่ pixel ถือว่าหมุนกล้อง ไม่ใช่ click")]
        [SerializeField, Range(2f, 30f)] private float dragThreshold = 6f;

        [Tooltip("ระยะ raycast สูงสุด")]
        [SerializeField] private float maxRaycastDistance = 8000f;

        [Header("Camera Focus")]
        [Tooltip("OrbitCameraController ที่จะสั่ง FocusOn เมื่อ JS เรียก FocusOnPin")]
        [SerializeField] private OrbitCameraController orbitCamera;

        [Tooltip("ระยะกล้องตอน focus ที่ pin (override) — 0 = คงระยะเดิม")]
        [SerializeField] private float focusDistance = 200f;

        private PinMarker[] _spawnedPins;
        private PinMarker _hoveredPin;
        private PinMarker _selectedPin;
        private Vector2 _pressPos;
        private bool _pressed;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void DashboardSelectPin(string id);
#else
        // Editor stub — test Play mode โดยไม่ต้อง build WebGL
        private static void DashboardSelectPin(string id)
        {
            Debug.Log($"[PinManager] (editor stub) DashboardSelectPin: {id}");
        }
#endif

        private void Awake()
        {
            if (raycastCamera == null) raycastCamera = Camera.main;
            if (orbitCamera == null && raycastCamera != null)
                orbitCamera = raycastCamera.GetComponent<OrbitCameraController>();
            SpawnPins();
        }

        private void SpawnPins()
        {
            if (lampData == null)
            {
                Debug.LogError("[PinManager] ❌ lampData not assigned");
                return;
            }
            if (pinPrefab == null)
            {
                Debug.LogError("[PinManager] ❌ pinPrefab not assigned");
                return;
            }

            var lamps = lampData.lamps;
            _spawnedPins = new PinMarker[lamps.Length];
            for (int i = 0; i < lamps.Length; i++)
            {
                var lamp = lamps[i];
                var pin = Instantiate(pinPrefab, lamp.worldPosition, Quaternion.identity, transform);
                pin.name = $"Pin_{lamp.id}";
                var color = lamp.status == LampDataAsset.Status.NeedRepair ? repairColor : normalColor;
                pin.Configure(lamp.id, color);
                _spawnedPins[i] = pin;
            }
            Debug.Log($"[PinManager] ✅ spawned {lamps.Length} pins");
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null || raycastCamera == null) return;

            // ทุก frame: hover detection
            UpdateHover(mouse.position.ReadValue());

            // press → จำตำแหน่ง (กัน drag ปลอมเป็น click)
            if (mouse.leftButton.wasPressedThisFrame)
            {
                _pressPos = mouse.position.ReadValue();
                _pressed = true;
            }

            // release → click หรือ drag?
            if (mouse.leftButton.wasReleasedThisFrame && _pressed)
            {
                _pressed = false;
                Vector2 releasePos = mouse.position.ReadValue();
                if (Vector2.Distance(_pressPos, releasePos) < dragThreshold)
                {
                    TryClickPin(releasePos);
                }
            }
        }

        private void UpdateHover(Vector2 screenPos)
        {
            var ray = raycastCamera.ScreenPointToRay(screenPos);
            PinMarker newHover = null;
            if (Physics.Raycast(ray, out var hit, maxRaycastDistance))
            {
                newHover = hit.collider.GetComponent<PinMarker>();
            }

            if (newHover == _hoveredPin) return;

            if (_hoveredPin != null) _hoveredPin.SetHovered(false);
            if (newHover != null) newHover.SetHovered(true);
            _hoveredPin = newHover;
        }

        private void TryClickPin(Vector2 screenPos)
        {
            var ray = raycastCamera.ScreenPointToRay(screenPos);
            if (!Physics.Raycast(ray, out var hit, maxRaycastDistance)) return;

            var pin = hit.collider.GetComponent<PinMarker>();
            if (pin == null) return;

            SetSelectedPin(pin);
            DashboardSelectPin(pin.LampId);
        }

        private void SetSelectedPin(PinMarker pin)
        {
            if (_selectedPin == pin) return;
            if (_selectedPin != null) _selectedPin.SetSelected(false);
            if (pin != null) pin.SetSelected(true);
            _selectedPin = pin;
        }

        /// <summary>
        /// เรียกจาก JS: <c>unityInstance.SendMessage("PinManager", "FocusOnPin", "LP-A012406")</c>
        /// ค้น pin ตาม id → สั่ง camera focus + mark selected
        /// </summary>
        public void FocusOnPin(string id)
        {
            if (_spawnedPins == null) return;
            for (int i = 0; i < _spawnedPins.Length; i++)
            {
                if (_spawnedPins[i] != null && _spawnedPins[i].LampId == id)
                {
                    SetSelectedPin(_spawnedPins[i]);
                    if (orbitCamera != null)
                    {
                        orbitCamera.FocusOn(_spawnedPins[i].transform.position, focusDistance);
                    }
                    return;
                }
            }
        }
    }
}
