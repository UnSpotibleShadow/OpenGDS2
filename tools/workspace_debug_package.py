#!/usr/bin/env python3
"""Repackage an authenticated debug artifact with a locally compiled assembly.

Not a clean Godot export: native engine/runtime and imported audio are reused.
Only community-owned resources are included. Original LZH remains local.
"""
import hashlib
import argparse
from pathlib import Path
import shutil
import struct
import zipfile
import re
import json

root = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("base", type=Path)
parser.add_argument("destination", type=Path)
parser.add_argument("--platform", choices=("linux", "windows"), default="linux")
parser.add_argument("--source-commit", default="unrecorded")
parser.add_argument("--build-label", default="development")
args = parser.parse_args()
base, destination = args.base, args.destination
pck = base / "OpenGDS2.pck"
data = pck.read_bytes()
file_base = struct.unpack_from("<Q", data, 24)[0]
count = struct.unpack_from("<I", data, 96)[0]
cursor = 100
entries = []
for _ in range(count):
    length = struct.unpack_from("<I", data, cursor)[0]
    raw = data[cursor+4:cursor+4+length]
    path = raw.rstrip(b"\0").decode()
    entry = cursor+4+length
    offset, size = struct.unpack_from("<QQ", data, entry)
    flags = struct.unpack_from("<I", data, entry+32)[0]
    content = data[file_base+offset:file_base+offset+size]
    if len(content)!=size or hashlib.md5(content).digest()!=data[entry+16:entry+32]:
        raise ValueError("Invalid base PCK entry: "+path)
    if path in ("localization/en.po", "localization/ja.po"):
        content = (root/path).read_bytes()
    entries.append((raw, content, flags))
    cursor = entry+36
# Meeting tracks were added after the base artifact. Include current source
# containers, import descriptors and engine resources explicitly.
added=[]
for descriptor in sorted((root/"audio/community/meetings").glob("*.ogg.import")):
    source=descriptor.with_suffix("")
    remap=re.search(r'^path="res://([^"\n]+)"',descriptor.read_text(),re.MULTILINE)
    if remap is None:raise ValueError("Missing meeting import remap")
    for file in [source,descriptor,root/remap.group(1)]:
        if not file.is_file():raise ValueError("Missing meeting resource: "+str(file))
        relative=file.relative_to(root).as_posix();raw=relative.encode();raw+=b"\0"*((-len(raw))%4)
        entries=[e for e in entries if e[0].rstrip(b"\0").decode()!=relative]
        entries.append((raw,file.read_bytes(),0));added.append(relative)
if len(added)!=12:raise ValueError("Expected four complete meeting recordings")
manifest=root/"audio/community/meetings/manifest.json"
raw=manifest.relative_to(root).as_posix().encode();raw+=b"\0"*((-len(raw))%4)
entries=[e for e in entries if e[0].rstrip(b"\0").decode()!=manifest.relative_to(root).as_posix()]
entries.append((raw,manifest.read_bytes(),0))
directory = bytearray()
payload = bytearray()
for path, content, flags in entries:
    directory.extend(struct.pack("<I",len(path))+path)
    directory.extend(struct.pack("<QQ",len(payload),len(content)))
    directory.extend(hashlib.md5(content).digest()+struct.pack("<I",flags))
    payload.extend(content)
    payload.extend(b"\0"*((-len(payload))%16))
new_base = (100+len(directory)+15)//16*16
header = bytearray(data[:100])
struct.pack_into("<Q",header,24,new_base)
struct.pack_into("<I",header,96,len(entries))
pck.write_bytes(header+directory+b"\0"*(new_base-100-len(directory))+payload)
assembly = root/".godot/mono/temp/bin/Debug/OpenGDS2.dll"
runtime = base/("data_OpenGDS2_windows_x86_64" if args.platform == "windows"
                else "data_OpenGDS2_linuxbsd_x86_64")
for suffix in (".dll", ".pdb"):
    shutil.copy2(assembly.with_suffix(suffix),runtime/("OpenGDS2"+suffix))
if args.platform == "linux":
    shutil.copy2(root.parent/"toolchain/thread-entry.so",base/"workspace-thread-entry.so")
    (base/"OpenGDS2.x86_64").chmod(0o755)
    (base/"OpenGDS2.sh").write_text('#!/bin/sh\n'
    'cd "$(dirname "$0")" || exit 1\n'
    'if [ ! -e /proc/self/exe ]; then\n'
    '  export LD_PRELOAD="$PWD/workspace-thread-entry.so"\n'
    '  export DOTNET_EnableDiagnostics=0 DOTNET_PROCESSOR_COUNT=2\n'
    'fi\n'
    'exec ./OpenGDS2.x86_64 --main-pack OpenGDS2.pck "$@"\n')
    (base/"OpenGDS2.sh").chmod(0o755)
(base/"WORKSPACE_BUILD.txt").write_text(
    "OpenGDS2 "+args.build_label+" "+args.platform+" development snapshot.\n"
    "Source commit: "+args.source_commit+"\n"
    "Includes meeting music, checkpoint choices, bank loans, office rendering and facility artwork.\n"
    "Godot engine, .NET runtime and imported community audio reused from M47 debug artifact.\n"
    "Not a clean source export; missing source OGG integrity gate remains unresolved.\n"
    "No original game files included. Import your legally obtained LZH locally.\n"
    + ("On Windows: extract the whole ZIP, then run OpenGDS2.console.exe.\n"
       "Windows launch has not been tested in this Linux workspace.\n"
       if args.platform == "windows" else
       "On Linux: run OpenGDS2.sh.\n"))
(base/"BUILD_MANIFEST.json").write_text(json.dumps({"platform":args.platform,"build_label":args.build_label,"source_commit":args.source_commit,"managed_assembly_sha256":hashlib.sha256(assembly.read_bytes()).hexdigest(),"updated_resources":added,"packaging":"updated managed assembly and resources; reused Godot/.NET native runtime","native_windows_tested":False},indent=2)+"\n")
checksums = []
for path in sorted(base.rglob("*")):
    if path.is_file() and path.name != "SHA256SUMS":
        checksums.append(hashlib.sha256(path.read_bytes()).hexdigest()+"  "+path.relative_to(base).as_posix())
(base/"SHA256SUMS").write_text("\n".join(checksums)+"\n")
destination.parent.mkdir(parents=True,exist_ok=True)
with zipfile.ZipFile(destination,"w",compression=zipfile.ZIP_DEFLATED) as archive:
    for path in sorted(base.rglob("*")):
        if path.is_file(): archive.write(path,Path(f"OpenGDS2-{args.platform}-{args.build_label}")/path.relative_to(base))
print("Development package:",destination)
