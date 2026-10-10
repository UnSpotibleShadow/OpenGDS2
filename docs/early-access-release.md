# Early Access release preparation

Target version: **0.1.0-ea.1**, with private candidate **0.1.0-ea.1-rc.4**. A candidate is for owner testing, not authorization to publish, tag a release or make the repository public.

## Minimum scope

A community release needs a dependable new-company → hire/assign → develop → meetings → ship/reviews → sales/reprints → calendar/annual loop, safe modern saves, clear controls, local original import, English interface/audio and a Windows package tested on a fresh machine. Partial fidelity is acceptable only with visible boundaries and no supported workflow dead ends.

The supported facilities include school, hot springs, recycling, technology capacity, production expansion and lawyer consultation. Casino, mystery lab, own hardware, external work, polishing, original-save compatibility and final cosmetic parity are deferred. The explicit campaign boundary is Beginner’s verified ending plus experimental ongoing Normal play: bankruptcy and office/company upgrades are incomplete and negative funds do not end play. The title, mode selection and package disclose this scope. See [decision register](early-access-decisions.md).

## Public release gates

- [x] Private versioned candidate with visible build identification.
- [x] Candidate data directory separated from ordinary development data.
- [x] Known limitations, owner test checklist, release-note draft and bug template prepared.
- [x] Durable GitHub backlog: [#55](https://github.com/UnSpotibleShadow/OpenGDS2/issues/55).
- [ ] Owner completes the Windows matrix in early-access-testing.md, including fresh import and at least three game years.
- [ ] Resolve every crash, save corruption, duplicate transaction and supported-workflow dead end reported during testing; rerun relevant regressions.
- [x] Decide and visibly document the allowed campaign boundary (EA-02); final owner release acceptance remains separate.
- [x] Restore all twelve exact seasonal source OGGs locally and pass source integrity checks, preserving existing remote blobs (2026-10-07).
- [x] Produce a clean Windows source export using Godot 4.4.1 .NET/.NET 8 and matching templates; packaged resource, license and checksum checks passed in M73. Native Windows acceptance remains separate.
- [ ] Review community-recording distribution permission, code/runtime notices and original-content exclusions for the actual public package.
- [x] Merge the validated M56–M62 branch stack in order (#50 → #51 → #52 → #53 → #54 → #57 → #58); owner confirmed the complete branch/PR/merge workflow on 2026-10-07.
- [ ] Owner approves the exact source SHA, final Windows ZIP/hash, known-issue list, supported scope and distribution destination.

Keep Actions paused. Never change repository visibility, publish a GitHub release or distribute to the community as part of candidate preparation. After approval, publish the reviewed artifact with notes/checksums and link the backlog/bug-report route. Keep source/asset licensing boundaries and required LZH import explicit.

## Reproduction

Install Godot 4.4.1 .NET, matching .NET export templates and .NET SDK 8. The
ExportRelease configuration pins the self-contained runtime to 8.0.31. On an
ordinary machine, build/import and run the Windows Desktop release exporter;
use the bounded commands in building.md. On this restricted Linux host, use
`tools/workspace_sdk.py` inside `tools/bounded_process.py`; see workspace-runtime.md.
No original archive or trusted old desktop artifact is needed to build the package.

Generate notices through `tools/export_licenses.gd` using that editor, then run:

```sh
python3 tools/package_desktop.py --source . --export /path/to/windows-export \
  --platform windows --configuration release --notices /path/to/notices \
  --candidate --source-commit <exact-source-sha> \
  --output /path/to/OpenGDS2-windows-0.1.0-ea.1-rc.4.zip
```

Set NUGET_PACKAGES to the cache used for the export so runtime notices match the
actual publish. BUILD_MANIFEST.json records source/assembly/runtime provenance;
SHA256SUMS covers all packaged files. Include current testing, limitation,
release-note and decision documents. Validate every PCK payload digest,
resource decoding, archive integrity and content exclusions before handoff.
Native Windows test results remain separate from exported-host checks.

See [M73 validation](windows-source-export-m73.md) for this source export's
provenance, checks and remaining acceptance gates. Actions remain paused.
