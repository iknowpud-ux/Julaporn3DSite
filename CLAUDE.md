# 🎮 Julaporn3DSite — Project CLAUDE.md

> Context สำหรับ Claude Code — อ่านทุกครั้งก่อนเริ่มงาน

---

## 1. 🌐 Project Overview

| Key | Value |
|-----|-------|
| **Name** | Julaporn3DSite |
| **Type** | 3D Interactive Portfolio Website |
| **Status** | Live (Beta) |
| **Live URL** | https://julaporn3-d-site.vercel.app |
| **Repository** | https://github.com/iknowpud-ux/Julaporn3DSite |

---

## 2. 🛠️ Tech Stack

| Layer | Detail |
|-------|--------|
| **Engine** | Unity 6.3 LTS (6000.3.14f1) + URP |
| **Build Target** | WebGL (Web Build Support module) |
| **Compression** | Disabled (deployment compatibility) |
| **Input Handling** | Both (Input System + Legacy) |
| **Hosting** | Vercel (Hobby plan) |
| **Source Control** | GitHub |
| **IDE** | Antigravity (Google) + Claude Code CLI |

---

## 3. 📁 Folder Structure

```
Julaporn3DSite/
├── Assets/
│   ├── MainScene.unity                  ← active scene (root, ไม่ใช่ Scenes/)
│   ├── Scripts/
│   │   ├── Camera/
│   │   │   ├── OrbitCameraController.cs ← Three.js-style: drag rotate + dolly + pan + touch (active)
│   │   │   └── CameraController.cs      ← Iso/TP presets — legacy, ไม่ attach กับ scene แล้ว
│   │   ├── Debug/
│   │   │   └── LightGizmo.cs            ← OnDrawGizmos sphere + range — Scene view only
│   │   ├── Player/                      ← legacy, ยังไม่ attach (ลบ Player GO ออกจาก scene แล้ว)
│   │   │   ├── PlayerInputReader.cs
│   │   │   ├── GroundChecker.cs
│   │   │   ├── PlayerLocomotion.cs
│   │   │   └── PlayerController.cs
│   │   └── UI/                          ← legacy, ลบ canvas ออกจาก scene แล้ว
│   │       ├── VirtualJoystick.cs
│   │       └── CameraViewSwitcher.cs
│   │       (Dashboard UI ย้ายไป HTML/TS overlay — ดู section 18)
│   ├── Editor/                          ← Editor tools (ดู section 17 Editor Script Conventions)
│   │   ├── SceneEditorOps.cs            ← utility: ReplaceGameObject, MarkDirtyAndSave, ฯลฯ
│   │   ├── ResetSceneLighting.cs        ← active config: 20 point lights + ambient
│   │   ├── FogTuner.cs                  ← active config: fog Linear 200..2000 #0D0F14
│   │   ├── CapturePoseRunner.cs         ← ดูด transform ของ Main Camera → initial pose
│   │   ├── WebGLBuilder.cs              ← build → WebBuild/ (compression Disabled)
│   │   ├── ApplyDarkCityMaterials.cs    ← legacy: bulk material color reset
│   │   ├── PlayerSetup.cs / JoystickSetup.cs ← legacy
│   │   ├── ImportGLB.cs / PlaceJulaporn.cs / BuildingCreator.cs / SceneChecker.cs
│   ├── Materials/                       ← URP Lit materials
│   │   ├── Mat_Buildings.mat            ← None_buildings (เมืองรอบ)
│   │   ├── Mat_MainBuilding.mat         ← building (ตึกหลัก — แยก material แล้ว)
│   │   ├── Mat_Forest.mat
│   │   ├── Mat_Ground.mat
│   │   ├── Mat_Roads.mat
│   │   └── Mat_Water.mat
│   ├── julaporn.glb                     ← city mesh (~1.2 MB, bounds 2843×57×3244 actual)
│   └── Settings/                        ← URP configs (PC_RPAsset: HDR on, MSAA 4x)
├── Packages/              ← Coplay, URP, Input System, Unity AI Assistant
├── ProjectSettings/
├── WebBuild/              ← committed for Vercel deploy (~105 MB)
│   ├── dashboard/         ← compiled HTML/TS overlay (built from WebOverlay/)
│   │   ├── dashboard.js   ← bundle ~11 KB minified
│   │   └── style.css      ← Inter font + glass-morphism dark theme
│   └── index.html         ← inject overlay <script type="module"> + <div id="dashboard-overlay">
├── WebOverlay/            ← TS source ของ Smart City Dashboard (ดู section 18)
│   ├── src/               ← data.ts, icons.ts, dom.ts, components.ts, main.ts, style.css
│   ├── tsconfig.json
│   ├── package.json
│   └── copy.ts            ← drop dist → WebBuild/dashboard/ หลัง build
├── vercel.json            ← Brotli headers config
├── .gitignore             ← modified to allow WebBuild/
├── .claude.json           ← NOT committed — has API keys
└── CLAUDE.md              ← this file
```

---

## 4. 🎯 Default Game Settings

> โหมด **Smart City Dashboard** — ผู้ใช้ดูเมืองจากมุมสูง drag rotate ได้, ไม่มี player character

| Setting | Value |
|---------|-------|
| **Mode** | Dashboard / city overview (ลบ Player GO ออกจาก scene แล้ว) |
| **Camera** | `OrbitCameraController` — drag rotate / right-drag pan / wheel dolly + touch |
| **Camera Initial Pose** | yaw 209.21°, pitch 45°, distance 462, focus (124, 0, 417) |
| **Camera Settings** | FOV 60, near 0.3, far 10000 (city extent 3244 units) |
| **Camera Reset Pose** | คลิกขวา Inspector ของ OrbitCameraController → "Capture Pose From Transform" |
| **Lighting** | 20 white point lights (intensity 60, range 1500, no shadows) cluster รอบ camera focus radius 300 |
| **Ambient** | Flat HDR `#232737 × 3` = (0.41, 0.46, 0.65) — โทน blue-grey เย็น |
| **Directional Light** | intensity 0 (ปิด — เผื่อใช้เป็น moonlight ทีหลัง) |
| **Fog** | Linear 200..2000 — color `#0D0F14` (Far city haze) |
| **Skybox** | None — solid bg color `#0D0F14` (เพื่อรองรับ day/night mode runtime swap) |
| **Reflections** | Off (Custom mode, intensity 0 — ไม่มี skybox ให้สะท้อน) |
| **Environment** | `julaporn.glb` city mesh — bounds **2843×57×3244** units (center -118, 28, -230) |
| **Materials** | Mat_Buildings (None_buildings) / **Mat_MainBuilding** (building) — แยกแล้ว |

---

## 5. 💻 Coding Style

> โปรเจกต์ใช้ **2 ภาษา** — แบ่งชัด: Unity scene/runtime = C# / Dashboard UI overlay = TypeScript

### 5.1 Unity (C#)
- **Language:** C# — clean code, SOLID principles
- **Comments:** ภาษาไทย / **Code identifiers:** English
- ใช้ `[SerializeField] private` แทน `public` fields
- ใช้ `Rigidbody.linearVelocity` (Unity 6 standard — ไม่ใช่ `.velocity`)

### 5.2 Dashboard Web Overlay (TypeScript)
- **Language:** TS strict mode (no `any`, `noUnusedLocals`, etc. — ดู `WebOverlay/tsconfig.json`)
- **Bundler:** Bun (built-in, ไม่ใช้ webpack/vite)
- **Framework:** ไม่มี (vanilla DOM + helper `el()` — ดู section 18) — เน้น bundle เล็ก + iterate เร็ว
- **CSS:** custom properties + backdrop-filter (glass-morphism), Inter + JetBrains Mono web fonts
- **Comments:** ภาษาไทย / **Code identifiers:** English (เหมือนฝั่ง C#)
- เลือก HTML/TS ทำ dashboard เพราะ TMP/uGUI render เป็น mesh ใน camera projection → ตัวอักษรเบลอ ไม่คมเท่า native browser text rendering

### ✅ Input System APIs ที่ใช้

```csharp
Keyboard.current.wKey.isPressed
Mouse.current.delta.ReadValue()
Keyboard.current.spaceKey.wasPressedThisFrame
```

### ❌ Deprecated APIs — ห้ามใช้

```csharp
Input.GetAxis()          // legacy
GetComponent() in Update // cache ไว้ใน Awake/Start แทน
```

---

## 6. 🔌 MCP & Tools Setup

| Item | Detail |
|------|--------|
| **Coplay MCP** | เชื่อม Claude Code ↔ Unity Editor |
| **Config** | `C:\Users\PUD\.claude.json` |
| **API Key** | `COPLAY_API_KEY` (in env, ห้าม commit) |
| **Coplay Plugin** | `com.coplaydev.coplay` v8.17.2 |
| **Unity Modules** | Web Build Support installed |

---

## 7. ⚠️ Known Issues & Solutions

| Status | Issue | Solution |
|--------|-------|----------|
| ✅ Fixed | Unity 6.4 = Shader Graph `BuiltInCanvasSubTarget` bug | ใช้ Unity 6.3 LTS |
| ✅ Fixed | Input Handling = New only → `InvalidOperationException` | ตั้งเป็น Both |
| ✅ Fixed | Brotli `.br` files ไม่โหลด | `vercel.json` + Content-Encoding headers |
| ✅ Fixed | Pointer Lock ทำให้ต้องคลิกก่อนเล่น | เปลี่ยน camera ไม่ใช้ cursor lock |
| ✅ Fixed | ThirdPersonCamera หมุนตามเมาส์ไม่พึงประสงค์ | เปลี่ยนเป็น CameraController (locked isometric) |
| ✅ Fixed | WASD กระตุก/เหวี่ยง — feedback loop ระหว่าง camera follow lag กับ camera-relative input | `PlayerLocomotion` ใช้ yaw-fixed (225°) แปลง input → world direction (ไม่อิง `Camera.main` runtime) |
| ✅ Fixed | กล้องสั่นเล็กน้อยตอนเดิน — `LookAt` + `SmoothDamp` แย่งกัน 2 motion source | `CameraController` set rotation ครั้งเดียวที่ Awake — เลิกใช้ `LookAt` ทุก frame |
| ✅ Fixed | HDR EXR skybox ฝัง runtime → swap day/night ลำบาก | ลบ skybox + ใช้ solid bg color + script-controllable RenderSettings |
| ✅ Fixed | Editor scripts รันซ้ำสะสม duplicate GO + dangling material refs (magenta shader) | refactor → `SceneEditorOps` utility + idempotent pattern (ดู section 17) |
| ✅ Fixed | มุมกล้องต่ำ ไม่เห็นเป็น 2.5D ชัดเจน | pitch 50→60°, distance 12→36, projection = Orthographic (true isometric) |
| ✅ Fixed | `PlayerController` รวมหลายหน้าที่ในที่เดียว ผิด SRP | split: `PlayerInputReader` + `GroundChecker` + `PlayerLocomotion` |
| ✅ Fixed | UI button + joystick กดไม่ได้ — Scene ขาด `EventSystem` | `JoystickSetup.EnsureEventSystem()` สร้าง EventSystem + `InputSystemUIInputModule` (ไม่ใช่ legacy `StandaloneInputModule`) |
| ✅ Fixed | ตึกสูง 57 units ถูก slice ในมุม Iso orthographic — frustum vertical ±18 ครอบไม่ถึง | switch Iso เป็น **Perspective fake-iso** (FOV 35°, distance 120) — render เหมือน TP ไม่มี hard frustum cap |
| ✅ Fixed | สีตึกใน Iso ortho ต่างจาก TP — HDR/post-FX/tonemap คำนวณต่างกัน | ใช้ Perspective ทั้ง 2 view → share rendering pipeline เดียวกัน |
| ✅ Fixed | ตอนสลับเป็น TP กล้องลอยสูง — `lookAtHeightOffset` ใช้ค่าเดียวทั้ง 2 view | แยกเป็น `isoLookAtOffset` (25) + `tpLookAtOffset` (1.5) |
| ✅ Fixed | TP ไม่มี mouse look | เพิ่ม `HandleMouseLook()` — right-click + drag (ไม่ทับ joystick) |
| ✅ Fixed | uGUI dashboard ตัวอักษรเบลอ — TMP render เป็น mesh + LiberationSans default ไม่คม | ย้าย dashboard ทั้งหมดไป **HTML/CSS/TS overlay** (ดู section 18) — Inter font + native browser anti-alias |
| ✅ Fixed | LiberationSans SDF ไม่มี Unicode glyph เช่น `◉ ◐ ▦ ▾` → TMP warn + แสดง □ | เลิกใช้ uGUI/TMP ตอน dashboard; ปัญหานี้ไม่ส่งผลกับ HTML overlay (ใช้ SVG icons) |
| ℹ️ Ignore | Coplay toolbar warning | ไม่กระทบ ปล่อยไว้ได้ |

---

## 8. 🚀 Deployment Workflow

### 8.1 Unity 3D scene เปลี่ยน
```
1. แก้ scene/script ใน Unity Editor หรือผ่าน Claude Code (Coplay MCP)
2. Build: File → Build Profiles → Build And Run (หรือ Tools→Julaporn→Build WebGL)
3. ทดสอบ local: cd WebBuild && bunx serve -s .
4. git add WebBuild/ && git commit -m "build: ..." && git push
5. Vercel auto-deploy (1–3 นาที)
```

### 8.2 Dashboard overlay เปลี่ยน (HTML/CSS/TS)
```
1. แก้ไฟล์ใน WebOverlay/src/ (data.ts, components.ts, style.css, ฯลฯ)
2. cd WebOverlay && bun run build  ← compile + copy → WebBuild/dashboard/
3. ทดสอบ local: cd ../WebBuild && bunx serve -s .
4. git add WebOverlay/ WebBuild/dashboard/ WebBuild/index.html && git commit
5. Push → Vercel auto-deploy
```

**Build ทั้งสองฝั่ง (Unity rebuild จะ overwrite WebBuild/ ทั้ง folder):**
```
cd WebOverlay && bun run build      # ทำหลัง Unity build เสมอ
# หรือผูก Editor script ให้ทำ post-build hook (ภายหลัง)
```

---

## 9. 🤖 Common Claude Code Commands

```
"สร้าง [object] ที่ตำแหน่ง (x,y,z) + components"
"แก้ไข [filename].cs ให้ [behavior]"
"build เป็น WebGL ที่ folder WebBuild compression=Disabled"
"ตรวจสอบ Console errors แล้วแก้"
"commit + push ขึ้น GitHub"
```

---

## 10. ⚡ Token Optimization

- ใช้ `/compact` เมื่อ context ใหญ่
- ใช้ `/clear` เริ่ม task ใหม่
- รวม tasks เป็น batch (อย่าส่งทีละ task)
- คำสั่งสั้น ตรงประเด็น (CLAUDE.md มี context อยู่แล้ว)

---

## 11. 📊 Performance Targets

| Metric | Target | Current |
|--------|--------|---------|
| Build size | < 100 MB | **105 MB ⚠️** (HDR + MSAA 4x — ต้อง optimize) |
| First load | < 30 วินาที | — |
| FPS Desktop | 60 | — |
| FPS Mobile | 30+ | — |
| Browser support | Chrome/Firefox/Safari/Edge modern | — |

---

## 12. 📱 Mobile Strategy

- **iOS Safari:** ไม่ support pointer lock
- Virtual Joystick สำหรับ touch input
- Audio ต้อง user gesture trigger ก่อน
- Texture max 2048px

---

## 13. 🔒 Security Notes

| Rule | Detail |
|------|--------|
| ❌ ห้าม commit | `.claude.json`, API keys, `.env` |
| ✅ Commit ได้ | `vercel.json`, `WebBuild/`, source code |
| 🔑 API Key | `COPLAY_API_KEY` เก็บใน `.claude.json` เท่านั้น |

---

## 14. 🌿 Git Workflow

- **Branch หลัก:** `main`
- **Commit message format:** `verb: subject`
- `WebBuild/` commit เพื่อ deploy บน Vercel
- ตรวจ `.gitignore` ก่อน push — ห้าม commit `Library/`, `Temp/`
- **ห้าม `git commit` หรือ `git push` นอกจาก user สั่งชัดเจน**

---

## 15. 🗺️ Roadmap

- [x] 3D scene พื้นฐาน + Player + Camera
- [x] Build WebGL + Deploy on Vercel
- [x] Virtual Joystick สำหรับ mobile
- [x] Locked isometric CameraController (ไม่ต้อง cursor lock)
- [x] Materials สำหรับ Buildings, Ground, Forest, Roads, Water, NightSky
- [x] julaporn.glb 3D asset + Editor tools (PlaceJulaporn, ImportGLB, ApplyDarkCityMaterials)
- [x] Refactor Player ตาม SRP — PlayerInputReader / GroundChecker / PlayerLocomotion
- [x] Fix WASD jitter — yaw-fixed input direction (ตัด feedback loop กับ camera follow)
- [x] Camera 2 view + UI toggle button (Iso 2.5D ↔ TP) + right-click mouse look ใน TP
- [x] Perspective fake-iso (FOV 35°) — สีตรงกับ TP, ตึกสูงไม่โดน slice
- [x] EventSystem + InputSystemUIInputModule (UI button + joystick ทำงาน)
- [x] WebGL builder script (Tools→Julaporn→Build WebGL)
- [x] **Smart City Dashboard mode** — ลบ Player/UI/Camera เก่า เริ่มใหม่จาก clean
- [x] **OrbitCameraController** — Three.js-style drag rotate + dolly + pan + touch
- [x] **CapturePoseFromTransform** — ContextMenu helper ดูด initial pose จาก editor
- [x] **Night atmospheric mood** — fog Linear 200..2000 + ambient HDR + 20 white point lights
- [x] **Material split** — None_buildings (Mat_Buildings) vs building (Mat_MainBuilding)
- [x] **LightGizmo** — Scene-view debug sphere (no Game/build impact)
- [x] **SceneEditorOps + Editor Script Conventions §17** — idempotent pattern
- [x] **Dashboard Web Overlay** — ย้ายจาก uGUI/TMP → HTML/CSS/TS (ดู section 18)
- [x] PinMarker UI element (data points บนเมือง) — pin markers + click → swap detail panel
- [ ] Hook Unity 3D → Dashboard (live pin position projection from world space + select event ↔ camera focus)
- [ ] Day/night mode toggle (script swap fog/ambient/light)
- [ ] Lamp Point Lights ตามแนวถนน (จาก profile_roads_*) — แทนที่ stress-test cluster
- [ ] Emissive windows + Bloom post-FX
- [ ] Optimize build size (< 100 MB) — ลด MSAA, HDR off ถ้าไม่จำเป็น
- [ ] Auto-play (ไม่ต้องคลิกก่อน)
- [ ] Custom domain
- [ ] Loading screen
- [ ] Background music

---

## 16. ✅ Pre-Deploy Checklist

- [ ] Unity Console: 0 errors / 0 warnings
- [ ] Test ใน Play mode
- [ ] Build WebGL สำเร็จ (`Tools→Julaporn→Build WebGL`)
- [ ] Dashboard rebuild: `cd WebOverlay && bun run build` (ทำหลัง Unity build ทุกครั้ง)
- [ ] Browser console: 0 errors (เปิด DevTools ตอน `bunx serve -s WebBuild`)
- [ ] Test local: `cd WebBuild && bunx serve -s .`
- [ ] Test บน iPhone Safari (touch interactions, backdrop-filter support)
- [ ] Push GitHub
- [ ] Verify Vercel deploy
- [ ] Test live URL

---

## 17. 🔧 Editor Script Conventions

> **บังคับ** สำหรับทุก editor script ใน `Assets/Editor/` — เพราะ Unity Coplay MCP รัน script หลายครั้ง ถ้าไม่ idempotent จะเกิด **bug จาก state ค้าง** (duplicate GameObjects, dangling material refs, magenta shader, etc.)

### Rule #1: Idempotent ทุกตัว
รัน script N ครั้ง → ผลลัพธ์เหมือนรัน 1 ครั้ง
- ❌ `new GameObject("Foo")` — Unity allow ชื่อซ้ำ → สะสม
- ✅ `SceneEditorOps.ReplaceGameObject("Foo")` — ลบทุกตัวที่ชื่อซ้ำ + สร้างใหม่

### Rule #2: ใช้ `SceneEditorOps` (ที่ `Assets/Editor/SceneEditorOps.cs`)
| Method | Use case |
|--------|----------|
| `EnsureNotPlayMode()` | ต้นไฟล์ — abort ถ้า play mode (กัน "cannot use during play mode" error) |
| `ReplaceGameObject(name)` | สร้าง root GO แบบ idempotent |
| `DeleteAllByName(name)` | ลบ root GO ทุกตัวชื่อนี้ |
| `EnsureFolder(path)` | สร้าง Asset folder รองรับ nested |
| `LoadOrCreateMaterial(path, shader)` | material idempotent |
| `DeleteAssetSafe(path)` | ลบ asset ปลอดภัย |
| `MarkDirtyAndSave()` | ท้ายไฟล์ — save scene |

### Rule #3: Skeleton ของทุก editor script
```csharp
public static void Execute()
{
    if (!SceneEditorOps.EnsureNotPlayMode()) return;

    // ... mutate scene/assets ...

    SceneEditorOps.MarkDirtyAndSave();
    Debug.Log("[ScriptName] ✅ Done");
}
```

### Rule #4: ลบ asset → ต้องลบ reference ใน scene ด้วย
ถ้าลบ `Mat_Foo.mat` ที่ MeshRenderer หลายตัว reference อยู่ → จะ render เป็น **magenta** (Unity fallback) → ตามเก็บลบ MeshRenderer/material ที่ใช้ asset นั้นด้วย

### Rule #5: Verify หลังรัน
ใช้ `mcp__coplay-mcp__list_game_objects_in_hierarchy` หรือ `get_game_object_info` เช็คผลลัพธ์ ก่อนบอก user ว่าเสร็จ — อย่าเชื่อแค่ "execute_script returned Success"

---

## 18. 🪟 Dashboard Web Overlay (HTML/CSS/TS)

> Smart City Platform UI ที่ลอยทับ Unity WebGL canvas — render ด้วย native browser ไม่ใช่ uGUI

### 18.1 ทำไมแยกออกจาก Unity
| ข้อจำกัด uGUI/TMP | แก้ด้วย HTML/CSS |
|---|---|
| TMP render text เป็น mesh → เบลอตอน scale, ไม่ subpixel | Browser text rendering + Inter font + `font-smoothing` |
| LiberationSans SDF default ไม่มี Unicode glyph หลายตัว | Inline SVG icons (stroke="currentColor") — ไม่ต้องใช้ font glyph |
| MSAA 4x ไม่ครอบ Canvas Overlay → ขอบ panel หยาบ | CSS rendering ผ่าน browser composite layer = คม Retina |
| Glass-morphism ต้อง custom shader | `backdrop-filter: blur(20px) saturate(140%)` |
| Iterate ต้อง rebuild Unity (slow) | `bun run build` ~30ms + hot reload ด้วย `--watch` |

### 18.2 Folder structure
```
WebOverlay/
├── src/
│   ├── data.ts          — POCO + 5 LampData + 15 extra pin positions (mock)
│   ├── icons.ts         — 15 inline SVG (Phosphor style, stroke 1.5px)
│   ├── dom.ts           — el() helper แทน JSX (~30 lines)
│   ├── components.ts    — topNav / sidebar / detailPanel / cardRow / pinLayer
│   ├── main.ts          — mount + state (selectedId), targeted re-render
│   └── style.css        — CSS vars (theme) + glass-morphism + responsive
├── tsconfig.json        — strict, ES2022, isolatedModules
├── package.json         — scripts.build = bun build + bun copy
└── copy.ts              — drop dist → WebBuild/dashboard/
```

### 18.3 Build pipeline
```
WebOverlay/src/*.ts ──[bun build]──► WebOverlay/dist/main.js (minified ES module)
                                              │
                                              ├──[copy.ts]──► WebBuild/dashboard/dashboard.js
                                              └──────────────► WebBuild/dashboard/style.css
                                                              │
                                          WebBuild/index.html ─┘ (link CSS + script type=module)
```

### 18.4 Conventions
- **No framework** — vanilla DOM + `el()` helper. ลด bundle, debugging ง่าย
- **SRP per file** — data ↔ icons ↔ dom helper ↔ components ↔ entry, ไม่ปนกัน (เหมือนแบบ Player/Camera ฝั่ง C#)
- **Strict TS** — `strict: true`, `noUnusedLocals`, `noUnusedParameters`, `noImplicitReturns`
- **Comments ภาษาไทย** อธิบาย WHY (เหมือนกฎ C#)
- **CSS variables** ใน `:root` — swap theme / accent color จากจุดเดียว
- **Inter + JetBrains Mono** จาก Google Fonts — preload ตอน index.html parse
- **pointer-events: none** ที่ overlay root → click ทะลุไป Unity canvas; เปิดเฉพาะที่ interactive (panel/card/pin)
- **z-index map:**
  - `#unity-container` = default (0)
  - `.pin-layer` = 9
  - `#dashboard-overlay > *` = 10–12 (topnav top)

### 18.5 จะต่อ live data จาก Unity ได้ยังไง (future)
- Unity → JS: `Application.ExternalCall("window.dashboardUpdate", JSON.stringify(payload))` (ผ่าน jslib)
- JS → Unity: `unityInstance.SendMessage("DashboardBridge", "OnPinSelect", id)` (รับด้วย MonoBehaviour `OnPinSelect(string)`)
- ตำแหน่ง pin จริง: คำนวณใน Unity (`Camera.WorldToScreenPoint`) + ส่ง array ของ `{id, x, y}` มาทุก ~16ms; JS update CSS `transform: translate(x, y)` ของ pin element แทน hardcoded `--x/--y` (`pinXPct/pinYPct` ใน data.ts)

### 18.6 ห้าม
- ❌ commit `WebOverlay/dist/` หรือ `node_modules/` — gitignore แล้ว
- ❌ แก้ `WebBuild/dashboard/*` direct — ต้องแก้ source ใน `WebOverlay/src/` แล้ว rebuild (จะถูก overwrite)
- ❌ ใช้ React/Vue/lib ใหญ่ — keep bundle < 20 KB
- ❌ load font จาก external server ใน production — ภายหลังควร self-host Inter/JetBrains Mono ที่ `WebBuild/dashboard/fonts/`
