from pathlib import Path
import struct, datetime, json, sys
b=Path(sys.argv[1]).read_bytes()
pos=0; out=[]
while pos+26<=len(b):
    total=struct.unpack_from('<H',b,pos)[0]
    if total==0: break
    method=b[pos+2:pos+7]
    packed,orig,ts=struct.unpack_from('<III',b,pos+7)
    attr=b[pos+19]; lvl=b[pos+20]; crc=struct.unpack_from('<H',b,pos+21)[0]; osid=b[pos+23]
    nextsz=struct.unpack_from('<H',b,pos+24)[0]
    if lvl!=2:
        raise SystemExit(f'unsupported lvl {lvl} at {pos}')
    q=pos+26; ext_total=0; name=b''; dirname=b''; exts=[]
    hs=nextsz
    while hs:
        chunk=b[q:q+hs]
        if len(chunk)!=hs: raise SystemExit('truncated ext')
        et=chunk[0]
        payload=chunk[1:-2]
        exts.append((et,hs,payload))
        if et==1: name=payload
        elif et==2: dirname=payload
        hs=struct.unpack_from('<H',chunk,len(chunk)-2)[0]
        q+=len(chunk); ext_total += len(chunk)
    full=dirname+name
    try: fn=full.decode('cp932').replace('\xff','/')
    except: fn=repr(full)
    dataoff=pos+total
    try: dt=datetime.datetime.fromtimestamp(ts).isoformat(sep=' ')
    except: dt=str(ts)
    out.append(dict(index=len(out),offset=pos,header_size=total,method=method.decode('ascii','replace'),packed=packed,original=orig,timestamp=ts,datetime=dt,attr=attr,level=lvl,crc=crc,osid=osid,filename=fn,data_offset=dataoff,exts=[{'type':et,'size':hs,'payload_hex':pl[:32].hex()} for et,hs,pl in exts]))
    pos=dataoff+packed
print(json.dumps(out,ensure_ascii=False,indent=2))
print('ENDPOS',pos,'SIZE',len(b),file=sys.stderr)
