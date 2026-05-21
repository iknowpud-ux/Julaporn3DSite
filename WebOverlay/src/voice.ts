// คำสั่งเสียงสำหรับ Smart City Dashboard — Web Speech API wrapper
// รองรับ TH + EN code-switching, continuous mode, toggle on/off
// Chrome/Edge รองรับดี — Firefox ไม่รองรับ Web Speech API

import type { Category } from './data';

export type VoiceCommand =
  | { type: 'category'; value: Category }
  | { type: 'select_index'; index: number }
  | { type: 'focus_lamp'; id: string }
  | { type: 'open' }
  | { type: 'close' };

export interface VoiceCallbacks {
  onCommand: (cmd: VoiceCommand, transcript: string) => void;
  /** ได้ยินแต่ไม่ match command ใดเลย — ใช้แสดง feedback ให้ user รู้ว่า mic ทำงาน */
  onUnrecognized?: (transcript: string) => void;
  onListening: (active: boolean) => void;
  onPermissionDenied?: () => void;
}

// command map — เรียงจาก specific (วลียาว) → generic (คำสั้น)
// first-match wins → สถานที่ก่อน category ก่อน open/close
const CMD_MAP: ReadonlyArray<{ patterns: readonly string[]; cmd: VoiceCommand }> = [
  // --- focus lamp by location (hardcode ตาม mock data) ---
  // ทำงานทุก tab — สั่งชื่อสถานที่แล้วกล้อง Unity บินไปเลย
  { patterns: ['ลานจอดรถ', 'parking a', 'จอดรถ'],            cmd: { type: 'focus_lamp', id: 'LP-A012423' } },
  { patterns: ['ทางเข้าหลัก', 'main entrance', 'ทางเข้า'],   cmd: { type: 'focus_lamp', id: 'LP-A012406' } },
  { patterns: ['อาคารวิจัย', 'research building', 'วิจัย'],  cmd: { type: 'focus_lamp', id: 'LP-A012417' } },
  { patterns: ['สวนกลาง', 'central garden', 'central park'],  cmd: { type: 'focus_lamp', id: 'LP-A012438' } },
  { patterns: ['ทางเชื่อม', 'connector', 'walkway'],         cmd: { type: 'focus_lamp', id: 'LP-A012414' } },
  // --- categories ---
  {
    patterns: ['smart lighting', 'ไฟถนน', 'แสงสว่าง', 'โคมไฟ', 'โคม', 'lamps', 'lamp', 'lighting'],
    cmd: { type: 'category', value: 'lamps' },
  },
  {
    patterns: ['กล้องวงจรปิด', 'วงจรปิด', 'surveillance', 'cctv', 'กล้อง', 'cameras', 'camera'],
    cmd: { type: 'category', value: 'cameras' },
  },
  {
    patterns: ['ยานพาหนะ', 'transport', 'vehicles', 'vehicle', 'รถ'],
    cmd: { type: 'category', value: 'vehicles' },
  },
  {
    patterns: ['power grid', 'electricity', 'พลังงาน', 'ไฟฟ้า', 'energy', 'power', 'grid'],
    cmd: { type: 'category', value: 'power' },
  },
  {
    patterns: ['เครือข่าย', 'อินเทอร์เน็ต', 'internet', 'network', 'เน็ต', 'wifi'],
    cmd: { type: 'category', value: 'network' },
  },
  {
    patterns: ['districts', 'district', 'พื้นที่', 'เขต', 'zone', 'area'],
    cmd: { type: 'category', value: 'districts' },
  },
  // ordinal card selection — ก่อน open/close กัน "อันแรก" match "แรก" ไม่ได้
  { patterns: ['อันแรก', 'อันที่หนึ่ง', 'ตัวแรก', 'ที่หนึ่ง', 'first', 'one'],  cmd: { type: 'select_index', index: 0 } },
  { patterns: ['อันที่สอง', 'ตัวที่สอง', 'ที่สอง', 'second', 'two'],            cmd: { type: 'select_index', index: 1 } },
  { patterns: ['อันที่สาม', 'ตัวที่สาม', 'ที่สาม', 'third', 'three'],           cmd: { type: 'select_index', index: 2 } },
  { patterns: ['อันที่สี่',  'ตัวที่สี่',  'ที่สี่',  'fourth', 'four'],          cmd: { type: 'select_index', index: 3 } },
  { patterns: ['อันที่ห้า',  'ตัวที่ห้า',  'ที่ห้า',  'fifth',  'five'],          cmd: { type: 'select_index', index: 4 } },
  // open/close — หลังสุดเพราะสั้นที่สุด (กัน false positive)
  {
    patterns: ['display', 'show', 'open', 'แสดง', 'เปิด'],
    cmd: { type: 'open' },
  },
  {
    patterns: ['dismiss', 'exit', 'hide', 'close', 'ซ่อน', 'ปิด'],
    cmd: { type: 'close' },
  },
];

function matchCmd(transcript: string): VoiceCommand | null {
  const t = transcript.toLowerCase().trim();
  for (const { patterns, cmd } of CMD_MAP) {
    // substring match — TH ไม่มี word boundary, EN ก็ใช้ includes ได้เพราะ pattern specific พอ
    if (patterns.some((p) => t.includes(p))) return cmd;
  }
  return null;
}

// ตรวจ browser support — Chrome/Edge ใช้ webkitSpeechRecognition, standard SpeechRecognition ยัง draft
const SR: (new () => SpeechRecognition) | undefined =
  (window as any).SpeechRecognition ?? (window as any).webkitSpeechRecognition;

export const voiceSupported = !!SR;

export function createVoice(cb: VoiceCallbacks): { toggle: () => void; listening: () => boolean } {
  if (!SR) return { toggle: () => {}, listening: () => false };

  const rec = new SR();
  rec.continuous     = true;
  rec.interimResults = false;
  // th-TH บน Chrome ใช้ Google STT ซึ่ง handle TH/EN code-switching ได้โดยธรรมชาติ
  rec.lang           = 'th-TH';
  // 3 alternatives เพิ่มโอกาส keyword match เมื่อ browser ไม่แน่ใจระหว่าง TH กับ EN pronunciation
  rec.maxAlternatives = 3;

  let _listening = false;
  let _wantOn    = false; // intent state กัน race condition start/stop

  rec.onresult = (e: SpeechRecognitionEvent) => {
    const last = e.results[e.results.length - 1];
    if (!last?.isFinal) return;

    for (let i = 0; i < last.length; i++) {
      const text = last[i]?.transcript ?? '';
      const cmd  = matchCmd(text);
      if (cmd) { cb.onCommand(cmd, text); return; }
    }
    // ไม่ match command ใดเลย — แจ้ง transcript เพื่อแสดง feedback
    cb.onUnrecognized?.(last[0]?.transcript ?? '');
  };

  rec.onend = () => {
    // browser auto-stop เพราะ silence timeout → restart ถ้า user ยังต้องการ listen
    if (_wantOn) {
      try {
        rec.start();
      } catch (_) {
        // restart ล้มเหลว (เช่น permission revoked หลัง start) → force stop state
        _wantOn = _listening = false;
        cb.onListening(false);
      }
    } else {
      _listening = false;
      cb.onListening(false);
    }
  };

  rec.onerror = (e: SpeechRecognitionErrorEvent) => {
    if (e.error === 'no-speech') return; // silence — ไม่ใช่ error จริง
    if (e.error === 'not-allowed') {
      _wantOn = _listening = false;
      cb.onListening(false);
      cb.onPermissionDenied?.();
    }
  };

  return {
    toggle() {
      if (_listening) {
        _wantOn = false;
        rec.stop(); // onend จะ set _listening=false + fire cb.onListening(false)
      } else {
        _wantOn = true;
        try {
          rec.start();
          _listening = true;
          cb.onListening(true);
        } catch (_) {
          // start ล้มเหลว (recognition already running หรือ permission issue)
          _wantOn = false;
        }
      }
    },
    listening: () => _listening,
  };
}
