# M85: Casino coins and poker

Implemented 2026-10-11 after the [M77 investigation](casino-investigation-m77.md), [M78 fidelity work](casino-fidelity-m78.md) and [M79 runtime boundaries](casino-runtime-boundaries-m79.md). This adds coin purchase and poker, not the remaining casino activities. No original assets, dialogue, executables or private traces are included.

## Original evidence and implemented behavior

The original selector at `00994780` distinguishes horse racing, poker, blackjack, exit, card-chain and coin purchase. Only poker and coin purchase are enabled here. Buying coins debits ¥10 million through financial accounting and adds 50 coins, without an affordability check. The original coin member `0110` resets for a new company; shared bet, pending payout and chain bonus are not written by the recovered company initializer/save/load procedures. The modern session preserves those shared amounts across new-company creation while resetting coins and the transient poker form.

Poker command entry at `009B9DE4` consumes its unused random draw even for rejected commands. The implementation uses the session RNG, rejection sampling of suit/rank pairs and a used-card map rather than a shuffled deck. Each accepted bet immediately spends one coin, up to 50. Opening the form preserves shared amounts but disables Deal until a new bet is accepted; reopening with a retained 50-coin bet consequently preserves the original inaccessible-deal behavior.

The five initial cards reveal every nine timer callbacks. Holds retain their cards; replacement draws follow ascending card order and retain the used-card map. Categories use the recovered multipliers `0, 1, 2, 3, 10, 15, 20, 100, 500, 1000`. A pair pays only for ten or higher; straights include ace-low and ace-high without other wrapping. Classification, blinking and award timing retain the recovered callback progression. Awards are total returns, collected one coin per two callbacks.

Double-up resets the used map and generates its new five-card hand on callback 18, after staged hiding. The dealer is revealed first, with four player choices. Comparison reveals the other cards at callbacks 1, 10 and 19 and resolves the chosen card at 28. Ace ranks highest. A win doubles the pending amount, a loss clears the active bet without inventing a payout, and a tie retains the amount and restarts after the recovered delay. Closing records the original company-activity flag and does not refund or drain the shared bet/pending amount.

The office clock and other gameplay commands pause while poker is open. Poker uses its own requested 30 ms callback interval without catch-up bursts. Native timer queue and rendering equivalence are not claimed.

## Explicit modern adaptations

Cards use authored rank/suit text buttons instead of original bitmap rendering. Modern saves persist an active round, cards, used map and timer cursors; original saves did not serialize that complete transient form. Loading resumes without replaying draws, charges or awards and avoids unrelated payroll-scratch reconstruction. Closing/reopening resets modern presentation scratch while preserving the established shared amounts; complete native presentation-global parity is not claimed.

Coins and pending payouts use bounded modern integers. Purchases, collection and doubling that would overflow are refused before financial mutation and explained in the interface. There is no arbitrary doubling-count cap; native huge-value VARIANT coercion parity is not claimed. Modern saves predating this feature default to a valid empty casino state, independently of unsupported original-save import.

## Validation and remaining scope

The domain suite passes 391 checks with the optional private original employee fixture, including 18 added casino checks covering category rules, random draw order, timer stages, close/reopen behavior, accounting, saved-round continuation, new-company reset and overflow boundaries. English/Japanese Godot smoke checks exercise purchase, dealing, holds, save/load, office freezing, close and double-up. Original casino evidence verification passes 52 fingerprints, 52 dispatch targets and 31 anchors; the reference-runtime fidelity and boundary verifiers also pass.

Blackjack, card-chain and horse racing remain explicitly unavailable. Their recovered rules and original defects are recorded in M77–M79 for subsequent implementation. Native Windows input, queue, audio and rendering acceptance remains community work; these Linux checks do not establish it. The existing RC.4 export predates this source change and must be rebuilt for release.
