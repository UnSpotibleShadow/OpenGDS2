#!/usr/bin/env python3
"""Check bounded development evidence without dumping original text or procedures."""
import argparse
import hashlib
from pathlib import Path
from inspect_vb6 import PeImage, ParseError
from recover_employee_database import recover
from trace_hiring_pcode import KNOWN_SHA256

def verify(data):
    if hashlib.sha256(data).hexdigest()!=KNOWN_SHA256:
        raise ParseError('Unsupported original executable')
    pe=PeImage(data)
    def require(start,offset,raw):
        expected=bytes.fromhex(raw)
        if pe.bytes_at(start+offset,len(expected))!=expected:
            raise ParseError(f'Development evidence changed at {start+offset:08X}')
    # Seven-profile draw, first four-field proposal.
    require(0x9EFA40,0x00BB,'27 5c ff 0a 27 00 04 00 f4 07 eb b3 fb e6')
    require(0x9EFA40,0x00E2,'28 6c ff 28 00 f5 00 00 00 00 04 44 ff 52')
    # Accuracy source+99, not current employee accuracy.
    require(0xA0283C,0x6427,'28 bc fe 58 00 6b 48 ff e7 05 6c 00 fc 96 28 fc fe 63 00 fb 94 0c ff')
    require(0xA0283C,0x6440,'05 99 00 fc 96 28 cc fe 02 00 fb bc ec fe')
    # Reset empties source-array cells 0..256 inclusive; initializer fills sixty.
    require(0x9DD234,0x076C,'28 3c ff 00 00 04 5c ff 28 4c ff 00 01 fe 68 bc fd 21 09')
    require(0x9DD234,0x07CA,'fc 67 6c ff 04 5c ff fc 22 07 08 00 50 0b 52')
    if len(recover(data)['Employees'])!=60:
        raise ParseError('Initial employee coverage changed')
    decl=0x61A0C8; array=decl+16
    if (pe.u16(decl),pe.u16(array),pe.u32(array+4),pe.u32(array+16),pe.u32(array+20))!=(0xB50,1,16,513,0):
        raise ParseError('Accuracy SAFEARRAY declaration changed')
    if not pe.u16(array+2)&0x800:
        raise ParseError('Accuracy cells are not VARIANTs')
    # Type points read a second roster-map index; fallback permits duplicates.
    require(0x9ACF14,0x0778,'05 20 00 fc 22 05 28 00 fc 96 fc 22 05 29 00 fc 96 fb 94')
    require(0x9D2F68,0x0551,'04 f4 fe fb 2f 58 ff fb c4 38 ff ff 1b 1c 71 05 28 48 ff 01 00 fc f6 c4 fd')
    require(0x9B00D8,0x0774,'05 07 00 fc 22 05 27 00 fc 96 05 20 00 fc 22 05 28 00 fc 96 fb 94')
    # Helper stores data-size member 1990; the formatting helper emits a size unit.
    require(0x99791C,0x03EB,'04 2c ff fb 94 3c ff fd 16 10 00 1c ff fc 22 07 08 00 90 19 52')
    require(0x9855D4,0x009C,'fd 16 14 00 5c ff fb e9 3c ff 3a 2c ff 53 00 fb ef 1c ff')
    require(0x99037C,0x01F4,'04 4c fe 05 01 00 28 5c ff 4a 00 fb b4 6c ff fb 94 3c ff 55 05 0d 00 fc 22 05 10 00 a2')
    return True

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('exe',type=Path)
    verify(parser.parse_args().exe.read_bytes())
    print('PASS: planning, work, reset cells, selections, size and computer rendering anchors.')
