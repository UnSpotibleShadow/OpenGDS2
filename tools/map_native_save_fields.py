#!/usr/bin/env python3
"""Pair all native save/load record member references; never import company state."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import struct
import map_original_game as mapping
from inspect_vb6 import inspect,PeImage


def build(executable):
    source=json.loads((mapping.ROOT/'tools/native_save_evidence.json').read_text());data=executable.read_bytes()
    if hashlib.sha256(data).hexdigest()!=source['source_sha256']:raise ValueError('Unsupported original executable')
    structure=mapping.read_json(mapping.STRUCTURE)
    mapping.pcode.OPS.update({k:tuple(v) for k,v in structure['opcode_schema'].items()})
    metadata=inspect(executable);obj=next(o for o in metadata['objects'] if o['name']=='Module1');pe=PeImage(data)
    sides=[]
    for procedure in source['procedures']:
        descriptor=next(p for p in obj['procedures'] if int(p.get('start','0'),16)==procedure['address'])
        raw,instructions,_=mapping.decode(pe,obj,descriptor)
        if hashlib.sha256(raw).hexdigest()!=procedure['sha256']:raise ValueError('Changed native persistence procedure')
        arities={int(r['relative'],16):r['values'] for r in procedure['records']};members=[];records=[];loops=[]
        for instruction in instructions:
            if instruction.mnemonic=='OpenFile':members=[]
            elif instruction.mnemonic=='FMemLdRf':
                members.append(dict(member=f'{struct.unpack_from("<H",instruction.raw,len(instruction.raw)-2)[0]:04X}',array_rank=0))
            elif (rank:=re.match(r'Ary([123])Ld',instruction.mnemonic)) and members:
                members[-1]['array_rank']=int(rank[1])
            if instruction.mnemonic.startswith('For'):
                loops.append(dict(start=instruction.offset,end=struct.unpack_from('<H',instruction.raw,len(instruction.raw)-2)[0],
                                  opcode=instruction.mnemonic,cardinality='requires verified bound-expression/state evaluation'))
            if instruction.offset in arities:
                arity=arities[instruction.offset]
                fields=list(reversed(members[-arity:])) if records else [dict(member='header-local',array_rank=0)]
                if len(fields)!=arity:raise ValueError('Unresolved record member references')
                records.append(dict(at=instruction.offset,arity=arity,fields=fields,
                                    enclosing_loops=[loop['start'] for loop in loops if loop['start']<instruction.offset<loop['end']]))
                members=[]
        sides.append(dict(procedure=procedure['name'],records=records,loops=loops))
    if len(sides[0]['records'])!=130 or len(sides[1]['records'])!=130:raise ValueError('Incomplete record template inventory')
    pairs=[]
    for index,(writer,reader) in enumerate(zip(sides[0]['records'],sides[1]['records'])):
        if writer['arity']!=reader['arity'] or writer['fields']!=reader['fields']:raise ValueError('Save/load field order or array rank differs')
        pairs.append(dict(template=index,fields=writer['fields'],arity=writer['arity'],write_at=writer['at'],read_at=reader['at'],
                          write_loops=writer['enclosing_loops'],read_loops=reader['enclosing_loops']))
    return dict(source_sha256=source['source_sha256'],record_templates=len(pairs),field_slots=sum(r['arity'] for r in pairs),
                distinct_members=len({f['member'] for r in pairs for f in r['fields'] if f['member']!='header-local'}),
                loops=sides[0]['loops'],records=pairs,
                limits=['Source-order member references and array ranks are paired, not native record values or modern state meanings.',
                        'Loop bounds/cardinalities, Variant subtypes, native encoding and a genuine GD2 fixture still require validation.',
                        'This tool does not load, export or certify interoperability with GD2 saves.'])


def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('executable',type=Path);parser.add_argument('--write',type=Path)
    args=parser.parse_args();report=build(args.executable)
    if args.write:args.write.write_text(json.dumps(report,indent=2)+'\n')
    print(f"PASS native persistence fields: {report['record_templates']} paired templates, {report['field_slots']} field slots, {report['distinct_members']} distinct members; GD2 interoperability pending")


if __name__=='__main__':main()
