# M105 — presentation plan, Step 1 evidence

Baseline: main `87aab7f7e834cbc66b44fb40367be2a9920fc22b` (PR #108).
Working branch: `codex/m105-presentation-evidence`.

The owner initially requested approval between steps, then authorized all remaining
steps while going to bed. Steps 1–7 are complete and checkpointed; Step 8 publishes
a review PR and updates the open handoff. See the individual Step 2–7 notes and
`presentation-completion-m105.md`. Issue #95 stays open. No merge, exported package,
release, Actions run or repository-visibility change is part of this work.

## Step 1 result

The remaining forms now have bounded designer geometry and control-event
bindings, supplementing the existing M98 census and recovered behavioral rules.
`tools/presentation_layout_evidence.json` covers **35 forms**, **1,633 exact
rectangle anchors** and **240 handler fingerprints**. It contains names, numeric
metadata and hashes, without original captions, pixels or audio.

Coordinates are serialized twips. Dividing by 15 gives design pixels at 96 DPI.
Control coordinates are relative to their native container. These are design
bounds, not a claim that every control is visible simultaneously: form loads,
resize handlers, scroll routines and timers change bounds, row counts and pages.
The native property-table reference was inspected privately to distinguish
packed rectangles, strings, pictures and font records. The full property decoder
and decoded original captions remain outside the checkout. Unknown properties
after the recovered rectangle are not treated as decoded.

## Confirmed layout decisions for the following steps

| Screen | Native client at 96 DPI | Evidence-backed implementation boundary |
| --- | ---: | --- |
| Bank (`fGMBank`) | 347×161 | Three loan-record rows above four loan choices arranged in two columns; borrow/cancel at right. Keep principal, due date, repayment and eligibility. |
| Hardware licences (`fGMHLicense`) | 313×247 | Scrollable hardware/ownership/market/manufacturer fields, lower device picture and description, two bottom actions. Load `009A9018`; selected-row handler `009964BC`; purchase `00989890`. |
| Training (`fGMWSTech`) | 273×238 | Selectable course list, lower description/current-experience area, learn/cancel. Course acquisition differs from technology purchase and use. |
| Secretary hiring (`fGMHisyoK`) | 337×239 | Candidate table, portrait/description, two actions. Keep replacement conversation and existing checks. |
| Secretary policies (`fGMHisyoSet`) | 332×262 | Three **stacked boxed groups**, not three unboxed columns. Production has three options, break-entry four, return three. Three commands include secretary advice, confirm and cancel. Advice command `0097743C` calls the existing advice helper `009B15F0`. |
| Reach-time records (`fGMToutatuRank`) | 400×159 | Separate company/date/record columns in the native picture/table panes, one close action; retain existing persisted records. |
| Sales rankings (`fGMRank100`) | 413×314 | Scrollable live list with separate page/picture areas; preserve the already-implemented refresh and scroll/selection retention. |
| Save / Load | 337×184 / 336×184 | Compact scrollable company/date/funds slot table and confirm/cancel. The designer has table picture panes; it does **not** establish an office screenshot preview. Current rendered office preview and backup loading are useful modern additions, to retain without describing them as native features. |
| Help (`fGMHelp`) | 395×328 | Clickable topic labels and five content pages inside the Picture1[0] viewport; scroll and close controls. Current topics 0–4 map to native Picture1 indices **2, 3, 1, 4, 5** (already established by OriginalHelpImporter). |
| Programming challenge (`fGMKoudo`) | 233×192 | Ten **vertical** reel labels inside Picture1; a separate employee/intro area and Start/Stop command. Existing ten-slot outcomes and timer progression remain authoritative. |
| Manga (`fGMManga`) | 369×264 | Scrollable title/rank/payment fields, lower selection details and two commands. Keep rights costs and source IDs. |
| Sequel (`fGMZokuhen`) | 368×272 | Scrollable predecessor selection, lower hardware/detail area and native commands. Third command is outside the initial designer client, so load/resize visibility must govern its use. |
| Ordinary recruitment (`fGMSKoyou`) | 391×327 | Candidate roster, lower ability/technology panes and hire/cancel. Separate from the graduate prompt (`fGMSKoyouKz`) and poaching selectors. |
| Poaching offer / selector | 506×354 / 392×303 | Offer compares employees in separate panes; selector has a roster with preview and controls. Negotiation result forms remain separate state transitions. |
| Market hardware information | 352×240 | `MDIForm1 00979D54` opens `fGMGameHardInfo`; load `00995588`, timer `00999D04`. A dedicated route and live view are missing. |
| Own-hardware report (`fGMZishaH`) | 226×79 | `MDIForm1 009815AC` opens it only after own hardware launches. Load `0098AFF4`, timer `0098CB60`. It is a compact post-launch report, not another software sales chart. |
| Own-hardware launch | 274×249 | Static 256×160 scene at `0x28D66E`, device Image1 and name/action controls. Load `00994E78` supplies the device variation. |
| Hardware development (`fGMDevH`) | 256×159 | Native status label arrays and timer rather than the current large instructional checklist. Existing lifecycle and technology requirements remain the baseline. |
| Additional production | 352×286 | Distinct `fGMSyukkaSai` cost/quantity form; initial shipment's editable price is not its equivalent. |

These client dimensions guide composition. English may need measured width
increases to keep complete labels; blindly enforcing Japanese pixel widths would
reintroduce the clipping already reported by the owner.

## Employee buttons and remaining artwork bindings

The `fGMInfo` BGraphic click handler `009A3E34` and hover helper `009879A8`
establish the action indices: **0 Level Up, 3 dismiss, 6 technology page, 9 ability
page, 12 additional production, 15 promotion**. The pressed/released handlers
`0098A030` / `0098A224` select the subsequent skin variants, with retained-tab
handling for the ability/technology pages. Match actions to these groups, not to
an arbitrary icon's appearance.

| Action | Active / normal / pressed BGraphic indices | Resource offsets |
| --- | --- | --- |
| Level Up | 0 / 1 / 2 | `29E2A8` / `29DC34` / `29D5C0` |
| Dismiss | 3 / 4 / 5 | `29CF4C` / `29C8D8` / `29C264` |
| Technologies tab | 6 / 7 / 8 | `2A1E13` / `2A162B` / `2A0E45` |
| Abilities tab | 9 / 10 / 11 | `2A065F` / `29FE77` / `29F691` |
| Additional production | 12 / 13 / 14 | `298B4A` / `2991BE` / `299832` |
| Promotion | 15 / 16 / 17 | `299EA6` / `29A51A` / `29AB8E` |

Small action resources are 23×22; tab resources 39×16. Original caps on the tabs
need bounded English text replacement. The existing helper accepts only 63×22
skins and 16×16 arrows, so resource extraction alone cannot display these skins.

The prior audit's card components, race-track resource, sales-achievement
captions and hardware-launch scene remain valid inputs for Steps 4–6. Asset
possession alone does not determine compositing offsets or every animation state;
the native handler/control binding must be used during those steps.

## MainForm3 icon correction

The serialized `MainForm3` header explicitly stores **Icon = -1**, between its
BorderStyle and LinkTopic properties. Its client is 361×40 design pixels. Offset
`0x22EA49` belongs to the following `MainForm16` and must not be reused.

Removed the false `MainForm3` entry from the importer and M104 evidence manifest.
There are now **82 valid child-form icon bindings**, correcting the earlier
83-binding claim. The M104 verifier now checks every icon against its own form's
serialized range. The optional-original domain check also asserts that import
does not produce `WindowIcon-MainForm3.png`. Existing unused stale cache files are
not a UI binding; the normal staged reimport produces the corrected set.

## Eight-step plan and permission checkpoints

1. **Native-screen evidence and icon correction — complete locally.**
2. **Shared window positioning and artwork handling — complete locally.**
   Stable screen identities, employee position retention, missing icon bindings,
   small button/tab formats, PNG overrides with precedence/dimension validation.
3. **Everyday management layouts — complete locally.** Licences, additional
   production, employee actions/pages, training/use, secretary screens, Bank and
   confirmed missing information panes.
4. **Hardware family — complete locally.** Market information, own-hardware
   report, illustrated launch, development monitor and third-party launch card.
5. **Casino graphics/layouts — permission required.** Native card compositor,
   three card-game tables, horse track, betting matrix and results.
6. **Remaining layouts/pixel patches — permission required.** Records, rankings,
   save/load, Help, challenge, manga, sequel, graduate and poaching presentation;
   sales-achievement, Help and hardware-launch English captions.
7. **Original audio integration — permission required.** Inventory original
   audio, identify native event/screen triggers, wire unused files, and check
   playback ordering, interruption/resume and settings. Do not infer a trigger
   from a filename alone or count already-wired audio as missing.
8. **Final verification/publication — permission required.** Consolidated
   bilingual review captures, relevant gameplay/save-continuation regressions,
   PR publication and updated open #95 handoff. No export/release/Actions or
   visibility changes are included.

## Validation and limits

- Native geometry/handler verifier: **35 forms, 1,633 rectangles, 240 procedures**.
- Corrected icon/facility verifier: **82 icons, nine facilities, four previews,
  five procedures**.
- Guard checks reject changed coordinates, an icon borrowed from another form
  and a control crossing its form boundary.
- Optional-original domain suite: **522 tests pass**, including the importer
  correction and absent-MainForm3-image assertion.

No UI layout or gameplay change beyond the incorrect unused icon import is made
in Step 1. Rendering tests are reserved for implementation changes. Private
source assets, captions, traces and the reference runtime stay outside the repo.
Windows DPI, font metrics, focus, stacking and actual timer delivery still require
native acceptance; these static anchors do not certify them.
