# Early Access decisions and unresolved evidence

Updated 16 October 2026 for the current release-preparation source. Version
`0.1.0-ea.1` is being prepared; no final Windows artifact or public release is
established by this register. Private RC.4/M90 packages are historical.
[Release preparation](early-access-release.md) tracks the remaining gates.
[Issue #55](https://github.com/UnSpotibleShadow/OpenGDS2/issues/55) remains the
continuation backlog. Keep stable decision IDs and distinguish source evidence,
implemented behavior, host checks and actual Windows acceptance.

Recent source work includes recovered facility/service conversations and six
promotion media, native-style awards and sales presentation, original Help,
English game names, backup/virus handling, poaching and toolbar/settings routes.
See [map backlog implementation](map-backlog-implementation.md),
[facility/promotion flows](facility-promotion-dialogue.md),
[TV correction](television-artwork-correction.md) and
[English names](english-game-names.md).

## Current release decisions

| ID | Decision | Reason / consequence |
| --- | --- | --- |
| EA-01 | Support implemented hiring/development/shipment/sales/calendar systems, modern saves and facilities; disclose incomplete parity. | Current source also supports all six promotion media, gameplay backup/virus handling, periodic poaching, lab/own hardware and the four recovered casino activities with their documented quirks. |
| EA-02 | Beginner retains its original Year 2, December, week 4 terminal boundary. Normal remains experimental ongoing play with no invented campaign cap. M84 implements the recovered negative-funds failure in either mode. | M75/M76 establish the failure predicate, dialogue and terminal UI branch. Modern saves resume pending failure messages; completed failure disables saving and returns to the title. Full presentation/campaign parity remains incomplete. |
| EA-03 | Retain one previous valid modern save and offer explicit recovery from the title. | Each successful replacement preserves the previous readable slot as `slot1.json.bak`. Recovery loads into memory only. Invalid new saves do not replace either file; saving after a damaged current slot retains the good backup. Recovery rolls back progress and is never merged with the newer state. |
| EA-04 | Use the permanent `OpenGDS2` data folder across all version/release labels. Preserve historical candidate provenance. | Owner decision on 16 October: first launch copies legacy `OpenGDS2-EarlyAccessTesting` data once without deleting originals or overwriting destination files. Conflicting save/backup pairs stay together; failed copies offer retry before gameplay. Save format is unchanged. Final packaging records the actual folder/source/hash. Actions remain paused. |
| EA-05 | Original-save import remains outside supported Early Access scope. Original casino quirks are preserved and disclosed, including discarded horse winnings. | M82/M83 implement the lab and hardware lifecycles recovered in M75/M76. Separate polishing and external contract work have no recovered workflow and are removed from unconditional reconstruction requirements. |
| EA-06 | Release target announced for 16 October 2026, 19:00 CEST; use community testing for full native Windows acceptance. | Owner decision on 2026-10-11 supersedes the full owner-matrix gate. Keep basic final-package launch/import/save/reload checks, known-defect fixes, notices and accessible reporting. Disclose native rows that have not run; do not record them as passes. |
| EA-07 | Use the OpenGDS2 GitHub issue tracker for community reports; owner plans to make the repository public before release. | Route chosen on 2026-10-11: https://github.com/UnSpotibleShadow/OpenGDS2/issues. Verify public access after the owner’s visibility change. Development does not change visibility as part of package preparation. |

## Public release gates still open

| ID | Status / owner | What is outstanding | Evidence needed to close |
| --- | --- | --- | --- |
| G-01 | Narrowed by owner / final-package check | Basic final-package launch, LZH import and new-game save/reload; full nineteen-row native matrix is community testing, not a publication gate | Exact final ZIP/source hash and results for checks actually run. Keep full native acceptance untested until community evidence arrives. |
| G-02 | Historical export method verified; final rebuild pending | Clean Windows source export completed for RC.4; a fresh final-source export is still required | Empty-cache standard SDK rebuild (zero warnings/errors), fresh Godot imports, official matching 4.4.1 .NET release templates, self-contained .NET 8.0.31 and successful Windows Desktop release export. All 184 PCK digests/25 audio imports, actual notices, archive/checksum/exclusion checks and exported-host startup/core loop/recovery pass. Build-host adapters stay excluded. See windows-source-export-m73.md. Basic final-package G-01 remains open; full native matrix is community acceptance. Actions remain paused. |
| G-03 | Pending final artifact and publication decision | Exact final ZIP/hash/source, GitHub release, scope and public bug-report access | Review the concrete artifact and disclosures before the final publication decision. GitHub Issues is the selected reporting route. After the visibility change, verify unauthenticated repository/reporting/download access before announcing availability. |
| G-04 | Pending final artifact review / development and owner | Distribution notices and original-content exclusions | The committed community notice records the owner's distribution authorization for all 25 tracks; there is no standalone MIT music grant. Verify current notice, actual engine/.NET notices and absence of originals in the final approved artifact. No new permission is inferred as necessary for private candidate preparation. |
| G-05 | Known-defect fixes / development | Known crashes, corrupted saves, duplicate charges or supported-workflow dead ends | Reproduction and regression for known blocking findings before publication. Unrun Windows rows are not defects and remain explicitly untested. |

## Undetermined original behavior and deferred reconstruction

These remain open unless a later milestone supplies evidence. They are not silently release blockers for the scope above, but any effect on a supported workflow becomes G-05.

| ID | Area | What cannot yet be determined | Next evidence / dependency |
| --- | --- | --- | --- |
| U-01 | Bankruptcy and company/office progression | Failure flow implemented by M84; native modal/presentation comparison remains community work | Strict negative-funds trigger, two introductions, source-slot reaction draws, closing message, simulation stop and title return are implemented. Modern pending cursor is explicit; completed failure cannot replace a save. Company data is preserved. Office relocation/rank progression has no recovered writer; do not invent upgrades. See financial-failure-m84.md. |
| U-02 | Bank | Exact bank form/message presentation comparison | M76 found no individual bank enable writer; its parent Actions menu is disabled on failure. No additional eligibility progression is established. Borrowing/repayment remain implemented. |
| U-03 | Mystery lab | Implemented by M82; native timing/presentation acceptance remains community work | Seven effects, per-active-iteration cadence, closure suspension/rebuild resumption and no start affordability guard are implemented with saved modern result cursors. Reset follows the final acknowledgement. See mystery-lab-m82.md for source anchors, validation and numerical/RNG boundaries. |
| U-04 | Casino | Implemented by M85–M89; native input/timing/rendering and numeric-format/coercion acceptance remain community work | M77 recovers ordinary outcome/deck rules, double-up, close/reload assignments and horse settlement. M78 recovers race recurrence (including the overlapping tick 290), RNG recurrence/seeding/draw ordering, serialized controls and timer kill/busy/rearm behavior. Reference DLL RNG, coordinate and timer instructions pass emulated vectors. M79 bounds all ordinary odds away from midpoints, traces input preview and drawing parameters, and corrects the queued-timer caveat. 32-bit Wine probes crash, so native Windows OLE/input/queue/rendering acceptance remains blocked. Original defects include exhausted card-chain retry, stale pending state, nonoptimal blackjack ace scoring and horse winnings overwritten before cash commit. Preserve evidence or label corrections. No slots, dice, redemption, split/insurance or horse-management workflow is established. M85–M89 implement coin purchase and all four activities. Horse wager/index/cap/refund defects and the overwritten-winnings path are retained explicitly; modern saved rounds and text presentation are adaptations. M88 recovers/imports all 44 runner and 10 jockey records through native member aliases and confirms selector reachability; see [race catalog](horse-catalog-m88.md). M87 pauses the exhausted-deck retry without inventing an award and preserves the lowercase keyboard branches. Modern cards, transient-round save continuity and numeric limits are documented adaptations; native Windows acceptance remains outstanding. |
| U-05 | Own hardware | Implemented by M83; native timing/presentation comparison remains community work | Quote, one-console/project guards, state-2 worker selection, four technology flags, completion/launch, production bands, licensing, market growth and imported design/media pictures are implemented. Modern saved messages and launch-name draft survive reload. See own-hardware-m83.md for evidence and boundaries. |
| U-06 | Polishing and external work | Unestablished feature proposals; no unconditional parity requirement | M76 follows all explicit room-state writers and project selectors. State 2 is hardware, and the contract form concerns employees. No separate polishing or external-job workflow was recovered. Reopen only with new source evidence. |
| U-07 | Payroll | Remaining writer coverage and original runtime comparison | Screen reset fixes and checkpoint speaker capture are implemented; periodic poaching now updates its recovered scratch count. Do not infer that every random-portrait caller or native scheduler path is complete. Unknown mappings retain explicit guards. Preserve recovered scratch arithmetic instead of substituting a salary sum. |
| U-08 | Employee progression | Native runtime comparison; unestablished student skill growth | M90 implements actor-slot-0 top-level stress relief. M91 connects speed's separate 50/100/150 timer, retained eligibility, actor-slot order and native interrupted-application stall under M76's documented Empty-frame premise; see [speed evidence](speed-technology-m91.md) and [stress evidence](stress-relief-m90.md). Unknown older-save mappings/frame values are guarded before charging. Periodic student skill growth has no recovered writer. |
| U-09 | Automatic conversations | Exact native cadence and complete caller coverage | Current source includes calendar conversations, periodic poaching, facility-specific pages and saved owner/selector lifetimes. Modern queues and save continuity do not establish native busy-window, timer or message-queue parity. |
| U-10 | Facility presentation | Native timing/animation and shared RNG-stream comparison | Nine facility artwork bindings, facility-specific flows and hot-spring employee reactions are implemented. Compare the running original for exact timing and draw ordering; keep accepted Lawyer behavior and one-time modern receipts intact. |
| U-11 | Room/tenant interaction | Original overlapping-hit priority, right-click paths and busy-window toggle sequence | Modern overlap chooser is intentional; original sequence remains unverified. |
| U-12 | Raster localization | Remaining bitmap writers and original pixel comparison | Guarded English overlays now include office MO/WK, vacancy signs, reviews, Hall/exhibition/awards and the bounded TV screen. Do not describe already-patched captions as wholly absent or claim every bitmap is translated. Original assets remain local. |
| U-13 | Names/text/fonts | Personal-name readings, setup defaults and native font/display comparison | All 65 rival games, ten publishers and 153 content examples have English display names. Player titles, imported identities and Japanese remain unchanged. Personal-name readings can remain provisional; native Windows screenshots are still needed. |
| U-14 | Original saves | Full serialization order/encoding, genuine fixture compatibility and dirty product-slot behavior | Module1 `009D9824`/`009D60E0`, genuine locally provided saves. Modern JSON support is separate; never publish original saves. |
| U-15 | Original comparison | Hiring-capacity/failure paths, exact recruitment runtime, initial staffing/development power, remaining meeting scenes and cosmetic draws | Targeted original runtime/static comparison. Existing authored synthetic tests are not original graphical proof. |
| U-16 | Package/native behavior | Fresh-machine dependencies, Windows graphics/scaling/input and sustained play | Basic final-package G-01 plus community testing for broader native coverage; exported host checks cover managed initialization/resources, not native Windows execution. |

## Historical continuation record — M72 through M84

The entries below describe their original milestones, not current release gates.

M72 added save recovery, visible scope and this register. Its RC.3 reused-runtime
candidate did not close G-02; its attempted persistent ZIP save remained unknown
and must not be blindly retried. Historical results remain recorded in issue #55.

M73 resolves the SDK named mutex/export problem by redirecting all required /tmp
operations, including mkdtemp/opendir, chdir and mkstemp, into owned host storage
while preserving real locks. Explicit MSBuild location permits Godot inspection.
All 25 exact source OGGs match immutable main Git blob hashes. A clean Windows
release source export and fresh RC.4 package now replace the fallback route.
334 standard-SDK domain tests, current UI, exported-host startup/core loop/recovery,
seven audio and five package tests pass. See windows-source-export-m73.md and #55
for provenance and final candidate hashes. No original mechanics were invented;
This is historical M73 validation. M76 subsequently resolves or narrows the research portions of U-01 through U-08 as recorded above; candidate gameplay, native Windows acceptance and public approval are unchanged.

M81 adds a tested public Early Access packaging mode with final-version/source/reporting guards, generated public notes and complete checksums. Ten package tests pass. It does not change the game version/private-candidate UI, export a new game build, publish a release or make the repository public. See early-access-release.md for the final packaging command and remaining artifact work.

M82 implements the mystery laboratory with saved progress and two-page result receipts; see [implementation and evidence](mystery-lab-m82.md). The historical RC.4 ZIP is unchanged and does not contain M80–M82. Final packaging must use the actual final source commit.

M83 implements own hardware and tightens the bank modal guard, including mystery-lab results. See [implementation and evidence](own-hardware-m83.md). RC.4 remains unchanged; final packaging must rebuild from the actual final source.

M84 adds the recovered financial-failure sequence. See [evidence and modern continuation rules](financial-failure-m84.md). These changes are not in the unchanged RC.4 ZIP; rebuild the final source before release.
