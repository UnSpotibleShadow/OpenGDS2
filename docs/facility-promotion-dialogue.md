# Facility and six-media promotion conversations

Follow-up to #118 and [the facility chain audit](facility-conversation-chains.md).
The owner authorized implementing the missing chains and checking every promotion
medium. The accepted Lawyer negotiation remains unchanged.

## Facilities

Direct service introductions, questions, results and refusals use their recovered
local text in Japanese and authored English translations. Older dialogue caches
use authored Japanese fallbacks. Schema 18 imports 374 entries, including the
five Hot Spring responses and all six promotion activity/result pairs.

- Hot Spring retains its illustrated facility through the secretary result and
  employee responses. Source records are scanned in ascending order; every record
  consumes the eager selection draw, even non-employees. With up to four employed
  staff each speaks; larger staff speak when the draw selects them, without a
  four-speaker cap. Every selected person gets a separate five-variant draw and
  their own name/portrait. Full known source history is required for this new
  presentation path. Cash and stress resolve once; selected responses and the
  unread page are saved. Loading never rerolls them.
- Recycling introduces stock quantity and the ¥500 unit price, then reports total
  proceeds. Production reports the saved old and new capacity. The latter still
  has no invented affordability restriction.
- Technology School keeps the illustrated school and its employee selector. Study
  applies directly from that selector; there is no extra confirmation. Funds
  refusal returns to the same selector and dragged position. A committed result
  restores its owner and selector after load.
- Mystery research has its own already-running refusal. Its delayed two-page
  outcome and laboratory illustration remain intact. Hardware has secretary
  refusals for existing/developing consoles, insufficient funds and an occupied
  room, with the appropriate parent/design window retained.
- Casino retains its illustrated facility behind the selector and no-coins
  conversation. Cancel returns to the parent; starting a game closes the facility
  and selector, as does accepting a coin purchase. Games retain their existing
  lifecycle. Construction funds/duplicate refusals also retain their picker.
- Demolition Cancel preserves its parent; declining a direct Hot Spring,
  Recycling, Production or Mystery service closes that facility. School's annual
  graduate flow and Lawyer's consultation are preserved.

The full owner window remains behind direct conversations; a duplicate standalone
picture is no longer substituted for it. Child/owner buttons are disabled while
covered and restored to their previous states when the child returns. Source
artwork aliases retain generic recovered-resource fallbacks; missing pictures
show a reimport hint. Service fees use an appropriate fee label.

New receipt fields are additive: source plot, old capacity, response snapshots
and page position. Old unread single-page receipts remain single-page; historical
random choices and old capacity are not invented. Save validation rejects bad
pages, unordered/duplicate speakers, inconsistent facility plots and altered
capacity snapshots. Modern service commits remain atomic before their saved
result is displayed; exact native instruction-level mutation timing is not claimed.

## All six promotion media

Magazine, Radio, Television, Cinema, Internet and Newspaper each use their
medium-specific activity caption and completion message, with the promoted title
and actual awareness adjustment. All six restored scene bindings are exercised;
Radio/Television retain their original 256×152 images and the others 256×160.
The existing English television pixel patch remains active. Native 32-bit BI_RGB
scene bitmaps are loaded as opaque RGB, matching Win32's reserved-byte semantics;
otherwise Godot makes part of the Cinema screen disappear. Explicit artwork
overrides and generated English patches keep their intended transparency.

The illustrated promotion stage advances with the recovered 100 ms timer
cadence, then the secretary reports the result. Previously the port used a
25 ms delay and one generic completion sentence. Existing fees, outcome draws,
use counts, cap behavior and one-time saved payment remain unchanged.

## Verification and review

562 optional-original domain checks pass. Interactive resize regressions are
checked with X11 mouse input, because headless mouse warping does not exercise
that behavior.

Verification receipts and private English/Japanese captures are under
`/workspace/scratch/facility-promotion-fixes/`. Source verifiers cover the facility
handlers/artwork, lawyer behavior and #118 scene/periodic evidence. Clean app build (zero warnings/errors), full
archive import and bilingual rendered/UI checks pass, including all six media,
nine facility closure paths, service/refusal/cancel branches, receipts and reloads.
Hardware/casino, interactive resize, #118 and the core-loop regressions also pass.
No original text, pixels, binaries, saves or full traces are committed.

Native Windows focus, timing and exact pixel acceptance remain separate review
work. This batch does not retry the previously crashing Wine runner or claim
complete original-game parity. Keep #118 and #95 open for owner review.
