#!/usr/bin/env python3
"""Verify the bounded #95 handler audit without emitting original text/assets."""
import argparse
import hashlib
import json
import struct
from pathlib import Path

from inspect_vb6 import PeImage, ParseError, read_controls


def verify(data):
    evidence = json.loads(Path(__file__).with_name("presentation_handler_evidence.json").read_text())
    if hashlib.sha256(data).hexdigest() != evidence["source_sha256"]:
        raise ParseError("Unsupported original executable")
    pe = PeImage(data)
    for name, span in evidence["spans"].items():
        if hashlib.sha256(pe.bytes_at(span["address"], span["size"])).hexdigest() != span["sha256"]:
            raise ParseError("Changed procedure: " + name)
    for anchor in evidence["anchors"]:
        expected = bytes.fromhex(anchor["hex"])
        if pe.bytes_at(anchor["address"], len(expected)) != expected:
            raise ParseError("Changed anchor: " + anchor["label"])
    cache = {}
    for binding in evidence["event_bindings"]:
        address = binding["object_info"]
        if address not in cache:
            cache[address] = read_controls(pe, address, address)
        control = next((c for c in cache[address] if c["name"] == binding["control"]), None)
        if control is None or not any(
            e["slot"] == binding["event_slot"] and
            int(e["procedure"]["start"], 16) == binding["procedure"]
            for e in control["events"]
        ):
            raise ParseError("Changed event binding: " + binding["form"] + "." + binding["control"])
    for resource in evidence["resources"]:
        start, size = resource["file_offset"], resource["size"]
        raw = data[start:start + size]
        if len(raw) != size or hashlib.sha256(raw).hexdigest() != resource["sha256"]:
            raise ParseError("Changed local resource: " + resource["label"])
        if raw[:2] == b"BM":
            dimensions = struct.unpack_from("<ii", raw, 18)
        elif raw[:6] in (b"GIF87a", b"GIF89a"):
            dimensions = struct.unpack_from("<HH", raw, 6)
        else:
            raise ParseError("Unsupported local resource format")
        if dimensions != (resource["width"], resource["height"]):
            raise ParseError("Changed resource dimensions")
    for literal in evidence.get("literal_checks", []):
        value = int.from_bytes(pe.bytes_at(literal["address"], literal["size"]), "little", signed=True)
        if value != literal["value"]:
            raise ParseError("Changed numeric literal: " + literal["label"])
    return {key: len(evidence[key]) for key in
            ("spans", "anchors", "event_bindings", "resources", "literal_checks")}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("executable", type=Path)
    args = parser.parse_args()
    counts = verify(args.executable.read_bytes())
    print("PASS presentation handler evidence: "
          f"{counts['spans']} fingerprints, {counts['anchors']} anchors, "
          f"{counts['event_bindings']} event bindings, {counts['resources']} resources, "
          f"{counts['literal_checks']} numeric literals")
