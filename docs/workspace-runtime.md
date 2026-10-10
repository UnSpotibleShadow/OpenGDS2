# Builds on the restricted workspace host — M73

The normal SDK/MSBuild path now works. `tools/workspace_sdk.py` runs the installed
.NET SDK or Godot using build-host adapters; it does not compile game code itself
or patch export templates. Use it inside `tools/bounded_process.py`, with the
usual separate 180-second deadlines, checkpoints and logs from AGENTS.md.

The host lacks `/proc` and `/tmp`. `workspace_thread_entry.c` supplies a real
pthread stack. `workspace_sdk_paths.c` redirects `/tmp` operations into an owned
state directory, including directory creation/opens, chdir and temporary files.
NuGet still uses real filesystem/shared-memory locks. No mutex or migration is
bypassed. A minimal process stat record permits SDK startup; it is not general
process emulation. Memory maps/command lines/debugging are unsupported. The helper
sets MSBUILD_EXE_PATH to the installed SDK for Godot's embedded project inspector.
It never changes HOME. These adapters must never enter a desktop package.

Requirements: Python 3, cc, installed .NET SDK 8, Godot 4.4.1 .NET and matching
official .NET release templates. Defaults expect toolchain/dotnet and the 4.4.1
Linux .NET editor beneath the workspace; override --sdk/--godot if different.
Use an absolute path for native executables. State includes isolated CLI home,
NuGet cache, temporary storage and Godot data/template cache. Install templates
under STATE/godot-data/godot/export_templates/4.4.1.stable.mono. Configure the
Godot-local nupkg feed and official NuGet feed using a local NuGet.Config;
machine-specific paths are not committed. ExportRelease pins runtime 8.0.31.

Example from the repository (replace absolute tool/project/output paths):

```sh
python3 tools/bounded_process.py --timeout 180 --heartbeat 15 \
  --label windows-export --checkpoint /work/export.json --log /work/export.log \
  --fail-pattern 'SCRIPT ERROR:|Unhandled exception' -- \
  python3 tools/workspace_sdk.py --state /work/sdk-state -- \
  /absolute/Godot_v4.4.1-stable_mono_linux.x86_64 --headless --path /absolute/OpenGDS2 \
  --export-release "Windows Desktop" /work/windows/OpenGDS2.exe
```

Run SDK build, domain tests and fresh editor import through the same wrapper,
then export and generate notices in separate bounded invocations. Do not infer
success merely from exit zero: inspect logs and required outputs. M73 discarded
the old .godot cache, restored all 25 exact source OGGs from immutable Git blobs,
ran a fresh-state SDK rebuild and regenerated imports before source export.
See [validation](windows-source-export-m73.md).

## Historical fallback

The direct Roslyn/reused-artifact tools remain available for historical explicit
debug-build requests. They are not needed for the M73 source export and cannot
close its clean-export gate. `workspace_roslyn_build.py export` produces an export
API assembly; editor TOOLS assemblies must never replace exported game code.
The fallback's checksum receipts, feature-preserving project config merge and
provenance rules still apply. Never silently use fallback resources when claiming
a fresh source export. Source audio integrity requires exact original community
OGG blobs, not imported resources recovered from an old artifact.

GitHub Actions remain paused; these tools dispatch no remote workflow.
Native Windows execution requires the owner's Windows acceptance matrix.
