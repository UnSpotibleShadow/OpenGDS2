# Early Access release preparation — 0.1.0-ea.2

The owner approved the final gameplay review and PR158, then authorized the whole
release on 19 October 2026: bump the version, integrate through develop → main,
build and verify Windows **x86_32 and x86_64**, and publish the GitHub prerelease.
No further approval pause is required for these steps. Preserve the published
v0.1.0-ea.1 tag and packages.

Version identity lives in `project.godot` and `src/Core/BuildInfo.cs`. The tag will
be `v0.1.0-ea.2` at the exact tested main merge commit. Release preparation starts
from the reviewed develop commit `3ff22a05cad88ce523a660d94f3eaf18d2fbf6f1`.

1. Merge the validated release-preparation feature PR into develop.
2. Merge the validated develop → main PR; only that branch pair triggers exports.
3. Require the entire Build merged main run to pass, including packaged Windows
   startup and development/release/save-reload core-loop checks for both targets.
4. Download the exact Actions packages; verify PE architecture, internal manifest
   source/version/runtime, SHA256SUMS and external ZIP checksums. Inspect content
   exclusions and code, engine, runtime and community-music notices.
5. Publish the tested commit as a GitHub prerelease with the two Windows ZIPs and
   their `.sha256` files, using [the release body](release-0.1.0-ea.2.md).
6. Verify anonymous asset downloads against the checked hashes. Finish #151 only
   after publication and download verification.

Expected public filenames:

- `OpenGDS_v0.1.0-ea.2_x86_32.zip` and `.zip.sha256`
- `OpenGDS_v0.1.0-ea.2_x86_64.zip` and `.zip.sha256`

Windows release packages include BUILD_MANIFEST.json with the exact source SHA,
version, self-contained runtime identifiers and assembly digest; SHA256SUMS covers
all package files. No imported original assets, private data or saves may ship.
Linux and debug exports are CI artifacts, not part of this Windows publication.

The permanent data folder remains `OpenGDS2`. Back up modern saves before
upgrading. Original-game saves remain unsupported. Full native Windows manual
playthrough, display/hardware and multi-year coverage is not established by
headless Actions. Consumer Investigation is accepted with the recorded manual-test
waiver. Preserve those limits in published notes; do not label unrun rows passed.

Earlier ea.1 agent build/upload records are historical; the owner's public ea.1
release is immutable. See [release workflow](release-workflow.md),
[known limitations](early-access-known-issues.md) and [testing](early-access-testing.md).
