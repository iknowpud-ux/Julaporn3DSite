// copy compiled bundle + CSS เข้า WebBuild/ (Unity build folder ที่ commit ขึ้น Vercel)
// ใช้ bun runtime — Bun.file/write API

import { copyFile, mkdir } from 'node:fs/promises';
import { join, dirname } from 'node:path';

const ROOT = dirname(import.meta.dir);
const SRC_DIST = join(import.meta.dir, 'dist');
const SRC_CSS  = join(import.meta.dir, 'src', 'style.css');
const DEST     = join(ROOT, 'WebBuild', 'dashboard');

await mkdir(DEST, { recursive: true });
await copyFile(join(SRC_DIST, 'main.js'), join(DEST, 'dashboard.js'));
await copyFile(SRC_CSS, join(DEST, 'style.css'));

console.log(`✅ copied → ${DEST}`);
console.log(`   dashboard.js (${(await Bun.file(join(DEST, 'dashboard.js')).size / 1024).toFixed(1)} KB)`);
console.log(`   style.css    (${(await Bun.file(join(DEST, 'style.css')).size / 1024).toFixed(1)} KB)`);
