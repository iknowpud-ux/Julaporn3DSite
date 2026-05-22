// Entry — รวม state + dispatch สำหรับ dashboard
// state shape: { category, selectedByCategory } — แต่ละ category จำ id ของตัวเองได้
//
// Behavior (per user spec):
//   • Sidebar = open/close toggle for detail + category switcher
//     - กด item ต่าง → switch category + เปิด detail
//     - กด item เดิม → toggle detail visibility
//   • Card click = silent data change (ไม่บังคับ detail เปิด/ปิด)
//   • Pin click ใน Unity 3D = เหมือน card (silent)
//   • X close = ปิด detail (มี class system)

import { ASSETS, CATEGORIES, type AssetData, type Category } from './data';
import {
  topNav, sidebar, viewportTabs, viewportControls,
  detailPanel, cardRow,
} from './components';
import { createVoice, voiceSupported, type VoiceCommand } from './voice';
import { el, icon } from './dom';
import { ICONS } from './icons';

interface Refs {
  root: HTMLElement;
  sidebar: HTMLElement;
  detail: HTMLElement;
  cards: HTMLElement;
}

declare global {
  interface Window {
    dashboardSelectPin?: (id: string) => void;
    unityInstance?: { SendMessage: (obj: string, method: string, value?: string) => void };
  }
}

// state: category + selected id ใน category นั้นๆ
// default: lamps → LP-A012406 (ตรงกับ Pin ที่ pre-spawned ใน Unity scene)
const DEFAULT_CATEGORY: Category = 'lamps';
const DEFAULT_IDS: Record<Category, string> = {
  districts: ASSETS.districts[0]!.id,
  vehicles:  ASSETS.vehicles[0]!.id,
  lamps:     'LP-A012406',
  power:     ASSETS.power[0]!.id,
  cameras:   ASSETS.cameras[0]!.id,
  network:   ASSETS.network[0]!.id,
};

const state = {
  category: DEFAULT_CATEGORY,
  selectedByCategory: { ...DEFAULT_IDS },
};

function currentItems(): AssetData[] {
  return ASSETS[state.category];
}

function currentSelected(): AssetData {
  const id = state.selectedByCategory[state.category];
  const items = currentItems();
  return items.find((i) => i.id === id) ?? items[0]!;
}

// ---------- builders (assign to refs + return) ----------
function buildSidebar(refs: Refs): HTMLElement {
  refs.sidebar = sidebar(CATEGORIES, state.category, (key) => onSidebarClick(refs, key));
  return refs.sidebar;
}

function buildDetail(refs: Refs): HTMLElement {
  refs.detail = detailPanel(currentSelected(), () => closeDetail(refs));
  return refs.detail;
}

function buildCards(refs: Refs): HTMLElement {
  const selId = state.selectedByCategory[state.category];
  refs.cards = cardRow(currentItems(), selId, (id) => select(refs, id, true));
  return refs.cards;
}

// animation timings — match กับ CSS .fade-in/.fade-out
const FADE_OUT_MS = 180;
const FADE_IN_MS  = 280;
// cleanup สำหรับ blur-reveal — เผื่อ span ที่ delay ที่สุดเล่นจบ (~1500ms รวม animation duration 500ms)
const REVEAL_CLEANUP_MS = 1500;

// ---------- event handlers ----------
function onSidebarClick(refs: Refs, key: Category): void {
  if (key === state.category) {
    // กด item เดิม → toggle visibility (detail fade in/out)
    toggleDetail(refs);
    return;
  }
  // กด item ต่าง → swap category พร้อม fade out → swap → fade in
  swapCategory(refs, key);
}

function swapCategory(refs: Refs, key: Category): void {
  const wasOpen = document.body.classList.contains('detail-open');

  // fade out current cards (+ detail ถ้าเปิดอยู่)
  refs.cards.classList.add('fade-out');
  if (wasOpen) refs.detail.classList.add('fade-out');

  setTimeout(() => {
    // swap state + rebuild ทั้ง sidebar/cards/detail
    state.category = key;
    refs.sidebar.replaceWith(buildSidebar(refs));
    refs.cards.replaceWith(buildCards(refs));
    refs.detail.replaceWith(buildDetail(refs));

    // ensure detail เปิด (อาจปิดอยู่ก่อนหน้า)
    document.body.classList.add('detail-open');

    // panel fade-in + text blur-reveal animation (Tailwind hero style)
    refs.cards.classList.add('fade-in', 'reveal-active');
    refs.detail.classList.add('fade-in', 'reveal-active');
    setTimeout(() => {
      refs.cards.classList.remove('fade-in', 'reveal-active');
      refs.detail.classList.remove('fade-in', 'reveal-active');
    }, REVEAL_CLEANUP_MS);
  }, FADE_OUT_MS);
}

function select(refs: Refs, id: string, syncToUnity: boolean): void {
  // silent VISIBILITY — ไม่แตะ body.detail-open (ผู้ใช้ตัดสินเอง via sidebar)
  // แต่ content เปลี่ยน → replay blur-reveal animation ถ้า detail เปิดอยู่
  if (state.selectedByCategory[state.category] === id) return;
  state.selectedByCategory[state.category] = id;

  refs.detail.replaceWith(buildDetail(refs));
  refs.cards.replaceWith(buildCards(refs));

  // ถ้า detail เปิดอยู่ ก็เล่น blur-reveal กับ content ใหม่ (ผู้ใช้จะเห็นการเปลี่ยน data)
  if (document.body.classList.contains('detail-open')) {
    refs.detail.classList.add('reveal-active');
    setTimeout(() => refs.detail.classList.remove('reveal-active'), REVEAL_CLEANUP_MS);
  }

  // sync ไป Unity ก็ต่อเมื่อ select มาจาก dashboard เอง + อยู่ในหมวด lamps (pins = lamps)
  if (syncToUnity && state.category === 'lamps' && window.unityInstance) {
    window.unityInstance.SendMessage('PinManager', 'FocusOnPin', id);
  }
}

// visibility ใช้ body.detail-open class เป็น single source of truth (ทั้ง desktop + mobile)
// .fade-in/.fade-out เป็น CSS keyframe animation utility — replay ได้ทุกครั้ง class ถูก re-add
function openDetail(refs: Refs): void {
  refs.detail.classList.remove('fade-out');
  document.body.classList.add('detail-open');
  refs.detail.classList.add('fade-in', 'reveal-active');
  setTimeout(() => {
    refs.detail.classList.remove('fade-in', 'reveal-active');
  }, REVEAL_CLEANUP_MS);
}

function closeDetail(refs: Refs): void {
  refs.detail.classList.remove('fade-in');
  refs.detail.classList.add('fade-out');
  setTimeout(() => {
    document.body.classList.remove('detail-open');
    refs.detail.classList.remove('fade-out');
  }, FADE_OUT_MS);
}

function toggleDetail(refs: Refs): void {
  if (document.body.classList.contains('detail-open')) closeDetail(refs);
  else openDetail(refs);
}

// ---------- voice listening indicator ----------
// Voice modal — Google-style speech input popup
// สร้างครั้งเดียว, แสดง/ซ่อนด้วย class, อัปเดต transcript สดๆ ขณะพูด
function getVoiceModal(): { modal: HTMLElement; textEl: HTMLElement } {
  let modal = document.getElementById('voice-modal');
  if (modal) {
    return { modal, textEl: modal.querySelector('.voice-modal__text')! };
  }

  const dot    = el('span', { className: 'voice-modal__dot' });
  const label  = el('span', {}, 'กำลังฟังเสียง');
  const header = el('div',  { className: 'voice-modal__header' }, dot, label);
  const textEl = el('div',  { className: 'voice-modal__text voice-modal__text--placeholder' }, 'พูดเลย...');
  const hint   = el('div',  { className: 'voice-modal__hint' }, 'เช่น "สวัสดี"  "ไฟถนน"  "ทางเข้าหลัก"');
  const box    = el('div',  { className: 'voice-modal__box' }, header, textEl, hint);

  modal = el('div', { id: 'voice-modal', className: 'voice-modal' }, box);
  document.body.appendChild(modal);
  return { modal, textEl };
}

function showListeningIndicator(active: boolean, interimText?: string): void {
  const { modal, textEl } = getVoiceModal();
  modal.classList.toggle('voice-modal--show', active);

  if (!active) {
    textEl.textContent = 'พูดเลย...';
    textEl.classList.add('voice-modal__text--placeholder');
    return;
  }

  if (interimText) {
    textEl.textContent = interimText;
    textEl.classList.remove('voice-modal__text--placeholder');
  } else {
    textEl.textContent = 'พูดเลย...';
    textEl.classList.add('voice-modal__text--placeholder');
  }
}

// ---------- voice toast ----------
// แสดง feedback สั้นๆ ว่า mic ได้ยินอะไร (auto-hide หลัง 2 วินาที)
let _toastTimer: ReturnType<typeof setTimeout> | null = null;

function showVoiceToast(text: string, matched: boolean): void {
  let toast = document.getElementById('voice-toast');
  if (!toast) {
    toast = el('div', { id: 'voice-toast', className: 'voice-toast' });
    document.body.appendChild(toast);
  }
  // prefix แสดงสถานะ: ✓ match / ~ ไม่ match
  toast.textContent = `${matched ? '✓' : '~'} "${text}"`;
  toast.classList.add('voice-toast--show');

  if (_toastTimer) clearTimeout(_toastTimer);
  _toastTimer = setTimeout(() => {
    toast!.classList.remove('voice-toast--show');
  }, 2000);
}

// หา card ในหมวดปัจจุบันที่ address/id ตรงกับ keyword
// ใช้เป็น fallback เมื่อ voice transcript ไม่ match command ใดเลย
function findItemByKeyword(transcript: string): AssetData | null {
  const t = transcript.toLowerCase().trim();
  const items = currentItems();

  // ลอง exact substring ก่อน (ครอบ TH + EN ในคำเดียว)
  const exact = items.find(
    (item) =>
      item.address.toLowerCase().includes(t) ||
      item.id.toLowerCase().includes(t),
  );
  if (exact) return exact;

  // ลอง token matching — แยก transcript ตาม space/· แล้วหาว่า address มี token ไหนบ้าง
  // กัน keyword สั้น 1-2 ตัวอักษรที่จะ false positive (เช่น "A", "B")
  const tokens = t.split(/[\s·\-–]+/).filter((w) => w.length >= 3);
  return (
    items.find((item) =>
      tokens.some((tok) => item.address.toLowerCase().includes(tok)),
    ) ?? null
  );
}

// ---------- voice command dispatcher ----------
function handleVoiceCommand(refs: Refs, cmd: VoiceCommand, transcript: string): void {
  showVoiceToast(transcript, true);
  switch (cmd.type) {
    case 'greeting':
      alert(`🎤 Voice command ทำงานปกติ!\nได้ยิน: "${transcript}"`);
      break;
    case 'category':
      onSidebarClick(refs, cmd.value);
      break;
    case 'select_index': {
      const item = currentItems()[cmd.index];
      if (item) select(refs, item.id, true);
      break;
    }
    case 'focus_lamp': {
      if (state.category !== 'lamps') {
        // pre-set id ก่อน swap → swapCategory จะ render card+detail ถูกต้องทันที
        state.selectedByCategory['lamps'] = cmd.id;
        swapCategory(refs, 'lamps');
        // ส่ง Unity focus หลัง fade animation เสร็จ (FADE_OUT_MS = 180ms)
        setTimeout(() => {
          window.unityInstance?.SendMessage('PinManager', 'FocusOnPin', cmd.id);
        }, FADE_OUT_MS + 50);
      } else {
        select(refs, cmd.id, true);
        if (!document.body.classList.contains('detail-open')) openDetail(refs);
      }
      break;
    }
    case 'open':
      openDetail(refs);
      break;
    case 'close':
      closeDetail(refs);
      break;
  }
}

// ---------- mount ----------
function mount(host: HTMLElement): void {
  const refs: Refs = { root: host } as Refs;
  buildSidebar(refs);
  buildDetail(refs);
  buildCards(refs);

  // สร้าง topNav ก่อน เพื่อ inject mic button เข้า .topnav__right
  const nav = topNav();
  host.appendChild(nav);
  host.appendChild(refs.sidebar);
  host.appendChild(refs.detail);
  host.appendChild(viewportTabs());
  host.appendChild(viewportControls());
  host.appendChild(refs.cards);

  // bridge listener — Unity เรียก window.dashboardSelectPin(id) เมื่อคลิก pin
  // pin = lamp เสมอ (pins spawn จาก LampDataAsset ฝั่ง Unity)
  // → switch ไปหมวด lamps อัตโนมัติถ้ายังไม่อยู่, แล้ว select silently
  window.dashboardSelectPin = (id: string) => {
    if (state.category !== 'lamps') {
      state.category = 'lamps';
      refs.sidebar.replaceWith(buildSidebar(refs));
      refs.cards.replaceWith(buildCards(refs));
      refs.detail.replaceWith(buildDetail(refs));
    }
    select(refs, id, false);
  };

  // mic button — mount ต่อจาก bridge setup เพื่อ refs พร้อมแล้ว
  initVoice(refs, nav);
}

// ---------- voice init ----------
function initVoice(refs: Refs, nav: HTMLElement): void {
  // ซ่อน mic button บน Firefox/Safari ที่ไม่รองรับ Web Speech API
  if (!voiceSupported) return;

  const micBtn = el('button', {
    className: 'icon-btn mic-btn',
    type: 'button',
    'aria-label': 'Toggle voice commands',
    title: 'คำสั่งเสียง (TH/EN)',
  }, icon(ICONS.mic, 'icon icon--md'));

  // inject เข้า .topnav__right ก่อน .avatar
  const navRight = nav.querySelector('.topnav__right');
  const avatar   = navRight?.querySelector('.avatar');
  if (navRight && avatar) navRight.insertBefore(micBtn, avatar);

  const voice = createVoice({
    onCommand(cmd, transcript) {
      handleVoiceCommand(refs, cmd, transcript);
    },
    onUnrecognized(transcript) {
      const item = findItemByKeyword(transcript);
      if (item) { select(refs, item.id, true); showVoiceToast(transcript, true); return; }
      showVoiceToast(transcript, false);
    },
    onInterim(transcript) {
      showListeningIndicator(true, transcript || undefined);
    },
    onListening(active) {
      micBtn.classList.toggle('mic-btn--listening', active);
      micBtn.setAttribute('aria-pressed', String(active));
      showListeningIndicator(active);
    },
    onPermissionDenied() {
      micBtn.title = 'Microphone permission denied';
      micBtn.classList.add('mic-btn--disabled');
    },
  });

  micBtn.addEventListener('click', () => voice.toggle());
}

function init(): void {
  let host = document.getElementById('dashboard-overlay');
  if (!host) {
    host = document.createElement('div');
    host.id = 'dashboard-overlay';
    document.body.appendChild(host);
  }
  host.innerHTML = '';

  // initial detail visibility — desktop: เปิด default, mobile: ปิด default
  // (mobile screen เล็ก ไม่อยากให้ panel ทับ canvas ตอนเริ่ม)
  // ผู้ใช้กดที่ sidebar เพื่อเปิด/ปิด detail
  const isMobile = window.matchMedia('(max-width: 768px)').matches;
  if (!isMobile) document.body.classList.add('detail-open');

  mount(host);

  // standalone preview (ไม่มี Unity canvas) → fade in ทันที
  if (!document.getElementById('unity-canvas')) {
    requestAnimationFrame(() => document.body.classList.add('unity-ready'));
  }
}

if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', init);
} else {
  init();
}
