# Early Access decisions and unresolved evidence — M72

Updated 2026-10-09 for private candidate **0.1.0-ea.1-rc.3**. This is the durable decision register; [issue #55](https://github.com/UnSpotibleShadow/OpenGDS2/issues/55) links it for continuation. Keep stable IDs when resolving entries. Record the source/evidence, validation and affected candidate for each resolution; never silently turn unknown original behavior into an implemented rule.

## Decisions made for this candidate

| ID | Decision | Reason / consequence |
| --- | --- | --- |
| EA-01 | Support the verified hiring/development/shipment/sales/calendar loop, modern saves and implemented facilities. Defer unsupported mechanics rather than enable incomplete actions. | A smaller dependable feature set is appropriate for Early Access. Casino, mystery lab and own-hardware purchases remain disabled. |
| EA-02 | Beginner retains its original Year 2, December, week 4 terminal boundary. Normal remains experimental ongoing play with no invented campaign cap or bankruptcy. | The ending is verified; full company/office progression and bankruptcy are not. Title, mode selection, scope page and package notes disclose this boundary. Negative funds can persist. This is a deliberate candidate scope choice, not proof of original endgame parity. |
| EA-03 | Retain one previous valid modern save and offer explicit recovery from the title. | Each successful replacement preserves the previous readable slot as `slot1.json.bak`. Recovery loads into memory only. Invalid new saves do not replace either file; saving after a damaged current slot retains the good backup. Recovery rolls back progress and is never merged with the newer state. |
| EA-04 | Keep RC.3 private and isolated in `OpenGDS2-EarlyAccessTesting`, with an exact version/source manifest. | Owner requested testing before release. No public release/tag, visibility change or Actions dispatch is authorized by candidate preparation. RC.3 includes M64–M71 and M72; RC.2's older build results are not results for this candidate. |
| EA-05 | Do not require casino, mystery lab, own hardware, polishing, contract work or original-save import for the first supported Early Access scope. | They depend on unresolved original lifecycles. Track them below; completing them is full-reconstruction work, not a reason to fabricate mechanics for this candidate. |

## Public release gates still open

| ID | Status / owner | What is outstanding | Evidence needed to close |
| --- | --- | --- | --- |
| G-01 | Pending owner Windows test | Native Windows fresh launch/import, modal reloads, graphics/scaling/audio, Beginner terminal and at least three Normal years | Exact RC.3 ZIP/source hash, machine settings and completed `early-access-testing.md`. Prior source-build launch confirmation is useful but does not accept this artifact. |
| G-02 | Blocked in current workspace / development | Clean Windows source export | Standard SDK/Godot export with matching 4.4.1 .NET templates, successful packaging/license/resource checks and startup tests. Direct Roslyn plus reused engine/runtime is a working candidate fallback, not a clean export. Local SDK process-metadata workaround reaches MSBuild; NuGet's `NuGet-Migrations` named mutex still fails. Actions remain paused. |
| G-03 | Pending owner release approval | Exact final ZIP/hash/source, distribution destination, scope and public bug-report access | Review concrete tested artifact and disclosures; approve publication separately. Repository remains private, so the eventual community reporting destination must be made accessible or an alternative chosen. |
| G-04 | Pending final artifact review / development and owner | Distribution notices and original-content exclusions | The committed community notice records the owner's distribution authorization for all 25 tracks; there is no standalone MIT music grant. Verify current notice, actual engine/.NET notices and absence of originals in the final approved artifact. No new permission is inferred as necessary for private candidate preparation. |
| G-05 | Acceptance-dependent / development | Any crash, corrupted save, duplicate charge or supported-workflow dead end found in Windows testing | Reproduction and regression for every blocking finding before publication. Automated host results cannot mark untested Windows rows passed. |

## Undetermined original behavior and deferred reconstruction

These remain open unless a later milestone supplies evidence. They are not silently release blockers for the scope above, but any effect on a supported workflow becomes G-05.

| ID | Area | What cannot yet be determined | Next evidence / dependency |
| --- | --- | --- | --- |
| U-01 | Bankruptcy and company/office progression | Bankruptcy trigger/outcome, office purchase eligibility/cost/effects, complete campaign progression | Trace periodic Module2 body `009E3830` and office purchase callers; compare original runtime before enabling. EA-02 discloses the candidate boundary. |
| U-02 | Bank | All original menu-enable writers and exact bank form/message presentation | Trace upstream eligibility writers; borrowing and repayment transactions themselves are implemented. |
| U-03 | Mystery lab | Seven delayed outcomes, dates/dispatch order and saved pending lifecycle | Entry `00987B40` is known: ¥25m debit and seven-way selection. Recover calendar dispatcher/effects before enabling construction. |
| U-04 | Casino | Poker/blackjack/dice rules, coin ownership, game and payout lifecycle | Coin purchase is known: 50 for ¥10m. Trace game forms and settlement, not only entry. |
| U-05 | Own hardware | Requirements, special room project, completion/launch/market lifecycle and ownership restriction | Entries `009864AC`/`00990B70` are known; trace remaining consumers before enabling. |
| U-06 | Polishing and external work | Entry limits, payment/progress/completion/cancellation and room lifecycle | Recover their original callers and consumers; keep unavailable until complete. |
| U-07 | Payroll | Remaining writers of shared scratch index/member 3018 and original runtime confirmation of apparent index defect | Calendar/statics preserve the known scratch arithmetic; Staff can resolve a missing projection. Do not replace it with a salary sum. See `payroll-state.md`. |
| U-08 | Employee progression | Student growth, speed-technology consumer and top-level stress-relief effects | Trace remaining stat/growth writers; known training/capacity transactions remain supported. |
| U-09 | Automatic conversations | Presenter timing, queues, salary/renewal/graduate callers and original busy-window/modal cadence | Recover caller → queue → completion sequence. Modern saved dialogue pages do not establish original scheduler parity. |
| U-10 | Facility presentation | Exact original hot-spring reactions/animation, shared VB RNG-stream parity and lawyer portrait/background timing | Verified transactions/odds are supported; compare original runtime for presentation and random-stream ordering. |
| U-11 | Room/tenant interaction | Original overlapping-hit priority, right-click paths and busy-window toggle sequence | Modern overlap chooser is intentional; original sequence remains unverified. |
| U-12 | Raster localization | Remaining month/week/moving text, ambiguous bubble captions 13/14 and some bitmap writers | Hash-guarded local overlays need exact source regions/text; original bitmaps remain local. Existing vacancy signs are translated. |
| U-13 | Names/text/fonts | Provisional English personal-name readings, original setup defaults and graphical Japanese glyph coverage | Owner/original comparison and native Windows screenshots; imported/save identities stay unchanged. |
| U-14 | Original saves | Full serialization order/encoding, genuine fixture compatibility and dirty product-slot behavior | Module1 `009D9824`/`009D60E0`, genuine locally provided saves. Modern JSON support is separate; never publish original saves. |
| U-15 | Original comparison | Hiring-capacity/failure paths, exact recruitment runtime, initial staffing/development power, remaining meeting scenes and cosmetic draws | Targeted original runtime/static comparison. Existing authored synthetic tests are not original graphical proof. |
| U-16 | Package/native behavior | Fresh-machine dependencies, Windows graphics/scaling/input and sustained play | G-01; exported host checks cover managed initialization/resources, not native Windows execution. |

## Continuation record

M72 adds save recovery, visible scope, current candidate documentation and this register. All 25 exact community OGGs were restored through the existing connector and matched to immutable main's Git blob hashes; seven music tests pass. The full source-host core loop passes at fixed 60 FPS. Standard SDK attempts failed first on missing process metadata and then on the NuGet named mutex; this is recorded rather than counted as a passing source export. Publication and final artifact verification are recorded in issue #55 after completion.
