#!/usr/bin/env python3
"""Verify the original window census; emit counts, never original text or pixels."""
import argparse
import hashlib
import json
import struct
from pathlib import Path
from inspect_vb6 import inspect, PeImage


def verify(executable):
    manifest = json.loads(Path(__file__).with_name('window_census_evidence.json').read_text())
    data = executable.read_bytes()
    if hashlib.sha256(data).hexdigest() != manifest['source_sha256']:
        raise ValueError('Unsupported original executable')
    report = inspect(executable)
    if len(report['objects']) != manifest['objects']:
        raise ValueError('Object count differs')
    forms = {o['name']: o for o in report['objects'] if o['kind'] == 'form_or_designer'}
    if set(forms) != {o['name'] for o in manifest['forms']}:
        raise ValueError('Form inventory differs')
    for row in manifest['forms']:
        form = forms[row['name']]
        actual = (form['ordinal'], len(form['controls']), form['procedure_count'],
                  sum(len(c['events']) for c in form['controls']))
        if actual != (row['ordinal'], row['controls'], row['procedures'], row['bound_events']):
            raise ValueError('Form structure differs: ' + row['name'])
        bindings = {(c['name'], c['index'], e['slot'], e['procedure']['start'])
                    for c in form['controls'] for e in c['events'] if e.get('procedure')}
        for event in row.get('event_bindings', []):
            if (event['control'], event['index'], event['slot'], event['start']) not in bindings:
                raise ValueError('Event binding differs: ' + row['name'])
    pe = PeImage(data)
    for procedure in manifest['fingerprints']:
        raw = pe.bytes_at(int(procedure['start'], 16), procedure['size'])
        if hashlib.sha256(raw).hexdigest() != procedure['sha256']:
            raise ValueError('Procedure fingerprint differs')
    for resource in manifest['resources']:
        offset = int(resource['offset'], 16)
        raw = data[offset:offset + resource['size']]
        if raw[:2] != b'BM' or struct.unpack_from('<ii', raw, 18) != (resource['width'], resource['height']):
            raise ValueError('Resource shape differs')
        if hashlib.sha256(raw).hexdigest() != resource['sha256']:
            raise ValueError('Resource fingerprint differs')
    print(f"PASS window census: {len(forms)} forms, "
          f"{sum(r['controls'] for r in manifest['forms'])} controls, "
          f"{sum(r['bound_events'] for r in manifest['forms'])} bound events, "
          f"{len(manifest['fingerprints'])} selected fingerprints, "
          f"{len(manifest['resources'])} resources")


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('executable', type=Path)
    verify(parser.parse_args().executable)
