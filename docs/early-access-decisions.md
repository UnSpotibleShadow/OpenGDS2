# Early Access decisions and unresolved evidence — M76 research

Updated 2026-10-11 with M76 static verification; the private candidate remains **0.1.0-ea.1-rc.4**. This is the durable decision register; [issue #55](https://github.com/UnSpotibleShadow/OpenGDS2/issues/55) links it for continuation. Keep stable IDs when resolving entries. Record the source/evidence, validation and affected candidate for each resolution; never silently turn unknown original behavior into an implemented rule. [M76 findings and verification boundaries](workflow-verification-m76.md) resolve several research gaps without changing candidate gameplay.

## Decisions made for this candidate

| ID | Decision | Reason / consequence |
| --- | --- | --- |
| EA-01 | Support the verified hiring/development/shipment/sales/calendar loop, modern saves and implemented facilities. Defer unsupported mechanics rather than enable incomplete actions. | A smaller dependable feature set is appropriate for Early Access. Casino, mystery lab and own-hardware purchases remain disabled. |
| EA-02 | Beginner retains its original Year 2, December, week 4 terminal boundary. Normal remains experimental ongoing play with no invented campaign cap or bankruptcy. | The ending is verified; full company/office progression and bankruptcy are not. Title, mode selection, scope page and package notes disclose this boundary. Negative funds can persist. This is a deliberate candidate scope choice, not proof of original endgame parity. |
| EA-03 | Retain one previous valid modern save and offer explicit recovery from the title. | Each successful replacement preserves the previous readable slot as `slot1.json.bak`. Recovery loads into memory only. Invalid new saves do not replace either file; saving after a damaged current slot retains the good backup. Recovery rolls back progress and is never merged with the newer state. |
| EA-04 | Keep RC.4 private and isolated in `OpenGDS2-EarlyAccessTesting`, with an exact version/source manifest. | Owner requested testing before release. No public release/tag, visibility change or Actions dispatch is authorized by candidate preparation. RC.4 includes M64–M72 and M73 source-export tooling. Earlier candidate results are not substituted for its checks. |
| EA-05 | Casino, mystery lab, own hardware and original-save import remain outside the first supported Early Access scope. | M76 recovers lab/hardware lifecycles but does not implement them. Separate polishing and external contract work have no recovered workflow and are removed from unconditional reconstruction requirements. |

## Public release gates still open

| ID | Status / owner | What is outstanding | Evidence needed to close |
| --- | --- | --- | --- |
| G-01 | Pending owner Windows test | Native Windows fresh launch/import, modal reloads, graphics/scaling/audio, Beginner terminal and at least three Normal years | Exact RC.4 ZIP/source hash, machine settings and completed `early-access-testing.md`. Prior source-build launch confirmation is useful but does not accept this artifact. |
| G-02 | Resolved by M73 / development | Clean Windows source export completed for RC.4 | Empty-cache standard SDK rebuild (zero warnings/errors), fresh Godot imports, official matching 4.4.1 .NET release templates, self-contained .NET 8.0.31 and successful Windows Desktop release export. All 184 PCK digests/25 audio imports, actual notices, archive/checksum/exclusion checks and exported-host startup/core loop/recovery pass. Build-host adapters stay excluded. See windows-source-export-m73.md. Native Windows G-01 remains open; Actions remain paused. |
| G-03 | Pending owner release approval | Exact final ZIP/hash/source, distribution destination, scope and public bug-report access | Review concrete tested artifact and disclosures; approve publication separately. Repository remains private, so the eventual community reporting destination must be made accessible or an alternative chosen. |
| G-04 | Pending final artifact review / development and owner | Distribution notices and original-content exclusions | The committed community notice records the owner's distribution authorization for all 25 tracks; there is no standalone MIT music grant. Verify current notice, actual engine/.NET notices and absence of originals in the final approved artifact. No new permission is inferred as necessary for private candidate preparation. |
| G-05 | Acceptance-dependent / development | Any crash, corrupted save, duplicate charge or supported-workflow dead end found in Windows testing | Reproduction and regression for every blocking finding before publication. Automated host results cannot mark untested Windows rows passed. |

## Undetermined original behavior and deferred reconstruction

These remain open unless a later milestone supplies evidence. They are not silently release blockers for the scope above, but any effect on a supported workflow becomes G-05.

| ID | Area | What cannot yet be determined | Next evidence / dependency |
| --- | --- | --- | --- |
| U-01 | Bankruptcy and company/office progression | Implementation of recovered failure UI/cleanup; modern terminal-save representation; native modal comparison | M75/M76 identify strict negative-funds failure in continuous loop `00A0283C`, cleanup, timer stop and menu/title changes. Office relocation/rank progression has no recovered writer; production-capacity expansion is real. Do not invent office upgrades. EA-02 remains the candidate boundary. |
| U-02 | Bank | Exact bank form/message presentation comparison | M76 found no individual bank enable writer; its parent Actions menu is disabled on failure. No additional eligibility progression is established. Borrowing/repayment remain implemented. |
| U-03 | Mystery lab | Integration and saved modern modal behavior | M75/M76 recover seven effects, per-active-iteration cadence, closure suspension/rebuild resumption, original pending save/load and no start affordability guard. Reset follows modal return. Ready for source-backed implementation. |
| U-04 | Casino | Outcome/deck rules, interrupted-round transitions and horse-racing settlement | M76 recovers coin purchase/bets, three payout tables, coin save/load and close helper. Correct inventory: poker, blackjack, card chain and horse racing; card chain is not dice. |
| U-05 | Own hardware | Integration and runtime comparison | M76 recovers quote, one-console/project guards, room initialization/work, four technology flags, completion/launch, production bands, licensing, market growth and original saved cells. Ready for source-backed implementation. |
| U-06 | Polishing and external work | Unestablished feature proposals; no unconditional parity requirement | M76 follows all explicit room-state writers and project selectors. State 2 is hardware, and the contract form concerns employees. No separate polishing or external-job workflow was recovered. Reopen only with new source evidence. |
| U-07 | Payroll | Integration of all recovered scratch writers and runtime comparison | M76 maps staff, technology selection, random-employee dialogue and periodic poaching writers, including weekly-before-periodic ordering. Original save/load/reset omit scratch member 3018. Preserve the recovered index arithmetic; do not substitute a salary sum. |
| U-08 | Employee progression | Deliberate treatment of recovered technology defects and runtime comparison | M76 resolves speed's retained eligibility local and top-level stress relief's unassigned local/workspace-slot-0 target under documented VB6 frame initialization. Periodic student skill growth has no recovered writer. |
| U-09 | Automatic conversations | Presenter timing, queues, salary/renewal/graduate callers and original busy-window/modal cadence | Recover caller → queue → completion sequence. Modern saved dialogue pages do not establish original scheduler parity. |
| U-10 | Facility presentation | Exact original hot-spring reactions/animation, shared VB RNG-stream parity and lawyer portrait/background timing | Verified transactions/odds are supported; compare original runtime for presentation and random-stream ordering. |
| U-11 | Room/tenant interaction | Original overlapping-hit priority, right-click paths and busy-window toggle sequence | Modern overlap chooser is intentional; original sequence remains unverified. |
| U-12 | Raster localization | Remaining month/week/moving text, ambiguous bubble captions 13/14 and some bitmap writers | Hash-guarded local overlays need exact source regions/text; original bitmaps remain local. Existing vacancy signs are translated. |
| U-13 | Names/text/fonts | Provisional English personal-name readings, original setup defaults and graphical Japanese glyph coverage | Owner/original comparison and native Windows screenshots; imported/save identities stay unchanged. |
| U-14 | Original saves | Full serialization order/encoding, genuine fixture compatibility and dirty product-slot behavior | Module1 `009D9824`/`009D60E0`, genuine locally provided saves. Modern JSON support is separate; never publish original saves. |
| U-15 | Original comparison | Hiring-capacity/failure paths, exact recruitment runtime, initial staffing/development power, remaining meeting scenes and cosmetic draws | Targeted original runtime/static comparison. Existing authored synthetic tests are not original graphical proof. |
| U-16 | Package/native behavior | Fresh-machine dependencies, Windows graphics/scaling/input and sustained play | G-01; exported host checks cover managed initialization/resources, not native Windows execution. |

## Continuation record

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
