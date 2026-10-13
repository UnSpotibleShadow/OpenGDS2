#!/usr/bin/env python3
"""Verify bounded programming-challenge evidence without emitting original text."""
import argparse
import hashlib
import json
import re
import struct
from pathlib import Path
from inspect_vb6 import PeImage


def verify(path):
    manifest = json.loads(Path(__file__).with_name('programming_challenge_evidence.json').read_text())
    data = path.read_bytes()
    if hashlib.sha256(data).hexdigest() != manifest['source_sha256']:
        raise ValueError('Unsupported original executable')
    pe = PeImage(data)
    for item in manifest['fingerprints']:
        raw = pe.bytes_at(int(item['start'], 16), item['size'])
        if hashlib.sha256(raw).hexdigest() != item['sha256']:
            raise ValueError('Procedure fingerprint differs')
    for item in manifest['anchors']:
        raw = bytes.fromhex(item['hex'])
        if pe.bytes_at(int(item['start'], 16) + int(item['relative'], 16), len(raw)) != raw:
            raise ValueError('Bounded numeric anchor differs')
    load = manifest['reel_load']
    raw = pe.bytes_at(int(load['start'], 16), load['size'])
    pattern = rb'\x28\x58\xff(..)\xf5(.{4})\x05\x14\x00\x52'
    matches = list(re.finditer(pattern, raw, re.S))
    if len(matches) != 50:
        raise ValueError('Reel writer count differs')
    values = [struct.unpack('<H', m.group(1))[0] for m in matches]
    indices = [struct.unpack('<I', m.group(2))[0] for m in matches]
    tables = [values[n:n + 10] for n in range(0, 50, 10)]
    if tables != load['tables'] or indices != list(range(10)) * 5:
        raise ValueError('Reel slot mapping differs')
    timer = 0x0098F68C
    modes = [struct.unpack('<H', pe.bytes_at(timer + offset + 3, 2))[0]
             for offset in (0x01AA, 0x01EB, 0x022C)]
    if modes != [6, 10, 17] or modes != manifest['result_modes']:
        raise ValueError('Result meeting mode differs')
    print(f"PASS programming challenge: {len(manifest['fingerprints'])} fingerprints, "
          f"{len(manifest['anchors'])} anchors, five ten-slot reels, result modes 6/10/17")


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('executable', type=Path)
    verify(parser.parse_args().executable)
