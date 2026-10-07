#!/usr/bin/env python3
"""Verify bounded burst display/commit facts; never emit original procedures."""
import argparse
import hashlib
from pathlib import Path
from inspect_vb6 import PeImage, ParseError
from trace_hiring_pcode import KNOWN_SHA256


def verify(data):
    if hashlib.sha256(data).hexdigest() != KNOWN_SHA256:
        raise ParseError('Unsupported original executable')
    pe = PeImage(data)
    start = 0xA0283C

    def require(offset, raw):
        expected = bytes.fromhex(raw)
        if pe.bytes_at(start + offset, len(expected)) != expected:
            raise ParseError(f'Burst contribution evidence changed at +{offset:04X}')

    for index, address in [(0x17, 0xA138C0), (0x18, 0xA138A4),
                           (0x2C, 0xA13888), (0x2D, 0xA138DC),
                           (0x7B, 0xA138F8), (0x31, 0xA149B8),
                           (0x51, 0xA12760), (0x4F, 0xA1277C),
                           (0x55, 0xA12798), (0x56, 0xA127B4),
                           (0x57, 0xA127EC), (0x58, 0xA127D0)]:
        if pe.u32(0x5F99AC + index * 4) != address:
            raise ParseError('Burst member mapping changed')
    # The number atlas reads the actual accumulated counter, before increment.
    require(0x9742, '6b 48 ff e7 05 7b 00 fc 96 28 6c ff 0a 00 fb bc 2c ff fb e9 0c ff')
    require(0x97D2, '6b 48 ff e7 05 2d 00 fc 96 28 6c ff 01 00 fb 94 2c ff 6b 48 ff e7 05 2d 00 52')
    # 60 * timing factor (2) is 120 accumulation ticks, then commit.
    require(0x9AE9, '6b 48 ff e7 05 2d 00 fc 96 28 6c ff 3c 00 05 31 00 fb b4 2c ff fb 5a 1c 15 9c')
    for offset, local in [(0x9B17, 'dc fb'), (0x9B38, 'cc fb'),
                          (0x9B59, 'bc fb'), (0x9B7A, 'ac fb'),
                          (0x9B9B, '9c fb'), (0x9BBC, '8c fb')]:
        require(offset, '6b 48 ff e7 05 7b 00 fc 96 fd 00 ' + local)
    # Each quality receives its full category counter.
    for offset, member, local in [(0x9C67, '55', 'dc'), (0x9C84, '56', 'cc'),
                                 (0x9CA1, '57', 'bc'), (0x9CBE, '58', 'ac')]:
        require(offset, f'04 1c fc fc 22 05 {member} 00 fc 96 04 {local} fb fb 94 2c ff 04 1c fc fc 22 05 {member} 00 52')
    # Burst progress gets one third for quality/bugs, full counter for speed.
    for offset, local in [(0x9DCF, 'dc'), (0x9DF5, 'cc'), (0x9E1B, 'bc'),
                          (0x9E41, 'ac'), (0x9E67, '8c')]:
        require(offset, f'04 1c fc fc 22 05 51 00 fc 96 04 {local} fb 28 6c ff 03 00 fb bc 2c ff fb 94 0c ff 04 1c fc fc 22 05 51 00 52')
    require(0x9E8D, '04 1c fc fc 22 05 51 00 fc 96 04 9c fb fb 94 2c ff 04 1c fc fc 22 05 51 00 52')
    require(0x9EC4, '04 1c fc fc 22 05 4f 00 fc 96 04 8c fb fb 94 2c ff 04 1c fc fc 22 05 4f 00 52')
    require(0x9C08, '28 6c ff 00 00 6b 48 ff e7 05 2c 00 52')
    return True


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('exe', type=Path)
    verify(parser.parse_args().exe.read_bytes())
    print('PASS: display counter, 120-tick commit, full quality/bug gain and category-specific progress.')
