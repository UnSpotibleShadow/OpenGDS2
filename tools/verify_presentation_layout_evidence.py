#!/usr/bin/env python3
"""Check bounded native designer geometry/handler fingerprints without emitting assets."""
import argparse
import hashlib
import json
import struct
from pathlib import Path
from inspect_vb6 import ParseError, PeImage


def verify(data, evidence=None):
    if evidence is None:
        evidence = json.loads(Path(__file__).with_name('presentation_layout_evidence.json').read_text())
    if hashlib.sha256(data).hexdigest() != evidence['source_sha256']:
        raise ParseError('Unsupported executable')
    pe = PeImage(data)
    controls = 0
    for form in evidence['forms']:
        start, end = form['start'], form['end']
        name = form['name'].encode('ascii')
        if data[start:start + 2 + len(name) + 1] != struct.pack('<H', len(name)) + name + b'\0':
            raise ParseError('Changed serialized form: ' + form['name'])
        if form['client_evidence']:
            check_rect(data, form['client_evidence'], form['client'], start, end)
        if form['icon']:
            icon = form['icon']; offset, size = icon['offset'], icon['size']
            if not start <= offset < offset + size <= end or hashlib.sha256(data[offset:offset + size]).hexdigest() != icon['sha256']:
                raise ParseError('Invalid form icon binding: ' + form['name'])
        for control in form['controls']:
            offset, size = control['record_offset'], control['record_size']
            if not start <= offset < offset + size <= end:
                raise ParseError('Control crosses its form boundary')
            record = data[offset:offset + size]
            if hashlib.sha256(record).hexdigest() != control['sha256']:
                raise ParseError('Changed designer control: ' + control['name'])
            name = control['name'].encode('ascii')
            if struct.pack('<H', len(name)) + name + b'\0' not in record[:80]:
                raise ParseError('Changed control name binding')
            check_rect(data, control['bounds_evidence'], control['bounds'], offset, offset + size)
            controls += 1
    for procedure in evidence['procedures']:
        if hashlib.sha256(pe.bytes_at(int(procedure['start'], 16), procedure['size'])).hexdigest() != procedure['sha256']:
            raise ParseError('Changed native procedure: ' + procedure['start'])
    return len(evidence['forms']), controls, len(evidence['procedures'])


def check_rect(data, anchor, expected, start, end):
    offset, fmt = anchor['offset'], anchor['format']
    if fmt not in ('<hhhh', '<iiii') or not start <= offset < offset + 1 + struct.calcsize(fmt) <= end:
        raise ParseError('Invalid rectangle anchor')
    if data[offset] != anchor['property_index'] or list(struct.unpack_from(fmt, data, offset + 1)) != expected:
        raise ParseError('Changed serialized rectangle')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('executable', type=Path)
    counts = verify(parser.parse_args().executable.read_bytes())
    print(f'PASS native layout evidence: {counts[0]} forms, {counts[1]} rectangles and {counts[2]} procedures')
