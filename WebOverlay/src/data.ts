// POCO ของข้อมูลโคมไฟ + mock data
// แยกออกจาก components.ts เพื่อให้ swap data source (API/JSON) ภายหลังโดยไม่กระทบ UI builder

export type LampStatus = 'On' | 'Off' | 'NeedRepair';
export type Connection = 'Good' | 'Weak' | 'Down';

export interface LampData {
  id: string;
  address: string;
  status: LampStatus;
  type: string;
  lifeSpanPercent: number;
  lightIntensityLm: number;
  consumptionKWh: number;
  powerW: number;
  connection: Connection;
  uptime: string;
  controllerId: string;
  controllerModel: string;
  // pin position บน overlay (% ของ viewport — mockup ไม่ project จาก 3D world จริง)
  pinXPct: number;
  pinYPct: number;
  // 24 ค่าราย hour (0..1)
  illuminationProfile: number[];
}

const refProfile = (): number[] => [
  0.10, 0.10, 0.10, 0.10, 0.10, 0.10,
  0.70, 0.70, 0.70, 0.60, 0.60, 0.60,
  0.05, 0.05, 0.05, 0.05, 0.05, 0.05,
  0.65, 0.65, 0.65, 0.65, 0.65, 0.65,
];

const flatProfile = (day: number): number[] =>
  Array.from({ length: 24 }, (_, i) => (i < 6 || i >= 18 ? 0.65 : day));

export const LAMPS: LampData[] = [
  {
    id: 'LP-A012423', address: '223 Mountbatten Rd, 01-18, Singapore',
    status: 'Off', type: 'LED', lifeSpanPercent: 73, lightIntensityLm: 0,
    consumptionKWh: 0, powerW: 40, connection: 'Good', uptime: '12d 04h 22m',
    controllerId: '120B962153E29', controllerModel: 'OPENSKY-442',
    pinXPct: 62, pinYPct: 56, illuminationProfile: flatProfile(0),
  },
  {
    id: 'LP-A012406', address: '223 Mountbatten Rd, 01-18, Singapore',
    status: 'NeedRepair', type: 'LED', lifeSpanPercent: 8, lightIntensityLm: 350,
    consumptionKWh: 12, powerW: 40, connection: 'Weak', uptime: '6h 24m 15s',
    controllerId: '120B962153E32', controllerModel: 'OPENSKY-442',
    pinXPct: 48, pinYPct: 64, illuminationProfile: refProfile(),
  },
  {
    id: 'LP-A012417', address: '223 Mountbatten Rd, 01-18, Singapore',
    status: 'On', type: 'LED', lifeSpanPercent: 88, lightIntensityLm: 420,
    consumptionKWh: 14, powerW: 40, connection: 'Good', uptime: '32d 11h 03m',
    controllerId: '120B962153E45', controllerModel: 'OPENSKY-442',
    pinXPct: 70, pinYPct: 60, illuminationProfile: flatProfile(0.7),
  },
  {
    id: 'LP-A012438', address: '223 Mountbatten Rd, 01-18, Singapore',
    status: 'Off', type: 'LED', lifeSpanPercent: 41, lightIntensityLm: 0,
    consumptionKWh: 0, powerW: 40, connection: 'Weak', uptime: '5d 19h 47m',
    controllerId: '120B962153E51', controllerModel: 'OPENSKY-442',
    pinXPct: 88, pinYPct: 62, illuminationProfile: flatProfile(0),
  },
  {
    id: 'LP-A012414', address: '223 Mountbatten Rd, 01-18, Singapore',
    status: 'On', type: 'LED', lifeSpanPercent: 92, lightIntensityLm: 480,
    consumptionKWh: 15, powerW: 40, connection: 'Good', uptime: '58d 02h 14m',
    controllerId: '120B962153E66', controllerModel: 'OPENSKY-442',
    pinXPct: 82, pinYPct: 55, illuminationProfile: flatProfile(0.7),
  },
];

// pin เสริม (blue dots) — ไม่มี detail panel
export const EXTRA_PINS: { x: number; y: number }[] = [
  { x: 42, y: 50 }, { x: 56, y: 53 }, { x: 68, y: 48 }, { x: 76, y: 51 },
  { x: 84, y: 49 }, { x: 92, y: 54 }, { x: 38, y: 58 }, { x: 54, y: 60 },
  { x: 64, y: 64 }, { x: 74, y: 66 }, { x: 80, y: 68 }, { x: 86, y: 64 },
  { x: 46, y: 70 }, { x: 60, y: 72 }, { x: 78, y: 74 },
];
