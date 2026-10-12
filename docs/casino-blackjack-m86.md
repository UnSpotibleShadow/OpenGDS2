# M86: Source-backed blackjack

Implemented 2026-10-12 following [M77 rules](casino-investigation-m77.md), [M78 fidelity](casino-fidelity-m78.md), [M79 runtime boundaries](casino-runtime-boundaries-m79.md) and [M85 poker](casino-poker-m85.md). Original executable identity remains SHA-256 `4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`. No original assets, dialogue or private traces are published.

## Rules and event ordering

Blackjack command `009D1968` maps index 0 to close, 1 to initial deal/MORE/double-up and 2 to bet/STAY/collect. Unlike poker, it has no unused entry random draw. Bets immediately spend one coin, up to 50; initial Deal stays disabled until an accepted bet. Start generates eighteen unique cards in side-major order, nine player cards followed by nine dealer cards. Hits use those predealt cards without new RNG calls.

Timer `009D7BB8` requests 50 ms callbacks. Initial player cards reveal at callbacks 18 and 36; the dealer's first card at 54; the initial decision/natural award is reached at 75. MORE reveals the next predealt card at callback 10. Initial scoring and later scoring follow separate branches in `009A7644`: two aces initially score 12, while the normal retry downgrades all eleven-valued aces together. A,A,9 therefore scores 11, not standard blackjack's 21. Initial two-card 21 pays 2× without a hidden dealer-natural check.

STAY checks five/six/seven cards for 4×/6×/20×, exactly three sixes for 25×, then eight or more cards for 100×, before entering ordinary dealer play. These bonuses are not awarded merely by reaching the card count. Dealer bust pays 2×; equality pays 1×; a dealer total above the player loses immediately even below 17. A lower dealer total at most 16 requests another card and resets its timer to -9, giving a 19-callback repeat cycle. A lower total at least 17 pays 2×. The recovered table is `[1,2,4,6,20,25,100]`.

Winning status starts a separate counter at 1; timer blocks execute sequentially, including the same-callback increment for timer-generated wins. Award controls become ready at 30. Collection credits one coin per two callbacks; remaining shared payout is not automatically refunded or drained on close.

Double-up deals five fresh unique cards immediately. Selection handler `00988514` records two player choices; the second must rank higher than the first, with ace high. The first chosen face is hidden in normal UI, but the handler has no distinct-index guard. The domain retains direct same-index invocation; it does not claim native focus/mouse reachability. Other cards reveal at callbacks 9/18/27, the second chosen face at 36, and comparison occurs at 38. A higher second card doubles pending payout, a lower card clears the active stake while retaining inactive payout storage, and a tie starts its counter in that callback and redeals at 30 without changing the amount.

## Retained state and explicit adaptations

Form load `009951FC` resets bet, phase, timer, blink and lose/reset flag. It does not write the win flag `00A15070`, win counter `00A15090`, award index `00A150A0` or shared pending payout. The 669-procedure reference index limits direct accesses to those three blackjack-specific globals to command, scorer and timer. The port preserves that retained win/counter/index state across form reopen, intervening poker and new-company creation, alongside M85's shared payout/bonus handling. Form reopen resets the bet and initial Deal enable state; retained winnings can still be collected through command 2 with a zero stake. A retained zero payout can leave collection waiting; closing remains available. Modern double-up is disabled for a zero award rather than creating a meaningless zero-value comparison.

Modern saves persist the active form, predealt cards, choices and callback cursors. This is a continuity policy, not an original serialized round. Reload performs no new draw, credit or debit and preserves unrelated payroll scratch. Modern presentation scratch is reset on form open and inactive counters are bounded rather than incremented indefinitely; complete native global-cell/presentation parity is not claimed. Existing pre-M86 modern saves default to a closed, empty blackjack state.

The original deals nine cards per side yet can request another card without a recovered ordinary exhaustion exit. The port pauses such a tenth-card request with an explicit message and a close action. It does not invent another card, reshuffle, award or refund. Integer overflow is refused before financial mutation. Cards are authored text; the independent requested callback timer does not run catch-up bursts. Native Windows queue/input/audio/bitmap parity remains separate acceptance work.

## Validation and continuation

The domain suite passes 412 checks with the optional private original fixture, including 21 added blackjack checks. Checks cover both scoring paths, all stay awards, dealer comparison precedence, callback boundaries and negative timer restart, immediate betting/RNG order, retained win/reopen handling, payouts, double-up wins/losses/ties, direct handler reachability, save roundtrips, invalid state/overflow, office guards and the predealt-array boundary. English/Japanese Godot checks cover the selector, opening bet reset, all eighteen cards, initial reveal, office pause, save/reload and payroll scratch, exact collection and two-card double-up. Poker is regression checked. Original casino fingerprints/dispatch targets/anchors are reverified locally.

Card-chain and horse racing remain explicitly unavailable. RC.4 is unchanged and predates this implementation; the final release export must be rebuilt. Native playthrough acceptance remains community-led; automated Linux checks do not establish it.
