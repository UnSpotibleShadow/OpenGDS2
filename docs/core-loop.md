# M13: development → release → sales

M13 makes the own-game, new-project loop playable in Godot. Import your authorized
original archive again to recover the newly added production tables. Start a new
company, hire staff, use **Staff → Assign Desk**, and begin development in that
room. Choose a type at 50% and a genre at 81%. Completed projects can ship with
bugs or continue debugging. Release sets title, price and production quantity.
The sales screen tracks inventory and net receipts and supports further production.
Staff stay at their desks, so the freed room can develop another game.

Older modern saves still load. Missing original roster/production/fee fields stay
unknown; they are not filled with invented defaults. Use a new company for M13.
English and Japanese controls are supplied; imported original catalog names stay
local and retain their original text.

## Evidence and scope

All addresses below refer to the executable hash in `source-hashes.txt`. They
are targeted static/data-flow findings, checked against local bytes and import
parity. We have not yet compared a complete cycle in the original Windows runtime.
`tools/verify_core_loop_evidence.py` checks bounded anchors without distributing
original procedures or text.

| Source | Recovered behavior |
| --- | --- |
| Module2 00A0283C +47A2..6689 | Seated own-game work eligibility, stress, breaks, idle threshold, category, tier and experience |
| Same procedure +8907..9C47 | Small/medium completion at animation tick 16; burst accumulation begins at tick 32 and completes after 120 accumulation ticks |
| Same procedure +A506..AE11 | 50% type, 81% genre, full-progress completion; triggering employee receives 10/20/40 experience |
| fGMKansei 00982A24 | Bugs ≥60 produce a warning; they do not prohibit shipment |
| fGMSyukka 009C9CB8 | Duplicate-title/funds checks, production charge, product snapshot, demand 100, market factor, inventory, room release |
| fGMSyukka 009B4344 +0160..0213/+0803..0824 | Floor data size, select hardware size band, production unit cost from table 111C plus 200 packaging |
| fGMSyukkaSai 00984BB0 | Reproduction checks funds, charges production cost and adds copies to stock; cost uses saved size band |
| Module2 009E3830 +0948..142D | Twenty-tick own-product sales, inventory, net receipts and demand decay |
| Initializer 009F651C +00F5..0105/+01A1..01B1 | Production capacity 50, company member 90 =23, per-copy fee cells 0860/0870 =1500 each |

The original working clock uses 0.04-second ticks in this reconstruction. Sales
run every 20 ticks. Menus pause the clock; project checkpoints pause all rooms and
sales until a selection is made. Modern RNG draws do not reproduce the VB6 seed
or complete global draw sequence. Cosmetic example-title draws, unrelated events
and sound calls are outside this loop.

## Development scheduling

For ordinary work, idle ticks/2 must reach `10 + (150 − 1.5 × speed)/10`.
Existing M11 focus/category/tier formulas apply. Successful classification adds
3.7 experience, capped at the current-level threshold. Ordinary stages add
`(floor(Rnd×3)+1)/60` stress per tick, capped at 100. A zero `floor(Rnd×130)` idle
break draw removes 2 stress; break variants last 50/80/100 animation ticks.
At stress 100 the original unclassified tier zero can stall an animation. Move
exhausted employees to the lounge, then return them to a desk; this build does not
invent automatic rest assignments.

Stage-three debugging waits for idle ticks/2 to reach `10+(100−speed)/10`,
then starts on zero `floor(Rnd×50)` when bugs remain. Its stress increment is
`(floor(Rnd×3)+1)/20`; accuracy/inspiration classify small or medium debugging.
These remove one or two bugs, without data-size or quality gains. Shipment remains
available before bugs reach zero.

The shared type roster has a 65-cell VARIANT SAFEARRAY declaration at 0061D580,
lower bound zero and no initialized data pointer. Fresh allocation yields Empty
cells, numerically zero. M13 initializes fresh company sessions accordingly and
persists overwritten prefixes and untouched tails across selections. It preserves
the original double-index points lookup and duplicate-permitting fallback. This
does not establish original-save import or same-process original company-restart
semantics; those remain unresolved.

M33 verifies the work-counter control flow: ordinary work bypasses the first
increment through `+66D8`, tests contributions, then increments at
`+9C15..9C43`. Its existing timing is retained. Eligible technology tier 4
instead passes through both increments. See
[technology application cadence](technology-application.md#work-cadence)
for the corrected 11/10 eligible-pass schedule and its bounded evidence.

## Release and production

The source defaults are ¥5,800 and 5,000 copies. The quantity spinner has a
100-copy floor and production-capacity×100 ceiling. Floors of data size select the
first matching one of the first three hardware thresholds; otherwise band three.
The unit production charge is that band's cost plus 200. Those four hardware
records are imported locally; hardware member 0F70 is not this production cost.

Own-game market factor starts at 1. A roll in 0..19 adds 1.5 for 0, .5 for 1/2,
−.5 for 3/4/5, otherwise zero. The first two releases subtract .1. Subsequent
releases have the recovered novelty/fun/music random bonuses. The music >100
branch shadows the later >150/>200 branches; M13 preserves that source quirk.
A negative market factor becomes .1, while exact zero stays zero.

M13 copies quality, bugs, recognition, data size and selected catalog identifiers
into an independent product record and charges production once. Production
spending and net receipts are modern accounting projections without gameplay
bonuses. Completed room work animations are cancelled on release to prevent a
pending contribution entering a different modern project; exact original worker
cleanup at shipment still needs runtime verification. Assignment clears the three
work-animation cells explicitly written by the original assignment branch.
Modern title length, nonnegative price and checked arithmetic are input/storage
safeguards; the original price spinner can continue below zero.

## Sales

For stock>0 and demand>0, start with:

```
base = floor((1.6×Fun + 1.2×Novelty + .8×Music + .7×Graphics − Bugs)/2
             × demand²/800) + floor(Rnd×20)
```

Multiply in source order by `1+hardware[1050]/80`, `1+recognition/40`, market
factor, audience factor, `.7+type[12EC]/10`, and the price factor. Then floor the
result and cap it against stock. New own-game plans set member 16D4 to zero; its
1.2 sales multiplier is therefore inactive in this supported branch.

| Audience identifier | Factor |
| --- | --- |
| 1 / 2 / 3 / 4 / 5 / 6 / 7 | 1 / .9 / .8 / .75 / .7 / .6 / .5 |

| Price | Factor |
| --- | --- |
| <3000 | 2 |
| 3000–3999 | 1.5 |
| 4000–4999 | 1.2 |
| 5000–5999 | 1 |
| 6000–6999 | .7 |
| 7000–7999 | .5 |
| 8000–8999 | .2 |
| 9000–9999 | .1 |
| 10000–19999 | .05 |
| ≥20000 | 0 |

Stock decreases and cumulative units increase by the result. Company receipts
are `(price − fee0860 − fee0870) × units`. Default ¥5,800 yields ¥2,800 per copy,
before the separately charged production cost. Every product's demand evolves
even when out of stock: zero `floor(Rnd×50)` leaves it unchanged; otherwise zero
`floor(Rnd×7)` adds 2, or subtract 2. Clamp demand at zero.

The source has no lower sales-result clamp. A sufficiently poor, buggy game can
produce negative units/receipts and increasing stock. M13 retains this defect;
regression tests cover it instead of silently changing the formula.

## Still unresolved or outside M13

- Full calendar economics, payroll defect verification, renewal/event integration.
- Polishing, sequels, external studios/jobs and their shipping modifiers.
- Contests/awards and full company progression. M14 adds reviews and sales
  milestone dialogs; M21 adds investigations and recall outcomes. See
  reviews-and-events.md and consumer-recalls.md.
- M16 connects company member 90/genre research; see company-activities.md.
  The earlier claim that sales
  credited genre experience was incorrect: A1301C is indexed by product. Its
  thirteen-cell weekly history and graph are implemented by M20; see sales-history.md.
- Complete meeting, idle-break and burst icon/animation presentation.
- Original save compatibility, seed-identical RNG and Windows behavioral comparison.

M19 adds the shared weekly production allowance. First shipments set the flag;
reprints wait for the next 200-tick budget reset. See [production-budget.md](production-budget.md).

The date display remains at the opening date while this isolated work/sales clock
runs. M13 is a playable core loop, not a claim that the whole game is complete.

M23 verifies the complete fixture against extracted Linux and native Windows
desktop packages. M24 fixes the modern office Pause/Resume control: its label and
status update immediately, and screen rebuilds retain the correct label. Manual
pause freezes the weekly budget and sales clocks; resuming restarts them. These
controls are runtime conveniences, separate from original simulation formulas.

The public setup screen asks for both company and president names. The former
embedded original default personal-name literal has been removed; its local-import
recovery remains unresolved rather than substituting an invented original name.
