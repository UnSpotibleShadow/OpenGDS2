# Employee initializer and M7 evidence

Evidence classification: **verified static data flow**, against extracted game SHA-256
`4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.
No original process was run. Runtime comparison and VB6 random-stream compatibility are still pending.

`Module1` initializer: CodeInfo `0x00A0274C`, P-Code start `0x009F651C`, size 49,712.
The executable supplied separately is an SFX wrapper and is intentionally rejected by the importer.

The employee helper thunk at `0x0060EE40` loads CodeInfo `0x00991B8C`;
its P-Code begins at `0x00991920`. Arguments at offsets `0x10..0x44` populate
parallel source arrays, then increment member `0x0A60`. The shared structure base
`0x00A1101C` is derived by correlating member offsets with the hiring form pool;
for example member `0x0AC4` maps to name global `0x00A11AE0`.

## Database shape

There are **60** helper calls in initializer offsets `+0x04A4..+0x2500`.
The first 35 have fixed names; at `+0x1624` the count is copied to member
`0x0A70`, the recruitment minimum (35). The next 14 use `-1` for generated names.
Eleven more have fixed names and a separate special flag. That separate flag is
not the roster gate and is retained structurally without inventing unlock rules.

The final helper count is 60 (exclusive upper bound). Original gate member
`0x0C84` is cleared to VB Empty by the reset routine, then becomes 1 in the hire
helper and 0 on dismissal. This confirms its role as an employed flag.

Eight given/family pairs are initialized by calls to thunk `0x0060ED50` at
`+0x0344..+0x049D`. Generated names independently sample the family and given
arrays, join with one ASCII space and reject names already in preceding rows.
Modern generation uses the same structure, with a bounded failure guard; it does
not reproduce the original VB6 random stream.

## Field mapping

| Helper argument | Member | Global | Meaning |
| --- | --- | --- | --- |
| 10 | 0C14 | A11C30 | Portrait reference |
| 14 | 0AC4 | A11AE0 | Name |
| 18 | 0AE0 | A11AFC | Fun |
| 1C | 0AFC | A11B18 | Novelty |
| 20 | 0B18 | A11B34 | Graphics |
| 24 | 0B34 | A11B50 | Music |
| 28 | 0B50 | A11B6C | Accuracy |
| 2C | 0B6C | A11B88 | Development speed |
| 30 | 0B88 | A11BA4 | Inspiration |
| 34 | 0D3C | A11D58 | Unresolved category/structure |
| 38 | 0BA4 | A11BC0 | Level |
| 3C | 0CE8 | A11D04 | Growth coefficient used by level-up |
| 40 | 0BDC | A11BF8 | Monetary parameter; payroll role not fully proven |
| 44 | 0BC0 | A11BDC | Contract fee |

The literal argument order is reversed relative to argument numbering. JSON
`RawAttributes` preserves the literal order: inspiration, speed, accuracy, music,
graphics, novelty, fun. Semantic properties are populated from that verified map.

### Why the stat labels are verified

The `fGMLevelUP` designer has caption indices 0 (fun), 4 (graphics), 1 (novelty),
3 (music), 2 (accuracy), 5 (speed), 6 (inspiration). Their Y coordinates match
value-label indices 7, 8, 9, 10, 11, 15, 16 respectively. The timer handler writes
source globals A11AFC, A11B34, A11B18, A11B50, A11B6C, A11B88, A11BA4 to those
same value labels. The evidence verifier checks the index and coordinate pairs.
This avoids guessing names from initializer argument ordering.

## Reproduction

```sh
python3 tools/recover_employee_database.py /local/extracted/GameDev2DX.exe --out /local/private/employees.json
python3 tools/verify_m7_evidence.py /local/extracted/GameDev2DX.exe
OPENGDS2_ORIGINAL_EXE=/local/extracted/GameDev2DX.exe python3 -m unittest discover -s tests -p 'test_*.py'
```

The Godot importer performs equivalent targeted extraction in C#, in memory,
and stores the JSON alongside assets in the user's cache. The executable is never
written into the project or cache. The optional C# parity test compares all rows
against the Python output. **The C# parity test passed for all 60 rows.**

## Progression formula

`fGMLevelUP.Timer1` begins at `0x009B375C`, size 2,940. Form load sets bonus
A145F4 to zero (`+0x0224`). Timer state zero evaluates `Int(Rnd * 10) = 0` and
sets bonus to 10 on success (`+0x007C`). Each of the seven stat branches then computes

```
gain = Int(Rnd * sourceGrowthCoefficient) + Int(sourceGrowthCoefficient / 3) + bonus
if current + gain >= 99: gain = 99 - current
current += gain
```

The first arithmetic branch is `+0x0341..+0x03DB`; the remaining six repeat it.
`EmployeeProgressionRules` implements this scalar behavior with supplied random
draws. The manual level-up handler, threshold table, cap and fee increase are
now traced in [employee-progression.md](employee-progression.md). Work/task
scheduling and other training systems remain unresolved.
Variant arithmetic rounding and original RNG sequences still need runtime fixtures.
