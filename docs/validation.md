# Validation — M16 checkpoint

M16 local checks passed: 90 C# checks (88 public), all 17 Python checks with the
local original, Godot source-generator compilation with zero warnings/errors,
and the complete synthetic gameplay UI including all company actions and shared
budget save/load/reset with zero failures. Normal SDK/Godot CI will be recorded
after publishing.

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
