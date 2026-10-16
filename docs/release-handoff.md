# Maintainer release handoff — 0.1.0-ea.1

The owner signed off the gameplay review on 16 October 2026 with no further
feedback. The owner subsequently authorized preparation and publication on GitHub,
superseding the earlier owner-only build/publication instruction. This checklist
records the required work, not evidence that a package has already passed it.
Actions remain paused. Follow [the release workflow](release-workflow.md).

## Source and prepared material

- Version: `0.1.0-ea.1`; suggested tag: `v0.1.0-ea.1` (not created).
- Gameplay baseline: PR131, `eca4921353cd48b5525cec3a1c3d351c30175b42`.
- Use the merged documentation-handoff commit or a later reviewed main commit
  for the final source. Record its actual full SHA in the package; do not use
  the earlier gameplay baseline if building later source.
- [Copy-ready GitHub release body](release-0.1.0-ea.1.md).
- [Build/package instructions](early-access-release.md#build-and-package-the-exact-source),
  [known issues](early-access-known-issues.md), [Windows checklist](early-access-testing.md)
  and [data migration](stable-user-data.md).

## Build and publication steps

1. Record the exact reviewed source commit. Use Godot 4.4.1 .NET, matching release
   templates and .NET SDK 8 with the pinned self-contained runtime. Build a fresh
   Windows x64 release export; do not reuse an old RC or debug artifact.
2. Package with the existing `tools/package_desktop.py --early-access` procedure.
   Keep engine/runtime/community-music notices and BUILD_MANIFEST.json/SHA256SUMS.
   Suggested ZIP: `OpenGDS2-windows-0.1.0-ea.1.zip`. Generate the real external
   SHA-256 after packaging. Original archives, assets, caches and saves stay out.
3. Check that exact ZIP on Windows: launch without editor/separate .NET install,
   import the supported LZH, start a company, save/exit/reload, check language/audio
   and copied legacy-profile migration. Back up saves first. Record actual results;
   gameplay sign-off does not establish these future package checks.
4. Before changing repository visibility, review tracked files, reachable history
   and hosted issues/attachments/releases/artifacts for private or original
   material. This handoff does not certify that public-content review complete.
5. Create the pre-release against the recorded source commit, attach the ZIP and
   external checksum, and paste the prepared notes. Add only actual artifact/test
   details. The historical automated upload HTTP401 does not establish whether a
   manual owner upload works; no upload was attempted here.
6. Verify the tag target and unauthenticated release/issue access, download the
   published ZIP and compare its SHA-256. Then replace README's preparation text
   with the actual release link and announce availability. Keep Actions paused.

No automatic closure of #95 or #118 is implied. Any source change after packaging
requires a fresh package and relevant checks. Full multi-year/display testing
remains community work; original saves and full native parity are not claimed.
