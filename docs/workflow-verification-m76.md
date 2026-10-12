# Workflow verification — M76

Casino follow-up: [M78 fidelity closure](casino-fidelity-m78.md) recovers the remaining source-level race, RNG, control defaults and timer lifecycle evidence. Windows input, numeric-format/coercion and rendering acceptance remains blocked by the documented runner failures.

Research completed 2026-10-11 against the supported 2001 PC executable. This supplements M75 and corrects earlier backlog assumptions. It changes research scope, not gameplay. Original files, text and complete traces remain private.

## Method and limits

The extracted executable is 6,365,184 bytes, SHA-256 `4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`. The ownership-filtered VB6 metadata has 86 objects and 669 procedures, totaling 571,148 descriptor bytes. All metadata-listed control events are represented. Private decoding covers all 669 procedures with no nonterminal decoder errors; at most three descriptor-tail bytes following a terminal exit remain uninterpreted.

The investigation indexes direct global references, explicit company-member references and resolvable p-code call thunks. Negative conclusions use state writers, readers, form/menu entry points and serialization, rather than text searches alone. This is not a complete interpreter or proof against every indirect alias. “Unestablished” below means there is no recovered reachable workflow supporting the proposed feature in this executable; it does not mean a mechanic should be invented to fill the gap.

`tools/verify_workflow_lifecycle_evidence.py` checks the supported hash, procedure fingerprints and bounded numeric/control-flow anchors. It does not independently re-prove absence, execute the original or validate OpenGDS2 gameplay.

## Which proposed features have no supporting workflow?

| Proposal | Verified source findings | Reconstruction consequence |
| --- | --- | --- |
| Office purchase, relocation or rank progression | Expansion handler `MDIForm1 009819A4` selects production-capacity activity 2. Rank `A110DC`/member `00C0` has a reset (`009DD234 +0068`) and display read (`009DF294 +1B29`), but no recovered promotion writer. Rank is omitted by original save/load. Twelve desk/computer mappings, two development rooms and three tenant plots are initialized. | Production expansion is real. Additional office prices, rank thresholds, rooms and layouts are unestablished. Remove relocation from unconditional parity requirements. |
| External development contracts | Project selector handlers `0097C19C`, `0097C26C`, `0097BF94` select own game, sequel and manga modes. Room-state writers cover software, hardware, abort/release, reset and load. `fGMKeiyakuS 00999858` handles employee contracts. No independent external-job entry, state or settlement workflow was recovered. | External work is unestablished, not an implementation blocker for demonstrated original parity. |
| Separate polishing phase | The purported capped quality branch consumes room state 2. The only ordinary creator of that value is hardware acceptance `00990B70 +0221..022E`; its completion opens `fGMSyukkaH`. | Reclassify this branch as hardware development. No separate polishing entry/transition was recovered. |
| Periodic student skill growth | School recruitment occurs in the annual April/week-2 workflow. Direct primary-skill writers are employee level-up; member-based writers are record initialization/import/load. No periodic school stat-growth writer was recovered. | Recruitment is evidenced. Automatic student skill gains are unestablished. |

Explicit room-state stores are: software plan `0099BA80 +0045`, room cancellations `009AAA8C +081F`/`009AA1B8 +07FF`, software shipment `009C9CB8 +1057`, alpha/beta abort `009ACF14 +00F6`/`009B00D8 +00FC`, software-only periodic cancellation `009E3830 +2279`, hardware acceptance `00990B70 +022B`, hardware launch `0099F384 +054C`, plus company reset and original-file load. All ordinary stores write 0 or 1 except hardware acceptance, which writes 2. `009B15F0` reads the two room states for advice; it adds no writer.

## Bankruptcy and bank menu

M75's strict funds < 0 test is followed by the ten distinct form unload handlers described in the earlier local research. They clear presentation flags, and one preserves dialogue position. They do not wipe company funds, staff, projects or inventory.

The formerly unidentified main-interface calls at `00A0283C +D4A5..D551` now resolve to:

| Control | Change |
| --- | --- |
| Timer1 | Enabled = false |
| Picture1 toolbar panel | Visible = false |
| Actions, Info, Window, Setup and Save menus | Enabled = false |

Control resolution uses the metadata control index `(VCallAd operand - 0x2F8) / 4`, not the order in which controls happen to appear in an extraction. The later branch restores the title caption/background and start/load presentation, then exits at `+D5FA`. New/load entry points remain separate from the disabled save action.

No individual bank-menu enable writer was found in the decoded procedures. Its enclosing Actions menu is disabled by financial failure. Borrowing and repayment guards already recovered in the bank implementation remain independent transaction rules; do not invent a bank eligibility progression.

The main loop checks Timer1.Enabled at its top. UI pause/resume helpers `0097B500` and `0097B410` set that timer false/true. The separate user-pause cell is `A14174`. A timer change partway through an iteration does not itself branch out of that iteration. At `+0351`, the weekly routine runs only on the week boundary; `+0356` calls the periodic routine on every active iteration. Financial failure is checked later in that iteration, so money changed by a modal callback is observed by the later predicate. The weekly routine returning early does not independently stop its caller.

Original save is disabled after failure; there is no explicit saved bankruptcy flag. A modern resumable terminal-state representation is an implementation design decision, not an original field waiting to be discovered. Exact native event reentrancy still requires a runtime comparison; the caller/branch order above is recovered statically.

## Mystery laboratory

Start `00987B40` refuses positive pending progress, then on confirmation writes progress 1, draws one `Int(Rnd * 7)` outcome and deducts 25,000,000. The called confirmation/load helpers have no affordability guard: acceptance may make funds negative.

Window close `0097B6E0` and unload `00977B3C` resume the UI without canceling work. Demolition `00984354 +00A6..00B3` writes -1 to the tenant slot without refund or progress/result reset. Rebuilding via `00988F78` does not reset those two cells. The consumer requires a type-6 tenant, so demolition suspends progress and reconstruction permits resumption.

Consumer `009E3830 +07BF..0940` adds 0.2 on each active iteration, not each week. It dispatches mode 15 after progress > 100 and resets progress only after the modal returns. The seven regular result effects remain those in M75; the latent result 7 is not selected by the start draw. The completion block and its secretary dialogue calls introduce no additional result-selection draw. Other active-loop cosmetic/random activity still consumes the shared stream.

Repeated binary64 addition from 1 crosses 100 after 495 additions; exact rational arithmetic would require 496. This is a numerical consequence to preserve when choosing modern arithmetic, not a claimed original-runtime timing measurement. No fixed wall-clock duration is promised across pauses and modals.

Original save `009D9824 +03BB..03CD` and load `009D60E0 +033B..034D` explicitly serialize result member `053C` and progress `052C`.

## Shared payroll scratch: writer lifecycle and order

The payroll consumer `009DB4F4 +06B7..0770` uses employed gate count multiplied by salary at shared scratch index member `3018` (`A14034`). It is not the staff screen's sum of individual salaries.

Recovered writers reset the scratch count and increment it once per employed record:

| Writer | Trigger |
| --- | --- |
| Staff list `009B0B60 +0051/+00AA` | Opening/refreshing staff list |
| School-capacity selector `fGMTenantTechSel 0099F9A4 +02B9/+0312` | Building employed selection maps; already connected in M70 |
| Dialogue helper `009933DC +0049/+00A1` | Portrait argument -1, selecting a random employed person; secretary -2 bypasses this path |
| Secondary dialogue helper `009930BC` | Same random-person behavior; no resolved direct call-thunk caller found |
| Periodic routine `009E3830 +1AA7..1B03` | Poaching/emergency employee selection after its counter and random gate |

The active-loop caller runs the weekly routine before the periodic one. That periodic scratch write therefore cannot change payroll already charged in the same iteration, but can affect a later boundary. The three pre-payroll weekly dialogue calls use secretary -2 and do not rebuild the count.

No original save/load/reset reference to member `3018` was found. Saving it in a modern state is a modern continuity choice; it must not be described as an original save field. Do not add an unconditional scratch reset on new-company/load when no original writer establishes one. The apparent payroll index defect now has a concrete source-backed writer lifecycle; runtime comparison remains useful, rather than a prerequisite to recording the arithmetic.

## Technology consumers and apparent original defects

Speed technology's branch `00A0283C +5656..565F` assigns 10,000 to work-speed local `-0x0494`, then jumps to `+5698`. It bypasses both assignments to eligibility local `-0x04A4` at `+5688/+5694`. The eligibility check at `+56C3` therefore uses that local's retained value from a previously processed worker/pass. It is not equivalent to marking the current employee eligible unconditionally.

Top-level stress relief `fGMInfo 009A98E8 +06AA..06BE` reads local `-0x02B4` without any assignment in that procedure and uses it to index workspace mapping `A11E38`, then writes stress zero to the mapped source. Selection and dialogue use the selected employee elsewhere. The procedure frame is 560 bytes and the local is in its cleanup metadata.

Independent VB6 runtime reverse-engineering documents zero initialization of p-code local frames: [ProcDscInfo frame_size](https://docs.rs/visualbasic/latest/visualbasic/vb/procedure/struct.ProcDscInfo.html#method.frame_size). Under that documented runtime behavior, the unassigned local is Empty/numeric zero, so stress relief targets the employee mapped to workspace slot 0. The speed local also begins Empty and then persists for the continuous procedure call. These are static conclusions with an explicit runtime premise; no native reproduction is claimed. They should receive deliberate fidelity treatment rather than invented intended effects.

## Own hardware: entry through market

The type-8 facility entry `009864AC` rejects an already launched own console (`A11E50 == 1`) or either room already in state 2. Only one hardware project may run at once, and only one own console may be launched through the ordinary workflow.

Design selection `00991BE8` chooses room, console design and cartridge/CD medium. Each of the two design dimensions contributes 200,000,000 for value 0 or 100,000,000 for value 1. The quote is therefore 200,000,000, 300,000,000 or 400,000,000. The form's load path chooses room 0; do not assume it resets all shared design choices.

Acceptance `00990B70` rejects funds < quote and a nonempty selected room. Equality is allowed. It debits the quote through the financial helper, copies design/media choices and initializes:

- Room state 2, stage 0, required work 2,500 and completed work 0.
- Fun, novelty, music and graphics targets of 800 each; all four current values 0.
- Other explicit project bookkeeping cells as in the acceptance procedure.

Acceptance does not explicitly reset the four hardware technology flags. Ordinary company initialization resets their backing cells; an implementation must distinguish those two facts.

The common worker loop treats state 2 as hardware work. Target-directed category selection draws `Int(Rnd * 400)`, with ordered inclusive intervals for fun 0–50, novelty 50–100, graphics 100–150 and music 150–200, and progress/idle behavior outside eligible quality contributions. Ordered branches and full-target cases must be preserved rather than replaced by four equal probabilities. Common finalization clips qualities to their targets and work to its requirement. State-1-only software checkpoint/debug/virus paths do not become hardware phases.

Hardware application of technologies sets four flags: graphics ID 1 (`+759E`), music ID 3 (`+7E78`), fun ID 6 (`+834C`) and novelty ID 7 (`+880F`). Completion `00A0283C +AE26..AF8C` requires state 2, work >= requirement, all four qualities >= targets and each technology flag exactly 1. It selects the room and opens the modal hardware launch form. There is no additional fixed calendar duration in this predicate.

Closing monitor `0097B640` clears UI flags, not the project. Demolishing the lab `00984228` clears only the facility slot. The worker consumer does not require the facility to remain present, so hardware work can continue after demolition. No ordinary state-2 cancellation writer was found; company reset and original save loading are separate paths. Financial failure stops simulation without clearing that room state.

Launch `0099F384` uses the entered name, appends a hardware record via `009874E8`, adds production bands via `009845AC`, sets license index 11, ownership flag 1 and selected room state 0. No additional launch debit is present. It also updates the hardware-arrival ranking path. Shape/media presentation globals are captured by launch-form load `00994E78 +0164..017C`.

| Appended record field | Cartridge | CD |
| --- | ---: | ---: |
| Identifier `0EC8` | 11 | 11 |
| Medium/type `0FA8` | 3 | 4 |
| Bit width `0F00` | 64 | 64 |
| License fee `0FC4` | 0 | 0 |
| Availability `0F1C` | 1 | 1 |
| Market `1050` | 0 | 10 |
| Raw member `0F38` | 1 | 1 |
| Growth scale `0F54` | 50 | 36 |
| Planning points `0FE0` | 400 | 600 |
| Planning cost `0FFC` | 0 | 0 |
| Retail-price field `0F70` | 400,000 | 400,000 |
| Growth `10A4` / boost `10C0` | 240 / 10 | 240 / 10 |

These values follow argument positions through the setter, not the visual order of pushed literals. In particular, member `1050` is market, not launch countdown: launch countdown is `106C`. Cartridge/CD growth scales are 50/36. Earlier speculative market interpretations must not be used.

Cartridge production bands have thresholds 64/128/256/512 KB and unit costs 600/1,000/1,500/2,000. CD bands have thresholds 20/40/60/80 MB and costs 700/1,000/1,300/1,500. The fourth band is the existing upper catchall convention. Ordinary software planning/shipment uses the new licensed record.

The launch appends at the current hardware count but separately writes license index 11. That assumes the original initial eleven records; nonstandard imported counts must not silently be treated as equivalent. The weekly hardware-market loop iterates the current record count, so it includes the appended record and applies the already recovered M35 growth formula to its market/scale/growth/boost. Availability is immediate; own launch does not assign a future launch countdown or retirement schedule. Record, market, license, ownership, design/media and room arrays enter the existing original save/load structure.

## Casino: verified common lifecycle and scope correction

Entry `00994780` offers poker, blackjack and a card-chain game, plus horse racing. The card-chain game is not dice: its help and card-state comparisons describe matching suit/rank. Horse racing is a separate cash-related form and must not disappear from the reconstruction inventory merely because it is not one of the three coin games.

The three coin-game entries require coins > 0. Confirmed purchase adds 50 coins and debits 10,000,000 through the financial helper without a separate affordability guard. Coin member `0110` is explicitly saved and loaded. Betting handlers subtract one coin per bet increment. Poker and blackjack payout timers add one coin per settlement tick; card-chain payout can add a batch while decrementing the pending payout by the same amount.

Recovered payout multipliers, in source table index order:

| Game | Table |
| --- | --- |
| Poker `009960C4 +01E2..025F` | 1000, 500, 100, 20, 15, 10, 3, 2, 1 |
| Blackjack `009951FC +018E..01EF` | 1, 2, 4, 6, 20, 25, 100 |
| Card chain `0099B03C +01E2..02EB` | 1, 2, 3, 4, 6, 8, 10, 13, 17, 20, 25, 30, 40, 50, 55, 60, 70, 80, 100 |

Poker payout uses `bet * table[9 - outcome]`; its double-up path doubles pending payout. Do not map every table index to a named hand without verifying the actual outcome branches.

All three unload handlers call UI resume and helper `00978364`, which sets member `01CC` to 1. They contain no automatic coin refund, conversion to cash, payout drain or complete state reset. Form-load paths rebuild their shared payout table and reset specific game-state cells; they do not establish a universal reset of every bet/pending-payout cell. Exact interrupted-round close/reopen behavior follows those individual state machines.

## Original saves and RNG

The PE imports `rtcRandomNext` and `rtcRandomize` from MSVBVM60 by ordinals 593/594. There are 310 decoded references to the random-next thunk and one to Randomize, at main-form startup `0099CA4C +00C4` with an omitted argument. New-company reset, original load and save introduce no further Randomize call.

The game uses the runtime's shared stream; cosmetic and gameplay draws are not independent feature generators. Microsoft documents the omitted Randomize argument as timer based: [Randomize](https://learn.microsoft.com/en-us/office/vba/language/reference/user-interface-help/randomize-statement), [Rnd](https://learn.microsoft.com/en-us/office/vba/language/reference/user-interface-help/rnd-function).

The original serialization has no recovered RNG-state field or runtime-state export. A historical seed/sequence cannot be reconstructed from an original save alone. The exact generator implementation belongs to the external runtime DLL, not solely to the supplied game executable. Reconstructing that runtime and matching all draw ordering is a distinct fidelity task; it must not be called missing game code. Genuine-save format compatibility still needs a parser and local fixture validation.

## What remains unverified

The office/external/polishing/student-growth existence checks above are complete within the documented static coverage. They are unestablished features, not an open request to invent mechanics. Bankruptcy caller/UI changes, lab closure/persistence, payroll writers, technology consumers and the hardware lifecycle now have concrete answers.

At the M76 checkpoint, remaining research included casino outcome/deck rules, interrupted-round transitions and horse settlement. [M77](casino-investigation-m77.md) subsequently recovers their ordinary static paths and identifies original defects; native input/control/timer comparison and the detailed horse recurrence remain bounded fidelity work. Full original save parsing with genuine fixtures, exact external-runtime RNG implementation/draw parity, and native modal/reentrancy reproduction also remain. No fresh Windows playthrough, .NET/Godot validation or desktop package is claimed. The existing runtime and release acceptance gates remain separate from static discovery.
