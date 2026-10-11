# Financial failure — M84

The supported extracted executable has SHA-256 `4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`. This implements the branch recovered in [M75](static-recovery-2026-10-10.md) and [M76](workflow-verification-m76.md). Original binaries, dialogue and full traces remain local.

## Recovered rules

| Rule | Static evidence |
| --- | --- |
| Strict funds below zero; zero survives; no new mode-specific exemption | Continuous work loop `00A0283C +CE69..CE76`, funds `A1103C` |
| Two introductions before employee scanning | `+CE77..CF20`, work pool `5F99AC` cells EF/F0, secretary portrait -2 |
| Scan source slots 0 through source upper bound minus one | `+CF21..CF35`, bound `A11A7C`; gate `A11CA0` at `+CF3B..CF4B` |
| Consume floor(Rnd × 2) for every slot, including unemployed slots; gate exactly 1 plus draw 1 selects a reaction | `+CF4F..CF73`; VB And evaluates both sides |
| One additional floor(Rnd × 8) for each selected employed slot | `+CF76..CF87`; eight branches through `+D33F` |
| Explicit source name and portrait; no random-employee selection helper or payroll-scratch rewrite | Name `A11AE0`, portrait `A11C30`; work-pool D6/D7/D9 captions and F3–FA reactions |
| Closing message after scanning | `+D348..D39C`, work-pool FB |
| Close ten distinct presentation forms (eleven calls, one repeated), stop timer, hide toolbar, disable gameplay/info/window/setup/save and restore title/start/load | `+D39D..D5FA`, M76 control resolution |
| Preserve company funds, accounting, staff, development, facilities and inventory | Closure handlers contain presentation cleanup; no recovered gameplay wipe, cash clamp, refund or new finance-helper call |

`tools/verify_missing_systems_evidence.py` verifies the reviewed whole-procedure fingerprints, negative-cash/draw anchors, all dialogue cells and source-array mappings without printing original strings. `tools/verify_workflow_lifecycle_evidence.py` verifies the timer/menu control anchors and reviewed closure handlers.

## Implementation and explicit modern adaptations

`FinancialFailureService` begins at the end of `GameSession.TickOffice`, after weekly work, periodic lab handling, sales and employees. Existing suspended transactions finish before that check. The original runs a continuous procedure with modal callbacks; this port resumes discrete ticks. It does not claim native reentrancy or global VB6 random-stream parity.

A modern saved continuation retains two introductory phases, an ordered source-roster snapshot, the next source cursor, the currently selected reaction, closing and completed phases. Selection draws occur only as the introductory/reaction pages are acknowledged; reloading a chosen page does not reroll it. Existing modern random generation is retained. Snapshotting captures identity/gates for this frozen presentation; it is not an original save layout.

Once failure starts, office simulation and gameplay controls stop. Explicit source portraits preserve the payroll scratch; rebuilding the desktop for a saved failure does not refresh the staff-list scratch. Cash and accounting are not changed by dissolution. Projects, including hardware, and experiments remain in memory. Finishing the closing message destroys the modern desktop and returns to the title. New-company and load entry points remain usable; a new company receives no failure continuation. Title settings remain a modern convenience.

Original saving is disabled after closure, and the original has no saved bankruptcy flag. Modern saving is available while messages are pending, consistently with other resumable workflows. `SaveStore.Save` rejects completed failure before writing or replacing files; serialized terminal state can still be validated internally. Existing saves and their backup are preserved. Loading an earlier pending closing-page save presents that page again, then returns to the title. Malformed cursors, reaction variants, snapshots and overlapping suspended transactions are rejected.

Older modern saves missing complete original source slots or employed portraits receive the two introductions, then an explicit recovery message with Load/Back to title. They do not fabricate slots, employment, portraits or random draws. Starting a fresh company with the supported archive provides the full roster.

Dialogue cache schema 11 adds thirteen local keys (186 total). Japanese uses the locally imported native lines when present; English and old-cache Japanese fallbacks are authored summaries, not copied dialogue. Reimporting the supported LZH refreshes the local dialogue cache.

## Validation

- 373 domain tests, including optional original import; ten new financial-failure tests cover strict zero boundary, both modes, slot order, unemployed draws, all eight reactions, resumable pages, unknown legacy rosters, frozen data, modal ordering, malformed state and save protection.
- Zero-warning application build; English/Japanese UI checks cover introductions, saved eighth reaction, explicit portrait, frozen clock, blocked gameplay/setup, closing/title return, earlier-save recovery, unknown-roster disclosure and actual negative/zero office-clock triggers.
- M75/M76 source verifiers pass; ten packaging tests pass. Package/title/README disclosures now reflect this implementation.

Native Windows modal timing, graphical acceptance and exact random-stream parity remain comparison work. RC.4 is unchanged and predates this source; the final release must be rebuilt. Office relocation and company-rank progression remain unestablished source workflows, not invented features required by this change.
