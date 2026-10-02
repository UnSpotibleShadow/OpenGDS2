from pathlib import Path
import struct, sys, os

DICBIT=13
NP=DICBIT+1
NT=16+3
PBIT=4
TBIT=5
MAXMATCH=256
THRESHOLD=3
NC=255+MAXMATCH+2-THRESHOLD  # 510
CBIT=9

class Bits:
    def __init__(self,data): self.data=data; self.pos=0
    def get(self,n):
        v=0
        for _ in range(n):
            bytepos=self.pos>>3
            if bytepos>=len(self.data):
                bit=0
            else:
                bit=(self.data[bytepos]>>(7-(self.pos&7)))&1
            v=(v<<1)|bit; self.pos+=1
        return v
    def peek(self,n):
        p=self.pos; v=self.get(n); self.pos=p; return v

class Huff:
    def __init__(self,lengths=None,const=None):
        self.const=const
        self.maps={}
        if const is not None: return
        if lengths is None: return
        maxlen=max(lengths) if lengths else 0
        counts=[0]*(maxlen+1)
        for l in lengths:
            if l: counts[l]+=1
        code=0; nextcode=[0]*(maxlen+1)
        for bits in range(1,maxlen+1):
            code=(code+counts[bits-1])<<1
            nextcode[bits]=code
        for sym,l in enumerate(lengths):
            if l:
                c=nextcode[l]; nextcode[l]+=1
                self.maps[(l,c)]=sym
        self.maxlen=maxlen
    def decode(self,b):
        if self.const is not None: return self.const
        c=0
        for l in range(1,self.maxlen+1):
            c=(c<<1)|b.get(1)
            s=self.maps.get((l,c))
            if s is not None: return s
        raise ValueError('bad huffman code')

def read_pt_len(b, nn, nbit, i_special):
    n=b.get(nbit)
    lens=[0]*nn
    if n==0:
        return Huff(const=b.get(nbit)), lens
    i=0
    while i<n and i<nn:
        c=b.get(3)
        if c==7:
            while b.get(1): c+=1
        lens[i]=c; i+=1
        if i==i_special:
            c=b.get(2)
            while c>0 and i<nn:
                lens[i]=0; i+=1; c-=1
    return Huff(lens), lens

def read_c_len(b, pth):
    n=b.get(CBIT)
    lens=[0]*NC
    if n==0:
        return Huff(const=b.get(CBIT)), lens
    i=0
    while i<n and i<NC:
        c=pth.decode(b)
        if c<=2:
            if c==0: z=1
            elif c==1: z=b.get(4)+3
            else: z=b.get(CBIT)+20
            for _ in range(z):
                if i>=NC: break
                lens[i]=0; i+=1
        else:
            lens[i]=c-2; i+=1
    return Huff(lens), lens

def lh5_decompress(data, outsize):
    b=Bits(data); out=bytearray(); block=0; ch=None; ph=None
    while len(out)<outsize:
        if block==0:
            block=b.get(16)
            pth,_=read_pt_len(b,NT,TBIT,3)
            ch,_=read_c_len(b,pth)
            ph,_=read_pt_len(b,NP,PBIT,-1)
            if block==0:
                raise ValueError('zero block')
        block-=1
        c=ch.decode(b)
        if c<=255:
            out.append(c)
        else:
            length=c-(256-THRESHOLD)
            j=ph.decode(b)
            if j!=0: j=(1<<(j-1))+b.get(j-1)
            dist=j+1
            for _ in range(length):
                if len(out)>=outsize: break
                if dist>len(out):
                    # LHA dictionaries are initially spaces in some variants; LH5 streams normally avoid prehistory.
                    out.append(0x20)
                else:
                    out.append(out[-dist])
    return bytes(out)

def entries(path):
    raw=Path(path).read_bytes(); pos=0
    while pos+26<=len(raw):
        total=struct.unpack_from('<H',raw,pos)[0]
        if total==0: break
        method=raw[pos+2:pos+7]; packed,orig=struct.unpack_from('<II',raw,pos+7); lvl=raw[pos+20]
        if lvl!=2: raise ValueError('level')
        hs=struct.unpack_from('<H',raw,pos+24)[0]; q=pos+26; name=b''; dirname=b''
        while hs:
            chunk=raw[q:q+hs]; et=chunk[0]; payload=chunk[1:-2]
            if et==1:name=payload
            elif et==2:dirname=payload
            hs=struct.unpack_from('<H',chunk,len(chunk)-2)[0]; q+=len(chunk)
        full=(dirname+name).replace(b'\xff',b'/')
        fn=full.decode('cp932','replace')
        dataoff=pos+total
        yield raw,fn,method,packed,orig,dataoff
        pos=dataoff+packed

def main():
    arc=sys.argv[1]; outdir=Path(sys.argv[2]); outdir.mkdir(parents=True,exist_ok=True)
    only=sys.argv[3] if len(sys.argv)>3 else None
    for raw,fn,method,packed,orig,off in entries(arc):
        if only and fn!=only: continue
        if method==b'-lhd-':
            (outdir/fn).mkdir(parents=True,exist_ok=True); continue
        comp=raw[off:off+packed]
        if method==b'-lh0-': dec=comp
        elif method==b'-lh5-': dec=lh5_decompress(comp,orig)
        else: print('skip',method,fn); continue
        p=outdir/fn; p.parent.mkdir(parents=True,exist_ok=True); p.write_bytes(dec)
        print(fn,len(dec))
if __name__=='__main__': main()
