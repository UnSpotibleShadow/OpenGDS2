# Mystery laboratory — M82

M82 implements facility type 6 using the supplied original executable, SHA-256
`4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.
[M75](static-recovery-2026-10-10.md) and [M76](workflow-verification-m76.md)
contain the recovered rules; no original executable, assets, Japanese dialogue
or full trace is committed.

## Behavior and evidence

| Behavior | Original evidence | Implementation |
| --- | --- | --- |
| Construction | Type-6 initializer/construction table | Existing ¥300 million facility purchase; service is now enabled. |
| Start | `00987B40`; fee initializer `009F651C +0188` | Confirmation, direct ¥25 million debit, progress 1, one `floor(Rnd × 7)` outcome. No affordability gate; negative funds are allowed. An active experiment cannot restart. |
| Progress | `009E3830 +07BF..0940` | Add binary64 0.2 per active simulation iteration while the lab is owned. Starting at 1 crosses the strict `>100` boundary after 495 additions. No weekly or fixed wall-clock duration is substituted. |
| Closure | `0097B6E0`, `00977B3C`, demolition `00984354 +00A6..00B3`, rebuild `00988F78` | Leaving the window retains work. Demolition clears the slot without refund or experiment reset; rebuilding resumes it. |
| Completion | `009EFA40` mode 15, `+5ABD..60A9` | Apply the selected effect before the first result page. Completion draws no new outcome. The final acknowledgement resets progress, retaining the last selected outcome. |
| Original persistence | Save `009D9824 +03BB..03CD`, load `009D60E0 +033B..034D` | Modern JSON saves progress/outcome and a two-page result cursor. Reloading never rerolls, repeats the charge or reapplies an effect. Original-save import remains unsupported. |

| Outcome | Effect at completion |
| --- | --- |
| 0 | Cash becomes `floor(current cash × 1.5)`. |
| 1 | Cash becomes `floor(current cash / 2)`. |
| 2 | Production capacity doubles. |
| 3 | Production capacity becomes `floor(current capacity × 0.8)`. |
| 4 | Company fame becomes 80 when below 80, otherwise 99, including when previously 100. |
| 5 | All currently employed gate-1 source staff stress becomes 0. |
| 6 | All currently employed gate-1 source staff stress becomes 90. |

Cash effects and the start debit do not change weekly, monthly or yearly
accounting snapshots. Staff effects update both the active modern employee and
its source projection; dismissed staff are excluded and newly hired staff are
included at completion. Company fame is the mapped `A1109C` recognition cell;
production capacity maps to `A1108C`.

The latent stock-liquidation handler for result 7 exists in the executable but
normal start cannot select it. It is neither exposed nor accepted as a modern
saved outcome.

## Modern integration and boundaries

The session runs the lab consumer after weekly/calendar work and before staff
work. Pending decisions suspend it; a result receipt also blocks recruitment,
advice, replacement, project cancellation, facility actions and active
simulation. Modern resumable pages replace the original blocking modal. Staff
work continues on a later active tick after dismissal; this is not a claim that
every native event-queue detail is reproduced.

Starting requires known cash, capacity, fame and a complete employed-source
projection, preventing a charged experiment with fabricated effect inputs.
These are modern incomplete-data guards, not extra original eligibility rules.
Arithmetic overflow is checked before mutations. The supported ordinary numeric
cases, including negative odd cash floor, are tested; unrestricted VB6/OLE
coercion and overflow parity are not claimed.

The single start draw uses the current session RNG. The recovered VB6 shared-RNG
recurrence and all unrelated draw ordering are not yet a complete session-wide
replacement. Cadence is expressed in active ticks; native Windows timing and
rendering remain community acceptance work.

Older modern saves without the new field load an idle lab because those builds
had no experiment workflow. Missing required nested fields, invalid/nonfinite
progress, outcome 7 and inconsistent result receipts are rejected.

Dialogue cache schema 10 imports nine lab keys locally from pool `005FAFCC`:
intro index `011C`, outcomes `011E/011F/0120/0122/0124/0128/0129` and the high-fame
variant `0125`. English and Japanese fallback strings are authored translations;
original Japanese text stays in the user's import cache. Older caches still load.

## Validation

- 344 synthetic domain tests pass, including nine new lab tests; 349 tests pass
  with the optional private original fixture. All nine original lab dialogue
  keys are nonempty and the schema-10 cache contains 173 entries.
- Application build: zero warnings and errors using the standard .NET 8 SDK.
- M75 source anchors and M76's 44 procedure fingerprints / 35 casino-table
  entries pass against the supplied original.
- Godot 4.4.1 .NET headless lab UI checks pass in English and Japanese: declining
  confirmation, negative-funds start, repeat-start blocking, saved outcome,
  real active-tick completion, all seven result receipts, saved page cursors,
  reload protection, blocked facility navigation and final reset.

The restricted-host UI check uses disposable data/config/cache and a small GC
heap with a drain before shutdown. The first attempt's shutdown error is not a
pass. This workspace snapshot lacks bundled community audio; missing-resource
messages are not evidence of an audio regression or an audio acceptance pass.
No new Windows package is built by this change. Historical RC.4 does not contain
M82; release notes must identify the source of the actual final artifact.
