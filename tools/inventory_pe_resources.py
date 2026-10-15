#!/usr/bin/env python3
"""Account for PE resource payloads separately from VB picture properties."""
import argparse
import hashlib
import json
from pathlib import Path
import struct
from inspect_vb6 import PeImage

SOURCE_SHA256='4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6'


def inventory(path):
    data=path.read_bytes()
    if hashlib.sha256(data).hexdigest()!=SOURCE_SHA256:raise ValueError('Unsupported original executable')
    pe=PeImage(data);optional=struct.unpack_from('<I',data,0x3c)[0]+24
    rva,size=struct.unpack_from('<II',data,optional+112);base=pe.image_base+rva;rows=[];visited=set()
    def bounded(offset,count):
        if offset<0 or count<0 or offset+count>size:raise ValueError('PE resource directory crosses its boundary')
        return pe.bytes_at(base+offset,count)
    def directory(offset,parents):
        if offset in visited or len(parents)>8:raise ValueError('Cyclic or excessive PE resource nesting')
        visited.add(offset);header=bounded(offset,16);named,ids=struct.unpack_from('<HH',header,12)
        if named+ids>4096:raise ValueError('Excessive resource count')
        for index in range(named+ids):
            name,target=struct.unpack('<II',bounded(offset+16+index*8,8))
            if name&0x80000000:
                location=name&0x7fffffff;length=struct.unpack('<H',bounded(location,2))[0]
                key={'name_sha256':hashlib.sha256(bounded(location+2,length*2)).hexdigest()}
            else:key={'id':name}
            chain=parents+[key]
            if target&0x80000000:directory(target&0x7fffffff,chain)
            else:
                payload_rva,payload_size,code_page,_=struct.unpack('<4I',bounded(target,16))
                payload=pe.bytes_at(pe.image_base+payload_rva,payload_size)
                rows.append(dict(path=chain,bytes=payload_size,code_page=code_page,sha256=hashlib.sha256(payload).hexdigest()))
    if size:directory(0,[])
    return dict(source_sha256=SOURCE_SHA256,resource_directory_bytes=size,payloads=rows,
                limit='PE resources are accounted independently of VB picture properties; identity/use and runtime rendering are separate evidence.')


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('executable',type=Path);parser.add_argument('--write',type=Path)
    args=parser.parse_args();result=inventory(args.executable)
    if args.write:args.write.write_text(json.dumps(result,indent=2)+'\n')
    print('PASS PE resource inventory: '+str(len(result['payloads']))+' payloads; no resource bytes exported')
