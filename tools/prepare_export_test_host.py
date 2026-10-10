#!/usr/bin/env python3
"""Prepare a Linux validation host for an unchanged Windows PCK/game assembly.

This is a disposable test fixture, never a Windows package or native acceptance.
Supply the official matching Linux release template and .NET runtime NuGet pack.
"""
import argparse
import json
from pathlib import Path
import shutil
import zipfile


def prepare(windows, template, runtime_pack, output):
    if output.exists():
        raise ValueError("Use a new empty test-host directory")
    data = output / "data_OpenGDS2_linuxbsd_x86_64"
    data.mkdir(parents=True)
    source = windows / "data_OpenGDS2_windows_x86_64"
    for name in ["OpenGDS2.dll", "OpenGDS2.deps.json", "GodotSharp.dll"]:
        shutil.copyfile(source / name, data / name)
    with zipfile.ZipFile(runtime_pack) as archive:
        if archive.testzip() is not None:
            raise ValueError("The Linux runtime pack failed its ZIP integrity check")
        for name in archive.namelist():
            if name.startswith(("runtimes/linux-x64/lib/net8.0/", "runtimes/linux-x64/native/")) and not name.endswith("/"):
                (data / Path(name).name).write_bytes(archive.read(name))
    # Self-contained hostfxr resolves native libraries from the application's
    # deps metadata. Replace only that host-specific pack with the official
    # Linux pack metadata; the Windows game assembly and PCK stay byte-identical.
    app = json.loads((data / "OpenGDS2.deps.json").read_text())
    runtime = json.loads((data / "Microsoft.NETCore.App.deps.json").read_text())
    packs = [key for key in app["libraries"] if key.startswith("runtimepack.Microsoft.NETCore.App.Runtime.win-x64/")]
    if len(packs) != 1:
        raise ValueError("Expected one Windows self-contained runtime pack")
    old = packs[0]
    new = old.replace("win-x64", "linux-x64")
    official = new.removeprefix("runtimepack.")
    target = runtime["runtimeTarget"]["name"]
    if official not in runtime["targets"][target]:
        raise ValueError("Linux and Windows runtime versions must match")
    entries = app["targets"].pop(app["runtimeTarget"]["name"])
    entries.pop(old)
    entries[new] = runtime["targets"][target][official]
    for entry in entries.values():
        if "dependencies" in entry:
            entry["dependencies"] = {key.replace("Microsoft.NETCore.App.Runtime.win-x64", "Microsoft.NETCore.App.Runtime.linux-x64"): value
                                     for key, value in entry["dependencies"].items()}
    app["targets"][target] = entries
    app["runtimeTarget"]["name"] = target
    app["libraries"][new] = app["libraries"].pop(old)
    if "path" in app["libraries"][new]:
        app["libraries"][new]["path"] = app["libraries"][new]["path"].replace("win-x64", "linux-x64")
    (data / "OpenGDS2.deps.json").write_text(json.dumps(app, indent=2)+"\n")
    shutil.copyfile(template, output / "OpenGDS2.x86_64")
    (output / "OpenGDS2.x86_64").chmod(0o755)
    print("Prepared test host; Windows PCK and game assembly unchanged. Native Windows acceptance remains pending.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--windows", type=Path, required=True)
    parser.add_argument("--template", type=Path, required=True)
    parser.add_argument("--runtime-pack", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    prepare(args.windows, args.template, args.runtime_pack, args.output)
