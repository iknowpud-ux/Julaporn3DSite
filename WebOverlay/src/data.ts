// POCO ของ asset data — generic shape ครอบทุก category (lamps, vehicles, cameras ...)
// สถาปัตยกรรม:
//   • Category = 1 หมวดหมู่ (sidebar item) มี items[] หลายตัว
//   • AssetData = 1 รายการในหมวด — ใช้ render ทั้ง card และ detail panel
//   • รวม sections + chart + controls แบบ optional → category ที่ data น้อยก็ตัดออกได้

import type { IconName } from './icons';

export type Category = 'districts' | 'vehicles' | 'lamps' | 'power' | 'cameras' | 'network';
export type StatusColor = 'blue' | 'red' | 'amber' | 'green';
export type Tint = 'red' | 'amber' | 'green';

export interface AssetData {
  id: string;
  address: string;
  status: { label: string; color: StatusColor };
  /** primary field ที่โชว์ใน card (column ซ้าย) */
  cardPrimary: { label: string; value: string; tint?: Tint };
  /** secondary field ที่โชว์ใน card (column ขวา) */
  cardSecondary: { label: string; value: string; tint?: Tint; signal?: 'good' | 'weak' };
  /** icon ใน preview area ของ detail panel */
  previewIcon: IconName;
  /** detail key-value sections (เช่น LAMP, CONTROLLER, VEHICLE, DRIVER) */
  sections: Array<{
    title: string;
    rows: Array<{ key: string; value: string; mono?: boolean; tint?: Tint; signal?: 'good' | 'weak' }>;
  }>;
  /** 24-bar chart (optional) — values 0-1 */
  chart?: { title: string; values: number[]; xLabels: string[] };
  /** dropdown row (operating mode / profile / mode etc.) optional */
  controls?: Array<{ label: string; value: string }>;
}

export interface CategoryMeta {
  key: Category;
  /** ICON name ใน sidebar */
  icon: IconName;
  /** ชื่อแสดงใน detail tab title (เช่น "Smart Lighting") */
  label: string;
}

export const CATEGORIES: CategoryMeta[] = [
  { key: 'districts', icon: 'globe', label: 'Districts' },
  { key: 'vehicles',  icon: 'car',   label: 'Vehicles' },
  { key: 'lamps',     icon: 'lamp',  label: 'Smart Lighting' },
  { key: 'power',     icon: 'bolt',  label: 'Power Grid' },
  { key: 'cameras',   icon: 'cam',   label: 'Surveillance' },
  { key: 'network',   icon: 'net',   label: 'Network' },
];

// ---------- helpers ----------
const HOURS = ['00:00', '12:00', '18:00', '00:00'];

const flatProfile = (day: number): number[] =>
  Array.from({ length: 24 }, (_, i) => (i < 6 || i >= 18 ? 0.65 : day));

const lampRefProfile = (): number[] => [
  0.10, 0.10, 0.10, 0.10, 0.10, 0.10,
  0.70, 0.70, 0.70, 0.60, 0.60, 0.60,
  0.05, 0.05, 0.05, 0.05, 0.05, 0.05,
  0.65, 0.65, 0.65, 0.65, 0.65, 0.65,
];

// deterministic pseudo-random — กัน flicker ตอน re-render (ค่าเดิมเสมอ)
const seedProfile = (seed: number, peak: number, baseline: number): number[] =>
  Array.from({ length: 24 }, (_, i) => {
    const v = (Math.sin(seed * 13.37 + i * 2.71) + 1) / 2; // 0..1 deterministic
    return baseline + v * (peak - baseline);
  });

const ADDR = (place: string) => `${place} · ราชวิทยาลัยจุฬาภรณ์ กทม.`;

// ---------- LAMPS (5 — keep richest data) ----------
const LAMPS: AssetData[] = [
  {
    id: 'LP-A012423',
    address: ADDR('ลานจอดรถ A'),
    status: { label: 'Off', color: 'blue' },
    cardPrimary:   { label: 'Lamp',       value: 'Off' },
    cardSecondary: { label: 'Connection', value: 'Good', signal: 'good' },
    previewIcon: 'lamp',
    sections: [
      { title: 'LAMP', rows: [
        { key: 'Status',              value: 'Off' },
        { key: 'Type',                value: 'LED' },
        { key: 'Remaining Life Span', value: '73%' },
        { key: 'Light Intensity',     value: '0 lm',   mono: true },
        { key: 'Consumption',         value: '0 kWh',  mono: true },
        { key: 'Power',               value: '40W',    mono: true },
      ]},
      { title: 'CONTROLLER', rows: [
        { key: 'Connection',   value: 'Good', signal: 'good' },
        { key: 'Uptime',       value: '12d 04h 22m',  mono: true },
        { key: 'Controller ID',value: '120B962153E29',mono: true },
        { key: 'Model',        value: 'OPENSKY-442',  mono: true },
      ]},
    ],
    chart: { title: 'ILLUMINATION PROFILE', values: flatProfile(0), xLabels: HOURS },
    controls: [{ label: 'Operating Mode:', value: 'Scheduling' }, { label: 'Profile:', value: 'Park Area' }],
  },
  {
    id: 'LP-A012406',
    address: ADDR('ทางเข้าหลัก'),
    status: { label: 'Need Repair', color: 'red' },
    cardPrimary:   { label: 'Lamp',       value: 'Need Repair', tint: 'red' },
    cardSecondary: { label: 'Connection', value: 'Weak',        tint: 'amber', signal: 'weak' },
    previewIcon: 'lamp',
    sections: [
      { title: 'LAMP', rows: [
        { key: 'Status',              value: 'Need Repair', tint: 'red' },
        { key: 'Type',                value: 'LED' },
        { key: 'Remaining Life Span', value: '8%' },
        { key: 'Light Intensity',     value: '350 lm', mono: true },
        { key: 'Consumption',         value: '12 kWh', mono: true },
        { key: 'Power',               value: '40W',    mono: true },
      ]},
      { title: 'CONTROLLER', rows: [
        { key: 'Connection',   value: 'Weak', tint: 'amber', signal: 'weak' },
        { key: 'Uptime',       value: '6h 24m 15s',    mono: true },
        { key: 'Controller ID',value: '120B962153E32', mono: true },
        { key: 'Model',        value: 'OPENSKY-442',   mono: true },
      ]},
    ],
    chart: { title: 'ILLUMINATION PROFILE', values: lampRefProfile(), xLabels: HOURS },
    controls: [{ label: 'Operating Mode:', value: 'Scheduling' }, { label: 'Profile:', value: 'Park Area' }],
  },
  {
    id: 'LP-A012417', address: ADDR('อาคารวิจัย B'),
    status: { label: 'On', color: 'green' },
    cardPrimary:   { label: 'Lamp',       value: 'On' },
    cardSecondary: { label: 'Connection', value: 'Good', signal: 'good' },
    previewIcon: 'lamp',
    sections: [
      { title: 'LAMP', rows: [
        { key: 'Status', value: 'On' }, { key: 'Type', value: 'LED' },
        { key: 'Remaining Life Span', value: '88%' },
        { key: 'Light Intensity', value: '420 lm', mono: true },
        { key: 'Consumption',     value: '14 kWh', mono: true },
        { key: 'Power',           value: '40W',    mono: true },
      ]},
      { title: 'CONTROLLER', rows: [
        { key: 'Connection', value: 'Good', signal: 'good' },
        { key: 'Uptime', value: '32d 11h 03m', mono: true },
        { key: 'Controller ID', value: '120B962153E45', mono: true },
        { key: 'Model', value: 'OPENSKY-442', mono: true },
      ]},
    ],
    chart: { title: 'ILLUMINATION PROFILE', values: flatProfile(0.7), xLabels: HOURS },
    controls: [{ label: 'Operating Mode:', value: 'Auto' }, { label: 'Profile:', value: 'Building' }],
  },
  {
    id: 'LP-A012438', address: ADDR('สวนกลาง'),
    status: { label: 'Off', color: 'blue' },
    cardPrimary:   { label: 'Lamp',       value: 'Off' },
    cardSecondary: { label: 'Connection', value: 'Weak', tint: 'amber', signal: 'weak' },
    previewIcon: 'lamp',
    sections: [
      { title: 'LAMP', rows: [
        { key: 'Status', value: 'Off' }, { key: 'Type', value: 'LED' },
        { key: 'Remaining Life Span', value: '41%' },
        { key: 'Light Intensity', value: '0 lm',   mono: true },
        { key: 'Consumption',     value: '0 kWh',  mono: true },
        { key: 'Power',           value: '40W',    mono: true },
      ]},
      { title: 'CONTROLLER', rows: [
        { key: 'Connection', value: 'Weak', tint: 'amber', signal: 'weak' },
        { key: 'Uptime', value: '5d 19h 47m', mono: true },
        { key: 'Controller ID', value: '120B962153E51', mono: true },
        { key: 'Model', value: 'OPENSKY-442', mono: true },
      ]},
    ],
    chart: { title: 'ILLUMINATION PROFILE', values: flatProfile(0), xLabels: HOURS },
    controls: [{ label: 'Operating Mode:', value: 'Scheduling' }, { label: 'Profile:', value: 'Park Area' }],
  },
  {
    id: 'LP-A012414', address: ADDR('ทางเชื่อม C'),
    status: { label: 'On', color: 'green' },
    cardPrimary:   { label: 'Lamp',       value: 'On' },
    cardSecondary: { label: 'Connection', value: 'Good', signal: 'good' },
    previewIcon: 'lamp',
    sections: [
      { title: 'LAMP', rows: [
        { key: 'Status', value: 'On' }, { key: 'Type', value: 'LED' },
        { key: 'Remaining Life Span', value: '92%' },
        { key: 'Light Intensity', value: '480 lm', mono: true },
        { key: 'Consumption',     value: '15 kWh', mono: true },
        { key: 'Power',           value: '40W',    mono: true },
      ]},
      { title: 'CONTROLLER', rows: [
        { key: 'Connection', value: 'Good', signal: 'good' },
        { key: 'Uptime', value: '58d 02h 14m', mono: true },
        { key: 'Controller ID', value: '120B962153E66', mono: true },
        { key: 'Model', value: 'OPENSKY-442', mono: true },
      ]},
    ],
    chart: { title: 'ILLUMINATION PROFILE', values: flatProfile(0.7), xLabels: HOURS },
    controls: [{ label: 'Operating Mode:', value: 'Auto' }, { label: 'Profile:', value: 'Pathway' }],
  },
];

// ---------- VEHICLES (3) ----------
const VEHICLES: AssetData[] = [
  {
    id: 'VH-A2401', address: ADDR('Loop A · จุด 3'),
    status: { label: 'Moving', color: 'green' },
    cardPrimary:   { label: 'Status',  value: 'Moving' },
    cardSecondary: { label: 'Battery', value: '78%' },
    previewIcon: 'car',
    sections: [
      { title: 'VEHICLE', rows: [
        { key: 'Type', value: 'EV Shuttle' },
        { key: 'Plate',   value: 'CRA-2401', mono: true },
        { key: 'Speed',   value: '24 km/h', mono: true },
        { key: 'Battery', value: '78%',     mono: true },
        { key: 'Range',   value: '142 km',  mono: true },
      ]},
      { title: 'DRIVER', rows: [
        { key: 'Status', value: 'Active', tint: 'green' },
        { key: 'Shift',  value: '06:00 - 14:00', mono: true },
        { key: 'Trip ID',value: 'TR-58291',      mono: true },
      ]},
    ],
    chart: { title: 'SPEED PROFILE (km/h)', values: seedProfile(1, 0.85, 0.05), xLabels: HOURS },
    controls: [{ label: 'Mode:', value: 'Autonomous' }, { label: 'Route:', value: 'Loop A' }],
  },
  {
    id: 'VH-B0418', address: ADDR('ลานจอด B'),
    status: { label: 'Parked', color: 'blue' },
    cardPrimary:   { label: 'Status',  value: 'Parked' },
    cardSecondary: { label: 'Battery', value: '92%' },
    previewIcon: 'car',
    sections: [
      { title: 'VEHICLE', rows: [
        { key: 'Type', value: 'EV Sedan' },
        { key: 'Plate',   value: 'CRA-0418', mono: true },
        { key: 'Speed',   value: '0 km/h',   mono: true },
        { key: 'Battery', value: '92%',      mono: true },
        { key: 'Range',   value: '186 km',   mono: true },
      ]},
      { title: 'DRIVER', rows: [
        { key: 'Status', value: 'Standby' },
        { key: 'Shift',  value: '—', mono: true },
        { key: 'Trip ID',value: '—', mono: true },
      ]},
    ],
    chart: { title: 'SPEED PROFILE (km/h)', values: seedProfile(2, 0.6, 0.0), xLabels: HOURS },
    controls: [{ label: 'Mode:', value: 'Manual' }, { label: 'Route:', value: '—' }],
  },
  {
    id: 'VH-C7102', address: ADDR('ทางออก C'),
    status: { label: 'Maintenance', color: 'amber' },
    cardPrimary:   { label: 'Status',  value: 'Maintenance', tint: 'amber' },
    cardSecondary: { label: 'Battery', value: '34%',         tint: 'amber' },
    previewIcon: 'car',
    sections: [
      { title: 'VEHICLE', rows: [
        { key: 'Type', value: 'Service Van' },
        { key: 'Plate',   value: 'CRA-7102', mono: true },
        { key: 'Speed',   value: '0 km/h',   mono: true },
        { key: 'Battery', value: '34%',      mono: true, tint: 'amber' },
        { key: 'Range',   value: '62 km',    mono: true },
      ]},
      { title: 'SERVICE', rows: [
        { key: 'Ticket',      value: 'SVC-2210',  mono: true },
        { key: 'Last Update', value: '34m ago' },
        { key: 'Technician',  value: 'K. Pichai' },
      ]},
    ],
    chart: { title: 'SPEED PROFILE (km/h)', values: seedProfile(3, 0.3, 0.0), xLabels: HOURS },
    controls: [{ label: 'Mode:', value: 'Service' }, { label: 'Bay:', value: 'B-2' }],
  },
];

// ---------- POWER (3) ----------
const POWER: AssetData[] = [
  {
    id: 'PW-N001', address: ADDR('สถานีย่อยเหนือ'),
    status: { label: 'Online', color: 'green' },
    cardPrimary:   { label: 'Output', value: '480 kW' },
    cardSecondary: { label: 'Load',   value: '62%' },
    previewIcon: 'bolt',
    sections: [
      { title: 'POWER UNIT', rows: [
        { key: 'Output',    value: '480 kW',  mono: true },
        { key: 'Voltage',   value: '22 kV',   mono: true },
        { key: 'Frequency', value: '50.0 Hz', mono: true },
        { key: 'Phase',     value: '3-phase' },
      ]},
      { title: 'GRID', rows: [
        { key: 'Sync',         value: 'Locked', tint: 'green' },
        { key: 'Load Balance', value: '62%',    mono: true },
        { key: 'PF (cos φ)',   value: '0.94',   mono: true },
      ]},
    ],
    chart: { title: 'LOAD PROFILE (kW)', values: seedProfile(4, 0.75, 0.3), xLabels: HOURS },
    controls: [{ label: 'Mode:', value: 'Auto' }, { label: 'Limit:', value: '600 kW' }],
  },
  {
    id: 'PW-S002', address: ADDR('สถานีย่อยใต้'),
    status: { label: 'Overload', color: 'red' },
    cardPrimary:   { label: 'Output', value: '590 kW', tint: 'red' },
    cardSecondary: { label: 'Load',   value: '94%',    tint: 'red' },
    previewIcon: 'bolt',
    sections: [
      { title: 'POWER UNIT', rows: [
        { key: 'Output',    value: '590 kW',  mono: true, tint: 'red' },
        { key: 'Voltage',   value: '22 kV',   mono: true },
        { key: 'Frequency', value: '49.8 Hz', mono: true, tint: 'amber' },
        { key: 'Phase',     value: '3-phase' },
      ]},
      { title: 'GRID', rows: [
        { key: 'Sync',         value: 'Locked', tint: 'green' },
        { key: 'Load Balance', value: '94%',    mono: true, tint: 'red' },
        { key: 'PF (cos φ)',   value: '0.88',   mono: true, tint: 'amber' },
      ]},
    ],
    chart: { title: 'LOAD PROFILE (kW)', values: seedProfile(5, 0.96, 0.55), xLabels: HOURS },
    controls: [{ label: 'Mode:', value: 'Auto' }, { label: 'Limit:', value: '600 kW' }],
  },
  {
    id: 'PW-E003', address: ADDR('โซลาร์ทางตะวันออก'),
    status: { label: 'Producing', color: 'green' },
    cardPrimary:   { label: 'Output', value: '128 kW' },
    cardSecondary: { label: 'Load',   value: '—' },
    previewIcon: 'bolt',
    sections: [
      { title: 'SOLAR ARRAY', rows: [
        { key: 'Panels',   value: '420 active' },
        { key: 'Output',   value: '128 kW', mono: true },
        { key: 'Voltage',  value: '600 V',  mono: true },
        { key: 'Inverter', value: '4× SMA 32kW' },
      ]},
      { title: 'DAILY', rows: [
        { key: 'Generated',  value: '842 kWh', mono: true },
        { key: 'Peak (12pm)',value: '156 kW',  mono: true },
        { key: 'Efficiency', value: '21.4%',   mono: true, tint: 'green' },
      ]},
    ],
    chart: { title: 'SOLAR YIELD (kW)', values: seedProfile(6, 0.95, 0.0), xLabels: HOURS },
    controls: [{ label: 'Mode:', value: 'Auto' }, { label: 'Inverter:', value: 'Sync' }],
  },
];

// ---------- CAMERAS (3) ----------
const CAMERAS: AssetData[] = [
  {
    id: 'CM-N001', address: ADDR('ประตูทางเข้า · เหนือ'),
    status: { label: 'Recording', color: 'green' },
    cardPrimary:   { label: 'Status',     value: 'Recording' },
    cardSecondary: { label: 'Resolution', value: '4K @30' },
    previewIcon: 'cam',
    sections: [
      { title: 'CAMERA', rows: [
        { key: 'Model',      value: 'Axis Q1798' },
        { key: 'Resolution', value: '4K @ 30fps', mono: true },
        { key: 'Codec',      value: 'H.265',      mono: true },
        { key: 'IR',         value: 'On' },
      ]},
      { title: 'STORAGE', rows: [
        { key: 'Recording',  value: 'Continuous', tint: 'green' },
        { key: 'Free Space', value: '4.2 TB / 8 TB', mono: true },
        { key: 'Retention',  value: '30 days',    mono: true },
      ]},
    ],
    chart: { title: 'MOTION EVENTS (per hour)', values: seedProfile(7, 0.65, 0.1), xLabels: HOURS },
    controls: [{ label: 'Quality:', value: '4K' }, { label: 'Profile:', value: 'Outdoor' }],
  },
  {
    id: 'CM-W002', address: ADDR('โถงทางเดิน · ตะวันตก'),
    status: { label: 'Alert', color: 'amber' },
    cardPrimary:   { label: 'Status',     value: 'Motion Alert', tint: 'amber' },
    cardSecondary: { label: 'Resolution', value: '1080p @60' },
    previewIcon: 'cam',
    sections: [
      { title: 'CAMERA', rows: [
        { key: 'Model',      value: 'Hikvision DS-2CD' },
        { key: 'Resolution', value: '1080p @ 60fps', mono: true },
        { key: 'Codec',      value: 'H.264',         mono: true },
        { key: 'IR',         value: 'Auto' },
      ]},
      { title: 'ALERT', rows: [
        { key: 'Type',       value: 'Motion detected', tint: 'amber' },
        { key: 'Triggered',  value: '2m ago' },
        { key: 'Confidence', value: '87%',  mono: true },
      ]},
    ],
    chart: { title: 'MOTION EVENTS (per hour)', values: seedProfile(8, 0.9, 0.15), xLabels: HOURS },
    controls: [{ label: 'Quality:', value: '1080p' }, { label: 'Profile:', value: 'Indoor' }],
  },
  {
    id: 'CM-E003', address: ADDR('ลานจอด · ตะวันออก'),
    status: { label: 'Offline', color: 'red' },
    cardPrimary:   { label: 'Status',     value: 'Offline', tint: 'red' },
    cardSecondary: { label: 'Resolution', value: '—',      tint: 'amber' },
    previewIcon: 'cam',
    sections: [
      { title: 'CAMERA', rows: [
        { key: 'Model',      value: 'Hikvision DS-2CD' },
        { key: 'Resolution', value: '—',      mono: true },
        { key: 'Codec',      value: 'H.264',  mono: true },
        { key: 'IR',         value: 'Unknown' },
      ]},
      { title: 'INCIDENT', rows: [
        { key: 'Last Seen',  value: '14m ago' },
        { key: 'Error',      value: 'No signal', tint: 'red' },
        { key: 'Ticket',     value: 'INC-7821',  mono: true },
      ]},
    ],
    chart: { title: 'MOTION EVENTS (per hour)', values: seedProfile(9, 0.4, 0.0), xLabels: HOURS },
    controls: [{ label: 'Quality:', value: '—' }, { label: 'Profile:', value: '—' }],
  },
];

// ---------- NETWORK (3) ----------
const NETWORK: AssetData[] = [
  {
    id: 'NT-CORE01', address: ADDR('ห้อง MDF'),
    status: { label: 'Online', color: 'green' },
    cardPrimary:   { label: 'Latency',    value: '12 ms' },
    cardSecondary: { label: 'Throughput', value: '480 Mbps' },
    previewIcon: 'net',
    sections: [
      { title: 'NODE', rows: [
        { key: 'Type',   value: 'Core Switch' },
        { key: 'IP',     value: '10.0.1.1',         mono: true },
        { key: 'MAC',    value: 'A4:5E:60:42:11:01',mono: true },
        { key: 'Uptime', value: '124d 08h',         mono: true },
      ]},
      { title: 'PERFORMANCE', rows: [
        { key: 'Latency',    value: '12 ms',     mono: true, tint: 'green' },
        { key: 'Throughput', value: '480 Mbps',  mono: true },
        { key: 'Packet Loss',value: '0.02%',     mono: true, tint: 'green' },
      ]},
    ],
    chart: { title: 'THROUGHPUT (Mbps)', values: seedProfile(10, 0.7, 0.2), xLabels: HOURS },
    controls: [{ label: 'QoS:', value: 'High' }, { label: 'Mode:', value: 'Auto' }],
  },
  {
    id: 'NT-AP-B12', address: ADDR('Building B · ชั้น 12'),
    status: { label: 'Degraded', color: 'amber' },
    cardPrimary:   { label: 'Latency',    value: '84 ms',   tint: 'amber' },
    cardSecondary: { label: 'Throughput', value: '42 Mbps', tint: 'amber' },
    previewIcon: 'net',
    sections: [
      { title: 'NODE', rows: [
        { key: 'Type',   value: 'WiFi 6 AP' },
        { key: 'IP',     value: '10.0.12.18',       mono: true },
        { key: 'MAC',    value: 'A4:5E:60:42:11:0C',mono: true },
        { key: 'Uptime', value: '12d 04h',          mono: true },
      ]},
      { title: 'PERFORMANCE', rows: [
        { key: 'Latency',    value: '84 ms',  mono: true, tint: 'amber' },
        { key: 'Throughput', value: '42 Mbps',mono: true, tint: 'amber' },
        { key: 'Packet Loss',value: '3.2%',   mono: true, tint: 'amber' },
        { key: 'Clients',    value: '47',     mono: true },
      ]},
    ],
    chart: { title: 'THROUGHPUT (Mbps)', values: seedProfile(11, 0.45, 0.1), xLabels: HOURS },
    controls: [{ label: 'QoS:', value: 'Normal' }, { label: 'Mode:', value: 'Auto' }],
  },
  {
    id: 'NT-FW-DMZ', address: ADDR('DMZ Firewall'),
    status: { label: 'Online', color: 'green' },
    cardPrimary:   { label: 'Latency',    value: '4 ms' },
    cardSecondary: { label: 'Throughput', value: '1.2 Gbps' },
    previewIcon: 'net',
    sections: [
      { title: 'NODE', rows: [
        { key: 'Type',   value: 'Firewall' },
        { key: 'IP',     value: '10.0.0.254',       mono: true },
        { key: 'MAC',    value: 'A4:5E:60:42:11:FE',mono: true },
        { key: 'Uptime', value: '212d 02h',         mono: true },
      ]},
      { title: 'SECURITY', rows: [
        { key: 'Threats Blocked', value: '142 today', mono: true },
        { key: 'Rules',           value: '482 active',mono: true },
        { key: 'Sessions',        value: '12,840',    mono: true },
      ]},
    ],
    chart: { title: 'THROUGHPUT (Gbps)', values: seedProfile(12, 0.85, 0.4), xLabels: HOURS },
    controls: [{ label: 'Mode:', value: 'Active' }, { label: 'Policy:', value: 'Strict' }],
  },
];

// ---------- DISTRICTS (3) ----------
const DISTRICTS: AssetData[] = [
  {
    id: 'DS-CAMPUS', address: 'พื้นที่หลัก · ราชวิทยาลัยจุฬาภรณ์',
    status: { label: 'Active', color: 'green' },
    cardPrimary:   { label: 'Population', value: '12,400' },
    cardSecondary: { label: 'Alerts',     value: '3', tint: 'amber' },
    previewIcon: 'globe',
    sections: [
      { title: 'DISTRICT', rows: [
        { key: 'Area',       value: '142 acres' },
        { key: 'Population', value: '12,400 daily', mono: true },
        { key: 'Density',    value: '87 / acre',    mono: true },
        { key: 'Zone',       value: 'Educational' },
      ]},
      { title: 'ACTIVITY', rows: [
        { key: 'Active Sensors', value: '1,247',   mono: true, tint: 'green' },
        { key: 'Active Alerts',  value: '3',       mono: true, tint: 'amber' },
        { key: 'Last Sync',      value: '12s ago', mono: true },
      ]},
    ],
    chart: { title: 'TRAFFIC FLOW (people/h)', values: seedProfile(13, 0.85, 0.05), xLabels: HOURS },
    controls: [{ label: 'View:', value: 'Heatmap' }, { label: 'Layer:', value: 'All' }],
  },
  {
    id: 'DS-PARK', address: 'สวนเหนือ · ราชวิทยาลัยจุฬาภรณ์',
    status: { label: 'Active', color: 'green' },
    cardPrimary:   { label: 'Population', value: '320' },
    cardSecondary: { label: 'Alerts',     value: '0' },
    previewIcon: 'globe',
    sections: [
      { title: 'DISTRICT', rows: [
        { key: 'Area',       value: '8 acres' },
        { key: 'Population', value: '320 daily', mono: true },
        { key: 'Zone',       value: 'Recreational' },
      ]},
      { title: 'ACTIVITY', rows: [
        { key: 'Active Sensors', value: '64',     mono: true, tint: 'green' },
        { key: 'Active Alerts',  value: '0',      mono: true, tint: 'green' },
        { key: 'Last Sync',      value: '8s ago', mono: true },
      ]},
    ],
    chart: { title: 'TRAFFIC FLOW (people/h)', values: seedProfile(14, 0.5, 0.0), xLabels: HOURS },
    controls: [{ label: 'View:', value: 'Map' }, { label: 'Layer:', value: 'Sensors' }],
  },
  {
    id: 'DS-PARKING', address: 'ลานจอด · ราชวิทยาลัยจุฬาภรณ์',
    status: { label: 'Busy', color: 'amber' },
    cardPrimary:   { label: 'Occupancy', value: '88%', tint: 'amber' },
    cardSecondary: { label: 'Alerts',    value: '1',   tint: 'amber' },
    previewIcon: 'globe',
    sections: [
      { title: 'PARKING', rows: [
        { key: 'Capacity', value: '420 slots' },
        { key: 'Occupied', value: '370',     mono: true, tint: 'amber' },
        { key: 'Available',value: '50',      mono: true },
        { key: 'EV Slots', value: '12 / 24', mono: true },
      ]},
      { title: 'ACTIVITY', rows: [
        { key: 'Active Sensors', value: '420',    mono: true, tint: 'green' },
        { key: 'Active Alerts',  value: '1 over-stay', mono: true, tint: 'amber' },
        { key: 'Last Sync',      value: '4s ago', mono: true },
      ]},
    ],
    chart: { title: 'OCCUPANCY (%)', values: seedProfile(15, 0.92, 0.4), xLabels: HOURS },
    controls: [{ label: 'View:', value: 'Slots' }, { label: 'Layer:', value: 'Occupancy' }],
  },
];

// ---------- registry ----------
export const ASSETS: Record<Category, AssetData[]> = {
  districts: DISTRICTS,
  vehicles:  VEHICLES,
  lamps:     LAMPS,
  power:     POWER,
  cameras:   CAMERAS,
  network:   NETWORK,
};
