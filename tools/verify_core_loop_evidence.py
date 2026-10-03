#!/usr/bin/env python3
"""Bounded structural checks for own-game development, release and sales."""
import argparse,hashlib
from pathlib import Path
from inspect_vb6 import PeImage,ParseError
from trace_hiring_pcode import KNOWN_SHA256
from recover_genre_catalog import recover

def verify(data):
    if hashlib.sha256(data).hexdigest()!=KNOWN_SHA256:raise ParseError('Unsupported original executable')
    pe=PeImage(data)
    def require(start,offset,raw):
        expected=bytes.fromhex(raw)
        if pe.bytes_at(start+offset,len(expected))!=expected:raise ParseError(f'Core-loop evidence changed at {start+offset:08X}')
    # Shared type map: 65 VARIANT cells, lower bound zero, fresh allocation.
    decl=0x61D580;array=decl+16
    if (pe.u16(decl),pe.u16(array),pe.u32(array+4),pe.u32(array+12),pe.u32(array+16),pe.u32(array+20))!=(0x31A8,1,16,0,65,0):
        raise ParseError('Type roster declaration changed')
    if not pe.u16(array+2)&0x800:raise ParseError('Type map is not VARIANT cells')
    # Company production capacity 50, genre eligibility cell 23 and two 1500 fees.
    require(0x9F651C,0x00F5,'28 6c ff 32 00 fd e6 08 00 70 00')
    require(0x9F651C,0x0100,'28 6c ff 17 00 fd e6 08 00 90 00')
    require(0x9F651C,0x01A1,'28 6c ff dc 05 fd e6 08 00 60 08 28 6c ff dc 05 fd e6 08 00 70 08')
    # Work experience always adds 3.7; animation accumulation uses a distinct cell.
    require(0xA0283C,0x6622,'fe c4 6c ff 9a 99 99 99 99 99 0d 40 fb 94 2c ff')
    # Twenty-tick sales counter (10 multiplied by original timing factor 2).
    require(0x9E3830,0x0958,'05 3d 01 28 68 ff 0a 00 05 31 00 fb b4 58 ff fb 5a')
    # Revenue subtracts both per-copy fees before multiplication.
    require(0x9E3830,0x10D7,'05 57 01 fb 9c 58 ff 05 58 01 fb 9c 34 ff')
    # Production reads the hardware/band table, not hardware planning cost.
    require(0x9B4344,0x0814,'05 92 00 a7 02 00 06 00 00 05 8d 00 fb 94 58 ff fd b6 06 00')
    # Reviews: four-stat sum, strict first threshold, replacement draw at factor >=2.
    require(0x9CAFCC,0x070D,'04 c8 fd 28 4c ff 82 00')
    require(0x9CAFCC,0x0899,'05 1f 00 28 4c ff 01 00')
    # Hall-of-Fame company counter starts at zero and increments once per ceremony.
    require(0x9F651C,0x0066,'28 6c ff 00 00 fd e6 08 00 00 01')
    require(0x986780,0x0124,'05 09 00 28 58 ff 01 00 fb 94 48 ff fd b6 09 00')
    # Dismissal clears the source employed gate/stress, not progression fields.
    require(0x9981CC,0x03AF,'07 08 00 84 0c 52')
    require(0x9981CC,0x03CB,'07 08 00 68 0c 52')
    # Monthly payroll's index is shared scratch 3018, not the loop variable.
    require(0x9DB4F4,0x06F6,'07 08 00 18 30 fc 22 07 08 00 dc 0b fc 96 fb 94 4c ff')
    # Investigation delay, four-way outcome and zero-stock refund branch.
    require(0x9EFA40,0x38A0,'28 6c ff 05 00 05 eb 00 fc 22 05 ec 00 52')
    require(0x9EFA40,0x3A58,'f4 04 eb b3 fb e6')
    require(0x9EFA40,0x3D15,'28 6c ff 00 00 05 eb 00 fc 22 05 fc 00 52')
    catalog=recover(data)
    if not all(len(x['ProductionBands'])==4 for x in catalog['Hardware']):raise ParseError('Incomplete production bands')
    return True

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('exe',type=Path)
    verify(parser.parse_args().exe.read_bytes());print('PASS: core-loop declarations, initial cells, sales cadence/fees and production bands.')
