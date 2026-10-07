#!/usr/bin/env python3
"""Verify bounded borrowing/repayment anchors without exporting source dialogs."""
import argparse
import hashlib
from pathlib import Path
from inspect_vb6 import PeImage, ParseError
from trace_hiring_pcode import KNOWN_SHA256

def verify(data):
    if hashlib.sha256(data).hexdigest()!=KNOWN_SHA256:
        raise ParseError("Unsupported bank source")
    pe=PeImage(data)
    def require(address,raw):
        expected=bytes.fromhex(raw)
        if pe.bytes_at(address,len(expected))!=expected:
            raise ParseError(f"Bank evidence changed at {address:08X}")
    require(0x97A3BC,'27 5c ff 25 28 6c ff 01 00 25 05 60 00 24 61 00 0d b0 02 62 00 13')
    start=0x992DA0
    require(start+0x0098,'1e 8c 02') # refusal leaves the transaction body
    require(start+0x009B,'28 1c ff 00 00 04 6c fe 28 4c ff 02 00 fe 68 4c fe 8c 02')
    require(start+0x00AE,'04 6c fe fc 22 05 00 00 fc 96 28 6c ff 00 00 5d fb 33 1c 83 02')
    for offset,amount in [(0x00FB,10_000_000),(0x013F,50_000_000),(0x0183,100_000_000),(0x01C7,200_000_000)]:
        require(start+offset,'fe c1 6c ff '+amount.to_bytes(4,'little').hex(' ')+' fc f6 20 fe')
    require(start+0x01D3,'04 30 fe 04 6c fe fc 22 05 00 00 fc b0')
    for offset,global_id in [(0x01E0,6),(0x01ED,8),(0x01FA,10)]:
        require(start+offset,f'05 {global_id:02x} 00 04 6c fe fc 22 05 {global_id+1:02x} 00 fc b0')
    require(start+0x0207,'04 20 fe 04 5c ff 0a 0c 00 08 00')
    require(start+0x025F,'0a 03 00 20 00') # receipt after the credit
    require(start+0x0280,'1e 8c 02') # successful transaction leaves the slot loop
    for index,address in [(0,0xA11318),(7,0xA11334),(9,0xA11350),(11,0xA1136C)]:
        if pe.u32(0x5FA124+index*4)!=address:raise ParseError("Bank loan globals changed")
    return {"slots":3,"principal_types":4,"receipt_after_credit":True}

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('exe',type=Path)
    result=verify(parser.parse_args().exe.read_bytes())
    print('PASS: bounded bank entry, first-free slot, four principal values, date storage and receipt order.')
