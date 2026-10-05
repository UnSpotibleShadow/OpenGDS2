# Office effects and local English bitmap patches — M49

Work now displays the original rising contribution sprites, tier-three growth
and burst counts, together with supported work/debug/bug and lounge speech
bubbles. The domain models timing and sprite rectangles; the Godot compositor
uses locally imported color/mask atlases. No original pixels are bundled.

## Evidence and compatibility scope

Module2 `00A0283C +88F3..9C15` establishes twelve seat coordinates, small 5×5
and medium 8×8 sprites, their rising motion, tier-three growth at ticks 16/20,
and numeric burst presentation from tick 32. The modern employee pass increments
at its end, so drawing uses the just-completed pass. A separate display count
preserves the source's pre-increment numeric value. Debugging has its own sprite
row. Bitmask compositing and original depth ordering remain in effect.

`+C940..CE5D` maps sixteen bubble rectangles, drops replacement requests while
a bubble is active, and expires them after the counter exceeds 15 × timing
factor (30). Bug work requests bubble 15; ordinary tier-three work requests 10;
other successful work uses the recovered 1-in-10 draw (debugging 1-in-6) for
bubbles 10/11. Lounge movement state 6 uses the recovered 1-in-100 idle draw and
1-in-3 choice of bubbles 5/3. Restored source draws do not establish complete
VB6 global RNG parity. Other walking/study/event bubble writers, burst flames
and additional meeting scenes remain unresolved; no substitute animations or
extra random bubble chatter are invented.

Office counters, pending/active bubble identity and burst display count survive
modern save/load and are range-validated. Older saves begin with no bubble and
use the existing count when the display snapshot is absent.

## English patches on the user's computer

English mode generates an atlas overlay under
`user://localized-artwork/en/<revision>/Graphic/Embedded/OfficeSprites.png`.
Generation requires the exact locally imported source and mask hashes. An
authored 5×7 alphabet draws fourteen translated captions, changing only opaque
caption interiors. It preserves dimensions, tails, borders, every other sprite
and all mask pixels; imported BMPs are never overwritten. Two ambiguous captions
(ids 13/14) are deliberately left unchanged. Wording such as HEAVE! and STUCK!
is an approximate contextual community translation, not an official localization.

The patch recipe and authored glyph shapes are public; the generated PNG is
local and must not be added to GitHub. The original LZH remains required.
Japanese uses the original atlas. Explicit local artwork overrides take
precedence over the generated overlay and must match source dimensions. Missing
or unsupported originals fall back to the normal importer rather than bypassing
it. Existing manual patch tooling remains available for other bitmap captions.

Pure C# checks verify frame coordinates, counters, draw order, lifetimes and
patch bounds/mask rejection without real artwork. The private original-asset
Godot fixture verifies generation and that every pixel outside the fourteen
caption interiors is unchanged, including the two unresolved captions. It also
exercises the 31-title chooser and guarded English labels. This is headless
validation; a full graphical comparison with the original remains outstanding.
