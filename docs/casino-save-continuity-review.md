# Casino save continuity review — 17 October 2026

Step 9 reviews an intentional difference, rather than a missing original rule.
**Retain modern active-round continuation.** No casino gameplay, financial rule
or save schema changes in this review. Original GD2 compatibility remains outside
this work; the owner explicitly skipped step 8.

## Original evidence

Supported executable SHA-256:
`4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.
Re-running `tools/map_native_save_fields.py` against that executable verifies
130 paired record templates, 448 field slots and 447 distinct company members.
The save/load procedure fingerprints are checked before decoding.

* Save `Module1 009D9824`, template 4 at `+0108`, and load `009D60E0` at
  `+0099` include coin member `0110` (global `00A1112C`).
* Shared bet `3E30`, phase `3E40`, pending payout `3EE0` and card-chain bonus
  `3E20` are absent from the paired serialization members. Their corresponding
  globals are `00A14E4C`, `00A14E5C`, `00A14EFC` and `00A14E3C`.
* Poker load `009960C4` and card-chain load `0099B03C` reset phase/outcome;
  blackjack load `009951FC` also resets bet and presentation counters. These are
  **form-open handlers**, not proof that loading an original company save clears
  every process-global cell. Cold process launch, loading into an existing
  process and closing/reopening a form are distinct operations.
* The original card unload handlers record company activity and resume the office.
  They do not refund a spent bet or automatically collect all pending winnings.

The narrower established fact is that original saves do not encode the complete
resumable card round. This audit is not native Windows save/load acceptance,
original GD2 interoperability or a complete reconstruction of every transient
horse-racing variable. See [M77](casino-investigation-m77.md),
[the paired field map](original-native-save-fields.json) and
[horse-racing evidence](casino-horse-racing-m89.md).

## Modern policy and reason to retain it

`SaveStore` serializes casino activity flags, shared amounts, cards/used maps,
selections, callback cursors and race snapshots. `GameSession.Restore` restores
that snapshot and the shared simulation RNG. The office reconstruction displays
the active game directly, without invoking its fresh-open initializer.

| Activity | Retained continuation |
| --- | --- |
| Poker | Reveals, holds/replacement, payout collection and double-up |
| Blackjack | Eighteen predealt cards, independent reveal/award/collection counters and double-up choices |
| Card-chain | Used deck, removed reference card, replacement cursor, chain/bonus and stepped collection |
| Horse racing | Draft wagers, committed stakes, selected runner/jockey records, positions/finish order and pending result |

Restoration itself must not draw, debit, credit, refund or settle. A callback
resumes from its saved cursor; future random draws use the saved shared stream.
Physical held wager input is excluded from JSON and stays released after load.
Office simulation remains blocked while an activity is active. Existing
close/reopen rules are separate and remain unchanged.

Removing the snapshot or calling a fresh-open handler on load would discard
already-paid play or reset partially collected awards. Resetting financial
scratch would also create new accounting rules. Neither change follows from the
native serialization omission alone. Retaining modern continuation avoids those
losses and keeps existing modern saves usable. It does not repair native casino
quirks: for example, horse temporary winnings are still discarded by the
recovered next-card path.

## Verification

Five new domain regressions compare uninterrupted play with a new `GameSession`
restored before **every callback**, using the real saved 24-bit RNG. They cover
poker reveal/replacement/double-up/partial collection, blackjack predealt reveal
and partial collection, card-chain replacement and 150-coin stepped collection,
and horse draft/commit/motion/result/next-card setup. Full serialized states match
at each boundary; restore has no financial or random effect, the office stays
paused and unrelated payroll scratch remains unchanged. The public domain suite
passes 565 checks.

The new English/Japanese UI fixture saves and loads each card game with callbacks
held still, verifies byte-identical saved state and correct restored window, then
resumes callbacks and checks one bet debit and one pending replacement. It passes
with zero failures. Existing horse UI continuation remains in source CI, alongside
the new fixture. The native field audit and visual/input/timing acceptance are
separate: no paired original Windows runtime acceptance is claimed.

All fixtures in Git use authored data. Original executable/graphics, generated
private field reports and logs stay outside Git. No release, tag, package or main
promotion is requested. Step 10 (casino exhaustion/numeric-boundary recovery)
requires separate owner permission.
