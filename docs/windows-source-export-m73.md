# Clean Windows source export — M73

RC.4 resolves release gate G-02. No gameplay mechanics changed in this milestone.
The SDK, resource importer, exporter and package tools now run from source on this
restricted host; the candidate uses release templates and self-contained runtime.
No previous desktop artifact, direct Roslyn assembly or recovered audio is reused.

## Toolchain and provenance

- Godot 4.4.1 .NET editor / official build 49a5bc7b6.
- .NET SDK 8.0.419; target net8.0; ExportRelease runtime pinned to 8.0.31.
- Official release archive: https://github.com/godotengine/godot-builds/releases/download/4.4.1-stable/Godot_v4.4.1-stable_mono_export_templates.tpz
- Selected official release members were fetched with HTTP byte ranges and ZIP
  member CRC verification. The whole 1.1 GB archive SHA512 was not verified;
  do not present member validation as a full-archive cryptographic check.
- Windows release engine SHA256:
  `00b78800def67efebf60aa3cdfb52c9f8d2df89e5ad6096e95d7f8aa8a06606f`.
- Official NuGet runtime/host packs 8.0.31 passed ZIP CRC and package identity checks.
- All 25 source OGGs were restored from immutable Git source and matched exact
  Git blob hashes. Old .godot cache was moved aside before SDK rebuild/import.
- Windows game assembly SHA256:
  `d11676fb3fd1912cb10e942bf84459a07881de0259588582cc046cf4206068e4`.
- Windows PCK SHA256:
  `1284edaf200e3aad54bd9edcc6c264b17e253cd379f7abdbba4371c9057b954c`.

## Build-host fix

CoreCLR needs a discoverable stack and SDK startup needs process metadata.
The existing pthread launcher remains host-only. The new path adapter supplies
minimal stat metadata and redirects absent /tmp operations into owned storage.
The missed NuGet calls were mkdtemp, rename and opendir during shared-memory
creation; Godot also requires chdir and mkstemp for cache/pack writes. Genuine
native shared-memory files and locks remain in use. No migration or mutex bypass
is used. The .NET PAL source supports this diagnosis:
https://github.com/dotnet/runtime/blob/v8.0.25/src/coreclr/pal/src/sharedmemory/sharedmemory.cpp

Godot's embedded MSBuild inspector needs an explicit MSBUILD_EXE_PATH because
/proc/self/exe is unavailable. Ordinary Windows builds require no adapters.
Build-host .so libraries, downloaded tools and local absolute NuGet paths are
excluded from Git and the desktop package. See workspace-runtime.md.

## Validation for RC.4

| Check | Result / scope |
| --- | --- |
| Empty-state standard SDK rebuild | Pass, zero warnings/errors |
| Fresh editor resource import | Pass; final exporter has no temporary-directory error |
| Standard Godot Windows Desktop release export | Pass, exit 0, fresh assembly/runtime/PCK |
| Domain tests via normal SDK | 334 pass; authored fixtures, no original runtime claim |
| Current UI smoke | Pass, no original import fixture |
| Source music checks | Seven pass; all 25 source blobs independently verified |
| Desktop packaging tests | Five pass, including candidate source identification/handoff and rejection gates |
| Exported host startup | Pass, dotnet metadata, managed title/version, translations and meeting streams |
| Exported host core loop | Pass, zero failures, fixed 60 FPS; synthetic development/shipment/sales/checkpoint paths |
| Exported host EA recovery/scope | Pass, zero failures, English/Japanese |
| Exported host meeting audio | Pass, zero failures; existing shutdown resource-leak warning remains in the isolated fixture |
| PCK payload/resource/exclusion audit | 184 payload MD5s; all 25 audio imports/remaps; no originals/private caches/tests/tools |
| Release archive/notices audit | All CRCs/checksums pass; actual runtime 8.0.31 and editor notices; no host adapters/debug symbols/original files |
| Native Windows graphics/import/multi-year play | Pending owner acceptance, G-01 |

The Linux test host uses the exact Windows game assembly and PCK, matching official
Linux release engine and .NET 8.0.31 runtime. Only its disposable dependency metadata
is adapted to Linux native libraries; the Windows package is unchanged. It is not
a Linux deliverable or a native Windows run. `tools/prepare_export_test_host.py`
reproduces this fixture and rejects mismatched runtime versions. Use separate
isolated data directories and bounded_process.py for tests; pass the Windows PCK
with --main-pack and an absolute source smoke-script path with --script.

## Handoff

`package_desktop.py --candidate --source-commit <exact SHA>` produces the private
Windows release handoff, current acceptance/decision documents, BUILD_MANIFEST.json
and SHA256SUMS. Final source/ZIP hashes and publication are recorded in issue #55.
Windows acceptance, final public-package review, reporting destination and exact
public-release approval remain outstanding. No Actions dispatch, release/tag or
visibility change was made. Optional mystery-lab reconstruction remains deferred.
