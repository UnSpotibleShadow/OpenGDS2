#!/usr/bin/env python3
"""Verify bounded original burst-flame anchors; no pixels or raw traces emitted."""
import argparse
import hashlib
from pathlib import Path
from inspect_vb6 import PeImage, ParseError
from trace_hiring_pcode import KNOWN_SHA256


def verify(data):
    if hashlib.sha256(data).hexdigest()!=KNOWN_SHA256:
        raise ParseError('Unsupported original executable')
    pe=PeImage(data)
    def require(start,offset,raw):
        b=bytes.fromhex(raw)
        if pe.bytes_at(start+offset,len(b))!=b:
            raise ParseError(f'Burst flame evidence changed at {start+offset:08X}')
    tick=0xA0283C
    require(tick,0x2E98,'6b 48 ff e7 05 18 00 fc 96 28 6c ff 03 00 5d fb 2f 2c ff')
    require(tick,0x2EAB,'6b 48 ff e7 05 15 00 fc 96 28 fc fe 06 00 5d fb 56 0c ff fb 27 ec fe ff 1b 1c 3e 32')
    rows=[(200,102,359),(232,118,359),(151,105,384),(210,147,359),(125,134,384),(157,150,384),(296,150,359),(328,166,359),(247,153,384),(306,195,359),(221,182,384),(253,198,384)]
    for i,(x,y,sy) in enumerate(rows):
        off=0x2EE2+i*0x2C
        for delta,value,local in [(0,x,'6c fc'),(9,y,'5c fc'),(18,sy,'3c fc')]:
            require(tick,off+delta,'28 6c ff '+value.to_bytes(2,'little').hex(' ')+' fc f6 '+local)
    require(tick,0x312F,'f4 19')
    require(tick,0x3147,'f4 19')
    require(tick,0x315F,'04 5c fc 28 6c ff 26 00 fb 94 2c ff')
    require(tick,0x3185,'6b 48 ff e7 05 15 00 fc 96 fc 22 05 6b 00 fc 96 28 6c ff 03 00 05 31 00 fb b4 2c ff fb 33')
    require(tick,0x31A6,'f3 3f 01')
    require(tick,0x31C2,'6b 48 ff e7 05 15 00 fc 96 fc 22 05 6b 00 fc 96 28 6c ff 06 00 05 31 00 fb b4 2c ff fb 5a')
    require(tick,0x31E3,'f3 5a 01')
    require(tick,0x31FC,'28 6c ff 00 00 6b 48 ff e7 05 15 00 fc 96 fc 22 05 6b 00 52')
    require(tick,0x3210,'6b 48 ff e7 05 15 00 fc 96 fc 22 05 6b 00 fc 96 28 6c ff 01 00 fb 94 2c ff')
    require(tick,0x3252,'f4 9c') # hide the tile at X=-100 for non-burst work.
    # Final initializer allocations override earlier temporary seat mappings.
    for i in range(12):
        require(0x9F651C,0x398B+i*0x54,'07 08 00 88 23 f5 '+(i+6).to_bytes(4,'little').hex(' ')+' 07 08 00 6c 22 fc b0')
    for index,value in [(0x15,0xA13770),(0x18,0xA138A4),(0x22,0xA13288),(0x6B,0xA132A4),(0x31,0xA149B8)]:
        if pe.u32(0x5F99AC+index*4)!=value:raise ParseError('Burst flame pool mapping changed')
    return True

if __name__=='__main__':
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('exe',type=Path)
    verify(p.parse_args().exe.read_bytes())
    print('PASS: burst guard, twelve desk mappings, 25-pixel frames, depth, phase and initializer slots.')
