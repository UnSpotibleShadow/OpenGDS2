#!/usr/bin/env python3
"""Extract employee definitions locally from the SHA-256 verified original.
No target execution; no database is distributed. See docs/employee-initializer.md.
"""
import argparse, hashlib, json, struct
from pathlib import Path
from inspect_vb6 import PeImage, ParseError
from trace_hiring_pcode import KNOWN_SHA256, OPS, parse_procedure, pool_index
OPS.update({'FEC1': (7, 'LitVarI4'), 'FDF0': (5, 'FMemStVarCopy'), 'FD86': (3, 'MemStVar')})
START=0x009F651C
POOL=0x0060DF28

def bstr(pe, index):
    address=pe.u32(POOL+index*4)
    size=pe.u32(address-4)
    if size>4096 or size%2: raise ParseError('Invalid BSTR length')
    return pe.bytes_at(address,size).decode('utf-16le')

def recover(data):
    if hashlib.sha256(data).hexdigest()!=KNOWN_SHA256: raise ParseError('Unsupported executable SHA-256; use extracted game, not SFX wrapper')
    pe=PeImage(data)
    instructions,_=parse_procedure(pe,START,0x250B)
    rows=[]; values={}; name_parts=[]; strings=[]; last_string=None
    minimum=None; opening_scalars={}
    for i in instructions:
        if i.mnemonic=='LitVarI4': values[struct.unpack_from('<h',i.raw,2)[0]]=struct.unpack_from('<i',i.raw,4)[0]
        elif i.mnemonic=='LitVarI2': values[struct.unpack_from('<h',i.raw,1)[0]]=struct.unpack_from('<h',i.raw,3)[0]
        elif i.mnemonic=='LitVarStr':
            value=bstr(pe,struct.unpack_from('<H',i.raw,3)[0]); values[struct.unpack_from('<h',i.raw,1)[0]]=value; strings.append(value); last_string=value
        elif i.mnemonic=='FStVarCopyObj': values[struct.unpack_from('<h',i.raw,1)[0]]=last_string
        elif i.mnemonic=='FMemStVar' and i.offset<0x124:
            opening_scalars[f'{int.from_bytes(i.raw[4:6],"little"):04X}']=values.get(-148)
        elif i.offset==0x1629: minimum=len(rows)
        elif i.mnemonic=='ImpAdCallFPR4' and pool_index(i)==0xBF:
            if len(strings)<2: raise ParseError('Missing name component pair')
            name_parts.append({'Given':strings[-2], 'Family':strings[-1]})
        elif i.mnemonic=='ImpAdCallFPR4' and pool_index(i)==0xCF:
            # Locals follow the decoded argument push sequence. The build hash and
            # final structure counts are mandatory; raw zero-backed PE globals are not data.
            raw_name=values[-324]
            rows.append({'SourceIndex':len(rows),'Name':raw_name if isinstance(raw_name,str) else None,
              'GeneratedName':raw_name==-1,'PortraitId':values[-308],
              'RawAttributes':[values[n] for n in [-548,-516,-484,-452,-420,-388,-340]],
              'Level':values[-612], 'ContractFee':values[-708],
              'RawParameter3C':values[-644], 'RawParameter34':values[-580], 'RawParameter40':values[-676],
              'DefinitionAddress':f'0x{START+i.offset:08X}',
              'InitialGate':0, 'InitialSpecialFlag':int(len(rows)>=49)})
    if len(rows)!=60 or minimum!=35 or len(name_parts)!=8:raise ParseError('Initializer shape changed')
    for row in rows:row['InitialTechnologyLevels']={str(n):0 for n in range(8)}
    for i in instructions:
        if i.mnemonic!='LitVarI2' or i.raw[1:3]!=bytes.fromhex('2c ff'):continue
        va=START+i.offset
        if pe.bytes_at(va+5,5)!=bytes.fromhex('07 08 00 60 0a'):continue
        if pe.bytes_at(va+10,12)!=bytes.fromhex('28 6c ff 01 00 fb 9c cc fe fc 22 f5') or pe.bytes_at(va+26,12)!=bytes.fromhex('07 08 00 58 0d a7 02 00 fd 86 00 00'):raise ParseError('Initial skill shape changed')
        level=struct.unpack('<h',i.raw[3:5])[0];technology=pe.u32(va+22)
        row=next(row for row in reversed(rows) if int(row['DefinitionAddress'],16)<va)
        if not 0<=technology<8 or not 1<=level<=3:raise ParseError('Initial skill outside supported table')
        row['InitialTechnologyLevels'][str(technology)]=level
    # Re-resolve the gate global through the known hiring pool and reset evidence.
    if pe.u32(0x60C264+0x33*4)!=0xA11CA0:raise ParseError('Gate map changed')
    return {'SchemaVersion':1,'SourceHash':KNOWN_SHA256,'MinimumSourceIndex':minimum,
      'SourceUpperBound':len(rows),'NameComponents':name_parts,'Employees':rows,
      'OpeningScalars':opening_scalars}

def main():
    ap=argparse.ArgumentParser(description=__doc__);ap.add_argument('exe',type=Path);ap.add_argument('--out',type=Path,required=True);a=ap.parse_args()
    result=recover(a.exe.read_bytes());a.out.parent.mkdir(parents=True,exist_ok=True)
    a.out.write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(f'Recovered {len(result["Employees"])} employee rows; minimum source index {result["MinimumSourceIndex"]}. Local output: {a.out}')
if __name__=='__main__':main()
