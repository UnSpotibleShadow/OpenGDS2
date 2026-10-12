# Review and event presentation — M96 / #95

M96 restores the native walkthrough's four vertical review rows, booth attendance
scene and five persistent colored awards rows. Ticket #95 stays open.

## Local artwork and corrected source roles

The supported PE contains four reviewer/name portraits (`fGMCrossR.Image1`), four
visible score slots (`Image2`), ten hidden score sources (`Image4`), and ten
reaction pictures (`Image3`). The hidden score BMPs use RLE8 compression; they
are decoded locally to PNG. This corrects M93's mistaken Image2 source indexing.
The booth and stage are single-frame ImageFormG[26]/[4] GIFs, locally decoded to
PNG; they are not animated GIFs. Five 40×40 trophy BMPs are ImageTF[0..4].
ImageTF[5] is an all-white empty placeholder, not a Hall of Fame illustration;
the compact Hall of Fame pane reuses ImageTF[2]. No source pixels are committed.
Older caches can be reimported; missing artwork keeps a readable fallback.

`tools/verify_presentation_assets_evidence.py` verifies 36 serialized resource
bindings, 14 bounded source anchors and 49 bounded comment branches. Independent
private decoding supplies golden RGBA hashes for the three GIFs and ten RLE8
score images; the optional original domain checks compare every decoded pixel.
The portable PNG writer is checked with authored pixels and decompression.

## Prepared reviews and attendance

New shipments save one comment variant per reviewer using the six native score
bands and 9/8/11/9/8/4 reachable choices. The unreachable fifth score-ten comment
is excluded. Japanese templates are assembled locally from the original pool;
English uses authored paraphrases, with genre/content/company substitutions.
Old saves without comment history retain an explicit unknown fallback.

Saved callbacks drive previews at 10/30/50/70, final rows at 20/40/60/80, and the
summary/acknowledgment at 90. Rendering, reload and callback advancement do not
draw again or change scores. Score 9 makes no final reaction write: retain the
initial row picture/text. Native cosmetic digit cycling/RNG, retained frames and
Windows timer delivery are not reproduced or benchmarked. Display pacing is
modern, separate from office pacing issue #94.

Exhibition selection separates developing rooms with floored completion and
released products with dates, retaining five-entry/fee guards. Participation
retains its existing one-time transaction and twenty-two visitor draws, now
saving their increments and the preceding record. The scene reveals those
increments every fourth callback through 90; save/reload resumes the display.
It does not repeat payment, awareness gains or visitor RNG. Native character
motion/window stacking still requires runtime reference.

Awards retain prepared winners/prizes and the thirteen saved rival reel picks.
Their five colored rows show prior category results while later categories are
announced; a legacy save with missing previous winners shows unknown rows.
Outcomes persist during the ceremony rather than being inferred from a product's
single marker, which later categories overwrite. Compact reports now show the
two native trophy/empty slots, separate from reviewer portraits.

## Award marker correction and modern-save migration

Both `009CEE10` writers and `009C4570` readers establish A12F74 markers:
1 Grand Prix, 2 runner-up, 3 music, 4 graphics, 5 worst game. The earlier port
transcribed four values incorrectly. Selection formulas, payments and overwrite
behavior remain unchanged; the internal Design enum still means runner-up.
Modern save schema 2 adopts the native markers. Loading schema 1 maps old
1/2/3/4/5 to 5/4/3/2/1 once, preserving the old award meanings and funds.
Saving does not mutate the caller. Original VB6 save import remains separate.

Validation: 498 optional-original domain checks; clean application build/import;
full core-loop UI checks; bilingual partial-review save/reload, score-nine reaction,
local artwork and booth attendance checks; M93 and M96 evidence verifiers.
