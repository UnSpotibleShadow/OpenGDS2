# Calendar transactions — M35/M36

New companies using the supported locally imported catalog now advance the
company date on the recovered 200-tick office boundary. Four weeks make a month;
twelve months make a year. Development and product modals pause this clock.
Original LZH importing remains required. Reimport a pre-M35 catalog to recover
the newly mapped hardware growth inputs, then start a new company.

## Evidence and implemented execution order

The source is weekly procedure `009DB4F4` in the supported executable. These
are targeted static/data-flow findings; original Windows runtime comparison
remains unavailable. Public reports contain bounded structures and behavior,
not original dialogs, catalogs, binaries or procedure dumps.

| Phase | Source offsets | Implemented behavior |
| --- | --- | --- |
| Boundary | +0000..01C6 | Clear company action/production flags; shift product history once; increment week |
| Early events | +01C7..05F7 | Beginner terminal ending and annual contest pause before payroll; March-end renewal precedes rollover |
| Tenants | +05F8..0671 | Decrement the two structural tenant countdowns; clear member 1A70 at zero |
| Accounting/payroll | +0672..0776 | Clear weekly net; at month rollover clear monthly net and debit recovered payroll; January clears yearly net first |
| Hardware triggers | +0777..1103 | Visit source records in order; test shipment counter, stage, funds and zero launch countdown |
| Hardware countdowns | +1104..1479 | Visit each source's launch then retirement branch, pausing at its news |
| Recalls | +147A..1547 | Advance each product in order; a result pauses before later products |
| Hardware market | +1548..1668 | Grow saved market cells and decrement growth counters |
| Exhibitions | +1669..177E | April/October week 1 selection, fee and awareness results; see annual-calendar-events.md |
| Loans | +177F..197E | Visit three saved loan slots, announce exact due-date repayments, then debit and clear the type |
| Graduates | +197F..1C0A | April week 2 school gate, bounded lottery and individual hire/decline prompts |

`CalendarTransaction` saves its closing date, phase, record cursor and pending
news/decision. Original temporary week 5 is represented by a pending closing
week 4; modern saved/rendered dates remain valid. Hardware launch becomes
available **after** news acknowledgement. Retirement sets availability 2 and
removes its license **before** news. Acknowledging a loan debits before clearing
its type; a second acknowledgement cannot debit again. Completing a suspended
boundary resumes that office tick's sales/work without adding another budget
clock tick. The old isolated history/recall boundary is not executed alongside
the new calendar. Calendar modals disable desktop management menus/toolbars,
including their direct callbacks and employee-table activation. Save, load,
settings and return-to-title are modern escape/preferences actions; they remain
available. The uncertain-payroll Staff entry captures its verified scratch
projection in a read-only list, then the pending Continue retries the boundary.

## Payroll and accounting

Monthly payroll retains the original scratch-index arithmetic:
`employed gate count × salary at original member 3018`.
It is distinct from the staff screen's sum of each employed salary. Renewal
charges selected annual fees and dismisses unchecked staff before payroll uses
the new gates. Period resets precede payroll; renewal fees belong to the closing
period. The money helper permits negative funds. No invented rent, affordability
clamp, salary sum, or bankruptcy outcome is added.

Missing scratch/salary/source cells pause the transaction. Opening Staff can
capture its verified shared-index projection, then Continue retries the pending
step. Startup and all other scratch writers remain incompletely reconstructed.
The retained behavior is statically verified, including the apparent salary
index defect; original runtime confirmation remains outstanding. Payroll and
repayment preflight checked arithmetic before financial mutation.

## Hardware state

Reset `009DD234` establishes stage/release-counter and countdown zeroes;
initialization `009F651C +0196` sets stage 0. First shipment
`009C9CB8 +102F..103E` increments member 1B98. Reprints do not increment it.
Starting a hardware countdown clears the counter. It counts first shipments
since the last trigger, not lifetime releases or elapsed weeks.

| Hardware identifier | Required stage | Minimum funds | Retirement identifier/countdown |
| ---: | ---: | ---: | --- |
| 3 | 0 | 150,000,000 | None |
| 4 | 1 | 300,000,000 | 0 / 14 |
| 2 | 2 | 450,000,000 | 1 / 14 |
| 10 | 3 | 70,000,000 | None |
| 5 | 4 | 1,000,000,000 | 8 / 14 |
| 9 | 5 | 1,200,000,000 | 3 / 14 |
| 6 | 6 | 1,400,000,000 | 4 / 14 and 2 / 17 |
| 7 | 6 | 1,600,000,000 | None |

Every trigger also requires at least four shipments and launch countdown zero.
It writes countdown 24, then the same weekly pass decrements it to 23 after news.
Launch warning occurs at 4; launch at 0 increments stage. Retirement warning
occurs at 12; retirement at 0 removes availability/licensing. The surprising
70,000,000 threshold and both stage-six branches are retained. No alternative
platform sequence is invented to make the latter branches reachable.

Setter `009874E8` maps arguments 48/4C to 10A4/10C0. Both importers now read
the corresponding initialization literals, including the concatenated fourth
record's different prefix. Mutable market state belongs to the company save,
not the imported catalog. With `growth = 10A4`, `boost = 10C0`, and `scale = 0F54`:

```text
if growth > 0:
    market += (boost * boost + growth) * (scale / 1.5) / 900
    if boost > 0: boost -= 1
    growth -= 1
```

`market` is a double projection of original VARIANT arithmetic. Sales uses this
saved value in its established `(1 + market / 80)` factor. Original catalog
values and names remain local and unchanged.

## Loans and unresolved branches

Loan type is member 2FC; borrowing year/month/week are 318/334/350, respectively.
The corresponding globals are A11318/A11334/A11350/A1136C. Types
1/2/3/4 have principal 10/50/100/200 million. Repayment is 1.3 times principal
exactly one year after borrowing, on the same month and week. News precedes the
debit; there is no affordability guard in this repayment block. M58 connects borrowing UI and full-slot refusal with saved acknowledgment receipts;
see [bank borrowing](bank-loans-m58.md). Exact original menu eligibility,
original save import and bankruptcy remain unimplemented.
Fresh initialization establishes all three loan types zero.

M36 connects annual events using the verified forms. See
[annual-calendar-events.md](annual-calendar-events.md) for exact award defects,
exhibition fees/awareness, the Beginner terminal branch, graduate lotteries and
remaining school/rival-presentation work. Missing old release dates or graduate
bounds pause those events instead of inventing eligibility. Facility slots begin
at −1; school construction remains a separate unresolved subsystem.

Pre-M35 saves keep `Calendar = null` and their previous isolated budget behavior.
Their historical countdowns, growth and loans cannot be inferred from a frozen
date, so loading does not fabricate a migration. The calendar inspector explains
this limitation. New calendar state and pending cursors are validated on both
save and load, including availability, finite markets, event-phase consistency
and post-acknowledgement cursor positions.

## Previous M35 validation

Seventeen added public domain regressions cover payroll uncertainty/negative
funds/overflow, renewal ordering, hardware gates/countdowns/news/licenses,
source order, dynamic market sales, multiple loan slots, per-product recall
pauses, unsupported event stops, legacy saves, shipment/reprint counting and
boundary sales resumption. Godot's synthetic core-loop fixture additionally
loads/saves hardware news, acknowledges a launch, renews selected staff and
checks the subsequent payroll and exhibition stop. All fixture names/data are
community-authored. Bounded original-evidence checks independently verify the
initial cells, trigger predicates, warning values, arithmetic and loan terms.

M35 standard builds and native Linux/Windows packages passed all four jobs of
[Validate run 37192745866](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37192745866).
The modal follow-up extends the UI suite with blocked menu/toolbar callbacks
and read-only payroll resolution. All four SDK/export jobs also pass for
[the follow-up run 37193608886](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37193608886),
including the packaged Linux and native Windows core-loop fixtures. Standard
builds report zero warnings/errors; both local original-import UI suites also
pass. These headless checks do not substitute for original Windows behavioral
comparison or full graphical usability testing.
