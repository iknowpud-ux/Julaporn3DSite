// Entry — มัน app state เดียว: selectedId
// re-render เฉพาะ detail panel + pin selection + card highlight เมื่อ select เปลี่ยน
// ไม่ใช้ framework — vanilla DOM patch แบบ targeted (เร็วพอสำหรับ mockup ที่ data ไม่ใหญ่)

import { LAMPS, LampData } from './data';
import {
  topNav, sidebar, viewportTabs, viewportControls,
  detailPanel, cardRow, pinLayer,
} from './components';

const DEFAULT_ID = 'LP-A012406'; // ตรงกับ reference image

interface Refs {
  root: HTMLElement;
  detail: HTMLElement;
  cards: HTMLElement;
  pins: HTMLElement;
}

let state = { selectedId: DEFAULT_ID };

function findLamp(id: string): LampData {
  return LAMPS.find((l) => l.id === id) ?? LAMPS[0]!;
}

function select(refs: Refs, id: string): void {
  if (state.selectedId === id) return;
  state.selectedId = id;
  // swap detail panel ทั้งก้อน (data เปลี่ยน chart bar height ก็เปลี่ยน — สร้างใหม่ง่ายกว่า patch ทีละช่อง)
  refs.detail.replaceWith(buildDetail(refs, id));
  refs.cards.replaceWith(buildCards(refs, id));
  refs.pins.replaceWith(buildPins(refs, id));
}

function buildDetail(refs: Refs, id: string): HTMLElement {
  refs.detail = detailPanel(findLamp(id), () => {
    refs.detail.classList.add('detail--closing');
    setTimeout(() => refs.detail.style.display = 'none', 180);
  });
  return refs.detail;
}

function buildCards(refs: Refs, id: string): HTMLElement {
  refs.cards = cardRow(LAMPS, id, (newId) => select(refs, newId));
  return refs.cards;
}

function buildPins(refs: Refs, id: string): HTMLElement {
  refs.pins = pinLayer(LAMPS, id, (newId) => select(refs, newId));
  return refs.pins;
}

function mount(host: HTMLElement): void {
  const refs: Refs = { root: host } as Refs;
  buildDetail(refs, state.selectedId);
  buildCards(refs, state.selectedId);
  buildPins(refs, state.selectedId);

  host.appendChild(topNav());
  host.appendChild(sidebar());
  host.appendChild(refs.pins);
  host.appendChild(refs.detail);
  host.appendChild(viewportTabs());
  host.appendChild(viewportControls());
  host.appendChild(refs.cards);
}

function init(): void {
  // หา host element ที่ index.html ประกาศไว้
  let host = document.getElementById('dashboard-overlay');
  if (!host) {
    host = document.createElement('div');
    host.id = 'dashboard-overlay';
    document.body.appendChild(host);
  }
  host.innerHTML = '';
  mount(host);
}

// auto-init: ทำงานทั้งกรณี DOMContentLoaded แล้ว และยังไม่ฟัก
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', init);
} else {
  init();
}
