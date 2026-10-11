import sys,struct,json,argparse,hashlib
from pathlib import Path
# Optional dependency: unicorn==2.1.4; isolated instruction check only.
parser=argparse.ArgumentParser(description='M78 reference RNG instruction probe, not Windows acceptance')
parser.add_argument('runtime',type=Path)
args=parser.parse_args()
from inspect_vb6 import PeImage
from unicorn import Uc,UC_ARCH_X86,UC_MODE_32,UC_HOOK_CODE
from unicorn.x86_const import UC_X86_REG_ESP,UC_X86_REG_EIP,UC_X86_REG_EAX
b=args.runtime.read_bytes()
manifest=json.loads(Path(__file__).with_name('casino_fidelity.json').read_text())
if hashlib.sha256(b).hexdigest()!=manifest['runtime']['sha256']:raise ValueError('Unsupported reference runtime')
p=PeImage(b)
u=Uc(UC_ARCH_X86,UC_MODE_32);u.mem_map(0x66000000,0x200000)
# Map each initialized PE section at preferred VA; no Windows loader/import execution.
nt=struct.unpack_from('<I',b,0x3c)[0];ns=struct.unpack_from('<H',b,nt+6)[0];os=struct.unpack_from('<H',b,nt+20)[0]
for i in range(ns):
 o=nt+24+os+40*i; va,sz,ptr=struct.unpack_from('<III',b,o+12);u.mem_write(0x66000000+va,b[ptr:ptr+sz])
u.mem_map(0x100000,0x20000);u.mem_map(0x200000,0x1000)
context=0x200000;stop=0x100000
# Replace only project-context lookup, not RNG instructions.
def hook(uc,address,size,user):
 if address==0x66001d94:
  sp=uc.reg_read(UC_X86_REG_ESP);ra=struct.unpack('<I',uc.mem_read(sp,4))[0]
  uc.reg_write(UC_X86_REG_EAX,context);uc.reg_write(UC_X86_REG_ESP,sp+4);uc.reg_write(UC_X86_REG_EIP,ra)
u.hook_add(UC_HOOK_CODE,hook)
u.mem_write(context+4,struct.pack('<I',0x50000))
def rnd(number):
 sp=0x110000;u.mem_write(sp,struct.pack('<If',stop,number));u.reg_write(UC_X86_REG_ESP,sp)
 u.emu_start(0x6606225d,stop,timeout=1000000,count=10000)
 return struct.unpack('<I',u.mem_read(context+4,4))[0]
rows=[];expected=0x50000
for i in range(12):
 expected=(expected*0x43fd43fd+0xc39ec3)&0xffffff;actual=rnd(1)
 assert actual==expected,(actual,expected);rows.append({'step':i+1,'seed':actual,'value':actual/16777216})
assert rnd(0)==expected
bits=struct.unpack('<I',struct.pack('<f',-1))[0];neg=(((bits+(bits>>24))&0xffffff)*0x43fd43fd+0xc39ec3)&0xffffff
assert rnd(-1)==neg
print(json.dumps({'status':'PASS','emulator':'Unicorn 2.1.4','scope':'reference rtRandomNext instructions; project lookup stubbed; Windows not executed','default_stream':rows,'rnd_zero_keeps_seed':True,'negative_one_seed':neg},indent=2))

for seed,number in [(0x50000,42.0),(0x123456,-42.0),(0x123457,42.0)]:
 u.mem_write(context+4,struct.pack('<I',seed));sp=0x110000
 u.mem_write(sp,struct.pack('<Id',stop,number));u.reg_write(UC_X86_REG_ESP,sp)
 u.emu_start(0x660ea89f,stop,timeout=1000000,count=10000)
 bits=struct.unpack('<II',struct.pack('<d',number))[1]
 expected=(seed&0xff0000ff)|(((bits&0xffff)^(bits>>16))<<8)
 actual=struct.unpack('<I',u.mem_read(context+4,4))[0];assert actual==expected
 print('RANDOMIZE_NUMBER',seed,number,actual)

for seed,timer in [(0x50000,0.0),(0x123456,43210.123),(0x123457,86399.999)]:
 u.mem_write(context+4,struct.pack('<I',seed));u.mem_write(0x200100,struct.pack('<f',timer))
 # Temporary in-memory GetTimer stub returns the controlled Single in ST(0).
 u.mem_write(0x6606030a,b'\xd9\x05'+struct.pack('<I',0x200100)+b'\xc3')
 sp=0x110000;u.mem_write(sp,struct.pack('<I',stop));u.reg_write(UC_X86_REG_ESP,sp)
 u.emu_start(0x66056179,stop,timeout=1000000,count=10000)
 bits=struct.unpack('<I',struct.pack('<f',timer))[0]
 expected=(seed&0xff0000ff)|(((bits&0xffff)^(bits>>16))<<8)
 actual=struct.unpack('<I',u.mem_read(context+4,4))[0];assert actual==expected
 print('RANDOMIZE_TIMER_BITS',seed,timer,actual)
