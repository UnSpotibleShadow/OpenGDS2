# Early Access release preparation

Target version: **0.1.0-ea.1**, with private candidate **0.1.0-ea.1-rc.4**. The announced target is Friday, 16 October 2026, at 19:00 CEST (17:00 UTC). The owner chose community testing for Early Access; completing the full native Windows matrix is not a pre-release gate. The existing RC.4 remains a private historical artifact, and announcing the target does not identify or publish the final package.

## Minimum scope

A community release needs a dependable new-company → hire/assign → develop → meetings → ship/reviews → sales/reprints → calendar/annual loop, safe modern saves, clear controls, local original import, English interface/audio and a Windows package with basic launch/import/save/reload checks. Multi-year play, display coverage and sustained native testing are community acceptance work. Disclose untested behavior; fix known crashes, save corruption, duplicate charges and supported-workflow dead ends before publishing.

The supported facilities include school, hot springs, recycling, technology capacity, production expansion and lawyer consultation. Casino, mystery lab, own hardware, external work, polishing, original-save compatibility and final cosmetic parity are deferred. The explicit campaign boundary is Beginner’s verified ending plus experimental ongoing Normal play: bankruptcy and office/company upgrades are incomplete and negative funds do not end play. The title, mode selection and package disclose this scope. See [decision register](early-access-decisions.md).

## Public release gates

- [x] Private versioned candidate with visible build identification.
- [x] Candidate data directory separated from ordinary development data.
- [x] Known limitations, owner test checklist, release-note draft and bug template prepared.
- [x] Durable GitHub backlog: [#55](https://github.com/UnSpotibleShadow/OpenGDS2/issues/55).
- [ ] Check final-package launch, original LZH import and a new-game save/reload; record exactly what ran and what remains untested.
- [x] Full nineteen-row Windows matrix moved to community testing by owner decision on 2026-10-11; multi-year playthrough is not a pre-release requirement.
- [ ] Resolve known crashes, save corruption, duplicate transactions and supported-workflow dead ends; rerun relevant regressions. Unrun community checklist rows do not count as known defects.
- [x] Decide and visibly document the allowed campaign boundary (EA-02); final owner release acceptance remains separate.
- [x] Restore all twelve exact seasonal source OGGs locally and pass source integrity checks, preserving existing remote blobs (2026-10-07).
- [x] Produce a clean Windows source export using Godot 4.4.1 .NET/.NET 8 and matching templates; packaged resource, license and checksum checks passed in M73. Native Windows community acceptance remains separate.
- [ ] Review community-recording distribution permission, code/runtime notices and original-content exclusions for the actual public package.
- [x] Merge the validated M56–M62 branch stack in order (#50 → #51 → #52 → #53 → #54 → #57 → #58); owner confirmed the complete branch/PR/merge workflow on 2026-10-07.
- [ ] Finalize the exact source SHA, final Windows ZIP/hash, known-issue list and distribution destination for owner publication approval.
- [x] Reporting destination chosen: the owner plans to make OpenGDS2 public before release; use https://github.com/UnSpotibleShadow/OpenGDS2/issues.
- [ ] Verify the repository and issue form are accessible without owner access after the owner changes visibility. Choosing the route does not close public-access verification.

Keep Actions paused. Never change repository visibility, publish a GitHub release or distribute to the community as part of candidate preparation. After approval, publish the reviewed artifact with notes/checksums and link the backlog/bug-report route. Keep source/asset licensing boundaries and required LZH import explicit.

## Reproduction

Install Godot 4.4.1 .NET, matching .NET export templates and .NET SDK 8. The
ExportRelease configuration pins the self-contained runtime to 8.0.31. On an
ordinary machine, build/import and run the Windows Desktop release exporter;
use the bounded commands in building.md. On this restricted Linux host, use
`tools/workspace_sdk.py` inside `tools/bounded_process.py`; see workspace-runtime.md.
No original archive or trusted old desktop artifact is needed to build the package. M80 changes source gameplay after RC.4: rebuild from the selected final source; do not relabel the existing RC.4 ZIP as containing later fixes.

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

## Public Early Access packaging — M81

The packager now has a separate `--early-access` mode; ordinary development and
private `--candidate` modes remain available. It requires a Windows release
export, a final `x.y.z-ea.n` project version (RC labels are rejected), exact source
commit, HTTPS reporting URL and explicit custom data directory. It does not
publish a release or verify that the reporting URL is publicly accessible.

Before the final export, select the final source/version and update the in-game
private-candidate wording. The current source still identifies RC.4; this command
intentionally refuses it. Build and export that final source normally, then:

```sh
python3 tools/package_desktop.py --source . --export /path/to/windows-export \
  --platform windows --configuration release --notices /path/to/notices \
  --early-access --source-commit <exact-source-sha> \
  --report-url https://github.com/UnSpotibleShadow/OpenGDS2/issues \
  --output /path/to/OpenGDS2-windows-0.1.0-ea.1.zip
```

Public packages include generated getting-started instructions, the public
release-note template, limitations/community checklist/decision notes, runtime
licenses, source/assembly provenance and checksums. They record native Windows
acceptance as incomplete; no owner-test completion or public URL availability
is inferred. The data directory is read from the source project settings.
Reusing an export directory replaces private candidate handoff files with the
public ones, but a fresh final export is still required for updated gameplay.

Ten authored-fixture package tests pass, including full archive checksum
verification, version/source/mode guards, reporting/data requirements, original
content exclusions and replacement of a private-candidate handoff. These tests
create no distributable game build and do not establish native acceptance.
