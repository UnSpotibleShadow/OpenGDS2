# M14: release reviews and sales milestones

M14 connects the original magazine scoring rules to own-game shipment. Four
scores appear before the sales screen. The ceremony pauses the office clock.
Results and pending presentation survive save/load; acknowledgement records Hall
of Fame status and increments the company counter once. Older modern saves keep
unknown reviews/counters nullable; loading does not invent or reroll past reviews.
Controls are bilingual. Review comments are newly authored short assessments;
original review text and graphics are not distributed.

## Verified static evidence

Addresses refer to the supported executable in source-hashes.txt. These are
bounded static/data-flow findings, not an original Windows runtime comparison.

| Source | Behavior |
| --- | --- |
| fGMSyukka 009C9CB8 +1240..127F | Modal reviews follow shipment; Hall of Fame flag copied to latest product A12F58, then scratch flag cleared |
| fGMCrossR 009CAFCC +065F..0706 | Sum the four development quality stats, multiply by latest product market factor and .7 + type[12EC]/10 |
| Same +070D..09F2 | Score thresholds, random replacement and upper clamp |
| Same +01D8..03EF | Total ≥32 qualifies for Hall of Fame; remaining assessment boundaries 24 and 12 |
| fGMCrossRDen 00986780 +0124..0130 | Company member 100 increments by one |
| Initializer 009F651C +0066..006B | Company member 100 starts at zero |
| Module2 009E3830 +1121..1241 | Ordered cumulative-sales thresholds 100k, 250k, 500k, 1m gated by A12ECC |
| fGMOuri 009B4F34 +014B/+0288/+05E2/+093C | Announcement advances product milestone counter to 1/2/3/4 |

Bugs and recognition are absent from the review basis. Reviews do not supply an
extra sales multiplier or a cash reward in this recovered branch.

| Review basis | Initial score | Replacement when market factor ≥2 |
| --- | --- | --- |
| <130 | 1 + floor(Rnd×3) | None |
| 130–<200 | 2 + floor(Rnd×3) | None |
| 200–<250 | 3 + floor(Rnd×3) | None |
| 250–<300 | 4 + floor(Rnd×3) | None |
| 300–<350 | 5 + floor(Rnd×3) | None |
| 350–<400 | 6 + floor(Rnd×3) | None |
| 400–<450 | 7 + floor(Rnd×2) | 7 + floor(Rnd×3) |
| 450–<800 | 8 + floor(Rnd×2) | 7 + floor(Rnd×3) |
| ≥800 | 8 + floor(Rnd×3) | 7 + floor(Rnd×4) |

The initial draw is consumed even when replaced. The replacement can lower the
score. The port preserves this rule. Scores are capped at 10. Four reviewer draws
are independent; cosmetic animation and random comment draws are omitted, so RNG
seed equivalence is not claimed. The modern UI presents the completed results
immediately rather than replaying the original timer-driven reveal.

Sales can cross several thresholds in one batch. Pending notifications preserve
order and pause subsequent ticks until acknowledged. Acknowledgement advances
only the product counter; no invented bonus is granted. The original company
record dates and sales chart history remain outside this checkpoint because the
complete calendar is not integrated. Hall of Fame marks sequel eligibility;
sequel development itself is still unimplemented.

## Recall investigation: traced, not yet integrated

After demand decay, Module2 +12FD tests demand <16 and recall-state A12F20 =0,
then sets state 1 even if no investigation occurs. Its condition evaluates three
random draws (VB And/Or are not short-circuit): bugs=0 and floor(Rnd×317)=1;
1≤bugs≤10 and floor(Rnd×25×bugs)=1; or bugs≥11 and
floor(Rnd×(50−bugs))≤5. The last range can be zero or negative; substituting
Random.Next with a negative bound would fail to reproduce it.

A successful test opens meeting mode 11. fGMKikakuKaigi 009EFA40 +38A0..38AD
sets the product countdown to 5. Weekly routine 009DB4F4 +1496..1538 checks
state 1, decrements a positive countdown and opens mode 12 at zero. Mode 12
+3A62..4136 draws floor(Rnd×4): 0 refunds (units sold + remaining stock) × price;
1/2 refunds that quantity ×1000; 3 dismisses the case. Refund branches set state
2 and stock zero; dismissal sets state 3. There is no immediate refund in mode 11.
Scheduling remains disabled until the weekly economics and event pipeline are
reconstructed. Bankruptcy/negative funds after a refund need further tracing.

## Research correction and limits

A1301C in Module2 +1090..10AF is indexed by product and column zero. The earlier
M13 phrase “genre experience credited by sales” was incorrect; it has been removed.
The weekly clock reaches 100×timing-factor ticks (factor 2 in Module2 +0005),
but calendar economics remain disabled. Monthly payroll reads BDC using shared
scratch member 3018, whose actual state depends on earlier employee/event paths;
see milestone-8.md. No intended per-employee wage formula is substituted.

Automated checks cover all strict score boundaries, maximum scores, replacement
draw consumption, excluded stats, Hall of Fame idempotence, pending save/load,
legacy unknowns, threshold crossing, announcement order and paused clocks. The
Godot smoke flow exercises release, review save/load, acknowledgement, sales,
reprinting, a second project and four consecutive milestone dialogs.
