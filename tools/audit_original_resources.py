#!/usr/bin/env python3
"""Compare every bounded original raster property with its private imported file."""
import argparse
from collections import Counter
import hashlib
import io
import json
from pathlib import Path
import struct
from verify_serialized_properties import inventory
from inventory_pe_resources import inventory as pe_resources


def audit(executable,cache):
    source=inventory(executable);data=executable.read_bytes()
    rows=json.loads((cache/'Graphic/Recovered/index.json').read_text())
    recovered={r['Offset']:r for r in rows};checked=[];other=[]
    for picture in source['pictures']:
        raw=data[picture['offset']+8:picture['offset']+picture['size']]
        if raw.startswith(b'BM') or raw.startswith(b'GIF8'):
            row=recovered.get(picture['offset']+8)
            if row is None:raise ValueError('Missing imported picture property')
            path=cache/'Graphic/Recovered'/row['File'];actual=path.read_bytes()
            if path.suffix.lower()=='.png':
                from PIL import Image
                expected=Image.open(io.BytesIO(raw)).convert('RGBA');rendered=Image.open(io.BytesIO(actual)).convert('RGBA')
                if expected.size!=rendered.size or expected.tobytes()!=rendered.tobytes():
                    raise ValueError('Converted raster pixels differ: '+row['File'])
            elif actual!=raw:
                raise ValueError('Imported raster bytes differ: '+row['File'])
            checked.append(dict(owner=picture['owner'],property=picture['property'],source_offset=picture['offset'],
                                private_file='Graphic/Recovered/'+row['File'],file_sha256=hashlib.sha256(actual).hexdigest()))
        else:
            form=picture['owner'].removeprefix('layout:')
            named='Graphic/Embedded/WindowIcon-'+form+'.png' if picture['owner'].startswith('layout:') and picture['property']=='Icon' else None
            other.append(dict(owner=picture['owner'],property=picture['property'],source_offset=picture['offset'],
                              classification='ICO' if raw.startswith(b'\0\0\1\0') else 'CUR' if raw.startswith(b'\0\0\2\0') else 'unknown',
                              named_icon=named if named and (cache/named).is_file() else None))
    if len(checked)!=len(rows):raise ValueError('Recovered raster inventory has unaccounted entries')
    sounds=[]
    for path in sorted((cache/'Sound').iterdir()):
        if path.suffix.lower()=='.wav':sounds.append(dict(file='Sound/'+path.name,bytes=path.stat().st_size,sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
    return dict(source_sha256=source['source_sha256'],raster_properties_verified=len(checked),
                remaining_picture_formats=dict(Counter(r['classification'] for r in other)),
                root_icon_exports=sum(r['named_icon'] is not None for r in other),raster_properties=checked,other_pictures=other,audio=sounds,pe_resources=pe_resources(executable),
                limits=['Imported pixels/files do not establish all dynamic image assignments or every visible use.',
                        'Unexported control ICO/CUR properties remain explicit; root-icon presence is not runtime pixel acceptance.',
                        'Open.wav has no established trigger; Bakuhatu.wav has a native dispatcher branch but no recovered caller.'])


def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('executable',type=Path);parser.add_argument('cache',type=Path);parser.add_argument('--write',type=Path)
    args=parser.parse_args();result=audit(args.executable,args.cache)
    if args.write:args.write.write_text(json.dumps(result,indent=2)+'\n')
    print('PASS original resources: '+str(result['raster_properties_verified'])+' raster properties, '+str(result['root_icon_exports'])+' root icons, '+str(len(result['audio']))+' audio files; runtime use remains separate')


if __name__=='__main__':main()
