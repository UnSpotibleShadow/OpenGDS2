# Original backup-device workflow discovered during mapping

The original Actions menu contains `mKoudouBackupK` (serialized menu ordinal 40),
bound to `MDIForm1` procedure `0098B1F4`. This is a gameplay purchase, distinct
from OpenGDS2's modern save-file `.bak` recovery. No matching purchase/ownership
service or menu route existed at baseline
`2eb7c0bf7de32b33b76e0bb1dc80840f406188e1`.

The supported original executable is pinned by the
[map inventory](original-game-inventory.json). Its procedure fingerprints and
all operand reference sites can be rebuilt with `tools/map_original_game.py`.
No original text, executable bytes or full traces are included here.

| Source location | Recovered meaning |
| --- | --- |
| `0098B1F4 +0000..000B` | Reads ownership cell `00A112E8`; already-owned branch gives a secretary response and does not repurchase. |
| `+0060..00D4` | Secretary explanation, purchase confirmation helper `00981104`, and confirmation-result gate. |
| `+00D7..00E5` | Compares funds cell `00A1103C` against `100000000`; insufficient funds takes the refusal branch. |
| `+0189..0191` | Stores ownership `1` to `00A112E8`. |
| `+0192..019D` | Passes `-100000000` to shared financial helper `0097DD04`. |
| `Module2 009E3830 +1CD1..1CDC` | Virus-event path reads the same ownership cell, compares against zero and branches to `+2280` when owned. |
| `+2280..22C4` | Protected branch reports that the previously purchased backup device prevented the serious outcome. The implemented protected branch retains project/stress and presents per-staff reactions. |
| `+274F..275A` | The same event sequence checks ownership again before its protected animation/result path. |

Fresh reference indexing finds exactly four `ImpAd` reference sites for
`00A112E8`: two in the purchase handler and two in `009E3830`. This narrowly
describes the mechanically indexed references; the correlated Module1 member 02CC reset/save/load accesses are now verified
separately. Other receiver aliases still require independent evidence.

The purchase and virus workflow are now implemented in the
[map-backlog follow-up](map-backlog-implementation.md). The reset, native
save/load fields, eager room draws, per-staff stress/reaction ordering, final
project-state write and backup animation are traced and regression-tested.
Modern JSON continuations preserve unread messages and resume the next room
without repeating company work. This is distinct from native GD2 interoperability.

Tracked as `W35` and `G11` in the [map catalog](original-game-workflows.json).
The adaptation comparison is present. Original runtime acceptance remains pending;
buying the device is not evidence that it prevents every other failure.
