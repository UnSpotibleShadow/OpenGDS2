# Quote/meeting layout — M42

The quote now follows private reference 174944: hardware table above audience
table, selected bit width and totals to the right, Planning Meeting and Cancel.
The toolbar can choose either room; an office-interior click retains its room
and hides the redundant selector. Numeric columns align right; unlicensed
available hardware is grey and not selectable.

## Verified source and retained rules

| Behavior | Source |
| --- | --- |
| Hardware + audience costs/points | `00986088`; existing DevelopmentPlanningRules |
| Hardware selection only proceeds with license flag = 1 | `009940B8 +0000..0040`, false branch to +0309 |
| Shipment display floors original market and appends ten-thousand units | `009C55EC +08FA..0918`; global A1206C/member 1050 |
| Gate target bits against owned computer; then check funds | `00988A34 +0020..0137` |
| Quote closes and starts meeting through helper Module1.38 | `00988A34 +0137..015E`; helper `0097BDB8` |
| Helper opens fGMKikakuKaigi modally | `0097BDB8 +000C..0021`; pool 23 → 005FAF54 |

English shipments display floor(market) × 10,000 as units, rather than the
Japanese ten-thousand-unit suffix. Missing mutable market history displays `?`;
initial catalog values are not substituted for an older campaign's market.

Opening the quote, changing selection and opening the meeting do not debit
funds or create a project. Acceptance still uses the existing strongly typed
plan check/service, charges once and starts the same development simulation.
No cost/quality/sales formula is changed by this presentation milestone.

## Presentation limits

Hardware picture resource-to-source mapping is unresolved; the layout reserves
its space and shows the verified selected bit width without a fabricated image.
The following meeting is the existing simplified title/focus proposal UI in a
separate compact window. Original animated meeting portraits, dialogue, voting,
proposal/title sequencing and all decision modes are not claimed restored.
The original quote separation is verified; graphical equivalence remains open.
English labels require modestly more width than the Japanese reference.

## Validation

Local C# compilation, all 206 domain checks, 30 Python checks and both Godot
suites pass. Public core-loop checks verify English/Japanese table labels,
quote/meeting separation, no early debit, acceptance, a second project and
release/sales regressions. Private original UI checks verify three opening
hardware rows, seven audience rows, grey license guards, ¥20,000,000 / 400-point
opening quote, 150,000 opening shipment units and both office room contexts.
SDK/export/native Windows validation is tracked in the checkpoint.
