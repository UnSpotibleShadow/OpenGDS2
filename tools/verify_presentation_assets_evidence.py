#!/usr/bin/env python3
"""Verify M96's bounded artwork roles/marker corrections; emit no original pixels/text."""
import argparse
import hashlib
import json
import struct
from pathlib import Path
from inspect_vb6 import PeImage, ParseError


def verify(data):
    evidence = json.loads(Path(__file__).with_name('presentation_assets_evidence.json').read_text())
    if hashlib.sha256(data).hexdigest() != evidence['source_sha256']:
        raise ParseError('Unsupported original executable')
    pe = PeImage(data)
    for row in evidence['resources']:
        at, size = row['file_offset'], row['size']
        raw = data[at:at + size]
        name = row['control'].encode()
        if (len(raw) != size or hashlib.sha256(raw).hexdigest() != row['sha256'] or
                data[at - len(name) - 18:at - 15] != name + b'\0\x18\x01' or
                struct.unpack_from('<H', data, at - 15)[0] != row['index']):
            raise ParseError('Changed resource binding: ' + row['tag'])
        dimensions = struct.unpack_from('<ii', raw, 18) if raw[:2] == b'BM' else struct.unpack_from('<HH', raw, 6)
        if dimensions != (row['width'], row['height']):
            raise ParseError('Changed resource dimensions: ' + row['tag'])
    for anchor in evidence['anchors']:
        raw = bytes.fromhex(anchor['hex'])
        if pe.bytes_at(anchor['address'], len(raw)) != raw:
            raise ParseError('Changed anchor: ' + anchor['label'])
    groups = evidence['comment_templates']
    if list(map(len, groups)) != [9, 8, 11, 9, 8, 4]:
        raise ParseError('Invalid comment variant bounds')
    for group in groups:
        for row in group:
            if hashlib.sha256(pe.bytes_at(row['body_address'], row['body_size'])).hexdigest() != row['body_sha256']:
                raise ParseError('Changed bounded comment branch')
            # Comment writes follow the validated supported timer; token strings
            # are read from the original pool only by the local importer.
            if not row['tokens'] or any(t < -3 or t == 0 for t in row['tokens']):
                raise ParseError('Invalid comment token reference')
    return f"PASS presentation assets: {len(evidence['resources'])} resource bindings, {len(evidence['anchors'])} anchors, 49 comment templates"


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('executable', type=Path)
    args = parser.parse_args()
    try:
        print(verify(args.executable.read_bytes()))
    except (OSError, ValueError, ParseError) as error:
        parser.exit(1, str(error) + '\n')


if __name__ == '__main__':
    main()
