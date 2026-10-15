#!/usr/bin/env python3
"""Verify #118 event/reset/copy/hover/scene source fingerprints without text or pixels."""
import argparse
import hashlib
import json
from pathlib import Path
from inspect_vb6 import PeImage

def verify(path):
    data=path.read_bytes()
    evidence=json.loads(Path(__file__).with_name('issue118_evidence.json').read_text())
    if hashlib.sha256(data).hexdigest()!=evidence['source_sha256']:raise ValueError('Unsupported original executable')
    pe=PeImage(data)
    for p in evidence['procedures']:
        if hashlib.sha256(pe.bytes_at(int(p['start'],16),p['size'])).hexdigest()!=p['sha256']:raise ValueError('Changed issue118 procedure '+p['start'])
    for r in evidence['resources']:
        raw=data[r['file_offset']:r['file_offset']+r['size']]
        if len(raw)!=r['size'] or hashlib.sha256(raw).hexdigest()!=r['sha256']:raise ValueError('Changed issue118 scene')
    print(f"PASS issue118 source: {len(evidence['procedures'])} procedures, {len(evidence['resources'])} distinct scenes")
if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('executable',type=Path);verify(parser.parse_args().executable)
