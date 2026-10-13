# M100: source-backed presentation completion

This batch implements the ten gap groups found by the M98 window census, using
M93–M99 handler evidence. It also replaces office missed-tick replay and supplies
a saved shared VB-family random stream. **Issue #95 stays open.** “Implemented”
here means the established workflow is connected, not that every native window
has passed a pixel comparison on Windows.

| Gap | Result and continuity |
| --- | --- |
| Advanced programming | Qualified actor passes consume the eager trigger draw and retained counter. The five ten-slot inspiration reels, start/stop, 5/6 deceleration steps, three-callback slow cadence and 13-callback hold produce modes 6/10/17. Saved outcomes/comments cannot reroll. Success/super adds 40/100 to the selected quality and 30 experience; failure adds 60 bugs and 6 KB. A saved actor cursor resumes the interrupted pass after the prefix, without repeating stress, counters, earlier employees or training. |
| Hall secretary sequence | Reviewer acknowledgment queues an assessment; completing it commits the existing Hall decision once, opens the two-frame banner, then the sequel message. Each boundary is saved. Existing historical Hall records do not replay. |
| Sales achievements | Each 100k/250k/500k/1m milestone has its separate locally imported illustration, then a saved secretary page, followed by the existing one-time milestone/ranking acknowledgment. |
| Media and capacity | Development, first shipment and reproduction expose the hardware's four native media bands, package cost and actual unit cost. Returning preserves edited shipment title/price/quantity or reproduction quantity. Viewing information does not charge funds. |
| Additional production | Separate hardware picture and media/package/unit/total/funds breakdown, preserving production-budget and recall checks. Held +/- controls follow the native quantity/price acceleration thresholds. |
| Hardware launch | Separate device picture, native name/bit/license fee/description card followed by a saved license-advice acknowledgment; the calendar transaction advances once. Missing fixture fields remain unknown. |
| Company activities | Action-specific confirmation precedes the existing PR/research/production transaction. Saved before/after/cost receipts survive reload; newly unlocked genres have a separate result page. No second fee or RNG draw on receipt replay. |
| Help | Five topic selector and scrollable text; all 91 native body/heading captions are extracted into the local cache, mapped through their Picture1 parents. Japanese uses those local captions; English and missing-cache fallback use authored summaries. The title illustration remains separate. |
| Save/load previews | Numbered modern JSON slots, company/president/date/funds/staff and office preview, independent quick save/load, damaged-save state and per-slot previous-save recovery. This does not import GD2. |
| Charts and records | Live top-100 sales rows retain selection and scrolling, with stable tie order; six reach-time records have category/company/game-date/record-date columns. |

The local import now inventories **422 bounded serialized resources**: 400 BMPs
and 22 GIFs. Seventeen RLE8 BMPs are decoded locally to PNG; other BMP/GIF files
retain their encoding. `Graphic/Recovered/index.json` records offset, dimensions,
format and filename. Reimport the original archive to populate this inventory,
help text and new scene artwork. Archive portraits remain a separate family.
`tools/local_artwork_reference.py` groups the recovered resources for private
inspection. Neither imported resources nor original prose are committed.

## Evidence and policy boundaries

- Programming: `docs/programming-challenge-m99.md`; verifier checks nine procedure
  fingerprints, 36 anchors and fifty reel writes. Failure is native mode **6**.
- Other dialogs/resources: `docs/window-census-m98.md`, M93 handler audit and
  M94–M97 implementation documents. Census verifier checks all 84 forms, 920
  controls, 572 events, 32 selected procedure fingerprints and seven resources.
- Help: supported executable hash guard; bounded fGMHelp serialized range
  `33E530..3618C8`; Label3/4 captions, optional ForeColor and layout properties;
  native parent order `[2,3,1,4,5]` yields the five topics. Optional-original checks
  establish 91 captions and the 422-resource inventory.
- RNG: default gameplay callers now share the saved 24-bit VB recurrence.
  An independent `Microsoft.VisualBasic.VBMath.Rnd` comparison checks its draw
  sequence. Initial seed remains a modern policy. Original `Randomize`, cosmetic
  draws and every native draw ordering have **not** been matched; the saved
  stream is a foundation, not a claim of whole-session RNG equivalence.
- Pacing (#94): original `00A0283C +DCC7..DD18` waits for the remaining portion
  of 40 ms, pumps events, then performs the next iteration. It does not replay
  missed iterations. The port now performs at most one office pass per frame,
  discarding whole missed ticks after stalls. Normal sub-tick frame rounding is
  retained to preserve the nominal 25 ticks/second. A scheduling check verifies
  250 ticks over ten seconds at 60 Hz and no replay after a one-second stall.
  This fixes the proven catch-up difference without inventing a slower game
  speed. Actual original/port Windows wall-clock pacing still needs measurement.
- Fonts prefer installed MS UI Gothic/MS Gothic/CJK fallback at the compact
  control size. No Windows fonts are redistributed. Exact glyphs, DPI, control
  focus, stacking, retained frame lifetime and timer/event delivery need Windows
  comparison; this Linux workspace has no original Windows runtime.

## Native GD2 import: precise remaining boundary

Save `Module1 009D9824` and load `009D60E0` use sequential VB `Write`/`Input`
records, not a binary memory image. Their two fingerprints, **130 paired static
record templates with matching arities**, and **40 ForVar loops each** are now
recorded in `tools/native_save_evidence.json`; run
`tools/verify_native_save_evidence.py` against the supported local executable.
These are templates inside loops, not a claim of 130 physical file lines.

The first record is the native title/version header. The following scalar
member groups are `0000/0010`, `0020..0060`, `0070..00B0`, `00D0..0110`,
`0120..0140`, `0150/0160`; then the `0174` array's 0..10 loop and scalar `018C`.
The rest includes further arrays and variable-length loops. Full field-to-domain
mapping, native Variant/Empty conversion and dirty product-slot behavior remain
unverified. No genuine `.GD2` fixture is present in this workspace. Parsing some
prefix fields is insufficient to restore a trustworthy game. Therefore the
modern picker does not offer an incomplete original-save import operation.

Next work is the complete serialization/mapping and local fixture comparison,
not re-establishing the Write/Input format or repeating these fingerprints.
Native host timing/pixel acceptance and exhaustive image-to-handler review also
remain open. All 84 forms are accounted for in the census; the read-only
`fGMZishaNet` data still does not establish multiplayer functionality.

## Validation

Validation is batched after implementation: 514 optional-original domain checks,
clean app build, bilingual window/transaction/save-reload/live-chart/cadence
smoke, complete core-loop UI, four evidence verifiers and eleven Python checks.
The integrated office check observes 49–51 office passes across two seconds at
60 Hz, preventing an accidental second elapsed-time subtraction during UI
integration. Final import/publication records are in AGENTS.md and #55/#95.
Public fixtures are authored; original-only checks use private local inputs.
