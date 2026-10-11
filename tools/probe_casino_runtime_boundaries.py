#!/usr/bin/env python3
"""M79 instruction probes. Requires unicorn==2.1.4; does not execute Windows.

Coordinates use the reference engine's x87 control word. Timer probes execute
its timer procedure but stub Windows APIs, event dispatch and generic handling.
No runtime binaries, Windows API implementations or game assets are bundled.
"""
import argparse
import hashlib
import json
import struct
from pathlib import Path
from inspect_vb6 import PeImage


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('runtime', type=Path)
    args = parser.parse_args()
    from unicorn import Uc, UC_ARCH_X86, UC_MODE_32, UC_HOOK_CODE
    from unicorn.x86_const import (UC_X86_REG_ESP, UC_X86_REG_EAX,
                                   UC_X86_REG_EIP, UC_X86_REG_FPCW)
    data = args.runtime.read_bytes()
    manifest = json.loads(Path(__file__).with_name('casino_runtime_boundaries.json').read_text())
    if hashlib.sha256(data).hexdigest() != manifest['runtime_sha256']:
        raise ValueError('Unsupported reference runtime')
    pe = PeImage(data)

    def image():
        uc = Uc(UC_ARCH_X86, UC_MODE_32)
        uc.mem_map(pe.image_base, 0x200000)
        for s in pe.sections:
            uc.mem_write(pe.image_base + s.rva,
                         data[s.raw_offset:s.raw_offset + s.raw_size])
        uc.mem_map(0x100000, 0x20000)
        uc.mem_map(0x200000, 0x1000)
        uc.reg_write(UC_X86_REG_FPCW, pe.u16(0x66102000))
        return uc

    cases = [(1.5, 2), (2.5, 2), (3.5, 4), (-1.5, -2), (-2.5, -2),
             (-3.5, -4), (1.499999999, 1), (1.500000001, 2),
             (2147483646.5, 2147483646)]
    for value, expected in cases:
        uc = image()
        uc.mem_write(0x200000, struct.pack('<H6xd', 5, value))
        uc.mem_write(0x110000, struct.pack('<II', 0x100000, 0x200000))
        uc.reg_write(UC_X86_REG_ESP, 0x110000)
        uc.emu_start(0x66002120, 0x100000, timeout=1000000, count=10000)
        raw = uc.reg_read(UC_X86_REG_EAX)
        actual = raw if raw < 0x80000000 else raw - 0x100000000
        assert actual == expected, (value, actual, expected)
        print('CI4_R8', value, actual)

    # Version check reads ABI revision, not build number. No DLL imports run.
    for build in [8988, 32767]:
        uc = image()
        uc.mem_write(0x200004, struct.pack('<H', build))
        uc.mem_write(0x200022, struct.pack('<H', 10))
        uc.mem_write(0x110000, struct.pack('<II', 0x100000, 0x200000))
        uc.reg_write(UC_X86_REG_ESP, 0x110000)
        uc.emu_start(0x6601AF2B, 0x100000, timeout=1000000, count=10000)
        assert uc.reg_read(UC_X86_REG_EAX) == 1
        print('ABI_ACCEPT', build, 10)

    uc = image()
    a, b = 0x200000, 0x200200
    hwnd_a, hwnd_b = 10, 11
    calls = []
    scenario = 'normal'
    uc.mem_write(0x660014EC, struct.pack('<I', 0x100500))
    uc.mem_write(0x660014E0, struct.pack('<I', 0x100600))
    for ctl, hwnd in [(a, hwnd_a), (b, hwnd_b)]:
        uc.mem_write(ctl + 0x3C, struct.pack('<I', hwnd))

    def nested_call(ctl, hwnd):
        code = b''
        for value in [0, 0, 0x113, hwnd, ctl]:
            code += b'\x68' + struct.pack('<I', value)
        return code + b'\xb8' + struct.pack('<I', 0x66030622) + b'\xff\xd0'

    # Synthetic handler sends A and B timer messages synchronously, then
    # returns to the real timer procedure. This is not a Windows message pump.
    uc.mem_write(0x100100, nested_call(a, hwnd_a) + nested_call(b, hwnd_b) +
                 b'\x31\xc0\xc2\x0c\x00')

    def finish(nbytes, result=0):
        sp = uc.reg_read(UC_X86_REG_ESP)
        return_address = struct.unpack('<I', uc.mem_read(sp, 4))[0]
        uc.reg_write(UC_X86_REG_EAX, result)
        uc.reg_write(UC_X86_REG_ESP, sp + 4 + nbytes)
        uc.reg_write(UC_X86_REG_EIP, return_address)

    def hook(_uc, address, _size, _user):
        sp = uc.reg_read(UC_X86_REG_ESP)
        if address == 0x100500:
            hwnd, timer_id = struct.unpack('<II', uc.mem_read(sp + 4, 8))
            calls.append(('kill', hwnd, timer_id))
            finish(8, 1)
        elif address == 0x100600:
            hwnd, timer_id, interval, callback = struct.unpack('<IIII', uc.mem_read(sp + 4, 16))
            calls.append(('set', hwnd, timer_id, interval, callback))
            finish(16, 1)
        elif address == 0x66003265:
            ctl = struct.unpack('<I', uc.mem_read(sp + 4, 4))[0]
            calls.append(('default', ctl))
            finish(20)
        elif address == 0x66005D6E:
            ctl = struct.unpack('<I', uc.mem_read(sp + 4, 4))[0]
            calls.append(('event', ctl))
            if ctl == a and scenario == 'nested':
                uc.reg_write(UC_X86_REG_EIP, 0x100100)
            elif scenario == 'disabled':
                # Model the final Enabled flag state; not its property setter.
                uc.mem_write(ctl + 0xD0, struct.pack('<I', 4))
                finish(12)
            elif scenario == 'unloaded':
                # Model event dispatch's nonzero invalidation result.
                finish(12, 1)
            else:
                finish(12)

    uc.hook_add(UC_HOOK_CODE, hook)

    def deliver():
        uc.mem_write(0x110000, struct.pack('<IIIIII', 0x100000, a, hwnd_a, 0x113, 0, 0))
        uc.reg_write(UC_X86_REG_ESP, 0x110000)
        uc.emu_start(0x66030622, 0x100000, timeout=1000000, count=10000)

    expected = {
        'normal': [('kill', 10, 10), ('event', a), ('set', 10, 10, 80, 0)],
        'nested': [('kill', 10, 10), ('event', a), ('default', a),
                   ('kill', 11, 11), ('event', b), ('set', 11, 11, 80, 0),
                   ('set', 10, 10, 80, 0)],
        'disabled': [('kill', 10, 10), ('event', a)],
        'unloaded': [('kill', 10, 10), ('event', a)],
    }
    for scenario in expected:
        calls.clear()
        for ctl in [a, b]:
            uc.mem_write(ctl + 0xD0, struct.pack('<II', 3, 80))
        deliver()
        assert calls == expected[scenario], (scenario, calls)
        print('TIMER', scenario, calls)
    # Deliver a later queued message to a disabled but surviving control.
    scenario = 'disabled'
    for ctl in [a, b]:
        uc.mem_write(ctl + 0xD0, struct.pack('<II', 3, 80))
    deliver()
    calls.clear()
    scenario = 'late_disabled'
    deliver()
    assert calls == [('event', a)], calls
    print('TIMER late_disabled', calls)
    print('PASS: M79 instruction probes; Windows/event APIs stubbed; no native UI acceptance')


if __name__ == '__main__':
    main()
