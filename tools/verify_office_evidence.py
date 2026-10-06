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
    # Member 2234 contains hidden placeholders, not the assigned computers.
    require(0x9F651C, 0x42A9, '07 08 00 88 23 f5 06 00 00 00 07 08 00 34 22 fc b0')
    require(0x9F651C, 0x49E1, '07 08 00 88 23 f5 11 00 00 00 07 08 00 34 22 fc b0')
    # Runtime pool 1D points to member 2218: IDs 50–61, with 36×29 crops
    # and allocated depths above the desktop, rather than placeholder −1200.
    if pe.u32(0x5F99AC + 0x1D*4) != 0xA13234:
        raise ParseError('Assigned-computer array mapping changed')
    for index in range(12):
        require(0x9F651C, 0x3504 + index*0x54,
            '07 08 00 88 23 f5 '+(index+6).to_bytes(4,'little').hex(' ')+' 07 08 00 18 22 fc b0')
    require(tick, 0x2726, '05 1d 00 fc 96 fc 22 05 1e 00 a2')
    require(tick, 0x276A, '05 1d 00 fc 96 fc 22 05 20 00 a2')
    # Final secretary selection overrides the temporary portrait-1 tile.
    # Setter argument 20 writes portrait array 0938, indexed by append count.
    require(0x9814E4, 0x50, 'fd 16 20 00 5c ff 07 08 00 80 08 fc 22 07 08 00 38 09 fc b0')
    for index, portrait in enumerate([1, 2, 4, 5, 6]):
        require(0x9F651C, 0x2516 + index * 0x4E, f'28 5c fe {portrait:02x} 00')
        require(0x9F651C, 0x253E + index * 0x4E, '0a ff 00 1c 00')
    if pe.u32(0x60DF28 + 0xFF * 4) != 0x60ECF0:
        raise ParseError('Secretary setter mapping changed')
    require(0x9F651C, 0x5142, '07 08 00 88 23 fd f0 08 00 a0 08')
    require(0x9F651C, 0xC0F9, '28 6c ff 01 00 fd e6 08 00 90 08')
    require(0x9F651C, 0xC104, '28 1c ff 30 00 07 08 00 90 08 fc 22 07 08 00 38 09 fc 96')
    require(0x9F651C, 0xC117, '28 6c ff 01 00 fb 9c cc fe 28 2c ff 98 00 fb b4 bc fe fb 94 ac fe')
    require(0x9F651C, 0xC12D, '55 07 08 00 a0 08 fc 22 07 08 00 bc 24 a2')
    require(tick, 0x247D, '28 6c ff b8 00 fc f6 6c fc 28 6c ff 6b 00 fc f6 5c fc')
    require(tick, 0x3FA1, 'fe c4 6c ff 33 33 33 33 33 33 d3 3f')
    require(tick, 0xDCE7, '1c 13 dd 28 6c ff 28 00')
    require(tick, 0xDD0D, '0a 2d 01 04 00 3c 0a 2e 01 00 00 1e 09 00')
    require(tick, 0xD957, 'f5 28 03 44 00')
    require(tick, 0xDA30, 'f5 46 00 66 00')
    # fMainVisual left-click scans workspace slots in ascending order. Its
    # inclusive 16x32 hit box omits the outer two pixels on each side.
    select = 0x9C66BC
    # Original native-pixel diamonds: y+x/2 and y-x/2, narrowed to Single.
    # Strict comparisons precede each room or tenant dispatch.
    require(select, 0x00D1, 'f3 be 00')  # first tenant sum lower: 190
    require(select, 0x00E2, 'f3 12 01')  # sum upper: 274
    require(select, 0x0142, 'f3 0e 01')  # second sum lower: 270
    require(select, 0x0153, 'f3 62 01')  # sum upper: 354
    require(select, 0x01B3, 'f3 8e 01')  # third sum lower: 398
    require(select, 0x01C4, 'f3 e2 01')  # sum upper: 482
    require(select, 0x0223, 'f3 e4 00')  # room zero sum lower: 228
    require(select, 0x0234, 'f3 04 01')  # sum upper: 260
    require(select, 0x0246, 'f4 22')     # difference lower: 34
    require(select, 0x0257, 'f4 45')     # difference upper: 69
    require(select, 0x026B, '28 6c ff 00 00 fd b6 0c 00')  # selected room 0
    require(select, 0x0470, 'f3 44 01')  # room one sum lower: 324
    require(select, 0x0481, 'f3 64 01')  # sum upper: 356
    require(select, 0x04B8, '28 6c ff 01 00 fd b6 0c 00')  # selected room 1
    require(select, 0x0758, '28 08 ff 00 00 04 18 ff 05 25 00')
    require(select, 0x07BE, '28 4c ff 02 00 fb 94 3c ff fb 56 b8 fe')
    require(select, 0x07E6, '28 88 fe 10 00 fb 94 78 fe 28 68 fe 02 00 fb 9c 58 fe fb 49 38 fe')
    require(select, 0x081B, '28 f8 fd 02 00 fb 94 e8 fd fb 56 c8 fd')
    require(select, 0x0847, '28 88 fd 20 00 fb 94 78 fd 28 68 fd 02 00 fb 9c 58 fd fb 49 38 fd')
    require(select, 0x0879, '04 18 ff fd c0 24 00')
    require(select, 0x08BF, '1e cb 08')
    # Pending notification 2EF4 is copied into the active visual field. It is
    # separate from work tier 28A4; notification 4 selects a 51x14 atlas bubble.
    for index, value in [(0x19,0xA14530),(0xEC,0xA139F4),(0x18,0xA138A4)]:
        if pe.u32(0x5F99AC+index*4) != value:
            raise ParseError('Notification/work-tier pool mapping changed')
    require(tick, 0xC96C, '05 19 00 fc 96 6b 48 ff e7 05 ec 00 fc b0')
    require(tick, 0xC97A, '28 6c ff 00 00 6b 48 ff e7 05 19 00 52')
    require(tick, 0xCA55, '28 6c ff 37 00 fc f6 4c fc 28 6c ff 8b 01 fc f6 3c fc')
    require(tick, 0xCA67, '28 6c ff 33 00 fc f6 7c fa 28 6c ff 0e 00 fc f6 6c fa')
    return True


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('exe', type=Path)
    verify(parser.parse_args().exe.read_bytes())
    print('PASS: office spawn, lounge sampling, animation, raster operations and tick timing anchors.')
