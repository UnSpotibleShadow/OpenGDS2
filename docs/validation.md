# Validation — M10 continuation

M10 adds office compositing, date glyphs, employee entry/lounge animation and a
classic control theme. The final checks currently pass:

- **33 C# checks**, including the optional local original fixture; 32 without it.
- **13 Python checks** with the local original fixture; 8 public checks and 5
  opt-in checks skipped when no original is supplied.
- Every C# source compiled with Godot 4.4.1 source generators: no errors.
- Imported-data Godot headless scene-tree suite: **0 failures**, including actual
  40 ms office animation, lounge arrival, persisted worker movement, texture
  decoding and office/sidebar viewport bounds.
- Private composed opening/hired-worker images inspected; no original graphics
  are shipped. Whole-window desktop rendering remains unverified.

## Environment workaround and scope

The environment has no `/proc`. The downloaded ordinary .NET SDK initially
failed during CoreCLR initialization (`0x8007000E`). An alternate Mono .NET 8.0.15
runtime also initially failed on its `/proc` check and glibc main-thread stack
metadata retrieval.

A temporary local native shim derived the stack's mapped upper boundary through
`mincore` and its size through `getrlimit`, and bypassed Mono's existence-only
`/proc/self/maps` startup check. With this shim, Roslyn 4.11, the C# test executable
and Godot headless scene-tree tests ran. Compiler options, .NET 8 references and
Godot source-generator inputs were supplied directly. The runtime, shim,
compiled assemblies and user data are outside the deliverable.

**Standard `dotnet build`/MSBuild remains unverified here.** SDK/MSBuild startup
still fails while querying process metadata from missing `/proc` files. Direct
compiler success does not establish the complete SDK restore/build pipeline.
The supplied CI workflow runs normal builds and tests without this workaround;
it has not been remotely executed.

M9 also attempted a headless editor refresh. Asset/translation scanning completed,
but the Godot editor's SDK discovery/MSBuild plugin reported missing SDK/assembly
errors in this host setup. This is not a successful editor build. Direct source
compilation and the game scene-tree tests afterward passed.

## Remaining verification

The office presentation correction compiles without errors and passes the
Godot imported-data UI suite (0 failures), including decoding the embedded
466 × 290 office bitmap and local sprite extraction. The no-data UI suite also
passes (0 failures): New Game opens the importer rather than an empty company.
Graphical rendering is still unverified in this host.

- Standard SDK restore/build and CI on a normal .NET-capable host.
- Graphical layout, native file picker, pointer/keyboard usability and packaging.
  Headless control tests do not prove rendered layout quality.
- Original Windows runtime comparison: recruitment draws, initial staff assignment,
  workspace capacity, weekly finance/contract behavior and RNG sequences.
- Original save fixtures to verify serialization/encoding before compatibility import.
- A verified development/release/sales loop and company progression are still
  required before declaring the project gameplay-complete.

No original executable, graphics, sounds, imported catalogs or private saves are
included in the public archive. Static reports retain structures/anchors rather
than redistributing a wholesale decoded original program.
