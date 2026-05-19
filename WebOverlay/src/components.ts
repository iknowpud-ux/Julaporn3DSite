// UI builders — แต่ละ function สร้าง 1 section ของ dashboard
// state อยู่นอกฟังก์ชัน (main.ts) — components รับ data + callback เท่านั้น (SRP/Pure)

import { el, icon } from './dom';
import { ICONS, IconName } from './icons';
import { LampData, EXTRA_PINS } from './data';

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
        el('span', {}, 'Singapore'),
      ),
      el('button', { className: 'chip', type: 'button' },
        icon(ICONS.moon, 'icon icon--sm icon--amber'),
        el('span', {}, '75°F, 11:00 PM'),
      ),
      el('button', { className: 'icon-btn', type: 'button', 'aria-label': 'Notifications' },
        icon(ICONS.bell, 'icon icon--md icon--muted'),
      ),
      el('div', { className: 'avatar' }),
    ),
  );
}

// ---------- LEFT SIDEBAR ----------
export function sidebar(): HTMLElement {
  const item = (name: IconName, active = false) =>
    el('button', {
      className: `side-item${active ? ' side-item--active' : ''}`,
      type: 'button',
      'aria-label': name,
    }, icon(ICONS[name], 'icon icon--md'));

  return el('aside', { className: 'sidebar' },
    item('globe'),
    item('car'),
    item('lamp', true),
    item('bolt'),
    item('cam'),
    item('net'),
  );
}

// ---------- VIEWPORT TABS (top center) ----------
export function viewportTabs(): HTMLElement {
  const pill = (label: string, active = false) =>
    el('button', { className: `pill${active ? ' pill--active' : ''}`, type: 'button' }, label);
  return el('nav', { className: 'viewport-tabs' },
    pill('City'), pill('District'), pill('Street', true), pill('Building'),
  );
}

// ---------- VIEWPORT CONTROLS (right side, floating) ----------
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

// ---------- LAMP DETAIL PANEL (left) ----------
export function detailPanel(d: LampData, onClose: () => void): HTMLElement {
  const needRepair = d.status === 'NeedRepair';
  const statusLabel = needRepair ? 'Need Repair' : d.status === 'On' ? 'Active' : 'Off';

  // 24 bars: <div style="height:Y%"></div>
  const bars = d.illuminationProfile.map((v) =>
    el('div', { className: 'bar', style: `--h:${Math.max(2, v * 100)}%` }),
  );

  const kv = (key: string, value: string, opts: { mono?: boolean; color?: 'red' | 'amber' } = {}) =>
    el('div', { className: 'kv' },
      el('span', { className: 'kv__key' }, key),
      el('span', {
        className: `kv__val${opts.mono ? ' kv__val--mono' : ''}${opts.color ? ' kv__val--' + opts.color : ''}`,
      }, value),
    );

  return el('section', { className: 'detail' },
    // header
    el('header', { className: 'detail__header' },
      el('h2', { className: 'detail__id' }, d.id),
      el('span', { className: `dot dot--${needRepair ? 'red' : 'blue'}` }),
      el('span', { className: 'detail__spacer' }),
      el('button', { className: 'icon-btn icon-btn--sm', type: 'button', 'aria-label': 'Close', onClick: onClose },
        icon(ICONS.close, 'icon icon--sm')),
    ),
    el('div', { className: 'detail__addr' },
      icon(ICONS.pin, 'icon icon--xs icon--muted'),
      el('span', {}, d.address),
    ),
    el('nav', { className: 'detail__tabs' },
      el('button', { className: 'subtab subtab--active', type: 'button' }, 'Overview'),
      el('button', { className: 'subtab', type: 'button' }, 'Settings'),
      el('button', { className: 'subtab', type: 'button' }, 'Reports'),
    ),
    el('div', { className: 'detail__preview' },
      el('div', { className: 'preview-glow' }),
      icon(ICONS.lamp, 'icon icon--xl'),
    ),
    // LAMP section
    el('div', { className: 'section-head' },
      el('span', { className: 'section-bullet' }),
      'LAMP'),
    el('div', { className: 'kv-list' },
      el('div', { className: 'kv' },
        el('span', { className: 'kv__key' }, 'Status'),
        el('span', { className: 'kv__val-cluster' },
          el('span', { className: `dot dot--xs dot--${needRepair ? 'red' : 'green'}` }),
          el('span', { className: `kv__val${needRepair ? ' kv__val--red' : ''}` }, statusLabel),
        ),
      ),
      kv('Type', d.type),
      kv('Remaining Life Span', `${d.lifeSpanPercent}%`),
      kv('Light Intensity', `${d.lightIntensityLm} lm`, { mono: true }),
      kv('Consumption', `${d.consumptionKWh} kWh`, { mono: true }),
      kv('Power', `${d.powerW}W`, { mono: true }),
    ),
    // illumination
    el('div', { className: 'section-head' },
      el('span', { className: 'section-bullet' }),
      'ILLUMINATION PROFILE',
    ),
    el('div', { className: 'chart' },
      el('div', { className: 'chart__y' },
        el('span', {}, '100%'), el('span', {}, '50%'), el('span', {}, '25%'), el('span', {}, '0%'),
      ),
      el('div', { className: 'chart__bars' }, ...bars),
      el('div', { className: 'chart__x' },
        el('span', {}, '00:00'), el('span', {}, '12:00'),
        el('span', {}, '18:00'), el('span', {}, '00:00'),
      ),
    ),
    // dropdowns
    el('div', { className: 'dropdowns' },
      dropdown('Operating Mode:', 'Scheduling'),
      dropdown('Profile:', 'Park Area'),
    ),
    // CONTROLLER section
    el('div', { className: 'section-head' },
      el('span', { className: 'section-bullet' }),
      'CONTROLLER',
    ),
    el('div', { className: 'kv-list' },
      el('div', { className: 'kv' },
        el('span', { className: 'kv__key' }, 'Connection'),
        el('span', { className: 'kv__val-cluster' },
          icon(d.connection === 'Good' ? ICONS.signalGood : ICONS.signalWeak,
            `icon icon--xs ${d.connection === 'Good' ? 'icon--green' : 'icon--amber'}`),
          el('span', { className: `kv__val${d.connection === 'Good' ? '' : ' kv__val--amber'}` }, d.connection),
        ),
      ),
      kv('Uptime', d.uptime, { mono: true }),
      kv('Controller ID', d.controllerId, { mono: true }),
      kv('Model', d.controllerModel, { mono: true }),
    ),
  );
}

function dropdown(label: string, value: string): HTMLElement {
  return el('div', { className: 'dd' },
    el('label', { className: 'dd__label' }, label),
    el('button', { className: 'dd__box', type: 'button' },
      el('span', {}, value),
      icon(ICONS.chevron, 'icon icon--sm icon--muted'),
    ),
  );
}

// ---------- BOTTOM CARD ROW ----------
export function cardRow(
  lamps: LampData[],
  selectedId: string,
  onSelect: (id: string) => void,
): HTMLElement {
  return el('div', { className: 'card-row' },
    ...lamps.map((d) => lampCard(d, d.id === selectedId, () => onSelect(d.id))),
  );
}

function lampCard(d: LampData, selected: boolean, onClick: () => void): HTMLElement {
  const needRepair = d.status === 'NeedRepair';
  const lampLabel = needRepair ? 'Need Repair' : d.status;
  return el('button', {
    className: `card${selected ? ' card--selected' : ''}`,
    type: 'button',
    onClick,
  },
    el('div', { className: 'card__head' },
      el('span', { className: 'card__id' }, d.id),
      el('span', { className: `dot dot--xs dot--${needRepair ? 'red' : 'blue'}` }),
      el('span', { className: 'card__spacer' }),
      icon(ICONS.arrow, 'icon icon--sm icon--muted'),
    ),
    el('div', { className: 'card__addr' },
      icon(ICONS.pin, 'icon icon--xs icon--muted'),
      el('span', {}, d.address),
    ),
    el('div', { className: 'card__info' },
      el('div', { className: 'card__col' },
        el('span', { className: 'card__label' }, 'Lamp'),
        el('span', { className: `card__value${needRepair ? ' card__value--red' : ''}` }, lampLabel),
      ),
      el('div', { className: 'card__col' },
        el('span', { className: 'card__label' }, 'Connection'),
        el('span', { className: 'card__value card__value--row' },
          icon(d.connection === 'Good' ? ICONS.signalGood : ICONS.signalWeak,
            `icon icon--xs ${d.connection === 'Good' ? 'icon--green' : 'icon--amber'}`),
          el('span', { className: d.connection === 'Good' ? '' : 'card__value--amber' }, d.connection),
        ),
      ),
    ),
  );
}

// ---------- PIN LAYER ----------
export function pinLayer(
  lamps: LampData[],
  selectedId: string,
  onSelect: (id: string) => void,
): HTMLElement {
  return el('div', { className: 'pin-layer' },
    ...lamps.map((d) => pin(d.id, d.pinXPct, d.pinYPct,
      d.status === 'NeedRepair' ? 'red' : 'blue',
      d.id === selectedId,
      () => onSelect(d.id))),
    ...EXTRA_PINS.map((p, i) => pin(`extra-${i}`, p.x, p.y, 'blue', false, () => {})),
  );
}

function pin(
  id: string, xPct: number, yPct: number,
  color: 'red' | 'blue', selected: boolean, onClick: () => void,
): HTMLElement {
  return el('button', {
    className: `pin pin--${color}${selected ? ' pin--selected' : ''}`,
    type: 'button',
    style: `left:${xPct}%;top:${yPct}%`,
    onClick,
    'data-id': id,
  },
    el('span', { className: 'pin__halo' }),
    el('span', { className: 'pin__dot' }),
    selected && el('span', { className: 'pin__pulse' }),
  );
}
