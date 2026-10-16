# Consumer investigations and recalls

**A recall can bankrupt a wealthy company.** Its charge is not capped by your
current funds or by the profit you kept from earlier sales.

## Trigger and countdown

A released game receives one investigation check when demand drops below 16 and
it has not already been checked. The test marks it checked even when no event
starts. Bugs affect the chance; even zero bugs has a small residual chance
(approximately 1 in 317 at that one check).

An investigation lasts five game weeks after its initial announcement is
acknowledged. Sales and additional production can continue while it is running.
Those later sales and inventory contribute to the eventual result.

## Result

| Chance in the current implementation | Charge | Inventory/production |
| --- | --- | --- |
| 25% | `(copies sold + remaining stock) × retail price` | Stock removed; further production prohibited |
| 50% | `(copies sold + remaining stock) × ¥1,000` | Stock removed; further production prohibited |
| 25% | No charge | Stock retained; normal production guards apply |

The retail-price outcome uses the **full retail price**, rather than your net
receipts after distribution fees. Unsold stock is included too.

Example: 2,000,000 sold copies plus 100,000 in stock at ¥6,000 creates a
**¥12,600,000,000** full-price charge. Cash can fall below zero, triggering company
dissolution in either mode. There is no recovered emergency rescue choice on
this result path.

## Reading and saving the result

Consumer-representative/secretary conversations explain quantity, per-copy price,
total and reactions before the compact result receipt. Localized text summarizes
the recovered exchange. The charge has already been committed before that saved
result appears. Saving, reloading or clicking Continue cannot charge it again;
loading a pending result can replay its explanatory conversation.

Debugging before shipment reduces avoidable risk, but zero bugs is not absolute
protection. Do not increase inventory during an investigation without considering
the liability. Loading an earlier save can lose progress; loading the already
charged result does not undo it.

These financial branches were checked against the supported original executable.
Exact original Windows modal timing and whole-session RNG parity remain unverified.
[Source evidence](https://github.com/UnSpotibleShadow/OpenGDS2/blob/main/docs/consumer-recalls.md).
