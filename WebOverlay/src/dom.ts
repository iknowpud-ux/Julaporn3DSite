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
