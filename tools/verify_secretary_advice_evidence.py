#!/usr/bin/env python3
"""Bounded manual secretary advice predicates; never exports original text/assets."""
import hashlib
from inspect_vb6 import PeImage, ParseError
from trace_hiring_pcode import KNOWN_SHA256

def verify(data):
    if hashlib.sha256(data).hexdigest()!=KNOWN_SHA256:
        raise ParseError("Unsupported secretary advice source")
    pe=PeImage(data)
    def require(address,raw):
        expected=bytes.fromhex(raw)
        if pe.bytes_at(address,len(expected))!=expected:
            raise ParseError(f"Secretary advice anchor changed at {address:08X}")
    # Native inclusive hit rectangle and call into Module1.46.
    require(0x9C6DDB,"82 14 00 f4 68 eb e1 82 14 00 f4 72 eb d7 c4 82 18 00 f4 50 eb e1 c4 82 18 00 f4 68 eb d7 c4 1c 46 07 0a 22 00 00 00")
    # Source branch/draw order, thresholds and comparisons.
    require(0x9B15F0,"f5 00 00 00 00 07 08 00 f0 16 fc 96 28 6c ff 00 00 5d fb 2f 5c ff f5 01 00 00 00 07 08 00 f0 16 fc 96 28 4c ff 00 00 5d fb 2f 3c ff fb 27 2c ff ff 1b 1c 84 00")
    require(0x9B1674,"f5 00 00 00 00 07 08 00 44 04 fc 96 28 6c ff 01 00 5d fb 2f 5c ff f5 01 00 00 00 07 08 00 44 04 fc 96 28 4c ff 01 00 5d fb 2f 3c ff fb 1f 2c ff f5 02 00 00 00 07 08 00 44 04 fc 96 28 1c ff 01 00 5d fb 2f fc fe fb 1f dc fe fb c4 bc fe f5 01 00 00 00 07 08 00 b4 04 fc 96 fe c1 0c ff 00 87 93 03 fb 94 9c fe 07 08 00 20 00 fb 49 8c fe fb 27 6c fe ff 1b 35 9c fe 1c 67 01 27 5c ff 0a 55 00 04 00 f4 02 eb b3 fb e6 ea f4 00 eb c8 35 5c ff 1c 67 01")
    require(0x9B17AC,"07 08 00 20 00 fe c1 6c ff 80 d1 f0 08 5d fb 56 5c ff 04 5c fe 28 4c ff 00 00 5d fb 49 3c ff fb 27 2c ff ff 1b 1c 4f 02 27 5c ff 0a 55 00 04 00 f4 02 eb b3 fb e6 ea f4 00 eb c8 35 5c ff 1c 4c 02")
    require(0x9B183F,"07 08 00 20 00 fe c1 6c ff 80 b2 e6 0e 5d fb 56 5c ff 04 5c fe 28 4c ff 02 00 5d fb 49 3c ff fb 27 2c ff ff 1b 1c e2 02 27 5c ff 0a 55 00 04 00 f4 02 eb b3 fb e6 ea f4 00 eb c8 35 5c ff 1c df 02")
    require(0x9B18D2,"07 08 00 20 00 fe c1 6c ff 80 93 dc 14 5d fb 56 5c ff 04 5c fe 28 4c ff 04 00 5d fb 49 3c ff fb 27 2c ff ff 1b 1c 72 03 27 5c ff 0a 55 00 04 00 f4 02 eb b3 fb e6 ea f4 00 eb c8 35 5c ff 1c 72 03")
    require(0x9B1962,"07 08 00 50 00 28 6c ff 02 00 5d fb 33 1c d1 03")
    require(0x9B19C1,"07 08 00 50 00 28 6c ff 03 00 5d fb 33 1c 30 04")
    require(0x9B1A20,"07 08 00 50 00 28 6c ff 0c 00 5d fb 2f 5c ff 07 08 00 60 00 28 4c ff 03 00 5d fb 49 3c ff fb 27 2c ff ff 1b 1c a6 04")
    require(0x9B1ABB,"04 4c fe fc 22 07 08 00 1c 0f fc 96 28 6c ff 01 00 5d fb 2f 5c ff 04 4c fe fc 22 07 08 00 8c 0f fc 96 28 4c ff 01 00 5d fb 2f 3c ff fb 27 2c ff ff 1b 1c 10 05")
    require(0x9B1B09,"27 5c ff 0a 55 00 04 00 f4 02 eb b3 fb e6 ea f4 00 eb c8 35 5c ff 1c 49 06")
    require(0x9B1C39,"07 08 00 98 09 fc 22 07 08 00 48 0a fc 96 28 6c ff 08 00 5d fb 2f 5c ff 07 08 00 b4 0e 28 4c ff 01 00 5d fb 56 3c ff fb 27 2c ff ff 1b 1c e4 06 27 5c ff 0a 55 00 04 00 f4 02 eb b3 fb e6 ea f4 00 eb c8 35 5c ff 1c e1 06")
    require(0x9B1CD4,"07 08 00 98 09 fc 22 07 08 00 48 0a fc 96 28 6c ff 10 00 5d fb 2f 5c ff 07 08 00 b4 0e 28 4c ff 03 00 5d fb 56 3c ff fb 27 2c ff ff 1b 1c 7f 07 27 5c ff 0a 55 00 04 00 f4 02 eb b3 fb e6 ea f4 00 eb c8 35 5c ff 1c 7c 07")
    require(0x9B1D6F,"07 08 00 98 09 fc 22 07 08 00 48 0a fc 96 28 6c ff 20 00 5d fb 2f 5c ff 07 08 00 b4 0e 28 4c ff 05 00 5d fb 56 3c ff fb 27 2c ff ff 1b 1c 17 08 27 5c ff 0a 55 00 04 00 f4 02 eb b3 fb e6 ea f4 00 eb c8 35 5c ff 1c 17 08")
    require(0x9B1E2C,"04 4c fe fc 22 07 08 00 ec 1d fc 96 28 6c ff 02 00 5d fb 33 1c 6a 08")
    require(0x9B1E63,"04 5c fe fe c1 6c ff 40 0d 03 00 5d fb 5a 1c ec 08 27 5c ff 0a 55 00 04 00 f4 02 eb b3 fb e6 ea f4 00 eb c8 35 5c ff 1c ec 08")
    require(0x9B1F0A,"04 4c fe fc 22 07 08 00 84 0c fc 96 28 6c ff 01 00 5d fb 33 1c 58 09 04 5c fe 04 4c fe fc 22 07 08 00 c0 0b fc 96 fb 94 5c ff fc f6 5c fe 04 dc fd 28 6c ff 01 00 fb 94 5c ff fc f6 dc fd")
    require(0x9B1F51,"07 08 00 20 00 fe c1 6c ff 80 f0 fa 02 5d fb 4d 1c c6 09")
    require(0x9B1FB6,"07 08 00 20 00 04 5c fe fb 67 1c 25 0a")
    # Technology count is ownership >=1; stock sum reads only state-2 products.
    require(0x9B177C,"04 4c fe fc 22 07 08 00 f0 14 fc 96 28 6c ff 01 00 5d fb 5a 1c b3 01")
    require(0x9B1E43,"04 5c fe 04 4c fe fc 22 07 08 00 5c 1e fc 96 fb 94 5c ff fc f6 5c fe")
    # Tenant setter fee argument and source-1 definition: 60m, plus 60m reserve.
    require(0x97D92C,"fd 16 14 00 5c ff 07 08 00 20 04 fc 22 07 08 00 b4 04 fc b0")
    require(0x9F651C+0x9999,"fe c1 bc fe 00 87 93 03")
    # Supported shipment writes member 1DEC/A12E08=2, inventory A12E78.
    require(0x9CAC74,"05 07 00 05 00 00 fc 22 05 59 00 fc b0 28 6c ff 02 00 05 00 00 fc 22 05 5a 00 52")
    if pe.u32(0x60736C+0x59*4)!=0xA12E78 or pe.u32(0x60736C+0x5A*4)!=0xA12E08:
        raise ParseError("Unexpected shipment stock/state globals")
    # All preliminary calls queue (argument 28=0); final three present (28=1).
    require(0x9B1625,"28 9c fe 00 00")
    require(0x9B1708,"28 9c fe 00 00")
    require(0x9B17ED,"28 9c fe 00 00")
    require(0x9B1880,"28 9c fe 00 00")
    require(0x9B1913,"28 9c fe 00 00")
    require(0x9B1972,"28 9c fe 00 00")
    require(0x9B19D1,"28 9c fe 00 00")
    require(0x9B1A47,"28 9c fe 00 00")
    require(0x9B1B30,"28 9c fe 00 00")
    require(0x9B1B8D,"28 9c fe 00 00")
    require(0x9B1BEA,"28 9c fe 00 00")
    require(0x9B1C82,"28 9c fe 00 00")
    require(0x9B1D1D,"28 9c fe 00 00")
    require(0x9B1DB8,"28 9c fe 00 00")
    require(0x9B1E8D,"28 9c fe 00 00")
    require(0x9B1F64,"28 9c fe 01 00")
    require(0x9B1FC3,"28 9c fe 01 00")
    require(0x9B2015,"28 9c fe 01 00")
    return {"messages":14,"hot_spring_funds":120000000,"technology_funds":[150000000,250000000,350000000],
            "computer_bits":[8,16,32],"computer_stages":[1,3,5],"stock_threshold":200000,
            "low_funds_max":50000000,"manual_only":True}
