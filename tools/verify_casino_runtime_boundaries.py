#!/usr/bin/env python3
"""M79 bounded source anchors and finite odds analysis; no game execution."""
import argparse
import hashlib
import json
import struct
from fractions import Fraction
from pathlib import Path
from inspect_vb6 import PeImage, ParseError


def require(ok, label):
    if not ok:
        raise ParseError('Changed M79 evidence: ' + label)


def verify(exe, runtime=None):
    m = json.loads(Path(__file__).with_name('casino_runtime_boundaries.json').read_text())
    data = exe.read_bytes()
    require(hashlib.sha256(data).hexdigest() == m['source_sha256'], 'original game')
    pe = PeImage(data)
    h = m['header']
    require(pe.u16(h['address'] + 4) == h['build'] and
            pe.u16(h['address'] + 34) == h['abi_revision'] and
            pe.u32(h['address'] + 36) == h['lcid'], 'VB6 header identity')
    for a in m['spans']:
        require(hashlib.sha256(pe.bytes_at(a['address'], a['size'])).hexdigest()
                == a['sha256'], a['name'])
    for a in m['render_literals']:
        if a['kind'] == 'i4':
            require(pe.bytes_at(a['address'], 1) == b'\xf5', a['label'] + ' opcode')
            value = struct.unpack('<i', pe.bytes_at(a['address'] + 1, 4))[0]
        else:
            require(pe.bytes_at(a['address'], 1) == b'\x28', a['label'] + ' opcode')
            value = struct.unpack('<h', pe.bytes_at(a['address'] + 3, 2))[0]
        require(value == a['value'], a['label'])
    odds = [Fraction(s) for s in m['odds']]
    pairs = [a * b / Fraction(m['pair_divisor'])
             for a in odds for b in odds]
    distance = min(abs((p * 10) % 1 - Fraction(1, 2)) for p in pairs)
    require(distance == Fraction(1, 12), 'nearest midpoint distance')
    # These are nearest-rounding consistency checks, not calls to VarFormat.
    for i, a in enumerate(odds):
        for j, b in enumerate(odds):
            exact = a * b / Fraction(m['pair_divisor'])
            tenth = (exact * 10 + Fraction(1, 2)).numerator // \
                    (exact * 10 + Fraction(1, 2)).denominator
            binary = float(a) * float(b) / float(Fraction(m['pair_divisor']))
            require(round(binary * 10) == tenth, 'binary64 odds rounding')
    if runtime:
        data = runtime.read_bytes()
        require(hashlib.sha256(data).hexdigest() == m['runtime_sha256'], 'reference DLL')
        rp = PeImage(data)
        for a in m['runtime_spans']:
            require(hashlib.sha256(rp.bytes_at(a['address'], a['size'])).hexdigest()
                    == a['sha256'], a['name'])
    return len(pairs), distance / 10


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('exe', type=Path)
    parser.add_argument('--reference-runtime', type=Path)
    args = parser.parse_args()
    count, margin = verify(args.exe, args.reference_runtime)
    print('PASS: M79 anchors; %d odds-value combinations (including repeated odds); no midpoint; '
          'minimum odds-unit distance %s (not native OLE acceptance)' % (count, margin))
