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
│   │   ├── Player/                      ← split ตาม SRP
│   │   │   ├── PlayerInputReader.cs     ← อ่าน input (WASD/Gamepad/On-Screen Stick)
│   │   │   ├── GroundChecker.cs         ← Raycast ตรวจพื้น (IsGrounded)
│   │   │   ├── PlayerLocomotion.cs      ← Rigidbody movement (read cam.Yaw runtime)
│   │   │   └── PlayerController.cs      ← Auto-migration shim (Awake → add 3 components)
│   │   ├── Camera/CameraController.cs   ← Iso/TP presets + ToggleView + mouse look
│   │   └── UI/
│   │       ├── VirtualJoystick.cs       ← Show/hide ตาม device (force show ใน Editor)
│   │       └── CameraViewSwitcher.cs    ← UI button toggle Iso ↔ TP
│   ├── Editor/                          ← Editor tools (MenuItem + static helpers)
│   │   ├── PlayerSetup.cs               ← Tools→Julaporn→Refactor Player To SRP / Setup Joystick
│   │   ├── JoystickSetup.cs             ← Canvas + Joystick + SwitchView Button + EventSystem
│   │   ├── WebGLBuilder.cs              ← build → WebBuild/ (compression Disabled)
│   │   ├── ApplyDarkCityMaterials.cs
│   │   ├── ApplyNightSky.cs
│   │   ├── ImportGLB.cs
│   │   └── PlaceJulaporn.cs
│   ├── Materials/                       ← URP Lit materials
│   │   ├── Mat_Buildings.mat
│   │   ├── Mat_Forest.mat
│   │   ├── Mat_Ground.mat
│   │   ├── Mat_MainBuilding.mat
│   │   ├── Mat_NightSky.mat
│   │   ├── Mat_Roads.mat
│   │   └── Mat_Water.mat
│   ├── julaporn.glb                     ← 3D character/model asset (~1.2 MB)
│   └── Settings/                        ← URP configs
├── Packages/              ← Coplay, URP, Input System
├── ProjectSettings/
├── WebBuild/              ← committed for Vercel deploy
├── vercel.json            ← Brotli headers config
├── .gitignore             ← modified to allow WebBuild/
├── .claude.json           ← NOT committed — has API keys
└── CLAUDE.md              ← this file
```

---

## 4. 🎯 Default Game Settings

| Setting | Value |
|---------|-------|
| **Player** | Sphere + Rigidbody (FreezeRotation ทุกแกน, Interpolate) |
| **Movement** | WASD + Virtual Joystick (mobile), speed = 12 |
| **Movement Direction** | sync จาก `CameraController.Yaw` ทุก FixedUpdate (รองรับ runtime view switch) |
| **Jump** | Space, force = 7 (ยังไม่ implement) |
| **Camera Modes** | 2 view สลับด้วยปุ่ม UI: Iso 2.5D ↔ Third Person |
| **Iso 2.5D View** | Perspective fake-iso (FOV 35°, distance 120, pitch 65°, yaw 225°, lookAt offset 25) |
| **TP View** | Perspective (FOV 50°, distance 8, pitch 20°, yaw 0°, lookAt offset 1.5) |
| **TP Mouse Look** | Right-click + drag → yaw + pitch (clamp -10° ... 70°) |
| **Camera Common** | nearClip 0.05, farClip 5000, followSmoothness 8–12 |
| **View Switch UI** | ปุ่มมุมขวาบน "🎮 2.5D" / "👤 3rd" — `CameraViewSwitcher.cs` บน Canvas |
| **EventSystem** | `InputSystemUIInputModule` (ไม่ใช่ legacy StandaloneInputModule) |
| **Cursor Lock** | ไม่ใช้ (ไม่ต้อง click ก่อน) |
| **Environment** | `julaporn.glb` mesh เมืองจริง — bounds 1330×57×712 units, ตึกสูงสุด 57 |

---

## 5. 💻 Coding Style

- **Language:** C# — clean code, SOLID principles
- **Comments:** ภาษาไทย / **Code identifiers:** English
- ใช้ `[SerializeField] private` แทน `public` fields
- ใช้ `Rigidbody.linearVelocity` (Unity 6 standard — ไม่ใช่ `.velocity`)

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
| ✅ Fixed | มุมกล้องต่ำ ไม่เห็นเป็น 2.5D ชัดเจน | pitch 50→60°, distance 12→36, projection = Orthographic (true isometric) |
| ✅ Fixed | `PlayerController` รวมหลายหน้าที่ในที่เดียว ผิด SRP | split: `PlayerInputReader` + `GroundChecker` + `PlayerLocomotion` |
| ✅ Fixed | UI button + joystick กดไม่ได้ — Scene ขาด `EventSystem` | `JoystickSetup.EnsureEventSystem()` สร้าง EventSystem + `InputSystemUIInputModule` (ไม่ใช่ legacy `StandaloneInputModule`) |
| ✅ Fixed | ตึกสูง 57 units ถูก slice ในมุม Iso orthographic — frustum vertical ±18 ครอบไม่ถึง | switch Iso เป็น **Perspective fake-iso** (FOV 35°, distance 120) — render เหมือน TP ไม่มี hard frustum cap |
| ✅ Fixed | สีตึกใน Iso ortho ต่างจาก TP — HDR/post-FX/tonemap คำนวณต่างกัน | ใช้ Perspective ทั้ง 2 view → share rendering pipeline เดียวกัน |
| ✅ Fixed | ตอนสลับเป็น TP กล้องลอยสูง — `lookAtHeightOffset` ใช้ค่าเดียวทั้ง 2 view | แยกเป็น `isoLookAtOffset` (25) + `tpLookAtOffset` (1.5) |
| ✅ Fixed | TP ไม่มี mouse look | เพิ่ม `HandleMouseLook()` — right-click + drag (ไม่ทับ joystick) |
| ℹ️ Ignore | Coplay toolbar warning | ไม่กระทบ ปล่อยไว้ได้ |

---

## 8. 🚀 Deployment Workflow

```
1. แก้ code ใน Unity Editor หรือผ่าน Claude Code
2. Build: File → Build Profiles → Build And Run
3. ทดสอบ local: cd WebBuild && bunx serve
4. git add WebBuild/ && git commit -m "..." && git push
5. Vercel auto-deploy (1–3 นาที)
6. ทดสอบ live URL
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
| Build size | < 100 MB | 53 MB ✅ |
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
- [ ] Auto-play (ไม่ต้องคลิกก่อน)
- [ ] Custom domain
- [ ] Real portfolio content (วาง julaporn.glb ใน scene จริง)
- [ ] Loading screen
- [ ] Background music

---

## 16. ✅ Pre-Deploy Checklist

- [ ] Console: 0 errors
- [ ] Test ใน Play mode
- [ ] Build WebGL สำเร็จ
- [ ] Test local: `bunx serve`
- [ ] Test บน iPhone Safari
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
