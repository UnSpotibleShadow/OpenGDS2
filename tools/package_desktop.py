#!/usr/bin/env python3
"""Package only an exported community runtime; never add an original-game cache."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import zipfile

def package(source, export, platform, notices, output):
    executable = "OpenGDS2.x86_64" if platform == "linux" else "OpenGDS2.exe"
    if not (export / executable).is_file() or not (export / "OpenGDS2.pck").is_file():
        raise ValueError("The native executable and game pack are required")
    if not list(export.rglob("OpenGDS2.dll")):
        raise ValueError("Export is missing its managed game assembly")
    forbidden = {"original","original_cache","private-original","asset-overrides","saves","upload","generated","tests","tools"}
    for file in export.rglob("*"):
        if file.is_symlink():
            raise ValueError("Symlinks are not accepted in desktop packages")
        if any(part.casefold() in forbidden for part in file.relative_to(export).parts):
            raise ValueError("A private/cache directory was added to the export")
        if file.name.casefold() in {"gamedev2dx.exe","employees.json","catalog.json","office.json"} or file.suffix.casefold() in {".lzh",".bmp",".wav",".gds"}:
            raise ValueError("Original-game content was added to the export")
    license_dir = export / "licenses"
    license_dir.mkdir(exist_ok=True)
    for filename in ("LICENSE","notice.md"):
        shutil.copyfile(source / filename, export / filename)
    music_notice = source / "audio" / "community" / "NOTICE.md"
    if music_notice.is_file():
        shutil.copyfile(music_notice, license_dir / "COMMUNITY_MUSIC_NOTICE.md")
    for filename in ("GODOT_LICENSE.txt","GODOT_THIRD_PARTY.txt"):
        shutil.copyfile(notices / filename, license_dir / filename)
    # The runtime pack selected by dotnet publish contains the authoritative
    # license and third-party notices for the bundled .NET runtime version.
    runtime_packs = set()
    for dependency_file in export.rglob("*.deps.json"):
        libraries = json.loads(dependency_file.read_text(encoding="utf-8")).get("libraries", {})
        for identity in libraries:
            name, _, version = identity.partition("/")
            name = name.removeprefix("runtimepack.")
            if name.startswith("Microsoft.NETCore.App.Runtime."):
                runtime_packs.add((name.casefold(),version))
    if not runtime_packs:
        raise ValueError("Cannot identify the bundled .NET runtime pack")
    nuget = Path(os.environ.get("NUGET_PACKAGES", str(Path.home() / ".nuget" / "packages")))
    for name, version in sorted(runtime_packs):
        pack = nuget / name / version
        files = {file.name.casefold(): file for file in pack.iterdir() if file.is_file()}
        for filename in ("license.txt","third-party-notices.txt"):
            if filename not in files:
                raise ValueError(f"Runtime pack {name}/{version} lacks {filename}")
            shutil.copyfile(files[filename],license_dir / (name+"-"+version+"-"+filename))
    start = """OpenGDS2 development build

Extract the entire archive before running the executable. Keep the .pck and
managed-runtime directory beside it. Linux: run ./OpenGDS2.x86_64 (chmod +x if
your archive extractor does not retain permissions). Windows: run OpenGDS2.exe.

The title screen runs without original files. For gameplay and the original
office appearance, select Import Original Game and choose your legally obtained
GameDev2DX.lzh archive. Import happens locally; this package contains no original
Kairosoft executable, graphics, sounds or game database.

Optional background music uses local files in the music folder shown in Settings.
No soundtrack recordings are bundled. Original LZH importing is still required.

Imported files are stored in Godot's user://original directory; saves are in
user://saves/slot1.json. Do not copy either directory into a public package.
Current gameplay includes development, release, sales, employee management,
training, reviews and recalls. Calendar economics, awards and office progression
remain incomplete. This is a development checkpoint, not a completed port.

Code licensing: LICENSE. Original-content exclusions/disclaimers: notice.md.
Engine and runtime attribution: licenses/.
Source, validation and current limitations:
https://github.com/UnSpotibleShadow/OpenGDS2
"""
    (export / "GETTING_STARTED.txt").write_text(start,encoding="utf-8")
    files = sorted(file for file in export.rglob("*") if file.is_file() and file.name != "SHA256SUMS")
    sums = "\n".join(hashlib.sha256(file.read_bytes()).hexdigest()+"  "+file.relative_to(export).as_posix() for file in files)+"\n"
    (export / "SHA256SUMS").write_text(sums,encoding="utf-8")
    output.parent.mkdir(parents=True,exist_ok=True)
    with zipfile.ZipFile(output,"w",compression=zipfile.ZIP_DEFLATED) as archive:
        for file in sorted(export.rglob("*")):
            if file.is_file():
                archive.write(file,Path("OpenGDS2-"+platform+"-x86_64") / file.relative_to(export))
    print(f"Desktop package: {output.name} ({output.stat().st_size} bytes)")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source",type=Path,default=Path("."))
    parser.add_argument("--export",type=Path,required=True)
    parser.add_argument("--platform",choices=("linux","windows"),required=True)
    parser.add_argument("--notices",type=Path,required=True)
    parser.add_argument("--output",type=Path,required=True)
    args = parser.parse_args()
    package(args.source,args.export,args.platform,args.notices,args.output)
