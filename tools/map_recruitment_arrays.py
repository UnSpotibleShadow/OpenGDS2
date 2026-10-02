#!/usr/bin/env python3
"""Map the fGMSKoyou recruitment arrays in the verified GameDev2DX VB6 build.

Static only: the target executable is never run. This extends the M5 trace by
resolving the form constant-pool entries to the global storage addresses used by
the roster loop and applicant display.
"""
from __future__ import annotations
import argparse, hashlib, struct, sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent))
from inspect_vb6 import PeImage

KNOWN_SHA256='4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6'
POOL=0x0060C264
ENTRIES={
  0x02:'contract_fee_array', 0x0D:'employee_name_array',
  0x30:'roster_count', 0x31:'source_upper_bound', 0x32:'minimum_source_index',
  0x33:'source_gate_array', 0x34:'five_slot_roster_source_indices',
  0x36:'employee_level_array', 0x37:'display_stat_array_1',
  0x38:'display_stat_array_2', 0x39:'display_stat_array_3',
  0x3A:'display_stat_array_4', 0x3B:'display_stat_array_5',
}

def main():
 ap=argparse.ArgumentParser(description=__doc__); ap.add_argument('exe',type=Path); a=ap.parse_args()
 data=a.exe.read_bytes(); sha=hashlib.sha256(data).hexdigest()
 if sha!=KNOWN_SHA256: raise SystemExit(f'unsupported SHA-256: {sha}')
 pe=PeImage(data)
 print('OpenGDS2 M6 recruitment global-array map')
 print('target execution: NEVER (static parsing only)')
 print(f'constant pool: 0x{POOL:08X}\n')
 for i,name in ENTRIES.items(): print(f'pool[{i:02X}] -> 0x{pe.u32(POOL+i*4):08X}  {name}')
 print('\nRecovered loop semantics:')
 print('  for sourceIndex = 0 to source_upper_bound - 1')
 print('    if minimum_source_index > sourceIndex: skip')
 print('    if source_gate_array[sourceIndex] != 0: skip')
 print('    if Int(Rnd * 4) != 0: skip')
 print('    roster[roster_count++] = sourceIndex')
 print('  repeat passes while roster_count < 5')
 print('\nApplicant display dereferences roster[i] through name, level, five stat arrays, and contract fee.')
 print('The semantic names/order of the five stat arrays are intentionally left unresolved here.')
if __name__=='__main__': main()
