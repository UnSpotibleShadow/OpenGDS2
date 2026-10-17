# Ordered original-game fidelity follow-ups

Owner instruction, 17 October: work in order, finish each step, then ask permission
before starting the next and name that step. Use git-flow, with feature PRs into
develop. Actions are authorized again; no new release is requested. Findings come
from current source and the supported original's static evidence, not a paired
native Windows playthrough. Deliberate adaptations need review before removal.

| Step | Finding | Status |
| --- | --- | --- |
| 1 | Horse-race scenery, camera and finish post | Merged into develop in [PR140](https://github.com/UnSpotibleShadow/OpenGDS2/pull/140); final Actions run passed. |
| 2 | Oversized new-company setup | Merged into develop in [PR141](https://github.com/UnSpotibleShadow/OpenGDS2/pull/141); Actions passed. |
| 3 | New-company Enter/default confirmation | Merged into develop in [PR143](https://github.com/UnSpotibleShadow/OpenGDS2/pull/143); Actions passed. |
| 4 | Missing original About/version/link dialog | Merged into develop in [PR144](https://github.com/UnSpotibleShadow/OpenGDS2/pull/144); Actions passed. |
| 5 | Original menu organization | Merged into develop in [PR145](https://github.com/UnSpotibleShadow/OpenGDS2/pull/145); Actions passed. |
| 6 | Facility conversation/transaction ordering | Merged into develop in [PR146](https://github.com/UnSpotibleShadow/OpenGDS2/pull/146); Actions passed. |
| 7 | Consumer Investigation paraphrasing and extra receipt | Merged into develop in [PR147](https://github.com/UnSpotibleShadow/OpenGDS2/pull/147); Actions passed. |
| 8 | Original GD2 save compatibility | Skipped at the owner’s explicit request; original GD2 compatibility remains unimplemented. |
| 9 | Persisted active casino rounds | Reviewed: retain the deliberate modern continuity policy; native evidence and fresh-session continuation regressions recorded in [the review](casino-save-continuity-review.md). |
| 10 | Casino exhaustion and numeric-boundary recovery | Not started; intentional protection against native hangs/errors to review. |

## Step 1: scenery recovery

Supported executable SHA-256:
`4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.
The official LZH contains all three backgrounds and both horse/mask sheets;
the existing importer already copies them. No original bytes are committed.

Original `fKeibaMain 0099061C +009D` loads `Graphic/Beibazyo.bmp` at race start.
Timer `009EB860 +3EA6` loads `Beibazyo2.bmp` at tick 45, then `+3F3C` loads
`Beibazyo3.bmp` at tick 200. The close-up source is 421×217; the visible client
is 421×173. Other backgrounds are 421×173.

The close-up self-copies at `+0331..0562` move bands right by 2 pixels
(y=0, height=77), 6 pixels (y=82, height=91) and 21 pixels (y=173, height=44).
The last band is outside the visible viewport. Each second copy reads the tail
AFTER the first overlapping copy. Its effective horizontal period is therefore
421 minus the band's shift; ordinary modulo-421 wrapping would differ.

Finish-phase `+1218..12D0` similarly shifts the full background right by 10
until camera B reaches −1010. `+1304..13AF` draws the 12×147 finishing post from
sheet (0,169) at (round-even(−B−800),10), using mask AND then colour XOR, before
drawing runners. The renderer previously cloned a fixed starting backdrop and
never used the camera to draw this post.

Rendering now reconstructs the applicable backdrop and cumulative scroll from
the saved phase/tick/camera. The native front-buffer copy precedes the background
scroll; tick increments and scene transitions happen after presentation. Saved
ticks 45 and 200 therefore still display the preceding scene. At tick 200 the
renderer reverses the known position transform for that last close-up frame;
sprite frames likewise use the pre-increment tick. The first visible close-up
scroll is tick 47 and the first visible finishing scroll is tick 202. Repeated
redraws and reloads do not advance scenery, consume randomness, mutate race state
or change wagers/results. Missing
scene files retain the reimport hint. Native timer delivery and final Windows
pixel acceptance remain separate from these recovered drawing rules.

Regression coverage executes an independent sequence of overlapping copies,
checks phase boundaries and the stopped camera, verifies post clipping/layering,
and compares repeated rendering plus JSON restoration without state changes.
CI validates source and casino workflows only; it does not export or publish.

Local validation: application build with zero warnings/errors; clean Godot 4.4.1
.NET editor import; 575 domain checks with the private original executable and
independently recovered employee fixture; 93 Python checks (17 optional skips); English/Japanese casino
layouts and horse save/reload/settlement workflows with zero failures. The source
verifier checks all 32 rendering literals, including 25 new scenery/post anchors.
Private logs and original materials remain outside Git. Native Windows visual
acceptance remains unrun.

## Step 2: compact new-company setup

The original `fGMGameNew` designer has a 274×232 client and a 260×164 picture
control. Its load handler `0098801C +0129..0154` assigns MDI `ImageFormG[18]`;
that 256×160 registration illustration starts at file offset `047345`. The
existing local artwork inventory already imports it, so the UI can use the
recovered file without distributing or newly extracting original pixels.

The port now uses a fixed 340×292 client, 25-pixel inputs and adjacent 25-pixel
Confirm/Cancel buttons, instead of the general 620-pixel screen and tall stacked
controls. English labels fit alongside both required names. The original
president control's serialized row is below the initial client; this pass keeps
both port-required fields visible rather than assuming the original runtime
always shows that hidden designer row. The in-game dialog also uses the recovered
original setup-window icon. The title route uses the same bounded canvas.

Validation messages have reserved space. Name limits and validation, trimmed
drafts returning from mode selection, explicit cancellation and window-close
cancellation retain the existing behavior. Missing illustration data shows the
reimport hint within the picture area. Mode-selection presentation and Enter
confirmation are separate from this layout step; step 3 is recorded below.

The dedicated bilingual UI fixture checks containment at 720p and in a tall
window, original-art cache binding, missing artwork, inline validation, full
32-character drafts, mode-picker return, title/in-game cancellation and retention
of the active company's name/funds. It uses authored assets and is included in
validation-only Actions. Native Windows pixel/font acceptance remains unrun.

Local result: application build passes with zero warnings/errors. The bilingual
company-setup smoke check passes with zero failures, including both entry routes,
draft restoration, cancellation and artwork fallback. Compact input padding keeps
the inherited desktop font and focus outline within the 25-pixel field height.

## Step 3: Enter confirms new-company names

The supported original's `fGMGameNew.Command1` record at file offset `235E1C`
serializes property 19 (`Default`) as true: marker/value `13 FF` at `235E42`.
The indexed property name/type is recorded in `tools/vb6_property_types.json`.
This is static evidence for the form's default Confirm action; native Windows
keyboard/IME acceptance remains unrun. It does not establish a separate
Enter handler on both original text controls.

Both port name fields now use Godot's `TextSubmitted` signal to call the same
local confirmation function as the Confirm button. Blank or whitespace-only
names keep the form and inline validation; valid names are trimmed before mode
selection. Submission is scoped to the name fields and does not start a game
or select a mode. Existing button and cancel behavior remain intact.

The bilingual company-setup smoke fixture sends actual Enter and keypad Enter
press/release events with each name field focused. It covers either missing name,
inline validation/focus retention, valid advancement, trimmed drafts after
canceling mode selection, title and in-game routes, and the existing cancellation,
artwork and layout checks. Step 4 is recorded below.

Local result: application build passes with zero warnings/errors; the expanded
company-setup keyboard/layout smoke check passes with zero failures in both
languages. The existing Actions workflow runs this fixture on the PR. No export,
release, main promotion or unrelated fidelity step is part of this change.

## Step 4: About/version/website dialog

Original `fMainAbout` has a 268×255 client, a 254×175 logo control at (8,8),
website/copyright labels at y=192, a Version label at y=208 and a website button
at y=224. The original local logo payload begins at executable file offset
`31EDEC` (250×171 BMP); the complete serialized picture record starts at `31EDE4`.
The existing importer already recovers it. No original pixels are committed.
The two ImageY controls are serialized below the client (y=280), so they are not
invented as visible controls in this adaptation.

The website label caption is `http://kairosoft.net/`; Command1's caption is
“Go to the Kairosoft website.” Their handlers `00980260` and `009801B4` have the
same decoded call sequence through the same imported target at +004B, with no
form-unload call before returning. The port uses the HTTPS form of that official
homepage for both actions and leaves About open. OS/browser launch acceptance
remains a native desktop check; automated fixtures never open a real browser.

Help > About / Version now opens a compact 300×296 client using the locally
imported original logo and form icon. A separate port-name/version row clearly
identifies Open Game Dev Story II and `BuildInfo.Version`; the original game's
2001–2008 Kairosoft copyright stays separately attributed. The small height
increase allows readable English labels. Missing artwork gives the bounded
reimport hint. Window close returns to the existing office without changing its
company or funds. Existing Help actions retain their order; broader menu changes
remain step 5 and still require owner permission.

The bilingual public fixture covers the real Help-menu route, client/control
bounds, exact current port identity/version, copyright, imported artwork/icon,
missing-art fallback, both connected website controls, repeated opening and close
return with unchanged company/funds. It uses authored graphics, not original
artwork. Native Windows pixel/IME/browser acceptance remains unrun.

Local result: application build has zero warnings/errors; Godot editor import and
the bilingual About fixture pass with zero failures. A fresh supported-original
map rebuild passes for all 86 objects / 669 procedures / 35 workflow dispositions;
generated Markdown/HTML and the source catalog are synchronized. Workflow lint
also passes. Source CI includes the new fixture; no export or release is requested.

## Step 5: original menu organization

The supported original menu inventory recovers six roots in this order: Menu,
Actions (`mKoudou`), Information (`mInfo`), Windows, Settings and Help. The port
now uses that order (the translated first caption remains Game), including the
native separators and Company development / Go out nested actions. Recruitment,
poaching, secretary policies/change, purchases and bank are under Actions; staff,
products, sales rankings, released hardware, own hardware and reach-time records
are under Information in native order. Help contains topics, a divider and About.
Existing command callbacks and transaction/availability guards are reused.

Existing port navigation is retained as a separated Back to Title entry, More
actions (planning shortcut and secretary advice), and Game details (expanded sales
history/reviews). These additions are not claimed as original menu entries.
Windows now has Office, Company and Other windows. The latter explicitly groups
Employees/Products; the original unbound Other-windows entry's automatic MDI list
behavior remains unresolved, rather than being guessed from its caption.
The three UP toolbar actions and other toolbar routes are unchanged.

Public fixtures now use the moved menu commands/submenus instead of the removed
Company/Employees/Development/Products top-level groups. The new bilingual menu
fixture checks ordering, separators, nested company/direct-game choices, guarded
own-hardware access, moved bank/ranking callbacks, employee-window reopening,
New Game/cancel and unchanged funds. Step 6 (facility conversation/transaction
ordering) still requires owner permission. No release or main promotion is part
of this change.

Local validation: application build has zero warnings/errors; editor import,
bilingual menu fixture, original Settings fixture and full core-loop UI check
pass with zero failures. Fresh
supported-original map rebuild and workflow lint pass. Source Actions includes the
menu fixture; native Windows menu/pixel acceptance remains unrun.

## Step 6: facility result boundaries

The earlier chain implementation deliberately committed all service effects before
showing saved results. Static instruction ordering establishes three observable
mismatches: recycling clears stock, calls the result dialogue (`0098DDC4 +0195`),
then calls the financial accumulator (`+01CE`); production reports its result
(`00988DAC +0109`) before the capacity/cash global stores (`+0138`, `+0146`);
Hot Spring debits (`0099E164 +01B2`), reports relaxation (`+01F6`), then clears
source stress in the loop (`+0213..0270`) before employee reactions.
These anchors are from the supported original and the existing facility-chain
fingerprints. Technology study already debits and increases capacity before its
result, so it is not changed. No generic delay is imposed on the other facility
flows, whose recovered chains are documented in the facility audit.

New receipts use additive `EffectsPending` (absent/false for older saves).
Recycling clears stock on acceptance but credits cash/profit accumulators only
when its result is acknowledged. Production saves the single rolled gain and old
capacity, then applies capacity and the direct cash debit on acknowledgement;
there is still no affordability gate or profit-accumulator debit. Hot Spring
still charges first, then clears both employee/source stress on acknowledgement
of the secretary page, before staff pages. Selected reaction snapshots remain
reserved at acceptance to preserve the established deterministic save/resume
path; this does not claim native RNG callback timing during the modal dialogue.

Pending effects are validated against their source plot, page, stock/capacity
state and applicable numeric bounds. Applying and clearing the pending flag is
one synchronous acknowledgement; reload cannot reroll or repeat the effects.
Old unread receipts remain already applied and are never credited/debited again.
Overflow preflight remains before irreversible stock disposal. Save/load and UI
transaction guards keep other services and the office clock suspended.

Domain regressions cover the before/after boundary, saved results, single draws,
negative cash, unchanged production profit totals, legacy already-applied
receipts, duplicate acknowledgements and invalid pending snapshots. The bilingual
UI fixture exercises all three through actual facility/result/save/load routes.
Step 7 (Consumer Investigation wording and extra receipt) still needs permission.

Local validation: 559 domain tests pass, including missing-field legacy-save
coverage; clean application build has zero warnings/errors; bilingual ordering UI
fixture passes with zero failures. The original-chain verifier passes all 56
procedure fingerprints / nine illustration and event bindings. The long combined
facility/promotion runner initially used stale payment assertions and hit a Godot
.NET shutdown assertion after finishing. Its assertions were updated for the
new boundaries and a locale filter permits independent bounded processes; record
those final process results separately. Both independent English/Japanese full
facility and six-media promotion runs subsequently pass with zero failures and
clean exits. Native Windows modal timing and pixel acceptance remain unrun.

## Step 7: Consumer Investigation dialogue and completion

Recover the actual three-page investigation and eight-page refund / two-page
no-recall result chains from `009EFA40 +38A0..4186`; see
[recall conversations](recall-conversations.md) for anchors, text bindings and
translation details. The native one-month dialogue wording stays distinct from
the existing five-week countdown. Missing secretary introductions, the distinct
full-price reaction and the separate dismayed reaction are restored. Schema 19
imports 15 local-only text entries (389 total); schemas 1–18 remain supported.
The final page acknowledges directly and returns to the existing sales route,
removing the authored receipt and its extra confirmation. Desktop save/load and
legacy pending events retain their accounting and replay behavior. No economic,
random, timing or bankruptcy rules are changed.

The bilingual UI fixture covers investigation and all four results, original page
counts, replaced parameters, zero-price legacy quantity fallback, unread save/load,
direct acknowledgement and completed reloads. The core-loop fixture uses the
real conversation boundary rather than waiting for the removed receipt button.
Optional-original domain checks cover schema/count and native constant-pool text
bindings. Step 8 (original GD2 save compatibility) still needs owner permission.

Local validation: clean build (zero warnings/errors), editor import, 577 optional-
original domain checks, bilingual recall UI checks (zero failures) and the full
core-loop UI check (zero failures) pass. Workflow lint also passes. Source CI
includes the recall fixture. Native Windows modal/pixel acceptance remains unrun;
no export or release is requested.

## Step 2 follow-up: registration illustration pixel patch

The owner requested this missing artwork translation after step 7 and explicitly
skipped step 8. The original registration bitmap is 256×160 at file offset
`047345`. Both the semantic CompanySetup alias and recovered-resource path now
use the existing verified-local-art patch pipeline in English. The source RGB
fingerprint is `1db83a89b10937eecb87433be0901367ecd94c923ded83d66c59c63b14d02b7a`.

Authored pixel lettering replaces only the town-hall sign and bottom registration
caption with TOWN HALL / REGISTRATION. The latter restores the original road/curb
gradient per scanline from adjacent clean pixels, instead of painting a flat box.
Both bounded interiors preserve all other pixels, including the character and
buildings. Japanese, unknown/modified source art and explicit user overrides are
preserved. The source import remains untouched; generated English PNGs live only
in the local localized-artwork cache. No original image bytes enter Git.

Public tests use authored data for bounds, immutable input, determinism, alias,
language and override behavior. A private original fixture exercises the real
fingerprint and produces a nearest-neighbor enlarged preview for visual review.
Validation: 560 domain checks, the C# build (zero warnings/errors), workflow lint
and both public/authored and private/original bilingual artwork fixtures pass.
The enlarged generated image was visually inspected.
The owner subsequently authorized step 9; its review follows below.


## Step 9: retain modern casino save continuity

The [save continuity review](casino-save-continuity-review.md) re-verifies original
coin serialization and the absence of shared card-round scratch from all 130
paired record templates. Native form-open resets are distinguished from company
save loading and process lifetime. The existing modern policy is retained:
loading a saved active game resumes it, without rerolling, recharging or replaying
payouts. Dropping that state would risk paid hands and partial awards.

No gameplay or save-schema change is warranted by this evidence. Five new domain
checks compare uninterrupted play with a fresh session restored every callback
for all four activities, including saved RNG and partial collections. The full
565-check public suite and bilingual card-game UI continuation pass. Source CI
runs the new UI fixture alongside the existing horse fixture. Native Windows
save/load acceptance is still unrun. Step 10 remains pending owner permission.
