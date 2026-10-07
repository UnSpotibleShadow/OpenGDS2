#!/usr/bin/env python3
"""Verify original shipment snapshots and subsequent empty-room burst cleanup."""
import argparse
import hashlib
from pathlib import Path
from inspect_vb6 import PeImage, ParseError
from trace_hiring_pcode import KNOWN_SHA256


def verify(data):
    if hashlib.sha256(data).hexdigest() != KNOWN_SHA256:
        raise ParseError('Unsupported original executable')
    pe = PeImage(data)

    def require(start, offset, raw):
        expected = bytes.fromhex(raw)
        if pe.bytes_at(start + offset, len(expected)) != expected:
            raise ParseError(f'Release burst evidence changed at {start + offset:08X}')

    # Confirm room fields and distinct product destinations, not a burst value.
    release_pool = 0x60736C
    for source, target, room_address, product_address in [
        (0x11, 0x12, 0xA12798, 0xA12BD8),
        (0x13, 0x14, 0xA127B4, 0xA12BF4),
        (0x15, 0x16, 0xA127EC, 0xA12C2C),
        (0x17, 0x18, 0xA127D0, 0xA12C10),
        (0x19, 0x1A, 0xA1277C, 0xA12C80),
    ]:
        if pe.u32(release_pool + source * 4) != room_address or pe.u32(release_pool + target * 4) != product_address:
            raise ParseError('Shipment snapshot member map changed')
        offset = 0x1E9 + ((source - 0x11) // 2) * 0x14
        require(0x9C9CB8, offset,
                f'05 10 00 fc 22 05 {source:02x} 00 fc 96 05 00 00 fc 22 05 {target:02x} 00 fc b0')
    # Shipment subsequently empties the development room.
    if pe.u32(release_pool + 0x62 * 4) != 0xA1270C:
        raise ParseError('Shipment room state map changed')
    require(0x9C9CB8, 0x104D, '28 6c ff 00 00 05 10 00 fc 22 05 62 00 52')
    # The worker's next empty-room pass discards its outstanding burst counter.
    pool = 0x5F99AC
    for index, address in [(0x4C, 0xA1270C), (0x7B, 0xA138F8)]:
        if pe.u32(pool + index * 4) != address:
            raise ParseError('Empty-room worker member map changed')
    require(0xA0283C, 0x4874, '04 1c fc fc 22 05 4c 00 fc 96 28 6c ff 00 00 5d fb 33 1c eb 48')
    require(0xA0283C, 0x48D1, '28 6c ff 00 00 6b 48 ff e7 05 7b 00 52')
    return True


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('exe', type=Path)
    verify(parser.parse_args().exe.read_bytes())
    print('PASS: completed-stat snapshot, empty-room shipment and pending burst counter cleanup.')
