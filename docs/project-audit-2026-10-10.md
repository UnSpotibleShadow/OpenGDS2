# OpenGDS2 state and original-fidelity audit — 2026-10-10

Audited source: `cd82cea6a5e51385f0c6b0ac9f7e8971f79b5c09` (main, M73 / RC.4).
This audit follows the owner's request to assess the project before resuming backlog work.

## Assessment

OpenGDS2 is a substantial playable reconstruction of the original own-game loop, not yet a complete recreation of the original campaign. The largest remaining differences affect progression and alternative workflows, rather than simply presentation. The original evidence supporting implemented rules is reproducible against the files supplied for this audit. Full behavioral parity is still unestablished: matching executable anchors and synthetic tests do not demonstrate a complete original campaign or graphical equivalence.

The newest handoff is issue #55 and the M73 decision/export documents. README's opening still identifies RC.1 and its final section RC.3; development-checkpoint.md opens at M62; original-comparison.md's main table describes M32–M36 with an M37 addendum. Several rows describe payroll, loans and hardware events as unconnected although the current code implements them. Use those documents as historical evidence, not current missing-feature lists.

## Fresh verification

- Restored source through the owner's existing GitHub connection at the immutable audited commit. All 300 restored text files matched their Git blob SHA-1; none differed. This is a source audit snapshot, not a full runnable checkout: 25 community OGG sources and some UID/import metadata were not restored.
- Supplied LZH SHA-256: `118fc868e3b888ec86b9dcd958f52109d0074717e8e78d9fbc44bd03ecdfc0b4`.
- Supplied distribution EXE SHA-256: `61b2a2352515ee354f35f2c0615a5d018c9312517cbe19ab2b9f0314fe587901`.
- Extracted game EXE SHA-256: `4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.
- All three match docs/source-hashes.txt. Extraction used tools/extract_lzh.py; private originals remained outside the source tree.
- All 20 `tools/verify_*.py` programs passed against the extracted executable. Coverage includes the core loop, facilities, lawyer consultations, cancellation, bursts/shipment boundaries, Beginner ending, bank, office, development, meetings, secretary behavior, sequels and manga.
- Selected Python suites: **60 tests run, 59 passed, one failed**, including all 16 opt-in original-fixture tests. Suites: test_reconstruction, test_bounded_process, test_local_artwork_patch, test_package_desktop and test_music_import.
- Connector-guard JavaScript test file passed.
- Fresh C# domain tests, Godot C# runtime/UI tests, audio-source checks, Windows exports and original Windows execution were **not run**. This workspace has no installed .NET SDK; its available Godot is 4.6.3 rather than the project's 4.4.1 .NET toolchain. An SDK download attempt failed under the command network restrictions; a later network-permission request was cancelled before execution. This audit does not substitute a different Godot version or report historical checks as fresh results.
- M73 previously recorded 334 domain tests, source/exported-host checks and a clean Windows export. Those are historical checkpoint evidence only. Native Windows RC.4 acceptance remains open.

## Confirmed fresh test failure

`tests/test_reconstruction.py::PublicTests.test_localization_key_parity` fails because ten English cancellation keys are absent from ja.po: CANCELLATION_EMPLOYEE_NAMED, CANCELLATION_REACTION_0 through _6, CANCELLATION_STRESS_SUMMARY and CANCELLATION_DISCARD_SUMMARY.

This is consistent with the intentional local-original-text boundary: Main.CoreLoop.cs loads Japanese cancellation text from imported dialogue and shows CANCELLATION_REIMPORT_REQUIRED when unavailable; OriginalDialogueImporter.cs extracts the corresponding keys. Both translation catalogs have unique keys. No Japanese runtime failure is established by this test.

Next correction: make parity validation explicitly recognize this narrow imported-only key set, assert its import/UI fallback contract, and retain strict parity for other authored keys. Do not copy original Japanese dialogue into the repository just to make the test pass. The existing failing test should not be described as a green full Python baseline.

## Comparison with the original

| Area | Current reconstruction | Remaining original-fidelity boundary |
| --- | --- | --- |
| Ordinary development | Planning, type/genre decisions, work, technologies, debugging, cancellation, shipment, reviews, sales/reprints/recalls | Full original runtime cycle, exact timing and shared RNG sequence |
| Sequels and adaptations | Hall of Fame lineage/inheritance and manga rights/development are connected | Remaining scene/cosmetic draws and runtime comparison |
| Calendar and finances | Payroll, annual renewals, hardware news/markets, loans/repayments, exhibitions, awards, rival history and rankings are connected | Payroll scratch writers, bank-menu eligibility and sustained campaign comparison |
| Campaign and office | Beginner closes at Y2 December week 4; Normal continues experimentally | Bankruptcy and office/company progression are missing; negative funds do not end Normal |
| Facilities | School construction/closure, hot springs, recycling, technology/production capacity and lawyer consultations | Casino, mystery lab and hardware lab services are disabled; student growth incomplete |
| Other development | Some low-level polishing arithmetic exists | Polishing and external/contract work have no complete enabled lifecycle |
| Staff and technologies | Hiring, assignment, dismissal, levels, study, secretary policies and six project-technology effects | Opening staffing/development power, capacity/failure paths, speed effect and level-three stress relief remain unresolved |
| Presentation and interaction | Imported office art, compact windows, meetings, bursts and saved dialogue cursors | Original automatic presenter/modal cadence, right-click paths, overlap priority, remaining bitmap labels and exact Windows display/font behavior |
| Saves | Validated modern JSON and previous-valid-save recovery | Original game-save import and serialization compatibility unsupported |
| Audio and localization | English labels plus local Japanese import; optional authorized community music | Community music is an intentional modern addition, not original parity; some names/bitmap captions remain provisional |
| Distribution | RC.4 clean Windows source export documented | Fresh Windows import/graphics/input/audio, modal reloads and multi-year acceptance still pending |

Code anchors: src/Core/GameSession.cs (opening/tick loop), src/Domain/CalendarService.cs (calendar transactions), OriginalPayrollRules.cs (shared scratch behavior), FacilityService.cs and src/UI/Main.Progression.cs (enabled facilities), DevelopmentSimulation.cs and TechnologyApplicationService.cs (supported work/effects), src/Persistence/SaveStore.cs (modern saves), src/UI/Main.CoreLoop.cs and src/Import/OriginalDialogueImporter.cs (cancellation text).

## Recommended backlog order

1. Correct the failing localization test contract and establish one accurate current audit/checkpoint entry; retain historical milestone evidence.
2. Complete RC.4 native Windows acceptance using early-access-testing.md. Prioritize any reproducible crash, save corruption, repeated transaction or supported-workflow dead end.
3. For full reconstruction, investigate bankruptcy and office/company progression (U-01), then remaining payroll/bank/progression dependencies (U-02, U-07, U-08, U-15). These affect ordinary campaigns more broadly than adding another optional facility.
4. Continue mystery-lab delayed outcomes (U-03) as the next bounded optional facility; recover all seven outcomes and calendar/save consumers before exposing it.
5. Build complete own-hardware, polishing/external-work and casino lifecycles from recovered evidence.
6. Use targeted original Windows comparisons for RNG/timing, automatic conversation and graphical fidelity; static anchors alone cannot close those gaps.

No gameplay changes, Actions dispatches, desktop packages, releases, tags or visibility changes were made during this audit. No original binaries, assets, strings, saves or full traces are included in this report.
