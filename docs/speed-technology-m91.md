# M91: Speed technology and the retained development frame

Implemented 2026-10-12 from the supported original executable, M76's runtime
premise and further bounded tracing. This is static-source fidelity; native
Windows execution and whole-session random-sequence equivalence remain pending.

## Recovered behavior

`fGMInfo 009A98E8 +076B/+079E/+07D1` gives identifier 4 a separate 50/100/150
actor-pass timer, according to learned level. It charges the selected employee's
experience normally. `+07DF` jumps over the ordinary remaining-unit assignment;
`+082A/+083D` still replaces the shared active flag and selected technology index.
The raw unit table is not required by this branch or by immediate stress relief.

In `Module2 00A0283C`, the actor loop visits slots in increasing order
(`+0A52/+CE64`). While the room is before debugging, `+5656` substitutes 10000
for the idle-counter comparison. `+565F` skips both stores to frame local -04A4,
whose retained value is tested at `+56C3`. A non-speed work pass assigns that
local 0 when no technology is active, or 1 when one is active. Debugging bypasses
these assignments and requires the technology flag to be clear. Thus speed
shortens ordinary work-start waits only when retained eligibility is 0; it does
not accelerate debugging or modify the employee's development-speed stat.

The documented M76 VB6 Empty/numeric-zero premise establishes the first frame
value as 0. It persists between actor passes, loop iterations and company resets
because none of those paths assign it. The port orders employee processing by
`Office.SlotIndex`, including slots reused after departures. Modern saves preserve
the retained value; older modern saves leave it unknown until an ordinary
non-speed work pass establishes it. Speed use is guarded before charging while
it is unknown. This is a modern recovery boundary, not an original-save importer.

`+5E31..5E67` can initialize tier 4 if the active flag is set and work ticks are
zero. `+66AC..66D8` excludes speed from the ordinary technology consumer. The
common work-tick increment still runs. Consequently, replacing an in-progress
technology, or entering with retained eligibility 1, can leave tier 4 stalled.
Timer expiry does not reset that tier or its ticks. This native defect is retained;
the use screen warns before replacing in-progress work. Changing the employee's
assignment resets the stalled work through the existing source-backed route.

`+AF91..B01B` lies outside the development-seat/room branch. Each active speed
actor pass requests speech bubble 4, decrements the separate timer and, at zero,
clears only the active flag and timer. Travel and rest consume it; paused/modal
office simulation does not. Empty-room cleanup clears the shared application
flag/units but leaves the unused speed timer. Selecting another technology also
leaves that timer unused; reactivating speed replaces it with the full duration.
The ordinary burst counter shares the source unit cell, so starts and increments
update the speed application's retained unit projection. Completion resets work
ticks but retains that cell; the next ordinary start clears it.
The same cell also matters when a generic technology is chosen during a numbered
burst: its unit assignment replaces the accumulated number, and subsequent burst
increments update the remaining application units. Both projections now use the
saved `SharedWorkUnits` cell. Older modern saves initialize it from the active
application or burst projection on the next known read/write. This also preserves
unused units after speed expiry and a later reactivation.

The UI shows the remaining duration in office-simulation seconds (40 ms per
actor pass), rather than presenting the unused generic units as progress. It
explains the debugging limit and replacement behavior in English and Japanese.

## Evidence and validation

`tools/verify_speed_technology_evidence.py` verifies the supported executable
identity, two procedure fingerprints and 32 bounded byte anchors. The manifest
contains numeric/structural evidence only. Original files, names, dialogue and
full traces remain private. See M76 for the Empty-frame initialization premise.

Seventeen new domain checks cover all grades, experience, retained shared units,
unknown-frame recovery, accelerated ordinary starts, retained eligibility 1,
interrupted-application stalls, debugging, travel, modal pause, replacement,
empty-room cleanup, burst contributions, reused-slot ordering, save/load, new
company continuation and malformed-state rejection. The optional-original suite
passes 476 checks. Clean application build and focused English/Japanese UI
checks pass, including timer/cost, live countdown, reload, interrupted-state
warning/retention and unknown-frame guard. The UI fixture uses a complete synthetic
hardware definition and compares against the paused state after load's normal
first office pass; no catalog or gameplay validators were weakened.
The broader development/release/sales/numbered-burst UI regression also passes.
Its English fixture now explicitly resets language after bilingual checks and
sets the shared native unit cell when injecting a synthetic numbered burst.

The earlier M90 testing ZIP is unchanged and does not contain this milestone.
Payroll scratch writers, automatic conversations, original-save import and
whole-session native RNG integration remain separate work.
