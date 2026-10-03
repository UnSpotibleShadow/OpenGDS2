# Validation — M27 checkpoint

M27 fixes modern save-size integrity: serialization now refuses output above the
loader's 4 MiB limit before creating a temporary file or replacing an existing
slot. Regressions verify preservation of the old readable slot, absence of
leftover temporary files, an exact-limit round trip including JSON overhead, and
rejection one byte above the limit. This does not change original simulation
rules or establish original-save compatibility.

The [M27 SDK/export workflow](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37135926273)
passed all four jobs: 130 public C# checks; 18 public Python checks (eight private
fixtures skipped); a normal Godot/.NET build with zero warnings/errors; both UI
suites; Linux/Windows exports; and full core-loop fixtures in extracted packages
on native Linux and Windows. Both packaged runs reported zero failures.
Download `OpenGDS2-desktop-development` from that run.

## M26 checkpoint

M26 adds four public domain checks for the independently recovered outcome
branches of both per-copy fee negotiations. Local direct compilation passed with
130 C# checks (128 public plus two private original fixtures), and all 26 Python
checks passed including the expanded bounded original-evidence verifier.
The fee rule is isolated and not connected to live consultations: entry costs,
eligibility, dialogue RNG and calendar timing remain unresolved.

The execution workspace disconnected after these checks. M25 is merged on
GitHub, and M26 source and validation are preserved there for continuation.
Further private binary tracing requires restored access to the supplied files.

## M25 checkpoint

Local workspace access has been restored. Direct C# compilation passed, with
126 domain checks including two optional original-fixture checks. All 26 Python
checks passed with the supplied original executable. The imported-original Godot
UI suite and the full development/release/sales UI suite reported zero failures.
M25 adds desktop geometry/window-control checks, required-original local artwork
overlays, and local soundtrack resource/preferences/playlist checks. Music files remain
local; public tests generate authored silent WAV fixtures. ZIP conversion tests
cover path isolation, no-overwrite behavior and failure without partial output.
The [M25 SDK/export workflow](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37134160088)
passed all four jobs: 124 public C# checks; 18 public Python checks (eight original
fixtures skipped); normal SDK build with zero warnings/errors; both Godot UI
suites; Linux/Windows exports; and the full core loop in extracted packages on
native Linux and Windows. Both packaged runs reported zero failures.
The `OpenGDS2-desktop-development` artifact contains both packages.
The local bitmap patch tool has five additional authored-fixture integrity tests.

The constrained local host uses a direct Roslyn compiler because Godot's editor
SDK discovery cannot use the unavailable /proc environment. These local runs do
not replace the normal SDK/export checks in GitHub Actions. They also do not
constitute a full graphical or audible comparison with the original game.
See [M25 presentation and audio scope](desktop-ui-and-music.md).

## M24 historical checkpoint

M24 extends the Godot core-loop fixture with manual pause/resume labels, immediate
status refresh, paused weekly/sales clocks, screen-rebuild persistence and resumed
clock advancement. It also removes an original default personal-name literal
from the public company-setup screen; that default remains unavailable until a
local import recovers it.

The [M24 workflow](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37121420386)
passed all 123 public C# checks, ten public Python checks, a build with zero
warnings/errors, both editor UI suites, both desktop exports and the complete
core-loop UI fixture against extracted Linux and native Windows packages.
Both packaged runs reported zero failures. Local original-fixture reruns were subsequently completed at the M25 checkpoint.

## M23 historical checkpoint

The [M23 desktop workflow](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37121052813)
passed 123 public C# checks, ten public Python checks, a build with zero
warnings/errors, both editor UI suites, Linux/Windows desktop exports, and the
complete UI fixture against extracted packages on Linux and native Windows.
Both packaged UI runs reported zero failures. This is headless validation, not
whole-window visual or original Windows behavioral comparison.

Desktop artifacts include community runtime binaries, project notices, Godot
license/copyright information, actual .NET runtime-pack notices and checksums.
Original-game files and imported caches remain excluded.

## M22 historical checkpoint

M22 adds five public payroll-projection checks and a Godot staff-screen fixture.
The local execution workspace disconnected after M21; M22 original-fixture and
local runtime reruns remain pending. The [M22 SDK/Godot workflow](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37120341700)
passed 123 public C# checks, ten public Python checks (eight optional original
checks skipped), zero build warnings/errors and both UI suites with zero failures.

## M21 historical checkpoint

M21 local validation passed: 120 C# checks (118 public), all 18 Python checks
with the local original, Godot C# compilation with zero warnings/errors, and both
UI suites with zero failures. Coverage includes all recall outcomes, live five-week
countdowns, refunds alongside ongoing sales, reprint guards, signed/negative
financial cases, legacy unknowns and both saved event dialogs.

The [M21 SDK/Godot workflow](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37118760081)
passed 118 public C# checks, ten public Python checks (eight original checks
skipped), a normal build with zero warnings/errors and both UI suites with zero
failures.

## M20 historical checkpoint

M20 local checks passed: 113 C# checks (111 public), all 18 Python checks with
the optional original fixture, direct Godot C# compilation and both Godot UI
suites with zero failures. Coverage adds original graph thresholds, signed weekly
counts, exact budget-boundary ordering, saved histories and legacy guards.
The [M20 standard SDK/Godot workflow](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37117477406)
passed 111 public C# checks, all ten public Python checks, a normal build with
zero warnings/errors and both UI suites with zero failures.

## M19 historical checkpoint

M19 local validation passed: 109 C# checks (107 public), all 18 Python checks
with the optional original fixture, direct Godot C# compilation, synthetic
gameplay UI and original-import UI with zero failures. Coverage adds company-wide
reprint limits, the exact budget reset, modal pauses and saved production state.
M18 technology-use and M19 production evidence anchors also match the original.
The [M19 standard SDK/Godot workflow](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37116610923)
passed 107 public C# checks, all ten public Python checks, the normal Godot build
with zero warnings/errors and both UI suites with zero failures.

## M18 historical checkpoint

The [M18 SDK/Godot workflow](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37091862119)
passed 104 public C# checks, all ten public Python checks (eight original checks
remain opt-in), a normal Godot build with zero warnings/errors, and both UI
suites with zero failures.

Coverage adds application costs, six effect formulas, diminishing returns,
original random-width differences, flooring, shared-counter timing, save/load,
desk lifecycle, stress relief and guards for unresolved branches. The UI runs
active-application save/load and live compression through the actual Godot game.

M18 was initially authored and verified through GitHub while the local execution
workspace was disconnected. After recovery on 2026-10-03, the local checkout was
reconciled with merged M18. All 106 C# checks (including both optional original
checks), all 18 Python checks with the local original, direct Godot C# compilation,
the synthetic gameplay UI and the original-import UI passed. Both UI suites
reported zero failures. Original Windows behavioral comparison remains outstanding.

## M17 historical checkpoint

M17 local checks passed: 98 C# checks (96 public), 18 Python checks with the
local original, direct Godot C# compilation, and the complete synthetic gameplay
UI with zero failures. The UI now covers technology purchase, course acceptance,
mid-course save/load, completed study and study-seat persistence. The [M17 SDK/Godot workflow](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37090668093)
passed 96 public domain checks, a build with zero warnings/errors, and both UI
suites with zero failures.

## M16 historical checkpoint

M16 local checks passed: 90 C# checks (88 public), all 17 Python checks with the
local original, Godot source-generator compilation with zero warnings/errors,
and the complete synthetic gameplay UI including all company actions and shared
budget save/load/reset with zero failures. The [M16 SDK/Godot workflow](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37089203543)
passed 88 public domain checks, a normal build with zero warnings/errors,
and both UI suites with zero failures.

## M15 historical checkpoint

M15 local checks passed: 85 C# checks (83 public), all 17 Python checks with the
local original, and the complete synthetic Godot gameplay UI including dismissal
confirmation/cancellation, reservation release and save persistence with zero
failures. The [M15 standard SDK/Godot workflow](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37088101468)
passed 83 public domain checks, ten public Python checks, the normal SDK/Godot
build with zero warnings/errors, and both UI suites with zero failures.

## M14 historical checkpoint

M14 local checks passed:

- 78 C# checks, including the optional imported-original full core cycle and parity.
- All 17 Python checks with the local original fixture.
- Godot C# source-generator compilation with zero warnings/errors.
- Synthetic Godot development/release/sales UI: zero failures, now including review
  save/load and acknowledgement plus all four consecutive sales milestone dialogs.

The public domain suite contains 76 C# checks. Original fixture checks remain
opt-in and no original content is distributed. The [M14 standard SDK/Godot workflow](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37087184742)
passed: 76 public C# checks, ten Python checks (seven optional checks skipped),
normal Godot SDK build with zero warnings/errors and both UI suites with zero
failures. The local imported-original UI suite also passed with zero failures.

## M13 historical checkpoint

M13 local checks passed:

- 71 C# checks, including an end-to-end imported-original development/release/sales
  cycle with ordinary lounge rest management and C#/Python catalog parity.
- 17 Python checks, including seven optional original-evidence/import checks.
- Godot C# source-generator compilation with zero warnings/errors.
- Original-import UI suite with zero failures.
- Synthetic-data Godot core-loop UI suite with zero failures: development,
  checkpoint save/load, type/genre selection, debugging, release, sales revenue,
  reprinting and a second project. Synthetic fixtures are authored test data.

The public suite has 69 C# checks and ten Python checks, with seven original
checks skipped when no local fixture is supplied. The [M13 standard SDK/Godot workflow](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37077097177)
passed: 69 public C# checks, ten public Python checks (seven optional checks
skipped), normal SDK build with zero warnings/errors, headless editor/game
startup, fixture-free UI smoke and complete core-loop UI smoke with zero failures.

## M12 historical checkpoint

The recovered final M11 source was retested after the execution workspace resumed:
52 C# checks and all 16 Python checks passed with the optional local original.
This closes the earlier pending accuracy-cell/computer/evidence-verifier checks.

M12 local checks passed:

- 58 C# checks, including the private original-import parity check.
- 16 Python checks, including all six optional original-evidence checks.
- Godot C# source-generator compilation: zero warnings/errors.
- Imported-data Godot UI suite: zero failures, including desk assignment,
  occupied-seat handling, seated save/load and returning to the lounge.
- Private opening/hired/desk compositor frames inspected.

The current public suite has 57 C# checks and ten Python checks. Original fixtures
are local-only and are not uploaded to CI. The M12 [normal GitHub SDK/runtime checks](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37068490579)
passed before merge: 57 public C# checks, ten public Python checks (six optional
original checks skipped), standard SDK build with zero warnings/errors, Godot
headless editor/game startup, and fixture-free UI checks with zero failures.

The preceding M11 normal [GitHub Validate workflow](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37035351393)
passed the standard .NET 8/Godot SDK build with zero warnings/errors, headless
editor/game startup and fixture-free UI checks.

## Local host limitations

The local host has no /proc. Its normal SDK/MSBuild process fails during runtime
or process-metadata queries. Direct Roslyn compilation uses .NET 8 references
and Godot 4.4.1 source generators; temporary private native shims allow compiler
and headless runtime checks. No shims, assemblies or toolchain are distributed.
A headless editor scan refreshed translations, but its SDK plugin fails locally.
That scan is not a successful local standard build; GitHub CI verifies that path.

## Remaining validation

- Original Windows behavior and original RNG sequence reproduction.
- Whole-window graphical rendering, keyboard/pointer usability and packaging.
- Original save fixtures for compatibility import.
- Complete calendar economics, recalls/reviews/awards and company progression.

No original binary, graphics, audio, imported catalog or private save is included.
