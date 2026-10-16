# OpenGDS2 working instructions

## Current work — release prepared, publication blocked, 16 October 2026

Owner authorized building and first public release. Workflow PR134 merged.
Release source is frozen at `83db494daf01a855fd7d9895142c7928265b58fd` and annotated
tag `v0.1.0-ea.1` is already pushed. NEVER recreate, move or retarget that tag.
Windows ZIP is built and verified; see docs/release-verification-0.1.0-ea.1.md.
Draft release ID415139616 exists, with notes and no assets. Repository stays private.
The GitHub integration rejects uploads.github.com asset upload with HTTP401 and
repository visibility changes with HTTP403. Reads confirmed no asset upload and
no visibility change. No unchanged upload/admin retries; these are connection
permission blockers, not outstanding user authorization. Actions remain paused.
Owner can upload the three verified /workspace/share release files, make the
repository public and publish the existing draft. Then verify anonymous access
and downloaded ZIP hash before announcing availability. On resume read remote
state first; never duplicate the existing draft/tag or overwrite assets.
Recovery: `/workspace/scratch/ea-release-20261016/`. Keep #95/#118 open.
Native Windows package acceptance is unrun; export-host checks are not native.
The optional legacy original/UI omnibus harness has stale expectations and 25
failures; focused fresh import/save/restart and migration checks passed separately.

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
- Create a feature branch, commit there and open a PR into main. Do not publish
  development commits directly to main. Preserve unrelated changes and generated
  sidecars; do not blanket-clean or stage them.
- Original LZH import remains required. Never commit imported original graphics,
  sounds, executables, databases, saves, extraction caches or full private traces.
  The supported PE hash is
  `4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.
- Keep the README Ko-fi section and code/runtime/community-music notices.
  Community audio has recorded distribution authorization and separate terms;
  the MIT code license does not grant standalone music rights.
- Keep Actions paused. Do not generate desktop debug packages, publish releases,
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
