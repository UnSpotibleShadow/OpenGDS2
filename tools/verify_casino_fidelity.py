#!/usr/bin/env python3
"""Check M78 source anchors; optional reference runtime, never execute the game."""
import argparse
import hashlib
import json
import struct
from pathlib import Path
from inspect_vb6 import PeImage, ParseError


def require(ok, label):
    if not ok:
        raise ParseError('Changed casino fidelity evidence: ' + label)


def verify(exe, runtime=None):
    m = json.loads(Path(__file__).with_name('casino_fidelity.json').read_text())
    data = exe.read_bytes()
    require(hashlib.sha256(data).hexdigest() == m['source_sha256'], 'executable hash')
    pe = PeImage(data)
    span = m['race_span']
    require(hashlib.sha256(pe.bytes_at(span['address'], span['size'])).hexdigest()
            == span['sha256'], 'race procedure fingerprint')
    for c in m['controls']:
        raw = data[c['offset']:c['offset'] + c['length']]
        require(hashlib.sha256(raw).hexdigest() == c['sha256'], 'serialized control')
    for a in m['movement']:
        address = a['literal_address']
        if a['literal_kind'] == 'i2':
            require(pe.bytes_at(address, 1) == b'\x28', 'I2 divisor opcode')
            value = struct.unpack('<h', pe.bytes_at(address + 3, 2))[0]
        else:
            require(pe.bytes_at(address, 2) == b'\xfe\xc4', 'R8 divisor opcode')
            value = struct.unpack('<d', pe.bytes_at(address + 4, 8))[0]
        require(value == a['denominator'], 'movement divisor')
        require(pe.bytes_at(a['divide_address'], 2) == b'\xfb\xbc', 'division')
        require(pe.bytes_at(a['arithmetic_address'], 2)
                == (b'\xfb\x94' if a['sign'] == '+' else b'\xfb\x9c'), 'movement sign')
        require(pe.bytes_at(span['address'] + a['store_offset'], 1) == b'\x52',
                'array destination store')
    for a in m['race_literals']:
        address = span['address'] + a['offset']
        require(pe.bytes_at(address, 1) == b'\x28' and
                struct.unpack('<h', pe.bytes_at(address + 3, 2))[0] == a['value'],
                'race threshold/window')
    kp = m['key_preview']
    require(data[kp['offset']:kp['offset'] + 2] == bytes([kp['code'], kp['value']]),
            'card-chain serialized KeyPreview')
    if runtime:
        raw = runtime.read_bytes()
        require(hashlib.sha256(raw).hexdigest() == m['runtime']['sha256'],
                'supported reference runtime (not bundled with game)')
        rp = PeImage(raw)
        for a in m['runtime']['spans']:
            require(hashlib.sha256(rp.bytes_at(a['address'], a['size'])).hexdigest()
                    == a['sha256'], a['name'])
        for a in m['runtime']['property_indices']:
            require(rp.u32(a['table'] + 4 * a['index']) == a['descriptor'] and
                    rp.u32(a['descriptor']) == a['name_address'] and
                    rp.bytes_at(a['name_address'], len(a['name']) + 1) ==
                    a['name'].encode('ascii') + b'\x00', 'runtime property index')
    return (len(m['movement']), len(m['race_literals']), len(m['controls']))


if __name__ == '__main__':
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('exe', type=Path)
    p.add_argument('--reference-runtime', type=Path)
    args = p.parse_args()
    counts = verify(args.exe, args.reference_runtime)
    print('PASS: M78 source anchors; %d movement operations; %d race literals; '
          '%d controls; reference runtime %s' %
          (*counts, 'checked' if args.reference_runtime else 'not supplied'))
