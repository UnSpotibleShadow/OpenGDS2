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

M43 resolves hardware picture mapping and shows the locally imported 64 × 64
image beside its bit width. Older caches can reimport the original LZH.
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

## M43 hardware pictures

`MDIForm1.ImageHard` contains fifteen serialized 64 × 64 pictures. Bitmap file
offsets start at 1A9F3E with stride 3069; the stored array index descends 14..0.
The control name and repeated index immediately before each bitmap establish
identity independently of visual similarity. Source 0 is at 1D44FC.
`009940B8 +009B..00E8` accesses the MDI array with the selected hardware source
index for 0..10. Other array entries are imported but are not used to invent a
new own-hardware workflow.

The importer validates control name/index and dimensions and writes only to the
user's local cache. No image bytes or reference contact sheet are public.
`tools/verify_hardware_picture_evidence.py` checks source-selection and resource
anchors. The private UI fixture compares normalized RGB pixels for the opening
platform and a newly licensed platform after changing selection. Public tests
continue to run without original artwork. M43 has 32 Python checks (22 public,
ten private) and 206 domain checks; both Godot suites pass.
