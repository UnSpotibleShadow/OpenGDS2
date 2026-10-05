# M48: sequel development

M48 connects the original mode-1 development workflow to the desk-room chooser.
Open a free room, choose Sequel, select a Hall of Fame predecessor, and continue
through the existing hardware/audience quote and meeting. Development inherits
its type and genre automatically, then uses ordinary debugging, shipment,
reviews, production and sales. Controls and notices support English/Japanese.
The original LZH remains required for normal play; public tests use authored
synthetic catalogs, employees and a flat office BMP generated at test time.

## Verified static behavior

Addresses refer to the supported extracted executable in source-hashes.txt.
These are targeted P-Code/data-flow findings; original Windows runtime parity
has not been established. Original dialogue, catalog records and procedure dumps
stay private. The public verifier contains only bounded structural anchors.

| Source | Finding |
| --- | --- |
| fGMKaihatuS 0097C26C | Room chooser selects original mode 1 and opens predecessor selection |
| fGMZokuhen 009A45D4 +0132..01A2 | Scan own products newest first; require A12FE4=0 (no shipped sequel) and A12F58=1 (Hall of Fame) |
| fGMZokuhen 009993B0 +0000..000D | Selected row resolves through roster A1411C into predecessor A14024 |
| fGMZokuhen 00988878 +0000..0088 | Reject a predecessor already used by a mode-1 project in either occupied room |
| fGMCrossR 009CAFCC +01E3..0251 and shipment +1255..1279 | Four reviews totaling at least 32 produce the Hall of Fame eligibility flag |
| fGMKikakusyo 0099BA80 +001E..0034/+009B..014E | Charge the ordinary quote once; floor 0.3 times each predecessor Fun, Novelty, Music, Graphics and Recognition value |
| Same +0249..0257/+02FD..0330/+0342..036F | Start with zero data/bugs; current selected hardware/audience; type/genre initially unset; save mode and predecessor |
| Module2 00A0283C +A6DB..A7DD | At the ordinary 50% checkpoint, copy predecessor type A12D28 into room A12920 and add A12324[type] directly to required points; stage becomes 1 |
| Same +AA75..AB79 | At the ordinary 81% checkpoint, copy predecessor genre A12D0C into room A12904 and add A12224[genre] directly; stage becomes 2 |
| Same checkpoint flow +A858..ABF2 | Re-evaluate subsequent thresholds in the same employee pass after changing required points; existing 10/20/40 checkpoint experience applies |
| fGMSyukka 009C9CB8 +02F9..0429 | Copy mode/predecessor; mark predecessor A12FE4=1; copy series base title A13000; increment fresh product A12FC8 |
| fGMSyukka 009B4344 +040A..0461 | Default title concatenates the predecessor's base-series title with predecessor counter +2 |
| Module1 reset 009DD234 +1511..151F; declaration 0061BE88 | Counter member 1FAC is a fixed 513-cell VARIANT array, cleared to Empty (numeric zero) at new-company reset |

Type points use the predecessor's source directly. Ordinary original-game type
selection's second roster lookup, selection fees, company-fame/3 addition and
genre audience-affinity Fun+20 are absent from the sequel branches. The hardware
and audience quote has the same cost/points for modes 0 and 1. No sequel-only
market-factor bonus occurs in the shipment branch; the manga mode-2 bonus is a
separate unsupported path. Technologies and normal work remain unchanged.

Selecting or cancelling a predecessor does not spend money or consume it.
Accepting a plan charges its quote, but only successful shipment consumes the
parent. A released child must itself enter the Hall of Fame to continue the
series. A poorly reviewed sequel ends that chain without restoring its parent.
The original candidate list can include an active predecessor, with confirmation
rejecting it; the port retains that distinction and explains the guard.

## Counter quirk and modern boundaries

The shipment code increments the fresh product's cleared counter. It does not
copy or increment its parent's counter. New original games have counter 0;
shipped sequels have counter 1. Thus the first suggested suffix is 2, later ones
can repeat 3. Static scans of both implicit global references and Module1 member
1FAC found only shipment, reset and original save/load accesses. The port keeps
this arithmetic; players can edit the release title. Existing modern duplicate
release-title validation remains. Sequential episode numbering would be a
separate modernization, not verified compatibility behavior.

Modern saves persist predecessor index, mode, consumed flag, counter and series
base title. Validation rejects dangling/cyclic lineage, duplicate children,
inconsistent type/genre/base-title inheritance and duplicate active sequels.
Pre-M48 modern saves could create only original games, so absent lineage fields
have known zero/null defaults; a missing root title falls back to its release
title. Loading never rerolls reviews or invents Hall of Fame status.
Original save compatibility is still unresolved.

The compact predecessor table and inheritance notices are authored presentation.
Exact original selection-pane layout, checkpoint conversation timing, meeting
animation, release dialogue and RNG stream equivalence remain unverified or
unimplemented. Automatic sequel checkpoints do not introduce new decision
pauses; the port still pauses for the already-supported debugging decision.
Manga adaptation, polishing and external/contract workflows remain unavailable.

## Validation

Seventeen added public domain cases cover eligibility/order, exact flooring,
quote/transaction guards, two-room conflicts, both automatic checkpoints,
threshold recalculation, experience, release failures, successful and failed
series, counter behavior, legacy saves and corrupt lineage/data. The public Godot
suite exercises selection/cancel, quote/meeting, both inherited checkpoints,
save/reload, shipment, reviews and sales through actual UI controls. The private
original-asset UI suite also checks the opening no-candidate chooser.

Run `python3 tools/verify_sequel_evidence.py /path/to/GameDev2DX.exe` locally, or
set `OPENGDS2_ORIGINAL_EXE` for the opt-in Python evidence test. Original files
and extracted data are never needed or included in public CI. Native SDK/export
results are recorded in development-checkpoint.md after verification.
