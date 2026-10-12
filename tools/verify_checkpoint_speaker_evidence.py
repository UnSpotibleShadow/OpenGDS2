#!/usr/bin/env python3
"""Verify numeric checkpoint speaker/payroll evidence without emitting original text."""
import argparse
import hashlib
import json
from pathlib import Path
from inspect_vb6 import PeImage, ParseError


def verify(data):
    j=json.loads(Path(__file__).with_name("checkpoint_speaker_evidence.json").read_text())
    if hashlib.sha256(data).hexdigest()!=j["source_sha256"]:raise ParseError("Unsupported executable")
    pe=PeImage(data)
    for name,span in j["spans"].items():
        if hashlib.sha256(pe.bytes_at(span["address"],span["size"])).hexdigest()!=span["sha256"]:
            raise ParseError("Changed procedure: "+name)
    for anchor in j["anchors"]:
        expected=bytes.fromhex(anchor["hex"])
        if pe.bytes_at(anchor["address"],len(expected))!=expected:raise ParseError("Changed anchor: "+anchor["label"])
    return len(j["spans"]),len(j["anchors"]),len(j["checkpoint_random_call_sites"])


if __name__=="__main__":
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("executable",type=Path)
    args=parser.parse_args()
    spans,anchors,calls=verify(args.executable.read_bytes())
    print(f"PASS checkpoint speaker evidence: {spans} fingerprints, {anchors} anchors, {calls} random-employee call sites")
