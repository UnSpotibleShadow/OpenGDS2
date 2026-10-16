# Archived agent handoffs through 15 October 2026

This is a historical record of completed and superseded work. Branch names,
next steps, task statuses and host paths below describe their original sessions.
They are not current instructions. Use the root AGENTS.md and current development
checkpoint for active status. Source evidence remains in its linked documents.

---

# Current task — English game-title display names

Owner requests translation of original game names, especially Award Ceremony
rivals. Branch `codex/english-game-titles`, base
`2610e61d1d5ccf282eac4d46dc8c7cb8d05de216`, covers all 65 rival titles, ten
publishers and 153 development example/magazine names. Change presentation only:
preserve imported/saved titles, own titles, Japanese and unknown-name fallbacks.
Check the award board, host dialogue, reel, sales rankings and example tooltips.
Verification complete: 565 domain checks, clean app build, translation import,
bilingual rendered title/chart/example checks, award continuation and core loop.
Recovery: `/workspace/scratch/english-game-titles/`. Standing verified-PR merge
authorization continues; no export/release/issue closure or unchanged Wine retry.

# Current task — Television screen patch correction

Owner requests a second pass on the TV artwork after the prior patch painted
outside the CRT. Branch `codex/television-patch-bounds` starts at
`cd4a8f89abc6c32af1336e9ae3251f54eac8d2c1`. Recovery records:
`/workspace/scratch/television-patch-v2/`. Implementation and verification complete: 562 domain checks, clean app build,
and real English/Japanese/override renders pass. Native pixel comparison confirms
zero changes outside the CRT glass; only the TV cache is invalidated. Standing verified-PR merge authorization
continues; keep #118/#95 open. No export/release or unchanged Wine retry.

# Current task — facility and six-media promotion fixes

Owner explicitly authorizes fixing the recovered facility chains and all six
promotion media. Work is on `codex/facility-promotion-dialogue`, based on
`065c1e5fcdc2da4f38f83b31c2a6d9e68ff884bb`. Implementation is complete. 562 domain checks, clean app
build, schema18/374-entry archive import, bilingual rendered facility/all-six-media
flows and core/hardware/casino/interactive resize regressions pass. Final scene
loader regression also passes. Publication receipts and the review ZIP are kept
with the recovery records. Read private checkpoints/logs at
`/workspace/scratch/facility-promotion-fixes/` before continuing. Do not stop at
the previous investigation report. Keep accepted Lawyer behavior unchanged.
Standing merge authorization continues; keep #118/#95 open for review. No export,
release or unchanged Wine retry. SDK/Godot checks must remain sequential/bounded.

# Current follow-up — facility chains after #121

Owner accepts the Lawyer and requests deeper recovery of the other facilities.
Read `docs/facility-conversation-chains.md`: all nine illustration bindings and
56 procedure fingerprints verified. Hot-spring employee reactions, owner window
lifetimes and facility-specific selector/refusal/return routes remain incomplete.
This follow-up is investigation-only; do not claim those corrections implemented.
Private traces/artwork previews: `/workspace/scratch/facility-chains/`. Keep #118
and #95 open; no unchanged Wine retry. Standing verified-PR merge authorization
continues. Next implementation order and acceptance branches are in the report.

# Current status — issue #118 implementation

Owner authorized implementing #118. Branch `codex/issue118-fixes`, base
`e88868471097ea894f2842beef933a09497a9c4b`, contains all six planned fix areas.
559 domain checks, clean app build, full schema17/334-entry archive import,
new bilingual workflow/facility/hover checks and affected layout/core regressions
pass. Final rendered checks and before/after review captures also pass. Publication
receipts and the before/after gallery are retained with the recovery records:
`/workspace/scratch/issue118-fixes/`. Xorg :60 PID39630 was used for captures;
verify ownership before stopping any remaining owned display. The unchanged Wine runner remains
blocked and must not be retried. Keep #118 open for owner review and #95 open.
No export/release/Actions/visibility changes are authorized. Standing verified-PR
merge authorization persists. Only explicit verified files should sync into the
primary `/workspace/OpenGDS2` snapshot after publication.

# Current handoff — original-map implementation, 15 October 2026

Owner authorized the full map backlog and standing merges of verified PRs.
Branch `codex/map-backlog-implementation`, base
`a5abd1de0336ea67ba87aad200899d5dbf7472c0`, implements the gameplay backup
purchase/virus branch, four-form employee poaching including automatic offers,
and original sound/graphics/number options/direct access. Read
`docs/map-backlog-implementation.md` and `docs/map-backlog-verification.md`.
551 optional-original domain checks, clean app build, 19 map/property tests,
eight affected Godot regressions and full official-LZH import pass.

The official archive is recovered and inventoried privately. All 45 CRCs,
422 raster properties, 82 root-icon exports and three PE resource payloads are
checked. All 2605 control streams/all 84 roots are structurally bounded, including
MDI's implicit ClientLeft header; 537 picture properties are accounted for.
Opaque marker meanings and dynamic semantics remain open. Native save mapping
pairs 130 templates/448 field slots/447 members; RNG maps 310 draws/one seed site.
No native GD2 compatibility or full native draw-order/pixel/timing parity is claimed.
Wine 11 crashes at host level before a usable original-game window; all 245
original-versus-port scenarios remain pending. Do not turn static coverage into
runtime acceptance. Reimport for 322 dialogue entries and negotiation portraits.
Keep #95 open. No release/export/Actions/visibility changes are authorized.

Recovery/check logs and guarded publication receipts:
`/workspace/scratch/map-backlog-implementation/`. The full git checkout is
`/workspace/scratch/m105-review-git`; the primary snapshot has no .git and needs
only verified changed files synced after publication. Avoid repeating passed
checks without a relevant change; use the repository watchdog for new checks.

**Next owner request:** after this batch is published, investigate the latest
issue #118, created while the owner compared the old and new games. Read its
latest body/attachments then. Persist it in `/workspace/review-task-list.md`.
The owner selects the active model through the UI; do not spawn agents merely
to emulate the requested Sol Light model switch.

# Development execution safeguards

## Original-game mapping — 15 October 2026

Owner authorized executing the full original-first mapping plan. Work on
`codex/original-game-map`, based on main
`2eb7c0bf7de32b33b76e0bb1dc80840f406188e1`, adds the independent inventory,
workflow/gap catalog, deterministic verifier and standalone searchable viewer.
Read `docs/original-game-map.md` before further original-game investigations.
All 86 objects (84 forms/two modules), 920 control definitions, 572 event slots,
669 procedures/167189 instructions, 55 serialized menus, 2605 known control
instances, 536 known picture properties and 36 known timers are accounted for.
Do not describe those denominators as complete game understanding or parity.

35 workflow dispositions/245 original-versus-port scenarios preserve unknowns.
The 2829 direct-call/object-reference edges are not a complete call graph;
13140 dynamic-dispatch candidate sites remain unresolved. Two root and228
control property parses stop early. Original archive inventory, native runtime
acceptance, complete table/state/audio/asset semantics and GD2/RNG parity remain
open. A private Wine11 binary exists, but no complete runnable original
installation or native Windows acceptance was established. No original code was
executed by this mapping tool; no gameplay/UI implementation changed.

New source-backed gap: original `mKoudouBackupK` / `0098B1F4` purchases a
100000000-yen gameplay backup device, sets `00A112E8`, debits the shared financial
helper and affects `009E3830` virus branches. This is absent from the adaptation
and distinct from modern save-file backups. Read `docs/original-backup-device-map.md`;
trace exact event mutations/init/persistence before implementing it. Menu/settings
routes and the existing graduate/renewal artwork aliases also need reconciliation.
Do not repeat source inventory or invent working multiplayer/office expansion.

Verification: fresh original rebuild,15 meaningful accounting/graph/view tests
and Chromium rendered search/filter checks pass. Recovery checkpoints/private
preview: `/workspace/scratch/original-game-map-20261015/`.
Keep #95 open. Standing owner authorization permits merging verified open PRs;
no release/export/Actions/visibility change is authorized.

## Toolbar year / office date — 15 October 2026

PR114 merged at65a655a8138174ea2d38940323da44376368c08e. Owner requests the
missing year before toolbar Funds and an English patch for the office date.
Implemented on codex/toolbar-year-office-date: year comes from state.Year;
English MO/WK replaces only the two background labels, retaining original number
sprites, frame, Japanese and override paths. English patch cache revision changes.
Clean build,533 optional-original domain checks and two bilingual rendered
regressions pass. See docs/toolbar-year-office-date.md; private checkpoint and
captures: scratch/toolbar-date-20261015. Standing open-PR merge authorization
continues; no exports, Actions or visibility changes are authorized.

## Native toolbar correction — 15 October 2026

Owner authorizes merging open PRs. PR110/111/113 are merged; latest main was
2b303b2e93b04c6c7c1114dda540d85b20265f04 before this follow-up.
Owner requests the original toolbar order, separate three UP actions, and removal
of port-only tools from in-game menus as well as the toolbar. Work is on
codex/native-toolbar-menu. See docs/native-toolbar-20261015.md and external
scratch/native-toolbar-20261015 checkpoints. Clean build and seven affected checks pass: bilingual toolbar, #112, management,
presentation continuation, financial failure and window/artwork, plus core loop.
No exported release, Actions dispatch or visibility change is authorized.

## Active review tasks — 15 October 2026

The owner authorized execution of all three tasks. Persist the running list in
`/workspace/review-task-list.md` and read it when continuing this review round.
Task 1: fully translate the original five Help pages / 91 recovered text blocks
into English, replacing summaries; retain Japanese and distinguish port-specific
notes. Implementation is now authorized.
Task 2: issue #112 is now ready to read/scope. Owner authorizes closing it
after its requirements are completed and verified.
Task 3: audit all eight player-activated technologies for actual effects, costs,
completion/duration, level scaling, interruptions and save/load against original
behavior. Investigation is authorized; do not assume all currently work.

The October 15 batch is implemented and verified: 91 Help blocks translated;
#112 source-proven product order, bounded/resizable lists, live-report employee
activation, native casino selector and activity confirmation; all eight technology
effects audited and their feedback clarified. No gameplay service was changed.
Clean build,531 optional-original domain checks and seven relevant Godot checks
pass. See docs/review112-help-technology-audit.md. Publication receipt is
scratch/review-20261015/verification.json; branch codex/review112-help-technology-audit.
Keep pending PR110/111 independent, #95 open; no merge/export/Actions authorized.
CompanyActivity.bmp needs reimport. Its English FAME/GENRES/OUTPUT labels are
now patched locally with exact-source fingerprint and bounded region guards;
Japanese and explicit overrides retain their paths.

## Active M105 — presentation plan, one step at a time

The owner authorized all remaining steps on 2026-10-14 while going to bed,
reporting 57% usage remaining. Earlier per-step permission pauses are superseded.
Continue Steps 5–8 without further approval stops; persist checkpoints after each
step. Usage is not exposed, so do not invent a remaining percentage.
Steps 1–4 are complete locally. See docs/presentation-evidence-m105.md,
docs/shared-presentation-m105-step2.md, docs/management-presentation-m105-step3.md
and docs/hardware-presentation-m105-step4.md. Steps 1–7 are complete; consolidated verification passes. Step 8 publication is
tracked in scratch/m105-evidence/publication-manifest.json and guarded receipts.
Step 7 adds verified native asynchronous effects with independent settings;528
domain checks and authored PCM/affected casino/core/music regressions pass.
See docs/original-audio-m105-step7.md; union step-7-completed.json. Publication is Step 8.
Branch codex/m105-presentation-evidence was created/read-back at main
87aab7f7e834cbc66b44fb40367be2a9920fc22b. Publish only the verified explicit union as a review PR; no merge is authorized.
Find the PR by feature branch and reconcile guarded remote receipts before retries.
Keep #95 open; no export/release/Actions/visibility change.

Step 1: corrected unused MainForm3 icon import (82 bindings); added form-boundary
verification and35 forms/1633 rectangle anchors/240 handler fingerprints.
Step 2: named older dialogs restore positions; employee details retain moved
coordinates across selection/modal return; native icon bindings and small23x22
button/39x16 localized-tab skins; PNG override support with ownership/dimension
guards. Step 3: compact licence, production, training/use, staff/employee,
secretary and Bank layouts; supplemental Company info; active employee tabs.
Step 4: six compact hardware screens, missing market/report routes, retained
monitor, shared launch illustration and fingerprint-guarded English title patch.
Planning selection retains shared choices on cancel and restores device preview.
Step 5 complete: native card components/three tables, horse roster/wager matrix
and recovered mask/XOR/coordinate compositor. See docs/casino-presentation-m105-step5.md.
Clean build,523 domain tests, bilingual four-table containment and four casino
workflow checks pass. Receipts scratch/m105-step5-*; union step-5-completed.json.
Race sheets remain ordinary original LZH assets; missing sheets request reimport.
Step 6 is complete: compact remaining reachable screens and Help/milestone caption patches.
Clean build,524 domain checks, bilingual layouts/core/continuation/window checks pass.
Union: scratch/m105-evidence/step-6-completed.json. Earlier audit overstated poaching counterparts: no modern
poaching event/state exists. Record that workflow gap explicitly; no fabricated
negotiation transaction is authorized by a presentation-only rewrite.

Step 4 checks: clean build;522 optional-original domain checks; bilingual hardware
layouts; own-hardware workflow; presentation continuation; headless core-loop all
pass. Receipts: scratch/m105-step4-*; union handoff step-4-completed.json. Owner
welcomed community preview images: six English PNGs/overview/ZIP in private
scratch/m105-step4-community (not committed/uploaded). :48 display stopped after
checks. No M105 publication; Step 7 audio is still pending.

Step 3 checks: clean build; bilingual management containment/transactions;
headless core-loop; bilingual speed/stress use; bilingual window/artwork/position
checks all pass. Source services unchanged. Media detour retains a value snapshot
rather than reading the disposed spinner. Test worker selection follows the new
bounded roster; preserve fixed-fps60 on headless core-loop checks. The initial
missing-frame-rate attempt was stopped, not left running. Receipts/captures:
scratch/m105-step3-*; union handoff scratch/m105-evidence/step-3-completed.json.
Owned :47 display stopped after verification. Publication remains Step 8.

Checks: clean app build;522 optional-original domain tests; focused bilingual
rendered window/artwork checks; headless core-loop regression; rendered native
layout sweep all pass. Rendered full core-loop attempt reached its180s deadline
without an assertion failure; changed to the previously successful headless
workflow route rather than extending the deadline. Failed focused fixtures lacked
audience/recall fields and were corrected. See scratch/m105-step2-*.json/log.
No passed check needs rerunning absent relevant changes. Owned :46 display was
stopped after verification. Keep original artwork/audio/captions/traces private.

## Current M104 — third review implementation

Owner approved the complete plan in docs/review-round-plan-m104.md. See
docs/review-round-implementation-m104.md for implementation, evidence and testing.
Branch codex/m104-third-review-presentation starts at M103 PR #107 merge
f1da985d014f9b622011be89910bb54437699bdf. Keep #95 open for the next owner review.
Reimport the original archive for the added native icons, facility/computer art and
Japanese awards dialogue. Original pixels and private captures are not published.
Local build, 522 domain checks, bilingual rendered review, core loop, saved awards
continuation, native layout, review/event and presentation completion checks pass. Publication receipts live externally
in /workspace/scratch/m104-*.json; verify remote state before recovering a write.
No new exported release, Actions dispatch or visibility change is authorized.

## Current handoff — M103 second playthrough feedback

Owner authorized this completed review round. Branch
`codex/m103-playthrough-presentation` is based on PR #106 merge
`f4b767e8d0f517dc44ad4be625aab483f46914c7`. Verify remote PR/main publication
before continuing; use the durable `/workspace/scratch/m103-*.json` receipts.
**Keep #95 open.** See `docs/playthrough-presentation-m103.md`.

Implemented: wider checkpoint names; compact media columns without horizontal
scrolling; wider numeric inputs; recovered 80ms shipment repetition with one-step
ordinary clicks; review speech bubbles, padded score art and bounded English
Reviewer 1–4 role labels; compact white sales graph; aligned exhibition counts
and centered button; boxed trophies; compact renewal; stable toolbar hover
margins; generic dialog positions and per-room reports retained behind meetings,
with actions blocked until the active dialog closes.

Original comments use independent score-band draws from a shared pool, so repeats
are valid; do not enforce uniqueness. Native report timers close on empty rooms;
original screenshots show reports beneath checkpoint selectors. BGraphic skins
are recovered locally and glyphs applied only where bindings support them. Plain
native command buttons remain text controls. All original assets/captures stay
private. Personal-name translations of reviewer captions are not established.

Validation: 521 optional-original domain checks; clean build (zero warnings and
errors); bilingual focused rendered regression and complete native layout sweep;
complete core-loop, review/event and presentation-continuation regressions;
original procedure/button/timer evidence verifier. Failed fixture attempts were
corrected (required staff stats, retained development eligibility, valid candidate
roster, production cap and deferred input text refresh). No successful check needs
repeating absent a relevant source change.

Rendering display startup previously stalled before a process was launched.
Use detached Popen with a bounded readiness loop; avoid foreground service calls
and redundant additional-permission overrides. Current check display is :44;
stop only the PID recorded in `m103-display-start.json`. SDK/Godot invocations
remain sequential and bounded. Remote writes require durable intent, expected
SHA/read-back and reconciliation of unknown outcomes before retry.
No release/export/Actions/visibility change was authorized in this batch.

## Current handoff — 2026-10-13, M102 awards follow-ups

M101 is merged in PR #105 at `b29103aec9af301c292c75cabf09d6c52d9fec54`.
M102 on `codex/m102-awards-presentation` adds a locally generated English
GAME CONTEST title-plate patch and wraps/measures the winner/payment message,
keeping acknowledgment below every line. See `docs/awards-presentation-m102.md`.
Verify PR/#55/main state. **Keep #95 open.** No award draw, payment or saved
ceremony ordering changes. Original stage/scenery stays untouched outside the
182×18 caption region; Japanese and explicit overlays retain their paths.

M102 validation: 518 optional-original domain checks; clean app build/import;
six rendered English/Japanese short/long/rival cases with complete-line and
save/reload checks; complete core-loop regression. M101 checks remain historical.

## Previous handoff — 2026-10-13, M101 native screen layouts

M100 is merged in PR #104 at `9579c392af6bd49976ef78f7480b26df03608237`.
M101 on `codex/m101-native-screen-layouts` addresses the owner's M100 playthrough
feedback: fixed compact proposal/project/checkpoint/completion/review/Hall/media/
product/exhibition/awards clients; aligned selection lists and shipment hold controls;
Sales defaults to the individual graph. See `docs/native-screen-layouts-m101.md`.
Verify PR/#55/main publication state. **Keep #95 open.** Implemented handlers and
available artwork did not establish matching layouts; do not repeat M93–M100
research or claim pixel parity from those milestones. Hall has no confirmed native
screenshot here; its recovered form/art dimensions guide the compact layout.

English Hall/booth signage uses bounded, fingerprint-guarded patches generated
from locally imported artwork. Japanese and explicit user overlays retain their
original paths. Reimport for missing pictures. Never commit original pixels,
private caches or rendered captures. #97's 422-resource inventory still needs
exhaustive image-to-handler coverage. The OFFICE atlas is not every game image.

Rendered checks found cached GD.Load audio disposed across Main lifetimes. M101
leaves those shared resources to Godot while retaining ownership of independently
loaded local tracks. Playlist selection and RNG are unchanged.

M101 validation: 516 optional-original domain checks; clean app build/import;
English/Japanese rendered layout checks for twelve windows; full core-loop and
review/event save-resume checks. Original captures stay private.

Related edits are batched before checks. Check logs/checkpoints outside the repo
before repeating validation. Usage percentage is unavailable; if the owner reports
15%, finish a safe step, persist state and update this handoff.

The shared saved 24-bit VB-family RNG does not establish whole-session native draw
ordering or initial-seed equivalence. Native GD2 import remains unavailable without
complete field mapping and a genuine fixture. #94 missed-tick replay is fixed;
original Windows wall-clock comparison is still needed for perceived-speed parity.
Exact native fonts, stacking, focus and timer delivery require Windows acceptance.

Release target remains Friday 2026-10-16, 19:00 CEST. Do not generate another build,
change visibility, publish releases or enable Actions unless requested. Preserve
README's Ko-fi and original LZH requirement. The M100 testing build predates M101.

## Previous handoff — M99 challenge audit and Hall scene

M98 is merged in PR #102 at `2507317c6ea5c1b5a2fde10c0698642cc8927564`.
M99 on `codex/m99-programming-audit` resolves challenge trigger/reels/actor,
result effects and return. Failure is mode 6, not M98's incorrectly transcribed 9.
Success/super are modes 10/17. Read `docs/programming-challenge-m99.md` and
verify PR/#55/main publication state. Nine fingerprints, 36 anchors and fifty
reel writes are checked by the new research verifier. Do not re-trace these.

Challenge integration still needs a saved mid-actor-pass continuation; re-running
an employee tick after its modal result would repeat stress/counters/RNG. No
challenge gameplay is wired in this batch. Hall celebration is implemented:
new qualifying reviews queue the two locally imported banner frames; frame/counter
resume from saves; closing does not repeat the existing Hall count. Legacy Hall
flags do not replay historical scenes. Reimport for new artwork. Exact native
secretary/sequel ordering remains pending; #95 MUST stay open.

## Previous handoff — M98 original-window census

M97 is merged in PR #101 at `39a3158c399221634f6e7c193ce21936f3e65650`.
M98 research on `codex/m98-window-census` inventories every native form:
84 forms/designers, 920 control definitions, 572 bound events; 32 targeted
procedure and seven image fingerprints. See `docs/window-census-m98.md` and
`tools/verify_window_census_evidence.py`; verify PR/#55/main publication state.
This is analysis only; engine tests/builds are unnecessary. #95 stays open.

Newly tracked gaps: separate Hall celebration, illustrated sales milestones,
advanced-programming start/stop challenge (workflow, not just decoration), media
information, reproduction tables, third-party hardware launch card, activity
confirmation/results, help topics, numbered save/load previews and live charts/
record panes. Do not reimplement already recovered simulation. Before coding
advanced programming, trace its reel weights/actor mapping/meeting modes 6/10/17
and resume mutations; existing ordinary bursts are not its equivalent.
`fGMZishaNet` is reachable but its displayed data has no recovered writers in
our decoded reference index; do not invent working multiplayer from its name.
All 84 forms are mapped for comparison, not certified for pixel/control parity.
Original artwork/previews/traces stay private. #94 timing and #97 exhaustive
artwork-family audit remain separate. Start from the newly expanded #95 rather
than repeating the original screenshot-only research.

## Previous handoff — M97 room promotion

M96 is merged in PR #100 at `b9e131c67009026822ba43edbc19396ee04084ef`.
M97 on `codex/m97-room-promotion` implements the remaining paid promotion path:
six media, alpha gate, target-wide diminishing returns, callback-27 recognition/
accounting commit and saved result/phase. Verify PR/#55/main publication status.
No released-product targeting is invented. Old saves lacking promotion counts
show an explicit history warning rather than receiving guessed first-use gains.
See `docs/room-promotion-m97.md`. Keep #95 open.

Owner's testing preference: batch related edits before running relevant checks;
repeat checks after failures or later changes warranting verification. Do not
rerun successful checks solely for publication. Usage percentage is not exposed;
if the owner reports 15% remaining, finish a safe step and update handoff notes.

## Previous handoff — M96 reviews and events

M94 is merged in PR #98 (`9d9a844ee3b5d22ade1c025ea8e377f94032f2e2`):
right-click desk assignment, room label/progress/panel phases and point columns.
M95 is merged in PR #99 (`7aa9f7983cf99d3dcb0a324856adccd3cdc0a6b5`): proposal
artwork/title timing, combined development/checkpoint/completion panels, detailed
shipment tables, compact product chart/report and annual renewal details.
Do not redo these. See their milestone documents.

M96 on `codex/m96-review-event-artwork` restores vertical reviewer rows, saved
comment choices/reveal phases, local reviewer/score/reaction artwork, booth and
stage GIF conversion, exhibition lists/attendance display and persistent five-row
award results. Original sprites/messages remain local. Reimporting the original
archive supplies new artwork and Japanese comment templates to older caches.
See `docs/review-event-presentation-m96.md`; verify PR/#55/main publication state.

Important corrected evidence: Image2 has four visible score slots, while Image4
has ten hidden RLE8 digit sources. ImageTF[5] is an empty white GIF, not a Hall
of Fame badge; that pane reuses ImageTF[2]. A12F74 markers are 1 Grand Prix,
2 runner-up, 3 music, 4 graphics, 5 worst. The old port's marker transcription is
fixed without changing selection/prizes. Modern save schema 2 migrates schema-1
marker meanings once; do not reapply the old mapping to schema 2. Older missing
comment phases, visitor steps and previous category winners stay unknown.

M96 validation: 498 optional-original domain checks, including thirteen independent
RGBA references and save migration; clean app build/import; full core-loop UI;
bilingual partial review/save-load/score-nine/local artwork and booth checks.
M96 evidence verifies 36 resource bindings, 14 anchors and 49 bounded comment
branches; M93 still verifies 32 fingerprints/60 anchors/15 events/4 resources/78
literals. Rendering/phase replay draws no outcomes and repeats no transactions.
Cosmetic cycling/native RNG ordering, retained-frame lifetime, exact scene motion,
font rendering and Windows window stacking remain runtime comparison limits.

#95 MUST stay open for further native walkthrough findings. Remaining source-backed
workflow: paid six-media promotion, using M93's documented fresh-Empty room scope
boundary; do not invent a cleaned-up released-game target selector. #94 office
pacing and #97 exhaustive artwork review remain deferred. The shown atlas is the
full OFFICE sprite sheet, not all game graphics. Device images, staff portraits,
dialog images and serialized controls are separate; the resource-family index and
private local HTML generator are in `docs/artwork-reference-index.md` and
`tools/local_artwork_reference.py`. Do not publish generated references/pixels.
`OriginalRoomPaused` remains the native disruption flag, not a new disaster system.

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

Issue #95 remains an implementation backlog; M94 completes only the office/columns group.
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
