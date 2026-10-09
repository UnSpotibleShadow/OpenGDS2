#!/usr/bin/env python3
"""Verify local supported-original anchors for M71; never execute or dump the PE."""
import argparse
import hashlib
import struct
from pathlib import Path
from inspect_vb6 import PeImage
from trace_hiring_pcode import KNOWN_SHA256


def verify(path: Path) -> None:
    data=path.read_bytes()
    if hashlib.sha256(data).hexdigest()!=KNOWN_SHA256:
        raise ValueError('Unsupported original executable')
    pe=PeImage(data)
    for start,mode in [(0x987678,13),(0x987810,14)]:
        for offset,raw in [(0x93,'05 05 00 05 0e 00 fb 74 1c f6 00'),(0x113,'0a 10 00 08 00')]:
            expected=bytes.fromhex(raw)
            if pe.bytes_at(start+offset,len(expected))!=expected:raise ValueError('Changed consultation branch')
        if pe.bytes_at(start+0x10b,5)!=bytes.fromhex('28 6c ff')+struct.pack('<h',mode):raise ValueError('Changed meeting mode')
    pool=0x5f7dd8
    for cell,address in [(5,0xa11508),(0xe,0xa1103c),(0x10,0x60ed80)]:
        if pe.u32(pool+4*cell)!=address:raise ValueError('Changed entry mapping')
    for cell,address in [(0x107,0xa1188c),(0x116,0xa1187c),(0x26,0x60eea0)]:
        if pe.u32(0x5fafcc+4*cell)!=address:raise ValueError('Changed outcome mapping')
    regions=[
        (0x987678,312,'86249504a3360278ae339bcb10be59e118428708f18d7a3abeb7b4e17e41448a'),
        (0x987810,312,'21c53be0bfc977b0f1bfc4d58523bfe41364cf1fc1a4e33430ab103f00f0fe8c'),
        (0x97bdb8,37,'fc52e73366fce26fd32a8f05310f7a6a232115921a6329d1d144318b4c1ccd4e'),
        (0x9a6ddc,2077,'a80a4a5670160144416e977286a3dd07ecf3ed40e247b915265f7e30709aa68c'),
        (0x9f3bde,6431,'6c39cfbd43257818820a2e91ec7272c4d209f0f269d67894b6773a369bd4d660'),
        (0x981104,127,'bb314df78575a8dc3a31181d5ca32adbfd4f014d9771a5be30fa8a88acffeaee'),
        (0x9933dc,703,'d43aa7d353c06c428a8f244989df7a7f9dee0f48d2abd32d6a8b41076628b2aa'),
        (0x97b410,25,'89586269d2a6fac91db6064ef6224c4749715f503276dc9f59ddd3cf6e975d37')]
    for start,size,digest in regions:
        if hashlib.sha256(pe.bytes_at(start,size)).hexdigest()!=digest:raise ValueError('Changed consultation region')
    print('PASS consultation affordability, modes, dispatch/helpers and both fee outcomes')


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('executable',type=Path)
    verify(parser.parse_args().executable)
