#!/usr/bin/env python3
"""Verify bounded VB Write/Input anchors; this does not import GD2 saves."""
import argparse
import hashlib
import json
from pathlib import Path
from inspect_vb6 import PeImage


def verify(executable):
    manifest = json.loads(Path(__file__).with_name('native_save_evidence.json').read_text())
    data = executable.read_bytes()
    if hashlib.sha256(data).hexdigest() != manifest['source_sha256']:
        raise ValueError('Unsupported original executable')
    pe = PeImage(data)
    signatures = []
    for item in manifest['procedures']:
        base = item['address']
        if hashlib.sha256(pe.bytes_at(base, item['size'])).hexdigest() != item['sha256']:
            raise ValueError('Save procedure fingerprint differs')
        for anchor in item['records'] + item['loops']:
            expected = bytes.fromhex(anchor['hex'])
            if pe.bytes_at(base + int(anchor['relative'], 16), len(expected)) != expected:
                raise ValueError('Save record/loop anchor differs')
        signatures.append([row['values'] for row in item['records']])
        if len(item['records']) != 130 or len(item['loops']) != 40:
            raise ValueError('Incomplete save control flow inventory')
    if signatures[0] != signatures[1]:
        raise ValueError('Writer/reader record arities differ')
    print('PASS native save evidence: two fingerprints, 130 paired record templates, 40 loops each')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('executable', type=Path)
    verify(parser.parse_args().executable)
