# October 15 review: Help, issue 112 and technology effects

The English Help now translates all 91 recovered blocks across the five native
pages (16/34/20/2/19), rather than substituting summaries. The original Japanese
import remains the Japanese text source. The About page retains the imported
illustration and historical website address. Separate OpenGDS2 notes identify
implementation differences, including missing poaching, genre research, modern
training/navigation and JSON saves. The historical site is a reference, not a
verified live service. This is complete text coverage, not a claim of identical
native Help navigation or whole-game fidelity.

## Issue 112

- Products now show newest releases first. Native `fGMZisha 009A369C`
  +0075..00C9 enumerates the release array from count minus one to zero, step -1,
  copying eligible entries into the display mapping. The port now uses reverse
  insertion order while preserving product-ID selection and activation.
- Products have bounded columns and no horizontal scrollbar. Products and
  Employees expose a bottom edge for vertical resizing. Resize interaction pauses
  office time like dragging, ends on mouse release/focus loss and respects the
  workspace. Existing minimize/maximize behavior is retained.
- The employee list's blanket dialog guard differed from office clicks. It now
  uses the office's rule: live reports permit opening details; modal/pending
  scenes continue to block it. The existing detail-opening path still checks
  pending calendar events and membership.
- Casino selection follows the six-command `fGMTenantCasinoSn` arrangement:
  horse racing, poker, blackjack, card change, coins, cancel. English requires
  wider commands; the coin counter/status remain port additions. Coin purchase
  uses a compact message box. Transactions and game services are unchanged.
- Company Activities uses compact metrics, three commands and a scrollable genre
  list. This overview is a port utility; no original standalone overview has been
  established. Activity confirmations use `fGMSeisanZ`'s illustration/message/cost/
  confirmation structure, with an English-width adaptation. Its load handler
  `00994414 +0286..02BB` reads `MDIForm1.ImageFormG[25]` (getter 032C) into the
  picture; that exact 256x160 bitmap is recovered locally at 0x16585. Existing
  imports need reimporting to acquire it. Japanese lettering inside this newly
  recovered illustration is retained; it is not an English artwork patch.
- Remaining generic desktop headings use compact typography and wrap inside
  their content width. Large duplicate headings and unwrapped labels no longer
  determine a fallback dialog's minimum width.

The four ticket attachment URLs returned 404 through both authenticated GitHub
reads and the private-image downloader. Their URLs and dimensions were retained
privately; their pixels were not inspected. Layout decisions above instead use
recovered original forms, handlers and fresh rendered checks. This does not assert
pixel equality to the unavailable ticket attachments.

## Technology audit

Activation follows `Main.Training` -> `CheckUse/TryUse` -> saved office application
-> actor loop in `GameSession` / `DevelopmentSimulation` -> `TickWork` -> effect.
Learning alone is not a passive quality bonus. Training completion increases the
learned level and clears training; it does not reassign the employee to a desk.
Learned levels cap at 3; company ownership gates training, not later use.

| ID | Technology | Result |
| --- | --- | --- |
| 0 | Data compression | Floors a percentage reduction of current data size |
| 1 | Graphics | Floors a percentage increase of current graphics |
| 2 | Debugging | Floors a percentage reduction of current bugs |
| 3 | Sound | Floors a percentage increase of current music |
| 4 | Speed | 50/100/150 actor passes; accelerates ordinary starts under the native retained-eligibility rule, not debugging |
| 5 | Stress relief | Immediate floored 50%/80% reduction; level 3 clears actor slot 0's stress, charging the selected employee |
| 6 | Fun | Floors a percentage increase of current fun |
| 7 | Originality | Floors a percentage increase of current originality |

The six work effects consume imported units at the development desk. Their first
use bases are 20/30/40%; second-use bases are 5/10/20%; later bases are zero, plus
the recovered small random range. Counters are per project and effect. Therefore
zero-quality values stay zero, small changes can disappear under flooring, and
later uses may do nothing. These are source-backed outcomes, not disconnected
technology paths. Hardware graphics/sound/fun/originality applications queue the
required saved confirmation steps instead of ordinary software quality gains.

Original imported use costs by learned level are 100/200/300 XP for IDs 0–3,
80/150/220 for speed, 50/100/150 for stress relief, and 150/300/450 for IDs 6–7.
Work-unit tables for the six delayed effects are 20/35/50. Speed and stress do
not consume their generic work-unit tables. Training has separate costs/work.

Leaving the development desk pauses a delayed application; clearing its room
cancels it. Replacing an application spends the new cost and replaces its saved
selection. Speed's timer also runs while travelling/resting, but pauses when the
office simulation is paused. Its native retained-frame and in-flight replacement
quirks are intentionally preserved; see `speed-technology-m91.md`. Level-3 stress
relief's different recipient is also intentional; see `stress-relief-m90.md`.
No audited effect defect required changing the source-backed gameplay services.
The Use screen now explains each actual effect, delay, rounding, diminishing
returns and hardware distinction in English/Japanese. Existing source warnings
and actual recipient names remain.

Validation is recorded in the review checkpoint outside the repository. New
regressions cover all six delayed effects at every level through real actor ticks
and halfway reload, original imported costs/work and all eight effects, per-page
Help coverage, bilingual layouts, real resize input, newest-first ordering and
employee activation with live versus pending dialogs. Native Windows execution
and whole-session original RNG parity remain outside these checks.

Completed checks: clean application build (zero warnings/errors), 531 domain
checks with the optional original fixture, and seven Godot regressions: bilingual
review112, core loop, casino layouts, management layouts, stress relief, speed
technology and presentation completion. All passed. Original artwork stays private.
