# M87: Card-chain and remaining shortcut evidence

Implemented 2026-10-12 from [M77 ordinary rules](casino-investigation-m77.md), [M78 native controls](casino-fidelity-m78.md) and [M79 input boundaries](casino-runtime-boundaries-m79.md). Original binary identity is unchanged; original assets/dialogue and full traces remain private. Horse racing is still unavailable.

## Rules and newly confirmed call order

Command `009C8A8C` consumes an unused random draw on entry, including rejected commands. Bets immediately debit coins, up to 50, and clear accumulated bonus while retaining pending payout. Form load `0099B03C` resets phase/outcome but retains shared bet, pending payout and bonus; initial Deal is disabled until a bet is accepted through ordinary controls.

The five-card initial hand uses rejection sampling with a persistent used-card map. Unlike poker, all five cards are presented immediately. **Initial command +0C1A calls virtual slot 06F8, the same bonus classifier `009A7EB8` called by timer +09C4.** Thus initial-deal bonuses are included, in addition to the replacement evaluations described in M77. Timer +0921 calls the separate visual helper 06FC and does not evaluate a second bonus.

The first removed card is unrestricted. Later choices must match the removed card's suit or rank. Timer `009E13B8` captures its reference on callback 1, draws the selected slot's replacement on callback 3, increments the chain count, evaluates one hand bonus and checks for another match. Used cards are not recycled. A flush adds 1× bet, full house 2×, four of a kind 5× and straight flush 20×, choosing the strongest category. Royal flush uses straight-flush bonus. Bonuses accumulate on every evaluation.

On chain end, count at least ten receives the recovered range multiplier times bet, plus accumulated bonus. Below ten with positive bonus, source code leaves pending payout unchanged before adding bonus: retained storage is preserved. A short chain without bonus loses and clears bet. The multiplier table is `[1,2,3,4,6,8,10,13,17,20,25,30,40,50,55,60,70,80,100]`; M77 records all range boundaries. Collection transfers ten coins per two callbacks while pending is at least 150, then one per two callbacks. It resets phase/bet when the award reaches zero. Close records company activity without refund or drain.

Double-up uses the poker dealer-versus-choice rule, ace high, with five fresh unique cards generated at callback 18, other-card reveals at 1/10/19 after selection, comparison at 28 and tie redeal after the recovered delay. The domain shares the verified double-up interpreter; poker behavior remains regression checked. Card-chain requests independent 40 ms callbacks without catch-up bursts.

## Keyboard branches and liveness boundaries

The source enables KeyPreview and handles characters 1–5, lowercase q and lowercase w. The port routes the same characters before focused controls; uppercase characters are not normalized. q/w invoke the two command handlers and consume the entry random draw. Direct w can start with no bet despite the ordinary disabled Deal button. In phase 1, w reaches the copied replacement branch: it replaces unheld cards immediately, then the ordinary timer replaces the retained selected slot once more and increments the chain by one. It is not treated as dead code or offered as another game.

The source card-selection handler also accepts phase-5 double-up choices before callback 18, including index 0 through the keyboard. An early choice stops phase-5 generation and compares retained cards; this path preserves its empty used map in modern saves. Selecting dealer index 0 enters phase 6, but comparison iterates only player indices 1–4 and never resolves it. The port preserves the waiting path and explains that the player may close; unused counter growth is bounded. Native Windows key/focus delivery equivalence remains unclaimed.

After five initial cards plus 47 replacements, the used map is full. The original retry loop cannot produce the next card and never returns. The port pauses that request, records exhaustion, shows a close action, and saves/reloads the boundary without another random draw, reshuffle, refund or fabricated terminal award. Native unreachable payout bands remain documented, not made reachable. Numeric overflow similarly pauses/refuses before financial mutation. Zero-award double-up is disabled; a zero-award collection remains waiting and closable.

## Modern save policy and checks

Modern saves retain active cards, used map, reference card, selection, bonus/pending payout, timer cursors and boundary flags. This is modern continuity rather than an original saved round. Loading performs no random draw or repeated credit, and preserves unrelated payroll scratch. The view uses authored text cards and resets presentation scratch on form open. Original bitmap, focus and timer queue parity are not claimed.

The domain suite passes 431 checks with the optional private original fixture, including 19 new card-chain checks. They cover every multiplier boundary, bonus strength and initial invocation, single replacement call order, retained versus overwritten awards, collection threshold, close, keyboard-only zero-bet/copied/early/dealer paths, exhaustion, overflow, save reconstruction and office guards. English/Japanese UI smoke checks exercise the selector, bet/deal/replacement, office freeze, saves, lowercase shortcuts and exhausted-deck recovery. Poker/blackjack are regression checked and original casino anchors reverified.

RC.4 remains historical and unchanged; a final Windows source export is still needed. The repository remains subject to the owner's visibility decision, Actions stay paused, and native playthrough acceptance remains community-led.
