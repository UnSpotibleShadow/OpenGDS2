#!/usr/bin/env python3
"""Asset-free M78 analytic helpers, not a replacement game/runtime.

Race values use Python binary64; VB6 VARIANT and native Windows acceptance
remain separate. Timer scheduling, controls and cash Format$ are not simulated.
"""
import json
import struct
from pathlib import Path

MASK = 0xFFFFFF


def rnd_step(seed):
    seed = (seed * 0x43FD43FD + 0xC39EC3) & MASK
    return seed, seed / 16777216


def randomize_bits(seed, bits):
    return (seed & 0xFF0000FF) | (((bits & 0xFFFF) ^ (bits >> 16)) << 8)


def randomize_number(seed, number):
    bits = struct.unpack('<II', struct.pack('<d', number))[1]
    return randomize_bits(seed, bits)


def phase2_delta(tick, mode, slot, performance, camera_step):
    """Apply *all* matching blocks, including both at tick 290, in source order."""
    m = json.loads(Path(__file__).with_name('casino_fidelity.json').read_text())
    rows = m['movement']
    windows = [(200, 230, 0x1429, 0x1429),
               (231, 260, 0x14DA, 0x1C67),
               (261, 290, 0x1D23, 0x24B8),
               (290, 330, 0x2549, 0x2549),
               (331, 360, 0x25FD, 0x2D9A),
               (361, float('inf'), 0x2E41, 0x35C6)]
    dx = dz = 0.0
    blocks = 0
    for low, high, first, last in windows:
        if low <= tick <= high:
            candidates = [r for r in rows if first <= r['store_offset'] <= last]
            row = candidates[0] if first == last else candidates[mode * 8 + slot]
            delta = performance / row['denominator']
            if row['sign'] == '-':
                delta = -delta
            dx += delta - (0 if first == 0x1429 else camera_step)
            dz += delta - 10
            blocks += 1
    return dx, dz, blocks


def self_check():
    # Published VB6 default first value: 0.7055475 (rounded decimal).
    seed, value = rnd_step(0x50000)
    assert seed == 11837123 and abs(value - 0.7055475) < 1e-7
    assert rnd_step(0)[0] == 0xC39EC3
    # Randomize keeps the low seed byte, so equal numeric arguments alone do
    # not erase the preceding stream. This is a parity requirement, not a fix.
    assert randomize_number(0x123456, 42) & 255 == 0x56
    assert randomize_number(0x123457, 42) != randomize_number(0x123456, 42)
    assert phase2_delta(290, 0, 0, 72, 0) == (-26.0, -46.0, 2)
    assert phase2_delta(290, 0, 0, 72, 10) == (-46.0, -46.0, 2)
    assert phase2_delta(289, 0, 0, 72, 0) == (-8.0, -18.0, 1)
    assert phase2_delta(291, 0, 0, 72, 0) == (-18.0, -28.0, 1)
    # These two mode-1 coefficients differ from the preceding windows.
    assert phase2_delta(231, 1, 6, 132, 0) == (-11.0, -21.0, 1)
    assert phase2_delta(231, 1, 7, 132, 0) == (12.0, 2.0, 1)
    print('PASS: M78 analytic vectors (not native runtime/UI acceptance)')


if __name__ == '__main__':
    self_check()
