#!/usr/bin/env python3
"""Extract the 18 genre records locally; unidentified columns stay structural."""
import argparse, hashlib, json, struct
from pathlib import Path
from inspect_vb6 import PeImage, ParseError
from trace_hiring_pcode import KNOWN_SHA256
from recover_employee_database import bstr, START

def recover(data):
    if hashlib.sha256(data).hexdigest()!=KNOWN_SHA256:raise ParseError('Unsupported executable SHA-256')
    pe=PeImage(data); rows=[]
    def require(va, value):
        if pe.bytes_at(va,len(value))!=value:raise ParseError(f'Catalog pattern changed at {va:08X}')
    # Seven direct setter arguments, with no derived arithmetic in this helper.
    require(0x981EA8,bytes.fromhex('fd 16 10 00 5c ff 07 08 00 a0 11 fc 22 07 08 00 40 12 fc b0'))
    for index in range(18):
        va=START+0x6331+index*0x4C
        require(va,bytes.fromhex('fe c1 1c fe'))
        fields={'1224':struct.unpack('<i',pe.bytes_at(va+4,4))[0]}
        for off,local,member in [(8,0xFE3C,'1208'),(13,0xFE5C,'11EC'),(18,0xFE7C,'11D0'),(34,0xFEBC,'125C'),(39,0xFECC,'1240')]:
            require(va+off,b'\x28'+struct.pack('<H',local))
            fields[member]=struct.unpack('<h',pe.bytes_at(va+off+3,2))[0]
        require(va+23,bytes.fromhex('3a 1c ff'))
        name=bstr(pe,pe.u16(va+26))
        require(va+28,bytes.fromhex('4e ac fe 04 ac fe'))
        require(va+44,bytes.fromhex('04 fc fd 0a 35 01 20 00'))
        rows.append({'SourceIndex':index,'Name':name,'RawMembers':fields,'DefinitionAddress':f'0x{va+47:08X}'})
    audiences=[]
    helper=0x981DCC
    for index,member in enumerate([0x14A8,0x1400,0x141C,0x1438,0x1454,0x148C,0x1470]):
        require(helper+index*0x14,bytes.fromhex('fd 16')+struct.pack('<H',0x10+index*4)+bytes.fromhex('5c ff 07 08 00 ec 13 fc 22 07 08 00')+struct.pack('<H',member)+bytes.fromhex('fc b0'))
    for index in range(7):
        va=START+0x9B3E+index*0x52
        require(va,bytes.fromhex('3a 2c fe'))
        description=bstr(pe,pe.u16(va+3))
        require(va+5,bytes.fromhex('4e 1c fe 04 1c fe fe c1 3c fe'))
        fields={'148C':struct.unpack('<i',pe.bytes_at(va+15,4))[0]}
        for off,local,member in [(19,0xFE5C,'1454'),(24,0xFE7C,'1438'),(29,0xFEAC,'141C'),(45,0xFECC,'14A8')]:
            require(va+off,b'\x28'+struct.pack('<H',local))
            fields[member]=struct.unpack('<h',pe.bytes_at(va+off+3,2))[0]
        require(va+34,bytes.fromhex('3a 2c ff'))
        name=bstr(pe,pe.u16(va+37))
        require(va+39,bytes.fromhex('4e bc fe 04 bc fe'))
        require(va+50,bytes.fromhex('04 fc fd 0a 25 02 20 00'))
        audiences.append({'SourceIndex':index,'Name':name,'Description':description,'RawMembers':fields,'DefinitionAddress':f'0x{va+53:08X}'})
    computers=[]
    for index in range(4):
        va=START+0xA332+index*0x51
        require(va,bytes.fromhex('3a 4c fe'))
        description=bstr(pe,pe.u16(va+3))
        require(va+5,bytes.fromhex('4e 3c fe 04 3c fe fe c1 5c fe'))
        price=struct.unpack('<i',pe.bytes_at(va+15,4))[0]
        require(va+19,bytes.fromhex('28 7c fe'))
        bits=struct.unpack('<h',pe.bytes_at(va+22,2))[0]
        require(va+24,bytes.fromhex('3a 1c ff'))
        label=bstr(pe,pe.u16(va+27))
        require(va+29,bytes.fromhex('4e ac fe 04 ac fe 3a 2c ff'))
        name=bstr(pe,pe.u16(va+38))
        require(va+40,bytes.fromhex('4e bc fe 04 bc fe 28 cc fe'))
        identifier=struct.unpack('<h',pe.bytes_at(va+49,2))[0]
        require(va+51,bytes.fromhex('04 1c fe 0a 49 02 1c 00'))
        if identifier!=index:raise ParseError('Computer index changed')
        computers.append({'SourceIndex':index,'Name':name,'Description':description,'BitLabel':label,'BitWidth':bits,'PurchasePrice':price})
    hardware=[]
    calls=[0x54F3,0x5644,0x5795,0x5905,0x5A58,0x5BA9,0x5CFA,0x5E4B,0x5F9C,0x60ED,0x623E]
    members=['1034','0F70','0FFC','0FE0','0F54','0F38','1050','0F1C','0FC4','0F00','0FA8','0EE4','0EC8']
    # Verify all setter fields, including columns not yet imported/semantically mapped.
    helper=0x9874E8
    for index,member in enumerate([0xEC8,0xEE4,0xFA8,0xF00,0xFC4,0xF1C,0x1050,0xF38,0xF54,0xFE0,0xFFC,0xF70,0x1034,0x1088,0x10A4,0x10C0]):
        require(helper+index*0x14,bytes.fromhex('fd 16')+struct.pack('<H',0x10+index*4)+bytes.fromhex('5c ff 07 08 00 94 0e fc 22 07 08 00')+struct.pack('<H',member)+bytes.fromhex('fc b0'))
    for index,call in enumerate(calls):
        pos=START+call-0x59;fields={};strings={}
        for member in members:
            if member in ['1034','0EE4']:
                require(pos,b'\x3a');strings[member]=bstr(pe,pe.u16(pos+3))
                require(pos+5,b'\x4e');local=pe.u16(pos+6)
                require(pos+8,b'\x04'+struct.pack('<H',local));pos+=11
            elif member in ['0F70','0FFC','0FC4']:
                require(pos,b'\xfe\xc1');fields[member]=struct.unpack('<i',pe.bytes_at(pos+4,4))[0];pos+=8
            else:
                require(pos,b'\x28');fields[member]=struct.unpack('<h',pe.bytes_at(pos+3,2))[0];pos+=5
        require(pos,b'\x04');require(pos+3,bytes.fromhex('0a 0c 01 44 00'))
        hardware.append({'SourceIndex':index,'Name':strings['0EE4'],'Description':strings['1034'],'RawMembers':fields})
    game_types=[]
    for index,member in enumerate([0x1394,0x13B0,0x12B4,0x12D0,0x12EC,0x1308,0x1378,0x1324,0x1340,0x135C]):
        require(0x9836A4+index*0x14,bytes.fromhex('fd 16')+struct.pack('<H',0x10+index*4)+bytes.fromhex('5c ff 07 08 00 a0 12 fc 22 07 08 00')+struct.pack('<H',member)+bytes.fromhex('fc b0'))
    for column in range(15):
        argument=0x4C if column==14 else 0x10+column*4
        require(0x98F178+column*0x27,bytes.fromhex('fd 16')+struct.pack('<H',argument)+
            bytes.fromhex('3c ff 07 08 00 a0 12 28 5c ff 01 00 fb 9c 4c ff fc 22 f5')+struct.pack('<i',column)+
            bytes.fromhex('07 08 00 cc 13 a7 02 00 fd 90 00 00'))
    for index in range(51):
        pos=START+0x6889+index*0xF5;fields={};strings={}
        for member in ['135C','1340','1324','1378','1308','12EC','12D0','12B4','13B0','1394']:
            if member in ['135C','1340','1324','12B4']:
                require(pos,b'\x3a');strings[member]=bstr(pe,pe.u16(pos+3))
                require(pos+5,b'\x4e');local=pe.u16(pos+6)
                require(pos+8,b'\x04'+struct.pack('<H',local));pos+=11
            elif member=='1378':
                require(pos,b'\xfe\xc1');fields[member]=struct.unpack('<i',pe.bytes_at(pos+4,4))[0];pos+=8
            else:
                require(pos,b'\x28');fields[member]=struct.unpack('<h',pe.bytes_at(pos+3,2))[0];pos+=5
        require(pos,b'\x04');require(pos+3,bytes.fromhex('0a 4b 01 2c 00'))
        pos=START+0x68FC+index*0xF5;table=[]
        for column in range(16):
            require(pos,b'\x28');table.append(struct.unpack('<h',pe.bytes_at(pos+3,2))[0]);pos+=5
        require(pos,b'\x04');require(pos+3,bytes.fromhex('0a 4c 01 44 00'))
        arguments=list(reversed(table))
        game_types.append({'SourceIndex':index,'Name':strings['12B4'],'ExampleTitles':[strings[m] for m in ['1324','1340','135C']],
            'RawMembers':fields,'RawTable13CC':arguments[:14]+arguments[15:], 'RawUnusedArgument48':arguments[14]})
    return {'SchemaVersion':1,'SourceHash':KNOWN_SHA256,'Genres':rows,'Audiences':audiences,'DevelopmentComputers':computers,'Hardware':hardware,'GameTypes':game_types}

if __name__=='__main__':
    ap=argparse.ArgumentParser(description=__doc__);ap.add_argument('exe',type=Path);ap.add_argument('--out',required=True,type=Path);a=ap.parse_args()
    data=recover(a.exe.read_bytes());a.out.parent.mkdir(parents=True,exist_ok=True);a.out.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n')
    print('Recovered genres, audiences, computers, hardware and 51 game types into local private data.')
