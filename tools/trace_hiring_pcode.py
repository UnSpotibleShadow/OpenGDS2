#!/usr/bin/env python3
"""Reproduce the OpenGDS2 M5 static hiring traces for GameDev2DX.exe.

This utility NEVER executes the target.  It parses a small, documented subset of
Visual Basic 6 P-Code used by the known official Game Dev Story 2 DX executable.
The goal is not to be a general VB6 decompiler; it is a repeatable evidence tool
for the recruitment/hiring rules currently implemented by OpenGDS2.

Opcode names/sizes are derived from the public VB P-Code table documented at:
    https://www.vb-decompiler.org/vb_pcode_table.htm
VB metadata structures are parsed by tools/inspect_vb6.py.
"""
from __future__ import annotations

import argparse
import hashlib
import struct
import sys
from dataclasses import dataclass
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from inspect_vb6 import PeImage, ParseError  # noqa: E402

KNOWN_SHA256 = "4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6"
FGMSKOYOU_POOL = 0x0060C264

# Known procedure spans in the official executable above.  These addresses are
# verified from the VB6 metadata graph, not by executing the game.
PROCEDURES = {
    "hire": (0x0099FFC8, 1480, "fGMSKoyou Hire/Command handler"),
    "load": (0x009BABFC, 3508, "fGMSKoyou form initialization / roster build"),
    "employee_copy": (0x0098735C, 328, "Module1 employee-slot copy helper"),
    "finance": (0x0097DD04, 88, "Module1 shared financial accumulator"),
}

# Only the opcodes reached by the M5 procedures are included.  The numeric size
# convention matches the public table used by the existing OpenGDS2 research
# tools.  For FB/FC/FD/FE/FF prefixed opcodes, the encoded prefix contributes an
# additional byte relative to this size field.
OPS: dict[str, tuple[int, str]] = {
    "04": (3, "FLdRfVar"), "05": (3, "ImpAdLdRf"), "06": (3, "MemLdRfVar"),
    "07": (5, "FMemLdRf"), "08": (3, "FLdPr"), "0A": (5, "ImpAdCallFPR4"),
    "0D": (5, "VCallHresult"), "0F": (3, "VCallAd"), "10": (5, "ThisVCallHresult"), "13": (1, "ExitProcHresult"),
    "19": (3, "FStAdFunc"), "1A": (3, "FFree1Ad"), "1B": (3, "LitStr"),
    "1C": (3, "BranchF"), "1E": (3, "Branch"), "21": (1, "FLdPrThis"),
    "24": (3, "NewIfNullPr"), "25": (1, "PopAdLdVar"), "27": (3, "LitVar_Missing"),
    "28": (5, "LitVarI2"), "29": (-1, "FFreeAd"), "2A": (1, "ConcatStr"),
    "2B": (3, "PopTmpLdAd2"), "2F": (3, "FFree1Str"), "35": (3, "FFree1Var"),
    "36": (-1, "FFreeVar"), "37": (1, "PopFPR4"), "3A": (5, "LitVarStr"),
    "40": (1, "Ary1LdRf"), "46": (3, "CVarStr"), "4E": (3, "FStVarCopyObj"),
    "51": (3, "FLdZeroAd_alias"), "52": (1, "Ary1StVar"), "55": (1, "CI2Var"),
    "59": (3, "PopTmpLdAdStr"), "5B": (3, "FStAdFuncNoPop"), "5D": (1, "HardType"),
    "6B": (3, "FLdI2"), "6C": (3, "ILdRf"), "6E": (3, "FLdFPR4"),
    "73": (3, "FStFPR4"), "7F": (3, "ILdI2"), "A2": (1, "Ary1StI2"),
    "A7": (3, "AryLdPr"), "AB": (1, "AddR8"), "AF": (1, "SubR4"),
    "B3": (1, "MulR8"), "B6": (1, "DivR8"), "C6": (1, "EqI2"),
    "C8": (1, "EqR4"), "DC": (1, "GtR4"), "E5": (1, "CI2R8"),
    "E7": (1, "CI4UI1"), "EA": (1, "CR4R4"), "EB": (1, "CR8I2"),
    "F3": (3, "LitI2"), "F4": (2, "LitI2_Byte"), "F5": (5, "LitI4"),
    "FB27": (3, "AndVar"), "FB2F": (3, "EqVar"), "FB33": (1, "EqVarBool"),
    "FB5A": (1, "GeVarBool"), "FB67": (1, "LtVarBool"), "FB74": (1, "GtVarBool"),
    "FB94": (3, "AddVar"), "FB9C": (3, "SubVar"), "FBB4": (3, "MulVar"),
    "FBCC": (3, "UMiVar"), "FBE6": (1, "FnIntR8"), "FBEF": (3, "ConcatVar"),
    "FC22": (1, "CI4Var"), "FC2A": (1, "CR8Var"), "FC96": (1, "Ary1LdRfVar"),
    "FCB0": (1, "Ary1StVarCopy"), "FCF6": (3, "FStVar"), "FD00": (3, "FStVarCopy"),
    "FD16": (5, "ILdRfDarg"), "FD6A": (3, "CVarR4"), "FD6B": (3, "CVarR8"),
    "FD6F": (3, "CVarAd"), "FD95": (3, "ExitProcCb"), "FD9C": (3, "FStAdNoPop"),
    "FDB6": (3, "ImpAdStVar"), "FDC0": (3, "ImpAdStVarCopy"),
    "FDC8": (3, "PopTmpLdAdFPR4"), "FDE6": (5, "FMemStVar"),
    "FDF8": (3, "CVarBoolI2"), "FDFE": (3, "CStrVarVal"),
    "FE68": (5, "ForVar"), "FE7E": (5, "NextStepVar"),
    "FF1B": (1, "CBoolVarNull"), "FF28": (1, "DOC"),
}

@dataclass(frozen=True)
class Instruction:
    offset: int
    key: str
    mnemonic: str
    raw: bytes


def _decode_one(data: bytes, pos: int) -> tuple[Instruction, int]:
    start = pos
    first = data[pos]
    pos += 1
    prefixed = first >= 0xFB
    if prefixed:
        if pos >= len(data):
            raise ParseError(f"truncated prefixed opcode at +0x{start:04X}")
        key = f"{first:02X}{data[pos]:02X}"
        pos += 1
    else:
        key = f"{first:02X}"

    if key not in OPS:
        raise ParseError(f"unsupported opcode {key} at +0x{start:04X}")
    size, mnemonic = OPS[key]

    # FFreeAd / FFreeVar are variable sized: a u16 byte-count follows, then that
    # many bytes when the count is the compact form used by this executable.
    if size < 0:
        if pos + 2 > len(data):
            raise ParseError(f"truncated {mnemonic} at +0x{start:04X}")
        count = struct.unpack_from("<H", data, pos)[0]
        pos += 2
        if count < 48:
            pos += count
        if pos > len(data):
            raise ParseError(f"{mnemonic} extends past procedure at +0x{start:04X}")
    else:
        # This mirrors the size convention in the public table and the existing
        # OpenGDS2 parser: one opcode byte is already consumed.  A prefixed
        # opcode consumes a second prefix byte before these remaining bytes.
        remaining = size - 1
        pos += remaining
        if pos > len(data):
            raise ParseError(f"{mnemonic} extends past procedure at +0x{start:04X}")

    return Instruction(start, key, mnemonic, data[start:pos]), pos


def parse_procedure(pe: PeImage, start: int, size: int) -> tuple[list[Instruction], int]:
    data = pe.bytes_at(start, size)
    out: list[Instruction] = []
    pos = 0
    while pos < len(data):
        # VB6 procedure blobs may include one/two zero alignment bytes after an
        # exit opcode.  Treat only terminal zeros as padding.
        if all(b == 0 for b in data[pos:]):
            break
        ins, pos = _decode_one(data, pos)
        out.append(ins)
    return out, len(data) - pos


def pool_index(ins: Instruction) -> int | None:
    if ins.mnemonic.startswith("ImpAd") and len(ins.raw) >= 3:
        # Operand follows either the one-byte opcode or the two-byte FB..FF prefix.
        operand = 2 if ins.raw[0] >= 0xFB else 1
        if operand + 2 <= len(ins.raw):
            return struct.unpack_from("<H", ins.raw, operand)[0]
    return None


def format_instruction(ins: Instruction) -> str:
    annotation = ""
    pi = pool_index(ins)
    if pi is not None:
        annotation = f" ; pool[{pi:02X}]"
    return f"+0x{ins.offset:04X}  {ins.raw.hex(' '):<20} {ins.mnemonic}{annotation}"


def _has_sequence(ins: list[Instruction], names: list[str]) -> bool:
    m = [x.mnemonic for x in ins]
    n = len(names)
    return any(m[i:i+n] == names for i in range(len(m) - n + 1))


def verify_evidence(pe: PeImage, parsed: dict[str, list[Instruction]]) -> list[str]:
    """Return human-readable checks, raising if a core M5 pattern disappears."""
    hire = parsed["hire"]
    load = parsed["load"]
    copy = parsed["employee_copy"]
    finance = parsed["finance"]

    # Exact first decision recovered from fGMSKoyou Hire.
    first = [x.mnemonic for x in hire[:7]]
    expected_first = ["ImpAdLdRf", "ImpAdLdRf", "CI4Var", "ImpAdLdRf", "Ary1LdRfVar", "LtVarBool", "BranchF"]
    if first != expected_first:
        raise ParseError(f"hire affordability prefix changed: {first!r}")
    if [pool_index(hire[i]) for i in (0, 1, 3)] != [0, 1, 2]:
        raise ParseError("hire affordability pool references are not [funds, selection, fee-array]")

    # Random roster construction requires Rnd -> *4 -> Int -> == 0 -> And.
    if not _has_sequence(load, ["LitVar_Missing", "ImpAdCallFPR4", "LitI2_Byte", "CR8I2", "MulR8", "FnIntR8"]):
        raise ParseError("roster random-selection sequence not found")
    if not any(x.mnemonic == "Ary1StVarCopy" for x in load):
        raise ParseError("roster store operation not found")

    # Successful transaction negates a selected source value before calling the
    # financial accumulator.  The helper then performs repeated AddVar stores.
    if not any(x.mnemonic == "UMiVar" for x in copy):
        raise ParseError("negated candidate fee not found in employee-copy helper")
    if sum(x.mnemonic == "AddVar" for x in finance) < 4 or sum(x.mnemonic == "FMemStVar" for x in finance) < 4:
        raise ParseError("shared financial accumulator pattern not found")

    # Roster limit literal 5 is the LitVarI2 at +0x02EC in this known build.
    roster_limit = next((x for x in load if x.offset == 0x02EC), None)
    if roster_limit is None or roster_limit.mnemonic != "LitVarI2" or roster_limit.raw[-2:] != b"\x05\x00":
        raise ParseError("literal roster limit 5 not found at expected form-load offset")

    # Post-hire Rnd * 10 dispatch: the handler explicitly compares the result to
    # all ten integer cases 0..9.
    greeting_cases: list[int] = []
    for i, x in enumerate(hire):
        if x.mnemonic != "LitI2_Byte" or len(x.raw) != 2 or i == 0:
            continue
        around = [y.mnemonic for y in hire[i-1:i+4]]
        if around == ["FLdFPR4", "LitI2_Byte", "CR8I2", "EqR4", "BranchF"]:
            greeting_cases.append(x.raw[-1])
    if greeting_cases != list(range(10)):
        raise ParseError(f"expected greeting dispatch cases 0..9, got {greeting_cases!r}")

    return [
        "Hire begins by comparing pool[00] (funds) against pool[02][pool[01]] (selected contract fee); lower funds branch to failure.",
        "Form initialization fills five roster slots and contains the Rnd -> multiply by 4 -> Int -> compare 0 acceptance test.",
        "Accepted source indices are copied into the applicant-roster array; passes repeat until the roster count reaches 5.",
        "The successful-hire helper copies the selected source record into the first vacant employee slot.",
        "That helper negates the selected monetary value and calls the shared financial accumulator, implementing the contract-fee charge.",
        "The Hire handler also contains a random dispatch across 10 post-hire greeting alternatives.",
    ]


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("exe", type=Path, help="path to the extracted original GameDev2DX.exe")
    ap.add_argument("--dump", choices=sorted(PROCEDURES), help="dump one decoded procedure")
    ap.add_argument("--allow-other-hash", action="store_true", help="parse even if SHA-256 is not the verified official build")
    args = ap.parse_args()

    data = args.exe.read_bytes()
    sha = hashlib.sha256(data).hexdigest()
    if sha != KNOWN_SHA256 and not args.allow_other_hash:
        print(f"error: unsupported executable SHA-256 {sha}", file=sys.stderr)
        print(f"expected: {KNOWN_SHA256}", file=sys.stderr)
        print("Use --allow-other-hash only for exploratory parsing; evidence offsets are build-specific.", file=sys.stderr)
        return 2

    pe = PeImage(data)
    parsed: dict[str, list[Instruction]] = {}
    print("OpenGDS2 M5 static hiring trace")
    print(f"SHA-256: {sha}")
    print("Target execution: NEVER (static parsing only)")
    print()

    for name, (start, size, desc) in PROCEDURES.items():
        try:
            ins, padding = parse_procedure(pe, start, size)
        except Exception as exc:
            print(f"{name}: parse failed: {exc}", file=sys.stderr)
            return 3
        parsed[name] = ins
        consumed = sum(len(x.raw) for x in ins)
        print(f"{name:13} start=0x{start:08X} size={size:4} decoded={consumed:4} padding={padding} instructions={len(ins):4}  {desc}")

    print("\nVerified M5 evidence:")
    try:
        checks = verify_evidence(pe, parsed)
    except Exception as exc:
        print(f"evidence verification failed: {exc}", file=sys.stderr)
        return 4
    for item in checks:
        print(f"  - {item}")

    print("\nSemantic boundary:")
    print("  The source employee database and the exact meaning of its roster gate/flag are not decoded yet.")
    print("  This tool therefore does not invent applicant identities or stats.")

    if args.dump:
        print(f"\nDecoded procedure: {args.dump} — {PROCEDURES[args.dump][2]}")
        for ins in parsed[args.dump]:
            print(format_instruction(ins))

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
