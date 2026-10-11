# Staff salaries and original payroll scratch — M80 screen-state fix

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

The staff screen shows individual salaries and their total. The separate calendar
inspector contains the original-payroll preview, keeping reconstruction details
out of the staff-management screen. It changes no funds. Opening Staff again rebuilds the
scratch; gate changes alone do not silently recompute it. Save/load retains the
snapshot independently of the current employed count. Legacy saves have null.

For example, with salaries 700, 100 and 300 in rows 0, 1 and 2, and only rows
1 and 2 employed, the displayed total is 400. Staff entry leaves scratch = 2;
the original monthly formula therefore projects 2 × 300 = 600.

The technology-capacity facility selector `fGMTenantTechSel 0099F9A4`
also rebuilds the employed count (`+02B9/+0312`). M70 already connects that
writer through `FacilityService.OpenTechnologyRoster`, including opening and
cancelling the selection without buying study.

M80 removes the three earlier uncertainty resets from the ordinary technology
purchase, employee training and technology-use screens. They were incorrectly
associated with that facility selector. Simply visiting these screens previously
set a valid payroll snapshot to null, potentially stopping month-end calendar
advancement until Staff was reopened. These screens now preserve the snapshot;
they do not recalculate it or supply an invented zero. Actual staff-list and
capacity-selector writers remain active. This is a source change after RC.4;
the existing RC.4 ZIP does not contain this fix.

M76 maps the Module1 `009933DC/009930BC` random-employee portrait (-1) writers
and periodic `009E3830` employee selection. Those paths remain unconnected;
secretary (-2) dialogue bypasses the random-employee writer. See
[workflow research](workflow-verification-m76.md#shared-payroll-scratch-writer-lifecycle-and-order).
Modern save/load retains the snapshot for continuity; no original save/load/reset
reference to member 3018 was found.

## Unresolved

- A startup/reset value for 3018 has not been proven; fresh sessions do not set zero.
- Original runtime values and the apparent stale-count payroll defect.
- Integration of the recovered random-employee and periodic scratch writers; source-slot runtime comparison.
- Bankruptcy and remaining original economy/runtime fidelity. M35 already connects monthly debit and renewal ordering.

Signed values are retained by the pure arithmetic; checked modern arithmetic
rejects overflow. The inspector reports an unavailable preview if monetary projection
overflows. Those are data-integrity protections, not a replacement salary rule.

## Validation

Five new public C# checks cover staff capture, different display/debit totals,
stale counts, conservative invalidation, incomplete tables, unknown salary rows,
out-of-table counts, signed values, overflow and save/load. The Godot core-loop
fixture checks the actual labels and saved scratch without charging money.
M22 is authored through GitHub while the local execution workspace is disconnected;
original-fixture and local runtime reruns remain pending.

## M80 validation

A synthetic regression exercises purchase, training and application, saves/reloads
the preserved snapshot, then crosses month end and verifies exactly one payroll
charge. The recovered facility roster writer and existing unknown-data guards
remain covered by the domain suite. Native Windows UI testing is not claimed.

Validation on 2026-10-11: all 335 domain tests pass; the Godot/.NET app
build succeeds with zero warnings/errors; M76 evidence verification passes
44 procedure fingerprints and 35 payout entries. The domain suite retains one
existing nullable warning in an unrelated calendar-dialogue test.
