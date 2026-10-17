# Casino exhaustion and numeric recovery — 17 October 2026

Step 10 retains the deliberate safeguards against original hangs/array errors and
unrepresentable modern integers. It fixes a poker save-validation gap and makes
the recovery instructions fit the existing compact card-game windows. No deck
recycling, extra award, refund or change to ordinary odds/payouts is introduced.

## Original evidence and retained policy

The supported executable is SHA-256
`4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.
The [M77 evidence](casino-investigation-m77.md) identifies card-chain's
`+025B..02DA` used-card retry, with `+0593 -> +025B` and no exhaustion exit.
Five initial cards plus 47 replacements mark all 52 cards used. The next request
cannot finish. OpenGDS pauses that request and permits closing the table. It
retains cards, stake, bonus and pending storage without drawing again, recycling,
automatically paying a terminal reward or refunding. High payout bands in the
native table do not justify making this stalled path reach them.

Blackjack predeals nine cards per side. The port stops an out-of-range tenth-card
request and permits closing instead of inventing a card. Existing nonoptimal ace
scoring, retained pending amounts, zero-award waiting and keyboard dealer-choice
waiting remain separate, documented original behaviors.

Modern card balances use bounded Int64 values. A purchase/collection/double-up
that cannot be represented is refused before financial mutation. This is a
modern protection, not a claim that the original Variant limits were Int64.
[M79](casino-runtime-boundaries-m79.md) distinguishes ordinary odds from external
OLE/Variant overflow and formatting. This review re-verifies M77's 52 fingerprints,
52 dispatch targets and 31 anchors, plus M79's anchors and all 64 ordinary
odds-value combinations. It does not execute native Windows input, error handling
or OLE arithmetic and does not claim exact huge-value parity.

## Fixes

Poker commands already refused starting a collection when coins plus the whole
pending amount exceeded Int64. Unlike blackjack and card-chain, its save
validator did not enforce that same condition. A modified/impossible active
collection could therefore load, stop on overflow and leave a timer cursor
outside the save validator's accepted range. Reject that malformed collection
before deserialization succeeds or an existing valid slot is replaced. Legal
collections ending exactly at Int64.MaxValue still complete normally. Valid
modern saves need no migration or schema change.

The exhaustion/card-limit warnings previously included long technical explanations
in small labels. English/Japanese messages now give concise close/continue
instructions. Numeric warnings replace the ordinary phase text instead of
appending paragraphs below it; poker's category text also yields to that warning.
Blackjack distinguishes a numeric collection stop from its nine-card stop.
Available safe actions and the existing close button remain usable. Detailed
native defects and the no-refund policy remain documented rather than crowding
these status labels.

## Validation

Five new domain checks cover malformed poker collection rejection on write/read,
refusal before replacing a valid slot, a collection ending exactly at Int64's
maximum, persistent exhausted-deck/card-limit stops and a saved chain-bonus
numeric stop. Repeated fresh-session reloads/ticks neither draw nor alter retained
finances, and closing returns office access. The full public domain suite passes
570 checks.

A focused bilingual UI fixture checks card-chain exhaustion/numeric pauses,
blackjack card/amount limits and poker amount limits. It measures wrapped warning
height, verifies unsafe actions are disabled and Close stays enabled, round-trips
the saved boundary exactly, and returns to the office. Int64 fixture literals are
written as raw JSON integers, avoiding generic GDScript JSON double rounding.
The fixture passes with zero failures in both languages. The application build
passes with zero warnings/errors, the editor import and workflow lint pass, and
the original source verifiers pass. Actions results are recorded with the PR;
original material stays outside Git and native Windows acceptance remains unrun.

This closes the last authorized fidelity-plan step. Original GD2 compatibility
(step 8) remains skipped. No release, tag, package or main promotion is requested.
