#!/usr/bin/env python3
"""Check facility-chain source fingerprints and all nine illustration bindings.

Reads only a locally supplied supported original. Emits no dialogue, code or pixels.
Fingerprints establish source identity, not native-runtime acceptance.
"""
import argparse
import hashlib
import json
import struct
from pathlib import Path

from inspect_vb6 import PeImage, inspect
import map_original_game as mapping


def verify(executable):
    data = executable.read_bytes()
    evidence = json.loads(Path(__file__).with_name('facility_chain_evidence.json').read_text())
    if hashlib.sha256(data).hexdigest() != evidence['source_sha256']:
        raise ValueError('Unsupported original executable')
    pe = PeImage(data)
    metadata = inspect(executable)
    objects = {o['name']: o for o in metadata['objects']}
    extra = {'0x009EFA40', '0x009E3830', '0x009A6DDC', '0x009933DC'}
    actual = {(o['name'], d['start']): d for o in objects.values() for d in o['procedures']
              if 'Tenant' in o['name'] or d.get('start') in extra}
    recorded = {(p['object'], p['start']) for p in evidence['procedures']}
    if set(actual) != recorded:
        raise ValueError('Facility procedure coverage changed')
    for procedure in evidence['procedures']:
        key = (procedure['object'], procedure['start'])
        if actual[key]['proc_size'] != procedure['size'] or hashlib.sha256(
                pe.bytes_at(int(procedure['start'], 16), procedure['size'])).hexdigest() != procedure['sha256']:
            raise ValueError('Changed facility procedure: ' + procedure['start'])
    schema = json.loads(Path(__file__).with_name('original_game_structure.json').read_text())
    mapping.pcode.OPS.update({k: tuple(v) for k, v in schema['opcode_schema'].items()})
    facilities = evidence['facilities']
    if [f['kind'] for f in facilities] != ['School', 'HotSpring', 'Recycling', 'TechnologySchool',
                                         'Casino', 'Lawyer', 'MysteryLab', 'ProductionLab', 'HardwareLab']:
        raise ValueError('Incomplete facility set')
    for facility in facilities:
        obj = objects[facility['form']]
        load = next(e['procedure'] for c in obj['controls'] if c['name'] == 'Form'
                    for e in c['events'] if e['slot'] == 6)
        if load['start'] != facility['load']:
            raise ValueError('Changed facility load event')
        _, rows, _ = mapping.decode(pe, obj, load)
        # Every facility loads the shared MDI ImageFormG getter (032C) using a
        # literal array index. Validate the binding, not just a resource's size.
        bindings = []
        for index, row in enumerate(rows):
            if row.mnemonic == 'VCallAd' and row.raw == bytes.fromhex('0f 2c 03'):
                window = rows[max(0, index - 4):index]
                bindings.extend(i.raw[1] for i in window if i.mnemonic == 'LitI2_Byte')
        if bindings != [facility['image_form_g_index']]:
            raise ValueError('Changed illustration binding: ' + facility['kind'])
        for button in facility['buttons']:
            if not any(c['name'] == button['name'] and any(
                    e['slot'] == 0 and e['procedure']['start'] == button['click']
                    for e in c['events']) for c in obj['controls']):
                raise ValueError('Changed facility button event')
        resource = facility['resource']
        offset, size = resource['file_offset'], resource['size']
        raw = data[offset:offset + size]
        if len(raw) != size or hashlib.sha256(raw).hexdigest() != resource['sha256']:
            raise ValueError('Changed facility illustration')
        dimensions = struct.unpack_from('<HH', raw, 6) if resource['format'] == 'gif' else struct.unpack_from('<ii', raw, 18)
        if dimensions != (256, 160):
            raise ValueError('Changed facility illustration dimensions')
    print(f"PASS {len(recorded)} procedure fingerprints; nine load/button bindings and 256x160 illustrations")


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('executable', type=Path)
    verify(parser.parse_args().executable)
