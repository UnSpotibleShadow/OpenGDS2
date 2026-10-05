#!/usr/bin/env python3
"""Bounded secretary transaction/policy anchors; no source text or assets exported."""
import hashlib
from inspect_vb6 import PeImage, ParseError
from trace_hiring_pcode import KNOWN_SHA256

def verify(data):
    if hashlib.sha256(data).hexdigest()!=KNOWN_SHA256:
        raise ParseError("Unsupported secretary management source")
    pe=PeImage(data)
    def require(address,raw):
        expected=bytes.fromhex(raw)
        if pe.bytes_at(address,len(expected))!=expected:
            raise ParseError(f"Secretary management anchor changed at {address:08X}")
    # Setter, funds guard, signed debit, farewell, identity write, greeting.
    require(0x9814E4,"fd 16 10 00 5c ff 07 08 00 80 08 fc 22 07 08 00 e4 08 fc b0 fd 16 14 00 5c ff 07 08 00 80 08 fc 22 07 08 00 00 09 fc b0 fd 16 18 00 5c ff 07 08 00 80 08 fc 22 07 08 00 70 09 fc b0 fd 16 1c 00 5c ff 07 08 00 80 08 fc 22 07 08 00 1c 09 fc b0 fd 16 20 00 5c ff 07 08 00 80 08 fc 22 07 08 00 38 09 fc b0 fd 16 24 00 5c ff 07 08 00 80 08 fc 22 07 08 00 54 09 fc b0")
    require(0x98ADF4,"05 00 00 fc 22 05 01 00 fc 96 05 02 00 fb 74 1c 64 00")
    require(0x98AE58,"05 00 00 fc 22 05 01 00 fc 96 fb cc 6c ff fc f6 3c ff 04 3c ff 04 1c ff 0a 06 00 08 00")
    require(0x98AEC5,"0a 05 00 20 00")
    require(0x98AEE4,"05 00 00 fd c0 08 00")
    require(0x98AF60,"0a 05 00 20 00")
    require(0x9ACA08,"04 58 ff 05 08 00 fb 2f f0 fe fb c4 e0 fe ff 1b 1c 80 04")
    require(0x9F656C,"28 6c ff 02 00 fd e6 08 00 b0 08 28 6c ff 02 00 fd e6 08 00 c0 08")
    require(0xA028FD,"05 39 00 28 6c ff 00 00 5d fb 2f 2c ff 05 3a 00 fe c1 fc fe 00 a3 e1 11")
    require(0xA0291E,"05 39 00 28 cc fe 01 00 5d fb 2f dc fe 05 3a 00 fe c1 bc fe 00 e1 f5 05")
    require(0xA0D9C4,"28 6c ff 64 00 fc f6 bc f9")
    require(0xA0D9DB,"28 6c ff 50 00 fc f6 bc f9")
    require(0xA0D9F2,"28 6c ff 3c 00 fc f6 bc f9")
    require(0xA0DA5F,"28 6c ff 01 00 6b 48 ff e7 05 df 00 52 6b 48 ff e7 05 15 00 fc 96 6b 48 ff e7 05 e0 00 fc b0 28 2c ff 12 00 04 48 ff 4d 6c ff 02 40 04 0c ff 0a e1 00 0c 00")
    require(0xA0DABF,"28 6c ff 28 00 fc f6 9c f9")
    require(0xA0DAD6,"28 6c ff 14 00 fc f6 9c f9")
    require(0xA0DAED,"28 6c ff 00 00 fc f6 9c f9")
    require(0xA0DB22,"28 6c ff 00 00 6b 48 ff e7 05 df 00 52 6b 48 ff e7 05 e0 00 40 04 48 ff 4d 6c ff 02 40 04 2c ff 0a e1 00")
    require(0x9AF85D,"fd 16 14 00 4c ff 28 5c ff 01 00 5d fb 5a 1c 92 09")
    # Constant-pool pointers establish roles independently of message contents.
    pools={0x603E1C:{0:0xA14578,1:0xA1198C,2:0xA1103C,5:0x60EEA0,6:0x60ED5C,8:0xA118AC,9:0xA11900,12:0xA11954},
           0x5F99AC:{0x39:0xA118EC,0xDE:0xA118CC,0xE2:0xA118DC,0xDF:0xA13A9C,0xE0:0xA13A80,0xE1:0x5FA088}}
    for pool,entries in pools.items():
        for index,value in entries.items():
            if pe.u32(pool+4*index)!=value:raise ParseError("Unexpected secretary global mapping")
    return {"records":5,"replacement_order":["charge","farewell","identity","greeting"],
            "production_thresholds":[300000000,100000000,None],"entry_thresholds":[None,100,80,60],
            "return_thresholds":[40,20,0],"break_default":2,"return_default":2}
