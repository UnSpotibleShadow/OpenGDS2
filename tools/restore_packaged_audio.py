#!/usr/bin/env python3
"""Recover engine-ready audio from a trusted desktop artifact for local checks.

This restores imported resources, not original source OGG containers. It never
changes the source integrity manifest or claims the source assets were restored.
"""
import hashlib
import io
from pathlib import Path
import struct
import sys
import zipfile

def recover(artifact, project, export):
    with zipfile.ZipFile(artifact) as outer:
        name = next(n for n in outer.namelist() if n.endswith("linux-x86_64-debug.zip"))
        with zipfile.ZipFile(io.BytesIO(outer.read(name))) as inner:
            inner.extractall(export)
            pck_name = next(n for n in inner.namelist() if n.endswith("/OpenGDS2.pck"))
            data = inner.read(pck_name)
    if data[:4] != b"GDPC" or struct.unpack_from("<I", data, 4)[0] != 2:
        raise ValueError("Expected a Godot 4 PCK version 2")
    base = struct.unpack_from("<Q", data, 24)[0]
    count = struct.unpack_from("<I", data, 96)[0]
    cursor = 100
    restored = 0
    for _ in range(count):
        length = struct.unpack_from("<I", data, cursor)[0]
        path = data[cursor+4:cursor+4+length].rstrip(b"\0").decode()
        entry = cursor + 4 + length
        offset, size = struct.unpack_from("<QQ", data, entry)
        md5 = data[entry+16:entry+32]
        cursor = entry + 36
        if not (path.startswith(".godot/imported/") and path.endswith(".oggvorbisstr") or
                path.startswith("audio/community/") and path.endswith(".ogg.import")):
            continue
        relative = Path(path)
        if relative.is_absolute() or ".." in relative.parts:
            raise ValueError("Unsafe PCK path")
        content = data[base+offset:base+offset+size]
        if len(content) != size or hashlib.md5(content).digest() != md5:
            raise ValueError("Invalid PCK entry: " + path)
        destination = project / relative
        if destination.exists() and destination.read_bytes() != content:
            raise ValueError("Refusing to replace a different cached resource: " + path)
        destination.parent.mkdir(parents=True, exist_ok=True)
        destination.write_bytes(content)
        restored += 1
    print("Restored", restored, "engine-ready resources (source OGG gate remains separate).")

if __name__ == "__main__":
    if len(sys.argv) != 4:
        raise SystemExit("usage: restore_packaged_audio.py ARTIFACT PROJECT EXPORT_DIRECTORY")
    recover(Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]))
