# Original Company summary sections

The compact Company window now follows the supplied original screenshot's order:

| Section | Contents |
| --- | --- |
| Blue | Company name and funds |
| Orange | Company recognition, production capacity with ×100 copy equivalent, library level as a percentage |
| Yellow | Number of released games, saved Hall of Fame count, highest cumulative unit sales and its game title |
| Green | Recorded weekly, monthly and yearly profit, including negative values |
| Cyan | Three tenant slots, with facility names or None |

Labels align left and values right. The English window is slightly wider to fit
translated labels; colors and grouping follow the original. Long values retain a
full-text tooltip. The existing facilities/information commands remain available.
Dates already appear in the toolbar/office and staff have their own window; those
extra summary rows are removed. A record refreshes when another own release
exceeds it. Highest sales means copies sold, not receipts/revenue. No products
shows zero and an empty-title marker. Known empty tenant slots show None; unknown
legacy values stay unknown. The saved company Hall counter is used rather than
assuming it can be rebuilt from the visible release list.

## Source evidence

The supplied original Company window shows all five groups. The original
`fGMInfo` own-company timer arm is `009DF294 +1723..2062`; capacity formatting
at `+1A0D..1A47` multiplies the capacity by100. The existing signed accounting
fields feed the profit block. The highest-sales own-product display already has
an adaptation projection in the detailed Company Information window.

The rank caption/value are Label1 indices76/78, positioned at Top4440twips.
Form load `00992494 +001B..0034` sets outer Height4600 and Width2625; with the
caption/frame inset the rank row is below the compact window. It was exposed by
the adaptation and is now omitted. This establishes a presentation correction,
not an original rank-progression formula; that question remains unconfirmed.

No economics, capacity gain, profit accounting or save format changes. This
corrects where and how existing values are displayed. English and Japanese
rendered checks cover grouping/color/bounds, copy/percentage formatting, signed
profits, empty/populated tenants, record changes and unknown legacy fields.
Original artwork and captures remain local. Native Windows final-package
acceptance remains pending.
