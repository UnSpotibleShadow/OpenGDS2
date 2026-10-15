# Original facility conversation chains — #118 follow-up

15 October 2026. Investigation requested after PR #121: the Lawyer is acceptable,
but the other facilities need the same depth of original-first recovery.

**Confirmed:** PR #121 did not finish facility conversation parity. It restored a
shared secretary/confirmation/result presentation, but several source-specific
steps and window lifetimes remain different. All nine facility illustrations are
already recovered and imported. Most other facilities use their own illustrated
facility window behind direct dialogue, rather than the Lawyer's separate
business-meeting sequence. Extending the Lawyer's five pages to every facility
would invent behavior.

This follow-up changes research and verification only. The gameplay and UI gaps
below remain to be implemented. Keep #118 and #95 open.

## Evidence and artwork

Supported original SHA-256:
`4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.
`tools/verify_facility_chain_evidence.py` checks 56 procedure fingerprints,
all nine form-load and button-event bindings, and all nine 256×160 pictures.
The evidence includes all procedures of the 12 Tenant forms plus the dialogue
helper, meeting loader, periodic dispatcher and meeting timer. Fingerprints
establish which source was inspected; they are not runtime or semantic tests.

Each facility's Load handler reads `MDIForm1.ImageFormG` through getter `032C`:

| Facility | Original form | ImageFormG index | Existing local asset |
| --- | --- | ---: | --- |
| Game school | fGMTenantGameS | 13 | Facility-School.png |
| Indoor hot spring | fGMTenantOnsen | 14 | Facility-HotSpring.png |
| Recycling shop | fGMTenantReS | 19 | Facility-Recycling.png |
| Technology school | fGMTenantTechS | 24 | Facility-TechnologySchool.png |
| Casino | fGMTenantCasino | 21 | Facility-Casino.png |
| Law office | fGMTenantBengo | 20 | Facility-Lawyer.png |
| Mystery laboratory | fGMTenantNazoP | 17 | Facility-MysteryLab.bmp |
| Production laboratory | fGMTenantSeisan | 22 | Facility-ProductionLab.png |
| Hardware laboratory | fGMTenantHard | 23 | Facility-HardwareLab.png |

Assets are under `Graphic/Embedded` in the user's local cache, extracted by
`NativePresentationAssetImporter`. The native load handlers and importer agree;
there is no evidence here that these nine pictures need replacement. A private
artwork contact sheet is retained in `scratch/facility-chains/`. No original
pictures, text, executable or complete disassembly are committed.

The direct service handlers below contain no new ImageFormG assignment or direct
meeting-mode call. The Lawyer alone calls meeting modes 13/14 from its two
service buttons, which load ImageFormG[12]. Mystery's later periodic result calls
mode 15 and loads ImageFormG[17]. Hardware launch has its separately recovered
launch illustration. These are different entry paths, not a common facility
cutscene sequence.

## Recovered chains and current gaps

| Facility | Original path | Port gap after #121 |
| --- | --- | --- |
| Hot spring | Secretary states the ¥8m fee → small yes/no prompt → no-staff/funds message if necessary → debit → secretary reports relaxation → employee reaction pages → facility closes. Bath illustration remains in the facility. | Five reaction variants and their staff selection are missing. Confirmation closes the illustrated owner too early. Results are one authored receipt. Error and service-cancel returns differ. |
| Recycling | Secretary states current stock and ¥500 per copy → small yes/no prompt → clear eligible software stock → report total proceeds → credit proceeds → facility closes. Shop illustration remains visible through the direct messages. | Intro only quotes a total; it omits stock count/unit rate. Owner is replaced by a separate result picture. The yes/no wording and return path are generic. |
| Technology school | No employees: secretary message. Otherwise open employee selector over the facility → select/accept → funds check/debit → increase capacity → secretary names the employee and new maximum → close selector and facility. | Port inserts a secretary cost prompt and another confirmation after selection. It removes the parent on opening the roster. Insufficient funds returns to the facility instead of leaving the selector available. |
| Production lab | Secretary states expansion cost → yes/no → one Single outcome draw → secretary reports old and new capacity → apply capacity and direct cash debit → facility closes. Factory illustration remains behind the messages. | Result quotes gain/cost rather than before/after capacity; owner disappears on confirmation. Service cancellation reopens it instead of following the handler's final close. No affordability restriction should be added. |
| Mystery lab | Already researching: secretary message. Otherwise fee introduction → yes/no → choose one of seven results and charge → close facility. Later periodic mode 15 shows its own two-page secretary result with the laboratory scene. | Delayed result and scene already exist. Immediate opening/return behavior remains generic; disabled service suppresses the native already-running message. This is not a missing five-page consultation. |
| Hardware lab | Secretary refusal if a console exists or is developing. Otherwise close facility → open hardware design/room selector → funds/busy-room refusal or start development. Later completion/launch is a separate path. | Port disables entry for the existing-console/developing cases and shows other validation in labels. These replace dialogue branches. It already has hardware design and launch pictures. |
| Casino | Illustrated facility → modal selector → horse racing or a card game; no coins produces a secretary message. Coin purchase uses a separate yes/no prompt. Returning from the selector reveals the facility. | Port clears the facility on selector entry, disables coinless card choices and substitutes a status label. Cancel returns to the office. Existing game artwork does not need to become a Lawyer-style meeting scene. |
| Game school | Illustrated facility has back/closure actions. Its service is the separate April week-2 graduate event: introduction, candidates, hire/decline and hired graduate greeting. | That event is already implemented. No additional immediate consultation chain is evidenced by the school form. Check its parent/background and candidate presentation in the focused review; do not fabricate a school service button. |
| Law office | Fee introduction/yes-no → affordability check → close facility → modes 13/14 → secretary/manager consultation and result. | Owner accepts the current Lawyer work. Leave it outside the correction batch except regression coverage. |

### Hot-spring selection matters

`0099E164 +0213..0556` scans the source records in ascending order. It first
counts employed records and clears their stress. Its second loop evaluates a
`floor(Rnd × 2)` expression for every source record, including non-employees,
because the compiled Boolean expression is eager. An employed record speaks
when staff count ≤4, or when that draw is zero for a larger staff. Each selected
speaker consumes a further `floor(Rnd × 5)` draw for one of five reactions.
The speaker's own name and portrait are used; this is not the random-staff
portrait helper. There is no four-speaker cap when staff count exceeds four.

The port currently consumes none of those draws. A correction needs persisted
selected speakers/variants and page position, so loading cannot reroll reactions,
charge again or clear stress again. Incomplete legacy source tables must remain
explicitly unknown rather than inventing source records or a source draw order.

### Window lifetime and cancellation

For hot spring, recycling, production and mystery, the final native unload is
outside the accepted branch: declining the service still closes that facility
window. This differs from declining demolition: the closure handlers return
without unloading when the user declines. Technology-roster Cancel closes the
child only; successful study closes both child and parent. The Casino selector
is a modal child without a parent unload in the opening handler. Hardware closes
the parent before opening its design form.

`Main.FacilityConversation` currently retains the owner only through the prompt,
then `FacilityQuestion.Confirm` calls `CloseFacilityOwner` before invoking the
transaction. `ShowPendingFacilityReceipt` reconstructs a separate scene picture.
That is the principal shared presentation mismatch, even when the right artwork
file is loaded. Preserving a picture alone does not preserve the original window.

## Correction order and acceptance

1. Give each facility an explicit route and owner lifetime. Keep its full
   illustrated window behind direct conversations and modal children; distinguish
   service cancel, child cancel, back and demolition cancel. Preserve positions.
2. Recover the facility-specific local dialogue keys and author English
   translations for introductions, questions, results and refusal branches.
   Restore recycling quantity/rate and production before/after values. Keep the
   accepted Lawyer sequence unchanged.
3. Restore the hot-spring reactions with source-ordered eager draws and a saved
   page/speaker snapshot. Define migration for unread old one-page receipts;
   never synthesize a historical reaction selection during load.
4. Correct technology-study, casino and hardware refusal/child-dialogue routes;
   keep the existing transactions, delayed mystery results, graduates and launch
   lifecycle. Audit source transaction timing separately from modern atomic-save
   boundaries instead of repeating a payment while rebuilding a view.
5. Check every branch in English/Japanese, including missing-cache behavior,
   Cancel/close/back, dragging positions, refusals, source draw counts and
   save/load at every committed page. Capture each facility's complete sequence
   for owner review, not only its first window.

Source anchors: hot spring `0099E164`; recycling `0098DDC4`; study entry
`00983B20`, selector `0099F9A4`, acceptance `009881C0`, cancel `0097AE58`;
production `00988DAC`; mystery start `00987B40`, periodic `009E3830`, meeting
`009EFA40`; hardware entry `009864AC`, acceptance `00990B70`; casino selector
entry `0097A494`, commands `00994780`; school closures `00984480`, annual
caller `009DB4F4`. Nine closure handlers and all Load bindings are listed in the
machine-readable evidence. Existing economic verifiers still apply.

This is static original-first recovery plus current-code comparison. Native
Windows focus, occlusion, animation cadence and exact pixels still require
paired runtime review; the previously crashing Wine runner was not retried.
