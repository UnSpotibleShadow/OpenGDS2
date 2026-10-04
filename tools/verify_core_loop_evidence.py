#!/usr/bin/env python3
"""Bounded structural checks for own-game development, release and sales."""
import argparse,hashlib,struct
from pathlib import Path
from inspect_vb6 import PeImage,ParseError
from trace_hiring_pcode import KNOWN_SHA256
from recover_genre_catalog import recover

def verify(data):
    if hashlib.sha256(data).hexdigest()!=KNOWN_SHA256:raise ParseError('Unsupported original executable')
    pe=PeImage(data)
    def require(start,offset,raw):
        expected=bytes.fromhex(raw)
        if pe.bytes_at(start+offset,len(expected))!=expected:raise ParseError(f'Core-loop evidence changed at {start+offset:08X}')
    # Shared type map: 65 VARIANT cells, lower bound zero, fresh allocation.
    decl=0x61D580;array=decl+16
    if (pe.u16(decl),pe.u16(array),pe.u32(array+4),pe.u32(array+12),pe.u32(array+16),pe.u32(array+20))!=(0x31A8,1,16,0,65,0):
        raise ParseError('Type roster declaration changed')
    if not pe.u16(array+2)&0x800:raise ParseError('Type map is not VARIANT cells')
    # Company production capacity 50, genre eligibility cell 23 and two 1500 fees.
    require(0x9F651C,0x00F5,'28 6c ff 32 00 fd e6 08 00 70 00')
    require(0x9F651C,0x0100,'28 6c ff 17 00 fd e6 08 00 90 00')
    require(0x9F651C,0x01A1,'28 6c ff dc 05 fd e6 08 00 60 08 28 6c ff dc 05 fd e6 08 00 70 08')
    # Work experience always adds 3.7; animation accumulation uses a distinct cell.
    require(0xA0283C,0x6622,'fe c4 6c ff 9a 99 99 99 99 99 0d 40 fb 94 2c ff')
    # Twenty-tick sales counter (10 multiplied by original timing factor 2).
    require(0x9E3830,0x0958,'05 3d 01 28 68 ff 0a 00 05 31 00 fb b4 58 ff fb 5a')
    # Revenue subtracts both per-copy fees before multiplication.
    require(0x9E3830,0x10D7,'05 57 01 fb 9c 58 ff 05 58 01 fb 9c 34 ff')
    # Production reads the hardware/band table, not hardware planning cost.
    require(0x9B4344,0x0814,'05 92 00 a7 02 00 06 00 00 05 8d 00 fb 94 58 ff fd b6 06 00')
    # Reviews: four-stat sum, strict first threshold, replacement draw at factor >=2.
    require(0x9CAFCC,0x070D,'04 c8 fd 28 4c ff 82 00')
    require(0x9CAFCC,0x0899,'05 1f 00 28 4c ff 01 00')
    # Hall-of-Fame company counter starts at zero and increments once per ceremony.
    require(0x9F651C,0x0066,'28 6c ff 00 00 fd e6 08 00 00 01')
    require(0x986780,0x0124,'05 09 00 28 58 ff 01 00 fb 94 48 ff fd b6 09 00')
    # Dismissal clears the source employed gate/stress, not progression fields.
    require(0x9981CC,0x03AF,'07 08 00 84 0c 52')
    require(0x9981CC,0x03CB,'07 08 00 68 0c 52')
    # Monthly payroll's index is shared scratch 3018, not the loop variable.
    require(0x9DB4F4,0x06F6,'07 08 00 18 30 fc 22 07 08 00 dc 0b fc 96 fb 94 4c ff')
    # Investigation delay, four-way outcome and zero-stock refund branch.
    require(0x9EFA40,0x38A0,'28 6c ff 05 00 05 eb 00 fc 22 05 ec 00 52')
    require(0x9EFA40,0x3A58,'f4 04 eb b3 fb e6')
    require(0x9EFA40,0x3D15,'28 6c ff 00 00 05 eb 00 fc 22 05 fc 00 52')
    # Shared business cost 5m; original menus guard one action per budget week.
    require(0x9F651C,0x010B,'fe c1 6c ff 40 4b 4c 00 fd e6 08 00 a0 00')
    require(0x981A78,0x0000,'05 73 00 28 6c ff 01 00 5d fb 33')
    # Genre research gain starts at 2 and uses a three-value random range.
    require(0x99ED68,0x026B,'f4 02 eb 6e 48 fe f4 03 eb b3 fb e6 ab')
    # Original main-loop automatic expansion and complete budget-week cadence.
    require(0xA0283C,0x009B,'28 6c ff 50 00 05 31 00 fb b4 2c ff')
    require(0xA0283C,0x0337,'28 6c ff 64 00 05 31 00 fb b4 2c ff')
    # Study capacity is employee member D3C; training task state is 2.
    require(0x993A2C,0x00E3,'05 0a 00 fc 96 fb 5a')
    require(0x993A2C,0x0253,'28 6c ff 02 00 05 11 00 fc 22 05 14 00 52')
    # Training increments its independent idle counter before threshold checking.
    require(0xA0283C,0x421B,'05 71 00 fc 96 28 6c ff 01 00 fb 94 2c ff')
    # Reset clears employee/source skill cells; no guessed fresh learned skills.
    require(0x9DD234,0x0903,'07 08 00 58 0d a7 02 00 fd 86 00 00')
    # Accepted technology subtracts table 1568 experience; timed units reset
    # the shared animation counter to 1 and decrement the application counter.
    require(0x9A98E8,0x0373,'a7 02 00 06 00 00 fb 9c 3c ff 05 00 00 fc 22 05 05 00 52')
    require(0xA0283C,0x6A9E,'28 6c ff 01 00 6b 48 ff e7 05 2c 00 52')
    require(0xA0283C,0x6AAB,'6b 48 ff e7 05 7b 00 fc 96 28 6c ff 01 00 fb 9c 2c ff')
    # Timed technology has TWO increments, ordinary work only the common
    # second one. Verify the branch bypass, threshold, both complete blocks
    # and tier guard, rather than inferring elapsed time from a reset alone.
    for index,value in [(0x2C,0xA13888),(0x18,0xA138A4),(0x31,0xA149B8)]:
        if pe.u32(0x5F99AC+index*4)!=value:raise ParseError('Work cadence pool mapping changed')
    require(0xA0283C,0x5E4E,'28 6c ff 01 00 6b 48 ff e7 05 2c 00 52 28 6c ff 04 00 6b 48 ff e7 05 18 00 52')
    require(0xA0283C,0x66D8,'1c b0 88')
    require(0xA0283C,0x6A84,'6b 48 ff e7 05 2c 00 fc 96 28 6c ff 0a 00 05 31 00 fb b4 2c ff fb 5a 1c 7f 88')
    require(0xA0283C,0x6B5F,'6b 48 ff e7 05 7b 00 fc 96 28 6c ff 00 00 5d fb 4d 1c 7f 88')
    require(0xA0283C,0x8865,'28 6c ff 00 00 6b 48 ff e7 05 2c 00 52')
    for offset,target in [(0x887F,'b0 88'),(0x9C15,'46 9c')]:
        require(0xA0283C,offset,'6b 48 ff e7 05 2c 00 fc 96 28 6c ff 01 00 5d fb 5a 1c '+target+
                ' 6b 48 ff e7 05 2c 00 fc 96 28 6c ff 01 00 fb 94 2c ff 6b 48 ff e7 05 2c 00 52 35 2c ff')
    require(0xA0283C,0x88F3,'1c 15 9c')
    # Shared production B0: fresh Empty, both shipments set 1, weekly reset 0.
    require(0x9DD234,0x005A,'fc 67 6c ff fd e6 08 00 b0 00')
    require(0x9C9CB8,0x01BC,'28 6c ff 01 00 fd b6 0e 00')
    require(0x984BB0,0x0064,'28 4c ff 01 00 fd b6 06 00')
    require(0x9DB4F4,0x000B,'28 6c ff 00 00 fd e6 08 00 b0 00')
    require(0x984A78,0x006D,'05 05 00 28 6c ff 01 00 5d fb 33 1c cd 00')
    # Product weekly units: thirteen reset cells, reverse shift at week end,
    # accumulation in cell zero, and the original chart's two scale thresholds.
    require(0x9DD234,0x153E,'28 3c ff 00 00 04 ac fd 28 4c ff 0c 00')
    require(0x9DB4F4,0x0135,'28 1c ff 0c 00 04 ec fe 28 2c ff 01 00 28 6c ff ff ff')
    require(0x9E3830,0x1090,'05 56 01 a7 02 00 06 00 00 05 41 01 fb 94 58 ff')
    require(0x9C2568,0x070E,'04 f8 fe fe c1 58 ff a0 86 01 00 5d fb 5a 1c e3 08')
    require(0x9C2568,0x08E3,'04 f8 fe 28 58 ff 30 75 5d fb 5a 1c b1 0a')
    # Recall checks mark state 1 even when the draw does not start an inquiry.
    require(0x9E3830,0x1300,'28 68 ff 01 00 6b 7a ff e7 05 60 01 52')
    require(0x9DD234,0x14B7,'fc 67 6c ff 04 5c ff fc 22 07 08 00 04 1f 52')
    require(0x9DD234,0x14C6,'fc 67 6c ff 04 5c ff fc 22 07 08 00 20 1f 52')
    require(0x9DB4F4,0x14F0,'07 08 00 20 1f fc 96 28 6c ff 01 00 fb 9c 4c ff')
    require(0x9EFA40,0x3B4E,'28 6c ff 02 00 05 eb 00 fc 22 05 f9 00 52')
    require(0x9EFA40,0x4129,'28 6c ff 03 00 05 eb 00 fc 22 05 f9 00 52')
    # The financial helper adds the signed delta; it does not clamp cash at zero.
    require(0x97DD04,0x0000,'07 08 00 20 00 fd 16 10 00 5c ff fb 94 4c ff fd e6 08 00 20 00')
    # Signed money helper accumulates the SAME argument into all three periods.
    for offset,member in [(0x15,0x22C),(0x2A,0x23C),(0x3F,0x24C)]:
        load=bytes.fromhex('07 08 00')+struct.pack('<H',member)
        store=bytes.fromhex('fd e6 08 00')+struct.pack('<H',member)
        require(0x97DD04,offset,(load+bytes.fromhex('fd 16 10 00 5c ff fb 94 4c ff')+store).hex())
    for offset,member in [(0x14D,0x22C),(0x157,0x23C),(0x161,0x24C)]:
        require(0x9DD234,offset,(bytes.fromhex('fc 67 6c ff fd e6 08 00')+struct.pack('<H',member)).hex())
    for offset,member in [(0x672,0x22C),(0x6AC,0x23C),(0x725,0x24C)]:
        require(0x9DB4F4,offset,(bytes.fromhex('28 6c ff 00 00 fd e6 08 00')+struct.pack('<H',member)).hex())
    # Ordered weekly calendar: first shipments count, zero initialization and
    # hardware predicates. Bounded instruction structures only, no dialog text.
    require(0x9DD234,0x12CE,'fc 67 6c ff fd e6 08 00 98 1b')
    require(0x9C9CB8,0x102F,'05 60 00 28 6c ff 01 00 fb 94 5c ff fd b6 60 00')
    require(0x9F651C,0x0196,'28 6c ff 00 00 fd e6 08 00 b4 0e')
    for offset,slot in [(0x00BA,0),(0x00CA,1),(0x00DA,2)]:
        require(0x9F651C,offset,(bytes.fromhex('28 6c ff ff ff f5')+struct.pack('<i',slot)+bytes.fromhex('07 08 00 44 04 52')).hex())
    for offset,member in [(0xB2C,0x106C),(0xB4A,0x10A4),(0xB59,0x10C0),(0xB68,0x10DC),
                           (0x1220,0x1A70),(0x122F,0x1A8C)]:
        require(0x9DD234,offset,(bytes.fromhex('fc 67 6c ff 04 5c ff fc 22 07 08 00')+struct.pack('<H',member)+b'\x52').hex())
    for offset,member in [(0x1F9,0x2FC),(0x208,0x318),(0x217,0x334),(0x226,0x350)]:
        require(0x9DD234,offset,(bytes.fromhex('fc 67 6c ff 04 5c ff fc 22 07 08 00')+struct.pack('<H',member)+b'\x52').hex())
    for offset,identifier,stage,funds in [(0x793,3,0,150000000),(0x887,4,1,300000000),
        (0x9C7,2,2,450000000),(0xB07,10,3,70000000),(0xBFB,5,4,1000000000),
        (0xD3B,9,5,1200000000),(0xE7B,6,6,1400000000),(0x1007,7,6,1600000000)]:
        require(0x9DB4F4,offset,'07 08 00 98 1b 28 6c ff 04 00 5d fb 5a')
        require(0x9DB4F4,offset+0x10,(bytes.fromhex('04 3c ff fc 22 07 08 00 c8 0e fc 96 28 6c ff')+struct.pack('<h',identifier)+bytes.fromhex('5d fb 2f 4c ff 07 08 00 b4 0e 28 2c ff')+struct.pack('<h',stage)).hex())
        require(0x9DB4F4,offset+0x39,(bytes.fromhex('07 08 00 20 00 fe c1 1c ff')+struct.pack('<i',funds)+bytes.fromhex('5d fb 56 9c fe')).hex())
        require(0x9DB4F4,offset+0x4F,'04 3c ff fc 22 07 08 00 6c 10 fc 96 28 7c fe 00 00 5d fb 2f 6c fe')
    require(0x9DB4F4,0x1143,'28 6c ff 01 00 fb 9c 4c ff')
    require(0x9DB4F4,0x1163,'28 6c ff 04 00 5d fb 33')
    require(0x9DB4F4,0x1275,'28 6c ff 0c 00 5d fb 33')
    require(0x9DB4F4,0x1222,'28 6c ff 01 00 04 3c ff fc 22 07 08 00 1c 0f 52')
    require(0x9DB4F4,0x1374,'28 6c ff 02 00 04 3c ff fc 22 07 08 00 1c 0f 52 28 6c ff 00 00 04 3c ff fc 22 07 08 00 8c 0f 52')
    require(0x9DB4F4,0x1593,'04 3c ff fc 22 07 08 00 c0 10 fc 96 fb b4 4c ff')
    require(0x9DB4F4,0x15BF,'fe c4 6c ff 00 00 00 00 00 00 f8 3f fb bc ac fe fb b4 9c fe 28 2c ff 84 03 fb bc 8c fe')
    for offset,principal in [(0x1818,10000000),(0x183B,50000000),(0x185E,100000000),(0x1881,200000000)]:
        require(0x9DB4F4,offset,(bytes.fromhex('fe c1 6c ff')+struct.pack('<i',principal)).hex())
    require(0x9DB4F4,0x1939,'04 f8 fb fe c4 6c ff cd cc cc cc cc cc f4 3f fb b4 4c ff fb cc bc fe')
    require(0x9DB4F4,0x1966,'28 6c ff 00 00 04 3c ff fc 22 07 08 00 fc 02 52')
    # Both negotiation modes have independently checked fee/result branches.
    # Only outcome Rnd is verified here; dialogue RNG and entry charges are not.
    pool=0x5FAFCC
    for index, expected in [(0x7,0xA148EC),(0x27,0x401076),(0x107,0xA1188C),(0x116,0xA1187C)]:
        if pe.u32(pool+index*4)!=expected:raise ParseError('Negotiation pool mapping changed')
    require(0x9EFA40,0x419E,'05 07 00 28 6c ff 0d 00 5d fb 33')
    require(0x9EFA40,0x4E58,'05 07 00 28 6c ff 0e 00 5d fb 33')
    # Provider dispatch records mode then opens the meeting form. This helper
    # has no financial call and does not establish a consultation charge.
    require(0x60ED80,0,'ba e0 bd 97 00')
    require(0x97BDB8,0,'fd 16 10 00 5c ff fd f0 08 00 d0 38')
    require(0x97BDB8,0x000C,'27 4c ff 25 28 5c ff 01 00 25 05 22 00 24 23 00 0d b0 02 24 00')
    require(0x987810,0x010B,'28 6c ff 0e 00 04 4c ff 0a 10 00 08 00')
    require(0x987678,0x010B,'28 6c ff 0d 00 04 4c ff 0a 10 00 08 00')
    for mode,index,compares,reductions,draws in [
        (13,0x107,[0x41FB,0x437F,0x45C3,0x4807,0x4A4B,0x4CE4],
         [0x436C,0x4509,0x474D,0x4991,0x4C2A],[0x4437,0x467B,0x48BF,0x4B03]),
        (14,0x116,[0x4EB5,0x5039,0x527D,0x54C1,0x5705,0x5949],
         [0x5026,0x51C3,0x5407,0x564B,0x588F],[0x50F1,0x5335,0x5579,0x57BD])]:
        for offset,fee in zip(compares,[1500,1300,1100,900,700,500]):
            raw=b'\x05'+struct.pack('<H',index)+b'\x28\x6c\xff'+struct.pack('<h',fee)+bytes.fromhex('5d fb 33')
            require(0x9EFA40,offset,raw.hex())
        for offset in reductions:
            raw=b'\x05'+struct.pack('<H',index)+bytes.fromhex('28 6c ff c8 00 fb 9c 5c ff fd b6')+struct.pack('<H',index)
            require(0x9EFA40,offset,raw.hex())
        for offset,width in zip(draws,[2,4,8,16]):
            raw=bytes.fromhex('27 5c ff 0a 27 00 04 00 f4')+bytes([width])+bytes.fromhex('eb b3 fb e6 ea f4 00 eb c8')
            require(0x9EFA40,offset,raw.hex())
    # Speed technology duration is a separate workspace countdown, not a
    # proven multiplier. Verify its writes and end-of-work-block expiry.
    for pool,index,value in [(0x61190C,0xA4,0xA13A64),(0x5F99AC,0xD5,0xA13A64),
                             (0x5F99AC,0x7C,0xA139A0),(0x5F99AC,0x7D,0xA13A48),
                             (0x5F99AC,0x7E,0xA12638),(0x5F99AC,0x19,0xA14530)]:
        if pe.u32(pool+index*4)!=value:raise ParseError('Speed lifetime pool mapping changed')
    for offset,ticks in [(0x076B,50),(0x079E,100),(0x07D1,150)]:
        require(0x9A98E8,offset,(bytes.fromhex('28 4c ff')+struct.pack('<h',ticks)+
            bytes.fromhex('05 23 00 fc 22 05 a4 00 52')).hex())
    require(0xA0283C,0xAFAD,'fc 22 05 7e 00 fc 96 28 fc fe 04 00 5d fb 2f 0c ff')
    require(0xA0283C,0xAFC7,'28 6c ff 04 00 6b 48 ff e7 05 19 00 52')
    require(0xA0283C,0xAFD4,'6b 48 ff e7 05 d5 00 fc 96 28 6c ff 01 00 fb 9c 2c ff 6b 48 ff e7 05 d5 00 52')
    require(0xA0283C,0xAFF2,'05 d5 00 fc 96 28 6c ff 00 00 5d fb 4d 1c 1c b0')
    require(0xA0283C,0xB002,'28 6c ff 00 00 6b 48 ff e7 05 7c 00 52 28 6c ff 00 00 6b 48 ff e7 05 d5 00 52')
    # Annual calendar forms: bounded branch checks, with no game text or assets.
    # Normal tutorial marker is -1.
    require(0x9886C4,0x001E,'28 68 ff ff ff fd b6 03 00')
    # Beginner marker starts nonnegative.
    require(0x9886C4,0x0048,'28 68 ff 00 00 fd b6 03 00')
    # Beginner ending checks marker >=0.
    require(0x9DB4F4,0x01FC,'07 08 00 0c 3b 28 7c fe 00 00 5d fb 56 6c fe fb 27 5c fe ff 1b')
    # Ending exits before renewal and payroll.
    require(0x9DB4F4,0x04CA,'14')
    # Five-product exhibition cap.
    require(0x98FBA8,0x0014,'05 00 00 28 6c ff 05 00 5d fb 5a 1c 7a 00')
    # Exhibition fee is ten million per item.
    require(0x98FBA8,0x01BA,'fe c1 6c ff 80 96 98 00 fd b6 05 00')
    # Visitor increment every fourth tick.
    require(0x99AB50,0x0084,'05 12 00 28 6c ff 04 00 5d fb 5a 1c 0c 01')
    # Visitor draw width 350 plus 450.
    require(0x99AB50,0x0098,'0a 17 00 04 00 f3 5e 01 eb b3 fb e6 f3 c2 01 eb ab fd 6a 4c ff fb')
    # Exhibition completes at ninety ticks.
    require(0x99AB50,0x010C,'05 0b 00 28 6c ff 5a 00 5d fb 5a 1c 66 04')
    # Awareness gain floors visitors divided by 500.
    require(0x99AB50,0x02B8,'05 0c 00 28 6c ff f4 01 fb bc 5c ff fb e9 3c ff')
    # Room cap mutates the shared gain.
    require(0x99AB50,0x0305,'04 f0 fd fb 94 5c ff 28 6c ff 64 00 5d fb 74 35 5c ff 1c 31 03 28 6c ff 64 00 f5 00 00 00 00 05 22 00')
    # Music first threshold seventy.
    require(0x9BD6F8,0x0098,'28 6c ff 46 00 5d fb 5a 1c d6 00')
    # Music draw denominator ten followed by next branch.
    require(0x9BD6F8,0x00A3,'27 5c ff 0a 0a 00 04 00 f4 0a eb b3 fb e6 ea f4 01 eb c8 35 5c ff')
    # Music first threshold bypasses higher thresholds.
    require(0x9BD6F8,0x00D3,'1e 58 01 04 34 ff fc 22 05 10 00 fc 96 fc 22 05 11 00 fc 96 28 6c ff 64 00 5d fb')
    # Graphics first threshold seventy.
    require(0x9BD6F8,0x0335,'28 6c ff 46 00 5d fb 5a 1c 73 03')
    # Design first threshold eighty.
    require(0x9BD6F8,0x05F7,'04 80 fe 28 6c ff 50 00 5d fb 5a 1c 38 06')
    # Worst category clears the first-pass winner.
    require(0x9BD6F8,0x0900,'28 6c ff 00 00 fc f6 44 ff')
    # Worst second pass draws unconditionally.
    require(0x9BD6F8,0x093E,'27 90 fe 0a 0a 00 04 00 f4 02 eb b3 fb e6 ea f4 00 eb c8 fd f8 14 ff fb 27 30 fe ff 1b')
    # Grand prize first threshold 120.
    require(0x9BD6F8,0x0BAE,'04 80 fe 28 6c ff 78 00 5d fb 5a 1c ef 0b')
    # Million-unit sales overrides grand prize lottery.
    require(0x9BD6F8,0x0C73,'05 10 00 fc 96 fc 22 05 24 00 fc 96 fe c1 6c ff 40 42 0f 00 5d fb 5a 1c a4 0c 28 6c ff 01 00 fc')
    # Worst-game payment is signed negative two hundred million.
    require(0x9CEE10,0x0DCA,'fe c1 4c ff 00 3e 14 f4')
    # Grand prize payment is five hundred million.
    require(0x9CEE10,0x1146,'fe c1 4c ff 00 65 cd 1d')
    # Graduate minimum and maximum are inclusive.
    require(0x9DB4F4,0x1A96,'07 08 00 70 0a 04 3c ff fb 49 4c ff 07 08 00 80 0a 04 3c ff fb 56 bc fe fb 27 ac fe ff 1b 1c 19')
    # Graduate gate does not skip the random draw.
    require(0x9DB4F4,0x1ACD,'27 bc fe 0a 55 00 04 00 f4 05 eb b3 fb e6 ea f4 00 eb c8 fd f8 1c ff fb 27 ac fe ff 1b 36 04')
    # More than two eligible sources repeats the whole pass.
    require(0x9DB4F4,0x1B22,'04 68 fb 28 6c ff 02 00 5d fb 4d 1c 33 1b 1e 46 1b 07 08 00 4c 35 28 6c ff 02 00 5d fb 67 1c 46 1b 1e 6f 1a')
    # Graduate maximum is source upper bound minus one.
    require(0x9F651C,0x1CDB,'07 08 00 60 0a 28 6c ff 01 00 fb 9c cc fe fd e6 08 00 80 0a')
    # Graduate acceptance calls the shared employee copy helper.
    require(0x999858,0x0260,'05 06 00 28 5c ff 01 00 5d fb 33 1c 3d 04 05 01 00 04 6c ff 0a 11 00 08 00')
    # Exhibition attendance record starts Empty.
    require(0x9DD234,0x01A7,'fc 67 6c ff fd e6 08 00 bc 02')
    catalog=recover(data)
    if not all(len(x['ProductionBands'])==4 for x in catalog['Hardware']):raise ParseError('Incomplete production bands')
    return True

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('exe',type=Path)
    verify(parser.parse_args().exe.read_bytes());print('PASS: core-loop declarations, initial cells, sales cadence/fees and production bands.')
