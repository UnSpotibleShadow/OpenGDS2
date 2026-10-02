# Validation — M11 checkpoint

Before the workspace disconnected, the continuation passed 50 C# checks including
the local original fixture, 15 Python checks including five opt-in checks, direct
Godot C# compilation, and the imported-data Godot UI suite with zero failures.
The UI checks exercised planning quotes/focus, imported employee portraits, hiring,
the real 40 ms office timer, save/load and viewport bounds.

The final branch contains 51 public C# checks (52 with the optional original
fixture), ten public Python checks and six optional original checks. Computer
sprite selection, preserved fresh-game accuracy cells and the new evidence
verifier were added after the last local test run. Their final verification is
pending GitHub CI and renewed local original-fixture checks.

The execution service then reported environment_offline. The source checkpoint
was recovered into GitHub from the authored changes and the exact M10 base.
Its final source must be verified by the normal SDK workflow, rather than assuming
the earlier local test run covers every final change.

## Build verification limits

Earlier local checks used direct Roslyn compilation with Godot 4.4.1 source
generators and .NET 8 references. This host has no /proc, and its normal SDK/MSBuild
pipeline failed during runtime/process metadata queries. Temporary private native
shims allowed compiler and headless runtime checks; no shims, assemblies or toolchain
are distributed. A headless editor resource scan refreshed translations, but its
SDK/MSBuild plugin failed, so that scan is not a successful standard build.

The repository's Validate workflow runs normal .NET domain tests, Python tests,
dotnet build, Godot headless editor/import and launch, and fixture-free UI checks.
Remote CI must be checked separately. It cannot validate the copyrighted optional
fixture, which must remain local.

## Remaining validation

- Original Windows behavioral comparison and original RNG sequence reproduction.
- Imported-data checks after the final sparse accuracy/computer sprite changes.
- A graphical host for full window rendering, keyboard/pointer usability and packaging.
- Original save fixtures for compatibility import.
- A verified development, release and sales loop, calendar economics and progression.

No original binary, graphics, audio, imported catalog or private save is included.
