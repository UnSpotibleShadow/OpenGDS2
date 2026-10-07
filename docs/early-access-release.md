# Early Access release preparation

Target version: **0.1.0-ea.1**, with private candidate **0.1.0-ea.1-rc.1**. A candidate is for owner testing, not authorization to publish, tag a release, merge PRs or make the repository public.

## Minimum scope

A community release needs a dependable new-company → hire/assign → develop → meetings → ship/reviews → sales/reprints → calendar/annual loop, safe modern saves, clear controls, local original import, English interface/audio and a Windows package tested on a fresh machine. Partial fidelity is acceptable only with visible boundaries and no supported workflow dead ends.

Other facility services, external work, polishing, exact original-save compatibility and final cosmetic parity can be deferred. Company progression and bankruptcy require an explicit decision: implement supported evidence-backed behavior or clearly limit the campaign. The current candidate does not invent those rules.

## Public release gates

- [x] Private versioned candidate with visible build identification.
- [x] Candidate data directory separated from ordinary development data.
- [x] Known limitations, owner test checklist, release-note draft and bug template prepared.
- [x] Durable GitHub backlog: [#55](https://github.com/UnSpotibleShadow/OpenGDS2/issues/55).
- [ ] Owner completes the Windows matrix in early-access-testing.md, including fresh import and at least three game years.
- [ ] Resolve every crash, save corruption, duplicate transaction and supported-workflow dead end reported during testing; rerun relevant regressions.
- [ ] Decide and document the allowed campaign boundary for incomplete bankruptcy/company progression.
- [ ] Restore all twelve exact seasonal source OGGs locally and pass source integrity checks, preserving existing remote blobs.
- [ ] Produce a clean Windows source export using Godot 4.4.1 .NET/.NET 8 and matching templates; compare packaged resource, license and checksum checks with the tested candidate. The reused-runtime fallback is not evidence of this gate.
- [ ] Review community-recording distribution permission, code/runtime notices and original-content exclusions for the actual public package.
- [ ] Review/merge the branch stack in order (#50 → #51 → #52 → #53 → #54 → #57 → M62 PR) after approval. Reconcile current remote state first.
- [ ] Owner approves the exact source SHA, final Windows ZIP/hash, known-issue list, supported scope and distribution destination.

Keep Actions paused. Never change repository visibility, publish a GitHub release or distribute to the community as part of candidate preparation. After approval, publish the reviewed artifact with notes/checksums and link the backlog/bug-report route. Keep source/asset licensing boundaries and required LZH import explicit.

## Reproduction

The candidate uses project.godot version/isolation settings. `tools/write_candidate_config.gd` serializes those settings to a private project.binary. Run the bounded app/domain/UI checks from AGENTS.md; then use a copied trusted Windows base with `tools/workspace_debug_package.py --platform windows --source-commit <verified-sha> --build-label 0.1.0-ea.1-rc.1 --project-config <generated-project.binary> --candidate`.

The fallback checks every reused PCK digest, includes current meeting audio/imports, the current assembly/translations and the three owner-facing documents. BUILD_MANIFEST.json and SHA256SUMS record provenance and contents. Check the packed settings, resource decoding, archive integrity and current assembly digest before offering the candidate. Native Windows test results must be recorded separately, not inferred from host checks.
