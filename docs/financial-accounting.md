# M34: signed company accounting

The shared financial helper `0097DD04` adds the same signed argument to cash
(member 20) and the weekly/monthly/yearly cells 22C/23C/24C. The helper does not
clamp negative funds or convert the argument to another unit. Reset
`009DD234 +014D..016A` writes Empty to those three cells, numerically zero.
Opening cash is assigned separately and is not revenue.

## Connected transactions

`FinancialRules` preserves this shared arithmetic in the domain. Hiring,
planning, type/genre selection, purchases, company activities, production,
sales and recall refunds now feed all three known periods. Negative receipts
and refunds retain their original signs. The company window displays bilingual
weekly, monthly and yearly net rows in the same units as its funds row.

Representative original helper call sites independently traced locally:

| Transaction | Procedure and call offset |
| --- | --- |
| Employee-copy hire debit | 0098735C +0127 |
| Annual renewal debit | 00995CDC +034F |
| Planning debit | 0099BA80 +002F |
| Type / genre debit | 009ACF14 +0755 / 009B00D8 +073B |
| First shipment / reprint debit | 009C9CB8 +01DD / 00984BB0 +00A5 |
| Computer / technology purchase | 0099037C +014C / 00986614 +00DE |
| Company-menu activity payment | 0098B1F4 +019D |
| Sales credit | 009E3830 +10F6 |
| Meeting payments and recall results | 009EFA40 financial calls, including +3D07 / +406C |

Modern arithmetic checks prepare all four sums before mutating cash, employee
gates, stock or project records. Overflow rejection is a storage safeguard;
it does not emulate VB6 numeric overflow. Older saves leave absent period totals
null. Subsequent cash changes cannot reconstruct the missing history, so each
unknown period remains null independently until a proven period reset.

## Reset order and limits

In the ordinary weekly routine `009DB4F4`, +0672..067C clears weekly 22C.
At week >=5, +06AC..06B6 clears monthly 23C. At month 13,
+0725..072F clears yearly 24C before the year rollover and payroll debit.
`ResetAtWeekEnd` implements these isolated reset boundaries and validates its
closing date before mutation. At M35 the resumable calendar integrates the corresponding resets before
payroll for new supported companies. Renewal, hardware events, loan dues and
per-product recalls now have saved ordering; unresolved modal branches stop
explicitly. Pre-calendar saves retain unknown historical state. See
[calendar-transactions.md](calendar-transactions.md).

The rows do not add spending categories, invented rent or profit bonuses. Original display rounding/units and complete graphical
comparison still need checking.

## Validation

Seven new public domain regressions cover signed changes, independent unknown
periods, save/reload, overflow without mutation, period boundaries, opening cash,
renewal dismissal atomicity, and actual hiring/sales integration. The core-loop
Godot fixture checks saved planning expenses and the actual company-window row.
Bounded original-evidence checks verify reset cells, all three helper additions
and the three weekly reset stores. Full original Windows runtime comparison
remains outstanding.
