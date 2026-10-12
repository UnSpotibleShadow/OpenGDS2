# Development execution safeguards

## Current handoff — 2026-10-12, M93 research

The owner requested investigation of presentation ticket #95 before implementing
its missing handlers/actions. M93 is an analysis/evidence milestone; it changes
no gameplay or UI. See `docs/presentation-handler-audit-m93.md` and the
`codex/m93-presentation-handler-audit` PR/issue #55 for publication state.
Baseline for comparison is M92 main `71aaa1dac96c31894a8a64ddc2102b69f298f16f`.
Verification passes 32 procedure fingerprints, 60 bounded anchors, 15 control
event bindings, four local resource fingerprints and 78 numeric literals.

Resolved #95 actions: proposal accept/redo/cancel; completion shipment versus
postpone/debugging; initial shipment title/price/quantity and breakdown; compact
post-launch reproduction; right-click desk geometry; room state/progress and
two-frame panel behind the PC; paid six-media promotion; magazine comment/reaction
bands and reveal callbacks; annual renewal's actual selector. The owner's red
outline is room-panel sprites 91/92 at atlas X=259/321, Y=226, 62x31, six ticks
per half-cycle. It is not the separate vertical computer-frame effect. Existing
flames/bubbles/contribution sprites remain implemented. Do not reopen these as
unidentified effects.

Important corrections/boundaries: the compact product window uses graph ceilings
20k/40k/80k/200k/400k, unlike the larger report's implemented variant. Its second
award pane is cross-review Hall of Fame, not a dedicated worst-game pane.
Annual renewal is fGMSKoyouKz, not the graduate/individual fGMKeiyakuS selector.
Paid promotion has an unwritten local scope Variant; fresh Empty selects room
promotion, and the dormant released-product branches use inconsistent indices.
Do not silently implement cleaned-up released promotion as native fidelity.
Review score 9 has no final reaction-image write. Exhibit/awards backgrounds
are GIF ImageFormG[26]/[4], not the currently imported BMP subset. All artwork
stays locally imported. Native frame lifetime, exact scene motion/window stacking
and original timing remain runtime comparison limits. The video was not reviewed
because its attachment exceeded the 32 MiB transfer limit.

Issue #95 remains an implementation backlog, not a completed UI milestone.
Keep pacing issue #94 separate; the owner deferred it. M92 validation below is
historical application validation; M93 required only evidence/Python checks.

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
- M90, PR #90 (merged at `1d285806bede5bc981075fd31983d768359d7d09`):
  level-three stress relief resolves office actor slot 0 under the documented
  VB6 Empty-frame premise, charges the selected employee and names the actual
  recipient. Missing source mapping disables use before charging experience.
  See `docs/stress-relief-m90.md`.
- M91: speed technology uses the separate 50/100/150 actor-pass timer, retained
  frame eligibility and actor-slot order. It accelerates ordinary starts, not
  debugging. Original in-flight application stalls and unused timer retention
  are preserved and disclosed. See `docs/speed-technology-m91.md`; check issue
  #55 and the `codex/m91-speed-technology` PR for publication state.
- M91 is merged in PR #92 at `875fb52993bd129f29d1ee16cfcc65948718fc66`.
- M92: checkpoint meetings select saved random employee portraits and rebuild
  payroll scratch; publicity/fun-bonus secretary calls bypass the writer. See
  `docs/checkpoint-speakers-m92.md` and the `codex/m92-checkpoint-speakers` PR.

M92 validation: 483 optional-original domain checks, clean build/import and
English/Japanese checkpoint-speaker UI checks. Evidence verifies six fingerprints,
25 anchors and 14 random-employee checkpoint calls. The saved queue contains
speaker source/portrait snapshots; re-render/load must not draw again. Empty
native portrait maps are unestablished and have a documented modern guard.

M91 validation: 476 optional-original domain checks, clean application build and
English/Japanese speed UI checks; speed evidence checks two procedure fingerprints
and 32 bounded numeric anchors. `SharedWorkUnits` models
the native burst/application-unit alias, including mid-burst technology use and
speed reactivation after expiry. Modern saves retain it; existing synthetic
fixtures that inject a burst must set this underlying cell as well as its display
projection. Native Windows and whole-session RNG equivalence remain pending.

M90 validation: 459 domain checks with the optional original fixture; clean
application build; English/Japanese stress-relief smoke checks including save/load
and missing mapping; M76 source verifier: 44 fingerprints/35 payout entries.
Those initial M90 checks preceded community-audio restoration; the later M90
source export/startup included verified audio. Native Windows acceptance remains
pending.

Next fidelity work, in priority order:

1. Payroll scratch writers and automatic conversations: the original uses
   employed count times salary at the scratch index, not a salary sum. Trace the
   remaining portrait-dialogue helper `009933DC` callers and periodic poaching
   `009E3830`, including queue/completion order. The selector `0099F9A4` is the
   already-connected school-capacity roster; do not treat it as another ordinary
   technology selector. Checkpoint calls are connected by M92. Remaining direct
   literal -1 sites: planning 37, final completion 12, awards 15, sales commentary
   1 and shipment 2. Do not reintroduce
   unsupported resets removed in M80.
2. Original save import and whole-session native RNG integration remain separate
   work. Local draw ordering is recovered; the application still uses shared
   `System.Random`. Do not claim native deterministic playthrough parity.

Office relocation, separate polishing/external-job workflows and automatic
student skill growth are not established; require original evidence before
adding them. Private traces, original PE and validation logs may be under
`/workspace/scratch`; check existence rather than assuming persistence. The
original PE SHA-256 is
`4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.

Release target is Friday 2026-10-16, 19:00 CEST. The previously shared RC.4 artifact
predates M86–M90 and is unchanged. At the owner's request, a fresh M90 Windows
release-template testing export was produced from the exact PR #90 merge above:
`builds/OpenGDS2-Windows-M90-testing.zip` (111887862 bytes), SHA-256
`8e77e2ccebc6ac81c2a81576a0f5a0f40815c13da4f198225104223f98f971f5`.
This generated package is not committed. Its internal version remains RC.4;
`M90_TEST_BUILD.txt` and `BUILD_MANIFEST.json` distinguish it from the old artifact.
All 25 community audio sources matched Git blob hashes; export/startup passed on
a disposable Linux host using the unchanged Windows assembly/PCK, with 198 PCK
payloads and 202 archive checksums verified. Native Windows execution is pending.
A final public-release version/export and review are still required. The owner chose
community native playthrough testing and plans to make the repository public;
do not change visibility, export packages or enable Actions without instruction.
Keep README's Ko-fi section and original LZH requirement. Issue #55 is the living
handoff; update it and this section when milestones or release decisions change.

M90 download delivery failed in the app. The configured GitHub credentials work
for API calls but were rejected by `uploads.github.com` (HTTP 401); no asset was
uploaded. The empty temporary draft release was deleted and HTTP 404 verified.
The owner chose to clone and build locally. Do not claim the package was received,
retry uploads blindly or create another release for this handoff. That ZIP also
predates M91; current source is the owner's testing route.

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
