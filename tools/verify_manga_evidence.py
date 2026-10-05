#!/usr/bin/env python3
"""Bounded adaptation evidence; never exports original catalogs or dialogue."""
import hashlib
from inspect_vb6 import PeImage, ParseError
from trace_hiring_pcode import KNOWN_SHA256

def verify(data):
    if hashlib.sha256(data).hexdigest() != KNOWN_SHA256:
        raise ParseError("Unsupported original adaptation source")
    pe = PeImage(data)
    def require(address, raw):
        expected = bytes.fromhex(raw)
        if pe.bytes_at(address, len(expected)) != expected:
            raise ParseError(f"Adaptation anchor changed at {address:08X}")
    require(0x60ECA8, 'ba 1c 13 98 00') # Helper 20 thunk -> code-info
    for index in range(31):
        require(0x9F651C+0xA481+index*0x57+0x39, '04 1c fe 0a 57 02 1c 00')
    # Six arguments are copied into six indexed catalog arrays.
    for index,member in enumerate([0x37C,0x398,0x3B4,0x3D0,0x3EC,0x408]):
        require(0x98128C+index*0x14, f'fd 16 {0x10+index*4:02x} 00 5c ff')
        require(0x98128C+index*0x14+0xD, f'07 08 00 {member&255:02x} {member>>8:02x} fc b0')
    # Shipment tests mode 2, then reads the selected adaptation's popularity.
    start = 0x9C9CB8
    require(start+0x45F, '28 6c ff 02 00 5d fb 33')
    for offset,limit in [(0x47B,10),(0x500,6),(0x585,3),(0x60A,2)]:
        require(start+offset, f'28 6c ff {limit:02x} 00')
    for offset,constant in [(0x489,'66 66 66 66 66 66 e6 3f'),(0x50E,'00 00 00 00 00 00 e0 3f'),(0x593,'9a 99 99 99 99 99 c9 3f'),(0x618,'9a 99 99 99 99 99 c9 3f')]:
        require(start+offset, 'fe c4 6c ff '+constant)
    # The mislabeled music announcement actually tests the graphics array.
    require(start+0x234, '05 18 00 fc b0')
    require(start+0xB7F, '05 18 00 fc 96 28 6c ff 64 00 5d fb 74')
    return {"catalog_records":31,"mode":2,"market_thresholds":[10,6,3,2],"quality_bonus":"graphics"}
