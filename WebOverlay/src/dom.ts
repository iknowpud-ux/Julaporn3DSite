// Tiny DOM helper — แทน hyperscript/JSX. ไม่ต้องใช้ React/Preact framework
// el('div', { className: 'foo', onClick: fn }, child1, child2)

type AttrValue = string | number | boolean | EventListener | undefined | null;
type Attrs = Record<string, AttrValue> & { html?: string };

export function el<K extends keyof HTMLElementTagNameMap>(
  tag: K,
  attrs: Attrs = {},
  ...children: (Node | string | null | undefined | false)[]
): HTMLElementTagNameMap[K] {
  const node = document.createElement(tag);
  for (const [k, v] of Object.entries(attrs)) {
    if (v === undefined || v === null || v === false) continue;
    if (k === 'className') node.className = String(v);
    else if (k === 'html') node.innerHTML = String(v);
    else if (k.startsWith('on') && typeof v === 'function') {
      node.addEventListener(k.slice(2).toLowerCase(), v as EventListener);
    } else if (typeof v === 'boolean') {
      if (v) node.setAttribute(k, '');
    } else {
      node.setAttribute(k, String(v));
    }
  }
  for (const c of children) {
    if (c === null || c === undefined || c === false) continue;
    node.appendChild(typeof c === 'string' ? document.createTextNode(c) : c);
  }
  return node;
}

// helper สำหรับ inline SVG (icon strings) — wrap ใน span เพื่อ control size ผ่าน CSS
export function icon(svg: string, cls = 'icon'): HTMLSpanElement {
  return el('span', { className: cls, html: svg });
}

// ============================================================
// REVEAL DELAY COUNTER — global cursor สำหรับ stagger block-level
//   ทุก renderer ที่อยาก stagger ระหว่างกัน เรียก nextRevealBase()
//   จะได้ delay เพิ่มทีละ REVEAL_BLOCK_STEP (default 60ms)
//   ก่อนเริ่ม render ใหม่ ต้องเรียก resetReveal()
// ============================================================
const REVEAL_BLOCK_STEP = 60;
let _revealCursor = 0;

export function resetReveal(): void { _revealCursor = 0; }
export function nextRevealBase(step = REVEAL_BLOCK_STEP): number {
  const d = _revealCursor;
  _revealCursor += step;
  return d;
}

// Split text → <span> ต่อตัวอักษร พร้อม animation-delay แบบ stagger
// stepMs = ช่องว่าง delay ระหว่าง span (ค่าน้อย = wave เร็ว)
// baseMs = offset เริ่มต้น (ใช้ stagger ระหว่าง element หลายตัว)
export function splitChars(text: string, stepMs = 25, baseMs = 0): HTMLSpanElement[] {
  return [...text].map((c, i) =>
    el('span', {
      className: 'reveal-char',
      style: `--d:${baseMs + i * stepMs}ms`,
    }, c === ' ' ? ' ' : c)
  );
}

// per-word version
export function splitWords(text: string, stepMs = 70, baseMs = 0): HTMLSpanElement[] {
  const words = text.split(' ');
  return words.map((w, i) =>
    el('span', {
      className: 'reveal-word',
      style: `--d:${baseMs + i * stepMs}ms`,
    }, w + (i < words.length - 1 ? ' ' : ''))
  );
}

// reveal whole text เป็น single block (ไม่ split) — ใช้กับ text ยาวที่ไม่อยากให้กระจาย
export function revealBlock(text: string, baseMs = 0): HTMLSpanElement {
  return el('span', {
    className: 'reveal-word',
    style: `--d:${baseMs}ms`,
  }, text);
}
