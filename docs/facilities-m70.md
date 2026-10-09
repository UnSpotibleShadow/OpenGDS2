# Facility management — M70

The company facility screen now connects the existing school and four additional
services: staff hot spring, stock recycling, technology-capacity study and the
production laboratory. It shows all nine facility types and their verified
construction prices. Purchases for the casino, lawyer, mystery laboratory and
hardware laboratory remain disabled until their dependent systems are recovered.
Previously saved occupied facilities can still be inspected and closed.

Construction is immediate, unique by type across the three plots, and subtracts
cash directly without financial accumulator changes or an invented rank gate.
Closure requires confirmation, restores the vacancy artwork and gives no refund.
The original affordability check precedes the duplicate-type check. The modern
service additionally rejects an occupied target plot and outstanding decisions.
The original LZH remains required for the artwork. No original strings, assets,
executables, saves or full research traces are published.

## Verified transactions

| Type | Construction | Service behavior | Original source |
| --- | ---: | --- | --- |
| School (0) | ¥600,000,000 | Existing annual graduates | Initializer `009F651C +9958..9985`; existing M37 |
| Staff hot spring (1) | ¥60,000,000 | ¥8,000,000; all employed stress becomes zero; signed financial accumulator | `0099E164`; service fee `009F651C +0150..0158` |
| Recycling (2) | ¥80,000,000 | Dispose all released-game stock and receive ¥500 per copy; signed financial accumulator | `0098DDC4 +0009..0056`, `+00EF..0135`, `+01B8..01D3` |
| Technology school (3) | ¥100,000,000 | ¥40,000,000; one selected employee gains one maximum technology; signed financial accumulator | `009881C0 +0000..009F`; fee `009F651C +015E..0166` |
| Casino (4) | ¥500,000,000 | Still disabled | Initializer `+9A30..9A5D` |
| Lawyer (5) | ¥250,000,000 | Still disabled | Initializer `+9A66..9A93` |
| Mystery lab (6) | ¥300,000,000 | Still disabled | Initializer `+9A9C..9AC9` |
| Production lab (7) | ¥150,000,000 | ¥10,000,000 direct cash debit; capacity gains `15 + floor(Rnd × 5)`; no weekly activity flag | `00988DAC +0093..014A` |
| Hardware lab (8) | ¥1,000,000,000 | Still disabled | Initializer `+9B08..9B35` |

The production-lab handler has no affordability test: confirmed expansion can
make cash negative. It does not consume the ordinary company's once-per-week
business action. The UI explicitly describes this payment behavior.
Technology study changes maximum capacity, not experience, existing technology
levels or desk assignments; it has no invented eight-technology cap. Both the
employed copy and canonical recruitment source are updated. Opening the original
selector scans gate-1 sources in ascending order and writes their count to the
shared payroll scratch cell (`0099F9A4 +02B4..031F`). A partial modern source table
cannot establish this count, so the selector requires the full known table.

The common construction handler is `00988F78`: selected-type price comparison,
three-slot uniqueness scan, direct cash debit at `+0123..0134`, slot write at
`+0138..0143`. All nine closure handlers have been inspected; confirmation writes
−1 to the selected slot, without a refund or clearing other facility effects.
`tools/verify_facility_evidence.py` checks the supported executable hash, handler
fingerprints, nine prices, five service fees, member mappings and transaction
anchors locally without printing original text.

## Save, UI and plot selection

Each service commits once and then stores an additive `PendingFacilityReceipt`
snapshot. The authored English/Japanese result pauses simulation and other
management actions until acknowledged. Save/load resumes the same result without
another charge, stock sale, capacity increase or random draw. Legacy saves lacking
the new field load with no pending result. Checked arithmetic is resolved before
cash, stock or staff mutations. Malformed result snapshots and out-of-range saved
facility types are rejected.

The narrow overlap of plots 0 and 1 now offers both plots through a small chooser.
`TenantSlotsAt` preserves the original independent strict hit tests and Single
coordinate narrowing; it does not invent first-slot priority. The chooser is a
modern UI adaptation. The original sequence of busy-window toggles is still
unverified. Occupied artwork refreshes on construction and closure.

## Remaining facility work

- Casino: `fGMTenantCasinoSn` exposes poker, blackjack and dice/coin games plus coin
  purchase. The selector includes a 50-coin / ¥10,000,000 transaction at
  `00994780 +029E..0323`; the three game forms and their payout/coin-state lifecycles
  must be reconstructed before exposing a paid facility.
- Lawyer: both consultation buttons (`00987810`, `00987678`) check the same
  ¥15,000,000 fee and dispatch original meeting modes 14 and 13. Existing M26
  `FeeNegotiationRules` recover outcome branches only; consultation payment,
  meeting timing, dialogue and saved result application still need integration.
- Mystery lab: `00987B40` starts a pending state, selects `floor(Rnd × 7)` and
  directly debits ¥25,000,000. Recovery must include its delayed dispatcher,
  seven outcomes and cancellation/closure interactions before charging players.
- Hardware lab: `009864AC` opens `fGMTenantHardS`; acceptance `00990B70` writes a
  development room's own-hardware mode and different project fields. This needs
  its project lifecycle, technology requirements, costs, launch/market behavior
  and one-hardware restriction, beyond ordinary game/sequel/manga development.
- Original hot-spring reaction dialogues, service animations and the exact shared
  VB random stream are not restored by this transaction milestone. Results use
  authored bilingual summaries; no original automatic presenter cadence is claimed.

## Validation

326 synthetic domain regressions pass, including 13 new facility tests for all
nine prices/three plots, construction/closure, rejection paths, checked arithmetic,
recycling stock/history, staff/source capacity, selector ordering/scratch, Single
production draw boundaries, negative funds, pending decisions, malformed saves and
overlap candidates. App compilation and English/Japanese headless UI checks pass.
The UI exercises confirmation decline, exact transactions, saved result reloads,
modal toolbar guards, closure and hot-spring repurchase.

Local UI validation uses the host engine and partial source restoration; optional
title audio is absent locally. The broader core-loop UI suite stops at its title-audio assertion before gameplay, so this run is not counted as passing. No native Windows validation, desktop package,
GitHub Actions run or public release is claimed. Track continued work in
[backlog #55](https://github.com/UnSpotibleShadow/OpenGDS2/issues/55).

M71 follow-up: lawyer construction and both fee consultations are now enabled.
See [lawyer consultations](lawyer-facility-m71.md) for the recovered entry,
affordability-only behavior and saved result sequence. The M70 unresolved notes
above describe the earlier milestone.
