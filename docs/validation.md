# Validation — M9 continuation

Date: 2026-10-02. Original executable inspected statically, never executed.

## Passed

- **27 C# tests**: 26 public tests plus optional local-original parity/extraction.
  Includes March-end renewal timing, atomic rejection, dismissal gates/stress,
  retained progression, stale identities and overflow guards.
  Includes computer purchase, license availability/payment/save state and planning quote rules.
  The latter compares all 60 employee records, 18 genres, 51 game types,
  7 audiences, 4 computers and 11 hardware records against independent
  Python extraction, extracts all 48 LHA entries with size/CRC validation, and
  compares the extracted executable bytes with the private original fixture.
- **12 Python checks**: 8 public tests and 4 opt-in original-fixture tests.
  Includes initializer shape, supported hash/tamper rejection, evidence anchors,
  localization key parity and exclusion of original assets from the public tree.
- Direct Roslyn C# compilation against .NET 8 reference assemblies, including
  every project source and **Godot 4.4.1 source generators**. No compiler errors
  or warnings in the final invocation.
- Godot **4.4.1 .NET** headless launch exited successfully.
- Headless scene-tree UI tests: title, English/Japanese settings, required-name
  validation, Normal startup, import, five applicant cards, hire, save/load,
  staff screen, computer upgrade, hardware license payment, persisted hardware
  state, loaded English/Japanese translations, calendar preview and return to office. Save checks use a disposable
  data directory. The imported cache contains no executable.
- A first-save regression was caught and fixed: the office refreshes after saving
  so Load Game becomes enabled immediately. The UI test exercises that transition.

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
