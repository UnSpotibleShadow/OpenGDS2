# Early Access release preparation

Prepared version: **0.1.0-ea.1**, proposed tag **v0.1.0-ea.1**.
The owner completed the final gameplay review on 16 October 2026 with no further
feedback. The gameplay baseline is PR131, `eca4921353cd48b5525cec3a1c3d351c30175b42`.
The owner will build and publish the release themselves. No build/export/package,
tag, GitHub release, upload or visibility change is part of this handoff. The
previous announced date was a target, not evidence of an available download.

Use [the owner handoff](release-handoff.md) for the remaining manual steps and
[copy-ready GitHub release notes](release-0.1.0-ea.1.md). Final package identity and
Windows acceptance cannot be recorded until that package exists. Historical
RC.4/M90 packages must not be relabeled as containing current source changes.
The permanent data folder is `OpenGDS2`; see [data migration](stable-user-data.md).

## Release gates

- [x] Owner completed final gameplay review with no further feedback (16 October 2026).
- [ ] Merge the reviewed release-preparation source; freeze its exact commit and version.
- [ ] Review tracked/history/hosted content before making the repository public; verify original-content exclusions and existing code/runtime/community-music notices.
- [ ] Build a fresh Windows x64 release export and identify its source, runtime, ZIP and checksums.
- [ ] Check that exact ZIP on Windows: launch, original LZH import, new company, save/exit/reload and a copied legacy test profile (including backup/settings). Verify migration preserves both old and existing destination data. Record unrun checks honestly.
- [ ] Present the concrete artifact, notes, limitations and validation for the final publication decision.
- [ ] After approval, make the repository public and publish the reviewed release/tag; verify unauthenticated issue access, asset download and the downloaded ZIP hash before announcing availability.

The full Windows/display/multi-year matrix remains community testing, per the
owner's existing decision. Unrun rows alone are not known defects. Complete native
presentation/RNG/save compatibility is not claimed. The current supported scope
and quirks are in [known issues](early-access-known-issues.md) and
[the decision register](early-access-decisions.md); use
[the Windows guide](early-access-testing.md) to record actual results.

## Build and package the exact source

Use Godot 4.4.1 .NET, matching official .NET release templates and .NET SDK 8.
The release configuration pins its self-contained runtime to .NET 8.0.31.
Use a fresh checkout/export directory at the frozen source commit and a clean
release build/import. Follow [building](building.md) and the bounded execution
rules in [AGENTS.md](../AGENTS.md). The restricted-host adapter is for local build
execution only and must not enter the distributed game.

Generate actual engine/runtime notices with `tools/export_licenses.gd`, then run:

```sh
python3 tools/package_desktop.py --source . --export /path/to/windows-export \
  --platform windows --configuration release --notices /path/to/notices \
  --early-access --source-commit <exact-source-sha> \
  --report-url https://github.com/UnSpotibleShadow/OpenGDS2/issues \
  --output /path/to/OpenGDS2-windows-0.1.0-ea.1.zip
```

Set NUGET_PACKAGES to the export's runtime-pack cache so its bundled .NET notices
match. The public mode requires a final EA version, exact SHA, HTTPS reporting
URL and explicit custom data folder; it rejects private-candidate/debug modes.
It writes BUILD_MANIFEST.json, SHA256SUMS, release notes, getting-started text,
limitations, community checklist, decisions and license notices. Public documents
identify the actual version/data folder; links between bundled notes remain local
and links to unbundled evidence point at the exact source commit.

Verify ZIP and PCK contents/digests, resource decoding, music and notices, plus
exclusion of original files, caches, saves and private traces. Produce an external
ZIP SHA-256 as well as the internal file list. Test the packaged executable, not
only an editor/source run. Linux/exported-host checks must not be reported as
native Windows playthrough acceptance.

## Publication and handoff

Keep Actions paused. Package preparation does not authorize a release, tag or
visibility change. The final decision reviews a concrete tested artifact and its
notes together. The reporting destination is
https://github.com/UnSpotibleShadow/OpenGDS2/issues; public access must be checked
after the visibility change, not assumed from that URL.

A previous upload to uploads.github.com returned HTTP401. Diagnose delivery
before relying on it; read back ambiguous mutations rather than retrying blindly.
If the configured upload route is unavailable, hand the owner the same verified
ZIP/checksum/notes for upload and verify the resulting download afterwards.

Historical export provenance is in [M73](windows-source-export-m73.md).
[Public release notes](early-access-public-notes.md) are the package template;
[release-note preparation](early-access-release-notes.md) describes current work.
Historical candidate checks are evidence for those artifacts only.
