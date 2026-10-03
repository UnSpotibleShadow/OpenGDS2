# M15: employee dismissal and supported contract cleanup

Employee management now supports dismissal, with a confirmation screen and
English/Japanese controls. Cancelling retains the employee. Confirming frees the
worker's current/destination desk or lounge reservation, stops their pending work,
clears the source employed gate and stress and retains source progression. There
is no severance charge. Active projects remain in their rooms and can receive a
replacement worker. Saves contain no dismissed workspace. Annual nonrenewal uses
the same cleanup; the annual scheduler is still unimplemented.

## Verified static behavior

Module1 009981CC takes an employed workspace slot. Members 2738/2754 hold current
and requested actions; the helper clears work/animation fields, hides associated
sprite rows, restores seat availability and releases lounge occupancy. Supported
modern workspaces express those reservations through the employed employee list;
removing the workspace therefore releases both a walking destination and a
currently occupied seat without renumbering other employees.

| Source offsets | Recovered effect |
| --- | --- |
| +00C3..01C2 | Clear task, animation and workspace fields |
| +01D2..0388 | Release lounge occupancy or work-seat availability for current/requested actions |
| +0389..0398 | Hide employee sprite |
| +0399..03B4 | Resolve source via E1C[slot] and clear source C84 employed gate |
| +03B5..03D0 | Clear source C68 stress |
| +03D1..0409 | Clear current action, destination action and lounge seat |

fGMInfo 009A3E34 +0525, renewal 00995CDC +0134, external contract
00992A98 +0273, poaching 009B7570 +0AD1 and main loop 00A0283C +B0ED call
this helper. Only dismissal and renewal's already recovered finance projection
are connected here. Training/external-project task cleanup still needs its own
models. The original retains source stats/level/experience; modern work experience
is synchronized to the source projection before dismissal. Guards reject duplicate
identities/data before mutation, and require an original identity. These guards
protect modern state and do not claim to reproduce original corruption handling.

## Payroll and recall arithmetic

OriginalPayrollRules now tests the exact month-rollover arithmetic: for each
employed gate, read BDC at shared scratch member 3018 and add that value. This is
not an intended per-employee wage sum. An unknown scratch index or missing salary
record remains unknown. No employed gates yields zero without needing the index.
This rule is not scheduled; live scratch-state correspondence remains unresolved.

An executable-wide scan located ten direct scratch writes, in five procedures:
Module1 009933DC and 009930BC; staff load 009B0B60; technology selection 0099F9A4;
and periodic events 009E3830. Staff load +004C..00D6 resets and counts employed
source gates. Message/event paths can call the other collectors. The scan does
not establish dynamic execution order or exclude indirect/native writes, so it
is evidence for the next investigation rather than permission to infer payroll.

RecallRules tests the traced eligibility, non-short-circuit random draws, signed
ranges, five-week countdown and three refund/dismissal outcomes documented in
reviews-and-events.md. It remains separate from scheduling, product flags and
money mutations until the weekly pipeline is recovered. In particular, a zero
countdown does not trigger an outcome unless decremented from a positive value.

## Validation

85 local C# checks pass, including original import parity and a complete imported
core cycle; 83 are fixture-free. Checks cover dismissal from seated/walking desks
and lounge seats, rehire/slot reuse, unchanged funds, retained progression,
reservation release, invalid identity atomicity, annual nonrenewal cleanup,
scratch-index payroll and all recall brackets/refund outcomes. The Godot core UI
suite exercises dismissal confirmation, cancellation, successful removal and save
persistence after the complete development/release/sales/milestone loop. Original
runtime comparison and full-game calendar integration remain outstanding.
