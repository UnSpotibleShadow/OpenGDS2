# Company production allowance — M19

These are targeted static findings for the supported executable hash. Original
Windows behavioral comparison remains outstanding. No original text or data is
distributed with this report.

## Shared flag and reset

Global `A110EC` is Module1 member `B0`. Reset `009DD234 +005A..0064`
writes Empty, which is numerically zero. First shipment `009C9CB8 +01BC..01C5`
and reproduction `00984BB0 +0064..006D` both set it to one after their acceptance
guards. The weekly procedure `009DB4F4 +000B..0016` clears it to zero.

The own-product menu `00984A78 +006D..00CD` rejects opening reproduction when
the flag is one. The list handler `009871D0 +00AF..010F` repeats this guard.
Both read one company cell, rather than a per-product allowance. The first
shipment handler sets the cell without checking this reprint guard: a new game's
first release is not blocked merely because another game was produced that week.

M19 connects this flag to the existing verified 200-tick budget phase. First
shipment consumes the allowance; an accepted reprint consumes it again after
the next reset. Failed/blocked reprints leave funds, stock, accounting and the
flag unchanged. All products share the allowance. Pending development decisions,
reviews and sales milestone dialogs continue to pause the clock and its reset.

## UI and persistence

The sales screen explains availability and refreshes reproduction buttons when
the budget resets. English and Japanese labels are authored for this UI. Saves
persist the flag and existing phase; old saves without the flag keep it unknown
and cannot silently obtain a fresh allowance. A new company starts with the
verified empty/zero flag.

This connects the production budget, not the full calendar/economics pipeline.
The displayed date still does not advance. Payroll, scheduled recalls, hardware
production and company progression require further reconstruction. The menu's
recall-state guard is traced but will be connected when the recall state machine
is implemented. No production multiplier, price change or new benefit is inferred.

## Regression coverage

Tests cover a first shipment when the flag is already set, blocking another
product's reprint, the exact 199/200-tick boundary, mid-budget save/load,
unknown legacy state, transaction failures and modal pauses. The Godot gameplay
test checks blocked immediate reprinting, live re-enabling and saved consumption.
Bounded evidence checks include flag reset/set sites and the menu guard.
