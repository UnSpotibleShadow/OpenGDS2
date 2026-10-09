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


def config_entries(data):
    """Read the length-prefixed Variant records in Godot's ECFG settings."""
    if data[:4] != b"ECFG" or len(data) < 8:
        raise ValueError("Invalid exported project configuration")
    entries = {}
    cursor = 8
    for _ in range(struct.unpack_from("<I", data, 4)[0]):
        length = struct.unpack_from("<I", data, cursor)[0]
        cursor += 4
        key = data[cursor:cursor+length].decode("utf-8")
        cursor += length
        size = struct.unpack_from("<I", data, cursor)[0]
        cursor += 4
        value = data[cursor:cursor+size]
        if len(value) != size or key in entries:
            raise ValueError("Invalid exported configuration record")
        entries[key] = value
        cursor += size
    if cursor != len(data):
        raise ValueError("Invalid exported configuration length")
    return entries


def preserve_export_configuration(base_config, candidate_config):
    entries = config_entries(base_config)
    features = entries.get("_custom_features", b"")
    if len(features) < 8 or struct.unpack_from("<I", features)[0] != 4:
        raise ValueError("Trusted export has no string feature record")
    size = struct.unpack_from("<I", features, 4)[0]
    if "dotnet" not in features[8:8+size].decode().split(","):
        raise ValueError("Trusted export lacks its dotnet runtime feature")
    entries.update(config_entries(candidate_config))
    # Export feature selection belongs to the trusted native runtime, not the editor.
    entries["_custom_features"] = features
    output = bytearray(b"ECFG" + struct.pack("<I", len(entries)))
    for key, value in entries.items():
        encoded = key.encode()
        output.extend(struct.pack("<I", len(encoded)) + encoded)
        output.extend(struct.pack("<I", len(value)) + value)
    return bytes(output)

root = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("base", type=Path)
parser.add_argument("destination", type=Path)
parser.add_argument("--platform", choices=("linux", "windows"), default="linux")
parser.add_argument("--source-commit", default="unrecorded")
parser.add_argument("--build-label", default="development")
parser.add_argument("--project-config", type=Path, help="Generated project.binary for an isolated test candidate")
parser.add_argument("--candidate", action="store_true", help="Include the private Early Access testing handoff")
args = parser.parse_args()
if args.candidate:
    version = re.search(r'Version = "([^"]+)"', (root/"src/Core/BuildInfo.cs").read_text())
    if args.platform != "windows" or not args.project_config or not version or args.build_label != version.group(1):
        raise ValueError("The private candidate requires Windows, its project configuration and the current build version")
base, destination = args.base, args.destination
assembly = root/".godot/mono/temp/bin/ExportDebug/OpenGDS2.dll"
runtime = base/("data_OpenGDS2_windows_x86_64" if args.platform == "windows"
                else "data_OpenGDS2_linuxbsd_x86_64")
receipt = json.loads(assembly.with_suffix(".build.json").read_text())
if receipt.get("editor_api") is not False or receipt.get("configuration") != "ExportDebug" \
        or receipt.get("assembly_sha256") != hashlib.sha256(assembly.read_bytes()).hexdigest() \
        or receipt.get("godot_api_sha256") != hashlib.sha256((runtime/"GodotSharp.dll").read_bytes()).hexdigest():
    raise ValueError("Assembly must be compiled against this export's Godot API; run workspace_roslyn_build.py export")
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
    if path == "project.binary" and args.project_config:
        content = preserve_export_configuration(content, args.project_config.read_bytes())
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
    "Godot engine, .NET runtime and imported community audio reused from a verified debug artifact.\n"
    "Managed assembly compiled against the export API; dotnet export metadata preserved.\n"
    "Not a clean source export. Native Windows testing remains required.\n"
    "No original game files included. Import your legally obtained LZH locally.\n"
    + ("On Windows: extract the whole ZIP, then run OpenGDS2.console.exe.\n"
       "Windows launch has not been tested in this Linux workspace.\n"
       if args.platform == "windows" else
       "On Linux: run OpenGDS2.sh.\n"))
build_manifest = {
    "platform": args.platform,
    "build_label": args.build_label,
    "source_commit": args.source_commit,
    "managed_assembly_sha256": hashlib.sha256(assembly.read_bytes()).hexdigest(),
    "managed_build": receipt,
    "dotnet_export_metadata_preserved": bool(args.project_config),
    "updated_resources": added+["audio/community/meetings/manifest.json"]
        + (["project.binary"] if args.project_config else []),
    "packaging": "updated managed assembly and resources; reused Godot/.NET native runtime",
    "native_windows_tested": False,
    "private_early_access_candidate": args.candidate,
    "candidate_data_directory": "OpenGDS2-EarlyAccessTesting" if args.candidate else None,
}
(base/"BUILD_MANIFEST.json").write_text(json.dumps(build_manifest,indent=2)+"\n")
if args.candidate:
    for source, name in [("docs/early-access-testing.md","TEST_CHECKLIST.md"),
                         ("docs/early-access-known-issues.md","KNOWN_ISSUES.md"),
                         ("docs/early-access-release-notes.md","RELEASE_NOTES.md"),
                         ("docs/early-access-decisions.md","DECISIONS.md")]:
        shutil.copy2(root/source,base/name)
    shutil.copy2(root/"audio/community/NOTICE.md",base/"licenses/COMMUNITY_MUSIC_NOTICE.md")
    (base/"GETTING_STARTED.txt").write_text(
        "OpenGDS2 "+args.build_label+" — PRIVATE EARLY ACCESS TEST CANDIDATE\n\n"
        "Extract the entire ZIP. Run OpenGDS2.console.exe (or OpenGDS2.exe).\n"
        "No editor or separate .NET installation is needed.\n"
        "Import your legally obtained GameDev2DX.lzh from the title screen.\n"
        "No original game files are included. Read LICENSE, notice.md and licenses/.\n\n"
        "This candidate uses a separate data folder: %APPDATA%\\OpenGDS2-EarlyAccessTesting.\n"
        "Your ordinary OpenGDS2 saves/imports/settings are not used. Start a new company.\n"
        "Modern JSON saves are supported; original-game saves are not supported.\n"
        "Save compatibility across future Early Access versions is not guaranteed. Back up before upgrading.\n\n"
        "Read TEST_CHECKLIST.md, KNOWN_ISSUES.md, RELEASE_NOTES.md and DECISIONS.md before testing.\n"
        "Beginner ends after Year 2, December, week 4. Normal is experimental ongoing play.\n"
        "Bankruptcy and company/office upgrades remain incomplete; negative funds do not end the game.\n"
        "The title's Load Previous Save restores the previous valid save without overwriting the current file.\n"
        "Capture a log with: OpenGDS2.console.exe --log-file opengds2-test.log\n"
        "Report the version, Windows version, exact steps and expected/actual result.\n"
        "Share logs privately; never publish imported originals, original saves or the LZH.\n\n"
        "This is a debug-runtime candidate prepared for owner testing, not a public release.\n"
        "WORKSPACE_BUILD.txt and BUILD_MANIFEST.json identify reused-runtime provenance.\n"
        "Native Windows execution has not been validated on the Linux build host.\n")
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
