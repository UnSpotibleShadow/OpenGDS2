# Release handoff — 0.1.0-ea.2

The owner delegated preparation, Windows 32/64-bit builds, verification and GitHub
prerelease publication on 19 October 2026. The preceding visual review is accepted.
Follow [the current release procedure](early-access-release.md) and use
[the copy-ready notes](release-0.1.0-ea.2.md). #151 is the live completion checklist.

Version: **0.1.0-ea.2**. Tag: **v0.1.0-ea.2**, targeting the verified main merge.
Use a feature PR into develop, then a develop → main PR. The latter starts the
read-only Actions build workflow; tagging/publication is a separate authorized
operation after package checks pass. Do not alter ea.1 or reuse historical ZIPs.

Both `OpenGDS_v0.1.0-ea.2_windows_x86_32.zip` and
`OpenGDS_v0.1.0-ea.2_windows_x86_64.zip` need matching SHA-256 files. Publish release builds
only. The permanent profile folder remains `%APPDATA%\OpenGDS2`.

Automated Windows packaged checks and the owner's source-play review are distinct
from a fresh manual Windows release-package playthrough. Record actual results
and retain known limitations. Never claim manual tests that were not run.
