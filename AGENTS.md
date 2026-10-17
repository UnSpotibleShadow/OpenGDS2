# OpenGDS2 working instructions

## Current work — fidelity follow-ups, 17 October 2026

The owner authorized the ten ordered findings in
[fidelity follow-ups](docs/fidelity-followups-20261017.md). Complete one step,
then ask before starting the next, naming that next step. Only step 1 is currently
authorized. Follow git-flow: `feature/...` branches start from `develop` and PRs
target `develop`; `main` receives reviewed release/hotfix integrations. The remote
had only main, so develop starts at its current released-source descendant.
The owner explicitly permits Actions again now the repository is public. Use
validation-only CI for these steps; no release, tag, asset replacement or desktop
packaging is requested. These instructions supersede the older pauses and direct
feature-to-main workflow below. Preserve the published release.

## Current work — post-release community handoff, 16 October 2026

The owner published v0.1.0-ea.1 with Windows x86_32/x86_64 ZIPs and checksums;
repository and player wiki are public. Published release ID415162340. The older
agent-prepared artifact/draft is historical and must not be confused with the
owner's uploaded binaries. Never move the v0.1.0-ea.1 tag or replace assets.
Owner explicitly authorized branch/issue cleanup, superseding earlier keep-open
instructions for #95/#118. 130 obsolete remote branches were removed after main
ancestry or exact merged-PR/tree-equivalence checks; a local bundle and all old
heads are retained externally. Only main remained before this housekeeping PR.
Closed #95/#118 as completed review rounds; #55 superseded by repository handoff.
Keep research #94 (native Windows pacing measurement) and #97 (exhaustive artwork
coverage) open, marked help wanted. Community bugs should get focused issues
with version, architecture and reproduction steps. Do not imply full parity.
Recovery: `/workspace/scratch/post-release-cleanup-20261016/`.
Finish reviewed documentation sync, remove its merged remote topic branch and
safe merged local branches, then read back remaining refs/issues/tags. Preserve
unmerged work and active worktrees. No builds, release changes or Actions dispatch.
The player wiki is live; canonical source is docs/wiki. Keep wiki and unrelated
local generated sidecars intact. Source PR merge/snapshot sync remain authorized.

## Current source and history

- PR123 implements the nine facility conversation chains and all six promotion
  media; PR124 corrects the TV patch; PR125 adds display-only English game names.
- Read [current development checkpoint](docs/development-checkpoint.md),
  [release preparation](docs/early-access-release.md) and the relevant feature
  evidence before changing a workflow. The historical handoff is archived in
  [docs/archive/agent-handoff-through-20261015.md](docs/archive/agent-handoff-through-20261015.md).
  Its superseded next steps are not new work orders.
- Do not describe static inventory coverage as runtime parity. Original save
  compatibility, whole-session native RNG and complete original presentation are
  not established. Do not retry the unchanged Wine runner, which crashes before
  a usable original window. Native Windows evidence must be recorded separately.
- Office relocation, separate polishing/external-job workflows and automatic
  student growth need original evidence before implementation.

## Workspace and content boundaries

- Full git checkout: `/workspace/scratch/m105-review-git`. The primary snapshot
  `/workspace/OpenGDS2` has no .git; sync only explicit verified changed files
  after merge, checking its base contents before replacement.
- Create a feature branch from develop, commit there and open a PR into develop.
  Do not publish development commits directly to main. Preserve unrelated changes and generated
  sidecars; do not blanket-clean or stage them.
- Original LZH import remains required. Never commit imported original graphics,
  sounds, executables, databases, saves, extraction caches or full private traces.
  The supported PE hash is
  `4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.
- Keep the README Ko-fi section and code/runtime/community-music notices.
  Community audio has recorded distribution authorization and separate terms;
  the MIT code license does not grant standalone music rights.
- Actions source validation is authorized. Do not generate desktop debug packages, publish releases,
  change visibility or communicate externally without the relevant authorization.
- Use the existing GitHub connection; never ask for a separate browser login.
  A historical uploads.github.com HTTP401 is unresolved. Diagnose the upload
  route before release delivery; do not blindly retry or claim an upload worked.
- Local SDK/Godot work must be sequential and bounded. Reuse successful results
  when still applicable; broaden/repeat checks only for a relevant change or
  unresolved failure. Stop only owned background processes after verifying them.

## Bounded local checks

- Run potentially blocking builds, imports, exports and Godot checks through
  `python3 tools/bounded_process.py`. Supply an explicit wall deadline, an external
  recovery checkpoint and a log. Use separate deadlines for separate phases.
- Default local-check deadline: 180 seconds. Set a longer deadline only for a
  known longer operation and explain the reason. Emit heartbeats every 15 seconds.
  Yield shell tool calls within 1 second and inspect the log between polls.
- Godot smoke checks must use `--fail-pattern 'SCRIPT ERROR:|Unhandled exception'`
  so a broken script cannot leave the engine waiting indefinitely.
- Silence alone does not establish a hang. Enable `--idle-timeout` only when the
  command normally reports progress. The absolute deadline always remains active.
- After a timeout or fatal error, stop that attempt, inspect its checkpoint/log,
  and make a concrete change before rerunning. Do not repeatedly poll or replay
  the same failing check without new evidence. POSIX watchdogs terminate the
  command's isolated process group, including descendants.

## Connector calls and publication

- Use `tools/connector_guard.mjs` for connector orchestration. It is dependency
  free JavaScript; `createConnectorGuard` can also be copied into code-mode calls.
  Save every state transition to an external JSON checkpoint before mutations.
  On recovery, restore a `started`/`unknown` mutation using `pendingWrite`.
- Connector deadline: 20 seconds normally, at most 45 seconds. Yield code-mode
  execution within 1 second to retain control and provide progress updates.
- A timeout ends waiting; it does **not** prove cancellation. Treat a timed-out,
  rejected, errored or unverified mutation as `unknown`. Do useful independent
  work, then reconcile by reading remote state. Never blindly repeat that write.
- Every mutation needs a read-back verifier. For ref updates, checkpoint the
  expected old/new SHA, use `force:false`, and read the ref before and after the
  update. If the desired SHA is already present, the task is complete. If it has
  moved elsewhere, inspect that state; do not reset it or assume the earlier
  update failed. Apply the same read-before-retry rule to branches, PRs and merges.
- An unresolved write blocks subsequent mutations. A different ref value is
  not permission to overwrite concurrent work. Confirm success only from read-back.
- Recovery records contain labels, operation type and necessary public IDs/SHA
  values; do not record credentials, environment variables or private assets.

Keep user-facing updates within 60 seconds. These safeguards cannot control a
platform/UI stall outside the orchestrator; do not claim they can cancel every
connector request. See `docs/execution-safeguards.md` for commands and limitations.
