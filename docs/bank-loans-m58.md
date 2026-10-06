# M58 bank borrowing

Bank borrowing now fills the first empty of three saved loan slots. The four
choices supply 10, 50, 100 or 200 million immediately; each repays 130% exactly
one year later on the same month and week through the existing calendar pipeline.
A full bank refuses another loan. Repayment can take funds below zero.

The private executable evidence is bounded to `fGMBank.Command1` at `00992DA0`:
+0000..0098 handles the full-slot refusal, +009B begins slot scanning,
+00FB/+013F/+0183/+01C7 load the principals, +01D3 stores type,
+01E0/+01ED/+01FA copy year/month/week, +020D credits funds and +025F calls
the receipt dialogue. +0280 exits the loop. `MDIForm1.mKoudouBank` at
`0097A3BC` opens the form without a local rank/funds predicate. The hash-guarded
`tools/verify_bank_evidence.py` verifies these anchors without publishing raw
traces or proprietary text.

The authored English/Japanese bank window shows all three records and due dates.
Selecting a principal only previews it; confirming credits funds once and saves
an acknowledgment receipt. Reloading that receipt neither credits again nor
advances the clock. Recruitment, hiring and secretary advice are suspended until
acknowledgment. Arithmetic and due-date overflow fail before mutation. Unknown
calendar/date/funds and other suspended events prevent borrowing.

These modern transaction guards and receipt persistence are reconstruction
choices. All original menu-enabled writers and exact form/dialogue presentation
remain comparison gaps. No borrowing rank requirement, upfront fee, early
repayment or bankruptcy outcome has been inferred. Original save import remains
separate. Private assets and original dialog text remain outside source control.

Validation covers all four principal/repayment values, negative funds, slot
reuse, full-slot refusal, overflow, malformed receipts and save/reload with 290
domain checks. The synthetic Godot core loop covers preview/cancel, three loans,
receipt reload/clock suspension and annual notices/debits. Direct app compilation
and the bounded original-evidence verifier pass. No desktop package, native
Windows validation or GitHub Actions run is produced.
