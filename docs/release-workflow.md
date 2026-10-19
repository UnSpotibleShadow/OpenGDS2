# Branches, tags and releases

Use git-flow: `main` holds released source and `develop` integrates upcoming work.
The maintainer authorized this process and resumed Actions on 17 October 2026.
Source validation may run automatically; a successful check does not authorize a
release. The owner authorized the 0.1.0-ea.2 publication on 19 October 2026 after
approving the fidelity review; see the current release-preparation document.

## Normal changes

Branch from current `develop` using `feature/<description>`. Open focused PRs back
into `develop`, describe behavior and verification, and integrate reviewed working
source after relevant checks pass. Do not merge individual feature PRs into main.
Original binaries, archives, imported assets,
private saves and extraction caches never belong in Git or packages. Retain
code, runtime and separately authorized community-music notices.

## Automatic desktop builds

Only a merged pull request whose source is this repository's `develop` and whose
base is `main` starts **Build merged main** (`.github/workflows/main-build.yml`).
Closed but unmerged PRs, other source branches, forks, direct pushes and ordinary
feature/develop activity do not export packages. There is no manual dispatch for
this workflow. Source validation still runs on PRs and develop independently.

All jobs check out the PR's exact merge commit, including if main advances before
the run starts. Domain and Godot checks must pass before export. The existing
presets produce Windows x86_64 and Linux x86_64 release/debug ZIPs, bundled notices
and external SHA-256 files, with packaged smoke checks on both operating systems.
The Windows Desktop 32-bit preset additionally exports x86_32 release packages,
which run the same packaged Windows core-loop checks as x86_64. Download artifacts from the eligible
run's Actions page; artifact names include the full merge SHA and expire after
14 days. Windows release ZIPs carry Early Access source/runtime manifests and notes;
Linux/debug ZIPs remain development artifacts. Publication is a separate step.

The workflow has read-only repository permissions and never tags, publishes or
replaces release assets. A skipped build on another PR source is intentional.
To exercise the full workflow, promote reviewed develop through a PR into main;
do not merge merely to test this automation.

## Release

1. Prepare a reviewed develop commit after the maintainer authorizes promotion.
   If a `release/<version>` stabilization branch is useful, merge its verified
   fixes back into develop first. Other feature work stays outside that promotion.
2. Open a PR **from develop into main** and merge after source checks pass. That
   exact branch pair triggers the build workflow above. A release/hotfix branch
   merged directly into main does not trigger it.
3. Review the resulting packages, source SHA, checksums and actual validation.
   Unrun acceptance checks stay explicitly unrun. Any source change requires a
   new eligible promotion/package/recheck. Publishing remains a separate maintainer
   decision: tag the tested main commit with `v<version>`, mark Early Access as a
   GitHub pre-release and attach the approved ZIPs and external SHA-256 files.
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

## One-time ea.2 publication

The connected credential can edit release metadata but receives HTTP401 on
uploads.github.com. The owner authorized completing publication through Actions.
A manual `Fidelity validation` dispatch on develop with `publish_ea2=true` uses a
separate, narrowly scoped write-permission job after source validation. Normal
push/PR validation stays read-only; Build merged main is unchanged.

`tools/publish_ea2.mjs` accepts only the exact successful build39251557441,
source `fb4d8f2969b6288349f4f1db0b68f71438003e82` and preverified Windows ZIP
hashes. It does not build, overwrite an asset or move a tag. Guarded writes record
checkpoints; publication is followed by anonymous download/hash verification.
The audit record is uploaded as an Actions artifact. This route publishes only
v0.1.0-ea.2; it is not automatic publication of future main merges.
