# M8: contracts and audience reconstruction

This is a tested source checkpoint, not a gameplay-complete release. M6 remains
the supplied baseline. No original program or imported catalog data is bundled.

## Verified contract schedule

Module1 procedure 61 starts at `009DB4F4`. It increments the week at `+01AE`.
At `+04CB..+04EF` it tests month = 3 AND week = 5, then opens the renewal modal
through pool 92 (`A15424`) at `+055C`. The month rollover follows at `+067D`.
Thus March week 4 ends with renewal for the next year; week 5 is temporary
internal state, not a fifth playable week. The associated message identifies
March-end renewal for the coming year. No hire-date-based contract term is
established by this evidence.

The renewal form load (`009BF5A0`, `+025A..+02B2`) scans the source database in
ascending index order, selecting records whose employed gate `A11CA0` equals 1.
Its Command1 handler (`00995CDC`) sums checked-source contract fees, rejects
insufficient funds at `+0079` before mutation, calls the dismissal helper for
unchecked entries, and charges the total once at `+033E..+0354`. No level,
experience, fee or contract-duration reset is present in this handler.

Dismissal helper `009981CC` takes a workspace slot, resolves its employee source
through member E1C, then clears source gate C84 at `+0399` and stress C68 at
`+03B5`. It also performs workspace/task cleanup. M15 reconstructs the currently supported
desk/lounge/walking projections; training and external jobs remain unresolved.
The new ContractRenewalService implements only the current domain's employee
and financial projection. It is deliberately not exposed as a free office
action or connected to the calendar inspector. Scheduling, pending-modal state
and cleanup during active work must be integrated before enabling real ticks.
Modern validation additionally rejects stale identities and overflowing totals
before mutation; these are save/data integrity guards, not original formulas.

## Audience catalog

Seven direct setter calls occur in initializer `009F651C`, offsets
`+9B3E + index * 52`, with call sites `+9B73 + index * 52` (hex offsets).
Helper `00981DCC` writes arguments 10,14,18,1C,20,24,28 to members
14A8,1400,141C,1438,1454,148C,1470 respectively. Arguments are pushed in
reverse order: the first argument is the numeric record identifier; the last
is the description. The importer retains names/descriptions locally and labels
the five numeric columns structurally. Independent C#/Python extraction agrees
on every field. Reimport older local caches to populate the new audience list.
Repeated original descriptions are preserved rather than silently corrected.
Demand, bonus-point and monetary-field semantics remain unresolved.

## Payroll investigation: unresolved original defect

The month-rollover block `009DB4F4 +06B7..+0770` iterates source indices and
tests each employed gate. For every employed entry, however, the sum loads
member BDC at the index stored in member 3018 (`+06F6`), rather than the loop
index. BDC is the separate monetary parameter imported as RawParameter40,
not the BC0 contract fee. Member 3018 corresponds to global `A14034`.

Targeted cross-reference tracing finds that member 3018 is used as a scratch
count: Module1 procedures 62 (`009933DC`) and 63 (`009930BC`) reset it to 0,
increment it while collecting employed sources, then use it to select a random
entry. Module2 periodic procedure 12 (`009E3830 +1AA7..+1B21`) similarly builds
an employed-source list using that count. Other employee UI paths also use it.
There is no assignment to 3018 inside procedure 61 before the payroll read.

Static evidence therefore suggests state-dependent payroll using a stale count
as a source index. This is **not dynamically verified** and is not implemented
as normal per-employee payroll or as an emulated bug. A Windows runtime/save
fixture is needed to establish actual values and effects. Do not substitute
the apparent intended sum while claiming original compatibility.

## Validation

24 C# tests and 12 Python checks pass, including local original fixtures.
All Godot C# sources and source generators compile without warnings/errors;
Godot 4.4.1 .NET headless import/hire/save/load UI checks pass. The host workaround
and standard SDK/graphical verification limitations remain in validation.md.
