# Toolbar year and English office date units

The native toolbar's funds panel also contains the current game year.
`MDIForm1.Label1[4]` and its shadow `[9]` carry the serialized `222年` caption
and occupy the field before Funds. The port now renders the saved state's year
there (`Year 3` / `3年`), including an explicit unknown marker for legacy states
without a year. The readout is refreshed by the existing desktop update path and
has room for the additional text.

The office's top-right plaque contains month/week, not year. Its 月/週 labels are
baked into the 466x290 background. The changing numbers are separate sprites
144/145 selected by `OfficePresentationRules.DateSprite`. English now receives
MO/WK lettering in two bounded label areas. The gradient is reconstructed locally
from their blank edges; the frame and number sprites are preserved. No date
values, sprite selection, calendar progression or save format changed.

This joins the existing local office patch generator, retaining its exact original
background/color/mask fingerprints, Japanese path and explicit artwork override
precedence. The generated cache revision changes so earlier English patches are
regenerated automatically. Original pixels and generated patches remain local;
no reimport is required when the supported office artwork is already imported.

Validation passes: clean application build;533 domain checks including bounded
caption preservation; bilingual rendered date/year checks with original office
artwork, separate saved years, reload, unchanged number/frame pixels; and the
complete bilingual native-toolbar workflow regression.
