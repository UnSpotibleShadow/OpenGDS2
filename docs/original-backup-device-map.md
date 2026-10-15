# Original backup-device workflow discovered during mapping

The original Actions menu contains `mKoudouBackupK` (serialized menu ordinal 40),
bound to `MDIForm1` procedure `0098B1F4`. This is a gameplay purchase, distinct
from OpenGDS2's modern save-file `.bak` recovery. No matching purchase/ownership
service or menu route was found in the current adaptation at baseline
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
| `+2280..22C4` | Protected branch reports that the previously purchased backup device prevented the serious outcome. Following employee/result handling still needs a complete mutation audit. |
| `+274F..275A` | The same event sequence checks ownership again before its protected animation/result path. |

Fresh reference indexing finds exactly four `ImpAd` reference sites for
`00A112E8`: two in the purchase handler and two in `009E3830`. This narrowly
describes the mechanically indexed references; receiver aliases, startup default
state and ordered native save persistence still need investigation.

The next implementation task must trace virus eligibility/random draws, both
branches' exact project/employee mutations, animation/callback ordering, ownership
initialization and persistence. Then add purchase, declined/poor/already-owned
cases and protected/unprotected event fixtures. Do not infer that buying this
device prevents every failure or that a modern save backup implements its effect.

Tracked as `W35` and `G11` in the [map catalog](original-game-workflows.json).
This mapping change adds no gameplay behavior. Original runtime verification is
pending.
