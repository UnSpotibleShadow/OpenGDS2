#!/usr/bin/env python3
"""Verify M88 structure/fingerprints without printing original race records."""
import argparse
import hashlib
import json
from pathlib import Path
from inspect_vb6 import PeImage, ParseError


def verify(path):
    manifest = json.loads(Path(__file__).with_name('horse_catalog_evidence.json').read_text())
    raw = path.read_bytes()
    if hashlib.sha256(raw).hexdigest() != manifest['source_sha256']:
        raise ParseError('Unsupported original executable')
    pe = PeImage(raw)
    for span in manifest['helpers']:
        if hashlib.sha256(pe.bytes_at(span['address'], span['size'])).hexdigest() != span['sha256']:
            raise ParseError('Horse append helper changed')
    c = manifest['selector_control']
    if hashlib.sha256(raw[c['offset']:c['offset']+c['length']]).hexdigest() != c['sha256']:
        raise ParseError('Casino horse selector control changed')
    for label, opcode, expected_count in [('runner_calls', bytes.fromhex('0ac3021400'), 44),
                                          ('jockey_calls', bytes.fromhex('0af0020c00'), 10)]:
        calls = manifest[label]
        if len(calls) != expected_count or any(pe.bytes_at(0x9F651C+offset, 5) != opcode for offset in calls):
            raise ParseError('Horse initialization call sequence changed')
    print('PASS: M88 horse catalog evidence; 44 runner calls; 10 jockey calls; 2 helpers; selector control')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('executable', type=Path)
    args = parser.parse_args()
    verify(args.executable)
