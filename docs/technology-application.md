# Technology application — M18

Evidence is static P-Code for the supported executable hash. The own-game mode-1
branch is connected; original Windows runtime comparison remains outstanding.
All technology names, prices and tables still come from the user's local import.

## Acceptance and scope

Employee use handler `009A98E8 +0000..0385` requires a seated development desk
(6–17), a nonempty project room, sufficient experience from table member 1568,
and no pending project decision. Selecting a learned technology does not require
company ownership; special recruits can use their prelearned technologies.

Accepted use subtracts experience up front. Stress relief acts immediately.
Other supported technologies copy table 158C's learned-level cell to the shared
workspace counter and set an active-application flag. A replacement application
spends experience again. Seat reassignment preserves the application, and work
pauses until the employee returns to a development desk. The effect targets the
room of the employee's current seated desk, matching the original workspace lookup.

The six project application counters are reset by `0099BA80 +02AC..02FE`.
New plans initialize them to zero. Older modern saves leave them unknown and
cannot silently assume first-use bonuses.

## Work cadence

Main loop `00A0283C +5E05..5E68` starts technology tier 4 when the shared work
counter is zero. An existing ordinary contribution can finish before technology
work begins. Ordinary new contributions are suppressed during supported timed
applications; ordinary animation, stress and break logic continue.

At shared work tick 20 (`+6A8A..6AC4`), the application counter decreases by one.
The work counter resets to 1 and then receives the common end-of-block increment.
The first unit takes 20 ticks and subsequent units take 19 ticks. No ordinary
contribution or invented experience reward is produced by technology work.

At zero remaining units, the effect applies, its project counter increments,
and the active flag/shared work counter clear (`+881D..88AC`). When a seated
worker's room has no project, `+4886..48EA` cancels the pending application.
Dismissal removes the workspace. Release and subsequent room processing cannot
carry a canceled application into another project.

## Effect formulas

Each completion draws a percentage. The base percentage depends on learned level
and the number of previous applications of that technology to this project:

| Previous applications | Level 1 | Level 2 | Level 3 |
| --- | --- | --- | --- |
| 0 | 20 | 30 | 40 |
| 1 | 5 | 10 | 20 |
| 2 or more | 0 | 0 | 0 |

Add `floor(Rnd × width)` to that base:

| Technology identifier | Effect | Width at levels 1 / 2 / 3 | Main-loop branch |
| --- | --- | --- | --- |
| 0 | Reduce data size | 3 / 3 / 3 | +6CB9..70E9 |
| 1 | Increase graphics | 3 / 5 / 7 | +70EC..75AC |
| 2 | Reduce bugs | 3 / 3 / 3 | +75AF..79C2 |
| 3 | Increase music | 3 / 3 / 3 | +79C6..7E86 |
| 6 | Increase fun | 3 / 5 / 7 | +7E9A..835A |
| 7 | Increase originality | 3 / 5 / 7 | +835D..881C |

Increases use `floor(value + value × percentage/100)`; reductions use
`floor(value − value × percentage/100)`. Compression additionally clamps data
size to zero. A zero starting quality stays zero, and even a zero percentage
still consumes the application and increments its project counter.

Immediate stress relief (identifier 5) uses
`floor(stress − stress × 0.5)` at level 1 and
`floor(stress − stress × 0.8)` at level 2 (`009A98E8 +039F..0627`).

## Explicitly unresolved

Speed boost (identifier 4) and stress relief level 3 remain disabled. M30 extends
the bounded speed trace: the use handler writes 50/100/150 for learned levels
1/2/3 to workspace member 2A48 / global A13A64, at offsets +076B/+079E/+07D1.
The main-loop block +AF91..B01C requires active-application A139A0 = 1 and
technology identifier 4 through A13A48 → definition A12638. It sets work tier
A14530 to 4, subtracts one from A13A64, and clears both the active flag and duration
when the result is <= 0. This establishes an eligible-pass countdown, not a
speed multiplier or a wall-clock duration. Original save/load/reset procedures
also reference this cell; their complete layouts remain unresolved.

The source-index/scratch mapping and interaction of tier 4 with ordinary work,
shared work counters, expiry and cancellation still need tracing. The public
bounded verifier checks the duration writes, pool identities, identifier guard,
decrement and expiry stores. No guessed multiplier or live speed effect is enabled.
The level-three
stress branch uses local 4CFD/workspace-source mapping instead of the selected
employee index. No assignment to that local was found in this handler; its actual
runtime target must be established before enabling it.

Mode-2 polishing sets different completion flags and is not connected here.
Original effect bubbles, timers, completion messages, graphical usability and
Windows behavioral comparison remain unfinished. No permanent skill bonuses,
cash rewards, sales multipliers or guessed speed effects have been added.
