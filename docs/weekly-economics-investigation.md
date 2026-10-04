# Weekly-economics investigation checkpoint

M35 integrates the verified branches into the resumable calendar. The older
findings below record the investigation progression; current implementation and
remaining stops are in [calendar-transactions.md](calendar-transactions.md).

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

Members 2FC/318/334/350 correspond to those four globals.
It writes the selected type and borrowing year/month/week to globals A11318 (type),
A11334 (year), A11350 (month) and A1136C (week), and credits principal through the signed money helper.
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
M26 independently checks mode 14 (`+4E63..5ABD`), which updates the first
per-copy fee A1187C. It has the same six fee comparisons, five subtract-200
stores and four outcome ranges. This is established from its own instructions,
not inferred from mode 13. `FeeNegotiationRules` implements the isolated outcome
branch for both modes, without charging money or changing live company fees.

| Evidence | Mode 13 offsets | Mode 14 offsets |
| --- | --- | --- |
| Fee comparisons: 1500, 1300, 1100, 900, 700, 500 | 41FB, 437F, 45C3, 4807, 4A4B, 4CE4 | 4EB5, 5039, 527D, 54C1, 5705, 5949 |
| Subtract-200 result stores | 4378, 4515, 4759, 499D, 4C36 | 5032, 51CF, 5413, 5657, 589B |
| Outcome draw starts: ranges 2, 4, 8, 16 | 4437, 467B, 48BF, 4B03 | 50F1, 5335, 5579, 57BD |

The meeting constant pool at 005FAFCC maps entries 107 and 116 to A1188C and
A1187C; entry 27 is Rnd. Unsupported current fee values skip all tier bodies,
retaining the fee. Outcome branches consume no outcome draw at 1500 or 500;
this does **not** imply that the complete conversation consumes no random draws.
Its dialogue helper can select speakers separately. Domain tests check exact
probability boundaries, terminal/unmatched values, draw consumption and the full
1500-to-500 sequence. Bounded original-evidence checks verify both branches.

The legal-provider form handlers `00987810` and `00987678` route to modes
14 and 13 respectively, after confirmation and an affordability comparison using
consultation-fee global A11508. M26 resolves thunk 0060ED80 to the 40-byte helper 0097BDB8. It stores its mode
argument into member 38D0 (A148EC) and opens the meeting form modally; it does not
call the financial helper. The meeting's 2080-byte load procedure 009A6DDC and
all five legal-provider procedures were inspected and contain no direct financial
helper call. The outcome bodies also contain no direct consultation debit. This
narrows the missing charge investigation; it does not prove that every caller or
indirect form-lifecycle path is free. Provider availability and fee initialization are also unresolved.
Do not expose this as a free fee-reduction action.

## Hardware and calendar targets

Weekly hardware code is bounded to `009DB4F4 +0806..1471`. It includes a
comparison of member 1B98 against 4 and availability transitions 0 → 1 → 2.
M34 tracing resolves 1B98 as a release counter; see below. Counters involving
constants 24/14/17, complete launch/retirement eligibility, and market-table
lifecycles remain unresolved.
Do not infer their units or replace them with a conventional release schedule.

March-end annual renewal and subsequent month rollover are documented in
milestone-8.md. Before connecting date advancement, reconstruct and test the
remaining weekly ordering, modal resumption, payroll scratch writers, loan
repayment, hardware events and bankruptcy. Source evidence must determine unknown
values; modern defaults must not silently stand in for missing original state.

## Ordered calendar body — M33 comparison follow-up

The restored local trace confirms the following order in `009DB4F4`. This is
static execution order, not a complete transaction implementation or a claim
that every branch runs on every week:

| Order | Bounded source | Established operation |
| --- | --- | --- |
| 1 | +0000..0015, +0119..01AD | Reset weekly action/production flags; shift product history and clear its current cell |
| 2 | +01AE..01C6 | Increment week, then call refresh helper through thunk 0060ED98 |
| 3 | +04CB..0566 | At March week 5, open the annual renewal form modally before month rollover |
| 4 | +0672..06B6 | Clear accounting 22C every ordinary weekly pass; at week >=5 set week 1, increment month and clear 23C |
| 5 | +06B7..0776 | Sum employed payroll through the shared scratch index; if month 13 clear 24C and roll to January/year+1; then debit through the signed helper |
| 6 | +0777..1471, +147A..153E | Hardware branches precede the pending recall countdown/meeting |
| 7 | +177F..197E | Visit loan slots and due-date repayment after these event branches |

The reset schedule identifies 22C/23C/24C as weekly/monthly/yearly signed
accounting projections. Their display units and every money-writer call site
still need checking. At January rollover payroll is charged after all three
resets. Renewal can alter employed gates before the same pass sums payroll.
These are reasons to preserve transaction ordering rather than attaching
independent monthly timers.

The earlier conditional branch at +01C7..04CA can exit the routine before
renewal/accounting rollover. Its year/month/week comparisons and additional
3B0C guard require complete lifecycle tracing; do not treat it as an ordinary
completed week. Original modal calls resume the procedure. Godot must represent
pending calendar work explicitly before connecting date advancement, including
save/reload and refusal/resumption tests. Existing isolated 200-tick services
must not execute those same transactions a second time.

## Hardware release gate — M34 tracing follow-up

Member 1B98 / A12BB4 is not an elapsed-week counter. Reset
`009DD234 +12CE..12D7` writes Empty. First shipment `009C9CB8
+102F..103E` increments it by one. A targeted reference scan finds no weekly
increment. The weekly hardware eligibility branches compare it against >=4,
then clear it when starting an eligible platform countdown (for example
`009DB4F4 +0801..081B`). Reprinting uses a separate handler and does not write
this counter. Thus the source gate counts first shipments since the last
hardware trigger; it must not be substituted with the total lifetime release
count after triggers begin resetting it.

Identifier membership, current-stage member EB4, funds and zero launch countdown
are additional predicates. The launch loop decrements the new countdown in the
same weekly pass, after the trigger loop. This narrows the remaining calendar
work; neither the counter nor live platform events is connected yet. Stage
transitions, retirement side effects, modal news/licensing and save fields need
complete reconstruction before enabling them.

## Resume and validation

Main contains the clean-room source and completed comparison/interactions/technology
cadence checkpoint through M33.
M23/M24 verify exported Linux and native Windows core-loop fixtures; those
synthetic checks do not verify the original Windows game.

Local execution was restored at M25 and opt-in original-evidence/import suites
passed. Preserve private original files and traces when reconciling with main. Continue with the bounded
procedures above, then implement verified calendar transactions and their tests.
Imported catalogs, original saves and raw traces stay local.

## Calendar follow-up: bank entry and information form

After M35 validation, targeted local traces narrow these next targets:

- `MDIForm1.mKoudouBank`, `0097A3BC` (24 bytes), opens the bank form
  modally without a local funds/rank predicate. This does not establish every
  menu-enabled writer or original availability state.
- `fGMBank.Command1`, `00992DA0 +0000..0098`, refuses when all three
  loan types are >=1. Otherwise +009B..028B scans slots 0..2 and uses the
  first type-zero slot, then leaves the loop after the transaction.
- +01D3..0205 writes type and current borrowing date; +020D credits principal
  through the financial helper. Its post-transaction dialog precedes form close.
  The four principal choices are the terms already documented above.
- Bank `Form_Load`, `00994AFC`, displays existing records; `Form_Unload`,
  `00977E4C`, invokes the 28-byte helper at `0097B410`. The helper has no
  direct global or financial writes in its bounded body.
- All six `fGMGameHardInfo` procedures were inspected locally. Its load builds
  a display scratch map at global A1431C/A14330. No direct financial helper
  call or platform market/countdown/license write was found in these bodies.
  This supports M35's news pause projection; it does not establish all indirect
  UI or graphics behavior.

Raw traces/dialogs remain local. Borrowing stays disabled until the menu-state
lifecycle is checked and the original refusal/confirmation paths are connected
to the already working due-date transaction. Bankruptcy remains a separate
periodic-procedure target.
