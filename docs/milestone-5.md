# Milestone 5 — recruitment algorithm and hire transaction

Milestone 5 moves the reconstructed **社員雇用** (Employee Hiring) screen from a UI-only shell to an
evidence-backed domain model. The original executable is still inspected **statically only**; no
original Windows code is executed.

The source employee database is not yet decoded, so this milestone deliberately stops short of
inventing applicant identities or statistics.

## Reproducible static trace

`tools/trace_hiring_pcode.py` parses the specific VB6 P-Code procedures used by the known official
GameDev2DX executable and verifies the M5 rule patterns. It checks the target SHA-256 before applying
build-specific offsets.

```bash
python tools/trace_hiring_pcode.py /path/to/extracted/GameDev2DX.exe
python tools/trace_hiring_pcode.py /path/to/extracted/GameDev2DX.exe --dump hire
python tools/trace_hiring_pcode.py /path/to/extracted/GameDev2DX.exe --dump load
```

The verified inner executable SHA-256 is:

```text
4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6
```

The small opcode subset is based on the public Visual Basic P-Code table:
https://www.vb-decompiler.org/vb_pcode_table.htm

The MSVBVM60 export table identifies ordinal 593 as `rtcRandomNext`, matching the random-call thunk
used by these procedures:
https://strontic.github.io/xcyclopedia/library/msvbvm60.dll-9618E4752B19CE24EFD729C662C8DB1E.html

## Applicant roster generation

The `fGMSKoyou` initialization procedure begins at `0x009BABFC` and is 3508 bytes in this build.
The relevant sequence is now structurally decoded:

1. roster count is initialized to zero;
2. source records are scanned;
3. a source gate/flag is compared with zero;
4. the game evaluates `Int(Rnd * 4) == 0`;
5. when both tests pass, the source-record index is written into the applicant-roster array;
6. roster count is incremented;
7. the scan repeats until **5 applicant slots** are filled.

The exact semantic name of the source gate/flag is not yet known. OpenGDS2 therefore describes it as
a source gate/flag rather than claiming it represents a specific employment condition.

This gives us two safe constants in `RecruitmentRules`:

```text
ApplicantSlotCount = 5
SelectionAcceptanceDenominator = 4
```

## Affordability check

The Hire handler starts at `0x0099FFC8` and is 1480 bytes. Its first decision loads:

- constant-pool entry 0: company funds;
- constant-pool entry 1: selected applicant index;
- constant-pool entry 2: contract-fee array;
- the selected fee from that array;
- then performs a less-than comparison.

If company funds are lower than the selected fee, the handler follows the failure branch, displays
the original insufficient-funds message, and exits. The success path is reached only when the
comparison is false.

`HiringService.TryHire` now mirrors this evidence-backed boundary: unknown funds/fees are not treated
as zero, insufficient funds fail, and a verified successful transaction deducts the fee.

## Employee copy and fee charge

On success, the Hire handler calls a Module1 helper at `0x0098735C` (328 bytes). The helper:

- scans employee slots for a record whose status value is `-1`;
- initializes the first vacant slot;
- copies fields from the selected source record into that employee record;
- loads the selected monetary field, negates it, and passes it to a shared financial helper.

The financial helper at `0x0097DD04` (88 bytes) adds its argument to four related financial state
fields using repeated `AddVar` / `FMemStVar` operations. Combined with the independently recovered
hiring-form label and affordability comparison, the selected monetary value is the contract fee and
the negative argument implements the charge.

## Random new-hire greeting

After the successful copy helper returns, the Hire handler calls the same VB6 random runtime entry,
multiplies the result by 10, applies `Int`, and explicitly compares against cases **0 through 9**.
That is a ten-way randomized greeting dispatch.

OpenGDS2 records this behavior without bundling the original greeting text in the public project.

## Runtime behavior in M5

The Godot recruitment screen now explains the recovered roster and transaction rules. The domain
layer contains the real affordability/deduction/copy behavior, but the **Hire** button remains
disabled because the source employee database is still unresolved. This is intentional: a working
button backed by fabricated applicants would be less faithful than a visible reconstruction boundary.

## Generated evidence

`generated/vb6/hiring_m5_evidence.txt` is a compact offset/rule report suitable for code review. It
contains no original executable and no bulk extracted text.

## Next target

Recover the source employee database and its gate/flag semantics, including:

1. candidate names;
2. each PC-era employee stat field;
3. contract fee;
4. source-record count and indexing;
5. the meaning of the roster gate/flag;
6. whether any mode/company-state filters alter the available source pool.

Once those are identified, the existing `HiringService` can be connected to real candidates and M6
can make **Hire** a fully usable management action.
