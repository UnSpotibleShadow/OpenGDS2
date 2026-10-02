#!/usr/bin/env python3
"""Recover printable length-prefixed strings from one serialized VB6 form block.

GameDev2DX stores designer/form text as a 16-bit little-endian byte length followed by
CP932/ASCII bytes. This helper finds a requested form name, stops at the next fGM*
form marker, and emits the readable strings in between. It is a structural evidence
tool, not a complete VB6 form parser, and never executes the target executable.
"""
from __future__ import annotations

import argparse
from pathlib import Path


def lp_bytes(text: str) -> bytes:
    raw = text.encode("cp932")
    if len(raw) > 0xFFFF:
        raise ValueError("string too long")
    return len(raw).to_bytes(2, "little") + raw


def printable_ratio(text: str) -> float:
    if not text:
        return 0.0
    good = 0
    for c in text:
        if c.isalnum() or c.isspace() or c in "==[]()（）・：:￥円万億,.!?！？-_/&+%#.'":
            good += 1
        elif "\u3040" <= c <= "\u30ff" or "\u3400" <= c <= "\u9fff":
            good += 1
    return good / len(text)


def decode_candidates(data: bytes, start: int, end: int):
    i = start
    while i + 2 <= end:
        n = int.from_bytes(data[i:i+2], "little")
        if 1 <= n <= 240 and i + 2 + n <= end:
            raw = data[i+2:i+2+n]
            if b"\0" not in raw:
                try:
                    text = raw.decode("cp932")
                except UnicodeDecodeError:
                    text = ""
                if text and all(ord(c) >= 32 for c in text) and printable_ratio(text) >= 0.75:
                    yield i, n, text
        i += 1


def find_next_form(data: bytes, after: int) -> int | None:
    # Serialized form/class names in this executable are short fGM* CP932/ASCII strings.
    for i in range(after, min(len(data) - 6, after + 0x10000)):
        n = int.from_bytes(data[i:i+2], "little")
        if not (4 <= n <= 48) or i + 2 + n > len(data):
            continue
        raw = data[i+2:i+2+n]
        if raw.startswith(b"fGM"):
            try:
                text = raw.decode("ascii")
            except UnicodeDecodeError:
                continue
            if text.replace("_", "").isalnum():
                return i
    return None


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("exe", type=Path)
    ap.add_argument("form_name")
    ap.add_argument("--out", type=Path)
    args = ap.parse_args()

    data = args.exe.read_bytes()
    marker = lp_bytes(args.form_name)
    start = data.find(marker)
    if start < 0:
        raise SystemExit(f"serialized form marker not found: {args.form_name}")
    end = find_next_form(data, start + len(marker)) or min(len(data), start + 0x8000)

    lines = [
        f"form\t{args.form_name}",
        f"block_start\t0x{start:08X}",
        f"block_end\t0x{end:08X}",
        "offset_hex\tbyte_length\ttext",
    ]
    for off, n, text in decode_candidates(data, start, end):
        clean = text.replace("\t", " ").replace("\r", " ").replace("\n", " ")
        lines.append(f"0x{off:08X}\t{n}\t{clean}")

    output = "\n".join(lines) + "\n"
    if args.out:
        args.out.parent.mkdir(parents=True, exist_ok=True)
        args.out.write_text(output, encoding="utf-8")
    print(output, end="")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
