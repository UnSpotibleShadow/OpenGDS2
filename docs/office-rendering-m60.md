# M60 assigned PCs, atlas artifacts and facility signs

Assigned PCs now use the correct imported computer tiles. Earlier code selected
IDs 93/95/…115 from member 2234, hidden 46×35 placeholders with drawing key
−1200. That put the PC behind desks and sampled atlas borders and adjacent
computer fragments. The source assignment block instead follows Module2 pool
1D to global A13234/member 2218. Its initialized action 6–17 mappings are IDs
50–61, 36×29 computer rectangles with proper allocated drawing depths.

`009F651C +3504..38A0` stores these twelve mappings; `00A0283C +2726..2730`
and +276A..2774 read that array for positions/source columns. The corrected
projection preserves those allocated depth keys and changes position/source
according to the existing verified twelve-seat/four-model rules. No new z-order
number or raster operation is invented. The bounded office verifier now checks
the array identity and initializer mappings explicitly. The earlier placeholder
anchors remain documented as placeholders rather than computers.

The tight crop excludes red cell borders and fragments of the adjacent model,
which caused the reported colored marks around assigned seats. Actual blue
keyboards and colored monitor/case pixels in the imported artwork remain intact;
there is no global red/blue color replacement. Staff sprites, masks and the
original two raster operations are unchanged.

## English bitmap lettering

The three vacant-facility signs are baked into `OfficeBase.bmp`; the earlier
fourteen speech captions patch only `OfficeSprites.bmp`. English mode now
creates a second local overlay, `OfficeBase.png`, with authored 5×7 FOR RENT
lettering in the three 60×13 sign interiors. Sign borders, poles and every other
background pixel are preserved. The label has its own translation key.

Generation requires exact source atlas, mask and background hashes. The revision
hash includes the lettering recipe and translated strings, so an updated build
creates the new patch automatically when an English office is loaded. Existing
imported files are never overwritten, Japanese uses the original bitmaps, and
explicit user artwork overrides still take precedence. Original pixels and
generated PNGs are kept in the user's local cache, outside the repository.
Month/week lettering, other embedded labels and two ambiguous bubble captions
remain separate localization work; this pass covers the facility signs.

## Validation

Direct app compilation, 294 domain checks, the full fixed-60-FPS synthetic core
loop and the original-LZH UI suite pass. The original fixture checks all 48
model/seat combinations for visible pixels confined to each computer rectangle,
checks atlas-border exclusion, and verifies the existing burst-flame regressions.
It verifies translated facility interiors with all other pixels unchanged,
Japanese source presentation and English restoration after a language switch.
Original background/atlas/mask hashes stay unchanged. No debug package, Actions
run or native Windows comparison is produced.
