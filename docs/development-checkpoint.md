# Development checkpoint — M56, 2026-10-06

M56 adds the four owner-supplied meeting recordings as Ogg Vorbis. Planning,
alpha/type, beta/genre and completion/debugging dialogues select their matching
track. Music loops continuously across consecutive pages, respects existing
mute/volume settings and returns to office/title music when dialogue ends.
Selection/proposal screens and ordinary conversations retain the office music.
See [mapping and playback boundaries](meeting-music-m56.md).

Direct app/domain compilation, 284 domain checks, two meeting-audio integrity
checks, real-time dummy-driver playback checks, the complete synthetic core loop
and the original-LZH UI suite pass locally. Three changed local old audio caches
were restored from MD5-verified artifact data for the original-data rerun.
Publication uses a feature branch and PR into main. Actions
remain paused; no desktop package or native Windows run is created.

## Previous M55 continuation

M55 restores calendar announcement pages and graduate greeting persistence and
fixes the reported dialogue/meeting window-position reset. Dialogue, quote and
meeting decision windows retain their position through page changes and later
conversations within the running application; illustrations retain an independent
position. Smaller desktops clamp restored positions to accessible bounds.
See [changes and regression scope](dialogue-regressions-m55.md).

Direct C# compilation, 283 domain checks, the full synthetic headless core-loop
suite and the original-LZH UI smoke suite pass locally. Two changed local imported
music caches were restored from the artifact's verified entries before the
original-data rerun. These checks use the host runtime; native Windows execution
has not been verified here. The Windows debug package reuses the authenticated
M47 engine/runtime/audio and supplies the current DLL, PDB and translations.
It is a development checkpoint, not a clean export or a public-release gate.
The twelve exact seasonal source OGGs remain missing locally; existing GitHub
audio blobs are preserved. Actions remain paused.

## Previous M54 checkpoint

M54 imports five secretary records locally, connects paid replacement with
resumable farewell/identity/greeting, updates named captions and the office sprite,
and exposes production/rest policies. Automatic production follows the selected
300m/100m/manual policy; automatic rest enters at stress 100/80/60 and returns at
40/20/0, with voluntary control. Reimport the required LZH for the 109-entry local
Japanese dialogue cache. See [source evidence and compatibility limits](secretary-management.md).

Local direct C# compilation passes 286 domain checks (281 public, five private).
All 54 Python checks pass (38 public, sixteen private). Both headless Godot suites
report zero failures, and the root project launches headlessly with the compiled
assembly. Actions remain paused; no new desktop export or native Windows run was
started. Historical M48 packages do not include M54. Secretary salary/renewal
transactions remain unproven and are not invented; English names are provisional.

M54 merged in [PR #48](https://github.com/UnSpotibleShadow/OpenGDS2/pull/48),
commit `b6f4039fc1588803cb090c1ad6d7fbacff7527f0`; verified source commit
`109c1e2af4484861032ff1aaf7a10dd2ec822428` matches local source `646b359`.
The merged tree `0d5015ff9642261be33e794edfe468439e3c0b4a` matches the
verified candidate tree. Graduate/calendar speakers, automatic timed presentation,
remaining meeting/form fidelity and other progression systems remain unfinished.

## Previous M53 validation

M53 connects manual secretary advice through the original office hit rectangle
and Company/Settings entries. Fourteen conditional messages preserve branch/draw
order and finish with the funds assessment. Modern saves retain the selected
queue/read cursor; new imported campaigns preserve opening secretary source 1
and portrait 2. Reimport the required LZH for the 106-entry local Japanese cache.
See [source conditions and compatibility limits](secretary-advice.md).

Local direct C# compilation passes all 271 domain checks (267 public, four
private). All 51 Python checks pass (36 public, fifteen private). Both headless
Godot suites report zero failures. Actions remain paused; no new desktop export
or native Windows run was started. The root project launches headlessly with
the current compiled assembly. Historical M48 packages do not include M53.

M53 merged in [PR #47](https://github.com/UnSpotibleShadow/OpenGDS2/pull/47),
commit `e327a4202e1b83f328c21af41a439f91a51ebb4d`; locally tested source head
`b5d2a25ab8b81c559910b4912af893f05e78468e`. The merged source tree matches the
verified candidate tree. Secretary names, recruitment/contracts, policies and the
automatic timed presenter remain unfinished.

## Previous M52 validation

M52 restores alpha/type and beta/genre meeting discussions and their shared
original planning illustration, with compact preview/confirmation panels.
Paid choices create resumable follow-up dialogue. Modern saves preserve the
discussion cursor and existing selection effects; loading cannot charge again,
repeat publicity or award genre affinity twice. The type/genre introductions now
respect the original product-count ≤2 gate. Reimport the LZH for the 92-entry
local Japanese dialogue cache. See [source evidence and compatibility limits](conversations.md#m52-alpha-and-beta-meetings).

Local direct C# compilation and all 256 domain checks pass (252 public, four
private). All 48 Python checks pass (34 public, fourteen private). Both headless
Godot suites report zero failures, covering original illustration use,
choice preview, confirmation/save/load and ordinary/sequel/manga sales. Actions
remain paused; no new desktop exports or native Windows runs were started.

M52 merged in [PR #46](https://github.com/UnSpotibleShadow/OpenGDS2/pull/46),
commit `21998396225b78f8c1c11ca5b1fad65fb4d646e1`; locally tested source head
`11dbda98aff912cfb649235385e813be4acfda17`. The root project launches
headlessly with the current compiled assembly. Historical M48 packages do not
include these changes.

## Previous M51 validation

M51 restores three development checkpoint introductions and nineteen possible
shipment remarks, sequenced before review scores. Remarks share the existing
market rolls and preserve original branch quirks. Modern saves retain read
checkpoint acknowledgments and shipment cursors; loading cannot repeat shipment
costs or reroll the market/reviews. Old release saves do not invent lost lottery
history. Reimport the LZH for the 73-entry local Japanese dialogue cache.
See [source anchors and compatibility boundaries](conversations.md#m51-development-and-shipment-conversations).

Local direct C# compilation and 246 domain checks pass (242 public, four private).
All 45 Python checks pass with the original fixture (32 public, thirteen private).
Both headless Godot suites report zero failures, including original-asset imports,
ordinary/sequel/manga workflows and checkpoint/shipment save/load. Checks run
locally. No Actions, new desktop exports or native Windows runs were started.
Original Windows audiovisual/timer comparison remains outstanding.

M51 merged in [PR #45](https://github.com/UnSpotibleShadow/OpenGDS2/pull/45),
commit `db121a35d9800cd4a44c824920bc105fdce078b8`; locally tested source head
`0017a1e24b4595f48a788fe119652c627ed24b9d`. The root project also launches
headlessly with the current compiled assembly. The historical M48 download
packages do not contain this milestone.

## Previous M50 validation

M50 imports the verified 256×160 planning illustration locally and displays it
in a separate classic window throughout the discussion. Finishing, cancelling,
loading or changing screens removes it. Conversation cancellation now also stops
the reveal timer before its controls are removed. See
[illustration evidence and cancellation tests](conversations.md#m50-planning-illustration-and-cancellation).

Local direct C# compilation and all 237 domain checks pass (four private). All
42 Python checks pass with the local original fixture (30 public, twelve private).
Headless Godot original-asset UI and fixture-free core-loop suites report zero
failures. The root project launches with the current compiled assembly. Automatic
Actions remain paused; no new Windows/Linux export or CI run was started.

M50 merged in [PR #44](https://github.com/UnSpotibleShadow/OpenGDS2/pull/44),
commit `a0da237e640129c9887858f8c07179a4bcf9a950`; locally tested source head
`d87c34dabab5d3b9db99a20175ad74ca0882a988`.

## Previous M49 validation

M49 enables 31 locally imported manga/comic adaptations through publisher rights,
combined planning cost, ordinary development, release, reviews and sales. It
restores 20 ms conversation reveal/mouth frames, seven planning discussions,
supported work/burst effects and office bubbles, and fourteen English bitmap
patches generated only on the player's computer. Provisional staff readings were
reviewed; unresolved kanji pronunciations remain provisional and support local
corrections. See [adaptation evidence](manga-adaptations.md),
[conversations](conversations.md), [effects/local patches](office-effects-and-local-patches.md)
and [staff-name review](staff-name-localization.md).

Local direct C# compilation and 237 domain checks pass (233 public, four private
original fixtures). All 30 public Python checks and twelve opt-in original checks
pass. Headless Godot original-asset UI and synthetic core-loop suites both report
zero failures. The root workspace also launches headlessly with the current
compiled assembly. Checks include adaptation development/release/sales/reload, typing
acknowledgment and pixel preservation outside translated captions.

Automatic GitHub Actions runs are paused at the owner's request to conserve
storage. The workflow is manual-only and must not be dispatched without renewed
authorization. M49 has no new CI run or Windows/Linux export artifact. These
results are local C# compilation and Linux headless scene tests; current clean
SDK/export/native Windows verification remains outstanding. The M48 package
links below are historical and do not include M49 changes.

M49 merged in [PR #43](https://github.com/UnSpotibleShadow/OpenGDS2/pull/43),
commit `b6a72f1d058430b789747630917e62e05b98398a`; locally tested source head
`d8b2092e59dc8b845b6cebec100a5b01291bd05c`.

## Previous M48 validation

M48 connects verified sequel development through the room chooser, Hall of Fame
predecessor selection, quote/meeting, inherited quality/recognition, automatic
type/genre checkpoints, debugging, release, reviews and sales. Modern saves
preserve and validate series lineage. See [source evidence and compatibility
boundaries](sequel-development.md). Manga, polishing and external jobs remain
unavailable; exact conversations and original Windows parity remain open.

Local direct C# compilation and 225 domain checks pass (three private original
fixtures). All 28 public Python checks and 11 opt-in original checks pass.
Headless Godot original-asset UI and synthetic core-loop suites pass, including
the sequel lifecycle across save/reload. All four jobs passed in [run 37286978229](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37286978229):
222 public C# checks, 28 public Python checks (11 private checks skipped), a clean
SDK build, and zero editor or packaged Linux/Windows release/debug core-loop
failures. PR #42 merged as `0dbffe49cc00a7a2631cefe5a17e45e6352b2e8b`; tested head
`64aa38f7cb480ce6d5578d04539b2f59116c7434`. The root workspace also launches
headlessly with the latest compiled assembly.

[Download the M48 debug packages](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37286978229/artifacts/11335300598).
The artifact includes both platform ZIPs, expires 2026-10-19, and has digest
`sha256:4e259f18a2f653e838cd2812f90efcaf96291bf4ae0257d1dbd6c812861c6d13`.
[Release packages](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37286978229/artifacts/11334881626)
passed the same native core-loop checks. All packages still require local import
of the original distribution for normal play.

## Previous M47 validation

M47 integrates twelve supplied seasonal recordings as compressed Ogg Vorbis;
spring, autumn and winter each have four tracks, while summer uses all eight
existing office tracks. A two-second equal-power crossfade follows season changes
without changing simulation rules, clocks or saves. The supplied WAVs remain
outside the repository. See [seasonal music and validation](seasonal-music.md).
Local direct C# compilation and 208 domain checks pass (three private original
fixtures); 27 public Python checks pass (10 private fixtures skipped). Both
headless Godot suites pass, including two-second crossfade completion while the
simulation is paused. Full SDK and desktop release/debug checks run in CI.

All four jobs passed in [run 37280387678](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37280387678): 205 public domain checks, 27 public Python checks
(10 private fixtures skipped), a clean SDK build and zero editor or packaged
Linux/Windows release/debug core-loop failures. PR #41 merged as
`8910e419b141e2acf823559db3706ab9aaf19748`; tested head
`ca0202330f73fd6cbaec1d65864b271306af3a32`.

[Download the M47 debug packages](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37280387678/artifacts/11332540879).
The artifact contains both platform ZIPs, expires 2026-10-19 and has digest
`sha256:377e1a2f8d318552c4e2c095d25ab5c4e130e8acffe27ff297e8258f9b76b09b`.


## Previous M46 validation

M46 adds optional seasonal office music, with three-month seasons, two existing
authorized recordings per season and local seasonal playlist overrides. The
title theme stays separate. Preferences do not change original simulation rules
or saves. See [seasonal soundtrack behavior and tests](seasonal-music.md).
Local direct C# compilation, 207 domain checks (including three private original
fixtures), 25 public Python checks and both Godot suites pass. The local editor's
SDK discovery remains unavailable in this sandbox; full SDK builds and desktop
exports are verified separately by the published CI workflow.

All four jobs passed in [run 37274798922](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37274798922): 204 public domain checks, 25 public Python checks
(10 private fixtures skipped), a clean SDK build and zero editor or packaged
Linux/Windows release/debug core-loop failures. PR #40 merged as
`5088ef5e26ea8fc2ba915f1caac888f8f25597b4`; tested head
`2d38d432426fd23a52a4895dd7ef75d05cc05f59`.

[Download the M46 debug packages](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37274798922/artifacts/11329208861).
The artifact contains both platform ZIPs, expires 2026-10-19 and has digest
`sha256:b95a8d5047bce0e8d963a81326796e314739b3e8c5c58f9050acc81ead723f38`.


## Previous M45 validation

M45 adds genuine Windows/Linux x86_64 debug exports with managed game symbols,
console/logging instructions and a separate **OpenGDS2-desktop-debug** artifact.
The release archive layout is retained. Three packaging regressions verify
symbol preservation, missing-symbol rejection and imported-cache exclusion.
The core-loop fixture now checks the requested runtime debug flag.
Local public Python checks and two debug-mode Godot core-loop runs pass.
All four jobs passed in [run 37268683644](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37268683644): 203 public domain checks, 25 public Python checks
(10 private fixtures skipped), and zero editor or packaged Linux/Windows
release/debug core-loop failures. PR #39 merged as `348b1f7470a6d8ddbb47f4057a03d04de6ca1992`;
tested head `1104a87a123dfc5b7afdb6deab66f17e4ad44c11`.

[Download the debug packages](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37268683644/artifacts/11327755713).
The artifact contains both platform ZIPs, expires 2026-10-19 and has digest
`sha256:22aa6ba7c845f8cffc97a4627f49ea8893d74b1e7a8cf86abce052d7b3d3ff63`.
See [debug package instructions](building.md#debug-packages).

## Previous M44 validation

M44 applies the original initializer's final secretary portrait selection to
local office imports. Direct C# compilation, 206 domain checks and 32 Python
checks pass. Both Godot suites pass with zero failures. All four SDK/export/native
Windows jobs passed in [run 37262835613](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37262835613).
PR #38 merged as `b0bf7c4f36bb98ee92e77963ee142a1a6f07aa03`; tested head
`6d6e275373594091e7320cf10a9bace10dc1f7de`. Download
**OpenGDS2-desktop-development** from that run; it expires 2026-10-19.
Artifact digest: `sha256:e150f7a230241dd6c9eab59f19fc6c842f0e4e50813716a29e5a12510f9dbbf1`.
See [secretary initialization](secretary-initialization.md).

## Previous M43 validation

M43 restores the locally imported original hardware pictures in development
quotes, including selection changes. Local C# compilation, 206 domain checks,
32 Python checks and both Godot suites pass. All four SDK/export/native Windows
jobs passed in [run 37235810413](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37235810413).
PR #37 merged as `80a498a98e611049d549cf5a475fefe51eaa5e5d`; tested head
`f37e30dac139e5ea80356ee80b7ba3cb76a326ad`. Download
**OpenGDS2-desktop-development** from that run; it expires 2026-10-18.
Artifact digest: `sha256:0d15630ea065dbf8478b8ad4d7bde80b5f9fb2952da2c09686cfa6c9acc79175`. See [hardware picture evidence](quote-and-meeting-layout.md#m43-hardware-pictures).

## Previous M42 validation

M42 restores compact hardware/audience quote tables and a separate proposal step.
Local C# compilation, 206 domain checks, 30 Python checks and both Godot suites
pass. All four SDK/export/native Windows jobs passed in [run 37235186967](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37235186967). PR #36 merged as `c71552ab3b9557dbdf5efb0fa4dc280d1d6b95b7`; tested head `ce16ad92f9cd1b41ba9db664f8c750937c6553e3`. See
[quote/meeting evidence and limits](quote-and-meeting-layout.md).

## Previous M41 validation

M41 adds compact, resumable normal-hire conversations and local Japanese dialogue
import. Local C# compilation, 206 domain checks and 30 Python checks pass; both
Godot suites report zero failures. All four SDK/export/native Windows jobs passed in [run 37234730728](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37234730728). Tested head: `82a80a475d49e7f169f4aaa96428b424992c3d72`.
See [conversation evidence and remaining gaps](conversations.md) and the
[unresolved sprite artifact](sprite-artifact-investigation.md).

## Previous M40 validation

M40 adds display-only English staff-name readings while preserving Japanese
and saved employee identity. Local direct C# compilation, 202 domain checks
(including two private original checks), 28 Python checks and both Godot suites
pass. All four SDK/export/native Windows jobs passed in [run 37233639154](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37233639154). PR #34 merged as `716aceef7561becab3f43ac15fab450d6a257cfb`; tested head `0a228235b69707d1eab7055382370c63ad23b340`.
See [staff-name evidence and limits](staff-name-localization.md).

## Previous M39 validation

M39 connects desk-interior development actions and focused tenant-slot school
management using recovered original hit regions. It also restores the sales
ranking column order. Local compilation, all 200 domain checks, all 27 Python
checks and both Godot suites pass. Published SDK/export/native Windows checks
passed all four CI jobs. See [M39 evidence and limits](office-clicks-m39.md).

M39 merged as `897867e72355d81d815f699ec42368dd2fe75d50` (PR #33).
The tested head is `2ca2d08bd038b173522e0e74d0148e694cd436de`. All four jobs
passed in [Validate run 37224787621](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37224787621):
198 public domain checks, 19 public Python checks (eight private fixtures skipped),
zero standard SDK warnings/errors and zero public Godot, packaged Linux or
native Windows core-loop failures. Download **OpenGDS2-desktop-development**
from that run; it expires 2026-10-18. Artifact digest:
`sha256:c18c24b1ef770cc3e39847df3233cc2f802e3d8f5c62d9b394a076858a60c957`.

## Previous M38 validation

M38 implements the first screenshot-feedback presentation fixes: dedicated title
music, contrasting caret/selection, native-pixel desktop resizing, compact
recruitment and a smaller desk selector. Local C# compilation, all 198 domain
checks, all 27 Python checks, and core-loop/original-import UI suites pass.
M38 passed all four SDK/export/native Windows jobs and merged in PR #32. See
[M38 scope and limits](presentation-fixes-m38.md) and
[the remaining screenshot tasks](user-feedback-2026-10-04.md).

M38 merged as `2c0a01075a1025573731b4551d334c91422ea8fa`. The tested head is
`9e1adf0c0ed429a16e06f8c7ca8468f2635243a8`. All four jobs passed in
[Validate run 37223533010](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37223533010):
zero SDK build warnings/errors and zero public UI/core-loop or packaged
Linux/native Windows failures. Download **OpenGDS2-desktop-development** from
that run; it expires 2026-10-18. Artifact digest:
`sha256:480be0feed47fa0195337eb4a2c2561b86552664e9c72726bf1ab4f1071e2893`.

## Previous M37 validation

M37 connects school construction/closure, locally imported rival releases/sales,
rival award names and thirteen-pick presentation, six independent earliest-date
records and read-only GameRank.GDS import. The requested compact employee pane
includes imported portrait, ability chart, status bars and a tracking red arrow.
Original LZH importing remains required. Reimport and start a new company for
known rival history; earlier missing history is never fabricated.

Local checks pass: 198 domain checks (196 public plus two private originals),
all 27 Python checks, direct C# builds with zero warnings/errors, and core-loop
and original-import Godot suites with zero failures. Standard SDK/export and
packaged Linux/Windows runs pass all four jobs. Own-hardware development, other
facilities, student growth, office progression, remaining economics and exact
original RNG/presentation parity remain unfinished. See
[school/rival/ranking evidence](school-rivals-and-rankings.md) and
[employee presentation](compact-employee-presentation.md).

M37 merged as `08318cdf8e71f71913cd444710253be811d1b54b` (PR #31).
The tested head is `d20ea082e900f57d5ff85c0add0857d6662aa296`. All four jobs
passed in [Validate run 37219588973](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37219588973):
196 public domain checks, 19 public Python checks (eight original fixtures
skipped), zero standard build warnings/errors and zero Godot/editor, packaged
Linux and native Windows core-loop failures. Download
**OpenGDS2-desktop-development** from that run; it expires 2026-10-18.
Artifact digest: `sha256:5da4edc2620fc3e702c35b01c509743d2a1ec9f285335ac398dc3c015ac35520`.

## Previous M36 validation



M36 implements exhibition participation/results, the five own-company award
categories, the terminal Beginner branch and source-ordered graduate prompts.
M35 exhibition/ending stops resume their existing cursors; unknown legacy dates
and graduate bounds remain explicit stops. The original LZH is still required.

Local verification: 183 domain checks (181 public plus two private original
fixtures), 27 Python checks, direct Roslyn domain/app builds, and the expanded
native Godot core-loop UI fixture. The first expanded UI run caught a reversed
synthetic mode value in the ending fixture; its corrected rerun passed with
zero failures. Original-import UI also passes with zero failures. Standard
Linux/Windows builds and packaged runs pass all four CI jobs. School construction/student growth, rival-name/animation
lifecycle and the original global rankings are documented remaining work.

M36 merged as `74827ba8b441395b8de00491441f42e4a1b370e7` (PR #30).
The tested head is `b2c3e18b91ef21190f810b110745a2bf121df3d0`. All four jobs
passed in [Validate run 37196872437](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37196872437):
181 public domain checks, 19 public Python checks (eight private fixtures skipped),
zero standard build warnings/errors, and zero UI/core-loop failures in the
editor, packaged Linux and native Windows. Download
**OpenGDS2-desktop-development** from this run; it expires 2026-10-18.
Artifact digest: `sha256:dace169eb91e566ac1c55871112037f39c8c9fcd9c58de29ac9157756a3de74c`.

## Previous M35 validation

M35 advances new supported companies through verified calendar phases. It
connects payroll/accounting, annual renewal, hardware launch/retirement/markets,
ordered recalls and saved loan dues, with persistent modal resumption. Original
LZH importing remains required; old calendar history is never fabricated.
Seventeen added public regressions bring the public domain count to 163; two
additional original-import fixtures also pass locally. Local Godot core-loop
and original-import UI suites pass with zero failures.
All 27 Python checks pass. Direct Roslyn builds have zero warnings/errors.
M35 merged as `05cd2275f9d77173c40cc8ace35180489d9fac3f` (PR #28).
Its tested head is `6dc1199266e0f445c17270cdd8cc157969b5bd6f`. All four
jobs passed in [Validate run 37192745866](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37192745866):
163 public domain checks, 19 public Python checks (eight original fixtures
skipped), zero standard build warnings/errors and zero UI/core-loop failures
in editor, packaged Linux and native Windows. Download
**OpenGDS2-desktop-development** from that run; it expires 2026-10-18.

The modal follow-up also guards toolbar/menu callbacks and employee-table
activation while a calendar transaction is pending. Its payroll resolution
screen runs the proven staff-list writer with management controls disabled.
The follow-up merged as `4278181cb43b5c1d06bb51d0c045b24656c91795`
(PR #29), tested head `8275386708f865c8ca934904408f17140fca7691`.
All four jobs passed in [Validate run 37193608886](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37193608886),
including the enlarged core-loop UI fixture in the editor, packaged Linux and
native Windows. Standard build warnings/errors and all UI failures are zero.
The latest **OpenGDS2-desktop-development** artifact is on this run and expires
2026-10-18.

See [annual-calendar-events.md](annual-calendar-events.md) for M36 evidence,
modeled event transactions, save compatibility and remaining school/rival gaps.
Borrowing/bankruptcy, facility progression and dynamic original comparison remain.

## Previous M34 validation


M34 connects the original signed financial accumulator to supported transactions
and restores the three company-window period rows. Seven new public regressions
pass; local domain total is 148 (146 public plus two original fixtures), all
27 Python checks pass and the complete core-loop UI suite passes. Original UI
QA with the committed soundtrack passes with zero failures. Standard SDK/export
checks passed in all four jobs of
[Validate run 37186347364](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37186347364).
M34 merged as `593dc3f3abd86421f7a654d6b3834db39a68754e` (PR #27); the tested
PR head is `66b41d3ca919ba58c94d3d4e741c6adc29fc5146`. Standard build warnings
and errors are zero. Linux/Windows packaged core-loop failures are zero.
Download **OpenGDS2-desktop-development** from that run; it expires 2026-10-18.
See [financial-accounting.md](financial-accounting.md). The calendar remains
unconnected; no payroll value or event schedule was invented.

Local execution and original-data tracing have recovered. The published M32
comparison and employee-selection implementation is
`91eed94096c1f88f3b46f20546e1414d9afcb918` (PR #25). All four jobs passed in
[Validate run 37153643487](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37153643487).
Download **OpenGDS2-desktop-development** from that run for Linux/Windows x86_64.
Its artifact expires on 2026-10-17 and can be rebuilt from the workflow.
Original LZH importing remains required to start a company.

M33 restores the second, technology-only work-counter increment. Its bounded
branch trace establishes 11 eligible passes for the first unit and ten thereafter;
ordinary work keeps its existing cadence. Local compilation, 141 C# checks
(139 public plus two original fixtures) and all 27 Python checks pass.
Both Godot UI suites pass, including the imported-original office suite.
M33 merged as `f8e4f7c1a6803323b9f9f27f4a5cbffe7e0d6bbb` (PR #26); its
[Validate run 37154482438](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37154482438)
passed domain, Godot, desktop export and native Windows. The standard build
has zero warnings/errors; both packaged core-loop fixtures report zero failures.
The tested PR head is `8e1ea3170ab125d06e655d23e3b0700bf7939d98`. Download
**OpenGDS2-desktop-development** from that historical run; it expires 2026-10-17.
See [original-comparison.md](original-comparison.md) and
[technology-application.md](technology-application.md#work-cadence).

## Delivered changes

- Eight user-supplied music recordings are bundled under explicit project-distribution authorization. Their separate notice accompanies desktop builds. A local playlist overrides bundled music; mute/volume preferences remain outside simulation saves.
- English display translations cover 18 genres, 51 types, seven audiences, eleven hardware records, four computers and eight technologies. Stable IDs and the supported executable hash select labels; imported catalogs and saves retain their original data. Japanese remains supported.
- The toolbar uses locally imported original images where identified, with authored pictograms for other actions. Every action has translated tooltip/status help. Language preference survives restarting the application.
- Local original-artwork inspection corrected Save/Load/Staff/License/Software/Management associations. Exact original click-handler correspondence remains unresolved.
- Targeted tracing verified the speed technology's 50/100/150 eligible-pass countdown and expiry. Its effective work contribution remains unresolved and disabled.
- Modern saves validate inactive source employees with the existing checks for active staff/applicants. Invalid progression is rejected before replacing a readable slot; nullable unknown progression remains nullable.

## Remaining translation and fidelity gaps

M40 supplies provisional English readings for fixed and generated employee names.
Their official pronunciations have not been established.
Original bitmap text requires locally generated artwork patches; original pixels
are not bundled. Full graphical/audible comparison, keyboard/pointer usability
and original Windows behavioral comparison remain outstanding.

The playable own-game loop does not constitute a complete port. M35–M37 connect
verified calendar accounting, payroll, saved loan dues, hardware events, awards,
exhibitions, ending and graduates. Borrowing/bankruptcy, office upgrades,
remaining economics and external workflows remain incomplete. M41–M43 restore
normal-hire conversations and development quotes; secretary advice, other
conversation callers and original meeting sequencing remain unresolved. Consultation entry costs/timing and original-save compatibility
also require further evidence. See [remaining-work.md](remaining-work.md).

## Historical disconnection and safe resumption

The local execution service disconnected after M30's local compilation,
original-import UI and Python evidence checks. M31 was initially tested in GitHub Actions. Its optional private-original
fixtures have now passed after execution resumed for M32. Supplied-binary
tracing is available again. No unknown compatibility mechanic was
substituted to work around that limitation.

GitHub main is the authoritative published checkpoint. The independent local
checkout has been reconciled with M30–M32; prior local versions were backed up
before replacement. Preserve this distinction when synchronizing git histories.
Do not discard local original files, bounded traces or user imports, and do not
publish them. No original executables, bitmaps, LZH files, imported catalogs,
whole disassemblies or private saves were added to the project.

Continue with targeted original runtime comparison when available, and tracing
of the periodic calendar body and its shared scratch writers. Preserve the established distinction between verified isolated rules
and unresolved live scheduling. Translation work should next establish name
readings and local-only bitmap patch behavior without guessing identities.
