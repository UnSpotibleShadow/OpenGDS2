# M59 office burst flames

Ordinary tier-three work now displays the locally imported two-frame flame at
its development desk, alongside the existing contribution particles and counter.
The twelve desk tiles retain independent animation phases across employees,
reassignment, inactive intervals and modern save/load. Rendering itself does not
advance a phase or consume random draws.

## Source evidence

Module2 `00A0283C +2E98..3269` requires work tier 3 and a seated action at least
6. The supported development seats are 6–17. +2EE2..30DD supplies twelve
X/Y/source-row triples. +312F/+3147 set a 25 by 25 rectangle; +315F stores
Y + 38 as its drawing depth. +3185 tests the seat counter against 3 times the
established timing factor 2 and selects source X 319. +31C2 tests at least
6 times the factor and selects X 346, resets the counter, then +3210 increments
it. Other work hides the tile at X −100 (+3252). The counter belongs to seated
action through global A132A4, rather than an employee's work or burst counter.

The initializer's final mappings at `009F651C +398B..3D27` assign tiles 64–75
to seats 6–17 through member 226C. Earlier temporary mappings are overwritten;
they are not rendered as extra flames. The projection uses each imported tile
and the recovered source rows 359/384. No source graphics are bundled.
`tools/verify_burst_flame_evidence.py` checks the supported executable hash,
positions, dimensions, depth, phase branches and final initializer mappings
without emitting pixels or raw traces.

## Persistence and boundaries

`OfficeBurstFlames` is a modern presentation snapshot containing twelve counters
and source columns. Saves validate lengths, counters 0–12 and columns 319/346.
Older modern saves start a fresh visual phase; this does not claim original
save compatibility or reconstruct a lost historical clock. Reassignment and
release hide inactive tiles while preserving the seat phase. Existing modal
transaction and pause guards suspend ticking. Development gains, fees, stress
and random draws are unchanged. Study-seat flames and unsupported external or
polishing workflows remain outside this bounded development-seat projection.

## Validation

Direct app compilation and 293 domain checks pass, covering frame boundaries,
all twelve positions, seat independence, inactive phase retention, reload and
malformed snapshots. The full fixed-60-FPS synthetic Godot core loop passes.
The original-data UI fixture compares both flame frames with the same loaded
office: each changes pixels only within its 25 by 25 desk rectangle, the frames
differ, reload preserves phase and leaving burst work restores the baseline.
Two corrupt local audio resources were restored from digest-verified packaged
entries before this fixture rerun; source audio and remote asset blobs are not
changed. No debug package, native Windows comparison or Actions run is created.
