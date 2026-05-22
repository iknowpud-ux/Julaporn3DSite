// คำสั่งเสียง — Web Speech API (Chrome/Edge)
// continuous mode: พูดได้เรื่อยๆ ไม่ต้องกดซ้ำ

import type { Category } from './data';

export type VoiceCommand =
  | { type: 'category'; value: Category }
  | { type: 'select_index'; index: number }
  | { type: 'focus_lamp'; id: string }
  | { type: 'greeting' }
  | { type: 'open' }
  | { type: 'close' };

export interface VoiceCallbacks {
  onCommand: (cmd: VoiceCommand, transcript: string) => void;
  onUnrecognized?: (transcript: string) => void;
  onInterim?: (transcript: string) => void;
  onListening: (active: boolean) => void;
  onPermissionDenied?: () => void;
}

const CMD_MAP: ReadonlyArray<{ patterns: readonly string[]; cmd: VoiceCommand }> = [
  { patterns: ['ลานจอดรถ', 'parking a', 'จอดรถ'],           cmd: { type: 'focus_lamp', id: 'LP-A012423' } },
  { patterns: ['ทางเข้าหลัก', 'main entrance', 'ทางเข้า'],  cmd: { type: 'focus_lamp', id: 'LP-A012406' } },
  { patterns: ['อาคารวิจัย', 'research building', 'วิจัย'], cmd: { type: 'focus_lamp', id: 'LP-A012417' } },
  { patterns: ['สวนกลาง', 'central garden', 'central park'], cmd: { type: 'focus_lamp', id: 'LP-A012438' } },
  { patterns: ['ทางเชื่อม', 'connector', 'walkway'],         cmd: { type: 'focus_lamp', id: 'LP-A012414' } },
  { patterns: ['smart lighting','ไฟถนน','แสงสว่าง','โคมไฟ','โคม','lamps','lamp','lighting'], cmd: { type: 'category', value: 'lamps' } },
  { patterns: ['กล้องวงจรปิด','วงจรปิด','surveillance','cctv','กล้อง','cameras','camera'],   cmd: { type: 'category', value: 'cameras' } },
  { patterns: ['ยานพาหนะ','transport','vehicles','vehicle','รถ'],                            cmd: { type: 'category', value: 'vehicles' } },
  { patterns: ['power grid','electricity','พลังงาน','ไฟฟ้า','energy','power','grid'],        cmd: { type: 'category', value: 'power' } },
  { patterns: ['เครือข่าย','อินเทอร์เน็ต','internet','network','เน็ต','wifi'],               cmd: { type: 'category', value: 'network' } },
  { patterns: ['districts','district','พื้นที่','เขต','zone','area'],                        cmd: { type: 'category', value: 'districts' } },
  { patterns: ['อันแรก','อันที่หนึ่ง','ตัวแรก','ที่หนึ่ง','first','one'],   cmd: { type: 'select_index', index: 0 } },
  { patterns: ['อันที่สอง','ตัวที่สอง','ที่สอง','second','two'],             cmd: { type: 'select_index', index: 1 } },
  { patterns: ['อันที่สาม','ตัวที่สาม','ที่สาม','third','three'],            cmd: { type: 'select_index', index: 2 } },
  { patterns: ['อันที่สี่','ตัวที่สี่','ที่สี่','fourth','four'],             cmd: { type: 'select_index', index: 3 } },
  { patterns: ['อันที่ห้า','ตัวที่ห้า','ที่ห้า','fifth','five'],              cmd: { type: 'select_index', index: 4 } },
  { patterns: ['สวัสดี','hello','hi'],                                        cmd: { type: 'greeting' } },
  { patterns: ['display','show','open','แสดง','เปิด'],                        cmd: { type: 'open' } },
  { patterns: ['dismiss','exit','hide','close','ซ่อน','ปิด'],                 cmd: { type: 'close' } },
];

function matchCmd(transcript: string): VoiceCommand | null {
  const t = transcript.toLowerCase().trim();
  for (const { patterns, cmd } of CMD_MAP) {
    if (patterns.some((p) => t.includes(p))) return cmd;
  }
  return null;
}

const SR: (new () => SpeechRecognition) | undefined =
  (window as any).SpeechRecognition ?? (window as any).webkitSpeechRecognition;

export const voiceSupported = !!SR;

export function createVoice(cb: VoiceCallbacks): { toggle: () => void; listening: () => boolean } {
  if (!SR) return { toggle: () => {}, listening: () => false };

  const rec = new SR();
  rec.continuous      = true;
  rec.interimResults  = true;
  rec.lang            = 'th-TH';
  rec.maxAlternatives = 3;

  let _listening = false;
  let _wantOn    = false;

  rec.onresult = (e: SpeechRecognitionEvent) => {
    const last = e.results[e.results.length - 1];
    if (!last?.isFinal) {
      cb.onInterim?.(last[0]?.transcript ?? '');
      return;
    }
    cb.onInterim?.('');
    for (let i = 0; i < last.length; i++) {
      const text = last[i]?.transcript ?? '';
      const cmd  = matchCmd(text);
      if (cmd) { cb.onCommand(cmd, text); return; }
    }
    cb.onUnrecognized?.(last[0]?.transcript ?? '');
  };

  rec.onend = () => {
    if (_wantOn) {
      try { rec.start(); } catch (_) { _wantOn = _listening = false; cb.onListening(false); }
    } else {
      _listening = false;
      cb.onListening(false);
    }
  };

  rec.onerror = (e: SpeechRecognitionErrorEvent) => {
    if (e.error === 'no-speech') return;
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
        rec.stop();
      } else {
        _wantOn = true;
        try { rec.start(); _listening = true; cb.onListening(true); }
        catch (_) { _wantOn = false; }
      }
    },
    listening: () => _listening,
  };
}
