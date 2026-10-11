# Casino investigation — M77

Follow-up: [M78 fidelity closure](casino-fidelity-m78.md) resolves the source-level RNG, race motion, control defaults and same-timer reentrancy gaps below. Its explicit Windows execution boundary supersedes those pending static-tracing items; this historical report remains the ordinary-rule evidence.

Research checkpoint: 2026-10-11. No casino gameplay is implemented by this change.
The supported original is the locally extracted 2001 executable, SHA-256
`4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.
Addresses below are executable virtual addresses; `+offset` refers to the
procedure start. Original assets, dialogue and full traces remain private.

## What actually exists

| Activity | Original evidence | Reconstruction boundary |
| --- | --- | --- |
| Poker | Five-card draw, holds, nine winning categories, collection, high-card double-up | Ordinary rules recovered; preserve the ten-or-better pair threshold and unusual pay table. |
| Blackjack | Player/dealer scoring, hit/stay, special awards, collection, two-card double-up | Ordinary rules recovered; substituting standard blackjack would change the game. |
| Card-chain | Five visible cards, suit/rank matching, replacement, chain rewards, hand bonuses, double-up | Ordinary rules recovered, with deck-exhaustion and retained-state defects requiring explicit treatment. |
| Horse racing | Eight runners, forecasts/odds, cash wagers, simulated finishes, payout dialog | Workflow recovered, including a normal-path cash-settlement defect. Exact race animation and runtime coercion remain separate fidelity checks. |

Selector `00994780` maps indices 0/1/2/4 to horse racing/poker/blackjack/card-chain;
3 closes the workflow and 5 buys coins. There is no recovered dice game,
slot machine or coin-to-cash redemption workflow. These are bounded negative
findings from the selector, associated forms and decoded state writers, not a
claim that arbitrary behavior can never be hidden in an external component.

The current C# project recognizes `FacilityKind.Casino`, but
`FacilityService.ServiceAvailable` excludes it. No playable casino service or
card/race state machine was found in `src`. Facility ownership is not evidence
that these activities have already been reconstructed.

## Common money, deck and lifecycle rules

Confirmed coin purchase adds **50 coins for 10,000,000 company cash**. The
financial helper receives the debit without a separate affordability check.
The three card-game entry paths require a positive coin balance. Each accepted
bet increment spends one coin immediately and increases the round bet by one,
up to 50; it is not a later debit at settlement. Horse racing uses cash instead.

Coin balance is `00A1112C`, original company member `0110`, explicitly saved and
loaded. Across the complete 669-procedure decoded inventory, its seven direct
global writers are purchase, three bet handlers and three payout timers.
Original member-based save/load/reset accesses are considered separately.
No discovered writer exchanges coins back into company funds.

Cards use suits 0..3 and ranks 0..12, with rank 0 = ace and rank 9 = ten.
The shared used-card map is `00A1500C[4,13]`. Selection draws
`Int(Rnd*4)` and `Int(Rnd*13)` repeatedly until an unused suit/rank appears,
then marks it used. This is rejection sampling, not a shuffled list. Removed
cards remain used during the round. Poker and card-chain command handlers also
make an unused random draw on entry, including unsuccessful command paths.
Blackjack generates future cards before they are displayed. Therefore a
replacement RNG or a fixed number of draws per visible card is not exact parity.
The external VB6 generator itself remains the separate M76 runtime boundary.

Shared transient cells include bet `00A14E4C`, phase `00A14E5C`, pending payout
`00A14EFC`, and card-chain bonus `00A14E3C`. Their corresponding company offsets
`3E30/3E40/3EE0/3E20` have no recovered original serialization access. Saving a
modern in-progress round is a new continuity policy, not an original save field.

The three unload handlers (`009783DC`, `00978274`, `009784CC`) call UI resume
and `00978364`, which writes company member `01CC = 1`. They do not refund the
bet, complete a payout, clear all shared state or redeem coins. Poker and
card-chain explicit close buttons unload their forms; blackjack has a close
branch in its command handler. The main simulation timer is paused while the
activity is open and resumed by unload.

| Reload | Explicit resets | Consequence established by code |
| --- | --- | --- |
| Poker `009960C4` | Phase and outcome, plus payout table | Bet/pending payout are not universally cleared; phase does not resume collection. |
| Blackjack `009951FC` | Bet, phase, timer, blink and lose/reset flag, plus payout table | Bet differs from poker/card-chain; pending payout and every win-related cell are not universally cleared. |
| Card-chain `0099B03C` | Phase and outcome, plus payout table | Bet, pending payout and accumulated bonus are not universally cleared. |

Coins already transferred by the payout timer remain credited on close. The
remaining pending amount is not automatically transferred, and reload resets
the phase rather than continuing settlement. Retained cells must not be
described as proof of a particular exploit: native initial control enabled
states and exact event timing still affect which next action is accessible.
An unconditional clean-round reset/refund would be a deliberate correction.

## Poker

Primary procedures: command `009B9DE4`, classifier `009C3564`, hold/selection
`00989E44`, timer `009D0374`.

The initial deal creates five unique cards and reveals them at nine-tick
intervals. During the first phase, selecting a card toggles its hold flag.
The next deal replaces each unheld card from the remaining used-card map;
held cards stay. The classifier tests increasingly strong categories so later
matches supersede earlier ones. A straight accepts A2345 through 9TJQK and
TJQKA, without other wraparound sequences.

| Outcome code | Hand | Total pending award / bet |
| --- | --- | ---: |
| 0 | No winning hand | 0 |
| 1 | Pair of tens, jacks, queens, kings or aces | 1 |
| 2 | Two pairs | 2 |
| 3 | Three of a kind | 3 |
| 4 | Straight | 10 |
| 5 | Flush | 15 |
| 6 | Full house | 20 |
| 7 | Four of a kind | 100 |
| 8 | Straight flush | 500 |
| 9 | Royal flush | 1000 |

These are total returns after the bet has already been spent, not profit plus
a separate returned stake. The table is stored in reverse outcome order;
timer `+11F8` computes `bet * table[9-outcome]`. A losing result clears phase
and bet. A win becomes pending; collection credits one coin every two timer
ticks until pending reaches zero, then clears phase and bet.

Double-up starts a fresh used-card map and deals five unique cards. Card 0 is
the revealed dealer card; the player selects a hidden card. Aces compare high
(rank 0 maps to 13). Higher wins double the pending amount, lower loses the
round, and equal rank starts another comparison deal without changing pending.
There is no recovered additional coin charge or fixed doubling limit. Losing
does not erase previously credited coins; the uncollected award ceases to be
an active payout. Remaining pending storage need not be zeroed by that branch.

## Blackjack

Primary procedures: command `009D1968`, scorer `009A7644`, double-up selection
`00988514`, timer `009D7BB8`.

Start generates **18 unique cards upfront**, nine for each side. Initial
presentation gives the player two cards and the dealer one. Number cards use
their face value, ten/face cards count ten, and aces initially count eleven
with repeated rescoring that downgrades marked aces to one when necessary.
The normal scorer downgrades every previously marked eleven-valued ace in the
retry pass, rather than finding the highest valid total by downgrading one at
a time. For example, A,A,9 becomes 11 after the retry, rather than 21. The
initial two-card scorer is separate and scores A,A as 12. Optimal modern ace
scoring would therefore be a correction, not an equivalent implementation.
The player's initial two-card 21 wins a 2x award before ordinary dealer
comparison; there is no recovered 3:2 natural payout or hidden-dealer natural
tie check on that branch.

Player bust loses. During dealer scoring, bust awards 2x, equal totals award
1x, and a dealer total already greater than the player wins immediately. Only
a dealer total below the player and at most 16 requests another card. A dealer
total below the player and at least 17 awards 2x. For example, dealer 15 versus
player 14 ends as a loss; dealer 15 versus player 15 ends as a tie. A generic
"dealer always draws to 17" rule would change this source behavior
(`009A7644 +06FB..07CD`).

Special awards are checked when the player requests **STAY**, before ordinary
dealer play, in this order. The control mapping is explicit: command index 1
is MORE and enters player-draw phase 3; index 2 is STAY and checks these awards
before entering dealer phase 8. The timer assigns those captions to indices
1/2 at `+0491..04F6`; do not infer actions from the handler branch order.

| Current player hand | Award / bet |
| --- | ---: |
| Five cards | 4 |
| Six cards | 6 |
| Seven cards | 20 |
| Exactly three sixes | 25 |
| At least eight cards | 100 |

They are stay-triggered awards, not an unconditional bonus automatically
applied on reaching the card count. Branches select the seven-entry table
`[1,2,4,6,20,25,100]`. Collection transfers one coin every two ticks.
No ordinary split, insurance or stake-doubling action was recovered.

Blackjack's double-up differs from poker: it generates five fresh unique cards,
then the player selects **two** cards. The second must rank higher than the
first to double pending; equal rank redeals, lower loses. Aces rank high.
The selection handler itself checks phase and index, not that the second index
differs from the first. Its UI hides/marks the first card. Whether native input
can select the same hidden card is a runtime reachability question; do not add
an alleged original distinct-index guard that the handler does not contain.

## Card-chain

Primary procedures: command `009C8A8C`, selection `0098EEF4`, bonus classifier
`009A7EB8`, timer `009E13B8`, keyboard `009846DC`.

Five unique cards are visible. The first choice is unrestricted. Each later
choice must match the suit or rank of the previous removed card. The timer
captures that reference, draws a fresh replacement into the selected slot,
increments the chain count, evaluates the new five-card hand and checks whether
another matching card remains. A chain ends when no matching card remains.
At ten or more successful replacements, the pending chain return is:

| Chain count | Multiplier | Chain count | Multiplier |
| --- | ---: | --- | ---: |
| 10–13 | 1 | 14–16 | 2 |
| 17–19 | 3 | 20–22 | 4 |
| 23–25 | 6 | 26–27 | 8 |
| 28–29 | 10 | 30–31 | 13 |
| 32–33 | 17 | 34–35 | 20 |
| 36–37 | 25 | 38–39 | 30 |
| 40–41 | 40 | 42–43 | 50 |
| 44–45 | 55 | 46–47 | 60 |
| 48–49 | 70 | 50–51 | 80 |
| 52+ | 100 | | |

After each replacement, a flush adds 1x bet to accumulated bonus, a full house
2x, four of a kind 5x, and a straight flush 20x. Only the strongest applicable
category is awarded for that evaluation. Royal flush is included in straight
flush, without a separate royal branch. Bonuses accumulate across evaluations;
they are not one-time achievements. Virtual call `+0921` is the visual helper,
while `+09C4` is the bonus classifier: this is one bonus evaluation, not two.

On chain end, an award exists if count >=10 **or bonus >0**. For count >=10,
pending is assigned bet times the range multiplier, then accumulated bonus is
added. For count <10 with a positive bonus, none of those range assignments
executes before `pending += bonus` (`+0A57..0F3C`). Thus the code relies on
whatever pending value already exists. A blanket fresh-state assumption would
hide this retained-state defect. Collection transfers ten coins per two ticks
while pending >=150, otherwise one. Double-up uses poker's revealed dealer
card and high-card comparison, including ace-high and tie redeals.

**Deck exhaustion:** the ordinary start deals five cards and each successful
replacement consumes one more unique card, with no ordinary chain recycling.
After 47 replacements all 52 cards are marked used. If play requests the next
replacement, timer `+025B..02DA` retries used draws via `+0593 -> +025B` without
an exhaustion exit. Under that state the loop cannot complete. The 48–49,
50–51 and 52+ payout bands exist in the table, but this ordinary path does not
prove they are reachable. Adding a reshuffle, discard recycling, or a terminal
deck award is a correction/design choice requiring an explicit policy.

**Keyboard/copy leftovers:** form event slot 16 is a keyboard handler, not a
query-unload safeguard. Keys 1..5 call the card-selection handler; Q/W directly
call command indices 0/1 through virtual slot `0700`. The command handler still
contains a phase-1 replacement path copied from poker. Disabled button
presentation alone is insufficient to classify that path as dead, since the
keyboard calls the handler directly. Exact key delivery and resulting native
presentation need comparison; these leftovers do not establish a separate
extra casino game.

## Horse racing

Primary procedures: race-card setup `009D46D0`, start `0099061C`, betting timer
`009E6148`, race timer `009EB860`, result timer `009A12F8`, payout accept
`009850B4` on `fKeibaModosi`.

The selector copies company cash `00A1103C` into temporary balance `00A13B90`.
The race uses eight unique runners and eight unique jockey selections. Three
forecast rows derive from runner strength plus `Int(Rnd*3)`; forecast rank
counts other scores at least as high, so ties are not silently broken by a
modern stable sort. First-forecast ranks map to win odds
`[2.1,4.5,5.7,7.5,8.9,11,28.6,47.7]`. Pair odds use the two win odds' product
divided by 2.4. The pair-selection display omits the selected first horse from
its seven second-horse columns.

Holding add/remove controls changes wagers by 100 cash per betting-timer tick,
against the temporary balance. The add guard tests balance >0, rather than
balance >=100. The single-wager path clamps stake at 99,900 after adding but
still performs the cash debit; do not substitute an assumed atomic cap check.
Pair wagers use a two-dimensional selected-runner matrix. Its compressed
display-odds indexing and payout indexing are not interchangeable: payout reads
the finish indices directly from the odds matrix. This needs deliberate
treatment rather than normalizing the matrix invisibly.

Start writes the temporary cash balance into company funds (`0099061C +0011`),
committing the bets. Race performance is runner strength /1.2, with successive
random tests: `Int(Rnd*12)==1` adds 6; otherwise a second test equal to 2 adds
4; otherwise add `-2 + Int(Rnd*6)`. Initial distance is 40. The race timer uses
piecewise performance adjustments, a random mode selected at tick 45, and a
later race phase beginning at tick 200. Finish registration stores runner
indices in order as they cross the recovered threshold; simultaneous crossings
follow the loop's runner visitation order. It is not a one-draw weighted winner.
The full camera/speed recurrence is preserved by the procedure fingerprint,
but is not claimed as a completed portable race-physics specification here.

The result timer opens a modal payout if a winning single or ordered first/second
pair wager is positive. Payout accept adds single stake times win odds and
pair stake times odds formatted with a one-decimal template to **temporary
cash**, using VB6 numeric coercion. Do not assume integer flooring or an
unordered pair return. Exact locale/coercion remains a runtime fidelity check.

**Settlement defect:** after modal return, result timer `+03FC` calls race-card
setup. Setup clears temporary cash (`+00D9`) and later copies company funds
back over it (`+02A7..02AA`). Setup's intervening `0700` call targets the
pair-selection handler `009CC3E0`, not Start; it does not commit winnings.
Payout accept has no company-funds writer, and Start is the cash commit in this
workflow. Thus the normal result-to-next-card path discards the credited
temporary winnings. The evidence supports an original broken settlement path,
not permission to describe a repaired cash return as unchanged parity.
Closing before Start does not commit draft wagers; closing after Start does
not refund the committed stakes. Horse unload uses the common resume helper.

There is also a conditional own-horse/special-race mode. No ordinary casino
entry assignment enabling that mode was recovered. It is not sufficient
evidence for implementing a horse-breeding or horse-management subsystem.

## Reproducibility and implementation gate

Indirect calls were resolved through each form's **event-link array** and its
native thunk's CodeInfo target, not by treating the metadata procedure ordinal
as the virtual slot. For example, horse slot `0700` resolves to `009CC3E0`,
while card-chain slot `0700` resolves to `009C8A8C`. The supplied evidence
checker verifies those targets, reviewed procedure fingerprints and selected
numeric/branch/global anchors without executing the original or printing its
content. It complements the M76 payout-table checker:

```sh
python3 tools/verify_casino_evidence.py /local/path/GameDev2DX.exe
python3 tools/verify_workflow_lifecycle_evidence.py /local/path/GameDev2DX.exe
```

Before implementation, keep four explicit boundaries:

1. Implement the recovered games and their unusual ordinary rules; omit the
   unsupported dice/slots/redemption/standard-blackjack additions.
2. Label corrections to exhausted decks, horse winnings, stale pending state,
   cap handling and shortcut leftovers. Do not quietly replace observed code
   with assumed intent.
3. Define modern close/save behavior explicitly. The original saves coins,
   not a complete resumable card round, and unload is not an automatic refund.
4. Keep exact VB6 RNG, serialized control defaults, native timer/modal/input
   comparison, horse camera/speed recurrence and locale coercion as bounded
   fidelity work. None justifies inventing another activity.

Static discovery now establishes the ordinary casino inventory and rules.
This report does not claim a Windows playthrough, engine tests, original-save
fixture validation, or completed casino implementation. Casino remains outside
the current supported Early Access scope until implementation and validation.
