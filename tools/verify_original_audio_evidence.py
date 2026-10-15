#!/usr/bin/env python3
"""Verify original cue callsites and asynchronous flags; never emit audio or dialogue."""
import argparse,hashlib,json,struct
from pathlib import Path
from inspect_vb6 import PeImage

def verify(path):
    evidence=json.loads(Path(__file__).with_name('original_audio_evidence.json').read_text());data=path.read_bytes()
    if hashlib.sha256(data).hexdigest()!=evidence['source_sha256']:raise ValueError('Unsupported original executable')
    pe=PeImage(data)
    for span in evidence['fingerprints']:
        if hashlib.sha256(pe.bytes_at(int(span['start'],16),span['size'])).hexdigest()!=span['sha256']:raise ValueError('Changed audio procedure')
    dispatcher=evidence['dispatcher'];start=int(dispatcher['start'],16)
    for offset in dispatcher['flag_offsets']:
        if pe.bytes_at(start+offset,5)!=b'\xf5'+struct.pack('<I',1):raise ValueError('Changed playback flags')
    for item in evidence['calls']:
        address=int(item['procedure'],16)+item['relative'];raw=pe.bytes_at(address-8,13)
        if raw.hex()!=item['anchor_hex'] or raw[0]!=0x28 or struct.unpack_from('<h',raw,3)[0]!=item['cue']:raise ValueError('Changed cue argument')
        if raw[8]!=0x0a:raise ValueError('Changed cue call')
        thunk=pe.u32(int(item['pool'],16)+struct.unpack_from('<H',raw,9)[0]*4)
        if pe.bytes_at(thunk,1)!=b'\xba' or pe.u32(thunk+1)!=0x0098ADA4:raise ValueError('Changed dispatcher target')
    print(f"PASS original audio: {len(evidence['fingerprints'])} fingerprints, {len(evidence['calls'])} callsites, four SND_ASYNC branches")
if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('executable',type=Path);verify(parser.parse_args().executable)
