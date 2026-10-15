# Map-backlog verification

15 October 2026. Baseline `a5abd1de0336ea67ba87aad200899d5dbf7472c0`.
See [the implementation/evidence notes](map-backlog-implementation.md).

- App build: zero warnings and errors.
- 551 domain checks pass, including optional supported-original extraction and
  the independent employee database comparison. New cases cover backup/virus,
  nine poaching paths, donation acknowledgement and number grouping.
- 19 Python map/property tests pass; fresh original inventory/report/view
  regeneration agrees with the committed generated files.
- Eight Godot regressions pass: company events, poaching, original settings,
  native toolbar, native layouts, management layouts, core loop and review/events.
  New workflows, settings and affected layout checks exercise English/Japanese.
  The layout fixture now uses the current Start Game Development menu route.
- Full official-LZH import passes: both race sheets, five WAV files, two negotiation
  portraits and 322 native dialogue entries. Older caches require reimport.
- Archive: 45 files/three directories, every length and CRC16 verified.
- Resources: 422 raster properties match imported bytes/decoded pixels; 82 root
  icons are exported; three PE payloads (icon/group/version) are accounted separately.
- Properties: all 2605 known controls and all 84 roots are bounded; 537 pictures.
  Reserved/object markers retain explicit unresolved meanings.
- Company events/settings: 34 procedure fingerprints and 40 bounded anchors pass.
  Updated visual-layout evidence passes 35 forms/1619 rectangles/240 procedures,
  excluding 14 nonvisual timer pseudo-rectangles.
- Native persistence: 130 paired record templates, 448 field slots, 447 distinct
  member references/array ranks and 40 loops. This is not a working GD2 importer.
- Native RNG: 310 draw sites and one startup seed site resolved by import ordinal;
  whole-session native draw-order/seed equivalence is not established.
- Whitespace checks pass. Original binaries, archives, captions, assets, saves,
  complete traces, private captures, generated editor sidecars and check logs are
  excluded from publication. No release/export/Actions change is included.

The original executable was attempted in a disposable official installation with
its VB6 runtime. Wine 11 fails with host SIGSEGV before a usable game window;
a bounded strace reproduces that host failure. It is not an original-game
acceptance pass. All 245 paired native scenarios remain pending. The map preserves
remaining source semantics, utility/menu differences, complete image use, GD2
interoperability and native timing/focus/pixel acceptance as concrete open work.
#95 remains open. Issue #118 is the owner's next requested investigation.
