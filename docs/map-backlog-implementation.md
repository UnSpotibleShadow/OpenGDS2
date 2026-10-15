# Original-map implementation and evidence follow-up

15 October 2026. Baseline: `a5abd1de0336ea67ba87aad200899d5dbf7472c0`.
The original executable remains pinned to the map's SHA-256. No original
executables, archives, captions, pixels, audio, saves or complete traces are bundled.

## Gameplay implemented

The original backup device is a one-time ¥100,000,000 purchase. Its secretary
introduction, confirmation, insufficient-funds/already-owned results and unread
receipt survive modern save/load without charging twice. Ownership starts false
when absent from older modern saves, which had no purchase path. Native member
`02CC` is independently present in reset/save/load, rather than inferred from
the existing modern `.bak` file feature.

The periodic virus branch requires five released products. It eagerly draws
`floor(Rnd × 500000)` for each of the two rooms, including empty rooms, and tests
for 33. Affected software developers react in actor-slot order with seven variants.
Without a device, each acknowledged employee reaction adds 20 stress, capped at
99, and the final secretary acknowledgement removes the project. This does not
run voluntary cancellation's additional work resets. With a device, the project
and stress survive. The native hidden tile 142 becomes visible at (18,109), using
the recovered two-frame counter. Saved company-pass cursors resume at the next
room without repeating weekly work or random draws.

Employee poaching now includes the target selector, confirmation, timed
negotiation, donation and contract selectors, result conversations, hires and
departures. Source eligibility is an unemployed source record with its special
flag set. Donations are ¥5/10/20/50 million and debit all financial accumulators
after the secretary acknowledgement. Contract increments are ¥10/20/50/100 million
and do not charge immediately. The 100ms native timer increments elapsed by 0.5;
rival bids occur at elapsed 20/40/55 and conditionally 80. Both amount selectors
explicitly disable/re-enable that timer in the original.

At elapsed 100, offer plus donations decides employment; ties favour the player.
A successful offensive negotiation charges the employee's **previous** contract
fee through the hire helper, then stores the raised fee. Successful defence
changes the future contract fee without an immediate contract payment. Donations
remain spent on loss or closure. Mutations follow the final secretary result,
and pending payments/results/bids resume from modern saves without replay.

The automatic defensive offer increments a retained counter, eagerly draws
`floor(Rnd × 7000)` every company pass, and can trigger after 30000 ticks on draw 1.
It precedes virus checks and updates the original payroll scratch count. Empty
rosters lead the native code to retained scratch-array data whose behaviour is
unresolved; the adaptation records that boundary and does not invent an employee.
Unknown legacy target data is likewise recorded without a fabricated negotiation.

The recovered two negotiation portraits and 322 dialogue entries are imported
locally. Reimport the supported LZH to refresh older caches. The source-backed
graduate/annual-renewal artwork aliases are corrected to `fGMKeiyakuS` and
`fGMSKoyouKz`, respectively.

## Original menu and display access

In-game Settings now exposes Sound On/Off, Normal/Enlarged office graphics and
Japanese/thousands number grouping. Native Sound controls the four-effect
dispatcher; community music retains its separate setting. Enlarged graphics use
two pixels per native pixel in a bounded scroll viewport, with input mapped back
to office coordinates. Number formatting is shared across the UI and retains
the original zero-filled four-digit lower groups, such as `1億0000万0001`.
Thousands grouping uses commas. 京 extends the display for modern Int64 values
beyond the native formatting range; this extension is not a native-parity claim.

Native members 0140/01DC are saved, so number grouping and graphic enlargement
belong to modern company saves too. Older modern saves use the current display
preference when first presented; their historical settings cannot be recovered.
Sound remains a user preference. Changing language preserves display settings.

Game > New Game/Exit and Company > Go out > Horse/Poker/Blackjack restore missing
direct access. Cancelling an in-game new-company draft retains the current
company and window positions. Direct casino entry retains the existing domain
ownership/coin/modal guards; equivalence of all native menu enable/visibility
rules remains pending. Menu grouping and About/utility-window behaviour remain
explicit comparison differences in the catalog.

## Resource and structural audit

The [official archive inventory](original-archive-inventory.json) covers 45 files
and 3 directories, including both horse-race sheets and five WAV files. Every
file's length and CRC16 is verified. The executable is the already supported build.
The [resource report](original-resource-coverage.json) checks all 422 BMP/GIF
property files against imported bytes or independently decoded pixels, plus 82
root-icon exports. The PE resource tree is independently inventoried (icon, icon group and version payload). Control ICO/CUR usage and dynamic assignments remain separate.
`Open.wav` has no established source trigger. `Bakuhatu.wav` has a native
dispatcher branch but no recovered caller. Neither receives an invented trigger.

The new bounded property verifier parses all 2605 known control streams and all 84
form roots. Timer design coordinates are separate Int32 fields, not rectangles;
14 nonvisual timer entries are removed from the older visual-rectangle manifest.
Continuing the office root reveals one additional icon: 537 picture properties
(400 BMP, 22 GIF, 114 ICO, one CUR). The MDI root omits the ordinary Form-class byte and begins directly with ClientLeft.
Zero object markers and reserved 98/99 markers are bounded with unresolved
semantics; structural consumption does not assign them behaviour.

The [native save field map](original-native-save-fields.json) pairs every member
reference and array rank across 130 read/write templates: 448 field slots and 447
distinct members, with 40 loops recorded. Variant subtypes, loop-bound evaluation,
complete modern field meanings and actual file encoding still need validation.
The [RNG map](original-rng-callsites.json) resolves 310 random draw sites and one
startup seed site through the MSVBVM60 export ordinals. Static sites do not
establish executed counts, initial-seed equivalence or whole-session draw order.

## Verification and remaining blockers

Domain checks cover one-time accounting, overflow, modal guards, source eligibility,
timer bids, both employment outcomes, donation acknowledgement, source-record
synchronisation, eager random draws, next-room continuation, save/load, original
sprite data and number grouping. Bilingual Godot checks cover purchase/virus,
poaching, selectors, settings, cancellation, window retention and saved receipts.
The optional full archive import is checked separately from authored fixtures.
Final command outcomes are recorded in the batch verification document.

Original runtime acceptance is **not complete**. A disposable official installation
with VB6 runtime was prepared; Wine 11 exits with host SIGSEGV before a usable
game window, including a bounded strace diagnostic. This blocks paired native
recordings and genuine newly generated GD2 fixtures on this host. All 245 original
scenario statuses remain pending. No native GD2 import/export, complete dynamic
dispatch/scheduler semantics, exhaustive artwork use or pixel/runtime parity is
claimed. The catalog keeps these concrete remaining tasks visible. #95 stays open.

Source boundaries are reproducible with `verify_company_events_evidence.py`
(34 procedure fingerprints / 40 anchors), `verify_serialized_properties.py`,
`verify_original_archive.py`, `audit_original_resources.py`,
`map_native_save_fields.py`, `map_original_rng.py` and `map_original_game.py`.
