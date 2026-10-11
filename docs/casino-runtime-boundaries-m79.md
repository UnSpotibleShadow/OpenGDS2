# Remaining casino fidelity analysis — M79

Reviewed 2026-10-11, using the extracted **6,365,184-byte game executable**, not the attached self-extracting installer. [M78](casino-fidelity-m78.md) and [M77](casino-investigation-m77.md) remain the rule evidence. This follow-up resolves more of their runtime questions and corrects an overly strong timer guarantee. It adds no casino implementation.

## What is now established

| Concern | Result | Evidence boundary |
| --- | --- | --- |
| Half-integer coordinates | Nearest, ties to even for the reference runtime's Double-to-Long conversion | Nine actual instruction probes pass under the engine's x87 control word; Windows not executed |
| Ordinary odds midpoints | None of the 64 combinations of the eight source odds is a one-decimal midpoint | Exhaustive rational and binary64 checks; includes repeated odds conservatively |
| Formatting locale | User-default Windows locale, not simply the thread locale or Japanese project locale | Native delegate traced to VarFormat; Microsoft API contract |
| Financial multiplication | Native VB6 wrapper delegates to OLE Automation VarMul | Exact return subtypes/locale parsing need the target OLEAUT32 implementation; no invented integer rounding |
| Same/different timers | Same busy timer does not reenter; another timer can reach event dispatch; normal return rearms | Instruction probes with explicit Windows/event stubs |
| Disabled timer with queued message | A late delivered WM_TIMER can invoke the event on a surviving disabled control | Native entry checks busy, not Enabled; emulated late-message vector plus KillTimer contract |
| Keyboard path | Native WM_CHAR preview path exists before the child-control path | Reference runtime routing plus original KeyPreview and key-handler evidence |
| Race drawing | Source coordinates, tile dimensions and raster operations are recoverable | Original BitBlt call arguments and native direct-call convention; screenshots remain acceptance |
| Runtime identity | Header build 8988, ABI revision 10, Japanese locale; no installed DLL version pinned | Original bytes, archive inventory and actual compatibility-check instructions |

## 1. Numeric conversions and ordinary odds

### Coordinate conversion is no longer an unexamined midpoint rule

P-code `CI4Var` calls `__vbaI4Var` at reference `66002120`, through opcode handler `66105258`. Its Double branch loads the value, executes x87 `FRNDINT`, and converts to a signed 32-bit integer. The engine control word at `66102000` is `0x133F`, with round-to-nearest mode; exact ties select the even integer.

Instruction probes execute the conversion body without a substituted conversion function:

| Input | Result |
| ---: | ---: |
| 1.5 | 2 |
| 2.5 | 2 |
| 3.5 | 4 |
| −1.5 | −2 |
| −2.5 | −2 |
| −3.5 | −4 |
| 1.499999999 | 1 |
| 1.500000001 | 2 |
| 2147483646.5 | 2147483646 |

These establish the reference routine's ordinary Double boundary behavior, not every VARIANT subtype, overflow handler or arbitrary externally changed floating-point environment. The race retains fractional x/z state and converts at its drawing calls; truncating positions on every update would still be wrong. Full race VARIANT arithmetic uses external OLE Automation and is separate from this tested conversion routine.

### Generic Format midpoints do not affect the source odds set

The eight win odds are `2.1, 4.5, 5.7, 7.5, 8.9, 11, 28.6, 47.7`. Every initialized pair odds value is a product of source odds divided by 2.4. Exhaustively analyzing all **64 odds-value combinations**, including repeated odds, finds no value of the form `(n+0.5)/10`. The nearest is **1/120 of an odds unit** away from a midpoint, well beyond the binary64 discrepancy for these inputs. The exact rational and binary64 nearest-rounding results agree throughout this finite set.

Thus generic tests such as Format(1.25, one decimal) are useful external-runtime probes but are not an unknown branch of ordinary horse settlement. This does not repair M77's compressed-display/direct-finish-index matrix mismatch, guarantee that uninitialized/out-of-range entries are valid, or assert historical OLEAUT32 formatting output. It narrows the midpoint concern to arbitrary nonordinary inputs rather than changing the source odds.

### Formatting and multiplication cross an identifiable external boundary

The original format thunk uses VB6 ordinal 660. The reference `rtcVarFromFormatVar` wraps `rtcBstrFromFormatVar` at `6600D152`, which calls the function pointer `gpfn_VarFormat` at `6611072C`. It returns a BSTR VARIANT. The multiplication wrapper at `6610848B` calls `gpfn_VarMul` at `6611002C`; it does not append a financial integer conversion. The game then adds that multiplication result to its temporary funds cache.

[Microsoft VarFormat](https://learn.microsoft.com/en-us/windows/win32/api/oleauto/nf-oleauto-varformat) specifies the **user-default locale**. Therefore, M78's privately prepared English/Japanese `SetThreadLocale` variants would not by themselves establish a correct VarFormat locale test. A valid acceptance matrix must record the Windows user-default locale and custom decimal separator, OLEAUT32 version, input/output VARIANT types and returned string. Project LCID `0x411` does not force VarFormat's numeric separator.

[Microsoft VarMul](https://learn.microsoft.com/en-us/windows/win32/api/oleauto/nf-oleauto-varmul) documents numeric-string multiplication. [Microsoft's arithmetic language specification](https://learn.microsoft.com/en-us/openspecs/microsoft_general_purpose_programming_languages/ms-vbal/e070115f-8d40-40cf-ac6d-ab18b9c6c906) and [Wine's compatibility implementation](https://github.com/wine-mirror/wine/blob/main/dlls/oleaut32/variant.c) support Double arithmetic for a numeric stake times a numeric string. These are contract/compatibility evidence, **not an observed result from the user's historical OLEAUT32**. The archive does not include that DLL. Exact native formatting/parsing edge cases remain external-runtime acceptance, but there is no missing in-game floor/round/payout formula to invent.

## 2. Timers, modal callbacks and close/unload

The instruction probe runs `TimerCtlProc`, `CtlTimerKill` and `HrCtlTimerSet`; it substitutes Win32 SetTimer/KillTimer success, the event dispatcher and generic window handling. The substituted handler can deliver timer messages synchronously. This checks the native procedure's flag/branch ordering, not a Windows modal message pump:

1. Normal event: kill A, dispatch A, rearm A using 80 ms.
2. Nested delivery: kill/dispatch A; another A message goes to generic handling without event dispatch; B kills, dispatches and rearms; A rearms after the outer handler returns.
3. Disabled final flag state: event returns with Enabled false; A is not rearmed.
4. Nonzero invalidation return from event dispatch: A is not rearmed. This models the branch outcome, not execution of a real form destructor.
5. A later WM_TIMER delivered to the surviving disabled A: event dispatch occurs once, with no rearm.

The fifth case is important. `TimerCtlProc` checks its busy bit at entry but does **not** check Enabled before dispatching WM_TIMER. [Microsoft KillTimer](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-killtimer) explicitly leaves already-posted WM_TIMER messages in the queue. Consequently, disabling a timer is not evidence that every future callback is impossible. The original procedure has no generation token that rejects such a stale delivery.

[WM_TIMER](https://learn.microsoft.com/en-us/windows/win32/winmsg/wm-timer) is generated at low priority when higher-priority messages are absent. [SetTimer](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-settimer) uses requested milliseconds and resets an existing timer when replaced. The runtime requests a new period after the handler returns, but this does **not** guarantee that the next delivered message must wait that entire period: an already-posted message is a separate case. There is still no source-backed elapsed-time catch-up loop, so an engine port should not fabricate one callback per missed interval.

This qualifies M78's broad “no backlog/next period” wording. Actual cadence is dependent on input load, OS queue state, modal dispatch and handler duration; exact historical wall-clock timestamps are not recoverable from code alone. Destruction of the target window and disabling a surviving timer are also different cases. M77's close handlers still contain no refund or pending-payout drain, and its horse cash overwrite defect is unaffected.

## 3. Input delivery

The original card-chain form serializes KeyPreview true and handles lowercase q/w plus 1..5 by directly calling methods. The reference runtime now supplies the missing delivery connection: `FormFOleControlKeyMessages` (`6602C3C6`) recognizes WM_CHAR (`0x102`), queries `DeskGetKeyPreviewVal` (`6602CF8B`), and follows the preview branch at `6607AC7D` into `_DefKey` (`6602D19A`) for the form. The getter reads the form preview flag (bit 10) and excludes its special type-0x20 case.

This is a real native input route, not merely an arbitrary direct invocation of a game handler. Once the relevant WM_CHAR reaches the active form's preview path, the game handler does not test the target button's Enabled state. Its phase guards still apply. A disabled button therefore does not establish that the copied W replacement path is dead.

It does not mean every physical W press always becomes lowercase 119: focus, modifiers, keyboard layout, accelerators, interception and modal activation influence message production/delivery. Uppercase W is not converted by the game handler. The source-backed claim is conditional on the delivered character and phase. Blackjack has no corresponding recovered numeric key handler that allows clicking an already hidden first card; the hidden-card/direct-handler distinction from M77 remains. Native mouse/focus/keyboard playthrough remains an acceptance check, not a missing casino rule.

## 4. Race rendering specification

The game's native-call thunk resolves `gdi32!BitBlt`. Reference `ImpAdCallI2` directly invokes the function address using the already-pushed arguments; their reversed emission order matters when decoding width/height and source x/y. The relevant sprite calls use these pixel expressions, where R is the tested nearest-even integer conversion, i is runner index and f is frame 0 or 1:

| Phase | Destination x, y | Tile width × height | Source x, y |
| --- | --- | --- | --- |
| 0 | R(x[i]), 38+12i | 29×21 | 29f, 21i |
| 1 | R(leader_x+15−x[i]), 130 | 61×35 | 116+61f, 35i |
| 2 | R(415+x[i]), 38+12i | 29×21 | 58+29f, 21i |

The code uses paired raster operations **SRCAND `0x008800C6` followed by SRCINVERT `0x00660046` (XOR)** with its separate image/mask sources. That is not an alpha-blending request. Frame advances after drawing and wraps from two to zero. M78's phase transitions, scenery scroll, unique-max/tie behavior and finish ranking gate remain applicable; this table does not substitute fixed-gap coordinates for the race's live x array.

The layout and sprite-selection formulas no longer require a screenshot to discover. Screenshots are still needed to validate actual bitmap alignment, clipping, repainting, colors, font metrics and scaling against the original Windows presentation. No original pixels or reconstructed original assets are committed by this milestone.

## 5. Runtime identity and historical limits

The extracted game has a file-version resource `1.2.0.9`. Its VB6 header records build **8988**, ABI revision **10**, LCID **0x411**, and language DLL name `VB6JP.DLL`. The separately inspected reference runtime has file version **6.0.82.68** and the M78 SHA-256. These are distinct identities.

Do not map build 8988 mechanically to an installed DLL version or service pack. In the reference `ExeFCheckVersion` (`6601AF2B`), compatibility checks use the header word at +0x22 (ABI revision), not the build word at +0x04. Instruction probes accept revision 10 with both build 8988 and a deliberately different 32767. This demonstrates that the build word does not uniquely select an installed runtime through that check; it is not a full guarantee that every part of the game works on this reference DLL.

The supplied LZH has 48 entries and **no DLL/OCX entries**. It cannot identify the historical machine's MSVBVM60, OLEAUT32, USER32, Japanese language resources, fonts or user locale settings. Code can identify requirements and behavior of a chosen reference stack; it cannot reveal which external files a particular player's machine had installed. Similarly, saves lack RNG seed/draw history, so a unique historical future random sequence cannot be reconstructed. These are unavailable environmental/state facts, not features hidden somewhere in the executable.

## Verification and remaining work

```sh
python3 tools/verify_casino_runtime_boundaries.py /path/to/GameDev2DX.exe --reference-runtime /path/to/MSVBVM60.DLL
# Optional isolated instruction probes, requires unicorn==2.1.4:
python3 tools/probe_casino_runtime_boundaries.py /path/to/MSVBVM60.DLL
```

Use the bounded-process wrapper required by AGENTS.md. The new verifier checks four original procedure fingerprints, seven rendering literals, header fields, thirteen optional reference-runtime spans and all 64 odds combinations. The instruction probe checks nine coordinate cases, two ABI-version cases and five timer scenarios. It stubs precisely the Windows/event operations described above and performs no game playthrough. Source anchors are consistency checks on reviewed interpretations, not an independent decompiler.

Remaining acceptance is now narrower: a chosen working Windows/VB6 stack must confirm OLE formatting/parsing/financial subtypes, physical input/focus paths, actual queue/modal/unload sequences and screenshot presentation. The original 2001 installed stack and historical random state are not uniquely recoverable from the supplied files. M78's Wine startup/segmentation-fault failures remain the concrete execution blocker in this workspace; those failed attempts are not evidence about original game behavior. No new unsupported gameplay is authorized or introduced by these findings.
