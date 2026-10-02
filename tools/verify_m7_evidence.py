#!/usr/bin/env python3
"""Verify concise M7 byte/data-flow anchors without dumping source code or assets."""
import argparse, struct
from pathlib import Path
from inspect_vb6 import PeImage, inspect, ParseError
from recover_employee_database import recover

def verify(data):
    database=recover(data); pe=PeImage(data)
    def require(va, expected):
        if pe.bytes_at(va,len(expected))!=expected:raise ParseError(f'Evidence changed at {va:08X}')
    start=0x9F651C
    require(start, bytes.fromhex('fe c1 6c ff 80 74 d2 1a fd e6 08 00 20 00'))
    require(start+0xE,bytes.fromhex('28 6c ff 01 00 fd e6 08 00 40 00 28 6c ff 04 00 fd e6 08 00 50 00 28 6c ff 01 00 fd e6 08 00 60 00'))
    require(0x9886C4+0x14C,bytes.fromhex('fe c1 68 ff 00 8c 86 47 fd b6 11 00'))
    # Designer indices directly preceding each serialized command name; M3 order was misleading.
    for off,index,caption in [(0x2307D3,1,'初心者モード'),(0x23081F,0,'通常モード')]:
        if struct.unpack_from('<H',data,off-2)[0]!=index or data[off+13:off+15]!=len(caption.encode('cp932')).to_bytes(2,'little'):
            raise ParseError('Mode designer layout changed')
    require(0x9DB4F4+0x1AE,bytes.fromhex('07 08 00 60 00 28 6c ff 01 00 fb 94 4c ff fd e6 08 00 60 00'))
    require(0x9DB4F4+0x67D,bytes.fromhex('07 08 00 60 00 28 6c ff 05 00 5d fb 5a'))
    require(0x9DB4F4+0x715,bytes.fromhex('07 08 00 50 00 28 6c ff 0d 00 5d fb 33'))
    # Opening computer is Empty/zero; opening hardware license source 0 is granted.
    require(0x9DD234+0x6A6,bytes.fromhex('fc 67 6c ff fd e6 08 00 98 09'))
    require(start+0x6321,bytes.fromhex('28 6c ff 01 00 f5 00 00 00 00 07 08 00 8c 0f 52'))
    # Development computer purchase checks downgrade, charges the selected price,
    # then updates the current index. Planning checks target bits against computer bits.
    require(0x99037C+0x65,bytes.fromhex('05 00 00 fc 22 05 06 00 fc 96 05 01 00 fc 22 05 06 00 fc 96 fb 67'))
    require(0x99037C+0x158,bytes.fromhex('05 00 00 fd c0 01 00'))
    require(0x988A34+0x20,bytes.fromhex('05 04 00 fc 22 05 17 00 fc 96 05 18 00 fc 22 05 19 00 fc 96 fb 74'))
    # License confirmation charges full fee and marks source licensed.
    require(0x9EFA40+0x1B75,bytes.fromhex('05 1c 00 fc 22 05 7f 00 fc 96 fb cc 5c ff fc f6 00 ff'))
    require(0x9EFA40+0x1C17,bytes.fromhex('28 6c ff 01 00 05 1c 00 fc 22 05 8a 00 52'))
    # March / temporary week 5 contract renewal occurs before the month rollover.
    require(0x9DB4F4+0x4CB,bytes.fromhex('07 08 00 50 00 28 6c ff 03 00 5d fb 2f 4c ff 07 08 00 60 00 28 2c ff 05 00 5d fb 2f bc fe fb 27 ac fe ff 1b'))
    require(0x9DB4F4+0x55C,bytes.fromhex('05 92 00 24 93 00 0d b0 02 94 00'))
    # Funds guard before any unchecked-source dismissal; one financial call afterward.
    require(0x995CDC+0x79,bytes.fromhex('05 09 00 04 6c ff fb 67 1c d6 00'))
    require(0x995CDC+0x33E,bytes.fromhex('04 6c ff fb cc 3c ff fc f6 e0 fe 04 e0 fe 04 d0 fe 0a 18 00 08 00'))
    # Dismissal clears source gate and stress, leaving progression arrays untouched.
    require(0x9981CC+0x399,bytes.fromhex('28 4c ff 00 00 fd 16 10 00 5c ff fc 22 07 08 00 1c 0e fc 96 fc 22 07 08 00 84 0c 52'))
    require(0x9981CC+0x3B5,bytes.fromhex('28 4c ff 00 00 fd 16 10 00 5c ff fc 22 07 08 00 1c 0e fc 96 fc 22 07 08 00 68 0c 52'))
    # Caption indices and matching value-label indices share Y coordinates.
    # These correlate with fGMLevelUP timer writes to value labels 7,8,9,10,11,15,16.
    pairs=[(0x241D20,0x241CE2,0,7),(0x241E3C,0x241E83,4,8),
           (0x241D6A,0x241EC1,1,9),(0x241DF6,0x241EFF,3,10),
           (0x241DB0,0x241F3D,2,11),(0x241FC5,0x24200D,5,15),
           (0x241F7B,0x24204B,6,16)]
    for label,value,li,vi in pairs:
        if struct.unpack_from('<H',data,label-2)[0]!=li or struct.unpack_from('<H',data,value-2)[0]!=vi:raise ParseError('Stat label indices changed')
        # Geometry record follows the array-index property 00 02 (captioned label)
        # or 02 (blank value label), then 05 and four 16-bit coordinates.
        n=struct.unpack_from('<H',data,label+11)[0]
        geometry=label+13+n+4
        if data[geometry]!=5 or data[value+13]!=5 or data[geometry+3:geometry+5]!=data[value+16:value+18]:
            raise ParseError('Stat label coordinate correspondence changed')
    require(0x9B375C+0x341,bytes.fromhex('27 5c ff 0a 17 00 04 00 fd 6a 4c ff'))
    require(0x9B375C+0x38C,bytes.fromhex('28 6c ff 63 00 5d'))
    # Experience threshold helper and all ten initializer entries.
    require(0x97B370,bytes.fromhex('fd 16 14 00 4c ff fd 16 10 00 5c ff fc 22 07 08 00 e0 02 fc b0'))
    for level,threshold in enumerate([100,150,200,250,300,350,400,500,600,777],1):
        require(start+0x204+(level-1)*0x20,
            b'\x28\xbc\xfe'+struct.pack('<h',threshold)+b'\x28\xcc\xfe'+struct.pack('<h',level)+bytes.fromhex('04 ac fe 0a bc 00 0c 00'))
    # Manual level-up cap, reset, level increment and 1.5 fee multiplier.
    require(0x9A3E34+0x19,bytes.fromhex('28 68 ff 0a 00 5d fb 5a'))
    require(0x9A3E34+0xED,bytes.fromhex('28 68 ff 00 00 05 00 00 fc 22 05 05 00 52'))
    require(0x9A3E34+0x213,bytes.fromhex('05 00 00 fc 22 05 0e 00 fc 96 fe c4 68 ff 00 00 00 00 00 00 f8 3f'))
    # Rank reset to Empty/zero; recognition initializer member 80 = 12.
    require(0x9DD234+0x64,bytes.fromhex('fc 67 6c ff fd e6 08 00 c0 00'))
    require(start+0xEA,bytes.fromhex('28 6c ff 0c 00 fd e6 08 00 80 00'))
    require(start+0x150,bytes.fromhex('fe c1 6c ff 00 12 7a 00 fd e6 08 00 cc 04'))
    require(0x9E3830+0x42,bytes.fromhex('28 44 ff 55 00 5d'))
    require(0x99E164+0x25B,bytes.fromhex('28 5c ff 00 00 04 2c fe fc 22 05 15 00 52'))
    # Information form maps those globals to company recognition/rank labels.
    info_pool=0x61190C
    if pe.u32(info_pool+0x109*4)!=0xA1109C or pe.u32(info_pool+0x110*4)!=0xA110DC:
        raise ParseError('Opening company field map changed')
    for caption,value in [(0x29B61A,0x29B5D6),(0x29B2FA,0x29B2B2)]:
        def y(address):
            n=struct.unpack_from('<H',data,address+9)[0]
            geometry=address+11+n+4
            if data[geometry]!=5:raise ParseError('Company label geometry changed')
            return data[geometry+3:geometry+5]
        if y(caption)!=y(value):raise ParseError('Company caption/value correspondence changed')
    return database
if __name__=='__main__':
    ap=argparse.ArgumentParser(description=__doc__);ap.add_argument('exe',type=Path);a=ap.parse_args();d=verify(a.exe.read_bytes())
    print('PASS: employee database, mode/funds/date, calendar, stat labels, level-up thresholds/reset/fee, opening recognition/rank.')
