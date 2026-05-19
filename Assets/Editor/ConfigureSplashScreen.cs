using UnityEngine;
using UnityEditor;

namespace Julaporn.EditorTools
{
    /// <summary>
    /// ปรับ Unity Splash Screen ให้เนียนกับ dashboard theme
    ///
    /// ทำไมต้องทำ:
    ///   • Unity Personal license บังคับแสดง splash ~2 วินาทีก่อน scene render
    ///   • default = ขาว flash ตัดกับ dashboard dark → user คิดว่า loading ค้าง
    ///   • set bg เดียวกับ dashboard (#060810) + static animation → เนียน
    ///
    /// ปิด splash ไม่ได้บน Personal — ต้องใช้ Pro/Enterprise license
    /// </summary>
    public static class ConfigureSplashScreen
    {
        [MenuItem("Tools/Julaporn/Configure Splash Screen")]
        public static void Execute()
        {
            // bg เดียวกับ dashboard --bg-page #060810
            PlayerSettings.SplashScreen.backgroundColor = new Color(6f / 255f, 8f / 255f, 16f / 255f);
            PlayerSettings.SplashScreen.background = null;
            PlayerSettings.SplashScreen.backgroundPortrait = null;

            // logo สี light บน dark
            PlayerSettings.SplashScreen.unityLogoStyle = PlayerSettings.SplashScreen.UnityLogoStyle.LightOnDark;

            // static (ไม่มี dolly animation) — ลด motion ที่ดึงความสนใจ
            PlayerSettings.SplashScreen.animationMode = PlayerSettings.SplashScreen.AnimationMode.Static;

            // draw mode = unity logo อย่างเดียว (ไม่มี company logo ด้านบน)
            PlayerSettings.SplashScreen.drawMode = PlayerSettings.SplashScreen.DrawMode.UnityLogoBelow;

            // show = true (Personal บังคับ) — แต่อย่างน้อยเนียน
            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.showUnityLogo = true;

            // overlay opacity ลดลง — splash ดูจาง bg dark กลืน
            PlayerSettings.SplashScreen.overlayOpacity = 0.5f;

            AssetDatabase.SaveAssets();
            Debug.Log("[ConfigureSplashScreen] ✅ Splash: bg #060810, static, LightOnDark, overlay 0.5");
        }
    }
}
