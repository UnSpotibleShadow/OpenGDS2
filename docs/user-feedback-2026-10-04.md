# Screenshot feedback and presentation backlog — 2026-10-04

The owner supplied three defect/reference images, eleven additional original-game
screenshots captured with readable Japanese, and a recording for the main title.
This is an analysis and implementation backlog, not a claim that the defects
have been fixed. Original screenshots and extracted game artwork remain local;
this document contains observations and source references only.

Evidence labels below distinguish visible behavior from code inspection and
unproven explanations. A screenshot establishes a displayed state, not every
eligibility guard, transaction, default, or simulation effect behind it.

## Immediate presentation work

| ID | Evidence and current gap | Required work and completion check |
| --- | --- | --- |
| UI-01 | `image(8).png`: Assign a Desk is an oversized two-column button grid, obscures the office, and shows a horizontal scrollbar. `Main.Office.cs` gives each of twelve buttons a 360-pixel minimum width; generic management content has a 620-pixel minimum and a 670 × 520 window. | Replace the oversized selector with a compact assignment interaction that keeps the office visible. Trace the original assignment gesture/form before claiming an original match: no supplied image shows the original assignment workflow. Preserve occupancy checks and room/desk mapping. Verify every seat, occupied seats, cancel, reassignment, English/Japanese, and mouse coordinates at multiple sizes. |
| UI-02 | `image(9).png`: a narrow red/blue vertical artifact appears at a staff sprite edge near the reception desk. This differs from the red selection arrow above an employee. `OfficeStaffRules.Sprite` maps 16 × 32 rectangles into the imported atlas; `OriginalOfficeRenderer` composites colors/masks in depth order. | Reproduce using the matching portrait, frame, facing and overlap. Compare raw imported color/mask samples, atlas rectangles, compositing, draw order and final display against the original. Atlas spill, mask interpretation and scaling are hypotheses, not a diagnosed cause. Check walking, sitting, working and resting frames before changing a verified raster rule. |
| UI-03 | The owner reports that the three additional plots cannot be clicked. The original office screenshot `173702` labels them “Tenant wanted.” `OnOfficeInput` handles only employee hits; `OfficePresentationRules.ForState` does not update facility artwork from occupied slots. | Recover plot hit boxes and original empty/occupied click handlers. Connect known school selection/inspection/closure to the appropriate slot and render occupied plots consistently with saved state. Keep unknown tenants unavailable. Test all three plots and interaction after resize, movement and reload. Current school management is accessible through Company Facilities; a plot is a general tenant slot, not exclusively a school. |
| UI-04 | The owner reports poor rendering above 1280 × 720; `image(8).png` was taken in an embedded Godot viewport labelled 1918 × 1079. `project.godot` uses a 1280 × 720 canvas with `canvas_items` stretch. Office images use nearest filtering, but child windows, fonts, tables and dialogs have fixed pixel geometry. | Establish a coherent desktop/UI scaling policy while preserving original pixel art. Investigate noninteger enlargement, table/header wrapping, dialog minimum sizes, bounds, input coordinates and DPI. Verify 1280 × 720, 1600 × 900, 1920 × 1080 and a larger desktop, plus resize/maximize/restore and narrow aspect ratios. Record desktop screenshots; headless layout checks alone cannot prove visual quality. |
| UI-05 | Original recruitment (`173532`, with `image(10).png` as an earlier mojibake reference) uses a compact scrollable candidate table above a selected-candidate detail section. It has portrait/level, a five-axis chart, speed/inspiration bars, a technology list, contract-fee column and one hire/cancel action pair. `ShowRecruitment` instead stacks a full card and hire button for every candidate. | Restore table selection and the compact detail arrangement, reusing the employee chart/bars where appropriate. Show available funds and selected contract fee clearly; align numeric columns and localize headers/help. Selecting a row must not regenerate candidates or debit funds. Check insufficient funds, successful/failed hire, an empty roster, scrolling, cancellation and both languages. |
| UI-06 | `174033`: a compact secretary conversation popup with portrait/role badge on the left, green speaker-name strip and multiline message on the right. This instance concerns the approaching game awards. `173559` shows a similar small new-hire greeting; `173702` also shows an in-office speech bubble. Current event screens use generic management presentation. | Recover and implement the compact dialogue presenter and its event callers. Trace speaker selection, trigger date/state, dismissal gesture, modal clock behavior, queuing and save/resume semantics. Keep dialogue popups distinct from in-office bubbles and action/choice forms. Import original text locally and support community English translations; avoid inventing a branching conversation system from these screenshots. |
| AUDIO-01 | Supplied `ゲームのテーマ(2).mp3`: approximately 75 seconds, stereo 48 kHz MP3, with embedded cover art. Current `BackgroundMusic` only plays an office playlist while gameplay is active; `ShowTitleScreen` disables it. | Use this recording specifically as the main-title theme, separate from the office playlist. Preserve enabled/volume settings and implement title entry, title return, gameplay transition and loop behavior without overlapping players. Convert audio only if needed; WAV is not inherently required. Record supplied-source attribution separately from the code MIT license. This is a community addition requested by the owner, not recovered original-game music. |

The ordering should start with UI-04 and UI-02 so subsequent screen restoration
is evaluated on reliable rendering, then UI-01/UI-05 and plot interaction.
Title music can be implemented independently. These tasks take priority in the
next presentation pass alongside the existing company-progression work.
UI-06 should accompany annual-event and employee-arrival presentation restoration.

## Additional original-screen references

The suffixes in this table refer to locally supplied
`image(20261004-HHMMSS).png` files. These images are not bundled with the project.

| Reference | Visible original presentation | Follow-up |
| --- | --- | --- |
| 173501 | Hardware licensing: platform/license/shipment/company table, selected hardware picture, bit width, descriptive text, acquire/cancel pair. | Replace stacked license headings/buttons with the original table/detail structure; compare displayed market/shipment fields with the verified model. |
| 173512 | Recognition expansion: illustrated panel, description, cost row and confirm/cancel. | Restore the original activity dialog structure using locally imported artwork; retain recovered activity transactions. |
| 173520 | Genre expansion: the same illustrated activity form with its own description. | Preserve the shared layout and distinct action/help text; verify original enabled-state behavior. |
| 173532 | Readable recruitment names, columns, highlight and detail pane. | Primary UI-05 reference; improve local imported-text translation coverage without publishing original name/catalog tables. |
| 173542 | Technology purchase: compact name/price table, selected technology description, purchase/cancel. | Restore selection/details while retaining verified purchase rules and local catalog import. |
| 173559 | New employee: small portrait, name strip and greeting window. | Trace notification dismissal, clock/modal behavior and sequencing before adding the arrival presentation. |
| 173608 | Secretary settings: automatic production growth at funds thresholds of ¥300 million or ¥100 million, or manual; break entry at stress 100/80/60 or voluntary; return at stress 20/0. This captured state selects ¥300 million, entry 80 and return 0. | Add secretary-policy reconstruction to the progression backlog. Visible options are established; screenshot selections are not proven new-game defaults. Trace storage, timing, eligibility, effects and save behavior before enabling automation. |
| 173618 | Computer purchase: name/bit-width/price table and selected computer picture/description, purchase/cancel. | Restore original table/detail presentation; retain recovered computer transactions and desk artwork. |
| 173629 | Bank: three loan rows, three amount choices, borrow/cancel, and a displayed one-year term with 30% annual interest. | Use as the bank UI reference. Existing repayment reconstruction agrees with the displayed term; entry/amount eligibility and transaction/menu lifecycle still require tracing. A shown option does not prove it is available in every company state. |
| 173702 | Main MDI desktop: small office, adjacent company summary, compact staff table, sales top-100 window, toolbar and bottom help bar. Includes tenant signs and an employee speech bubble. | Compare company-field order, compact staff/product tables, sales-chart column order/number alignment, toolbar associations, window placement, room state and bubbles. Current sales chart puts company before units; this screenshot shows title, units, company. Do not infer startup visibility or initial staff from this captured campaign state. |
| 174033 | Readable secretary conversation popup about the approaching awards, with named speaker and portrait/role badge. | Primary UI-06 reference. Match compact structure and verify the original reminder trigger and dismissal. The image alone does not establish timing, automatic duration, or a response-choice mechanism. |

## Verification and release boundaries

- Original screenshots provide stronger presentation/localization evidence now
  that Japanese is readable. Employee readings, hidden interactions and gameplay
  defaults still need independent verification.
- Do not publish original screenshots, artwork, decoded catalogs or patches
  containing original pixels. Required original assets continue to come from
  the user's legally obtained LZH, with translated pixel output generated locally.
- Keep the supplied title recording separate from original-content imports and
  the source-code MIT grant. Its use is an explicit owner requirement.
- Add focused interaction/rendering regressions when implementing these changes,
  then perform graphical desktop comparisons. The previous SDK/export/core-loop
  passes do not resolve these newly reported usability defects.

See [remaining work](remaining-work.md),
[original desktop and music](desktop-ui-and-music.md),
[school facilities](school-rivals-and-rankings.md), and
[employee inspection](compact-employee-presentation.md).
