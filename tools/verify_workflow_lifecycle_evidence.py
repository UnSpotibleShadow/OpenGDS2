#!/usr/bin/env python3
"""Check bounded M76 source evidence without executing or emitting original content.

Fingerprints preserve the procedures reviewed privately. Numeric anchors validate
key interpretations; this does not independently prove feature absence or runtime
event behavior, and is not an OpenGDS2 gameplay test.
"""
import argparse
import hashlib
import json
import struct
from pathlib import Path

from inspect_vb6 import PeImage, ParseError


def require(condition, label):
    if not condition:
        raise ParseError("Changed evidence: " + label)


def verify(data):
    manifest = json.loads(Path(__file__).with_name("workflow_lifecycle_evidence.json").read_text())
    require(hashlib.sha256(data).hexdigest() == manifest["source_sha256"], "supported executable")
    pe = PeImage(data)
    for name, span in manifest["spans"].items():
        require(hashlib.sha256(pe.bytes_at(span["address"], span["size"])).hexdigest()
                == span["sha256"], name + " fingerprint")

    def raw(address, expected, label):
        require(pe.bytes_at(address, len(expected)) == expected, label)

    def i2(address, value, label):
        b = pe.bytes_at(address, 5)
        require(b[0] == 0x28 and struct.unpack_from("<h", b, 3)[0] == value, label)

    def i4(address, value, label):
        b = pe.bytes_at(address, 8)
        require(b[:2] == b"\xfe\xc1" and struct.unpack_from("<i", b, 4)[0] == value, label)

    # Each quotation dimension uses a separate additive branch.
    for off, value in [(0x1CD, 200_000_000), (0x1EE, 100_000_000),
                       (0x20F, 200_000_000), (0x230, 100_000_000)]:
        i4(0x991BE8 + off, value, "hardware quote")
    for off, value in [(0x15D, 0), (0x16B, 2500), (0x179, 0),
                       (0x187, 800), (0x195, 800), (0x1A3, 800),
                       (0x1B1, 800), (0x221, 2)]:
        i2(0x990B70 + off, value, "hardware acceptance initialization")
    for index, address in [(0x07, 0xA1270C), (0x12, 0xA12744), (0x13, 0xA12760),
                           (0x14, 0xA12808), (0x15, 0xA12824),
                           (0x16, 0xA12840), (0x17, 0xA1285C)]:
        require(pe.u32(0x5F8BB0 + index * 4) == address, "hardware acceptance cell")
    # Completion requires hardware state and four independently applied flags.
    i2(0xA0283C + 0xAE30, 2, "hardware completion state")
    for off in [0xAECF, 0xAEE3, 0xAEFB, 0xAF13]:
        i2(0xA0283C + off, 1, "hardware technology completion flag")
    raw(0xA0283C + 0xAF89, bytes.fromhex("24d300"), "hardware launch form creation")
    for off, control in [(0xD4AD, 0x304), (0xD4C6, 0x308),
                         (0xD4DF, 0x360), (0xD4F8, 0x3B4),
                         (0xD511, 0x3D8), (0xD52A, 0x3E8), (0xD543, 0x350)]:
        raw(0xA0283C + off, b"\x0f" + struct.pack("<H", control), "failure UI control")
    raw(0xA0283C + 0x565F, bytes.fromhex("1e9856"), "speed branch skips eligibility stores")
    i2(0xA0283C + 0x5656, 10000, "speed threshold")
    raw(0xA0283C + 0x5688, bytes.fromhex("fcf65cfb"), "eligibility store zero path")
    raw(0xA0283C + 0x5694, bytes.fromhex("fcf65cfb"), "eligibility store one path")
    raw(0xA0283C + 0x56C3, bytes.fromhex("045cfb"), "retained eligibility read")
    raw(0x9A98E8 + 0x6AF, bytes.fromhex("044cfd"), "stress relief unassigned local read")
    require(struct.unpack("<H", pe.bytes_at(0x9AA128 + 6, 2))[0] == 560,
            "stress relief frame size")
    raw(0x9AA128 + 0x34, bytes.fromhex("4cfd0200"), "stress relief local cleanup entry")
    raw(0x99CA4C + 0xC1, bytes.fromhex("2704ff0a17000400"), "startup Randomize omitted argument")

    # Follow the launch call's reverse-pushed argument positions through the
    # setter. This prevents accidental shifts of market/scale/points/cost.
    setter_members = [0xEC8, 0xEE4, 0xFA8, 0xF00, 0xFC4, 0xF1C, 0x1050, 0xF38,
                      0xF54, 0xFE0, 0xFFC, 0xF70, 0x1034, 0x1088, 0x10A4, 0x10C0]
    for n, member in enumerate(setter_members):
        raw(0x9874E8 + n * 0x14, bytes.fromhex("fd16") + struct.pack("<H", 0x10 + n * 4),
            "hardware setter argument")
        raw(0x9874E8 + n * 0x14 + 0xD, bytes.fromhex("070800") + struct.pack("<H", member),
            "hardware setter destination")
    for off, value in [(0x27, 10), (0x2C, 240), (0x4F, 0), (0x54, 400),
                       (0x59, 50), (0x5E, 1), (0x63, 0), (0x68, 1),
                       (0x6D, 0), (0x72, 64), (0x77, 3), (0x82, 11),
                       (0x185, 10), (0x18A, 240), (0x1AD, 0), (0x1B2, 600),
                       (0x1B7, 36), (0x1BC, 1), (0x1C1, 10), (0x1C6, 1),
                       (0x1CB, 0), (0x1D0, 64), (0x1D5, 4), (0x1E0, 11)]:
        i2(0x99F384 + off, value, "hardware launch record literal")
    i4(0x99F384 + 0x47, 400000, "cartridge retail-price field")
    i4(0x99F384 + 0x1A5, 400000, "CD retail-price field")

    tables = [(0x9960C4, 0x1E2, [1000, 500, 100, 20, 15, 10, 3, 2, 1]),
              (0x9951FC, 0x18E, [1, 2, 4, 6, 20, 25, 100]),
              (0x99B03C, 0x1E2, [1, 2, 3, 4, 6, 8, 10, 13, 17, 20,
                                25, 30, 40, 50, 55, 60, 70, 80, 100])]
    for start, offset, values in tables:
        for n, value in enumerate(values):
            a = start + offset + n * 14
            i2(a, value, "casino payout multiplier")
            raw(a + 5, b"\xf5" + struct.pack("<i", n), "casino payout index")
            raw(a + 13, b"\x52", "casino payout store")
    i4(0x994780 + 0x2CF, -10000000, "coin purchase cash debit")
    i2(0x994780 + 0x2E9, 50, "coin purchase increment")
    raw(0x978364, bytes.fromhex("286cff0100fde60800cc01"), "casino close shared flag")
    return {"procedure_fingerprints": len(manifest["spans"]),
            "casino_table_entries": sum(len(values) for _, _, values in tables)}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("exe", type=Path)
    result = verify(parser.parse_args().exe.read_bytes())
    print("PASS: M76 workflow anchors; " + str(result["procedure_fingerprints"])
          + " procedure fingerprints; " + str(result["casino_table_entries"]) + " payout entries")
