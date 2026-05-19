using UnityEngine;

namespace Julaporn.Pins
{
    /// <summary>
    /// Visual ของ pin 1 ตัว — Sphere + Lit material + emissive glow (Bloom post-FX)
    ///
    /// SRP: รับผิดชอบแค่ตัวเอง (สี/emission state hover&amp;selected)
    /// การ spawn + click/hover detection อยู่ที่ <see cref="PinManager"/>
    ///
    /// State levels (idle → hover → selected) — animate "emission" อย่างเดียว
    /// (เลิก animate scale แล้ว เพราะ user ต้องการให้ pin ขนาดคงที่ + ใช้ glow แทน feedback)
    ///
    /// Glow ทำงานผ่าน Bloom post-FX (Global Volume):
    ///   • _EmissionColor &gt; 1.0 (HDR) → bloom shader pick up เป็น halo รอบ pin
    ///   • ยิ่ง emission สูง halo ยิ่งกว้าง+แรง
    ///   • Selected = base × multiplier + sin pulse → "เต้น" สม่ำเสมอ
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(SphereCollider))]
    public sealed class PinMarker : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("ID lamp นี้ — ส่งข้าม bridge เวลาถูก click")]
        [SerializeField] private string lampId;

        [Header("Size (constant — ไม่ animate)")]
        [Tooltip("World-space scale ของ pin sphere — ตัว sphere ดอท bloom จะขยายมันให้ดูใหญ่ขึ้นเอง")]
        [SerializeField] private float baseScale = 5f;

        [Header("Emission Intensity (HDR multiplier บน _BaseColor)")]
        [Tooltip("ความสว่าง emission ปกติ — มี glow ตั้งแต่เริ่มต้น")]
        [SerializeField] private float baseEmission = 4f;

        [Tooltip("ตอน mouse hover — boost bloom radius กว้างชัด")]
        [SerializeField] private float hoverEmission = 13f;

        [Tooltip("ตอนถูกเลือก — glow แรงสุด, dominate scene + pulse")]
        [SerializeField] private float selectedEmission = 17f;

        [Header("Selected Pulse")]
        [Tooltip("ความเร็ว pulse ตอน selected (rad/sec)")]
        [SerializeField] private float pulseSpeed = 3f;

        [Tooltip("ความกว้าง pulse (±% ของ selectedEmission)")]
        [SerializeField, Range(0f, 0.5f)] private float pulseAmplitude = 0.22f;

        [Header("Damping")]
        [Tooltip("ยิ่งสูงยิ่ง snap, ต่ำยิ่ง smooth")]
        [SerializeField, Range(1f, 30f)] private float damping = 14f;

        private Material _matInstance;
        private Color _baseColor = Color.white;
        private bool _hovered;
        private bool _selected;

        // damped emission state
        private float _curEmission, _targetEmission;

        public string LampId => lampId;
        public bool IsSelected => _selected;

        /// <summary>ตั้งค่า id + สี — เรียกตอน spawn จาก PinManager</summary>
        public void Configure(string id, Color color)
        {
            lampId = id;
            _baseColor = color;
            ApplyBaseColor();
        }

        public void SetHovered(bool v)
        {
            if (_hovered == v) return;
            _hovered = v;
            UpdateTarget();
        }

        public void SetSelected(bool v)
        {
            if (_selected == v) return;
            _selected = v;
            UpdateTarget();
        }

        private void UpdateTarget()
        {
            // priority: selected > hover > idle
            if (_selected) _targetEmission = selectedEmission;
            else if (_hovered) _targetEmission = hoverEmission;
            else _targetEmission = baseEmission;
        }

        private void Awake()
        {
            // เริ่มที่ base ทันที กัน animate จาก 0 ตอน spawn
            _curEmission = _targetEmission = baseEmission;

            // scale คงที่ — set ครั้งเดียวที่ Awake แล้วไม่แตะอีก
            transform.localScale = Vector3.one * baseScale;

            EnsureMaterialInstance();
        }

        private void EnsureMaterialInstance()
        {
            var rend = GetComponent<MeshRenderer>();
            // .material → instance อัตโนมัติ (ไม่กระทบ pin อื่นที่ share material asset เดิม)
            _matInstance = rend.material;
        }

        private void ApplyBaseColor()
        {
            if (_matInstance == null) EnsureMaterialInstance();
            if (_matInstance == null) return;

            if (_matInstance.HasProperty("_BaseColor")) _matInstance.SetColor("_BaseColor", _baseColor);
            if (_matInstance.HasProperty("_Color")) _matInstance.SetColor("_Color", _baseColor);
            if (_matInstance.HasProperty("_EmissionColor"))
            {
                _matInstance.EnableKeyword("_EMISSION");
                _matInstance.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                _matInstance.SetColor("_EmissionColor", _baseColor * baseEmission);
            }
        }

        private void LateUpdate()
        {
            if (_matInstance == null) return;
            if (!_matInstance.HasProperty("_EmissionColor")) return;

            // exponential smoothing → frame-rate independent
            float t = 1f - Mathf.Exp(-damping * Time.deltaTime);
            _curEmission = Mathf.Lerp(_curEmission, _targetEmission, t);

            // pulse บน emission ตอน selected — bloom จะตอบสนอง emission ขึ้นลงเป็น halo เต้น
            float emissionFinal = _curEmission;
            if (_selected)
            {
                emissionFinal *= 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
            }

            _matInstance.SetColor("_EmissionColor", _baseColor * emissionFinal);
        }
    }
}
