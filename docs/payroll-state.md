# M22: staff salaries and original payroll scratch

This milestone adds a staff-information projection. It does **not** enable monthly
charges or advance the company date. The original Windows runtime has not verified
this apparent payroll defect.

## Verified static data flow

The monthly block `009DB4F4 +06B7..0770` counts employed source gates
`A11CA0` and repeatedly loads `A11BDC` using shared scratch `A14034`
(member `3018`). It does not use each employed source's own index.
The existing bounded evidence check covers the payroll read at `+06F6`.

The staff-form procedure `009B0B60` clears that scratch at `+0051`,
stores forward/reverse employed-source mappings at `+008F/+009C`, and
increments the scratch at `+00AA`. Its salary display accumulates each
employed source's own BDC amount at `+00BF`. Consequently the displayed
salary total and the monthly debit need not agree.

`EmployeeState.MonthlySalary` exposes the existing imported RawParameter40
without duplicating it in saves. No salary table or original name is bundled.

## Implemented projection

Entering **Staff** captures the employed-source count when the modern source
table contains every index from zero through SourceUpperBound minus one.
A partial, duplicate or unbounded table leaves the scratch unknown.
This completeness requirement is a modern guard, not an original formula.

The staff screen shows individual salaries, their total and a separately labeled
original-payroll preview. It changes no funds. Opening Staff again rebuilds the
scratch; gate changes alone do not silently recompute it. Save/load retains the
snapshot independently of the current employed count. Legacy saves have null.

For example, with salaries 700, 100 and 300 in rows 0, 1 and 2, and only rows
1 and 2 employed, the displayed total is 400. Staff entry leaves scratch = 2;
the original monthly formula therefore projects 2 × 300 = 600.

Technology-selection procedure `0099F9A4` also writes this scratch. The current
technology screens conservatively invalidate the projection because that writer's
complete lifecycle is not reconstructed. This null assignment is a modern
uncertainty guard; it is not a claimed original numeric write. The known Module1
`009933DC/009930BC` and periodic `009E3830 +1AAC/+1AF7` writers are still
unconnected. They must be reconstructed before treating this as complete global
scratch emulation.

## Unresolved

- A startup/reset value for 3018 has not been proven; fresh sessions do not set zero.
- Original runtime values and the apparent stale-count payroll defect.
- Remaining scratch writers, source-slot lifecycle and original save restoration.
- March-end renewal ordering, monthly debit, bankruptcy, complete weekly events
  and hardware progression in a live calendar.

Signed values are retained by the pure arithmetic; checked modern arithmetic
rejects overflow. The UI reports an unavailable preview if monetary projection
overflows. Those are data-integrity protections, not a replacement salary rule.

## Validation

Five new public C# checks cover staff capture, different display/debit totals,
stale counts, conservative invalidation, incomplete tables, unknown salary rows,
out-of-table counts, signed values, overflow and save/load. The Godot core-loop
fixture checks the actual labels and saved scratch without charging money.
M22 is authored through GitHub while the local execution workspace is disconnected;
original-fixture and local runtime reruns remain pending.
