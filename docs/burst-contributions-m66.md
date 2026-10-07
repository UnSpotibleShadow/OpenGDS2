# Numbered development bursts — M66 / issue #61

The reported missing gain was not reproduced. A numbered orb is the accumulated
work counter, not an immediate increment to the development percentage. The
existing simulation commits its contribution when the burst finishes. No gameplay
formula or animation deadline was changed for this investigation.

## Original evidence

Static verification uses the supported executable SHA-256
`4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.
`tools/verify_burst_contribution_evidence.py` checks bounded structural anchors;
original binaries, assets and procedure traces remain local. Original Windows
runtime equivalence is not claimed.

| Module2 `00A0283C` source | Fact |
| --- | --- |
| `+9742..979E` | Number atlas reads the actual accumulated counter, pool member 7B |
| `+97D2` | Accumulation clock increments after the number is drawn |
| `+9AE9..9B02` | Commit is gated by 60 × original timing factor 2 = 120 accumulation ticks |
| `+9B17..9BC8` | Counter is copied to the local delta for its selected category |
| `+9C67..9CE0` | Four quality fields receive their full respective delta |
| `+9DCF..9EAD` | Burst progress receives quality/bug delta divided by 3, speed delta in full |
| `+9EC4` | Bug delta increases the bug field |
| `+9C08` | Completed work counter resets, preventing replay of the same contribution |

The animation starts accumulating at work tick 32 and ends after 120 eligible
accumulation ticks, about 4.8 seconds at the 40 ms office tick. At normal speed,
this is about six seconds from work start. Modal windows and pause stop eligible
ticks; save/reload retains the counters. The drawn value precedes the current
accumulation increment by one pass, matching the original order. The counter
stops growing after 100 accumulation ticks and caps at 50.

For example, a Fun orb showing **37** adds **37 Fun** and **12⅓ development
points** when it finishes, not 37 percentage points. Progress is points divided
by required points, and type/genre choices can increase that denominator. A bug
orb adds bugs; a Speed orb adds only development points. Quality gains remain
full even when development progress clips at its required total.

## Regression coverage

- Deterministic numbered bursts reach 50 in each of all six ordinary work
  categories. Before completion no gain is committed; the exact atlas cells for
  50 and final quality/bug/progress values are checked.
- Save/reload immediately before and after completion neither drops nor repeats
  the contribution.
- The real GameSession office tick path survives a modal pause and mid-animation
  reload; checkpoint crossings retain quality and progress clips independently.
- The core-loop UI injects a synthetic 37 Fun burst before commit. Paused reload
  retains it; resuming updates the live report to +37 Fun and the saved project
  to +12⅓ points. A second reload displays the committed gain.
- 305 domain checks passed: 300 synthetic checks plus five supported-original
  import/parity/development/release/sales checks. Original burst evidence and the
  full native exported-host core-loop UI checks also passed.

The issue contains no affected save or per-category before/after values. These
checks verify the currently implemented pipeline; they do not prove every
unreported campaign state. If a particular case still loses a completed burst,
record its category, before/after quality and points, and the preceding save.
No desktop package or GitHub Actions run was produced.


## M67 follow-up: another employee finishes development

The earlier investigation did not cover immediate shipment while a different
employee has an unfinished burst. M67 confirms that phase changes preserve it,
but shipment excludes its uncommitted gain, supported by original snapshot and
empty-room cleanup evidence. See [release boundary](release-burst-boundary-m67.md).
