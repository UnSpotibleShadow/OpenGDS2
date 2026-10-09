#!/usr/bin/env python3
"""Verify facility transactions in the locally supplied original; no game text emitted."""
import argparse
import hashlib
import struct
from pathlib import Path
from inspect_vb6 import PeImage
from trace_hiring_pcode import KNOWN_SHA256


def verify(path):
    data=path.read_bytes()
    if hashlib.sha256(data).hexdigest()!=KNOWN_SHA256:
        raise ValueError('Unsupported original executable')
    pe=PeImage(data)
    handlers=[
        (0x00988f78,352,'de3aaf4a6abee9741c3ef359483743ef68b1b6eb0496f204a1c17740cdeabc42'),
        (0x0099e164,1388,'8aaa1c8ebbb094220624bdaed7e4d9d92d9a7a2f675cbe08219a460103b6ddf1'),
        (0x0098ddc4,496,'16a8089c259870eacb2dc4b7b02a67f53074681bb9c4cda1020f67334ced5fdf'),
        (0x009881c0,328,'0d655bde2e56208df0574cd335c081b7aa6adf5ff24c791ccdb560f0fe89c94b'),
        (0x00988dac,352,'cae0b49d0263db792a1b7345a92d08c8ecef4213fc3b31e6f0de50c94f3330e0'),
        (0x0097d918,84,'1e1a94c1a87f85fd60d88d7bbb14f5ec367ac5a1398ef400497a964cfe99ca76'),
        (0x00984480,208,'d3ec2cae15a00510e510d6f5717338d8f685d09542756e496a90e89a48ee4dc4'),
        (0x00983d78,208,'c2b9c1bd67be96ab9762c83d78eb9d7317e4a84f049f8afabd2dd6590f4705c3'),
        (0x00983fd0,208,'ca9925d110b3ec624a34a57c0897eba6a4bee854f6f00a74d5fca2f40373f971'),
        (0x00983c4c,208,'1ec35163c2168dd587b9fd679bb8880cb205b5c588d4dd6a557bf2b1ac5f4cf0'),
        (0x00983ea4,208,'d3ec2cae15a00510e510d6f5717338d8f685d09542756e496a90e89a48ee4dc4'),
        (0x009839f4,208,'b6f447e136452d3606e45a1818ecfcc0167fe47f0b76309a7128123d5ebbd006'),
        (0x009840fc,208,'b9ce9a60135c5b72df23e137b4440afcf79d48ea9687383e94c8ebb4c4d05ea0'),
        (0x00984354,208,'332ea03ea7234bd8cbc39647ef172416b6254e7e51a70127ae199709f425d7ea'),
        (0x00984228,208,'f85b9e5d6b3575ac32c76442a8da8e949446fea8d6f2cf065fda9286c360dac6'),
    ]
    for start,size,digest in handlers:
        if hashlib.sha256(pe.bytes_at(start,size)).hexdigest()!=digest:
            raise ValueError(f'Changed facility handler {start:08X}')
    for offset,price in zip([0x9963,0x9999,0x99cf,0x9a05,0x9a3b,0x9a71,0x9aa7,0x9add,0x9b13],
        [600_000_000,60_000_000,80_000_000,100_000_000,500_000_000,250_000_000,300_000_000,150_000_000,1_000_000_000]):
        if pe.bytes_at(0x9f651c+offset,4)!=bytes.fromhex('fe c1 bc fe') or pe.bytes_at(0x9f651c+offset+4,4)!=struct.pack('<i',price):
            raise ValueError('Changed facility price')
    for offset,member,price in [(0x150,0x4cc,8_000_000),(0x15e,0x4dc,40_000_000),(0x16c,0x4ec,15_000_000),(0x17a,0x4fc,10_000_000),(0x188,0x50c,25_000_000)]:
        raw=bytes.fromhex('fe c1 6c ff')+struct.pack('<i',price)+bytes.fromhex('fd e6 08 00')+struct.pack('<H',member)
        if pe.bytes_at(0x9f651c+offset,len(raw))!=raw:raise ValueError('Changed service fee')
    for pool,index,address in [(0x5f4a34,1,0xa114d0),(0x5f4a34,6,0xa11460),(0x5f1a68,6,0xa12e78),(0x5f319c,7,0xa11d58),(0x5f4400,0x10,0xa1108c)]:
        if pe.u32(pool+4*index)!=address:raise ValueError('Changed facility member mapping')
    for start,offset,raw in [
        (0x988f78,0x123,'05 02 00 05 00 00 fc 22 05 01 00 fc 96 fb 9c 6c ff fd b6 02 00'),
        (0x9881c0,0x80,'05 06 00 fc 22 05 07 00 fc 96 28 5c ff 01 00 fb 94 6c ff'),
        (0x988dac,0x93,'27 6c ff 0a 0e 00 04 00 73 38 fe  f4 0f eb 6e 38 fe f4 05 eb b3 fb e6'),
        (0x988dac,0x13c,'05 13 00 05 05 00 fb 9c 6c ff fd b6 13 00'),
        (0x98ddc4,0x135,'05 07 00 04 6c ff 28 5c ff f4 01 fb b4 3c ff'),
    ]:
        expected=bytes.fromhex(raw)
        if pe.bytes_at(start+offset,len(expected))!=expected:raise ValueError('Changed transaction anchor')
    print('PASS nine construction prices and closure handlers; hot spring, recycling, study capacity and production transactions')


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('executable',type=Path)
    verify(parser.parse_args().executable)
