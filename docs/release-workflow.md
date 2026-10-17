# Branches, tags and releases

Use git-flow: `main` holds released source and `develop` integrates upcoming work.
The maintainer authorized this process and resumed Actions on 17 October 2026.
Source validation may run automatically; a successful check does not authorize a
release. The current fidelity follow-ups do not create releases or tags.

## Normal changes

Branch from current `develop` using `feature/<description>`. Open focused PRs back
into `develop`, describe behavior and verification, and integrate reviewed working
source after relevant checks pass. Do not merge individual feature PRs into main.
Original binaries, archives, imported assets,
private saves and extraction caches never belong in Git or packages. Retain
code, runtime and separately authorized community-music notices.

## Release

1. After the maintainer authorizes a release, branch `release/<version>` from a
   reviewed develop commit. Allow release fixes only; other work stays on develop.
2. Export/package from a clean checkout at that exact commit. Record source,
   toolchain/runtime, checksums and actual validation. Unrun Windows checks must
   stay explicitly unrun. Any source change requires a new package/recheck.
3. Merge the verified release branch into main through a PR and bring release
   fixes back into develop. The main integration must have the verified source
   tree. Tag that release commit with `v<version>`, recording both the tested
   source SHA and integration SHA if different. Mark Early Access as a GitHub
   pre-release and attach the complete ZIP and external SHA-256 file.
4. Verify tag target, release assets and downloaded ZIP hash; check anonymous
   access for a public release. Only then announce a working download link.

Published tags and artifacts are immutable: never move a published tag or replace
a package silently. Corrections get a new version/tag/package. Draft preparation
is not public availability. Delete release branches only after both integrations.

## Fixing a released version

Branch `hotfix/<version>-<description>` from the released main commit. Fix and verify,
then release a new version, normally the next Early Access sequence such as
`0.1.0-ea.2`. Bring the fix into main and develop (and any active release branch) through PRs.
Avoid duplicate application when the fix already exists there. Never reset main
to an old tag. Release branches can be deleted after their changes are merged;
published tags remain.

See [release preparation](early-access-release.md), [known issues](early-access-known-issues.md)
and [the maintainer checklist](release-handoff.md).
