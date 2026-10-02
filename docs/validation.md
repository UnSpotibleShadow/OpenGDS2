# Validation — M11 checkpoint

The final functional source at commit 2b5185a passed the repository's normal
[GitHub Validate workflow](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37035351393):

- Standard .NET 8/Godot SDK build: zero warnings, zero errors.
- 51 public C# checks passed.
- Ten public Python checks passed; six original-fixture checks were skipped.
- Godot 4.4.1 .NET headless editor/resource import and game launch passed.
- Fixture-free Godot UI suite: zero failures.

This verifies the standard SDK/MSBuild pipeline on GitHub's Ubuntu runner,
including the new project/save models, sparse accuracy projection, route graph,
planning UI and computer presentation code. CI contains no original assets.

Before the local workspace disconnected, an intermediate continuation passed
50 C# checks including the original fixture, 15 Python checks including five
opt-in checks, direct Godot C# compilation and the imported-data Godot UI suite
with zero failures. Those UI checks exercised planning quotes/focus, imported
employee portraits, hiring, the actual 40 ms office timer, save/load and bounds.
The final computer sprite/accuracy-cell changes and new evidence verifier still
need renewed local original-fixture checks; CI success does not cover them.

The final suite has 52 C# checks with its optional original fixture and 16 Python
checks with all six original checks enabled. These totals are available coverage,
not a claim that the final optional suite has run.

## Local host limitations

The local host has no /proc. Its normal SDK/MSBuild process failed during runtime
or process-metadata queries. Direct Roslyn compilation used .NET 8 references
and Godot 4.4.1 source generators; temporary private native shims allowed compiler
and headless runtime checks. No shims, assemblies or toolchain are distributed.
A headless editor scan refreshed translations, but its SDK plugin failed locally.
That scan is not a successful local standard build; the successful standard build
is the remote CI run linked above.

The execution service subsequently reported environment_offline. The authored
checkpoint was recovered through the GitHub connection from the exact M10 base,
then verified using CI. Private original files remain outside GitHub.

## Remaining validation

- Original Windows behavior and original RNG sequence reproduction.
- Imported-data checks after the final sparse accuracy/computer sprite changes.
- Graphical rendering, keyboard/pointer usability and packaging on a normal host.
- Original save fixtures for compatibility import.
- A verified development, release and sales loop, calendar economics and progression.

No original binary, graphics, audio, imported catalog or private save is included.
