# M105 Step 4 — hardware presentation

The owner separately authorized Step 4. Work remains local on
`codex/m105-presentation-evidence`; issue #95 stays open and publication is Step 8.
Step 5 (casino graphics/layouts) needs the next permission. Audio remains Step 7.

## Source-backed screens

The Step 1 layout manifest records the designer rectangles and handler hashes.
Private designer records and disassemblies remain outside the repository.

- **Market (`fGMGameHardInfo`, 352×240 designer)**: instruction strip and four
  columns: hardware, licence, shipped units and manufacturer. The original menu
  entry is `MDIForm1 00979D54`; load `00995588`, timer `00999D04` filters available
  platforms and refreshes their market cells. The new Company menu route is a
  bounded live table; refresh preserves selected source and scrollbar position.
- **Own-hardware report (`fGMZishaH`, 226×79 designer)**: device, name, home/handheld
  form, cartridge/CD-ROM medium and shipped units. Original menu `009815AC` gates
  this on ownership; load `0098AFF4` selects picture 11–14 and timer `0098CB60`
  formats the market projection times 10,000. Stock/price labels are outside the
  saved client, and stock/price globals lack modern saved projections; they are
  not invented as active console commerce. The Products menu route requires a
  launched own console and updates the report while open.
- **Development (`fGMDevH`, 256×159 designer)**: design/medium, four quality/800
  values, four required technology on/off flags, completion note and point total.
  Timer `0099BFB0` refreshes these fields. The new compact monitor uses the existing
  per-room retained-report mechanism, remains visible behind meetings/other views,
  retains position and keeps normal office simulation running. No project-cancel
  action is invented. English uses a slightly wider client for technology names.
- **Own launch (`fGMSyukkaH`, 274×249 designer)**: shared 256×160 scene, design/media
  device, editable name and launch action. Load `00994E78` supplies device 11–14;
  acceptance `0099F384` remains the existing saved transaction. Draft names still
  survive reload and launch adds no extra charge. Modern Save access remains.
- **Third-party launch (`fGMHardHatubai`, 274×311 designer)**: the same illustration,
  device, name, manufacturer, licence fee and bounded description. The existing
  calendar page/secretary conversation order is unchanged; acknowledgment still
  advances the saved transaction once.

The importer now extracts `HardwareLaunchScene.bmp` locally from `0x28D66E`.
It validates its 256×160 BMP header and byte equality with the second bound copy
at `0x2A3130`. English replaces only the verified 164×23 title area with authored
NEW HARDWARE lettering, guarded by the decoded RGB SHA-256. Japanese and explicit
user overlays retain their original paths. The stage/audience/device pixels outside
that area remain unchanged; original pixels are not committed.

The hardware planning preview is restored as well. Native selection `009940B8`
writes the shared design/media choices; cancelling preserves those choices without
spending funds. M104's compact selector had lost that behavior. Updating the choices
on selection restores it without changing quote/acceptance rules. Preview artwork
uses the established 11 + 2×design + medium mapping.

## Verification and handoff

`tests/hardware_layout_smoke.gd` covers English/Japanese client containment for all
six screens, retained hardware monitor position, market rows, selected technology
flags, draft-name reload, own launch/accounting/report and calendar continuation.
The own-hardware workflow regression uses the current compact selector action name
and verifies retained choices, completion acknowledgments, fees and both media.
Existing presentation completion and full core-loop regressions cover ordering and
saved transitions. The optional-original domain check verifies exact extracted BMP
bytes, decoded RGB fingerprint and every pixel outside the caption patch.

Final receipts and private crops are `/workspace/scratch/m105-step4-*`; the union
handoff is `/workspace/scratch/m105-evidence/step-4-completed.json`. Clean English
preview screenshots for the owner's community updates are generated outside the
repository; they use authored test state rather than a release/export build.
Exact Windows font/stacking acceptance remains for the next owner review.

Final checks pass: clean build (zero warnings/errors), 522 optional-original domain
checks, bilingual six-screen hardware layout sweep, own-hardware workflow,
presentation continuation and headless full core-loop. The original own-hardware
check exposed the dropped shared selection state; that behavior and the preview
were restored. Final planning screenshots use a complete authored 11-platform
catalogue and calendar, matching the start gate rather than hiding its warning.

The owner welcomed preview images for daily community updates. Six English PNGs,
an overview and a ZIP are available in `/workspace/scratch/m105-step4-community`.
They are local deliverables; no remote upload/publication or exported build occurred.
