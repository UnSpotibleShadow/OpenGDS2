# M105 Step 5 — casino presentation

All remaining steps are authorized by the owner's 2026-10-14 message; earlier
per-step approval pauses are superseded. No release/export/Actions/visibility change
is included. The work remains local until Step 8; issue #95 stays open.

The three card tables now use recovered 71×96 front/back frames, 26 red/black rank
pieces, four suit pieces and three court portraits. The compositor follows the
original component offsets: rank (4,2), small suit (4,22), large suit (16,40) and
court portrait (24,8). Native suit ordering is mapped separately from the saved
card ordering. Rendering reads saved cards/reveal/hold state; it never draws a new
hand, changes a wager or classifies an outcome. Missing imported components retain
an authored readable fallback. Poker's pay table uses its established multipliers;
Card Chain shows its distinct bonus multipliers, previous card and remaining deck.
Blackjack now draws its normal dealer/player hands as overlapping cards, retaining
its separate double-up selection/hiding rules.

The horse view has a compact eight-runner/forecast/odds roster and clickable wager
matrix, plus a dedicated 421×173 track viewport. The raster compositor uses the
M79 recovered coordinates, nearest-even integer conversion and paired SRCAND then
SRCINVERT operations, not an invented normalized progress chart. It reads the
original `Graphic/KeibaC.bmp` / `KeibaM.bmp` 233×316 sheets supplied by LZH import.
Missing sheets show the reimport message. Embedded coloured markers are wagering
icons, not horse sprites. Pair stake display continues to use actual runner cells;
the domain's compressed-index defects and settlement behavior remain unchanged.

The importer adds card components, the track and 24 wagering markers. Original
bytes stay in local/private import caches. A failed optional-original check caught
an incorrect marker-offset range; the corrected 8–23 range starts at 0x3B50DC.

Validation: clean app build, 523 domain checks (including an independent authored
mask/clipping/rounding raster test and exact optional-original import checks),
bilingual four-table client containment/captures and all four existing casino
workflow regressions. The first layout fixture attempted blackjack without first
placing its required bet; the fixture now follows the real input sequence. Existing
phase, held-input, keyboard, save/reload, charge, payout and native-defect checks pass.
Receipts: `/workspace/scratch/m105-step5-*`.

Original Windows repaint, native scenery scroll, focus/input and exact frame timing
still need visual acceptance. The recovered raster formulas do not establish those
runtime details. No game rules or original assets are published by this step.
