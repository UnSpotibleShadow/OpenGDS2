#!/usr/bin/env python3
"""Verify native third-review resource/handler bindings without emitting assets."""
import argparse
import hashlib
import json
import struct
import re
from pathlib import Path
from inspect_vb6 import PeImage, ParseError


def verify(data):
    evidence = json.loads(Path(__file__).with_name('review_round_evidence.json').read_text())
    if hashlib.sha256(data).hexdigest() != evidence['source_sha256']:
        raise ParseError('Unsupported executable')
    pe = PeImage(data)
    # A valid ICO signature alone must not let one form borrow its neighbour's icon.
    census = json.loads(Path(__file__).with_name('window_census_evidence.json').read_text())
    markers = []
    for form in census['forms']:
        name = form['name'].encode('ascii')
        hits = [m.start() for m in re.finditer(re.escape(struct.pack('<H', len(name)) + name + b'\0'), data)]
        if not hits:
            raise ParseError('Missing serialized form: ' + form['name'])
        markers.append((max(hits), form['name']))
    markers.sort()
    ranges = {name: (start, markers[i + 1][0] if i + 1 < len(markers) else len(data))
              for i, (start, name) in enumerate(markers)}
    for entry in evidence['procedures']:
        if hashlib.sha256(pe.bytes_at(entry['address'], entry['size'])).hexdigest() != entry['sha256']:
            raise ParseError('Changed procedure: ' + entry['name'])
    for entry in evidence['icons']:
        offset, size = entry['offset'], entry['size']
        start, end = ranges[entry['form']]
        if not start <= offset < offset + size <= end:
            raise ParseError('Icon crosses its serialized form boundary: ' + entry['form'])
        raw = data[offset:offset+size]
        if raw[:4] != b'\0\0\1\0' or hashlib.sha256(raw).hexdigest() != entry['sha256']:
            raise ParseError('Changed form icon: ' + entry['form'])
    for entry in evidence['facilities']:
        art = entry['art'];offset = art['offset']
        bitmap = data[offset:offset+2] == b'BM'
        size = struct.unpack_from('<I', data, offset+2 if bitmap else offset-4)[0]
        if hashlib.sha256(data[offset:offset+size]).hexdigest() != art['sha256']:
            raise ParseError('Changed facility art: ' + entry['form'])
        marker = b'ImageFormG\0\x18\x01'
        if data[offset-28:offset-15] != marker or struct.unpack_from('<H', data, offset-15)[0] != entry['image_form_g_index']:
            raise ParseError('Changed shared illustration binding')
    for entry in evidence['computers']:
        offset = entry['offset'];size = struct.unpack_from('<I', data, offset+2)[0]
        if hashlib.sha256(data[offset:offset+size]).hexdigest() != entry['sha256']:
            raise ParseError('Changed computer preview')
        if data[offset-24:offset-15] != b'Image1\0\x18\x01' or struct.unpack_from('<H', data, offset-15)[0] != entry['control_index']:
            raise ParseError('Changed computer preview binding')
    return len(evidence['icons']), len(evidence['facilities']), len(evidence['computers']), len(evidence['procedures'])


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('executable', type=Path)
    counts = verify(parser.parse_args().executable.read_bytes())
    print(f'PASS third review evidence: {counts[0]} form icons, {counts[1]} facilities, {counts[2]} computer previews and {counts[3]} procedure fingerprints')
