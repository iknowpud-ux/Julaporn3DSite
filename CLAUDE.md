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
│   ├── Scenes/MainScene.unity
│   ├── Scripts/
│   │   ├── Player/PlayerController.cs
│   │   └── Camera/ThirdPersonCamera.cs
│   └── Settings/          ← URP configs
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
| **Player** | Sphere + Rigidbody (freeze rotation X, Z) |
| **Movement** | WASD, speed = 5 |
| **Jump** | Space, force = 7 |
| **Camera** | Third-person, offset (behind 5, up 2) |
| **Mouse Look** | Mouse X/Y rotation |
| **Environment** | Plane 50×50, 10 random Cubes (height 5–15) |

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
| ⚠️ Open | Pointer Lock ทำให้ต้องคลิกก่อนเล่น | อยู่ใน Roadmap |
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
- [ ] Auto-play (ไม่ต้องคลิกก่อน)
- [ ] Custom domain
- [ ] Real portfolio content
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
