# M89: Ordinary horse racing with explicit native defects

Implemented 2026-10-12 from [M77 rules](casino-investigation-m77.md), [M78 motion/runtime evidence](casino-fidelity-m78.md), [M79 numeric/input boundaries](casino-runtime-boundaries-m79.md) and [M88 recovered catalog](horse-catalog-m88.md). Original binary hash is unchanged; names, artwork, executable and full traces remain private. All four recovered casino activities now have playable modern workflows.

## Setup, forecasts and random ordering

Horse racing uses company cash, not casino coins. A refreshed local LZH import supplies all 44 runner and 10 jockey records. Older catalog JSON remains readable but race entry explains the required reimport. Active saved races retain their eight selected records, independently of the complete catalog; next-card setup pauses for recovery if that complete catalog is missing.

Selector `00994780` index 0 and Form_Load `009896AC` enter setup `009D46D0`. Form load and setup each consume an unused Rnd draw. Setup selects eight distinct runner indices and eight distinct jockey indices by rejection sampling, consumes 24 forecast draws in row-major order and then sixteen cosmetic weight-change draws in runner order. Forecast rank counts scores at least as high, including self then subtracting one: tied ranks are not normalized. Win odds are `[2.1,4.5,5.7,7.5,8.9,11,28.6,47.7]` by the first forecast rank. Each pair-odds row stores seven second runners excluding the first; column 7 remains zero. Later payout reads raw arrival indices into this compressed matrix, preserving the mismatch.

Start `0099061C` consumes its unused draw and computes strength /1.2 followed by successive independent random tests: first Int(Rnd*12)==1 adds six, otherwise the second test equal to two adds four, otherwise Int(Rnd*6)-2 applies. A runner therefore consumes one to three performance draws. The motion-mode bit consumes one further draw at the transition to callback 45. Remaining motion is deterministic. The session's modern RNG is shared with other gameplay; local draw ordering is preserved, but a .NET RNG is not claimed to reproduce the native VB6 generator or an uncontrolled original playthrough.

## Wagers, caps and original matrix mistakes

Requested draft-wager callbacks are 100 ms. Holding add/remove changes a wager by 100 cash per callback. Add tests temporary cash >0, not >=100; a one-unit balance can therefore become -99. A single stake clamps at 99,900 after addition while still debiting the extra 100. Closing an unstarted draft does not commit it. Start directly assigns the temporary draft balance to company funds; it does not call the accounting helper or modify weekly/monthly/yearly net totals. Closing after Start does not refund committed stakes.

The pair-wager handler `009E6148` contains additional defects confirmed while porting:

- Pair (first 4, second 3) adds/removes wager cell `[4,4]` while its display locals retain `[4,3]`.
- Pair removal guards and add/remove clamps read the compressed selected column rather than the actual second-runner cell. A newly placed stake can consequently be impossible to remove through its own displayed selection.
- An unrelated positive compressed cell can enable removal from an empty actual cell, creating a negative stake while refunding 100 cash. The port preserves that behavior; it does not assert that a repaired atomic wager or nonnegative pair matrix is native fidelity.
- Pair caps can clamp an adjacent cell instead of the updated stake, allowing the latter to exceed the displayed single-wager cap.

These are source-backed rules of this port, not unnoticed regressions. Numeric underflow/overflow is refused before mutation. The interface discloses the original wager and settlement defects; names and cash remain ordinary local game state.

## Motion, finish and settlement

Race callbacks request 80 ms. Phase 0 adds 7 + performance/3 to each initial position of 40. At callback 45, motion mode is selected and a unique-maximum search updates the leader; a tie retains its earlier assignment. Phase 1 uses the recovered inclusive 65–80, 110–130 and 140–180 signed-division windows from M78. At callback 200, a unique maximum updates front distance, then position and finish coordinate become (front - position)/1.6; camera starts at 700.

Phase 2 uses the recovered coefficients and camera step. The 261–290 and 290–330 blocks both run at tick 290, in order, including two finish-coordinate decrements and camera-step applications. Individual stores preserve source arithmetic grouping rather than sorting outcomes or substituting a weighted winner. After a runner's updates, a coordinate <=-1920 registers it once; same-callback crossings follow ascending runner visitation. Finished runners continue moving until all eight arrive. The port stores position/camera state but uses modern text presentation, not native sprite/camera drawing.

Result callbacks request 100 ms. At callback 100, a positive winning single or ordered pair stake opens the payout state; otherwise next-card setup runs. Single winnings multiply stake by win odds. Pair winnings multiply by one-decimal formatted odds read at raw arrival indices. M79 proves all 64 ordinary odds-value combinations avoid midpoint boundaries; the port uses binary64 rounding then modern decimal temporary money, without claiming native OLE midpoint/locale/VARIANT equivalence at artificial large values. A positive stake using the source's zero odds column still opens the original zero-award result.

**Native settlement remains broken:** payout accept adds the displayed award only to temporary cash; after modal return, `009A12F8 +03FC` calls setup, which overwrites it with company funds. The port follows that path. Displayed winnings do not increase company money. The UI explains this before wagering and at the result. No repaired cash credit, redemption or unsupported own-horse-management mode is invented. Race motion's random mode 0/1 is distinct from the unestablished special own-horse entry mode.

## Modern continuity, presentation and verification

Modern saves persist draft ledger, runner snapshots, forecasts, odds, race positions, finish order, callback cursors and pending result. They do not replay charges/draws/awards on load and preserve unrelated payroll scratch. Physical held input is deliberately not resumed. Late native posted-timer messages, exact focus delivery, native rendering/audio and financial VARIANT subtype behavior are not emulated or claimed. Timers deliver one callback per interval without catch-up bursts; the office and other gameplay actions pause while racing. Catalog recovery waits with a close/continue action rather than inventing runner records.

The domain suite passes 454 checks with the optional private original fixture, including 21 new race checks. These verify data/RNG order, all performance branches, forecasts, direct cash commit, unstarted close, caps/overspend/negative pair stakes, odds indexing and bounded decimal vectors, transition/overlap motion, unique maxima, visitation finish order, complete races under both modes, result timing, discarded winnings, save/held-input continuity, recovery and numeric/modal guards. Clean application build and editor import pass.

English/Japanese UI checks cover cash entry without coins, held draft changes, Start, frozen office, save/reload and physical-input release, payroll scratch, result continuation and older-cache guidance. An initial fixture reconstruction changed typed JSON integers and floating values through Godot's generic JSON representation and was rejected; the corrected fixture preserves the raw C# save and edits only its synthetic result scalars. This is a harness correction, not a save-reader coercion workaround. Poker, blackjack and card-chain are regression checked. M77's 52 fingerprints/52 dispatch targets/31 anchors, M78's 86 movements/23 literals/20 controls and M79's runtime/ordinary-odds boundaries are reverified against the private original/reference DLL; none is native Windows acceptance.

The existing RC.4 download is unchanged and predates these source milestones. A final Windows export remains necessary. Actions stay paused, repository visibility remains the owner's decision and native playthrough acceptance remains community-led.
