#!/usr/bin/env python3
"""Check targeted office rendering/movement anchors in a local supported original.

No graphics, text catalog or whole procedure dump is emitted.
"""
import argparse
import hashlib
from pathlib import Path
from inspect_vb6 import PeImage, ParseError
from trace_hiring_pcode import KNOWN_SHA256


def verify(data):
    if hashlib.sha256(data).hexdigest() != KNOWN_SHA256:
        raise ParseError('Unsupported original executable')
    pe = PeImage(data)

    def require(start, offset, hex_bytes):
        expected = bytes.fromhex(hex_bytes)
        if pe.bytes_at(start + offset, len(expected)) != expected:
            raise ParseError(f'Office evidence changed at {start + offset:08X}')

    hire = 0x98735C
    require(hire, 0x30, 'f3 80 00 04 5c ff fc 22 07 08 00 84 24 a2')
    require(hire, 0x3E, 'f4 38 04 5c ff fc 22 07 08 00 a0 24 a2')
    require(hire, 0x6B, '28 4c ff 07 00')
    require(hire, 0xC6, '28 3c ff 12 00 04 5c ff 04 1c ff 0a 20 00 0c 00')
    action = 0x9AF6BC
    # A failed draw tests counter <=12, increments, then jumps back to Rnd.
    # Counter starts at zero; there is also the initial draw (14 total).
    require(action, 0x180, '04 9c fe 28 5c ff 0c 00 5d fb 4d')
    # Target occupancy must be -1; reassignment clears seated/offset fields.
    require(action, 0x01D3, 'fc 22 05 16 00 fc 96 28 1c ff ff ff 5d fb 2f fc fe')
    require(action, 0x0685, '28 4c ff 00 00 fd 16 10 00 5c ff fc 22 05 27 00 52')
    tick = 0xA0283C
    require(tick, 0, '28 6c ff 02 00 fd b6 31 00')
    require(tick, 0xC044, '1c 5b c0 f4 00 6b 48 ff e7 05 20 00 a2 f4 00')
    # Portrait column step 152; feet depth = Y + slot/1000 +28 + adjustment.
    require(tick, 0xC740, '28 fc fe 98 00')
    # Work sitting draws intermediate facing frame, then shifts base position.
    require(tick, 0x3444, '28 6c ff 18 00 6b 48 ff e7 05 67 00 fc 96 fb 94 2c ff')
    require(tick, 0x369B, '6b 48 ff e7 05 1e 00 9d f4 08 ad')
    # The locally imported twelve computer allocations map actions 6..17.
    require(0x9F651C, 0x42A9, '07 08 00 88 23 f5 06 00 00 00 07 08 00 34 22 fc b0')
    require(0x9F651C, 0x49E1, '07 08 00 88 23 f5 11 00 00 00 07 08 00 34 22 fc b0')
    require(tick, 0x247D, '28 6c ff b8 00 fc f6 6c fc 28 6c ff 6b 00 fc f6 5c fc')
    require(tick, 0x3FA1, 'fe c4 6c ff 33 33 33 33 33 33 d3 3f')
    require(tick, 0xDCE7, '1c 13 dd 28 6c ff 28 00')
    require(tick, 0xDD0D, '0a 2d 01 04 00 3c 0a 2e 01 00 00 1e 09 00')
    require(tick, 0xD957, 'f5 28 03 44 00')
    require(tick, 0xDA30, 'f5 46 00 66 00')
    return True


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('exe', type=Path)
    verify(parser.parse_args().exe.read_bytes())
    print('PASS: office spawn, lounge sampling, animation, raster operations and tick timing anchors.')
