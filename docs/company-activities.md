# M16: public relations, genre research and production expansion

Company activities are now playable. Open **Company activities** from the office
and choose public relations, genre research or production expansion. All three
share the original cost and one-action budget. Public relations affects the fame
used when selecting a new project's type; genre research unlocks genre choices;
production expansion raises the maximum production quantity. Menu controls and
results are bilingual. Imported genre names remain local original data.

## Verified static behavior

| Source | Recovered behavior |
| --- | --- |
| Initializer 009F651C +010B..0113 | Shared activity cost member A0 =5,000,000 |
| MDIForm1 00981A78 / 00981C20 / 009819A4 | Three menus reject while member B0 =1, otherwise set selected activity and open fGMSeisanZ |
| fGMSeisanZ 0099ED68 +0000..0062 | Check funds before setting member B0 =1 |
| Same +0170..024B | Public relations: 1+floor(Rnd×3), fame capped at 100 |
| Same +0252..03F4 | Genre research: 2+floor(Rnd×3), research capped at 100; detect crossed genre thresholds |
| Same +0406..0565 | Production: floor(Rnd×12)=1 gives 17+floor(Rnd×8), otherwise 4+floor(Rnd×4); no 100 cap |
| Same +0569..0573 | Subtract the shared activity cost once |
| Module2 00A0283C +0098..0351 | At phase ≥80×2, an unused budget automatically expands production if Normal funds ≥300m or Beginner funds ≥100m; week routine called at phase ≥100×2 |
| Weekly routine 009DB4F4 +000B..0010 | Reset member B0 =0 |

Company member 90 (A110AC) is now identified as **genre research**, rather than
an unidentified eligibility value. It starts at 23. Genre member 125C is its
required threshold. Research does not directly improve employee stats or a
project's quality. The UI lists the currently available genres from this same
predicate, and development uses the updated threshold at genre selection.

Even an action at capped fame/research still costs the full amount and consumes
the budget. Production can rise beyond 100. Ordinary and rare expansion branches
consume the conditional second draw in source order. A successful manual action
prevents that budget's automatic production expansion.

## Clock scope and remaining calendar work

M16 persists the recovered 0..199 business-budget phase and used flag. Office
simulation advances this phase, applies the original automatic expansion and
resets the budget after 200 ticks. Menus, development decisions, reviews and sales
milestone dialogs pause it with the rest of the office. Older modern saves leave
cost/phase/flag unknown and cannot perform activities without established state.

This is the business-action portion of the original week, not the complete weekly
pipeline. The displayed company date remains at its opening value while payroll,
contracts, platform events, contests and other weekly work remain unintegrated.
The separate business phase is an explicit partial reconstruction; it must be
unified with the calendar phase when that pipeline is recovered. No weekly payroll
or event values are fabricated to make the phase advance. Original seed/global
RNG correspondence and Windows behavioral comparison remain outstanding.

## Validation

90 C# checks pass locally (88 fixture-free), including capped-but-paid actions,
shared budget guards, insufficient funds, missing legacy state, rare/ordinary
production ranges, Normal/Beginner automatic thresholds, exact budget reset,
save/load, genre threshold crossings and subsequent project recognition. The
synthetic Godot UI completes development/release/sales, reviews, all milestones,
dismissal, all three business actions and budget/save restoration with zero
failures. All 17 Python checks pass with private original evidence anchors.
