#!/usr/bin/env python3
"""Verify indexed hardware picture resources; never export original pixels."""
import hashlib, struct
from inspect_vb6 import PeImage, ParseError
from trace_hiring_pcode import KNOWN_SHA256

def verify(data):
    if hashlib.sha256(data).hexdigest() != KNOWN_SHA256:
        raise ParseError("Unsupported hardware picture source")
    pe=PeImage(data)
    def require(address, hexadecimal):
        expected=bytes.fromhex(hexadecimal)
        if pe.bytes_at(address,len(expected)) != expected:
            raise ParseError("Hardware picture selection anchor changed")
    require(0x9940B8+0x9B,'05 04 00 28 6c ff 0a 00 5d fb 4d')
    require(0x9940B8+0xA9,'04 54 ff 05 04 00 55 05 24 00 24 25 00 0f 34 03')
    # Own-console globals captured by the launch form; four design/media pairs.
    if pe.u32(0x608B44+0x3C*4)!=0xA11EA0 or pe.u32(0x608B44+0x3D*4)!=0xA11E90:
        raise ParseError("Own-console presentation map changed")
    for offset,design,medium,index in [(0xF7,0,0,11),(0x163,0,1,12),(0x1CF,1,0,13),(0x23B,1,1,14)]:
        require(0x9940B8+offset,"053c00")
        require(0x9940B8+offset+3,"286cff"+struct.pack("<h",design).hex())
        require(0x9940B8+offset+13,"053d00")
        require(0x9940B8+offset+16,"2838ff"+struct.pack("<h",medium).hex())
        require(0x9940B8+offset+38,"f4"+bytes([index]).hex()+"0524002425000f3403")
    for index in range(15):
        offset=0x1A9F3E+(14-index)*0x3069
        if (data[offset-27:offset-15] != b'ImageHard\0\x18\x01' or
            struct.unpack_from('<H',data,offset-15)[0] != index or
            data[offset:offset+2] != b'BM' or
            struct.unpack_from('<ii',data,offset+18) != (64,64)):
            raise ParseError("Hardware picture resource shape changed")
    return {"picture_count":15,"dimensions":[64,64],"quote_source_indices":[0,10],"own_console_indices":[11,12,13,14]}
