# Weekly product sales history — M20

This reconstruction uses targeted static P-Code for the supported executable.
Original Windows comparison remains outstanding. The plot's controls, colors,
font layout and English/Japanese labels are community-authored presentation;
the history data and scale rules below come from the original.

## Record and accumulation

Global `A1301C`/Module1 member `2000` is indexed by product and history cell.
It is not genre experience. Reset `009DD234 +153E..1574` writes Empty to
cells 0..12 for each product slot. New shipments therefore receive thirteen
zero numeric cells. Modern saves persist them as signed 64-bit unit counts.

Periodic sales `009E3830 +1087..10B3` adds the actual sold units to cell zero.
This is units sold, not gross revenue, net receipts, demand or lifetime sales.
The original sales-result defect can make these counts negative; M20 retains
those signed values. Recording history does not consume another random draw.

## Week boundary

Weekly procedure `009DB4F4 +0119..01A5` iterates all product slots, copies
cell 11 to 12 down through cell 0 to 1, then sets cell zero to zero. Cell zero
is the current incomplete week; cells 1..12 are completed earlier weeks.
The oldest week drops out. M20 connects this portion to the existing 200-tick
budget reset before periodic sales: a sales batch on the boundary enters the
new week. Modals pause this phase together with other office simulation.

The date/calendar economic pipeline is still incomplete. These recorded weeks
follow the recovered office budget cadence; dates are not fabricated for plot
labels. Company payroll, platform events, contracts and awards remain separate
unfinished work.

## Original graph scale

Product details `fGMZishaK 009C2568 +06A0..0C6E` scan cells **0..11** for a
maximum initialized at zero. The graph draws twelve segments over cells 0..12.
The omission of the oldest cell from scale selection is preserved.

| Maximum in cells 0..11 | Axis ceiling | Original units per 16-pixel interval |
| --- | ---: | ---: |
| Below 30,000 | 40,000 | 10,000 |
| 30,000 through 99,999 | 80,000 | 20,000 |
| At least 100,000 | 200,000 | 50,000 |

The high scale is fixed even if a value exceeds 200,000. This does not imply
an adaptive ceiling, cap on sales, forecasting bonus or altered market formula.
The Godot graph uses the recovered scale with responsive community-drawn lines.

## Persistence and validation

An older modern save with no history retains `null`. Sales continue, but no
historical cells are synthesized from lifetime sales; the graph explains that
records are unavailable. Known rows require exactly thirteen cells. Saves reject
malformed rows and preserve signed counts. Fresh releases initialize known rows.

Regression coverage includes signed accumulation, a simultaneous sales/week
boundary, save/load, oldest-cell expiry, original scale boundaries, the oldest-cell
scale omission, unknown legacy records and malformed-row rejection. The Godot
gameplay UI opens the chart and persists all thirteen cells. Bounded checks verify
the original reset, weekly shift, accumulation and scale threshold sites.
