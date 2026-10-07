#!/usr/bin/env python3
"""Verify original Beginner opening/ending predicates without publishing traces."""
import argparse
import hashlib
import json
import struct
from pathlib import Path
from inspect_vb6 import PeImage, ParseError
from trace_hiring_pcode import KNOWN_SHA256


def verify(data):
    if hashlib.sha256(data).hexdigest() != KNOWN_SHA256:
        raise ParseError("Unsupported original executable")
    pe = PeImage(data)

    def require(address, expected):
        raw = bytes.fromhex(expected)
        if pe.bytes_at(address, len(raw)) != raw:
            raise ParseError(f"Beginner ending evidence changed at {address:08X}")

    # Common date initialization: year/month/week = 1/4/1.
    require(0x9F651C + 0xE, "28 6c ff 01 00 fd e6 08 00 40 00")
    require(0x9F651C + 0x19, "28 6c ff 04 00 fd e6 08 00 50 00")
    require(0x9F651C + 0x24, "28 6c ff 01 00 fd e6 08 00 60 00")
    # Each comparison loads a member, a literal, then HardType / EqVar.
    ending = []
    for offset, member in [(0x1C7, 0x40), (0x1D6, 0x50), (0x1E9, 0x60)]:
        record = pe.bytes_at(0x9DB4F4 + offset, 15)
        if record[:3] != b"\x07\x08\x00" or struct.unpack_from("<H", record, 3)[0] != member \
                or record[5] != 0x28 or record[10:13] != b"\x5d\xfb\x2f":
            raise ParseError("Ending comparison no longer has its original member/equality structure")
        ending.append(struct.unpack_from("<h", record, 8)[0])
    if ending != [2, 12, 5]:
        raise ParseError("Original terminal date changed")
    require(0x9DB4F4 + 0x1E5, "fb 27 ac fe")
    require(0x9DB4F4 + 0x1F8, "fb 27 8c fe")
    require(0x9DB4F4 + 0x1FC, "07 08 00 0c 3b 28 7c fe 00 00 5d fb 56 6c fe fb 27 5c fe ff 1b")
    # False jumps past the ending; true exits before renewal/payroll.
    require(0x9DB4F4 + 0x211, "1c cb 04")
    require(0x9DB4F4 + 0x4CA, "14")
    require(0x9886C4 + 0x1E, "28 68 ff ff ff fd b6 03 00")
    require(0x9886C4 + 0x48, "28 68 ff 00 00 fd b6 03 00")
    return {"opening": [1, 4, 1], "terminal_transient_date": ending,
            "modern_closing_date": [2, 12, 4], "beginner_marker": "nonnegative",
            "normal_marker": -1, "exit_before_later_transactions": True}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("exe", type=Path)
    args = parser.parse_args()
    print(json.dumps(verify(args.exe.read_bytes()), sort_keys=True))
