# Consumer investigations and recalls — M21

Current presentation: [consumer/secretary conversations](recall-conversations.md)
explain quantity, per-copy recall price and total; the final page acknowledges
the event directly without a separate receipt.

Evidence is targeted static P-Code for the supported executable. The original
Windows runtime and a complete global RNG sequence have not been compared.
The supported product investigation pipeline now runs on the recovered budget
week; the complete calendar, economics and other event pipeline remain unfinished.

## One check after late sales

After demand decay, periodic procedure `009E3830 +12D1..1417` tests demand
below 16 and product recall state zero. It writes state one **before** the
investigation test. A failed test therefore remains checked and is not retried.
The three random draws are all consumed, including draws whose bug predicate
is false; the conditions are documented in [reviews-and-events.md](reviews-and-events.md).
Zero and negative random ranges in the high-bug branch are retained.

On success, meeting mode 11 (`009EFA40 +38A0..38AD`) sets the countdown to
five. It does not charge money, remove stock or prohibit reproducing copies.
M21 persists an investigation modal, pauses office clocks until acknowledgement,
and shows the remaining weeks on the sales screen. Fresh product state/countdown
are zero, established by reset `009DD234 +14B7..14D4`.

## Five weekly boundaries and the result

The weekly routine `009DB4F4 +1496..1538` decrements a positive countdown only
for state one. When that decrement reaches zero it opens meeting mode 12.
A zero countdown belonging to an unsuccessful original check does not trigger
a result. M21 schedules this on the existing 200-tick budget boundary; pending
reviews, development decisions, sales milestones and recalls pause this clock.

Result mode `009EFA40 +3A58..4186` draws one of four outcomes:

| Draw | Refund | Result state | Stock | Reprinting |
| --- | --- | ---: | --- | --- |
| 0 | (units sold + current stock) × current price | 2 | Zero | Prohibited |
| 1 or 2 | (units sold + current stock) × 1,000 | 2 | Zero | Prohibited |
| 3 | Zero | 3 | Retained | Normal budget guard |

Units sold may be signed because the original sales result has no lower clamp.
Refund arithmetic retains this behavior. The financial helper `0097DD04` adds
its signed argument directly to funds and three accounting cells, with no zero
clamp. M21 can therefore persist negative funds after a refund. The recovered
negative-cash dissolution is now implemented; see
[financial failure](financial-failure-m84.md). No debt forgiveness is added.

Sales continue during an acknowledged investigation. Those receipts and new
inventory affect the eventual result. Reprints are allowed while state is one;
the original reproduction menu tests state two before the shared budget flag.
The Godot result is committed once before its saved modal appears. Reloading
or acknowledging that result cannot debit the company again.

## Scope and integrity

Each product stores its original state/countdown and a modern pending-dialog
and result projection. Missing legacy recall state stays unknown; it is neither
initialized as unchecked nor inferred from bugs or units sold. Legacy products
with unknown recall state cannot bypass the original reproduction guard.

The simulation records a complete sales batch before showing its queued modal
results. Product-index ordering preserves each product's sales milestones before
its investigation. This is modern persistence infrastructure; exact original
mid-dialog/global execution resumption is still not established. Other events,
dates, payroll, annual contracts and bankruptcy are not silently filled in.

Tests cover one-time checks, three-draw evaluation, all four outcomes, five live
week boundaries, modal pauses, save/load, repeat acknowledgement, reprint guards,
negative funds and signed refunds, overflow rejection and malformed/legacy saves.
The Godot UI suite drives a guaranteed high-bug investigation through both saved
dialogs and the live five-week clock, accounting for concurrent sales receipts.
