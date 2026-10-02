# Milestone 6 — recruitment source-array map

M6 moves the applicant generator from a prose reconstruction into reusable domain code and resolves
how `fGMSKoyou` addresses the original VB6 recruitment database. Inspection remains static-only.

## Newly resolved globals

`tools/map_recruitment_arrays.py` resolves the hiring form's constant pool to the global storage used
by the roster loop and applicant display. In the verified executable the important entries are:

- `pool[31]`: source upper bound;
- `pool[32]`: minimum source index;
- `pool[33]`: per-source structural gate array;
- `pool[34]`: five-slot roster of selected source indices;
- `pool[0D]`: employee-name array;
- `pool[36]`: employee-level array;
- `pool[37..3B]`: five applicant stat arrays displayed by this form;
- `pool[02]`: contract-fee array.

The PE image contains zero-filled backing storage for most of these globals. Their authoritative
contents are populated during game initialization, so reading the raw PE addresses as if they were
the database would be incorrect.

## Recovered roster loop

The form initialization scans source indices from zero to `source_upper_bound - 1`, skips indices
below `minimum_source_index`, requires the structural gate value to equal zero, and then applies the
already-recovered `Int(Rnd * 4) == 0` test. Passing source indices are appended to the roster. Full
passes repeat until five slots have been filled.

The wider semantic meaning of the gate and bounds is not named yet. We know what they do in this
procedure, not yet what every other subsystem means by them.

## Modern implementation

`RecruitmentGenerator.BuildRoster` now implements the recovered loop against explicit
`RecruitmentSourceRecord` rows. This separates two concerns:

1. **algorithm** — now reconstructed and implemented;
2. **database initialization** — still being decoded.

When the startup initializer is recovered, it can populate source records without changing the
recruitment algorithm or UI.

## External corroboration

Historical player documentation says reopening the hiring window can produce different applicants,
which is consistent with the recovered rebuild-on-open random roster loop. Old save-editing notes
also describe employee names followed by multiple employee stats and contract fee in save data.
These sources are corroboration only; OpenGDS2 does not treat community tables as authoritative
binary data.

## Next target

Trace writes to the global employee arrays during new-game/startup initialization. The first useful
checkpoint is to recover source count/bounds plus one complete source row (name, level, five displayed
stats, contract fee and gate). Once the initializer is understood, expand to the full database and
connect it to `RecruitmentGenerator`.
