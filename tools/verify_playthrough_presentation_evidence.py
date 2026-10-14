#!/usr/bin/env python3
"""Check presentation anchors against the owner's local original; emit no assets."""
import argparse
import hashlib
import json
import struct
from pathlib import Path
from inspect_vb6 import PeImage, ParseError


def verify(data):
    evidence = json.loads(Path(__file__).with_name('playthrough_presentation_evidence.json').read_text())
    if hashlib.sha256(data).hexdigest() != evidence['source_sha256']:
        raise ParseError('Unsupported executable')
    pe = PeImage(data)
    for label, span in evidence['spans'].items():
        if hashlib.sha256(pe.bytes_at(span['address'], span['size'])).hexdigest() != span['sha256']:
            raise ParseError('Changed procedure: ' + label)
    for anchor in evidence['file_anchors']:
        expected = bytes.fromhex(anchor['hex'])
        if data[anchor['file_offset']:anchor['file_offset']+len(expected)] != expected:
            raise ParseError('Changed anchor: ' + anchor['label'])
    for resource in evidence['resources']:
        offset = resource['file_offset']
        raw = data[offset:offset+resource['size']]
        if raw[:2] != b'BM' or hashlib.sha256(raw).hexdigest() != resource['sha256']:
            raise ParseError('Changed resource: ' + resource['label'])
        if struct.unpack_from('<ii', raw, 18) != (resource['width'], resource['height']):
            raise ParseError('Changed bitmap dimensions')
    return len(evidence['spans']), len(evidence['resources'])


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('executable', type=Path)
    counts = verify(parser.parse_args().executable.read_bytes())
    print(f'PASS playthrough presentation evidence: {counts[0]} procedure fingerprints, {counts[1]} button resources, timer anchor')
