# Missing-system static recovery — 2026-10-10

This investigation follows the owner's request to recover missing original rules before further gameplay development. Static recovery is the primary method; Windows execution is a later validation step, not a prerequisite for discovering the programmed rules. No gameplay changes are made here.

The supported extracted executable is SHA-256 `4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`. Private procedure traces, original dialogue and reference-table downloads stay outside the repository. The new `tools/verify_missing_systems_evidence.py` checks whole-procedure fingerprints, shared-state mappings and narrow anchors without emitting original strings or traces.

## Financial failure: recovered trigger and terminal branch

The relevant branch is in Module2's continuous work loop `00A0283C`, not the smaller periodic routine `009E3830` previously prioritized by the backlog.

- At `+CE69..CE76`, company funds (`A1103C`) are compared with zero using a strict less-than predicate. Zero funds skip failure; negative funds enter it.
- The branch presents two introductory dissolution messages. It then scans the employee slots. Each slot consumes a `floor(Rnd * 2)` draw; an employed slot with result 1 gets a reaction selected by a further `floor(Rnd * 8)` draw. VB's combined expression evaluates the draw even when the slot is not employed.
- After the staff sequence, it presents a closing message, makes eleven unload-style calls covering ten distinct form globals, changes the main interface controls, restores the idle/start-or-load presentation and exits the work loop at `+D5FA`. The non-failure branch resumes at `+D5FB`.
- There is no funds clamp or financial-helper call in this branch. Unload-handler side effects and exact property meanings still need mapping before implementing persistence or claiming a complete reset rule.
- The work loop repeats through `+DD18` back to `+0009`. Its pause gate at `+0058..0065` skips simulation work to `+D6B1`. The failure predicate therefore belongs to an active loop iteration, not an immediate check inside every money-writing helper or solely a month-end check.
- The weekly routine is called at `+0351` before this later failure check. A debit during the weekly pipeline can therefore be observed later in the same active iteration. Earlier terminal events and reentrant modal/unload behavior still require caller-level analysis.

This establishes financial failure as an original feature. It does not close U01: implementing it still requires the closure-handler mapping, modal continuation and saved terminal-state design.

## Mystery laboratory: recovered selection, progress and effects

Start handler `00987B40` rejects an already-positive pending progress value. If confirmation accepts, it sets progress (`A11548`) to 1, stores `floor(Rnd * 7)` in `A11558`, and directly subtracts the configured cost from funds. The initializer sets that cost to 25,000,000. The handler calls the existing confirmation machinery; its affordability semantics remain to be followed through that helper.

Periodic routine `009E3830 +07BF..0940` requires a tenant entry of type 6 and positive progress. It adds 0.2, dispatches meeting mode 15 once progress is strictly greater than 100, then clears progress. Removing the matching tenant skips this block; that observation does not by itself establish whether another closure handler clears or preserves pending work. Do not convert this increment into a promised wall-clock duration or invent cancellation/refund rules.

Meeting timer `009EFA40 +5ABD..60A9` reads the previously selected result and applies:

| Selected result | Effect |
| --- | --- |
| 0 | Funds become `floor(funds * 1.5)` |
| 1 | Funds become `floor(funds / 2)` |
| 2 | Production capacity doubles |
| 3 | Production capacity becomes `floor(capacity * 0.8)` |
| 4 | Recognition becomes 80 if previously below 80; otherwise 99 |
| 5 | Employed staff stress becomes 0 |
| 6 | Employed staff stress becomes 90 |

Funds effects write the funds cell directly, without the shared accounting helper. Preserve that distinction when implementing the original accounting behavior.

A further result-7 handler exists and liquidates qualifying product stock for 5,000 per unit through the financial helper. The normal start selector cannot generate 7. Treat it as a latent handler until another writer or save-import path establishes reachability; it is not an eighth normally selectable outcome.

The timer's 27,016-byte descriptor includes one trailing `FC` byte after its final exit at `+6986`. The private decoder leaves that byte uninterpreted. The result block is before it; the verifier fingerprints the entire descriptor span. No claim of a general, fully validated VB6 decoder is made.

## Office progression: verify existence before inventing a feature

The expansion menu `MDIForm1 009819A4` opens the already-reconstructed company-activity form, and its established effect is production-capacity expansion. It is not evidence of buying a larger office.

A separate office purchase/relocation system has not been established. The earlier completion plan's proposed upgrade prices, layouts and capacity transitions were requirements to investigate, not recovered facts. Continue tracing original company-rank writers, menu guards, initialization and artwork/layout state. Do not infer a PC-version feature from later games in the series, and do not interpret lack of a keyword or a partially decoded procedure as proof that a feature is absent.

## Remaining static work before gameplay implementation

1. Map financial-failure form globals, unload handlers and main-control property changes; establish event interruption and mode/caller boundaries.
2. Finish mystery-lab confirmation, closure/reconstruction, saved pending work and shared-RNG ordering. Keep the latent result separate.
3. Resolve company rank and any actual office-layout progression; revise U01 scope if source evidence establishes that no such workflow exists.
4. Trace remaining payroll scratch writers, bank eligibility, school growth and unresolved technology effects.
5. Recover own-hardware, polishing/contract work and casino lifecycles, including refusal/failure paths and their calendar consumers.
6. Investigate original saves and RNG state separately from modern persistence. Runtime comparison can validate the completed interpretation afterward.

Fresh validation: the new evidence verifier passes against the supplied supported original. These are source-evidence findings, not a fresh .NET/Godot build or original Windows playthrough. The existing RC4 limitations remain in effect until gameplay implementations and their checks are complete.
