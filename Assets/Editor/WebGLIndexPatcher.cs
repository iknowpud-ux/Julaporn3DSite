using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Julaporn.EditorTools
{
    /// <summary>
    /// Post-build hook สำหรับ WebGL — patch index.html ที่ Unity generate
    ///
    /// ทำไมต้อง patch (ไม่ใช้ WebGLTemplate):
    ///   • Template ต้อง copy ทุก asset (logo, css) ลง Assets/WebGLTemplates/ — เปลือง repo
    ///   • Patch สั้นกว่า + ง่ายต่อการเข้าใจ
    ///
    /// สิ่งที่ inject:
    ///   1. Dashboard CSS + Inter/JetBrains Mono font ใน &lt;head&gt;
    ///   2. window.unityInstance = unityInstance — ให้ dashboard.js เรียก SendMessage ได้
    ///   3. &lt;div id="dashboard-overlay"&gt; + dashboard.js module ก่อน &lt;/body&gt;
    ///
    /// Idempotent — รัน build ซ้ำได้ ไม่ inject ซ้ำ (เช็ค marker ก่อน)
    /// </summary>
    public sealed class WebGLIndexPatcher : IPostprocessBuildWithReport
    {
        public int callbackOrder => 100;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL) return;

            string outDir = report.summary.outputPath;
            string indexPath = Path.Combine(outDir, "index.html");
            if (!File.Exists(indexPath))
            {
                Debug.LogWarning($"[WebGLIndexPatcher] ⚠ index.html not found at {indexPath}");
                return;
            }

            string html = File.ReadAllText(indexPath);
            bool changed = false;

            // 1. dashboard CSS + font preconnects ใน <head>
            if (!html.Contains("dashboard/style.css"))
            {
                const string headInject =
@"    <!-- Dashboard overlay (HTML/CSS/TS) — source ที่ WebOverlay/, ห้ามแก้ไฟล์ใน dashboard/ direct -->
    <link rel=""preconnect"" href=""https://fonts.googleapis.com"">
    <link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin>
    <link rel=""stylesheet"" href=""https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=JetBrains+Mono:wght@400;500&display=swap"">
    <link rel=""stylesheet"" href=""dashboard/style.css"">
  </head>";
                html = html.Replace("</head>", headInject);
                changed = true;
            }

            // 2. expose unityInstance global + trigger fade-in dashboard หลัง Unity load
            //    + safety net: ถ้า Unity load fail หรือใช้เวลานานเกิน → force unity-ready กันค้างหน้า loading
            //    marker: ".then((unityInstance) => {" stable ใน Unity 6 template
            if (!html.Contains("window.unityInstance"))
            {
                const string fromMark = ".then((unityInstance) => {";
                const string toMark =
                    ".then((unityInstance) => {\n" +
                    "                window.unityInstance = unityInstance;\n" +
                    "                document.body.classList.add('unity-ready');";
                if (html.Contains(fromMark))
                {
                    html = html.Replace(fromMark, toMark);
                    changed = true;
                }

                // catch case: Unity ล้มเหลว → ยังให้ dashboard ขึ้น (ไม่ค้าง loading)
                const string catchFrom = "}).catch((message) => {\n                alert(message);";
                const string catchTo = "}).catch((message) => {\n                document.body.classList.add('unity-ready');\n                alert(message);";
                if (html.Contains(catchFrom))
                {
                    html = html.Replace(catchFrom, catchTo);
                }

                // watchdog: 60s ยัง load ไม่เสร็จเลย → force unity-ready กัน user ค้างหน้าจอ
                // วาง watchdog หลัง createUnityInstance call กัน fire ก่อนเริ่ม load
                const string watchdogAnchor = "var script = document.createElement(\"script\");";
                const string watchdogInject =
                    "// watchdog: ถ้า Unity load ไม่เสร็จใน 60s → unhide dashboard กันค้าง\n" +
                    "      setTimeout(() => document.body.classList.add('unity-ready'), 60000);\n" +
                    "      var script = document.createElement(\"script\");";
                if (html.Contains(watchdogAnchor) && !html.Contains("watchdog"))
                {
                    html = html.Replace(watchdogAnchor, watchdogInject);
                }
            }

            // 3. dashboard mount + script ก่อน </body>
            if (!html.Contains("id=\"dashboard-overlay\""))
            {
                const string bodyInject =
@"    <!-- Dashboard overlay mount point + bundle -->
    <div id=""dashboard-overlay""></div>
    <script type=""module"" src=""dashboard/dashboard.js""></script>
  </body>";
                html = html.Replace("</body>", bodyInject);
                changed = true;
            }

            if (changed)
            {
                File.WriteAllText(indexPath, html);
                Debug.Log($"[WebGLIndexPatcher] ✅ patched {indexPath}");
            }
            else
            {
                Debug.Log("[WebGLIndexPatcher] ℹ index.html already patched — no changes");
            }
        }
    }
}
