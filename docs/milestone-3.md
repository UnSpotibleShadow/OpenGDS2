# Milestone 3 — P-Code map and first office state

Milestone 3 moves OpenGDS2 from serialized-form recovery into executable behavior mapping.
No original executable is executed by the tooling.

## Major binary finding: this is a P-Code build

`tools/inspect_vb6.py` follows the compiled VB metadata graph:

`VBHeader -> ProjectInfo -> ObjectTable -> ObjectInfo -> controls/events/procedures`

For the supplied official executable it recovers:

- VB runtime signature: `VB5!`
- language runtime: `VB6JP.DLL`
- `VBHeader`: `0x005E9AD4`
- `ProjectInfo`: `0x005E9C58`
- `ObjectTable`: `0x00616D2C`
- object array: `0x00616D80`
- compiled objects: **86**
- compile mode: **P-Code** (`ProjectInfo.aNativeCode == 0`)

The structure layout and P-Code procedure relationship are cross-checked against the public
Semi-VB-Decompiler research/source:

- https://github.com/VBGAMER45/Semi-VB-Decompiler/blob/master/Semi%20VB%20Decompiler/modGlobals.bas
- https://github.com/VBGAMER45/Semi-VB-Decompiler/blob/master/Semi%20VB%20Decompiler/modPCode.bas
- https://github.com/VBGAMER45/Semi-VB-Decompiler/blob/master/Semi%20VB%20Decompiler/opcodes.txt

Generated output is under `generated/vb6/`.

## Startup event map

### `fGMGameNew`

Five event procedures are statically recoverable. The important startup controls include:

- `Command1`, event slot 0 -> CodeInfo `0x0099AB00`
- `Command2`, event slot 0 -> CodeInfo `0x0097AFEC`
- `Text1`, event slot 0 -> CodeInfo `0x0097AFA0`
- Form event slots 6 and 8 -> CodeInfo `0x00988180` and `0x00978164`

### `fGMGameKaishi`

- indexed `Command1`, event slot 0 -> CodeInfo `0x00988820`
  - P-Code starts at `0x009886C4`
  - size: 348 bytes
- Form event slot 6 -> CodeInfo `0x00985D94`
  - P-Code starts at `0x00985C6C`
  - size: 296 bytes

`tools/scan_startup_pcode.py` recognizes a small high-confidence opcode sequence in the
`Command1` handler. It finds:

- `0x009886D9`: indexed argument equals `0`; false branch -> `0x00988703`
- `0x00988703`: indexed argument equals `1`; false branch -> `0x0098881C`

The first false branch lands exactly on the second comparison. The second false branch lands at
the procedure epilogue. This is consistent with the three serialized choices
Beginner / Normal / Cancel and confirms that they are an indexed button array.

This is deliberately a narrow structural interpretation. We have **not** yet claimed what every
P-Code call in those branches means.

## First office-state behavior

Two different evidence streams now line up:

1. The original binary contains the tutorial heading `社員を雇用` (Hire Employees) and the text:
   `社員を雇用しなくては始まりません。まずは新規雇用しましょう、会社が発展すれば優秀な社員を引き抜くこともできます。`
2. A contemporary Vector review of the PC release describes Beginner mode as showing tutorial
   guidance, giving more starting money than Normal mode, and limiting play to three in-game years.
   It also describes the opening company as having a secretary and one employee, with hiring as the
   first task.

Reference:
https://www.vector.co.jp/magazine/softnews/040630/n0406303.html

OpenGDS2 therefore now models the following without inventing exact numbers:

- Beginner: tutorial enabled
- Beginner: 3-year time limit
- Beginner: more starting funds than Normal (exact amounts **unknown**)
- opening staff: secretary + one employee (identity/stats still unresolved)
- first documented objective: hire employees
- verified office labels: Funds, Company Rank, Company Fame

## Important modeling change

Unknown original numeric values are now nullable in the domain model. In particular, starting
money, date, rank, fame and development power no longer silently default to zero. The M3 UI displays
`Not yet decoded` for these fields.

This protects the clean-room reconstruction from accidentally turning missing evidence into rules.

## Next target

Milestone 4 should decode the first hiring path and identify the original initial employee and
candidate-generation rules. In parallel, extend the P-Code decoder enough to follow the mode branch
into the common new-game initialization procedure and recover exact starting cash/date values.
