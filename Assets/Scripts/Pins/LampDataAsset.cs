using UnityEngine;

namespace Julaporn.Pins
{
    /// <summary>
    /// ScriptableObject เก็บข้อมูล lamp ทุกตัวในเมือง
    ///
    /// ทำไม ScriptableObject (ไม่ใช่ JSON/CSV):
    ///   • Inspector edit ได้ — ไม่ต้อง parse JSON runtime
    ///   • Reference จาก scene/prefab ได้ตรงๆ — ไม่ต้อง Resources.Load
    ///   • เป็น single source of truth ฝั่ง Unity (mirror ของ data.ts ฝั่ง dashboard)
    ///
    /// id ต้องตรงกับ LampData.id ใน WebOverlay/src/data.ts (ไว้ map กลับไป HTML detail panel)
    /// </summary>
    [CreateAssetMenu(fileName = "Lamps", menuName = "Julaporn/Lamp Data", order = 1)]
    public sealed class LampDataAsset : ScriptableObject
    {
        public enum Status { Off, On, NeedRepair }

        [System.Serializable]
        public sealed class Lamp
        {
            [Tooltip("ID เดียวกับ data.ts ฝั่ง dashboard — ใช้ส่งข้าม bridge")]
            public string id;

            [Tooltip("สถานะปัจจุบัน — กำหนดสี pin (blue = On/Off, red = NeedRepair)")]
            public Status status;

            [Tooltip("ตำแหน่งในโลก 3D (world space) — pin จะ spawn ที่จุดนี้")]
            public Vector3 worldPosition;
        }

        [Tooltip("รายการ lamp ทั้งหมดที่จะ spawn เป็น pin marker")]
        public Lamp[] lamps;
    }
}
