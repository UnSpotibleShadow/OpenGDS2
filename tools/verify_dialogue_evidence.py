#!/usr/bin/env python3
"""Check targeted original dialogue anchors without exporting original text/assets."""
import hashlib
from inspect_vb6 import PeImage, ParseError
from trace_hiring_pcode import KNOWN_SHA256

def verify(data):
    if hashlib.sha256(data).hexdigest() != KNOWN_SHA256:
        raise ParseError("Unsupported original dialogue source")
    pe=PeImage(data)
    def require(address, hexadecimal):
        expected=bytes.fromhex(hexadecimal)
        if pe.bytes_at(address,len(expected)) != expected:
            raise ParseError(f"Dialogue anchor changed at {address:08X}")
    require(0x99FFC8+0xD6,'05 01 00 04 6c ff 0a 0a 00 08 00') # copy before greeting
    require(0x99FFC8+0xE4,'27 6c ff 0a 0b 00 04 00 f4 0a eb b3 fb e6') # floor(Rnd*10)
    require(0x99FFC8+0x14F,'0a 05 00 20 00') # eight-argument modal helper
    require(0x9933DC+0x2AA,'27 3c ff 25 28 5c ff 01 00 25 05 b3 00 24 b4 00 0d b0 02 b5 00')
    require(0x981674,'28 6c ff 00 00 fd b6 00 00') # click clears typing flag
    require(0x981674+0x18,'05 02 00 fb eb 5c ff fd b6 01 00') # reveal remaining text
    require(0x981674+0x6C,'28 6c ff 00 00 fd b6 04 00') # clear queue then unload
    require(0x9930BC+0x141,'07 08 00 58 3c 28 5c ff 01 00 fb 9c 4c ff fc 22 07 08 00 fc 3c fc 96 fb eb ec fe 28 3c ff 03 00 fb b4 dc fe')
    require(0x648EAC,'06 00 54 69 6d 65 72 31 00 0b 03 14 00 00 00') # 20 ms timer property
    for offset,step in [(0x32,2),(0xCD,4),(0x168,6),(0x203,8),(0x29E,10)]:
        require(0x998AB0+offset,f'28 4c ff {step:02x} 00') # mouth cycle branches
    return {"hire_variants":10,"modal_helper":"Module1.62","timed_helper":"Module1.63","text_length_countdown_factor":3,"typing_interval_ms":20}
