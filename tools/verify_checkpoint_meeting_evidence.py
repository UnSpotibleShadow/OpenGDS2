#!/usr/bin/env python3
"""Bounded alpha/beta meeting evidence; no original text/assets are exported."""
import hashlib
from inspect_vb6 import PeImage, ParseError
from trace_hiring_pcode import KNOWN_SHA256

def verify(data):
    if hashlib.sha256(data).hexdigest() != KNOWN_SHA256:
        raise ParseError("Unsupported checkpoint meeting source")
    pe=PeImage(data)
    def require(address,raw):
        expected=bytes.fromhex(raw)
        if pe.bytes_at(address,len(expected))!=expected:
            raise ParseError(f"Checkpoint meeting anchor changed at {address:08X}")
    work=0xA0283C
    for offset,target in [(0xA7E0,'3d a8'),(0xAB7A,'d7 ab')]:
        require(work+offset,'05 c3 00 28 6c ff 02 00 5d fb 4d 1c '+target)
    if pe.u32(0x5F99AC+0xC3*4)!=0xA12B84:
        raise ParseError("Unexpected product-count global")
    form=0x9A6DDC
    require(form+0x0387,'f4 00 05 00 00 24 01 00 0f 2c 03')
    require(form+0x03DD,'f4 00 05 00 00 24 01 00 0f 2c 03') # both use ImageFormG[0]
    timer=0x9EFA40
    require(timer+0x1CC1,'05 8c 00 24 8d 00 0d b0 02 8e 00')
    require(timer+0x1CFC,'05 8f 00 24 90 00 0d b0 02 91 00')
    alpha=0x9D2F68
    require(alpha+0x062F,'f4 03 eb b3 fb e6')
    require(alpha+0x065A,'f4 06 eb b3 fb e6')
    require(alpha+0x07DA,'6e 50 fd f4 04 eb c8') # variant 3 has no dialogue arm
    beta=0x9C7860
    require(beta+0x01DE,'28 48 ff 0a 00 5d fb 4d')
    require(beta+0x024B,'28 48 ff 1e 00 5d fb 4d')
    require(0x9ACF14+0x0799,'28 68 ff 01 00') # stage changes before type confirmation
    require(0x9ACF14+0x07F7,'0a 02 00 20 00')
    require(0x9ACF14+0x086D,'0a 02 00 20 00')
    require(0x9B00D8+0x0798,'28 68 ff 02 00')
    require(0x9B00D8+0x0910,'28 68 ff 14 00 fb 94 58 ff')
    return {"meeting_modes":[4,5],"illustration_index":0,"type_variants":6,"silent_variant":3,"recognition_thresholds":[10,30],"fun_bonus":20,"intro_product_limit":2}
