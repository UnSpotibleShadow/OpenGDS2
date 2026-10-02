#!/usr/bin/env python3
"""Heuristic scanner for length-prefixed CP932 text in the VB6 executable.
Produces candidates for manual review; false positives are expected.
"""
from pathlib import Path
import argparse

ap=argparse.ArgumentParser()
ap.add_argument('exe')
ap.add_argument('-o','--output',default='cp932_string_candidates.tsv')
a=ap.parse_args()
b=Path(a.exe).read_bytes()
rows=[]
for i in range(len(b)-4):
    n=b[i] | (b[i+1] << 8)
    if not (2 <= n <= 160) or i+2+n > len(b):
        continue
    raw=b[i+2:i+2+n]
    if b'\x00' in raw:
        continue
    try:
        s=raw.decode('cp932')
    except UnicodeDecodeError:
        continue
    jp=sum(1 for c in s if ('\u3040'<=c<='\u30ff' or '\u3400'<=c<='\u9fff' or c in '￥円億万年月日'))
    if jp < 1 or any((ord(c)<32 or '\ue000'<=c<='\uf8ff') for c in s):
        continue
    if len(set(s)) < 2 and len(s) > 3:
        continue
    rows.append((i,n,s))
seen=set()
with open(a.output,'w',encoding='utf-8') as f:
    f.write('offset_hex\tbyte_length\tja_candidate\n')
    for off,n,s in rows:
        if s in seen: continue
        seen.add(s)
        s=s.replace('\t',' ').replace('\r',' ').replace('\n',' ')
        f.write(f'0x{off:08X}\t{n}\t{s}\n')
print(f'wrote {len(seen)} unique candidates to {a.output}')
