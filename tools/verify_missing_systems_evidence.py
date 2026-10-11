#!/usr/bin/env python3
"""Verify bounded bankruptcy and mystery-lab evidence in the supported original.

Never executes the original or emits its strings/assets/procedure traces.
This verifies source anchors; it is not an OpenGDS2 gameplay test.
"""
import argparse
import hashlib
import struct
from pathlib import Path
from inspect_vb6 import PeImage, ParseError
from trace_hiring_pcode import KNOWN_SHA256

SPANS = {'work': (10496060, 56604), 'periodic': (10369072, 10216), 'meeting': (10418752, 27016), 'mystery_start': (9993024, 316)}
FINGERPRINTS = {'work': '2a02c38bb853e37b5e616f4251350a25d12e0d27d19406098f5037b29a678ff4', 'periodic': '8ed78499bf69505d290087f05309d948c2bf63b1259a144df437f23201760243', 'meeting': '873bf218fd41a10dfbac8ff7e394f68ceb018df513a432033c8cfc5d088f936b', 'mystery_start': 'dcc2458017474d2b7ed2ae66f92cac7316bbbaf6fd620336d7a39472536ca35f'}
ANCHORS = [('work', 88, '053600286cff00005dfb331cb1d6'), ('work', 820, '053700286cff6400053100fbb42cfffb5a1c5603286cff0000fdb637000a47000000'), ('work', 52841, '053a00286cff00005dfb671cfbd5'), ('work', 53071, '270cff0a13000400f402ebb3fbe6eaf401ebc8fdf8ccfefb27ecfeff1b3604'), ('work', 53110, '272cff0a13000400f408ebb3fbe6'), ('work', 54778, '14'), ('work', 56600, '1e090014'), ('mystery_start', 0, '050400286cff00005dfb741c6000'), ('mystery_start', 243, '286cff0100fdb60400'), ('mystery_start', 252, '275cff0a10000400f407ebb3fbe6fd6a4cfffdb6110035'), ('mystery_start', 277, '051200050900fb9c5cfffdb61200'), ('periodic', 1983, 'e7053901fc962868ff06005dfb331c4009053b012868ff00005dfb741c4009'), ('periodic', 2291, '053b01fec468ff9a9999999999c93ffb9458fffdb63b0105'), ('periodic', 2314, '053b012868ff64005dfb741c40092858ff0f000434ff0a8f00080036040058ff34ff2868ff0000fdb63b01f49c'), ('meeting', 23229, '050700286cff0f005dfb331caa60'), ('meeting', 23322, '051d01fd00'), ('meeting', 23422, '058700fec46cff000000000000f83ffbb45cfffbe900fffdb68700'), ('meeting', 23545, '058700286cff0200fbbc5cfffbe900fffdb68700'), ('meeting', 23661, '052101286cff0200fbb45cfffdb62101'), ('meeting', 23773, '052101fec46cff9a9999999999e93ffbb45cfffbe900fffdb62101'), ('meeting', 23817, '052301286cff50005dfb671c725d'), ('meeting', 23910, '286cff5000fdb62301'), ('meeting', 24001, '286cff6300fdb62301'), ('meeting', 24074, '286cff0000047cfcfc2205da0052'), ('meeting', 24240, '286cff5a00047cfcfc2205da0052'), ('meeting', 24345, '04fcfa286cff07005dfb331c9260')]

def verify(data):
    if hashlib.sha256(data).hexdigest() != KNOWN_SHA256:
        raise ParseError("Unsupported original executable")
    pe = PeImage(data)
    for name, (address, size) in SPANS.items():
        if hashlib.sha256(pe.bytes_at(address, size)).hexdigest() != FINGERPRINTS[name]:
            raise ParseError("Changed " + name + " procedure")
    for name, offset, expected in ANCHORS:
        raw = bytes.fromhex(expected)
        if pe.bytes_at(SPANS[name][0] + offset, len(raw)) != raw:
            raise ParseError("Changed " + name + " anchor")
    # Failure dialogue calls use this procedure's pool, not the weekly pool.
    for offset,cell in [(0xCEA1,0xEF),(0xCEF6,0xF0),(0xD372,0xFB),
                        (0xCFA3,0xD6),(0xCFB6,0xD7),(0xCFC8,0xD9)]:
        raw=pe.bytes_at(0xA0283C+offset,5)
        if raw[0]!=0x3A or struct.unpack_from("<H",raw,3)[0]!=cell:
            raise ParseError("Changed failure dialogue cell")
    for i,offset in enumerate([0xCFD3,0xD04A,0xD0C1,0xD138,0xD1AF,0xD226,0xD29D,0xD314]):
        raw=pe.bytes_at(0xA0283C+offset,5)
        if raw[0]!=0x3A or struct.unpack_from("<H",raw,3)[0]!=0xF3+i:
            raise ParseError("Changed failure reaction cell")
    for index,address in [(0xF1,0xA11A7C),(0xF2,0xA11CA0),(0x78,0xA11AE0),(0xD8,0xA11C30)]:
        if pe.u32(0x5F99AC+index*4)!=address:
            raise ParseError("Changed failure source roster mapping")
    # The same funds variable is read by the failure predicate and lab effects.
    for pool, index, address in [(0x5F99AC,0x3A,0xA1103C),
                                 (0x5F5D08,0x12,0xA1103C),
                                 (0x5FAFCC,0x87,0xA1103C),
                                 (0x5F99AC,0x13B,0xA11548),
                                 (0x5F5D08,0x04,0xA11548),
                                 (0x5F5D08,0x11,0xA11558),
                                 (0x5FAFCC,0x11D,0xA11558)]:
        if pe.u32(pool + index * 4) != address:
            raise ParseError("Changed shared state mapping")
    # Fixed starting fee: member 050C = 25,000,000.
    raw = pe.bytes_at(0x9F651C + 0x188,14)
    if raw[:4] != bytes.fromhex("fec16cff") or struct.unpack_from("<i",raw,4)[0] != 25000000 \
            or raw[8:] != bytes.fromhex("fde608000c05"):
        raise ParseError("Changed mystery-lab cost")
    # Explicitly distinguish seven selectable cases from the latent case 7.
    for offset, value in [(0x5B24,0),(0x5B9F,1),(0x5C13,2),(0x5C83,3),
                          (0x5CFE,4),(0x5DD0,5),(0x5E76,6),(0x5F1C,7)]:
        raw = pe.bytes_at(0x9EFA40 + offset,5)
        if raw[:3] != bytes.fromhex("286cff") or struct.unpack_from("<h",raw,3)[0] != value:
            raise ParseError("Changed mystery result case")
    return {"failure_predicate":"funds < 0", "staff_reaction_variants":8,
            "mystery_selectable_results":list(range(7)),"latent_result":7,
            "mystery_fee":25000000,"mystery_progress_increment":0.2,
            "mystery_completion":"progress > 100"}

if __name__ == "__main__":
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("exe",type=Path)
    args=parser.parse_args()
    verify(args.exe.read_bytes())
    print("PASS: funds failure branch, lab scheduling, seven selectable results and latent eighth handler")
