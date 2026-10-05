#!/usr/bin/env python3
"""Bounded sequel evidence checks. Never emits original text or assets."""
import argparse, hashlib
from pathlib import Path
from inspect_vb6 import PeImage, ParseError
from trace_hiring_pcode import KNOWN_SHA256

def verify(data):
    if hashlib.sha256(data).hexdigest()!=KNOWN_SHA256:
        raise ParseError("Unsupported original executable")
    pe=PeImage(data)
    def require(address,raw):
        expected=bytes.fromhex(raw)
        if pe.bytes_at(address,len(expected))!=expected:
            raise ParseError(f"Sequel evidence changed at {address:08X}")
    # Room chooser selects mode 1.
    require(0x97C26C,"6c 08 00 fd 9c 78 ff 05 00")
    # Eligibility reads the predecessor-consumed marker.
    require(0x9A4725,"04 48 ff fc 22 05 1b 00 fc 96 28 58 ff 00 00")
    # Eligibility also reads Hall of Fame.
    require(0x9A473E,"05 1c 00 fc 96 28 28 ff 01 00 5d fb 2f e8 fe")
    # Selected row resolves through the eligible product roster.
    require(0x9993B0,"7f 0c 00 e7 05 1d 00 fc 96 fd c0 03 00")
    # Mode 1 selects the exact 0.3 double constant.
    require(0x99BB1B,"05 0c 00 28 38 ff 01 00 5d fb 33 1c 03 02 fe c4 38 ff 33 33 33 33 33 33")
    # Fun multiply/floor/write; remaining qualities share this structure.
    require(0x99BB39,"05 0d 00 fc 22 05 0e 00 fc 96 04 28 ff fb b4 68 ff fb e9 58 ff 05 05 00 fc 22 05 0f 00")
    # Recognition multiply/floor/write.
    require(0x99BBB1,"05 0d 00 fc 22 05 16 00 fc 96 04 28 ff fb b4 68 ff fb e9 58 ff 05 05 00 fc 22 05 17 00")
    # Inherited type uses the predecessor source ID directly.
    require(0xA0CFBE,"04 1c fc fc 22 05 bb 00 fc 96 fc 22 05 be 00 fc 96 04 1c fc fc 22 05 c1 00 fc b0")
    # Type points add directly to room required points.
    require(0xA0CFF6,"05 c2 00 fc 96 fb 94 2c ff 04 1c fc fc 22 05 52 00")
    # Inherited genre uses the predecessor source ID directly.
    require(0xA0D358,"04 1c fc fc 22 05 bb 00 fc 96 fc 22 05 c8 00 fc 96 04 1c fc fc 22 05 ca 00 fc b0")
    # Genre points add directly to room required points.
    require(0xA0D390,"05 cb 00 fc 96 fb 94 2c ff 04 1c fc fc 22 05 52 00")
    # Review total >=32 sets Hall of Fame scratch flag.
    require(0x9CB1AF,"04 e4 fe 28 4c ff 20 00 5d fb 5a 1c bc 02")
    # Hall of Fame scratch flag becomes 1.
    require(0x9CB218,"28 4c ff 01 00 fd b6 0c 00")
    # Shipment copies mode and predecessor to product fields.
    require(0x9C9FB1,"05 10 00 fc 22 05 2d 00 fc 96 05 00 00 fc 22 05 2e 00 fc b0 05 10 00 fc 22 05 2f 00 fc 96 05 00 00 fc 22 05 30 00")
    # Increment the fresh product counter, not the predecessor counter.
    require(0x9CA094,"05 00 00 fc 22 05 3d 00 fc 96 28 6c ff 01 00 fb 94 5c ff 05 00 00 fc 22 05 3d 00 52 35 5c")
    # Mark the predecessor as having a shipped sequel.
    require(0x9CA0B3,"28 6c ff 01 00 05 10 00 fc 22 05 2f 00 fc 96 fc 22 05 3e 00 52 05 10 00")
    # Copy series base title from predecessor.
    require(0x9CA0C8,"05 10 00 fc 22 05 2f 00 fc 96 fc 22 05 3f 00 fc 96 05 00 00 fc 22 05 3f 00 fc b0")
    # Default suffix is predecessor counter plus 2.
    require(0x9B4780,"05 3d 00 fc 96 28 48 ff 02 00 fb 94 58 ff fb ef 38 ff")
    # New-company reset empties the counter cells (VB arithmetic zero).
    require(0x9DE745,"fc 67 6c ff 04 5c ff fc 22 07 08 00 ac 1f 52")
    if pe.u32(0x60105C)!=0xA1411C: raise ParseError("Sequel pool mapping changed")
    if pe.u32(0x60736C)!=0xA12B84: raise ParseError("Sequel pool mapping changed")
    if pe.u32(0x607460)!=0xA12FC8: raise ParseError("Sequel pool mapping changed")
    if pe.u32(0x5F9C98)!=0xA1269C: raise ParseError("Sequel pool mapping changed")
    if pe.u32(0x5F9CA4)!=0xA12D28: raise ParseError("Sequel pool mapping changed")
    if pe.u32(0x5F9CB0)!=0xA12920: raise ParseError("Sequel pool mapping changed")
    if pe.u32(0x5F9CB4)!=0xA12324: raise ParseError("Sequel pool mapping changed")
    # Fixed VARIANT counter array: 513 cells, lower bound zero.
    if (pe.u16(0x61BE88),pe.u32(0x61BE88+20),pe.u32(0x61BE88+32),pe.u32(0x61BE88+36))!=(0x1FAC,16,513,0):
        raise ParseError("Sequel counter declaration changed")
    return {"carry_fraction":0.3,"hall_of_fame_total":32,"fresh_sequel_counter":1}

if __name__=="__main__":
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("exe",type=Path)
    verify(parser.parse_args().exe.read_bytes())
    print("PASS: eligibility, inheritance, automatic checkpoints, shipment and series counter anchors.")
