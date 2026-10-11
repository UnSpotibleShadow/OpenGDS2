#!/usr/bin/env python3
"""Verify M77 bounded static anchors; never execute or emit original game content.

This checks the reviewed source and selected interpretations, not playable
casino behavior, feature absence, runtime UI reachability or VB6 RNG parity.
"""
import argparse
import hashlib
import json
import struct
from pathlib import Path

from inspect_vb6 import PeImage, ParseError


def require(condition, label):
    if not condition:
        raise ParseError("Changed casino evidence: " + label)


def verify(data):
    manifest = json.loads(Path(__file__).with_name("casino_evidence.json").read_text())
    require(hashlib.sha256(data).hexdigest() == manifest["source_sha256"],
            "supported executable")
    pe = PeImage(data)
    for name, span in manifest["spans"].items():
        require(hashlib.sha256(pe.bytes_at(span["address"], span["size"])).hexdigest()
                == span["sha256"], name + " fingerprint")
    for entry in manifest["dispatch"]:
        thunk = pe.u32(entry["link_address"])
        require(pe.bytes_at(thunk, 3) == b"\x33\xc0\xba", "event-link thunk")
        code_info = pe.u32(thunk + 3)
        target = code_info - pe.u16(code_info + 8)
        require(target == entry["target"], entry["form"] + " dispatch target")
    for anchor in manifest["anchors"]:
        a, kind = anchor["address"], anchor["kind"]
        if kind == "var_i2":
            require(pe.bytes_at(a, 1) == b"\x28" and
                    struct.unpack("<h", pe.bytes_at(a + 3, 2))[0] == anchor["value"],
                    anchor["label"])
        elif kind == "operand_u16":
            require(pe.bytes_at(a, 1)[0] == anchor["opcode"] and
                    pe.u16(a + 1) == anchor["value"], anchor["label"])
        elif kind == "copy_global":
            require(pe.bytes_at(a, 2) == b"\xfd\xc0" and
                    pe.u16(a + 2) == anchor["index"], anchor["label"])
        else:
            raise ParseError("Unsupported anchor kind: " + kind)
    return {key: len(manifest[key]) for key in ("spans", "dispatch", "anchors")}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("exe", type=Path)
    result = verify(parser.parse_args().exe.read_bytes())
    print("PASS: M77 casino evidence; " + str(result["spans"]) + " fingerprints; "
          + str(result["dispatch"]) + " dispatch targets; "
          + str(result["anchors"]) + " numeric/branch anchors")
