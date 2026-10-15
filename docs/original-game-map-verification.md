# Original-game map verification

Baseline: main `2eb7c0bf7de32b33b76e0bb1dc80840f406188e1`, 15 October 2026.
The initial PR116/117 mapping was a research/tooling change. No simulation, game UI, importer behavior,
release package or Actions configuration was changed.

Validation completed:

- Fresh supported-original rebuild: 86 objects, 84 forms/designers, 920 control
  definitions, 572 bound event slots, 669 procedures and 167189 instructions.
  Descriptor bodies total 571148 bytes; alignment tails are recorded separately.
- Source-bound menus: 55 records, six top-level roots; names/ordinals/classes,
  nesting markers and delimiters agree with original metadata/serialization.
- All known 2605 non-menu serialized instances rechecked for bounds, fingerprint,
  control name/class/array-index binding and recovered rectangle bytes. All 84
  layout regions and 536 picture-property payload fingerprints were rechecked.
  Picture payloads classify as 400 BMP, 22 GIF, 113 ICO and one CUR occurrence.
- All 36 known serialized timers retain explicit intervals/enabled values and
  bound callbacks. Omitted Enabled values remain unknown/default.
- Reference inventory rebuilt: 2829 direct-call/object-reference edges, 18591
  pool/member operand sites, 13140 unresolved dynamic-dispatch candidate sites.
- Original backup price and accounting debit independently checked as numeric
  operands in the purchase procedure. Four indexed ownership-cell references
  connect the purchase and virus-event procedure.
- All 86 objects have workflow dispositions; all 55 menus have dispositions.
  Repository evidence, comparison/importer and test paths exist. Runtime scenario
  IDs are unique; all 245 retain pending original-runtime status.
- Fifteen Python regressions pass: accounting/link integrity, missing object and
  menu dispositions, dropped references, duplicate IDs, dangling event bindings,
  menu nesting/delimiters/ordinal mismatch/cycle, source-hash rejection, rejected
  unsupported runtime claims, address queries and deterministic report/view
  generation with escaped embedded JSON.
- Chromium preview via a temporary loopback HTTP server rendered all 35
  workflows. Search, missing-workflow filter, empty results, backup source detail
  and the runtime-acceptance boundary pass, with no browser exceptions. Direct
  file preview was blocked by the managed browser policy; that policy was kept.
- Python compilation, JSON parsing, whitespace and public metadata-content guards
  pass. Original bytes, assets, captions and private traces are excluded.

Reproduction commands are in [the map](original-game-map.md). The standalone
HTML can be opened locally; if local-file navigation is restricted, serve the
checkout with a normal local preview server:

```sh
python3 -m http.server 8000 --bind 127.0.0.1
```

Then visit `http://127.0.0.1:8000/docs/original-game-map.html`.

The initial mapping did not claim original runtime acceptance, archive
completeness, pixel parity, semantic completeness, GD2 interoperability or
whole-session RNG equivalence. The subsequent
[implementation and evidence batch](map-backlog-implementation.md) recovers the
official archive, bounds all known control properties and all 84 roots, adds the
missing gameplay/options, and advances save/RNG/resource evidence. It preserves
opaque property meanings, dynamic-dispatch semantics
and all 245 native runtime scenarios as open work. #95 stays open.
