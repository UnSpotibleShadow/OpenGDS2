#!/usr/bin/env python3
"""Local fallback compiler for workspaces without /proc; not a release exporter."""
import json
import hashlib
import os
from pathlib import Path
import subprocess
import sys
import zipfile
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
WORKSPACE = ROOT.parent
SDK = WORKSPACE / "toolchain/dotnet"
GODOT = WORKSPACE / "toolchain/godot/Godot_v4.4.1-stable_mono_linux_x86_64"
BUILD = WORKSPACE / "toolchain/build"
BUILD.mkdir(parents=True, exist_ok=True)
env = os.environ.copy()
env.update(LD_PRELOAD=str(WORKSPACE / "toolchain/thread-entry.so"),
           DOTNET_ROOT=str(SDK), DOTNET_EnableDiagnostics="0",
           DOTNET_PROCESSOR_COUNT="2", DOTNET_GCHeapHardLimit="20000000")

mode = sys.argv[1] if len(sys.argv) > 1 else "domain"
if mode not in ("domain", "app", "export"):
    raise SystemExit("usage: workspace_roslyn_build.py domain|app|export")
sdk_version = sorted((SDK / "sdk").glob("8.*"), key=lambda p: tuple(map(int, p.name.split("."))))[-1].name
reference_version = sorted((SDK / "packs/Microsoft.NETCore.App.Ref").glob("8.*"),
                           key=lambda p: tuple(map(int, p.name.split("."))))[-1].name
global_usings = BUILD / "GlobalUsings.cs"
global_usings.write_text("\n".join("global using " + ns + ";" for ns in
    ["System", "System.Collections.Generic", "System.IO", "System.Linq",
     "System.Net.Http", "System.Threading", "System.Threading.Tasks"]) + "\n")
refs = sorted((SDK / "packs/Microsoft.NETCore.App.Ref" / reference_version / "ref/net8.0").glob("*.dll"))
args = ["-nologo", "-langversion:12", "-nullable:enable", "-nostdlib+", "-debug:portable"]
if mode == "domain":
    project = ROOT / "tests/DomainTests/DomainTests.csproj"
    sources = [project.parent / "Program.cs"]
    for item in ET.parse(project).iter("Compile"):
        sources.extend(project.parent.glob(item.attrib["Include"]))
    output = BUILD / "DomainTests.dll"
    args += ["-target:exe"]
else:
    sources = sorted((ROOT / "src").rglob("*.cs"))
    output = ROOT / (".godot/mono/temp/bin/ExportDebug/OpenGDS2.dll" if mode == "export"
                     else ".godot/mono/temp/bin/Debug/OpenGDS2.dll")
    output.parent.mkdir(parents=True, exist_ok=True)
    # Debug exports still use the export (Release) API, not the editor API.
    refs += [GODOT / ("GodotSharp/Api/Release/GodotSharp.dll" if mode == "export"
                     else "GodotSharp/Api/Debug/GodotSharp.dll")]
    package = next((GODOT / "GodotSharp/Tools/nupkgs").glob("Godot.SourceGenerators.*.nupkg"))
    generator_dir = BUILD / "generators"
    with zipfile.ZipFile(package) as archive:
        archive.extractall(generator_dir)
    generators = list(generator_dir.rglob("Godot.SourceGenerators.dll"))
    config = BUILD / "godot.globalconfig"
    config.write_text("is_global = true\n"
        "build_property.GodotProjectDir = " + str(ROOT) + "/\n"
        "build_property.GodotProjectName = OpenGDS2\n")
    args += ["-target:library", "-define:DEBUG;GODOT" if mode == "export" else "-define:DEBUG;TOOLS;GODOT",
             "-analyzer:" + str(generators[0]), "-analyzerconfig:" + str(config)]
args += ["-out:" + str(output)]
args += ["-reference:" + str(p) for p in refs]
args += [str(global_usings)] + [str(p.resolve()) for p in sources]
rsp = BUILD / (mode + ".rsp")
rsp.write_text("\n".join('"' + arg + '"' for arg in args) + "\n")
result = subprocess.run([str(SDK / "dotnet"),
    str(SDK / "sdk" / sdk_version / "Roslyn/bincore/csc.dll"), "@" + str(rsp)], env=env)
if result.returncode:
    raise SystemExit(result.returncode)
output.with_suffix(".runtimeconfig.json").write_text(json.dumps({"runtimeOptions": {
    "tfm": "net8.0", "framework": {"name": "Microsoft.NETCore.App", "version": reference_version}}}))
if mode == "export":
    output.with_suffix(".build.json").write_text(json.dumps({
        "configuration": "ExportDebug", "editor_api": False,
        "godot_api_sha256": hashlib.sha256(refs[-1].read_bytes()).hexdigest(),
        "assembly_sha256": hashlib.sha256(output.read_bytes()).hexdigest(),
        "sdk_version": sdk_version, "reference_version": reference_version,
    }, indent=2) + "\n")
print("Compiled", output)
