# OpenGDS2 architecture

## Principles

1. **No original assets in Git.** Players import their own `GameDev2DX.lzh` into `user://original`.
2. **No execution of the VB6 binary.** The compatibility engine reads data/assets and independently reimplements behavior.
3. **Domain logic is UI-independent.** Reconstructed rules live under `src/Domain`; Godot nodes are thin views/controllers.
4. **Stable localization IDs.** UI code refers to IDs (`NEW_GAME`, `HIRE_EMPLOYEES`) rather than hard-coded source strings.
5. **Behavior before embellishment.** Match original mechanics with evidence/regression fixtures before optional modernization.
6. **Unknown means unknown.** Missing values stay nullable/disabled rather than becoming plausible-looking guesses.

## Runtime flow

`GameDev2DX.lzh` → `LhaArchive` → `OriginalGameImporter` → `user://original` → `OriginalAssetStore` → Godot UI.

The importer currently extracts BMP, WAV and GDS entries only. It deliberately skips the original game executable.

## Reverse-engineering flow

`GameDev2DX.exe` (local research copy) → static Python tools → structural JSON/TSV evidence → clean C# domain model/UI.

Important tools:

- `inspect_vb6.py`: VB project/object/control/event/P-Code map.
- `scan_startup_pcode.py`: narrow verified startup-branch interpretation.
- `scan_form_serialization.py`: length-prefixed CP932/ASCII designer-string recovery for one form block.
- `scan_cp932_strings.py`: broader heuristic Japanese-string discovery.

## Current domain boundary

M4 models the original PC employee fields recovered from `fGMSKoyou`/`fGMSKoyouKz`:
Level, Fun, Novelty, Accuracy, Graphics, Music, Development Speed, Inspiration and Contract Fee.

The applicant database and generator are reconstructed. Recruitment populates
`RecruitmentState.Candidates`; hiring charges the recovered fee and creates an
independent employed record with an office workspace.

## Compatibility milestones

- M1: LZH import + title shell (implemented).
- M2: company/new-game flow (implemented).
- M3: VB6/P-Code object map + first office state (implemented).
- M4: employee-hiring form/stat schema (implemented).
- M5: applicant generator + hiring transaction + active employee roster.
- M6: supplied baseline; development remained a reconstruction target.
- Later: complete economics, reviews/awards, progression, original-save compatibility and isolated modernization.

- M13: `DevelopmentSimulation` schedules seated own-game work and debugging.
  `ReleaseService` snapshots projects and charges production; `SalesService`
  advances inventory, receipts and demand. `Main.CoreLoop.cs` presents checkpoints
  and release/sales controls without embedding formulas in Godot nodes.
  New fields remain inside the backward-readable schema-1 modern envelope.
