#!/usr/bin/env python3
"""Resolve native random draw/seed import callsites without executing the original."""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path
import struct
from inspect_vb6 import PeImage

ROOT=Path(__file__).resolve().parents[1]


def build(executable):
    inventory=json.loads((ROOT/'docs/original-game-inventory.json').read_text());data=executable.read_bytes()
    if hashlib.sha256(data).hexdigest()!=inventory['source_sha256']:raise ValueError('Unsupported original executable')
    pe=PeImage(data);optional=struct.unpack_from('<I',data,0x3c)[0]+24
    import_rva=struct.unpack_from('<I',data,optional+104)[0];imports={}
    def string(address):
        chars=[]
        for index in range(256):
            value=pe.bytes_at(address+index,1)[0]
            if value==0:return bytes(chars).decode('ascii')
            chars.append(value)
        raise ValueError('Unbounded import name')
    for index in range(128):
        original,_,_,name,first=struct.unpack('<5I',pe.bytes_at(pe.image_base+import_rva+index*20,20))
        if name==0:break
        library=string(pe.image_base+name)
        for slot in range(2048):
            thunk=pe.u32(pe.image_base+(original or first)+slot*4)
            if thunk==0:break
            if thunk&0x80000000:
                # MSVBVM60's named export table identifies these stable ordinals.
                if library.lower()=='msvbvm60.dll':
                    imports[pe.image_base+first+slot*4]={593:'rtcRandomNext',594:'rtcRandomize'}.get(thunk&0xffff,'unclassified ordinal')
                continue
            imports[pe.image_base+first+slot*4]=string(pe.image_base+thunk+2)
    sites=[]
    for reference in inventory['references']:
        if reference['kind']!='pool-address; role-unclassified' or not reference['opcode'].startswith('ImpAdCall'):continue
        address=int(reference['value'],16)
        try:raw=pe.bytes_at(address,6)
        except ValueError:continue
        if raw[:2]!=b'\xff\x25':continue
        function=imports.get(struct.unpack_from('<I',raw,2)[0],'')
        if function in ('rtcRandomNext','rtcRandomize'):
            sites.append(dict(procedure=reference['procedure'],at=reference['at'],import_name=function,
                              role='draw' if function=='rtcRandomNext' else 'seed'))
    if not any(r['role']=='draw' for r in sites):raise ValueError('No established native random draw imports resolved')
    return dict(source_sha256=inventory['source_sha256'],import_ordinals={'MSVBVM60.dll:593':'rtcRandomNext','MSVBVM60.dll:594':'rtcRandomize'},counts=dict(Counter(r['role'] for r in sites)),sites=sites,
                limits=['Static sites do not establish reachability, executed branch counts, dynamic calls or whole-session draw order.',
                        'OpenGDS2 saves its shared 24-bit state. Native seed initialization and paired Windows-session equivalence remain unverified.'])


def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('executable',type=Path);parser.add_argument('--write',type=Path)
    args=parser.parse_args();report=build(args.executable)
    if args.write:args.write.write_text(json.dumps(report,indent=2)+'\n')
    print('PASS original RNG callsites: '+json.dumps(report['counts'])+'; executed draw order pending')


if __name__=='__main__':main()
