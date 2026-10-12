# Development execution safeguards

## Current handoff — 2026-10-12, M90

Start by reading GitHub issue #55 and checking current `main` and open PRs through
the owner's connector. This workspace can be a snapshot without `.git`; remote
state is authoritative. Preserve the remote base tree when publishing explicit
changed paths. Do not mirror snapshot omissions into repository deletions.

The following milestones are implemented; do not redo their investigations or
replace their documented native quirks with guessed behavior:

- M82–M85: laboratory, hardware lifecycle, financial failure and casino poker.
- M86, PR #86: blackjack, retained form state, ace retry and double-up timing;
  see `docs/casino-blackjack-m86.md`.
- M87, PR #87: card-chain, initial-deal bonus, keyboard paths, shared double-up
  and safe exhausted-deck boundary; see `docs/casino-card-chain-m87.md`.
- M88, PR #88: locally recovered 44-runner/10-jockey catalog, member aliases and
  selector evidence; see `docs/horse-catalog-m88.md`. Native names stay private.
- M89, PR #89: horse wagers, odds, both physics modes, retained finish behavior
  and temporary winnings overwritten by the next setup; see
  `docs/casino-horse-racing-m89.md`. Verified main before M90:
  `83277904a7424741cd2c1232cff051597cc43156`.
- M90: level-three stress relief resolves office actor slot 0 under the documented
  VB6 Empty-frame premise, charges the selected employee and names the actual
  recipient. Missing source mapping disables use before charging experience.
  See `docs/stress-relief-m90.md` and check its PR/merge state before resuming.

M90 validation: 459 domain checks with the optional original fixture; clean
application build; English/Japanese stress-relief smoke checks including save/load
and missing mapping; M76 source verifier: 44 fingerprints/35 payout entries.
The resumed environment lacks community audio assets; these checks establish
functional behavior, not audio or native Windows acceptance.

Next fidelity work, in priority order:

1. Speed technology: native identifier 4 sets a separate 50/100/150 counter
   (`A13A64`, company member `2A48`) and skips ordinary application units. Main
   `00A0283C +5656..56C3` retains worker eligibility; `+AFD8..B01B` decrements the
   counter and clears work state. Trace surrounding guards and actor iteration
   before implementation; the current generic application model is insufficient.
2. Payroll scratch writers and automatic conversations: the original uses
   employed count times salary at the scratch index, not a salary sum. Trace the
   technology-employee selector, portrait-dialogue helper `009933DC` and periodic
   poaching `009E3830`, including queue/completion order. Do not reintroduce
   unsupported resets removed in M80.
3. Original save import and whole-session native RNG integration remain separate
   work. Local draw ordering is recovered; the application still uses shared
   `System.Random`. Do not claim native deterministic playthrough parity.

Office relocation, separate polishing/external-job workflows and automatic
student skill growth are not established; require original evidence before
adding them. Private traces, original PE and validation logs may be under
`/workspace/scratch`; check existence rather than assuming persistence. The
original PE SHA-256 is
`4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.

Release target is Friday 2026-10-16, 19:00 CEST. RC.4 predates M86–M90 and is
unchanged: a final-source Windows export is still required. The owner chose
community native playthrough testing and plans to make the repository public;
do not change visibility, export packages or enable Actions without instruction.
Keep README's Ko-fi section and original LZH requirement. Issue #55 is the living
handoff; update it and this section when milestones or release decisions change.

Keep the original LZH requirement. Never commit imported original assets,
executables, saves, private extraction caches or full reverse-engineering traces.
GitHub Actions are paused at the owner's request; do not dispatch them or change
the workflow to run automatically. Preserve unrelated worktree changes.

Create a feature branch first, commit the changes on that branch, then open a
pull request into main. Do not publish development changes directly to main.
Do not generate desktop debug packages unless the owner asks for one.
Use the owner's existing GitHub connection for publication. Never ask the owner
to log in through a separate browser session.

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
