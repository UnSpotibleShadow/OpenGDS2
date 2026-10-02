# Development work and planning — M11

These findings come from targeted static analysis of the supported local executable.
They have not yet been compared against the original Windows runtime. No original
graphics, catalog text, binaries or procedure dumps are distributed.

## Implemented scope

The domain contains new-project acceptance, planning focus, work-tier and contribution
formulas, type candidate construction, type/genre confirmation effects and modern
project save validation. These APIs are foundations for the scheduled simulation.
The UI offers a planning preview without financial mutation; Start Development
remains disabled until assignment, completion and release are connected.

Employee cards use user-imported 64×64 portraits. Their file path is constructed
from portrait member C14 in fGMSKoyou procedure 5 +003C..0056. Computer purchases
now update office tile 140: initializer 009F651C +50F4 stores its allocated ID in
member 9A8; fGMComK 0099037C +01B2..0220 sets X=91, Y=96, atlas X=571+74×model,
atlas Y=318. Width, height and depth stay in the imported scene.

## Planning and acceptance

Meeting mode 0 in 009EFA40 +00BB..0D86 draws one of seven focus profiles:

| Profile | Fun | Novelty | Graphics | Music |
| --- | ---: | ---: | ---: | ---: |
| 0 | 40 | 16 | 16 | 16 |
| 1 | 16 | 40 | 16 | 16 |
| 2 | 16 | 16 | 40 | 16 |
| 3 | 16 | 16 | 16 | 40 |
| 4 | 22 | 22 | 22 | 22 |
| 5 | 34 | 34 | 15 | 15 |
| 6 | 15 | 15 | 34 | 34 |

It cycles through four fields, adding one on a floor(Rnd×3)=0 draw, until the total
is 100. These are ordinary development focus weights, not review scores.

New-project acceptance, fGMKikakusyo 0099BA80 +0025..03BC, charges the planning
quote and initializes room state 1, points, zero progress, four zero quality fields,
zero bugs, recognition and data size, hardware/audience selection and stage 0.
Room indices are 0 and 1. The API models the already-licensed, mode-0 branch;
sequel carry-over, licensing conversations and room availability scheduling remain
separate. A modern project label is metadata, not a recovered original title-entry
stage. The project API is not exposed as a playable simulation.

## Ordinary work

The timing factor is 2 and the office tick is 40 ms. Eligibility at Module2
00A0283C +5698..573D uses an idle threshold of
2×(10+(150−1.5×development speed)/10), plus work/task/stress flags.
This function alone is not a complete scheduler.

Category selection draws floor(Rnd×300). The four cumulative focus branches have
inclusive endpoints, and the earlier branch wins a tie. Quality therefore covers
rolls 0..100, bugs 101..110, speed 111..299.

| Category | Roll range before floor | Burst threshold |
| --- | --- | --- |
| Four qualities, stat below 80 | 130−stat/2 | 1+inspiration/17 |
| Four qualities, stat at least 80 | 100−stat/2 | 1+inspiration/17 |
| Speed | 165−speed/2 | 1+inspiration/20 |
| Bugs | 88−Accuracy[source+99]/2 | 1+inspiration/20 |
| Debugging | 120−accuracy/2 | Medium at 1+inspiration/20; no burst |

Ordinary work is medium if its roll is at most 35 after failing the burst test.
Small/medium contributions add 2/4 quality, speed progress or bugs; debugging
removes 1/2 bugs. Burst accumulation caps at 50, stops sampling after 100 ticks
and completes at 120. Quality/speed increments use
floor(Rnd×(60−stat/2)×2)≤10; bug increments use floor(Rnd×accuracy)≤5.
A burst adds its counter to quality or bugs; progress is counter/3 for qualities
and bugs and the full counter for speed. Progress is clipped to required points,
bugs to zero, and polishing quality to supplied original targets.

The work-start experience increment is 3.7, not the unused local tier values
1/3/10. Milestone bonuses 10/20/40 affect the triggering eligible employee,
not every employee. Full eligibility/scheduling remains to be integrated.

## The source+99 accuracy reference

The +643A addition is genuine. It must not be replaced with the employee's own
accuracy. Reset 009DD234 +076C..0921 clears employee arrays 0..256 inclusive;
+07CA..07D8 writes Empty to member B50. The supported initializer fills source
indices 0..59. Thus cells 99..158 used by fresh supported employees remain Empty,
which is numeric zero. Fresh sessions preserve this sparse projection in modern
saves. Legacy saves with no projection retain unknown cells.

The compiled public declaration at 0061A0C8 identifies member B50 and a
one-dimensional SAFEARRAY template of 513 16-byte VARIANT cells, lower bound 0.
This corroborates that these references are in bounds. Structure and numeric Empty
semantics are documented by Microsoft:
[SAFEARRAY](https://learn.microsoft.com/en-us/windows/win32/api/oaidl/ns-oaidl-safearray)
and [Variant](https://learn.microsoft.com/en-us/office/vba/language/reference/user-interface-help/variant-data-type).
This is a fresh supported reset finding, not general original-save compatibility.

The Python PE reader was also corrected: virtual zero-fill is not file-backed
data. Raw-size-zero sections cannot be mapped into the PE header, and reads may
not silently cross a section or truncate.

## Type and genre transitions

Stage 0 opens type selection at ≥50%; stage 1 opens genre selection at ≥81%;
stage 2 switches to debugging at full required progress. Type and genre selection
increase required points, so the denominator changes.

| Record member | Verified use |
| --- | --- |
| Type 1378 | Selection fee |
| Type 1308 | Added development points |
| Type 13B0 | Audience-affinity code |
| Type 13CC, fifteen columns | Genre-affinity identifiers |
| Genre 1224 | Selection fee |
| Genre 1208 | Added development points |
| Genre 1240 | Identifier compared with the type table |
| Genre 125C | Eligibility threshold against company member 90 |
| Audience 14A8 | Audience identifier 1..7 |

Type confirmation 009ACF14 +06C2..08B4 charges the selected source's fee,
but reads points through rosterMap[selectedSource], after list selection already
mapped the source. The API requires that original map. Unknown or stale tail
entries are not substituted with a direct type lookup. Recognition adds
floor(company recognition/3).

Type roster construction 009D2F68 +0184..05F3 scans all sources, accepting a
floor(Rnd×6)=0 draw when audience affinity matches. If fewer than five result,
fallback scans use floor(Rnd×10)=0 until five are present. The original fallback
permits a source when ANY existing entry differs, so duplicates can occur.
Cosmetic example-title RNG after this phase is not modeled.

Genre confirmation 009B00D8 +06C8..0921 charges the selected fee and adds its
points directly. If audience identifier>1 and its identifier appears in the
selected type's fifteen-column table, Fun receives +20. Member 90's wider company
meaning remains unresolved; eligibility takes it explicitly.

## Data size is separate from recognition

Helper 0099791C, thunk 0060EDBC, stores member 1990 (A129AC). Formatter 009855D4
renders that field with KB/MB, establishing data size. Recognition is member
1974 (A12990), shown with a percent suffix.

The size helper's gains are .03+floor(Rnd×8)/1000 for Fun/Novelty,
.08+floor(Rnd×16)/1000 for Graphics, .05+floor(Rnd×12)/1000 for Music,
.1+floor(Rnd×5)/1000 for Speed and .05+floor(Rnd×8)/1000 for Bugs.
Multipliers use hardware member EC8's identifier, not its array source:
1/4/8/10→1.1, 2→4, 3→2, 5→.6, 6/7→1.2, other→1.
The helper's special category 99 and its callers remain outside the typed ordinary
work API. Debugging does not call this ordinary gain path.

## Office routes

Module2 +0A7D..2358 maps fourteen route nodes; +2362..2BB1 supplies work targets
and facings for seventeen actions. +B327..BB89 chooses diagonal movement from
coordinate sum/difference lines. Non-diagonal destinations use isometric detours,
not straight-axis movement. +BB89..BD08 moves ±4 X, ±2 Y per tick.

Tests cover all 289 pairs of work destinations and nearby diagonal/axis targets.
Hiring continues to use its recovered entry/lounge state machine. Desk standing,
chair visibility, assignment, meetings and working animation are not yet integrated.

## Next integration work

Trace assignment and pending modal transitions; implement chair/standing state;
connect work cadence, stress, experience and project contributions to the original
clock; recover debug completion/polishing targets, release/reviews/sales; integrate
payroll, events and progression. Preserve uncertainties and compatibility quirks.
