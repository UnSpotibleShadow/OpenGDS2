# Own hardware — M83

M83 enables the hardware laboratory and connects console development to the
existing software, production and market systems. Evidence comes from the
supported original executable SHA-256
`4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`,
[M76 lifecycle tracing](workflow-verification-m76.md), and the private bounded
traces. Original executables, pictures, dialogue and full traces stay private.

## Recovered behavior

| Behavior | Evidence | Implemented result |
| --- | --- | --- |
| Entry | `009864AC`; type-8 facility table | Existing ¥1 billion construction price. Reject an already launched own console or an active state-2 project. |
| Design quote | `00991BE8` | Each design/medium dimension costs ¥200 million for 0 or ¥100 million for 1: total ¥200/300/400 million. Room defaults to 0 when opening; shared choices persist when leaving. |
| Acceptance | `00990B70` | Check funds before occupied room; equality is allowed. Debit through accounting; initialize state 2, stage 0, work 0/2,500 and each quality 0/800. Copy design/media. |
| Retained flags | Acceptance has no four-flag reset; ordinary company initialization resets them | Start retains the selected room's flags. Active-project flags and the retained room snapshot persist in modern saves. |
| Work selection | `00A0283C +59CE..5BD3` | One 0–399 category roll. Ordered inclusive intervals: fun 0–50, novelty 50–100, graphics 100–150, music 150–200; later rolls select speed if work remains. Full targets idle in their own interval; boundary rolls do not fall through. |
| Contributions | Common worker/finalization paths | Existing work tiers and burst accumulation apply; work/qualities clip to their targets. No software checkpoint, debugging stage or virus phase is invented for hardware. |
| Technologies | Graphics `+759E`, music `+7E78`, fun `+834C`, novelty `+880F` | These four hardware branches notify, then set their flags to 1; they do not apply software percentage buffs or draw percentage RNG. Saved acknowledgements prevent replaying experience charges or work completion. |
| Completion | `00A0283C +AE26..AF8C` | Work and all four qualities must reach target, with all four flags exactly 1. No extra calendar wait. Saved ready state opens the launch form. |
| Closure | Monitor `0097B640`, demolition `00984228` | Closing the monitor or demolishing the lab keeps hardware work alive. No ordinary state-2 cancellation writer was recovered; the monitor offers no cancellation action. |
| Launch | `0099F384`, setter `009874E8`, bands `009845AC` | Append identifier/source 11, grant license/availability immediately, clear the room and queue the own-hardware ranking. No extra launch charge. Only one own console can be launched. |

The constructor preserves the original assumption of eleven initial platforms.
Modern incomplete-data guards reject a missing/nonstandard catalog/calendar,
unknown funds or missing licenses/availability instead of fabricating record
indices. Project labels and console names use the existing modern 1–128-character
constraint; that is not an additional recovered native rule.

## Launched record and downstream use

| Field | Cartridge | CD |
| --- | ---: | ---: |
| Identifier | 11 | 11 |
| Medium/type `0FA8` | 3 | 4 |
| Bits / license fee / availability | 64 / 0 / 1 | 64 / 0 / 1 |
| Initial market `1050` | 0 | 10 |
| Raw `0F38` / growth scale `0F54` | 1 / 50 | 1 / 36 |
| Planning points / cost | 400 / 0 | 600 / 0 |
| Raw `0F70` | 400,000 | 400,000 |
| Growth weeks / boost | 240 / 10 | 240 / 10 |
| Future launch / retirement countdown | 0 / 0 | 0 / 0 |
| Production band thresholds | 64 / 128 / 256 / 512 | 20 / 40 / 60 / 80 |
| Band costs before the existing fixed ¥200 production component | 600 / 1,000 / 1,500 / 2,000 | 700 / 1,000 / 1,300 / 1,500 |

The persisted console is combined with the imported catalog for planning,
licensing, shipment/reprints, sales, advice and weekly calendar processing. The
original import is not edited or shared between companies. Market/growth changes
remain in the calendar projection, separate from the initial definition. The
fourth production band remains the existing upper catchall convention.

The source ownership flag `A11E50` has recovered consumers in menu entry
`009815AC` and laboratory entry `009864AC`, and its launch writer in `0099F384`.
This implementation does not infer a separate console-sales/revenue workflow
from ownership or the raw `0F70` value.

## Design/media pictures

Launch-form load `00994E78 +0164..017C` captures room design/media into
`A11EA0`/`A11E90`. Planning selection `009940B8 +00F7..02A7` reads those globals
and selects the MDI picture array:

| Design | Medium | Imported picture |
| --- | --- | --- |
| 0 | Cartridge | Hardware11.bmp |
| 0 | CD | Hardware12.bmp |
| 1 | Cartridge | Hardware13.bmp |
| 1 | CD | Hardware14.bmp |

All fifteen pictures were already imported locally by M43. The design preview
and software-planning selection now use these four mappings. The picture
verifier checks the global references, predicates, indices and serialized
resource identities. No original image bytes are committed.

## Modern persistence and boundaries

Hardware design/media, required flags, pending technology notification, launch
name draft, launched definition and market/license state survive reload. The
technology flag is set after acknowledgement, matching its position after the
native modal call. The launched room's retained flags remain available; a
second console is still refused. Invalid flags, missing designs/targets,
fabricated platform-11 references, altered production bands and overlapping
modals are rejected. Older supported modern saves without the new own-hardware
field load an unlaunched state. Earlier unsupported state-2 placeholders are not
silently converted into a real hardware project.

Pending hardware notifications/launch suspend simulation and block other
management transactions. Bank borrowing now uses the common pending-decision
guard, also covering mystery-lab results. Launch clears outstanding work cursors
using the existing modern software-shipment safeguard so immediate new planning
cannot inherit an unfinished old contribution; native empty-room cleanup occurs
on its next active iteration.

The model retains the historical `PolishingTargets` JSON member for hardware's
quality targets; this is not evidence of a separate polishing feature. Inert
software bookkeeping cells in a hardware room use modern defaults rather than
claiming a complete native shared-cell snapshot. The current session RNG remains
the existing modern generator; exact VB6 stream, unrelated draw order, numeric
coercion and native modal/timer fidelity are not claimed. Active-tick progress is
supported; full native Windows presentation and timing remain community work.
English/Japanese notification text is authored, not bundled original dialogue.

## Validation

- 357 synthetic domain checks pass; 363 pass with the private original fixture.
  Thirteen new hardware checks include the full own-platform planning, shipment,
  sales and reprint path, plus original-catalog planning after console launch.
- Application build: zero warnings/errors on the standard .NET 8 SDK.
- M76 workflow anchors and the expanded hardware-picture verifier pass against
  the supported original; all four picture mappings are established.
- Godot 4.4.1 .NET hardware UI checks pass in English and Japanese: quote gates,
  shared choices/cancel, one-time charge, project restrictions, demolition and
  reload, saved technology acknowledgement, launch-name draft, both media types,
  no launch fee, platform availability/licensing and downstream planning/picture
  selection. JSON-array membership assertions were corrected to use parsed JSON
  number types; their initial failures were test defects.

The restricted-host check uses disposable data/config/cache and the established
GC drain. The snapshot lacks bundled community audio, so missing-audio messages
are not an audio acceptance result. No new Windows package or Actions run is
produced. RC.4 remains historical; it contains none of M80–M83.
