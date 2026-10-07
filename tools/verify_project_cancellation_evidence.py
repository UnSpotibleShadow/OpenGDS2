#!/usr/bin/env python3
"""Verify M68 cancellation evidence locally; never execute or redistribute the original."""
import argparse
import hashlib
from pathlib import Path

from inspect_vb6 import PeImage
from trace_hiring_pcode import KNOWN_SHA256


def verify(path: Path) -> None:
    original = path.read_bytes()
    if hashlib.sha256(original).hexdigest() != KNOWN_SHA256:
        raise ValueError("Unsupported original executable.")
    pe = PeImage(original)
    # Full-handler fingerprints establish that the locally inspected paths have
    # not changed. Only small transaction anchors and conclusions are published.
    handlers = [
        (0x009AAA8C, 2108, 0x005FEE38,
         "048a6b63daecd62c59f8df20a69286809547b19870e3e051a5f625abe515373d",
         [4, 0x12, 0x13, 0x14, 0x17, 0x23, 0x27],
         {0x0191:"f406c61c3808", 0x0200:"0512002868ff00005dfb331c3808",
          0x0213:"fdb60400", 0x0236:"051400fc96", 0x06F3:"2868ff1400fb9458ff",
          0x0720:"2868ff63005dfb741c4007", 0x081F:"05270052"}),
        (0x009AA1B8, 2076, 0x005FE5FC,
         "b931c1129ba1be1ea6f8e590806e248bca8dfeadf4c92f8c946647aac8377f56",
         [4, 0x10, 0x11, 0x12, 0x15, 0x21, 0x25],
         {0x0171:"f406c61c1808", 0x01E0:"0510002868ff00005dfb331c1808",
          0x01EE:"2868ff0100fdb60400", 0x0216:"051200fc96", 0x06D3:"2868ff1400fb9458ff",
          0x0700:"2868ff63005dfb741c2007", 0x07FF:"05250052"}),
    ]
    globals_expected = [0xA14384, 0xA14394, 0xA1358C, 0xA13770, 0xA11E38, 0xA11C84, 0xA1270C]
    for address, size, pool, fingerprint, indices, anchors in handlers:
        body = pe.bytes_at(address, size)
        if hashlib.sha256(body).hexdigest() != fingerprint:
            raise ValueError(f"Changed cancellation handler {address:08X}")
        if [pe.u32(pool + 4 * index) for index in indices] != globals_expected:
            raise ValueError("Changed cancellation global mapping")
        for offset, hex_bytes in anchors.items():
            expected = bytes.fromhex(hex_bytes)
            if body[offset:offset + len(expected)] != expected:
                raise ValueError(f"Changed transaction anchor {address:08X}+{offset:04X}")
        print(f"PASS room handler {address:08X}: confirmation, assigned action, +20, cap 99, empty room")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("executable", type=Path)
    verify(parser.parse_args().executable)
