#!/usr/bin/env python3
"""Bound original designer properties without exporting captions, fonts or pictures."""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path
import struct

ROOT = Path(__file__).resolve().parents[1]
SOURCE_SHA256 = '4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6'
CLASSES = {0:'Pix',1:'Label',2:'Edit',3:'Frame',4:'Push',5:'Check',6:'Opt',7:'Combo',8:'LBox',9:'HScroll',10:'Scroll',11:'Timer',13:'Form',24:'Img'}


def parse(data, start, end, kind, schemas):
    if not 0 <= start < end <= len(data):
        raise ValueError('Invalid property range')
    cursor, fields, opaque = start, [], []
    def take(count):
        nonlocal cursor
        if count < 0 or cursor + count > end:
            raise ValueError('Property crosses its owning record')
        value = data[cursor:cursor+count];cursor += count
        return value
    while cursor < end:
        at = cursor;index = take(1)[0]
        if index == 255:
            return dict(end=cursor, fields=fields, opaque=opaque)
        if index in (98,99):
            if take(1) != b'\0':
                raise ValueError('Unsupported reserved-property marker')
            opaque.append(dict(offset=at,index=index,bytes=1,meaning='unresolved'))
            continue
        table = schemas.get(kind, [])
        if index >= len(table):
            raise ValueError(f'Unknown {kind} property index {index}')
        descriptor = table[index];name = descriptor['name'];value_type = descriptor['type']
        field = dict(offset=at,index=index,name=name,type=value_type)
        if value_type in (1,13,18):
            size = struct.unpack('<H',take(2))[0];payload=take(size)
            if cursor < end and data[cursor] == 0:take(1)
            field.update(payload_bytes=size,payload_sha256=hashlib.sha256(payload).hexdigest())
        elif value_type in (2,17) or name == 'ScaleMode':
            field['value'] = struct.unpack('<h',take(2))[0]
        elif value_type in (3,5):
            field['value'] = struct.unpack('<I',take(4))[0]
        elif value_type in (4,6):
            field['value'] = take(1)[0]
        elif kind == 'Timer' and value_type in (8,10):
            field['value'] = struct.unpack('<i',take(4))[0]
        elif value_type in (8,9,10,11):
            fmt='<iiii' if name == 'ClientLeft' else '<hhhh'
            field['rectangle_twips'] = list(struct.unpack(fmt,take(struct.calcsize(fmt))))
        elif value_type == 21:
            size=struct.unpack('<i',take(4))[0]
            if size == -1:field['empty']=True
            else:
                offset=cursor;payload=take(size)
                field.update(payload_offset=offset,payload_bytes=size,payload_sha256=hashlib.sha256(payload).hexdigest())
        elif value_type == 20:
            font=take(11)
            if font[0] != 1:raise ValueError('Unsupported StdFont version')
            name_bytes=take(font[10])
            field.update(charset=struct.unpack_from('<H',font,1)[0],flags=font[3],weight=struct.unpack_from('<H',font,4)[0],
                         size_10000=struct.unpack_from('<I',font,6)[0],font_name_sha256=hashlib.sha256(name_bytes).hexdigest())
        elif value_type == 7:
            field['value']=struct.unpack('<f',take(4))[0]
        elif value_type == 22:
            if take(1) != b'\0':raise ValueError('Unsupported persisted object payload')
            opaque.append(dict(offset=at,index=index,bytes=1,meaning='zero object marker; object semantics unresolved'))
        else:
            raise ValueError(f'Unsupported property type {value_type}')
        field['encoded_bytes']=cursor-at;fields.append(field)
    raise ValueError('Missing property terminator')


def inventory(executable,source=None):
    data=executable.read_bytes()
    if hashlib.sha256(data).hexdigest()!=SOURCE_SHA256:raise ValueError('Unsupported original executable')
    if source is None:source=json.loads((ROOT/'tools/original_game_structure.json').read_text())
    schemas=json.loads((ROOT/'tools/vb6_property_types.json').read_text())['classes']
    records=[];pictures=[]
    for instance in source['instances']:
        name=instance['control'].split('.')[-1];array=instance['array_index'] is not None
        name_start=instance['offset']+1+(7 if array else 5)
        marker=struct.pack('<H',len(name))+name.encode('ascii')+b'\0'
        if data[name_start:name_start+len(marker)]!=marker:raise ValueError('Changed control-name binding')
        class_at=name_start+len(marker)
        if CLASSES.get(data[class_at])!=instance['class']:raise ValueError('Changed intrinsic control class')
        end=instance['offset']+instance['size']
        parsed=parse(data,class_at+1,end,instance['class'],schemas)
        if parsed['end']!=end:raise ValueError('Unaccounted control bytes')
        records.append(dict(id=instance['id'],status='bounded-properties-parsed',**parsed))
    for form in source['layouts']:
        name=form['object'].split(':')[-1];class_at=form['offset']+3+len(name)
        # The MDI designer omits the ordinary 0D Form-class byte: its first
        # property is 53/ClientLeft, followed by the complete Int32 rectangle.
        implicit_mdi=name=='MDIForm1' and data[class_at]==53
        if data[class_at]!=13 and not implicit_mdi:
            records.append(dict(id=form['id'],status='unsupported-root-class',class_code=data[class_at]))
            continue
        parsed=parse(data,class_at if implicit_mdi else class_at+1,form['offset']+form['size'],'Form',schemas)
        records.append(dict(id=form['id'],status='bounded-properties-parsed',encoding='implicit-MDI-properties' if implicit_mdi else 'explicit-Form-class',**parsed))
    for record in records:
        for field in record.get('fields',[]):
            if field['type']==21 and not field.get('empty'):
                pictures.append(dict(owner=record['id'],property=field['name'],offset=field['payload_offset'],size=field['payload_bytes'],sha256=field['payload_sha256']))
    return dict(source_sha256=SOURCE_SHA256,counts=dict(records=len(records),controls=len(source['instances']),
                parsed_roots=sum(r['id'].startswith('layout:') and r['status']=='bounded-properties-parsed' for r in records),
                unsupported_roots=sum(r['status']!='bounded-properties-parsed' for r in records),pictures=len(pictures)),
                opaque_marker_counts=dict(Counter(str(o['index']) for r in records for o in r.get('opaque',[]))),
                records=records,pictures=pictures,
                limit='Bounded parsing does not establish semantics of reserved/object markers, unsupported classes outside the known inventory, or runtime assignments.')


def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('executable',type=Path);parser.add_argument('--write',type=Path)
    args=parser.parse_args();report=inventory(args.executable)
    if args.write:args.write.write_text(json.dumps(report,indent=2)+'\n')
    print('PASS serialized properties: '+json.dumps(report['counts'])+'; opaque meanings remain unresolved')


if __name__=='__main__':main()
