#!/usr/bin/env python3
"""Package only an exported community runtime; never add an original-game cache."""
import argparse
import hashlib
import json
import os
import re
from pathlib import Path
import shutil
import zipfile
from urllib.parse import urlparse

def package(source, export, platform, notices, output, configuration="release", candidate=False, source_commit=None,
            early_access=False, report_url=None):
    if configuration not in {"release", "debug"}:
        raise ValueError("Unsupported export configuration")
    if candidate and early_access:
        raise ValueError("Private candidate and public Early Access modes are mutually exclusive")
    project = (source / "project.godot").read_text() if candidate or early_access else ""
    candidate_version = re.search(r'^config/version="([^"]+)"$', project, re.M)
    if candidate and (platform != "windows" or configuration != "release" or not candidate_version
                      or not re.fullmatch(r"[0-9a-f]{40}", source_commit or "")):
        raise ValueError("A candidate requires a Windows release export, current version and exact source commit")
    if early_access:
        if (platform != "windows" or configuration != "release" or not candidate_version or
            not re.fullmatch(r"\d+\.\d+\.\d+-ea\.\d+", candidate_version.group(1)) or
            not re.fullmatch(r"[0-9a-f]{40}", source_commit or "")):
            raise ValueError("Public Early Access requires a Windows release export, final EA version and exact source commit")
        if (not report_url or any(c.isspace() for c in report_url) or
            urlparse(report_url).scheme != "https" or not urlparse(report_url).hostname):
            raise ValueError("Public Early Access requires an HTTPS community reporting URL")
        directory = re.search(r'^config/custom_user_dir_name="([^"\\/]+)"$', project, re.M)
        if not re.search(r'^config/use_custom_user_dir=true$', project, re.M) or not directory:
            raise ValueError("Public Early Access requires an explicit custom data directory")
        data_directory = directory.group(1)
    executable = "OpenGDS2.x86_64" if platform == "linux" else "OpenGDS2.exe"
    if not (export / executable).is_file() or not (export / "OpenGDS2.pck").is_file():
        raise ValueError("The native executable and game pack are required")
    if not list(export.rglob("OpenGDS2.dll")):
        raise ValueError("Export is missing its managed game assembly")
    if configuration == "debug" and not list(export.rglob("OpenGDS2.pdb")):
        raise ValueError("Debug export is missing its managed game symbols")
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

Owner-supplied background and meeting tracks are bundled; Settings can mute or adjust them.
Local files in the music folder shown in Settings override the bundled playlist.
Original LZH importing is still required. See licenses/COMMUNITY_MUSIC_NOTICE.md.

Imported files are stored in Godot's user://original directory; saves are in
user://saves/slot1.json. Do not copy either directory into a public package.
Current gameplay includes development, release, sales, employee management,
training, reviews, recalls and verified calendar/annual events. Office progression
and other systems remain incomplete. This is a development checkpoint, not a completed port.

Code licensing: LICENSE. Original-content exclusions/disclaimers: notice.md.
Engine and runtime attribution: licenses/.
Source, validation and current limitations:
https://github.com/UnSpotibleShadow/OpenGDS2
"""
    if configuration == "debug":
        start = start.replace("OpenGDS2 development build", "OpenGDS2 debug build", 1)
        start += """
This package uses Godot's debug export and includes OpenGDS2.pdb C# symbols.
On Windows, use OpenGDS2.console.exe to keep diagnostics visible. To capture
diagnostics on either platform, launch with --log-file opengds2-debug.log.
Attach a compatible .NET debugger to the running game for managed debugging.
The original LZH requirement and game rules are the same as the release export.
"""
    (export / "GETTING_STARTED.txt").write_text(start,encoding="utf-8")
    if candidate:
        for filename, target in [("early-access-testing.md", "TEST_CHECKLIST.md"),
                                 ("early-access-known-issues.md", "KNOWN_ISSUES.md"),
                                 ("early-access-release-notes.md", "RELEASE_NOTES.md"),
                                 ("early-access-decisions.md", "DECISIONS.md")]:
            shutil.copyfile(source / "docs" / filename, export / target)
        (export / "GETTING_STARTED.txt").write_text(
            f"OpenGDS2 {candidate_version.group(1)} — PRIVATE EARLY ACCESS TEST CANDIDATE\n\n"
            "Extract the complete ZIP and run OpenGDS2.exe or OpenGDS2.console.exe.\n"
            "No editor or separate .NET installation is needed. Import your legally obtained\n"
            "GameDev2DX.lzh from the title. No original game files are included.\n"
            "Read LICENSE, notice.md and licenses/ for code, engine, runtime and music notices.\n\n"
            "Data folder: %APPDATA%\\OpenGDS2-EarlyAccessTesting. Start a new company.\n"
            "Modern JSON saves are supported; original saves are not. Back up before upgrading.\n"
            "Load Previous Save recovers the previous valid slot without overwriting the current file.\n\n"
            "Read TEST_CHECKLIST.md, KNOWN_ISSUES.md, RELEASE_NOTES.md and DECISIONS.md.\n"
            "Beginner ends after Year 2, December, week 4. Normal is experimental ongoing play.\n"
            "Bankruptcy and company/office upgrades are incomplete; negative funds do not end play.\n"
            "Capture logs: OpenGDS2.console.exe --log-file opengds2-test.log\n"
            "Report the version, Windows version, steps and expected/actual results privately.\n"
            "Never publish imported originals, original saves or the LZH.\n\n"
            "This is a standard SDK/Godot release export prepared for owner testing.\n"
            "Native Windows acceptance remains pending; host checks do not establish it.\n"
            "BUILD_MANIFEST.json records provenance. This is not a public release.\n", encoding="utf-8")
        manifest = {
            "build_label": candidate_version.group(1), "source_commit": source_commit,
            "platform": platform, "configuration": configuration,
            "build_method": "standard Godot .NET source export",
            "private_early_access_candidate": True,
            "candidate_data_directory": "OpenGDS2-EarlyAccessTesting",
            "runtime_packs": [f"{name}/{ver}" for name, ver in sorted(runtime_packs)],
            "assembly_sha256": hashlib.sha256(next(export.rglob("OpenGDS2.dll")).read_bytes()).hexdigest(),
            "native_windows_acceptance": "pending owner testing",
        }
        (export / "BUILD_MANIFEST.json").write_text(json.dumps(manifest, indent=2)+"\n", encoding="utf-8")
    if early_access:
        version = candidate_version.group(1)
        for filename, target in [("early-access-testing.md", "TEST_CHECKLIST.md"),
                                 ("early-access-known-issues.md", "KNOWN_ISSUES.md"),
                                 ("early-access-decisions.md", "DECISIONS.md")]:
            text = (source / "docs" / filename).read_text(encoding="utf-8")
            if target == "KNOWN_ISSUES.md":
                text = text.replace("# Known limitations — 0.1.0-ea.1-rc.4", f"# Known limitations — {version}", 1)
                text = text.replace("This private test candidate requires", "This Early Access release requires", 1)
            # Keep historical evidence intact; identify the artifact these notes accompany.
            text = f"Packaged build: {version}. Report bugs: {report_url}\n\n" + text
            (export / target).write_text(text, encoding="utf-8")
        notes = (source / "docs" / "early-access-public-notes.md").read_text(encoding="utf-8")
        notes = notes.replace("@VERSION@", version).replace("@REPORT_URL@", report_url).replace("@DATA_DIRECTORY@", data_directory)
        (export / "RELEASE_NOTES.md").write_text(notes, encoding="utf-8")
        (export / "GETTING_STARTED.txt").write_text(
            f"OpenGDS2 {version} — COMMUNITY EARLY ACCESS\n\n"
            "Extract the entire ZIP and run OpenGDS2.exe or OpenGDS2.console.exe.\n"
            "No editor or separate .NET installation is needed. Import your legally obtained\n"
            "GameDev2DX.lzh from the title. The self-extracting installer is not an import input.\n"
            "No original game files are included.\n\n"
            f"Data folder: %APPDATA%\\{data_directory}. Back up saves before upgrading.\n"
            "Modern JSON saves and previous-valid-save recovery are supported; original saves are not.\n"
            "Beginner ends after Year 2, December, week 4. Normal is experimental ongoing play.\n"
            "Casino, mystery lab, own hardware, bankruptcy and full company progression remain incomplete.\n\n"
            "Read RELEASE_NOTES.md, KNOWN_ISSUES.md and the community TEST_CHECKLIST.md.\n"
            "Full native Windows acceptance remains incomplete; community testing is welcome.\n"
            f"Report bugs: {report_url}\n"
            "Include version, Windows/display settings, reproduction steps and expected/actual results.\n"
            "Capture logs: OpenGDS2.console.exe --log-file opengds2-test.log\n"
            "Review logs before sharing; never upload the LZH, original files or original saves.\n\n"
            "Read LICENSE, notice.md and licenses/ for code, engine, runtime and music terms.\n"
            "BUILD_MANIFEST.json records source/runtime provenance; SHA256SUMS covers package files.\n",
            encoding="utf-8")
        manifest = {
            "build_label": version, "source_commit": source_commit,
            "platform": platform, "configuration": configuration,
            "build_method": "standard Godot .NET source export",
            "private_early_access_candidate": False, "public_early_access": True,
            "data_directory": data_directory, "community_report_url": report_url,
            "runtime_packs": [f"{name}/{ver}" for name, ver in sorted(runtime_packs)],
            "assembly_sha256": hashlib.sha256(next(export.rglob("OpenGDS2.dll")).read_bytes()).hexdigest(),
            "native_windows_acceptance": "incomplete; community testing",
        }
        (export / "BUILD_MANIFEST.json").write_text(json.dumps(manifest, indent=2)+"\n", encoding="utf-8")
    files = sorted(file for file in export.rglob("*") if file.is_file() and file.name != "SHA256SUMS")
    sums = "\n".join(hashlib.sha256(file.read_bytes()).hexdigest()+"  "+file.relative_to(export).as_posix() for file in files)+"\n"
    (export / "SHA256SUMS").write_text(sums,encoding="utf-8")
    output.parent.mkdir(parents=True,exist_ok=True)
    with zipfile.ZipFile(output,"w",compression=zipfile.ZIP_DEFLATED) as archive:
        for file in sorted(export.rglob("*")):
            if file.is_file():
                root = ("OpenGDS2-"+platform+"-"+candidate_version.group(1)) if candidate or early_access else "OpenGDS2-"+platform+"-x86_64"+("-debug" if configuration == "debug" else "")
                archive.write(file,Path(root) / file.relative_to(export))
    print(f"Desktop package: {output.name} ({output.stat().st_size} bytes)")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source",type=Path,default=Path("."))
    parser.add_argument("--export",type=Path,required=True)
    parser.add_argument("--platform",choices=("linux","windows"),required=True)
    parser.add_argument("--notices",type=Path,required=True)
    parser.add_argument("--output",type=Path,required=True)
    parser.add_argument("--configuration", choices=("release", "debug"), default="release")
    modes = parser.add_mutually_exclusive_group()
    modes.add_argument("--candidate", action="store_true", help="Include the private Windows Early Access handoff")
    modes.add_argument("--early-access", action="store_true", help="Include public Windows Early Access notes and provenance")
    parser.add_argument("--report-url", help="HTTPS community bug-report destination for public Early Access")
    parser.add_argument("--source-commit", help="Exact 40-character source commit for versioned provenance")
    args = parser.parse_args()
    package(args.source,args.export,args.platform,args.notices,args.output,args.configuration,args.candidate,args.source_commit,
            args.early_access,args.report_url)
