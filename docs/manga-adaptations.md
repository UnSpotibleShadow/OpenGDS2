# Manga/comic adaptations — M49

A free development room now offers **Manga / Comic Adaptation**. Reimport the
required original LZH to add its 31 adaptation records to the local catalog.
Choose a title, read the publisher discussion, agree to the rights, then select
hardware/audience and continue through the planning meeting and proposal.
Development, type/genre selection, debugging, release, reviews and sales use the
ordinary own-game loop. A released adaptation can qualify for a normal sequel.

## Verified rules

| Evidence in the supported executable | Recovered behavior |
| --- | --- |
| Initializer `009F651C +A481`, 31 calls spaced `57` hex bytes | Title, author, popularity description/value, publisher and rights fee |
| Thunk `0060ECA8` → helper 20 `0098128C` | Six arguments copied to catalog arrays; source index incremented |
| fGMManga selection; selected-row handler | All 31 records are offered, with no consumption or popularity filter |
| fGMKikakuKaigi `009EFA40`, meeting mode 16 (`+60C7..639E`) | Publisher introductions, rights question, insufficient-funds and decline branches |
| Existing fGMKikakusyo mode-2 quote/acceptance branches | Rights fee is added to hardware × audience planning cost; points use the ordinary quote |
| fGMSyukka `009C9CB8 +0455..067B` | Mode 2 reads the selected adaptation's popularity and adjusts market factor |

The rights question checks only the rights fee. Agreement does not debit money.
The final accepted development proposal charges the full combined cost once.
Cancelling a discussion, rejecting rights or cancelling the proposal leaves
money and rooms unchanged. Room occupancy, hardware availability/license,
computer bit width and known funds are checked again at acceptance.
Repeated adaptations of the same title are permitted; no invented exclusive
rights timer or consumed-source restriction is added.

| Popularity | Market-factor adjustment |
| --- | ---: |
| ≥10 | +0.7 |
| 6–9 | +0.5 |
| 3–5 | +0.2 |
| ≤2 | −0.2 |

The adjustment precedes the ordinary market random roll, first-two-release
penalty, later quality bonuses and final negative-factor clamp. It consumes no
additional random draw. Exact zero remains zero; a negative result becomes 0.1.

Tracing the same shipment procedure corrected an older reconstruction mistake:
the bonus described by its original announcement as music actually reads the
**graphics** array (`00A127D0` copied to `00A12C10`, then tested at `+0B7F`).
The >100 branch still shadows its later >150/>200 branches. The misleading
announcement does not justify changing the variable read by the original code.

## Persistence, localization and limits

Projects and products retain mode 2 plus a validated source index (0–30). A
subsequent sequel uses mode 1 and its predecessor lineage; it does not inherit a
new adaptation fee. Missing catalogs in older caches request reimport rather
than inventing records. Older saves without adaptations remain readable.

English labels and dialogue are unofficial authored translations selected by
source index/hash. Kanji author-name readings are provisional. Japanese titles,
authors, publishers and dialogue come only from the user's local import. No
original catalog or Japanese dialogue table is published.

The rights meeting/proposal before acceptance is reversible UI state and is not
saved as an original conversation queue. Saving during it preserves the company;
loading returns to the office without a charge. Event introductions at shipment,
original meeting backdrops and original Windows behavioral comparison remain
unresolved. Static/data-flow checks establish the rules above, not complete
original runtime parity.

Public tests cover fee atomicity, failed acceptance, both rooms, normal
checkpoints, adaptation-to-sequel lineage, all popularity boundaries, market
clamping, random draw order and the graphics correction. The Godot core-loop
fixture runs an adaptation through rights, quote, meeting, release, reviews,
sales and reload. Private import tests verify record counts and source fields;
`tools/verify_manga_evidence.py` checks bounded anchors without exporting text.
