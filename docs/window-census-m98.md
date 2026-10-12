# Original window census and additional presentation gaps — M98 / #95

Baseline: main `39a3158c399221634f6e7c193ce21936f3e65650`, after M94–M97.
This is a fresh static inspection of the supported original executable, followed
by comparison with current UI methods. No game behavior is changed. Keep #95 open.

The original contains **86 compiled objects: 84 forms/designers and two modules**.
The forms expose **920 metadata control records and 572 bound control events**.
Control records are definitions, not a count of visible control-array instances.
The inventory below covers all 84 form names, including both development-room
reports and utility forms. Mapping a modern counterpart does not prove pixel,
interaction, animation, or modal-order parity. This is a census plus targeted
handler research, not a claim that all 572 events have been fully decompiled.

## Newly tracked, source-supported gaps

| Gap | Original evidence and current difference | Next implementation boundary |
| --- | --- | --- |
| Separate Hall of Fame celebration | `fGMCrossR 009CAFCC +025F..0265` opens `fGMCrossRDen` in the qualifying review branch. Its load `00986780 +0124..0130` increments the company counter; timer `00982814` copies image 1 at counter 3 and image 2 at >=5 into image 0, then resets the counter. Two separate 256×160 BMP sources show the celebration banner. Modern `ReviewRows` shows an assessment and sequel eligibility; `ReviewService.Acknowledge` applies Hall status/count without this intervening celebration window. | Add the scene and native ordering while preserving the existing one-time counter/sequel behavior. Image 0 is the destination, not a third recovered celebration image. |
| Illustrated sales-achievement notices and secretary sequence | `Module2 009E3830 +1164/+11AB/+11F2/+1239` opens `fGMOuri`. Load `009B4F34` chooses the 100k/250k/500k/1m branch, its corresponding Image1 illustration, milestone write and secretary/comment/record-update paths. Four 256×160 illustrations are embedded separately. `ShowSalesMilestone` currently has title, threshold and acknowledgment only. | Restore artwork and bounded message order; retain saved milestones, ranking writes and accounting. A single text notice is not the entire native sequence. |
| Advanced-programming start/stop window | `Module2 00A0283C +50B7..516B` establishes software-room/stage and counter/random guards; `+5469` opens `fGMKoudo`. Load `009A19A0` initializes the reel and employee portrait/text. Button `00980EB8` moves mode 0→1→2, changes to Stop, disables after stopping and sets a 5/6 stop counter from a draw. Timer `0098F68C` cycles/reveals results and opens meeting modes 9/10/17 for failure/success/super-success. There is no corresponding modern start/stop scene. Ordinary burst counters and `TechnologyApplicationService` are not evidence that this separate interactive flow is implemented. | **Workflow gap, not merely cosmetic.** Trace reel weighting, selected-actor mapping, mode-9/10/17 mutations and caller resume before implementing it. Do not approximate it with an automatic technology result or ordinary burst animation. |
| Media/capacity information dialog | Both room reports bind their label handler (`0097C2D4` / `0097C3AC`) to `fGMMedia`; shipment load `009B4344 +0B65` also opens it. Load `009A2F24` reads current size, media-price/capacity data and selects hardware illustration variants. The modern project panel prints size and the shipment panel prints costs, but exposes no equivalent explanatory dialog. | Recover the exact label-index gate and displayed media bands; keep it informational. It is not an additional charge or development checkpoint. |
| Detailed additional-production form | `fGMSyukkaSai` has its own native load/timer and quantity handlers, distinct from initial shipment. Current `ShowProduction` has a quantity spinner and total cost, while the M95 cost/distribution tables apply to initial `ShowRelease`. | Compare and restore the reproduction-specific rows, title/hardware display and held-spinner behavior without copying editable retail-price controls from another form. Existing recall/weekly allowance guards remain authoritative. |
| New third-party hardware announcement card | Weekly `009DB4F4 +121A` opens `fGMHardHatubai`; load `009936FC` fills device/cost/bit fields and image, and acknowledgment `00980D38` emits the secretary's license encouragement. Current calendar presentation supplies conversations and a hardware-name notice, without the separate data/image card. | Restore the launch card and acknowledgment sequence; do not treat it as a new own-hardware release or an automatic license purchase. |
| Company-activity confirmation/result presentation | Three MDI activity handlers open `fGMSeisanZ`; load `00994414` supplies activity-specific explanation/cost. Confirm `0099ED68` emits before/after secretary reports, new-genre notice and production-expansion variants. Modern `ShowCompanyActivities` combines choices, directly calls the implemented service and reports a result enum. | Recover and save the confirmation/result sequence around existing formulas, fees and weekly gate. Preserve reported values/order without applying the activity twice. |
| Original help/topic viewer | MDI `00979D10` opens `fGMHelp`. Metadata binds topic-label interaction `00998634`, scrolling `00989310`, button `0098291C`, and load `00980064`. Serialized data includes a separate 250×171 title illustration. Current Help menu links inspectors, rankings and charts; there is no native-style topic browser. | Inventory the topic content/bindings before local extraction or authored equivalents. The title artwork alone does not establish every help-page illustration or link. |
| Numbered save/load selection and previews | Separate `fGMGameSave` / `fGMGameLoad` forms have scroll, row selection, preview pictures, company/date labels and confirm/cancel. Confirm `0099D598` / `009B2C24` uses a selected numbered `GameSave*.GD2` path. Modern Save/Load operates a single JSON slot with a backup, without the native selection/preview windows. | Presentation/product feature gap; modern multiple-slot UX is separable from still-unresolved original-save compatibility. Do not claim that adding a picker imports native GD2 files. |
| Live sales-chart and reach-time record presentation | `fGMRank100` load/resize/scroll/timer (`009968B8`, `0098D908`, `0097CEF4`, `00995924`) renders a live scrollable chart. `fGMToutatuRank 009A658C` has separate company/date/record panes. Modern `ShowSalesChart` builds a static table when opened; `ShowRankings` presents six text records. Underlying chart sorting and record persistence already exist. | Restore native view refresh, selection/scroll and record-pane layout. Do not invent a new ranking formula. |

## Candidates that are not confirmed new gameplay

- **`fGMZishaNet`: reachable but apparently dormant data.** MDI `00979C44`
  opens it; `0099C4EC` renders an opponent-software list using A1315C/A13170/
  A1318C/A131A8. The saved decoded reference index has only this reader and no
  recovered writer for those four cells. This is not evidence for an implemented
  network session, online multiplayer, or a missing working network simulator.
  Audit initialization/load/helper aliases before deciding to expose it.
- **Company-information pages and utility dialogs:** `fGMInfo` has 32 control
  records including several pictures, staff/game pages and a timer. The modern
  desktop preserves many underlying fields in separate windows, but exact page
  grouping/bars/graphs need a control-level comparison. `MainForm16`, `MainForm3`
  and the generic `fGMKoudou` helper are kept in the inventory rather than
  assigned guessed game workflows. About/version/link differences are lower
  priority and do not imply missing simulation.
- **Own-hardware, recruitment, secretary and tenant layouts:** counterparts
  exist; the census does not certify their full pictures, native table rows,
  default selection, focus/keyboard/held-button behavior or modal ordering.
  Earlier dedicated milestones remain the behavioral baseline. Compare their
  controls before reopening any recovered lifecycle or facility rule.
- **Casino presentation:** dedicated forms are mapped to the existing casino
  screens. Earlier M85–M89 analysis governs game rules; this pass does not claim
  all native card/race art, timing and scene composition match.

## Resources and reproducible evidence

The manifest contains metadata names/counts/bindings and procedure/resource hashes,
not original text, pixels, private traces or executable bytes. The seven bounded
BMP fingerprints are the two Hall banner sources, four sales-milestone pictures,
and the help title image. They are absent from current `EmbeddedGraphicsImporter`.
All preview extraction stayed outside the checkout. Existing artwork #97 remains
an exhaustive-family task; this window pass does not claim to finish every atlas
sprite or serialized picture. Office timing #94 also remains separate.

Run the one research verifier against the supported original:

```sh
python tools/verify_window_census_evidence.py /private/path/GameDev2DX.exe
```

It reconstructs metadata directly from the PE and verifies all 84 form records,
920 controls, 572 bound events, 32 selected procedure fingerprints and seven
resource fingerprints. This research-only change needs no engine build or game
regression rerun. Native Windows delivery, exact fonts/window placement and
retained-frame behavior still require runtime comparison.

## Complete form-to-modern-counterpart census

The UI column names comparison sources, not assertions that every native window
is implemented separately. Utility/dormant/challenge rows retain explicit limits.

| Ordinal | Original form | Controls / bound events | Family | Modern comparison source |
| --- | --- | ---: | --- | --- |
| 0 | `MDIForm1` | 72 / 50 | Desktop/company | Main.Desktop.cs |
| 2 | `MainForm16` | 6 / 3 | Utility/about — native roles require further classification | Main.cs |
| 3 | `fGMInfo` | 32 / 20 | Desktop/company | Main.Desktop.cs |
| 4 | `MainForm3` | 3 / 4 | Utility/about — native roles require further classification | Main.cs |
| 5 | `fGMKaiwa` | 6 / 8 | Portrait conversations | Main.Conversations.cs |
| 6 | `fGMSKoyouKz` | 22 / 9 | Staff/hiring/renewal | Main.cs; Main.RenewalPresentation.cs |
| 7 | `fMainAbout` | 6 / 5 | Utility/about — native roles require further classification | Main.cs |
| 8 | `fMainVisual` | 10 / 10 | Desktop/company | Main.Desktop.cs |
| 9 | `fGMKaihatu` | 18 / 10 | Planning/proposal/checkpoints | Main.Development.cs; Main.CoreLoop.cs |
| 10 | `fGMWSTech` | 10 / 8 | Training/level-up/technology/challenge | Main.Training.cs; Main.EmployeeDetails.cs |
| 11 | `fGMKikakusyo` | 5 / 3 | Planning/proposal/checkpoints | Main.Development.cs; Main.CoreLoop.cs |
| 12 | `fGMDevW1` | 10 / 10 | Development reports | Main.CoreLoop.cs |
| 13 | `fGMZisha` | 14 / 13 | Own-product lists/reports; Net remains a dormant-data candidate | Main.CoreLoop.cs; Main.ProductPresentation.cs; Main.SalesHistory.cs |
| 14 | `fGMSyukka` | 14 / 10 | Shipment/reproduction/media information | Main.CoreLoop.cs |
| 15 | `fGMKansei` | 8 / 5 | Planning/proposal/checkpoints | Main.Development.cs; Main.CoreLoop.cs |
| 16 | `fGMalpha` | 14 / 10 | Planning/proposal/checkpoints | Main.Development.cs; Main.CoreLoop.cs |
| 17 | `fGMbeta` | 13 / 9 | Planning/proposal/checkpoints | Main.Development.cs; Main.CoreLoop.cs |
| 18 | `fGMSyain` | 20 / 8 | Staff/hiring/renewal | Main.cs; Main.RenewalPresentation.cs |
| 19 | `fGMSendenBaitai` | 8 / 7 | Promotion/company activities | Main.Promotion.cs; Main.Company.cs |
| 20 | `fGMMedia` | 7 / 3 | Shipment/reproduction/media information | Main.CoreLoop.cs |
| 21 | `fGMSyukkaSai` | 13 / 9 | Shipment/reproduction/media information | Main.CoreLoop.cs |
| 22 | `fGMCrossR` | 9 / 4 | Reviews/Hall celebration/sales milestones | Main.ReviewPresentation.cs; Main.CoreLoop.cs |
| 23 | `fGMCrossRDen` | 5 / 3 | Reviews/Hall celebration/sales milestones | Main.ReviewPresentation.cs; Main.CoreLoop.cs |
| 24 | `fGMKikakuKaigi` | 3 / 2 | Planning/proposal/checkpoints | Main.Development.cs; Main.CoreLoop.cs |
| 25 | `fKeibaMain` | 37 / 14 | Casino/horse racing | Main.Casino.cs; Main.HorseRacing.cs |
| 26 | `fKeibaModosi` | 5 / 2 | Casino/horse racing | Main.Casino.cs; Main.HorseRacing.cs |
| 27 | `fGMLevelUP` | 5 / 4 | Training/level-up/technology/challenge | Main.Training.cs; Main.EmployeeDetails.cs |
| 28 | `fGMHisyoK` | 15 / 7 | Secretary/employee action-choice utility | Main.Secretary.cs; Main.EmployeeDetails.cs |
| 29 | `fGMComK` | 13 / 8 | Computers/licenses/hardware information/announcement | Main.cs; Main.Calendar.cs |
| 30 | `fGMHLicense` | 13 / 8 | Computers/licenses/hardware information/announcement | Main.cs; Main.Calendar.cs |
| 31 | `fGMKoudou` | 4 / 3 | Secretary/employee action-choice utility | Main.Secretary.cs; Main.EmployeeDetails.cs |
| 32 | `fGMKaiwa2` | 6 / 8 | Portrait conversations | Main.Conversations.cs |
| 33 | `fGMGameDexSn` | 12 / 7 | Exhibition/awards | Main.AnnualEvents.cs; Main.EventPresentation.cs |
| 34 | `fGMGameDexKs` | 5 / 3 | Exhibition/awards | Main.AnnualEvents.cs; Main.EventPresentation.cs |
| 35 | `fGMGameHardInfo` | 11 / 6 | Computers/licenses/hardware information/announcement | Main.cs; Main.Calendar.cs |
| 36 | `fGMKeiyakuS` | 8 / 6 | Staff/hiring/renewal | Main.cs; Main.RenewalPresentation.cs |
| 37 | `fGMHardHatubai` | 7 / 3 | Computers/licenses/hardware information/announcement | Main.cs; Main.Calendar.cs |
| 38 | `fGMRank100` | 13 / 6 | Sales chart/reach-time records | Main.Progression.cs |
| 39 | `fGMTenaSel` | 8 / 7 | Tenants/facility selection | Main.Progression.cs |
| 40 | `fGMTenantOnsen` | 8 / 4 | Tenants/facility selection | Main.Progression.cs |
| 41 | `fGMTenantGameS` | 6 / 3 | Tenants/facility selection | Main.Progression.cs |
| 42 | `fGMTenantReS` | 7 / 4 | Tenants/facility selection | Main.Progression.cs |
| 43 | `fGMOuri` | 5 / 3 | Reviews/Hall celebration/sales milestones | Main.ReviewPresentation.cs; Main.CoreLoop.cs |
| 44 | `fGMHikinuki` | 16 / 7 | Poaching/employment offers | Main.Calendar.cs |
| 45 | `fGMHikinukiSel` | 22 / 10 | Poaching/employment offers | Main.Calendar.cs |
| 46 | `fGMHikinukiKeS` | 6 / 7 | Poaching/employment offers | Main.Calendar.cs |
| 47 | `fGMHikinukiKnS` | 6 / 7 | Poaching/employment offers | Main.Calendar.cs |
| 48 | `fGMTechKau` | 9 / 8 | Training/level-up/technology/challenge | Main.Training.cs; Main.EmployeeDetails.cs |
| 49 | `fGMGameNew` | 7 / 5 | New game/save/load/start gate | Main.cs |
| 50 | `fGMGameSave` | 10 / 9 | New game/save/load/start gate | Main.cs |
| 51 | `fGMGameLoad` | 10 / 9 | New game/save/load/start gate | Main.cs |
| 52 | `fGMKontesuto` | 7 / 3 | Exhibition/awards | Main.AnnualEvents.cs; Main.EventPresentation.cs |
| 53 | `fGMKontesutoHp` | 4 / 5 | Exhibition/awards | Main.AnnualEvents.cs; Main.EventPresentation.cs |
| 54 | `fGMTenantTechS` | 8 / 5 | Tenants/facility selection | Main.Progression.cs |
| 55 | `fGMTenantTechSel` | 8 / 6 | Tenants/facility selection | Main.Progression.cs |
| 56 | `fGMTenantCasino` | 7 / 5 | Casino/horse racing | Main.Casino.cs; Main.HorseRacing.cs |
| 57 | `fGMDevW2` | 10 / 10 | Development reports | Main.CoreLoop.cs |
| 58 | `fGMTenantBengo` | 9 / 5 | Tenants/facility selection | Main.Progression.cs |
| 59 | `fGMKaiwaC` | 6 / 7 | Portrait conversations | Main.Conversations.cs |
| 61 | `fGMSeisanZ` | 6 / 4 | Promotion/company activities | Main.Promotion.cs; Main.Company.cs |
| 62 | `fGMTenantSeisan` | 8 / 4 | Tenants/facility selection | Main.Progression.cs |
| 63 | `fGMTenantNazoP` | 8 / 5 | Tenants/facility selection | Main.Progression.cs |
| 64 | `fGMTenantHard` | 7 / 4 | Own hardware development/launch/report | Main.Hardware.cs |
| 65 | `fGMTenantHardS` | 9 / 5 | Own hardware development/launch/report | Main.Hardware.cs |
| 66 | `fGMDevH` | 3 / 4 | Own hardware development/launch/report | Main.Hardware.cs |
| 67 | `fGMSyukkaH` | 7 / 3 | Own hardware development/launch/report | Main.Hardware.cs |
| 68 | `fGMZishaH` | 4 / 2 | Own hardware development/launch/report | Main.Hardware.cs |
| 69 | `fGMTenantCasinoSn` | 2 / 2 | Casino/horse racing | Main.Casino.cs; Main.HorseRacing.cs |
| 70 | `fCasinoPoker` | 23 / 8 | Casino/horse racing | Main.Casino.cs; Main.HorseRacing.cs |
| 71 | `fGMToutatuRank` | 8 / 3 | Sales chart/reach-time records | Main.Progression.cs |
| 72 | `fGMGameKaishi` | 2 / 2 | New game/save/load/start gate | Main.cs |
| 73 | `fCasinoBJ` | 23 / 7 | Casino/horse racing | Main.Casino.cs; Main.HorseRacing.cs |
| 74 | `fCasinoCC` | 26 / 10 | Casino/horse racing | Main.Casino.cs; Main.HorseRacing.cs |
| 75 | `fGMBank` | 10 / 4 | Bank | Main.Bank.cs |
| 76 | `fGMHelp` | 12 / 7 | Original help viewer — no equivalent topic browser | Main.cs |
| 77 | `fGMZishaNet` | 10 / 6 | Own-product lists/reports; Net remains a dormant-data candidate | Main.CoreLoop.cs; Main.ProductPresentation.cs; Main.SalesHistory.cs |
| 78 | `fGMZishaK` | 7 / 3 | Own-product lists/reports; Net remains a dormant-data candidate | Main.CoreLoop.cs; Main.ProductPresentation.cs; Main.SalesHistory.cs |
| 79 | `fGMHisyoSet` | 17 / 9 | Secretary/employee action-choice utility | Main.Secretary.cs; Main.EmployeeDetails.cs |
| 80 | `fGMKaihatuS` | 7 / 7 | Planning/proposal/checkpoints | Main.Development.cs; Main.CoreLoop.cs |
| 81 | `fGMManga` | 10 / 8 | Planning/proposal/checkpoints | Main.Development.cs; Main.CoreLoop.cs |
| 82 | `fGMZokuhen` | 12 / 9 | Planning/proposal/checkpoints | Main.Development.cs; Main.CoreLoop.cs |
| 83 | `fGMSKoyou` | 22 / 8 | Staff/hiring/renewal | Main.cs; Main.RenewalPresentation.cs |
| 84 | `fGMZishaKW` | 10 / 7 | Own-product lists/reports; Net remains a dormant-data candidate | Main.CoreLoop.cs; Main.ProductPresentation.cs; Main.SalesHistory.cs |
| 85 | `fGMKoudo` | 6 / 4 | Training/level-up/technology/challenge | Main.Training.cs; Main.EmployeeDetails.cs |
