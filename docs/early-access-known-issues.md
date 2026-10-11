# Known limitations — current source / historical RC.4

The game requires the supported, locally imported GameDev2DX.lzh. The title/settings work without it; gameplay needs the import. No original files are bundled.

| Area | Candidate boundary |
| --- | --- |
| Campaign | Beginner ends after Year 2, December, week 4. Normal is experimental ongoing play. Bankruptcy and office/company upgrades are incomplete; negative funds do not end the game. No complete original endgame is claimed. |
| Development | Original projects, eligible Hall of Fame sequels, manga adaptations, meetings, cancellation, debugging and shipment are supported. Polishing and external/contract work are unavailable. |
| Economy | Verified payroll/calendar events, borrowing and annual repayment are supported. Some upstream economy/menu gates and payroll scratch writers need original comparison; missing data pauses with an explanation rather than fabricated values. |
| Facilities | School, hot springs, recycling, technology capacity, production expansion, lawyer consultation, mystery-lab experiments and own-hardware development/launch are supported with saved results. Casino remains unavailable. Student-growth effects are incomplete. |
| Fidelity | Automatic conversation scheduling/cadence, some growth effects/form geometry/bubbles/bitmap labels and provisional English name readings remain open. Existing English facility vacancy signs are translated locally. |
| Saves | Modern JSON only. Saving retains one previous valid slot; the title's Load Previous Save restores earlier progress without changing the current file until explicit Save. Original save import is unsupported; future versions may require a fresh company. Back up saves before upgrading. |
| Package | Windows x86_64 standard SDK/Godot release export, matching official Godot 4.4.1 .NET templates and .NET 8.0.31. All 25 exact OGG sources are locally verified and resources regenerated. Build-host adapters are excluded. |
| Acceptance | Automated domain, source-host and exported-host checks do not establish native Windows acceptance. Full graphical, multi-year and sustained Windows testing is assigned to the community; untested behavior is disclosed rather than treated as accepted. |

The title and mode selection disclose the campaign boundary. Detailed unresolved evidence and release decisions are in DECISIONS.md / `early-access-decisions.md`. TEST_CHECKLIST.md guides community testing; its full completion is not a pre-release requirement. Known crashes, save corruption, duplicate transactions and supported-workflow dead ends need fixing before publication. The target is 16 October 2026, 19:00 CEST; the exact final artifact is still to be selected.

M80 fixes unsupported payroll-state resets when visiting ordinary technology screens. M82 adds mystery-lab experiments; M83 adds own hardware and protects bank borrowing during pending results. These source changes are not in the existing RC.4 ZIP; use the final package’s manifest to identify the code actually shipped.
