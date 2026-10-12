# Presentation handlers and missing actions — M93 / issue #95

This is an analysis milestone, not a gameplay or UI implementation. The owner's
native screenshots establish visible layouts and the desk-assignment gesture.
The handler findings below come from the supported original PE and its control
event tables. Compared source is M92 main
`71aaa1dac96c31894a8a64ddc2102b69f298f16f`. Full traces, original pictures,
reviewer names and original message bodies remain private. Native Windows timing,
focus and retained-stack behavior are not newly benchmarked.

`tools/verify_presentation_handler_evidence.py` checks the bounded evidence in
`tools/presentation_handler_evidence.json`, including procedure fingerprints,
control-to-event bindings, numeric anchors and local resource identities.

## Proposal and final completion actions

| Form/control | Handler | Established action |
| --- | --- | --- |
| Proposal `fGMKikakusyo.Command1`, index 0 | `0099BA80 +0000..0423` | Close proposal, debit its quote, initialize the selected room/project and enable its panel. |
| Same control, index 1 | `0099BA80 +0426..04A7` | Discard the proposal, close it and reopen planning meeting mode 0; no acceptance debit. |
| Same control, index 2 | `0099BA80 +04AA..04C8` | Close/cancel without accepting the project. |
| Completion `fGMKansei.Command1` | `0097D3CC` | Close completion and open `fGMSyukka` modally: initial shipment/title/price/quantity setup. |
| Completion `Command2` | `0097C484` | Restore the main UI and close completion; no shipment and no bug removal. Ordinary simulation can continue debugging. |
| Completion `Command3` | `00982A24` | At bugs >=60, display the secretary's warning and postponement advice; not itself shipment or a forced rejection. |

The proposal's accept/redo/cancel behavior already has modern equivalents.
The title field on the modern proposal is additional metadata: the original's
initial-shipment form contains the title input. Do not invent an earlier native
title-entry stage to justify that field.

Completion load `009974D8 +03BC..03EC` selects `MDIForm1.ImageFormG[0]`, the
same already-imported 256x160 planning scene. This resolves the completion
picture's identity; it does not require a new invented illustration. Proposal
artwork is different: serialized `fGMKikakusyo.Picture1` contains the 256x160
document picture at file offset `2AE079` (42,036-byte BMP). The form's serialized
name occurs at `2ADA88`; its nearby Picture1 resource and private visual
inspection corroborate the binding. It is not imported by the current graphics
importer.

Completion timer `0099B554` runs its commentary once per load through latch
`A146D0`. It praises the first qualifying >=100 quality in order Fun, Novelty,
Music, Graphics, then selects bug advice at >=60, >=30, >=10 or below 10. Twelve
possible random-employee portrait calls are already listed as pending in M92;
do not repeat that caller inventory or assume M92 implemented these speakers.
The modern completion decision and final-meeting messages exist, but the combined
illustration/summary window and native layout remain missing.

## Promotion is a separate missing workflow

Active report `fGMDevW1.BGraphic` click `009AAA8C`, control index 3, checks room
stage `A12974[room] >=1` before opening `fGMSendenBaitai`. Earlier stages show
an alpha-completion refusal. This is independent of an exhibition calendar event.

Media selector click `00980A38` writes selected media index `A148FC` and enables
confirmation. Confirm `00982064` compares the selected fee with funds, shows an
insufficient-funds message on failure, or opens meeting mode 1 through
`0097BDB8`. Opening/selecting media does not charge. Cancel `0097A998` closes it.

The initializer calls catalog writer `0097CA20` six times at
`009F651C +A252..A31F`. The recovered fees and recognition draw rules are:

| Media index / authored description | Fee | First use base | Second use base | Later base | Random span |
| --- | ---: | ---: | ---: | ---: | ---: |
| 0 / Magazine | 5,000,000 | 6 | 3 | 0 | 9 |
| 1 / Radio | 10,000,000 | 7 | 3 | 0 | 9 |
| 2 / Television | 20,000,000 | 10 | 5 | 0 | 9 |
| 3 / Cinema | 25,000,000 | 8 | 4 | 0 | 9 |
| 4 / Internet | 20,000,000 | 11 | 5 | 0 | 11 |
| 5 / Newspaper | 25,000,000 | 12 | 6 | 0 | 13 |

Gain is base + floor(Single Rnd * span). Use count is per promoted target,
not per medium. Meeting timer `009EFA40 +0DB5..17D5` waits until its phase counter
is >=27, caps the gain against recognition 99, reports the result, increments
target recognition/use count, then charges through the financial helper at
`+178C..17A4`. The cosmetic frame cycle is separate from this gameplay draw.
If recognition already exceeds 99 (for example after an exhibition), this cap
can produce a negative adjustment back to 99. Do not add a zero-floor to the
native formula while presenting it as unchanged behavior.

**Native scope quirk:** this timer tests local Variant -616 at `+0FBE`, `+134B`
and `+1703`, rather than the apparent scope global `A145C4`. The complete decoded
timer has no write to that local. Selector load writes `A145C4=0`; its decoded
reference inventory contains no consumer (nor a recovered member-35A8 reader).
Under the documented fresh-Empty local-frame premise, the timer follows the
development-room path. Its unused released-product branches are also inconsistent:
the earlier count read uses room index `A14384`, but the commit uses product index
`A12BC4`. Do not implement a clean released-product advertising API by assuming
these branches are the intended behavior. Room promotion has concrete rules;
released targeting/retained-frame parity needs an explicit compatibility boundary
or a native experiment. This is a specific native quirk, not missing fee evidence.

The modern free recognition change at type confirmation and company public
relations are different actions. Neither implements this paid media workflow.

## Desk right-click assignment

`fMainVisual.Form` mouse-down event slot 19 binds to `009C66BC`. Left input
selects a workspace actor through `+0758..08CB`; selected actor global `A11ACC`
is required by the right-input branch at `+08CE..08E2`.

Desk actions 6..17 have distinct hit regions, preserved numerically in the
evidence manifest. X bounds are inclusive. The Single-narrowed transforms
S=Y+X/2 and D=Y-X/2 must fall strictly between their recorded bounds. These are
desk targets, not the broader two room-menu diamonds currently used by the UI.
The right handler applies secretary break-policy stress thresholds 100/80/60
for policies 1/2/3, bypasses that gate for policy 0/rest targets, clears the
worker timer at `+10E2..10EF`, then calls existing assignment helper `009AF6BC`
at `+10F9`. Rejected rest requests mark the native break-request flag.

The helper owns occupied-seat rejection, releasing the old desk and starting
walking, not teleporting. Existing modern assignment preserves those core rules.
However, the native caller clears the timer *before* the helper's occupancy
check; do not assert that occupied-target requests are wholly side-effect-free.
Current office UI lacks the right-input path. Preserve the existing selection
identity and modal guards when adding it; review the failure side effect explicitly.

## The outlined panel behind the PC

The owner's corrected red outline identifies the room's animated table panel,
not the monitor/computer tile. Private atlas inspection matches its two frames.

| Item | Verified native mapping |
| --- | --- |
| Allocated room-panel sprites | IDs 91 and 92, member `22DC`, global `A132F8` |
| Initial allocations | `009F651C +41BE..425F`, 62x31 pixels, source Y=226 |
| Frames | Source X=259 and 321 |
| Clock | Per-room member `22F8`, global `A13314`; timing factor 2 |
| First frame | Counter equals 3*2 (6), `00A0283C +06A4..06C7` |
| Second frame/reset | Counter equals 6*2 (12), `+06CB..0706` |
| Advance condition | Room pause flag `A12A8C=0`, `+0707..0737` |

Room state 1 executes this software block; state 2 has the corresponding hardware
block at `+0975..09FA`. Proposal acceptance enables the room panel at X=162 or
258. Empty-room branch `+0A01..0A36` hides the label, progress and panel sprites.
At nominal office speed each half-cycle is six ticks / 0.24 seconds. This is a
per-room effect and is not dependent on a tier-three flame or a particular worker.
Current `OfficePresentationRules` does not project these IDs from project state.

**Separate recovered effect:** computer tiles can also change vertical source
frame using member `3660` at `+2786..27AE`; helper `0098C930` cycles 0/1/2 at
10/18/26 eligible actor passes for seated actions 1..5, 9..11 and 15..17. This
does not identify the owner's outlined panel and must not replace its two-frame
animation. The existing flame and contribution/bubble effects remain implemented.

## Room state/progress labels

Software room branch `00A0283C +0436..073B` chooses native label tiles for
paused work, development stages 0/1/2 and debugging stage 3. It floors
progress/required*100 at `+05F5..061A` and picks the original digit cell through
`+0650..0696`; the screenshot's percentage is now source-backed.
Debugging hides progress digits, shows a debugging state while bugs>0, and
alternates two completion label frames when bugs reach zero. Empty room hides
all three pieces. That completion blink uses shared counter `A13FF4`, whereas
the panel counters are per room. Labels sit at (164,127)/(260,175), and digit tiles at
(194,129)/(290,177). Hardware uses its recovered composite-progress numerator
and denominator, not software progress alone. Restore these as local atlas
projections/translations, not employee speech bubbles or a permanent caption.

## Shipment and post-launch details

Initial shipment form `fGMSyukka` has title input `Text1`, confirmation
`Command1 ->009C9CB8`, and cancel `Command2 ->0097A94C`. Its price/quantity
arrow controls feed timer `009A4D78`. Price steps are 10 then 100 while held;
quantity accelerates through 100, 1,000, 10,000, 30,000, 100,000 and 1,000,000,
with a 100-copy floor and company capacity*100 ceiling. Existing modern spinners
are functional equivalents, not exact native hold-repeat presentation.

Load `009B4344` and timer `009A4D78` prepare/show media + packaging cost per
copy, total production cost, retail price, retailer and transport shares, the
displayed net per-copy amount and quantity times that amount. The cyan amount
is price minus the two shares; it does not subtract production cost a second
time. Existing `ShowRelease` calculates these core values but lacks the full
two-table presentation. Do not change accounting while restoring the breakdown.

The compact post-launch screenshot is `fGMZishaKW`, not the larger
`fGMZishaK` report used by the earlier graph investigation. Its `BGraphic` click
`00984940` checks recall state 2, checks the company-wide weekly production
flag, then opens `fGMSyukkaSai`. The button means another production/shipment
run, not dispatching stock through an otherwise missing transport simulation.
Existing `ShowProduction` supports it with recovered guards.

Compact timer `009C4570` updates units, stock, hardware image and award markers.
The first award box is the contest result (including runner-up/worst markers);
the second is cross-review Hall of Fame. Correct the earlier screenshot summary
that described the second box as a dedicated worst-game panel.

**Two native graph variants:** both use thirteen history cells/twelve segments
and ignore oldest cell 12 when selecting scale, but their scales differ:

| Maximum of cells 0..11 | Compact `009C4570` ceiling | Larger `009C2568` ceiling |
| --- | ---: | ---: |
| Below 20,000 | 20,000 | 40,000 |
| 20,000..29,999 | 40,000 | 40,000 |
| 30,000..39,999 | 40,000 | 80,000 |
| 40,000..99,999 | 80,000 | 80,000 |
| 100,000..199,999 | 200,000 | 200,000 |
| At least 200,000 | 400,000 | 200,000 |

Modern `SalesHistoryRules.ChartCeiling` implements the larger variant. A compact
window needs its own scale; do not overwrite the existing variant's source rule.

## Reviews, exhibition, awards and renewal

Review `fGMCrossR.Timer1 ->009CAFCC` first reveals the four final scores at
callback counters 20/40/60/80, with preview events at 10/30/50/70 and total
commentary at 90. These are timer callbacks, not office ticks. Native cosmetic
score cycling consumes additional random draws; modern score generation omits
that cycling/comment RNG and should not claim whole-session RNG equivalence.
`Command1 ->0097A524` closes the review form.

Final score digits use local `Image2[score-1]`. Individual comments draw from
six score bands <=2, <=4, <=6, <=8, <=9, else, with respectively
9/8/11/9/8/4 reachable variants. The last band contains a fifth case which its
floor(Rnd*4) cannot reach. Vertical reaction pictures are copied from `Image3`
indices 4/8/9/6 for the first four bands and index 7 for score 10; **score 9 has
no final reaction-picture write**, so retain its native display boundary rather
than inventing a missing caption. Portrait/name images are local serialized
controls; their artwork/wording must not be committed. Modern review UI lacks
these individual comments/reactions and the staged presentation.

Exhibition selection `fGMGameDexSn` uses `0098FBA8` for rooms and `009908C4`
for released products; confirm `00989C5C` debits 10 million per entry, at most
five, and opens results. Booth load `0098B810 +0124..014B` selects
`ImageFormG[26]`: a single-frame 256x160 GIF at file offset `11243`, size 21,264.
Awards load `00996CB8 +0129..014B` selects `ImageFormG[4]`: a single-frame
256x160 GIF at `1367D8`, size 19,520. The current importer only extracts a BMP
subset, so these GIF resources require local import support. Single-frame
backgrounds must not be mistaken for the native timer-driven scene animation.
Exhibition gameplay and awards winner/prize logic already exist; keep their
saved results separate from any restored presentation/reels.

The contest's runner-up label is corroborated by compact award-marker value 4
in `009C4570 +0578..05ED`. Current `AWARD_Design` translations need correction
to runner-up Grand Prix while preserving the implemented selection formula.

Annual renewal is **`fGMSKoyouKz`, not `fGMKeiyakuS`**: weekly routine
`009DB4F4 +0552..0562` opens the former at March end. The latter is the separate
individual renewal/graduate workflow. `Check1 ->009837BC` totals checked rows
using the source roster map. Confirm `00995CDC` checks that sum against funds,
dismisses unchecked employees with individual reactions, and debits once.
Roster select `009BC890` and load `009BF5A0` populate the detail/skill controls.
Current annual checkboxes/transaction exist; restore the selected-person panel,
skill radar and totals rather than routing this to the graduate selector.

## Implementation boundary and handoff

Issue #95 remains open: analysis resolves handlers/resources, not its unchecked
UI work. Start presentation fixes with the right-click desk gesture, room labels
and corrected table columns, then the combined proposal/completion/shipment and
post-launch forms. Add promotion as a separate source-backed gameplay milestone
with a documented native-scope boundary and saved one-time result/charge state.
Restore review/event animation from already prepared outcomes rather than
rerolling results while drawing. Include target identities, native graph variant,
room/actor counters and modal/close behavior in focused checks as appropriate.

The remaining limits are concrete: garbled captions cannot establish native
font rendering; native frame lifetime and mode-1 promotion scope need a native
comparison for claims beyond the stated Empty-frame premise; exact scene motion
and focus/window stacking require runtime reference. The video attachment was
unreadable because it exceeded the tool's 32 MiB transfer limit; screenshots and
private static tracing, not a claimed video review, support this audit.
