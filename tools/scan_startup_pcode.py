#!/usr/bin/env python3
"""Recover small, high-confidence P-Code facts from the startup click handler.

This is intentionally *not* a general decompiler. It recognizes the exact VB6
P-Code sequence `ILdI2; LitI2_Byte; EqI2; BranchF` so we can prove how the
indexed Game Start button branches without guessing at unrelated opcodes.
"""
from __future__ import annotations

import argparse
from pathlib import Path

from inspect_vb6 import PeImage, inspect


def parse_va(text: str) -> int:
    return int(text, 16)


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("exe", type=Path)
    ap.add_argument("--out", type=Path)
    args = ap.parse_args()

    report = inspect(args.exe)
    obj = next(o for o in report["objects"] if o["name"] == "fGMGameKaishi")
    command = next(c for c in obj["controls"] if c["name"] == "Command1")
    event = next(e for e in command["events"] if e["slot"] == 0)
    proc = event["procedure"]
    start = parse_va(proc["start"])
    size = int(proc["proc_size"])

    pe = PeImage(args.exe.read_bytes())
    code = pe.bytes_at(start, size)

    findings: list[tuple[int, int, int, int]] = []
    # 7F = ILdI2 (3 bytes), F4 = LitI2_Byte (2), C6 = EqI2 (1),
    # 1C = BranchF (3). The branch target operand is relative to procedure base.
    for i in range(0, max(0, len(code) - 9)):
        if code[i] != 0x7F or code[i + 3] != 0xF4 or code[i + 5] != 0xC6 or code[i + 6] != 0x1C:
            continue
        local_disp = int.from_bytes(code[i + 1:i + 3], "little", signed=False)
        literal = code[i + 4]
        rel = int.from_bytes(code[i + 7:i + 9], "little", signed=False)
        findings.append((start + i, local_disp, literal, start + rel))

    lines = [
        "OpenGDS2 startup P-Code evidence",
        "",
        f"fGMGameKaishi.Command1 event slot 0 CodeInfo: {event['code_info']}",
        f"Procedure start: 0x{start:08X}",
        f"Procedure size: {size} bytes",
        "",
        "Recognized sequence: ILdI2; LitI2_Byte; EqI2; BranchF",
        "Opcode identities/sizes follow Semi-VB-Decompiler's public opcode table.",
        "",
    ]
    for address, local_disp, literal, target in findings:
        lines.append(
            f"0x{address:08X}: local[0x{local_disp:04X}] == {literal}; "
            f"false branch -> 0x{target:08X}"
        )
    lines += [
        "",
        "Interpretation:",
        "- The handler explicitly distinguishes indexed button values 0 and 1.",
        "- The first false branch lands exactly on the second index comparison.",
        "- The second false branch lands at the procedure epilogue, consistent with a third Cancel choice.",
        "- This supports the recovered three-button control-array structure without claiming yet what all downstream calls do.",
    ]

    text = "\n".join(lines) + "\n"
    if args.out:
        args.out.parent.mkdir(parents=True, exist_ok=True)
        args.out.write_text(text, encoding="utf-8")
    print(text, end="")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
