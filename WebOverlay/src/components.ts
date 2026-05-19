// UI builders — แต่ละ function สร้าง 1 section ของ dashboard
// components เป็น pure renderers — รับ data + callback เท่านั้น
// state อยู่นอกฟังก์ชัน (main.ts) ← SRP
//
// Text reveal: ทุก text ที่เป็น content (ไม่ใช่ UI chrome) wrap ด้วย splitWords/splitChars
//   counter resetReveal() + nextRevealBase() ทำงานเป็น stagger between blocks
//   ปลอดภัยตอน render parallel (cards + detail) เพราะ buildCards/buildDetail ใน main.ts รัน sequential

import { el, icon, splitChars, splitWords, resetReveal, nextRevealBase } from './dom';
import { ICONS, IconName } from './icons';
import type { AssetData, Category, CategoryMeta } from './data';

// ---------- TOP NAV ----------
export function topNav(): HTMLElement {
  const tab = (label: string, active = false) =>
    el('button', { className: `nav-tab${active ? ' nav-tab--active' : ''}`, type: 'button' }, label);

  return el('header', { className: 'topnav' },
    el('div', { className: 'topnav__left' },
      icon(ICONS.apps, 'icon icon--md icon--muted'),
      icon(ICONS.logo, 'icon icon--md icon--accent'),
      el('span', { className: 'brand' }, 'Smart City Platform'),
    ),
    el('nav', { className: 'topnav__center' },
      tab('Overview'),
      tab('Monitoring', true),
      tab('Predictive AI'),
      tab('Management'),
    ),
    el('div', { className: 'topnav__right' },
      el('button', { className: 'chip', type: 'button' },
        icon(ICONS.pin, 'icon icon--sm icon--muted'),
        el('span', {}, 'ราชวิทยาลัยจุฬาภรณ์'),
      ),
      el('button', { className: 'chip', type: 'button' },
        icon(ICONS.moon, 'icon icon--sm icon--amber'),
        el('span', {}, '28°C, 23:00'),
      ),
      el('button', { className: 'icon-btn', type: 'button', 'aria-label': 'Notifications' },
        icon(ICONS.bell, 'icon icon--md icon--muted'),
      ),
      el('div', { className: 'avatar' }),
    ),
  );
}

// ---------- LEFT SIDEBAR ----------
export function sidebar(
  categories: CategoryMeta[],
  activeKey: Category,
  onItemClick: (key: Category) => void,
): HTMLElement {
  return el('aside', { className: 'sidebar' },
    ...categories.map((c) =>
      el('button', {
        className: `side-item${c.key === activeKey ? ' side-item--active' : ''}`,
        type: 'button',
        'aria-label': c.label,
        onClick: () => onItemClick(c.key),
      }, icon(ICONS[c.icon], 'icon icon--md'))
    ),
  );
}

// ---------- VIEWPORT TABS ----------
export function viewportTabs(): HTMLElement {
  const pill = (label: string, active = false) =>
    el('button', { className: `pill${active ? ' pill--active' : ''}`, type: 'button' }, label);
  return el('nav', { className: 'viewport-tabs' },
    pill('City'), pill('District'), pill('Street', true), pill('Building'),
  );
}

// ---------- VIEWPORT CONTROLS ----------
export function viewportControls(): HTMLElement {
  const btn = (svg: string, label: string, sub = false) =>
    el('button', { className: `ctrl-btn${sub ? ' ctrl-btn--sub' : ''}`, type: 'button', 'aria-label': label },
      icon(svg, 'icon icon--md'));
  return el('div', { className: 'viewport-controls' },
    btn(ICONS.plus, 'Zoom in'),
    btn(ICONS.minus, 'Zoom out'),
    el('div', { className: 'ctrl-divider' }),
    btn(ICONS.expand, 'Fullscreen'),
    el('button', { className: 'ctrl-btn ctrl-btn--text', type: 'button' }, '2D'),
    btn(ICONS.layers, 'Layers'),
  );
}

// ---------- DETAIL PANEL ----------
export function detailPanel(d: AssetData, onClose: () => void): HTMLElement {
  // reset reveal counter — แต่ละ render ของ detail นับใหม่
  resetReveal();

  // pre-compute spans ตามลำดับ render (counter เพิ่มทีละ block)
  const idBase = nextRevealBase(0);
  const idSpans = splitChars(d.id, 18, idBase);

  const addrBase = nextRevealBase(55);
  const addrSpans = splitWords(d.address, 32, addrBase);

  return el('section', { className: 'detail' },
    el('header', { className: 'detail__header' },
      el('h2', { className: 'detail__id' }, ...idSpans),
      el('span', { className: `dot dot--${d.status.color}` }),
      el('span', { className: 'detail__spacer' }),
      el('button', { className: 'icon-btn icon-btn--sm', type: 'button', 'aria-label': 'Close', onClick: onClose },
        icon(ICONS.close, 'icon icon--sm')),
    ),
    el('div', { className: 'detail__addr' },
      icon(ICONS.pin, 'icon icon--xs icon--muted'),
      el('span', {}, ...addrSpans),
    ),
    // sub tabs — UI chrome static (ไม่ animate)
    el('nav', { className: 'detail__tabs' },
      el('button', { className: 'subtab subtab--active', type: 'button' }, 'Overview'),
      el('button', { className: 'subtab', type: 'button' }, 'Settings'),
      el('button', { className: 'subtab', type: 'button' }, 'Reports'),
    ),
    el('div', { className: 'detail__preview' },
      el('div', { className: 'preview-glow' }),
      icon(ICONS[d.previewIcon], 'icon icon--xl'),
    ),
    ...d.sections.flatMap(renderSection),
    ...(d.chart ? [renderChart(d.chart)] : []),
    ...(d.controls && d.controls.length > 0
      ? [el('div', { className: 'dropdowns' }, ...d.controls.map(renderDropdown))]
      : []),
  );
}

function renderSection(s: AssetData['sections'][number]): HTMLElement[] {
  const titleBase = nextRevealBase(80);
  return [
    el('div', { className: 'section-head' },
      el('span', { className: 'section-bullet' }),
      ...splitWords(s.title, 45, titleBase),
    ),
    el('div', { className: 'kv-list' },
      ...s.rows.map(renderKv),
    ),
  ];
}

function renderKv(row: AssetData['sections'][number]['rows'][number]): HTMLElement {
  // key + value share same base — reveal as one row, stagger ระหว่าง row ผ่าน nextRevealBase()
  const rowBase = nextRevealBase(35);
  const valClass = `kv__val${row.mono ? ' kv__val--mono' : ''}${row.tint ? ' kv__val--' + row.tint : ''}`;
  const keySpans = splitWords(row.key, 28, rowBase);
  const valSpans = splitWords(row.value, 22, rowBase + 55);

  if (row.signal) {
    return el('div', { className: 'kv' },
      el('span', { className: 'kv__key' }, ...keySpans),
      el('span', { className: 'kv__val-cluster' },
        icon(row.signal === 'good' ? ICONS.signalGood : ICONS.signalWeak,
          `icon icon--xs ${row.signal === 'good' ? 'icon--green' : 'icon--amber'}`),
        el('span', { className: valClass }, ...valSpans),
      ),
    );
  }
  return el('div', { className: 'kv' },
    el('span', { className: 'kv__key' }, ...keySpans),
    el('span', { className: valClass }, ...valSpans),
  );
}

function renderChart(chart: NonNullable<AssetData['chart']>): HTMLElement {
  const titleBase = nextRevealBase(100);
  const bars = chart.values.map((v) =>
    el('div', { className: 'bar', style: `--h:${Math.max(2, v * 100)}%` }),
  );
  return el('div', { className: 'chart-wrap' },
    el('div', { className: 'section-head' },
      el('span', { className: 'section-bullet' }),
      ...splitWords(chart.title, 45, titleBase),
    ),
    el('div', { className: 'chart' },
      el('div', { className: 'chart__y' },
        el('span', {}, '100%'), el('span', {}, '50%'), el('span', {}, '25%'), el('span', {}, '0%'),
      ),
      el('div', { className: 'chart__bars' }, ...bars),
      el('div', { className: 'chart__x' },
        ...chart.xLabels.map((l) => el('span', {}, l)),
      ),
    ),
  );
}

function renderDropdown(c: { label: string; value: string }): HTMLElement {
  const dBase = nextRevealBase(55);
  return el('div', { className: 'dd' },
    el('label', { className: 'dd__label' }, ...splitWords(c.label, 28, dBase)),
    el('button', { className: 'dd__box', type: 'button' },
      el('span', {}, ...splitWords(c.value, 28, dBase + 40)),
      icon(ICONS.chevron, 'icon icon--sm icon--muted'),
    ),
  );
}

// ---------- BOTTOM CARD ROW ----------
export function cardRow(
  items: AssetData[],
  selectedId: string,
  onSelect: (id: string) => void,
): HTMLElement {
  // counter เริ่มใหม่สำหรับ card row (ไม่ปนกับ detail)
  resetReveal();
  return el('div', { className: 'card-row' },
    ...items.map((d) => assetCard(d, d.id === selectedId, () => onSelect(d.id))),
  );
}

function assetCard(d: AssetData, selected: boolean, onClick: () => void): HTMLElement {
  // แต่ละ card stagger จาก card ก่อนหน้า + content cascade ภายใน
  const cardBase = nextRevealBase(80);
  const idSpans   = splitChars(d.id, 15, cardBase);
  const addrSpans = splitWords(d.address, 28, cardBase + 40);

  return el('button', {
    className: `card${selected ? ' card--selected' : ''}`,
    type: 'button',
    onClick,
  },
    el('div', { className: 'card__head' },
      el('span', { className: 'card__id' }, ...idSpans),
      el('span', { className: `dot dot--xs dot--${d.status.color}` }),
      el('span', { className: 'card__spacer' }),
      icon(ICONS.arrow, 'icon icon--sm icon--muted'),
    ),
    el('div', { className: 'card__addr' },
      icon(ICONS.pin, 'icon icon--xs icon--muted'),
      el('span', {}, ...addrSpans),
    ),
    el('div', { className: 'card__info' },
      renderCardField(d.cardPrimary,   cardBase + 90),
      renderCardField(d.cardSecondary, cardBase + 120),
    ),
  );
}

function renderCardField(f: AssetData['cardPrimary'], baseMs: number): HTMLElement {
  const valClass = `card__value${f.tint ? ' card__value--' + f.tint : ''}`;
  const labelSpans = splitWords(f.label, 22, baseMs);
  const valSpans   = splitWords(f.value, 22, baseMs + 28);

  if (f.signal) {
    return el('div', { className: 'card__col' },
      el('span', { className: 'card__label' }, ...labelSpans),
      el('span', { className: 'card__value card__value--row' },
        icon(f.signal === 'good' ? ICONS.signalGood : ICONS.signalWeak,
          `icon icon--xs ${f.signal === 'good' ? 'icon--green' : 'icon--amber'}`),
        el('span', { className: f.signal === 'good' ? '' : 'card__value--amber' }, ...valSpans),
      ),
    );
  }
  return el('div', { className: 'card__col' },
    el('span', { className: 'card__label' }, ...labelSpans),
    el('span', { className: valClass }, ...valSpans),
  );
}
