using UnityEngine;

/// <summary>
/// วาด gizmo sphere สีตาม Light component ที่ติดอยู่ — ช่วยให้เห็น point light ใน Scene view
///
/// Gizmos.* render เฉพาะใน Scene view ของ Editor — ไม่โผล่ใน Game view, ไม่ติดไป build
/// (compiler strip code ใน OnDrawGizmos ตอน build อัตโนมัติ)
/// </summary>
[RequireComponent(typeof(Light))]
[ExecuteAlways]
public sealed class LightGizmo : MonoBehaviour
{
    [Tooltip("ขนาด sphere gizmo ใน Scene view")]
    [SerializeField, Range(1f, 50f)] private float gizmoSize = 8f;

    [Tooltip("วาด wire sphere ของ range ด้วย — ช่วยดู scope ของ light")]
    [SerializeField] private bool showRange = true;

    private void OnDrawGizmos()
    {
        var light = GetComponent<Light>();
        if (light == null) return;

        Vector3 pos = transform.position;

        // solid sphere สีตาม light
        Color c = light.color;
        c.a = 0.85f;
        Gizmos.color = c;
        Gizmos.DrawSphere(pos, gizmoSize);

        // outline ขาวบางๆ ให้เห็นขอบชัด
        Gizmos.color = new Color(1f, 1f, 1f, 0.4f);
        Gizmos.DrawWireSphere(pos, gizmoSize * 1.05f);

        // wire sphere ของ range — โปร่งมาก (เห็น scope แต่ไม่บัง view)
        if (showRange && light.type == LightType.Point)
        {
            Color rangeColor = light.color;
            rangeColor.a = 0.08f;
            Gizmos.color = rangeColor;
            Gizmos.DrawWireSphere(pos, light.range);
        }
    }
}
