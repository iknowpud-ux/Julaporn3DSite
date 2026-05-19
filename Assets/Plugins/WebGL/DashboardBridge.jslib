// Bridge: Unity → JS dashboard
// ทำไม jslib (ไม่ใช่ Application.ExternalCall):
//   • ExternalCall deprecated ใน Unity 6 / Emscripten — ใช้ jslib เป็น standard
//   • Type-safe กว่า — รับ string pointer แล้ว decode เอง
//   • Direct call ไม่ผ่าน eval() — เร็วและปลอดภัย
//
// ฝั่ง C# import:
//   [DllImport("__Internal")] private static extern void DashboardSelectPin(string id);
//
// ฝั่ง JS ต้องลงทะเบียน:
//   window.dashboardSelectPin = (id) => { /* swap detail panel */ };

mergeInto(LibraryManager.library, {
  DashboardSelectPin: function (idPtr) {
    var id = UTF8ToString(idPtr);
    if (typeof window !== 'undefined' && typeof window.dashboardSelectPin === 'function') {
      window.dashboardSelectPin(id);
    } else {
      console.warn('[Bridge] window.dashboardSelectPin not registered — pin click ignored:', id);
    }
  }
});
