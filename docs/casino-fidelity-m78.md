# Casino fidelity closure — M78

Reviewed 2026-10-11. This extends [M77](casino-investigation-m77.md) without changing game behavior or expanding the supported Early Access scope. The original executable is identified by SHA-256 `4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`. Original files, full traces, dependencies and runtime binaries stay private. The LZH import remains required.

Follow-up: [M79 remaining-runtime analysis](casino-runtime-boundaries-m79.md) resolves coordinate rounding, bounds ordinary odds midpoints, traces native keyboard preview and supplies exact drawing parameters. It also qualifies the timer guarantee below: KillTimer does not remove already-posted messages.

## Result and remaining boundary

The remaining **source-level** race recurrence, RNG recurrence/seeding, serialized timer/control defaults and same-timer reentrancy questions now have evidence. M77's ordinary casino rules and known defects still apply. There is no new evidence for slots, dice, redemption, split, insurance or an ordinary horse-management workflow.

This is not native Windows acceptance. A reference VB6 runtime's actual RNG instructions were executed in an x86 emulator, with only project-context lookup stubbed. The game was not executed in that emulator. Attempts to run isolated 32-bit runtime/type-library/numeric probes failed in this Linux workspace: legacy Wine required a 32-bit ELF loader the host could not execute; userspace emulation could not start its child Wine loader; official Wine 11 WoW64 reached initialization after optional Mono/HTML installers were disabled, but all three probes terminated with segmentation faults (exit 139). Earlier bounded setup attempts timed out and were not counted as passes. This is a concrete execution blocker, not evidence that a workflow is absent.

The remaining acceptance work requires a working Windows/VB6 environment (or a compatibility runner that can execute these probes): mouse/focus/keyboard event delivery, modal interaction between different timers, actual wall-clock timing, `Format("###.0", value)` midpoint/locale output and numeric-string multiplication, screenshots of race camera/sprites. The available original archive does not pin a VB6 runtime DLL version. Do not claim that this separately obtained reference DLL is the user's exact 2001 installed runtime.

## Native runtime identity and reproducibility

The separately inspected DLL has SHA-256 `e86cbb17c8fcd97bb04d9e0b7fb3ef7a33c6896e681705fc95405f14008737cd` (1998 PE timestamp). Symbols were corrected through the DBG OMAP-from-source table; uncorrected COFF addresses do not identify actual DLL instructions. Export addresses and instruction bodies independently confirm the mapping. Public references:

- [Reference runtime and symbols](https://sandsprite.com/vb-reversing/files/msvbvm60.zip).
- [Microsoft VBMath implementation](https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.VisualBasic.Core/src/Microsoft/VisualBasic/VBMath.vb), explicitly annotated as equivalent to VB6 routines.
- [Microsoft KB 231847 archive](https://ftp.zx.net.nz/pub/archive/ftp.microsoft.com/MISC/KB/en-us/231/847.HTM), VB random-generator constants and default seed.
- [VB6 form-control serialization description](https://sandsprite.com/blogs/files/Parsing_VB6_Form_Controls.pdf), corroborated against native indexed property tables rather than guessed from byte values.

Run the asset-free source checker locally with a user-supplied original executable:

```sh
python3 tools/verify_casino_fidelity.py /path/to/GameDev2DX.exe
python3 tools/verify_casino_fidelity.py /path/to/GameDev2DX.exe --reference-runtime /path/to/MSVBVM60.DLL
python3 tools/casino_reference_math.py
# Optional isolated instruction probe, requires unicorn==2.1.4:
python3 tools/probe_casino_rng.py /path/to/MSVBVM60.DLL
```

Use `tools/bounded_process.py` as required by AGENTS.md. The checker verifies one full race procedure fingerprint, 86 reviewed movement arithmetic sites, 23 threshold/window literals, 20 complete serialized control-record fingerprints, the card-chain KeyPreview byte and optional runtime fingerprints/property-index pointers. These are source-consistency checks, not an independent decompiler or a gameplay test. The analytic helper uses Python binary64 and is deliberately not a complete VB6 VARIANT interpreter.

## Random stream

`rtRandomNext` at `6606225D` updates a project-local seed:

```
s = (s * 0x43FD43FD + 0xC39EC3) & 0xFFFFFF
result = s / 16777216
```

The native multiply/subtract optimization is equivalent modulo 24 bits. Default seed is `0x50000`; the first result is exactly `0.7055475115776062`, seed `11837123`. All 24-bit fractions are representable as Single. `Rnd(0)` returns the present value without advancing. For negative arguments, interpret the argument's Single bits as unsigned `b`, set `s=(b+(b>>24))&0xFFFFFF`, then advance. `Rnd(-1)` produces seed `3758214`.

`Randomize` is not a full reset. Timer-based `rtRandomizeTimer` (`66056179`) obtains local seconds since midnight plus milliseconds/1000, stores that value as Single, and uses its 32-bit representation. Numeric `rtRandomizeValue` (`660EA89F`) uses the upper 32 bits of the Double argument. Both replace only the middle seed bytes:

```
s = (old_s & 0xFF0000FF) | (((b & 0xFFFF) ^ (b >> 16)) << 8)
```

The next nonzero Rnd masks to 24 bits. Repeating `Randomize(42)` alone does not reproduce a stream after different preceding draws. The emulator verified twelve default advances, zero retention, negative seeding, three numeric Randomize cases, and three timer-bit reseeding cases against the instructions. The timer-bit cases stub GetTimer with a controlled Single; local-time acquisition was statically traced, not executed under Windows.

A historical save does not store the RNG seed or draw history. Its future random outcomes cannot be uniquely reconstructed from the save alone; the missing information is state, not an undiscovered generator formula. A new reproduction can maintain the same generator and draw order given a controlled seed and the same preceding actions.

### Draw ordering that affects casino outcomes

- Poker command `9B9DE4` consumes an otherwise unused Rnd on each invocation. Unique-card attempts consume two draws (suit and rank), including rejected attempts. Initial deal/replacement and the fresh double-up hand retain their source order; reveal does not invent another draw.
- Card-chain command `9C8A8C` likewise consumes an unused entry draw. Initial five cards, one-card replacements, fresh double-up hands, and the keyboard-reachable copied replacement path use the source's uniqueness/retry state. The exhausted-deck retry defect from M77 remains.
- Blackjack predeals eighteen unique cards, two draws per attempted card including rejections. Later normal hits use those predealt cards; they do not draw new randomness. Double-up generates its fresh five-card hand. Do not substitute per-hit shuffling.
- Horse form load `9896AC`, race-card setup `9D46D0`, and Start `99061C` each consume an unused entry draw. Setup consumes a conditional mode-1 draw, rejection sampling for eight distinct horses and eight distinct jockeys, 24 forecast draws, then sixteen cosmetic draws (two per runner). Cosmetic weight text uses `Int(Rnd*2)` for its sign and `Int(Rnd*8)+1` for magnitude. Removing this text generation changes later race randomness.
- Start computes eight performance values in runner order: `Int(Rnd*12)==1` adds 6; otherwise another `Int(Rnd*12)==2` adds 4; otherwise `Int(Rnd*6)-2` applies. That is one to three draws per runner. Base performance is divided by 1.2 before adjustment.
- Race Timer2 consumes one additional draw for its mode bit at the transition to tick 45; its later movement is deterministic from those values. The recurrence below adds no per-frame noise.

These are local draw-order requirements; unrelated gameplay can also advance the shared project stream. Do not assert that a casino-local seed reproduces an uncontrolled complete playthrough.

## Control defaults and input

The VB6 GUI table and form blobs serialize timer properties using native indexed descriptors: Timer Enabled index 2, Interval index 3. `InitTimer` (`66030766`) initializes Enabled true and Interval zero. Serialized overrides establish:

| Form/control | Requested interval | Initially enabled |
| --- | ---: | --- |
| Poker Timer1 | 30 ms | True |
| Blackjack Timer1 | 50 ms | True |
| Card-chain Timer1 | 40 ms | True |
| Horse Timer1 (draft wager) | 100 ms | True |
| Horse Timer2 (race) | 80 ms | False |
| Horse Timer3 (results) | 100 ms | False |

These are requested Win32 timer intervals, not guaranteed wall-clock frame durations. CommandButton Enabled is indexed property 8 in `Push_rgbpprop` (`66021998`). All three card games serialize their initial deal/confirm button disabled; betting buttons retain the default enabled state. A direct procedure call does not automatically enforce that button state.

Form KeyPreview is native indexed property 49 in `Form_rgbpprop` (`6601BC60`), pointing to `Form_propKeyPreview` (`6601BB20`). Card-chain serializes property 49 true at executable file offset `2DDB6B`. Its key handler (`9846DC`) handles character codes 49..53 and lowercase q/w (113/119); it directly calls the card mouse-up or command method. Consequently, the M77 copied poker-style replacement path is not safely dismissible merely because the corresponding button is disabled. Uppercase characters are not converted in this handler. Native focus and event-delivery acceptance is still required.

Label Visible is indexed property 10 in `Label_rgbpprop`, not CommandButton's Enabled index. Blackjack double-up hides the first chosen card. Ordinary mouse reachability therefore differs from invoking its handler twice directly; the missing explicit same-card guard should not be described as proof that a player can click a hidden card again.

## Timer lifecycle and modal behavior

`TimerCtlProc` (`66030622`) handles WM_TIMER (`0x113`). It checks the handler-busy bit, kills the current Win32 timer before dispatch, marks the handler busy, and fires its event. If the object survives, it clears that bit and rearms using the stored interval **after the handler returns**. If the form/control unloads, it skips rearming. `HrCtlTimerSet` (`660305CD`) also checks Enabled, positive interval, valid window and busy/armed flags before SetTimer.

Thus the same busy timer cannot recursively enter its event through a modal message loop. The runtime rearms after return and has no elapsed-time catch-up loop. However, KillTimer does not remove already-posted WM_TIMER messages; a late message can still dispatch on a surviving disabled timer. M79 checks that branch. Rearming is not a guarantee about the next message delivery time. Other independently enabled timers can still have messages processed by that modal loop. Do not simulate all timers with one blanket pause, nor create catch-up bursts from elapsed time. This constrains M77's payout dialog and close/reload findings; it does not change their cash/state assignments.

## Horse recurrence and finish order

Procedure `fKeibaMain-009EB860` is the authoritative race event. Let `t=A13BA0`, phase `A13BB0`, camera/scenery `B=A13BC0`, mode `A13BE0`, performance `p[i]=A13D00[i]`, displayed position `x[i]=A13E44[i]`, finish coordinate `z[i]=A13E98[i]`. Each event advances `t` at its end, after movement. Runner loops use indices 0..7.

Phase 0 starts at `x[i]=40`. Each event applies `x[i] += 7+p[i]/3`. After the increment reaches t=45, one Rnd chooses the mode bit, the code searches for a **unique** maximum x, stores fixed gaps `(x[leader]-x[i])*3.5` and enters phase 1. Its max search counts every position greater than or equal to the candidate, including itself; tied maxima do not select a new leader. Preserve that behavior rather than silently using a stable argmax.

Phase 1 has no uniform advance outside the following inclusive windows. Each listed coefficient c means `x[i] += c*p[i]`; unlisted runners receive zero:

| Entry t | Mode 0 | Mode 1 |
| --- | --- | --- |
| 65..80 | 0:−1/5; 4:−1/4; 7:+1/5 | 0:+1/5; 2:−1/4; 5:+1/5; 7:−1/3 |
| 110..130 | 1:+1/4; 2:+1/3; 7:−1/4 | 0:−1/4; 3:−1/3; 7:+1/2.5 |
| 140..180 | 3:+1/5; 4:+1/5; 6:−1/5 | 1:−1/7; 5:+1/7; 7:−1/7 |

At end t=200, another unique-max search sets front distance F. Tied maxima skip its assignment. The code sets **both** `x[i]` and `z[i]` to `(F-old_x[i])/1.6`, sets B=700 and enters phase 2. Preserve the fractional state; coordinate conversion occurs at drawing calls, not by truncating every race update.

In phase 2, while B>−1010, B decreases by 10 and local camera step d is zero; thereafter B stays fixed and d=10. For entry t=200..230, `x-=p/5` and `z-=10+p/5`. Subsequent blocks use `x=(x-d)+c*p`, `z=(z-10)+c*p`, with these coefficients in runner order:

| Entry t | Mode 0: runners 0..7 | Mode 1: runners 0..7 |
| --- | --- | --- |
| 231..260 | +1/7, -1/9, -1/9, +1/9, -1/8, -1/9, +1/9, +1/10 | -1/8, +1/7, -1/6, -1/8, +1/9, +1/7, -1/12, +1/11 |
| 261..290 | -1/9, +1/7, +1/9, +1/6, -1/9, +1/8, -1/10, -1/9 | +1/10, -1/10, +1/10, -1/9, +1/8, -1/7, +1/10, -1/11 |
| 331..360 | +1/8, -1/11, +1/7, -1/10, +1/7, +1/7, +1/6, +1/9 | -1/9, -1/8, -1/9, -1/8, +1/9, +1/8, +1/9, +1/8 |
| 361+ | +1/8, -1/7, -1/9, +1/6, -1/9, -1/8, +1/6, -1/9 | -1/4, +1/7, -1/6, +1/6, +1/7, -1/8, -1/6, +1/8 |

The separate t=290..330 block uses c=−1/4 for every runner in both modes. **Tick 290 executes both 261..290 and 290..330 blocks**, in that order, including two z decrements of 10 and two camera-step applications. Raw lower-bound literal at +24F5 is 290; the preceding window branches to +24F2, not past it. An `elif` rewrite would change the race. For p=72, mode 0 runner 0 and d=0, tick 289 changes x by −8 and z by −18; tick 290 changes x by −26 and z by −46; tick 291 changes x by −18 and z by −28. The checked helper includes these vectors and mode-1 runners 6/7 in the first finish window (−1/12 and +1/11).

After each runner's movement, if z<=−1920 and its finish flag is zero, mark it finished, append its index to the arrival array and increment the count. Same-event finish ties therefore follow ascending runner index, not interpolated finish time or a sort after all movement. The race timer is disabled and result timer enabled at count eight. Finished runners are not filtered out of the earlier arithmetic loop before that point.

Drawing uses a different coordinate basis by phase. Phase 1 uses a unique leader search and screen x based on `leader_x+15-x[i]`; a tie skips the local leader assignment. Phase 2 uses screen x based on `415+x[i]` and runner-index lanes. Animation frames toggle between zero and one. Ranking display stops once its selected leader's finish coordinate falls below −1320. These rendering branches and native integer conversions should be compared with screenshots, independently of the finish/payout logic. The fixed-gap setup array does not substitute for the subsequent x recurrence.

## Financial numeric operations

The horse payout thunk at original `004010A6` imports VB6 ordinal 660. In the reference DLL this is `rtcVarFromFormatVar` (`660FB246`), which constructs a **BSTR VARIANT**, not a numeric rounded VARIANT. Pair winnings multiply the stake by the one-decimal formatted odds string through VARIANT arithmetic; single-win multiplication follows its separate numeric path. For odds 2.1 and 4.5, pair raw odds are 3.9375; the intended one-place string is 3.9 before multiplication, so a stake of 100 follows a 390 result rather than 393.75. Preserve operation order and the M77 pair-index mismatch.

The exact formatting midpoint policy, locale-dependent separators/string coercion and financial VARIANT subtype at boundary values remain **unexecuted acceptance cases**, not new game-rule inventions. Probe inputs prepared privately include 1.25, 1.35, 2.25, 2.35, −1.25 under English/Japanese thread locales, plus coordinate conversions at ±1.5 and ±2.5. Their crashed Wine runs produced no valid output. Do not use Python `round`, a decimal floor, or a modern engine's formatter as evidence of original results.

## Implementation handoff

M77 plus this report supplies the ordinary source-backed rules and known defects for a casino implementation proposal. First decide which original defects to preserve and label any corrections. A proposal must preserve the shared RNG order, source-indexed dispatch, tick windows including 290, ascending-index finish ties, requested timer intervals and callback rearm semantics. It must explicitly retain the unverified Windows input/coercion/rendering acceptance boundary. Casino is still outside supported EA; this research adds no game implementation, assets, packaging or Actions dispatch.

M79 supersedes the coordinate and locale-test portions of the acceptance note above: native coordinate conversion is verified as nearest, ties to even; all 64 ordinary odds combinations avoid decimal midpoints; VarFormat uses the Windows user-default locale, so changing only the thread locale would not establish that behavior. See the linked M79 report for the remaining external-runtime acceptance cases.
