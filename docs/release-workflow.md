# Branches, tags and releases

Use a lightweight workflow: reviewed source on `main`, short-lived topic branches
and pull requests. There is no permanent `develop` branch. Keep automatic Actions
paused until the maintainer explicitly changes that decision; manual checks and
PR evidence remain valid while CI is paused.

## Normal changes

Branch from current main using `feature/…`, `fix/…` or another descriptive name.
Keep each PR focused, describe the behavior and relevant verification, and merge
only reviewed working source. Original binaries, archives, imported assets,
private saves and extraction caches never belong in Git or packages. Retain
code, runtime and separately authorized community-music notices.

## Release

1. Select a reviewed main commit and record its full SHA and version. If feature
   development must continue during stabilization, create `release/<version>`
   and allow release fixes only. Otherwise build directly from frozen main.
2. Export/package from a clean checkout at that exact commit. Record source,
   toolchain/runtime, checksums and actual validation. Unrun Windows checks must
   stay explicitly unrun. Any source change requires a new package/recheck.
3. After package verification, create an annotated tag `v<version>` at the exact
   built commit. For this first release use `v0.1.0-ea.1`. Mark Early Access as a
   GitHub pre-release and attach the complete ZIP and external SHA-256 file.
4. Verify tag target, release assets and downloaded ZIP hash; check anonymous
   access for a public release. Only then announce a working download link.

Published tags and artifacts are immutable: never move a published tag or replace
a package silently. Corrections get a new version/tag/package. Draft preparation
is not public availability. Keep main free for the next version after release.

## Fixing a released version

Branch `hotfix/<version>-<description>` from the published tag. Fix and verify,
then release a new version, normally the next Early Access sequence such as
`0.1.0-ea.2`. Bring the fix into main (and any active release branch) through PRs.
Avoid duplicate application when the fix already exists there. Never reset main
to an old tag. Release branches can be deleted after their changes are merged;
published tags remain.

See [release preparation](early-access-release.md), [known issues](early-access-known-issues.md)
and [the maintainer checklist](release-handoff.md).
