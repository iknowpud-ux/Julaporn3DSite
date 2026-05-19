// Inline SVG icons — สโตรกบาง 1.5px สไตล์ Phosphor/Lucide
// ทุก icon คืน string เพื่อ inject เข้า innerHTML ได้ตรงๆ
// stroke="currentColor" ทำให้สีปรับตาม CSS color ของ parent

const svg = (path: string, vb = '0 0 24 24'): string =>
  `<svg viewBox="${vb}" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${path}</svg>`;

export const ICONS = {
  apps:   svg('<rect x="3" y="3" width="6" height="6" rx="1"/><rect x="15" y="3" width="6" height="6" rx="1"/><rect x="3" y="15" width="6" height="6" rx="1"/><rect x="15" y="15" width="6" height="6" rx="1"/>'),
  logo:   svg('<path d="M4 18 L9 8 L13 14 L17 6 L20 18"/>'),
  pin:    svg('<path d="M12 22s7-7.5 7-13a7 7 0 1 0-14 0c0 5.5 7 13 7 13z"/><circle cx="12" cy="9" r="2.5"/>'),
  bell:   svg('<path d="M6 8a6 6 0 0 1 12 0c0 7 3 8 3 8H3s3-1 3-8"/><path d="M10 21h4"/>'),
  moon:   svg('<path d="M21 12.8A9 9 0 1 1 11.2 3a7 7 0 0 0 9.8 9.8z"/>'),
  globe:  svg('<circle cx="12" cy="12" r="9"/><path d="M3 12h18M12 3a14 14 0 0 1 0 18M12 3a14 14 0 0 0 0 18"/>'),
  car:    svg('<path d="M5 17h14M5 17l2-7h10l2 7M5 17v3M19 17v3M8 13h8"/><circle cx="8" cy="17" r="1.5"/><circle cx="16" cy="17" r="1.5"/>'),
  lamp:   svg('<path d="M9 3h6l3 8H6z"/><path d="M9 11v4a3 3 0 0 0 6 0v-4M12 18v3"/>'),
  bolt:   svg('<path d="M13 2 4 14h7l-1 8 9-12h-7z"/>'),
  cam:    svg('<rect x="3" y="6" width="13" height="12" rx="2"/><path d="m16 10 5-3v10l-5-3z"/>'),
  net:    svg('<circle cx="6" cy="6" r="2"/><circle cx="18" cy="6" r="2"/><circle cx="12" cy="18" r="2"/><path d="M7 8l4 8M17 8l-4 8M6 8v4M18 8v4M8 18h8"/>'),
  close:  svg('<path d="M6 6l12 12M18 6 6 18"/>'),
  plus:   svg('<path d="M12 5v14M5 12h14"/>'),
  minus:  svg('<path d="M5 12h14"/>'),
  expand: svg('<path d="M4 9V4h5M20 9V4h-5M4 15v5h5M20 15v5h-5"/>'),
  layers: svg('<path d="m12 3 9 5-9 5-9-5z"/><path d="m3 13 9 5 9-5M3 18l9 5 9-5"/>'),
  chevron:svg('<path d="m6 9 6 6 6-6"/>'),
  arrow:  svg('<path d="M5 12h14M13 5l7 7-7 7"/>'),
  signalGood: svg('<path d="M4 18h2v-2H4zm5 0h2v-5H9zm5 0h2V8h-2zm5 0h2V3h-2z" fill="currentColor" stroke="none"/>'),
  signalWeak: svg('<path d="M4 18h2v-2H4zm5 0h2v-5H9z" fill="currentColor" stroke="none"/><path d="M14 18h2V8h-2zm5 0h2V3h-2z" opacity="0.25" fill="currentColor" stroke="none"/>'),
};

export type IconName = keyof typeof ICONS;
