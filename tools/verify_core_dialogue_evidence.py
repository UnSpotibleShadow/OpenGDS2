#!/usr/bin/env python3
"""Bounded checkpoint/shipment anchors; no original strings or assets exported."""
import hashlib
from inspect_vb6 import PeImage, ParseError
from trace_hiring_pcode import KNOWN_SHA256

def verify(data):
    if hashlib.sha256(data).hexdigest() != KNOWN_SHA256:
        raise ParseError("Unsupported core dialogue source")
    pe = PeImage(data)
    def require(address, raw):
        expected = bytes.fromhex(raw)
        if pe.bytes_at(address, len(expected)) != expected:
            raise ParseError(f"Core dialogue anchor changed at {address:08X}")
    work = 0xA0283C
    for speaker, body, entry, call in [(0xA807,0xA812,0xC4,0xA820),(0xABA1,0xABAC,0xCC,0xABBA)]:
        require(work+speaker, '3a fc fe 77 00')
        require(work+body, f'3a 6c ff {entry:02x} 00')
        require(work+call, '0a 7a 00 20 00') # shared modal helper, eight arguments
    require(work+0xADC5, '3a fc fe a1 00 04 1c fc 28 6c ff 01 00 fb 94 2c ff')
    require(work+0xADDA, '3a cc fe cd 00 fb ef ec fe')
    require(work+0xADE6, '0a 7a 00 20 00')
    shipment = 0x9C9CB8
    for offset, limit in [(0xDB8,4000),(0xE1F,20000),(0xE86,10000),(0xEED,8000),(0xF54,7000)]:
        require(shipment+offset, f'28 6c ff {limit&255:02x} {limit>>8:02x}')
    require(shipment+0x1215, '3a 6c ff 6d 00') # review introduction before review form
    require(shipment+0x1250, '0d b0 02 70 00')
    # The optional market summary indexes the next slot, after incrementing.
    require(shipment+0x101F, '05 00 00 28 6c ff 01 00 fb 94 5c ff fd b6 00 00')
    require(shipment+0x10B1, '05 00 00 fc 22 05 50 00 fc 96')
    require(0x9DD234+0x1403, 'fc 67 6c ff 04 5c ff fc 22 07 08 00 b4 1d 52') # new-company slot cleared
    return {"checkpoint_introductions":3,"price_thresholds":[4000,20000,10000,8000,7000],"summary_slot":"next","review_intro":True}
