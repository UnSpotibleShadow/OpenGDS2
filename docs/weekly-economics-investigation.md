# Weekly-economics investigation checkpoint

These are targeted static/data-flow findings from the supported executable whose
hash is recorded in source-hashes.txt. The private traces were examined locally
before the execution workspace disconnected. They are **not original Windows
behavioral verification**. No original procedure dumps, dialogs, binaries, tables,
graphics or audio are included here.

M19–M21 run recovered production, history and recall boundaries on the isolated
200-tick budget clock. The company date still does not advance. M22 records a
staff-list payroll projection; it does not complete monthly economics.

## Payroll scratch writers

The monthly reader is `009DB4F4 +06F6`. Its source salary index is shared
`A14034` / member `3018`, rather than the payroll loop variable.

| Procedure | Writes to the shared scratch | Current integration |
| --- | --- | --- |
| Staff list `009B0B60` | Clear at +0051, increment at +00AA; forward/reverse mappings at +008F/+009C | M22 captures the count for a complete modern source table |
| Module1 `009933DC` | +0049 and +00A1 | Unconnected; reconstruct call sites and random-selection lifecycle |
| Module1 `009930BC` | +0049 and +00A1 | Unconnected; reconstruct call sites and random-selection lifecycle |
| Technology selector `0099F9A4` | +02B9 and +0312 | M22 conservatively invalidates the projection |
| Periodic `009E3830` | +1AAC and +1AF7 | Unconnected employee-collection branch |

The staff list also sums individual source salaries at +00BF. Its displayed
salary total is distinct from the payroll reader's repeated scratch-index salary.
See payroll-state.md for the implemented projection and its modern integrity guards.

No explicit startup/reset write to 3018 was established by the inspected reset
and initializer procedures. Its address is not backed by file bytes; this alone
does not prove a modern opening value. Keep the fresh projection null until the
declaration/startup lifecycle or original runtime establishes that value.

## Signed financial helper

Thunk `0060ED5C` resolves to Module1 helper `0097DD04` (88 bytes).
It adds the signed argument directly to company money, then to accounting members
22C, 23C and 24C. The four successive additions end at offsets +0015, +002A,
+003F and +0054. No zero clamp or affordability check appears in that helper.

M21 preserves signed recall refunds and possible negative company funds. This
does not establish bankruptcy, annual accounting or rank progression. The three
accounting totals still need semantic/lifecycle mapping before complete economics.

## Bank terms: static findings, not an enabled feature

Bank handler `00992DA0` has three loan-record slots. The selected loan types
map to these principal amounts:

| Original type | Principal |
| ---: | ---: |
| 1 | 10,000,000 |
| 2 | 50,000,000 |
| 3 | 100,000,000 |
| 4 | 200,000,000 |

It writes the selected type and borrowing year/month/week to globals A11318,
A11334, A11350 and A1136C, and credits principal through the signed money helper.
The weekly repayment block `009DB4F4 +178C..1975` tests borrowing year + 1
and the same month/week, charges principal × 1.3, and clears the loan type.

Remaining targets are opening/reset state, eligibility and refusal paths,
slot selection, complete calendar ordering, original save fields and insufficient
funds/game-over behavior. Do not add a bank UI that supplies principal without a
working due-date/repayment pipeline. No bank service is enabled at this checkpoint.

## Per-copy fee negotiations

Meeting mode 13, `009EFA40 +41A9..4E58`, updates the second per-copy fee
`A1188C` used in own-game sales:

| Current fee | New fee on success | Success condition |
| ---: | ---: | --- |
| 1,500 | 1,300 | Guaranteed; no draw |
| 1,300 | 1,100 | floor(Rnd × 2) = 0 |
| 1,100 | 900 | floor(Rnd × 4) = 0 |
| 900 | 700 | floor(Rnd × 8) = 0 |
| 700 | 500 | floor(Rnd × 16) = 0 |
| 500 | Unchanged | No draw |

Unrecognized current values do not establish another tier.
Mode 14 (`+4E63..5ABD`) concerns the first per-copy fee A1187C; its complete
tier/chance flow has **not** been established. Do not assume it mirrors mode 13.

The legal-provider form handlers `00987810` and `00987678` route to modes
14 and 13 respectively, after confirmation and an affordability comparison using
consultation-fee global A11508. A charge was not established in the inspected
caller/mode-13 branch; the shared helper at thunk 0060ED80 and form lifecycle still
need tracing. Provider availability and fee initialization are also unresolved.
Do not expose this as a free fee-reduction action.

## Hardware and calendar targets

Weekly hardware code is bounded to `009DB4F4 +0806..1471`. It includes a
comparison of member 1B98 against 4 and availability transitions 0 → 1 → 2.
The full identity/meaning of 1B98, counters involving constants 24/14/17,
launch/retirement eligibility, and market-table lifecycles remain unresolved.
Do not infer their units or replace them with a conventional release schedule.

March-end annual renewal and subsequent month rollover are documented in
milestone-8.md. Before connecting date advancement, reconstruct and test the
remaining weekly ordering, modal resumption, payroll scratch writers, loan
repayment, hardware events and bankruptcy. Source evidence must determine unknown
values; modern defaults must not silently stand in for missing original state.

## Resume and validation

Main contains the clean-room source and completed milestones through M24.
M23/M24 verify exported Linux and native Windows core-loop fixtures; those
synthetic checks do not verify the original Windows game.

After local execution is restored, preserve the existing private original files
and traces, reconcile the checkout with main without overwriting private work,
and rerun the opt-in original-evidence/import suites. Continue with the bounded
procedures above, then implement verified calendar transactions and their tests.
Imported catalogs, original saves and raw traces stay local.
