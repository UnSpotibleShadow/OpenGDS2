#!/usr/bin/env python3
"""Check bounded speed-technology evidence; never execute or emit original content."""
import argparse
import hashlib
import json
from pathlib import Path
from inspect_vb6 import PeImage, ParseError


def verify(data):
    manifest=json.loads(Path(__file__).with_name("speed_technology_evidence.json").read_text())
    if hashlib.sha256(data).hexdigest()!=manifest["source_sha256"]:
        raise ParseError("Unsupported executable")
    pe=PeImage(data)
    for name,span in manifest["spans"].items():
        if hashlib.sha256(pe.bytes_at(span["address"],span["size"])).hexdigest()!=span["sha256"]:
            raise ParseError("Changed fingerprint: "+name)
    for anchor in manifest["anchors"]:
        expected=bytes.fromhex(anchor["hex"])
        if pe.bytes_at(anchor["address"],len(expected))!=expected:
            raise ParseError("Changed anchor: "+anchor["label"])
    return len(manifest["spans"]),len(manifest["anchors"])


if __name__=="__main__":
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("executable",type=Path)
    args=parser.parse_args()
    fingerprints,anchors=verify(args.executable.read_bytes())
    print(f"PASS speed technology evidence: {fingerprints} procedure fingerprints, {anchors} bounded anchors")
