# Annual calendar events — M36

This milestone replaces the exhibition, annual contest, Beginner ending and
known graduate-event stops with resumable domain services and English/Japanese
Godot dialogs. Evidence is targeted static analysis of the supported local
executable. Original Windows behavioral comparison is still unavailable.
The original LZH remains required; no original assets, executable, full traces
or employee/rival catalogs are included in the repository.

## Exhibitions

April and October week 1 open the selection form after hardware markets and
before loans. The source is `009DB4F4 +1669..177E`, opening `fGMGameDexSn`.

| Behavior | Verified source |
| --- | --- |
| Eligible own development rooms: state 1 | `009AD898 +06D8..0932` |
| Eligible released products: state 2; newest first | `009AD898 +03B5..0418` |
| Maximum five products; room selections precede reverse-release selections | `0098FBA8 +0014..017E`; corresponding product handler `009908C4` |
| Fee ¥10,000,000 per selected product | `0098FBA8 +01BA..01C2` |
| Reject no selection and insufficient funds; debit before results | `00989C5C +0000..00F7` |
| Every fourth timer tick: add floor(Rnd × 350) + 450 visitors | `0099AB50 +0084..0108` |
| End at tick 90: 22 visitor increments; range 9,900–17,578 | `0099AB50 +010C..0117` |
| Gain = floor(visitors / 500) | `0099AB50 +02B8..02C8` |
| Room awareness caps at 100 by reducing the shared gain local | `0099AB50 +02E6..03BB` |
| Released-game awareness is not capped by this branch | `0099AB50 +03BC..03E4` |

The shared cap is preserved: a room at 99.5 reduces the gain to 0.5 for every
later exhibited product, even if the original gain was 19. The report displays
the final shared gain. It is not a uniform per-product bonus. Company fame is
not increased by this inspected results procedure. Opening attendance record
member 2BC is Empty/zero (`009DD234 +01A7`). Missing historical records in older
modern saves remain unknown.

Godot shows selection and then a saved results dialog. The original timed
visitor animation is compressed into one transaction; all 22 gameplay visitor
draws and selection order remain. Cancel/skip continues to the loan phase.
Reloading results does not repeat the fee, visitor draws or awareness changes.

## Annual contest

December week 4 opens `fGMKontesuto` before weekly accounting. Own entries are
all products whose recorded release year equals the current year
(`00996CB8 +033B..0390`). New releases now save year/month/week. Older products
with unknown dates pause the contest instead of acquiring an invented year.

Selection is in `fGMKontesutoHp.Timer1`, `009BD6F8`; rewards and product markers
are in `fGMKontesuto.Timer1`, `009CEE10`.

| Category, presentation order | Own-company selection | Signed payment | Product marker |
| --- | --- | --- | --- |
| Music | Music ≥70 and floor(Rnd × 10) =1 | +¥100,000,000 | 3 |
| Graphics | Graphics ≥70 and floor(Rnd × 10) =1 | +¥100,000,000 | 2 |
| Design | (Fun + Novelty)/2 ≥80 and floor(Rnd × 10) =1 | +¥300,000,000 | 4 |
| Worst game | Second pass: Fun ≤30 and floor(Rnd × 2) =0 | −¥200,000,000 | 1 |
| Grand prize | Four-stat average ≥120 and floor(Rnd × 10) =1; or lifetime units ≥1,000,000 | +¥500,000,000 | 5 |

**Verified source quirks:** the ascending ElseIf thresholds make higher
music/graphics/design/grand-prize thresholds unreachable. Each successful later
entry replaces the previous winner; the scan does not stop at the first win.
Worst-game selection first consumes draws for average ≤50, then clears that
pass's winner and runs the second pass. The second pass draws for every entry,
including entries with Fun >30, because the original And is not short-circuit.
The million-unit grand-prize override follows the quality draw; it does not
skip a qualifying quality roll. Product markers are overwritten by later
categories, as the original stores one marker rather than an award flag set.

Each category has separate announce and continue actions. Its winner, payout
and cursor survive saving; announcing again cannot pay again. Worst-game
penalties can take funds negative. Period resets later in the same boundary
retain monthly/yearly awards but clear the weekly accumulator. No extra fame,
Hall-of-Fame count or invented quality bonus is added.

**Remaining presentation/compatibility gaps:** the original rival-product pool,
its refresh/release lifecycle, competitor names, stage animation and their
cosmetic random draws have not been connected. Godot reports that another
company won when no own entry succeeds, without inventing a rival title.
Exact VB6 global random-stream/seed parity is not claimed. The modern saved
first-grand-prize date is a per-company record; the original cross-company
`GameRank.GDS` leaderboard remains separate and unimplemented.

## Beginner ending

`fGMGameKaishi`, `009886C4 +0015..0056`, writes tutorial marker 3B0C =−1 for
Normal and a nonnegative marker for Beginner. The weekly guard tests marker
≥0 at year 2, December, transient week 5 (`009DB4F4 +01C7..0211`). It closes
windows, restores the title presentation and exits at +04CA, before renewal,
payroll, tenant expiry or hardware work. OpenGDS2 preserves a valid closing
week 4 in the terminal pending event and offers return to the title screen.
Normal crosses this boundary into January. No Normal ending date is invented.
The tutorial's prose about a three-year limit is not used to replace the
explicit source predicate. Original restrictions on saving Beginner games and
the full tutorial sequence remain compatibility work.

## Graduates

April week 2, after loans, checks whether any of three member 444 facility slots
is zero (`009DB4F4 +197F..19F6`). The zero identifies the game school: its form is
`fGMTenantGameS`, and the initializer's facility definition supplies its name.
All three slots start at −1; ordinary new companies do not silently gain a school.

Source indices are scanned from 0 through A60−1 and filtered inclusively by
A70 ≤ index ≤ A80. Initialization sets A80 = A60−1
(`009F651C +1CDB..1CEE`). Supported new databases save this maximum explicitly;
older missing bounds remain unknown. Gate C84 =0 is eligible.

The source counts eligible employees, then repeatedly scans the entire bounded
range. Every in-range record consumes floor(Rnd × 5), including employed
records. Gate zero and draw zero selects the record. With at most two eligible
sources, the first pass is accepted even when empty. With more than two, whole
passes repeat until at least two are selected. The accepted list can contain
more than two; it is not a two-person cap.

A nonempty result opens one `fGMKeiyakuS` prompt per candidate in source order
(+1BB3..1C04, mode 37E0 =1). Acceptance checks the displayed source contract fee
and calls the shared employee-copy/debit helper (`00999858 +0260..0279`), then
consumes one of four greeting draws. Declining only closes this candidate's
prompt. No unproven flat school discount is added. The persisted queue resumes
at the same candidate after load and cannot hire or charge an accepted one twice.

Additional facility evidence for the next reconstruction pass: initialization
`009F651C +9963..9979` defines school cost ¥600,000,000. The purchase handler
`00988F78 +0000..0143` checks funds, rejects a duplicate across all three slots,
subtracts directly from cash (not through the signed financial accumulator),
and writes the chosen facility into the selected slot. Company-info label
indices 21–23 map to facility slots 0–2 in `0098D21C +0000..0042`; an empty slot
opens selection at +01E7..020E. The school close handler `00984480` writes −1 to the
selected slot after confirmation (+0098..00B3). Availability/visibility
of those company-info controls still needs tracing. No separate student-growth
formula was found in the inspected school form; this does not prove its absence
elsewhere.

**Remaining school work:** facility selection, purchase availability, school
construction/closure, student growth and office/rank progression have not been
connected to normal play. The graduate calendar branch works for a known saved
school slot and source bounds; automated fixtures exercise those known states.
It does not fabricate a school or expose an unverified construction shortcut.

## Saves and validation

New annual payloads belong to the existing calendar transaction. Required
selection/entry/cursor fields are checked; detached payloads, duplicate or
unknown candidates, mismatched winners and invalid dates are rejected.
M35 exhibition and ending stops resume their original cursors. Missing earlier
release dates or graduate bounds remain explicit uncertainty stops. Pre-M35
`Calendar = null` saves retain their established legacy behavior.

Eighteen added public domain regressions cover visitor limits, shared cap,
selection/fees, source ordering, all award categories, original branch defects,
signed payouts, overflow, save/load idempotence, graduate retries/bounds,
hire/decline, Beginner/Normal and M35 cursor migration. The native Godot core
fixture exercises actual exhibition controls/results, all five award dialogs,
a deterministic million-unit grand prize, graduate hire/decline, reloads and
title return. Bounded checks in `verify_core_loop_evidence.py` independently
verify the corresponding original instructions without publishing game text.
