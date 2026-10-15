#!/usr/bin/env python3
"""Check bounded original event/settings evidence; emit no original text or assets."""
import argparse
import hashlib
import json
from pathlib import Path
from inspect_vb6 import PeImage


def verify(executable):
    evidence=json.loads(Path(__file__).with_name('company_events_evidence.json').read_text());data=executable.read_bytes()
    if hashlib.sha256(data).hexdigest()!=evidence['source_sha256']:raise ValueError('Unsupported original executable')
    pe=PeImage(data)
    for row in evidence['procedures']:
        if hashlib.sha256(pe.bytes_at(int(row['start'],16),row['size'])).hexdigest()!=row['sha256']:
            raise ValueError('Changed company event procedure')
    for row in evidence['anchors']:
        if hashlib.sha256(pe.bytes_at(int(row['procedure'],16)+row['at'],row['bytes'])).hexdigest()!=row['sha256']:
            raise ValueError('Changed company event anchor')
    print(f"PASS company events/settings: {len(evidence['procedures'])} fingerprints and {len(evidence['anchors'])} bounded anchors; original runtime pending")


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('executable',type=Path);verify(parser.parse_args().executable)
