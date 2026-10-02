# Original presentation recovery — M10

The target is the full 2001 PC game in Godot, preserving its office and workflow.
M10 restores the populated opening office and the hiring-to-lounge presentation.
Development/release/sales and company progression remain incomplete.

## Locally imported graphics and structures

The supported executable contains serialized VB6 form BMP files, rather than
PE bitmap resources. The importer checks its supported SHA-256 and bitmap bounds.

| File offset | Dimensions | Local import name |
| --- | --- | --- |
| `0x3B5DC6` | 466 × 290 | `OfficeBase.bmp` |
| `0x3D7469` | 1600 × 530 | `OfficeSprites.bmp` |
| `0x4A6960` | 1600 × 530 | `OfficeMasks.bmp` |

These files and `office.json` are recovered into the per-user original cache.
They are never bundled in the repository. Reimport older caches to obtain the
128 literal tiles from initializer `009F651C +2BEF..53F5`. Employee tiles occupy
IDs before these static tiles. An empty, zero-sized placeholder is a valid no-op.

Module1 ordinal 67 (`00987E78`) maps allocator arguments to destination X/Y,
source X/Y, width/height and depth offset. Its depth key is the round-to-even
16-bit conversion of `Y + depthOffset + tileId / 1000`. Module2 ordinal 11
(`00A0283C +D6B1..DAB5`) filters on `X + width > 0` and `Y > 0`, stably sorts
ascending keys, then applies two raster operations for every visible tile:

1. `SRCERASE`, `00440328`: mask AND NOT destination.
2. `SRCINVERT`, `00660046`: preceding result XOR color atlas.

White mask/white color preserves background; black mask replaces it with color.
This is reproduced channel by channel, including clipping. Calendar glyph tiles
144/145 use the month/week rectangles recovered at `+D5FB..D6AD`. The office is
displayed with nearest filtering at exactly 2×, within a 1280 × 720 canvas.
Community-authored beveled controls surround the imported scene.

## Employee entry, sitting and rest

The implemented subset is statically verified, not an observed Windows run.
It uses domain state independent of Godot, and persists movement across saves.

| Evidence | Implemented behavior |
| --- | --- |
| Hire helper `0098735C +0000..0108` | First unused workspace; entrance `(128,56)`; initial frame 7; source/portrait assignment |
| Action helper `009AF6BC +0000..01A1` | 12 lounge seats; initial random draw plus up to 13 retries (14 draws total); groups select actions 18/19/20 |
| Main loop `00A0283C +0000..0005`, `+DCE7..DD18` | Timing factor 2; minimum iteration time 40 ms |
| `+0A7D..2358`, `+2BC2..2E98` | Entry waypoints, selected lounge approach, final seat coordinates and facing |
| `+BB89..C033` | Diagonal movement ±4 horizontal/±2 vertical pixels per tick; original walking frame sequence |
| `+37B8..3B06` | Sitting offsets and frames; transition to rest, including same-tick fall-through |
| `+3BF0..404E` | Four rest animation branches; stress decreases by 0.3/2 per tick and clamps at zero |
| `+C044..C7AD` | 44 animation rectangles; portrait-column stride 152; feet depth and visual offsets |

A failed lounge random search remains unassigned; it does not force an occupied
seat. The route implementation deliberately accepts only the traced diagonal
entry/lounging segments. Work paths must be reconstructed before enabling them.
A shared session random source supplies modern random draws; VB6 RNG parity and
exact random consumption across the entire original loop remain unresolved.
Office animation currently runs while the office is shown and pauses in menus.
It does not advance the still-incomplete calendar/economy simulation.

Initial employee identity/count and the hiring UI's capacity guard remain
unresolved. Earlier external prose suggesting one starting employee is retained
as a research lead, not inserted into the domain state. Legacy modern saves
without workspace fields retain unknown placement rather than respawning staff.

## Validation and limits

All twelve lounge destinations are exercised, along with retries/exhaustion,
rest, portrait/depth mapping, workspace validation and in-flight save/load.
`tools/verify_office_evidence.py` checks concise byte anchors against a local
supported original. Domain tests and the real Godot scene-tree clock pass.
The compositor's exported opening and hired-worker frames were visually inspected.
The dummy headless renderer does not expose `ImageTexture.Update` pixel changes
through `get_image`; private image checks force recomposition after loading.

There is no desktop display/Xvfb on this host. Whole-window graphical rendering,
input usability and comparison with the original Windows runtime remain pending.
Original graphics and private QA images must not be committed or redistributed.
