# Campaign progression investigation — U01

Scope: determine the original bankruptcy trigger, its position in the weekly/monthly sequence, the resulting company/employee/project transitions, and office progression conditions before implementing them. Normal-mode negative funds in OpenGDS2 remain unresolved; no automatic game-over rule is inferred from that observation.

## Current source evidence

The supported extracted executable has SHA-256 `4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`. A private decode of Module2 ordinal 12 at `009E3830`, length 10216 bytes, produced 3115 instructions with zero trailing padding. Decoder coverage alone does not establish the meaning of every instruction or member.

The known shared payroll scratch variable at `00A14034` is also written in this periodic routine. At `+1AAC` it is reset to zero. A loop tests an employee-indexed array entry for equality to one, stores each matching employee index into another array, and increments the scratch at `+1AF7`. This is an index-list count in that block, not evidence of a company-funds threshold. Its later payroll use still requires lifecycle analysis; see [weekly economics](weekly-economics-investigation.md).

A complete bankruptcy condition, branch target and terminal/reset sequence has not been recovered. Office upgrade price, prerequisites and capacity/layout effects are likewise unresolved. Full traces and extracted original text remain private and are not included here.

## Implementation gate

1. Trace the funds comparison and all guards to their actual weekly/monthly caller, including mode and calendar conditions.
2. Identify terminal, reset or rescue effects and their order, including pending modal events and negative funds caused by annual transactions.
3. Trace office purchase handlers, affordability checks, prerequisites and resulting state changes separately.
4. Add narrow, hash-guarded original evidence fixtures and domain regressions for the established cases, including save/reload and one-time effects.
5. Implement only established behavior, then run the required .NET/Godot checks and add native Windows scenarios to the acceptance record.

This checkpoint closes neither U01 nor the unresolved shared-scratch decision. The next investigation should start from callers and company-funds reads, rather than treating the periodic scratch writers as a bankruptcy path.
