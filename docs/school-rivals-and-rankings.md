# M37 — School facilities, rival awards and global rankings

This milestone connects the school to normal company management, imports the
original rival catalog locally, presents rival award winners, and saves the six
cross-company records separately from company saves. Reimport the original LZH
and start a new company to initialize rival history. Original titles, owner
names and numeric catalog rows are extracted into the user's cache; none are
bundled here. Earlier saves with unknown competitor history remain unknown.

## School construction and closure

The inspected default company page in `fGMInfo 009DF294 +1723..1F3B` enables the
company frame and fills the three facility labels without a rank predicate.
`0098D21C +0000..0042` maps labels 21–23 to facility slots 0–2. An empty slot
opens selection at +01E7..020E. This establishes the available UI path; it does
not establish all future facility or office-upgrade prerequisites.

The school definition at `009F651C +9963..9979` has facility ID 0 and costs
¥600,000,000. `fGMTenaSel 00988F78 +0000..0143` checks funds, rejects a duplicate
across all three slots, subtracts directly from cash, and writes the selected
facility. It does **not** use the shared period-accounting helper. Construction
therefore does not change weekly, monthly or yearly net totals. No construction
countdown or rank gate is invented.

`fGMTenantGameS 00984480 +0098..00B3` clears the selected slot to −1 after
confirmation. There is no refund in that path. The UI requires confirmation;
cancelling preserves the school. Purchasing through this service enables the
previously recovered April graduate event and closing it removes that gate.
No student-growth formula was found in the inspected school form. Other
facilities, student growth elsewhere, and office progression remain unresolved.

## Rival catalog, release schedule and sales chart

`Module1 00981B4C` appends six fields: title, cumulative sales, owner, sales-market
platform, sales power and demand. The initializer `009F651C +AF0A..B665` contains
30 starting rows; `Module1 009B5BE0` contains 35 staged releases. Owner helper
`0098548C` supplies ten company names. The importer reads bounded, hash-verified
literal shapes without publishing the records or names.

The weekly caller `009DB4F4 +01C2` runs rival refresh before the early calendar
events. Each eligible hardware-stage branch uses its own age counter (member
1188, eight cells), emits rows at specified ages, then increments once. The
**trigger platform is separate from each product's sales market**: some later
stage releases still use the older market. The implementation retains this
quirk. Loading or resuming a paused week never emits its releases again.

The twenty-tick sales batch at `Module2 009E3830 +098D..0A88` visits rows with
positive demand. Its calculation is:

```
sales = floor((floor(power / 2 * demand² / 700) + floor(Rnd * 20))
              * (1 + hardware_market / 80))
demand = max(0, demand - 2)
units += sales
```

Own shipment appending at `009C9CB8 +0FD7..1012` links its product index to a
catalog row with owner −1 and zero demand. After own sales, its chart row copies
lifetime units at `009E3830 +1102..111B`. The insertion sort at +144F..1538 swaps
only strictly greater sales, preserving append order for ties. The screen shows
the first 100 entries. An own-company first place queues the corresponding
achievement (+1567..17E8).

## Rival award presentation

Contest initialization at `00996CB8 +02D0..033B` draws 31 entries **with
replacement**, rejects own-company owners, and uses `floor(Rnd * (count - 1))`.
The newest catalog row is excluded by that original bound. All five award
categories reuse this fixed pool.

`009BD6F8 +0000..0048` displays thirteen randomly selected rival titles before
own-company selection. If no own title wins, the fallback draws another entry
from the same pool. OpenGDS2 now persists those thirteen picks and the actual
rival winner, then presents its locally imported game and company names.
Existing own-company selection defects and signed prizes remain intact.
Loading a revealed result never reruns the lottery or payment.

The 0.08-second display pacing is a modern presentation choice; the original
timer interval has not been established. Exact VB6 RNG seed/stream reproduction
and dynamic Windows comparison remain unverified. Older ceremonies without
known rival entries retain their previous generic rival result.

## Six local cross-company records

These are the original local “global rankings,” not an online leaderboard.
Each category stores the company achieving it at the earliest game date:

| Index | Achievement | Verified writer |
| --- | --- | --- |
| 0 | 250,000 sales | `009B4F34 +032E` onward |
| 1 | 500,000 sales | `009B4F34 +0688` onward |
| 2 | 1,000,000 sales | `009B4F34 +09E8` onward |
| 3 | Release own hardware | `0099F384 +02EA..0530` |
| 4 | Grand Prize | `009CEE10 +1166..13BA` |
| 5 | Number one on sales chart | `009E3830 +1567..17E8` |

The 100,000-sales announcement does not write a global record. Sales records
are queued on acknowledgement, Grand Prize on the own-company award result,
and number one after updating the sales chart. Dates compare year, month and
week strictly; equal or later dates do not replace the earlier company.
The original host-date text is separate from the game date.

`MDIForm1 0099CA4C` initializes unreached markers and loads `GameRank.GDS` through
`Module1 0098704C`. Writer `00986BD8` emits seven CSV rows, each with six columns:
four record fields (marker, company, formatted game date, host date), followed
by year, month and week. The importer accepts this CP932 format read-only,
preserves quoted fields and merges only earlier records. It never overwrites
an original file. This limited importer does not establish full original
company-save compatibility.

Modern records live in `user://rankings.json`, independently of the company
slot. Saves keep uncommitted achievements so a failed profile write can retry;
strict date comparisons prevent replay from replacing records. Invalid profiles
are reported and preserved. Title and office menus expose records; the office
also exposes the rival sales chart. Own-hardware records can be imported and
shown, but creating and releasing own hardware remains an unfinished upstream
workflow. No fake hardware-achievement trigger is provided.

## Validation scope

Fifteen added domain regressions cover school transactions, April linkage,
rival schedule/market separation, floors/noise, stable sorting, award draw order,
save corruption, ranking dates and independent persistence, and CP932 CSV
parsing. The optional original fixture checks 30/35/10 imported row counts and
a complete development/release/sales cycle with rival history present. Bounded
Python anchors independently check the relevant instructions and mappings.
Godot fixtures exercise purchases, cancellation/closure, the thirteen-pick
rival reel, reloads, rank presentation and the legacy-history notice.
See [validation.md](validation.md) for actual run results and remaining limits.
