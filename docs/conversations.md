# Conversation reconstruction — M49–M52

Normal hire greetings, ordinary planning discussions and manga rights meetings
now share the compact portrait/name-strip/message window. Portraits and Japanese
bodies come from the required local original import. Reimport the LZH to refresh
`original/dialogue.json` with the planning and rights sequences. English uses
unofficial community translations; no original text table or artwork is shipped.

## Verified source

| Behavior | Supported executable anchor |
| --- | --- |
| Copy employee and charge fee before greeting lottery | `0099FFC8 +00D6..00E1` |
| Ten variants selected by floor(Rnd × 10) | `0099FFC8 +00E4..00F5` |
| Hire passes portrait/name, caption and modal flag 1 | `0099FFC8 +0102..0154` and subsequent arms |
| Shared click/key advance handler | `00981674 +0009..008A` |
| Timer1 interval of 20 milliseconds | Serialized fGMKaiwa property at file offset `248EAC` |
| Mouth cycle branches at 2, 4, 6, 8, 10 | `00998AB0 +0032/+00CD/+0168/+0203/+029E` |
| Seven planning profiles and their dialogue | fGMKikakuKaigi `009EFA40 +00BB..0D86`, pool `005FAFCC` |
| Publisher rights meeting (mode 16) | Same procedure `+60C7..639E`; see manga-adaptations.md |
| Separate timed presenter | Module1 helper 63 `009930BC +0122..0165`, fGMKaiwa2 timer `00998F30` |

`tools/verify_dialogue_evidence.py` checks bounded hire/timer anchors privately.
The source advances its text cursor in UTF-16 units; Godot displays complete
Unicode characters so a surrogate pair is never split. The first click while
text is typing reveals the remaining message. The next timer pass marks it
complete, and a later click/Enter advances to the next page. Employee portraits
cycle through locally imported closed/half/open/half mouth frames and settle
closed. Modal typing continues while the simulation clock is paused.

## Meetings and persistence

Planning generates one of seven profiles and its 100-point focus with the
existing recovered random sequence. The discussion uses that same profile;
it does not draw a second proposal. The introduction is included for released
product counts ≤3; selected explanatory lines appear only for counts ≤2.
Profile 3's final line and all four profile 6 lines remain unconditional, matching
the branches. The source speaker is the named secretary; invented staff voting
or random speaker changes are not added. A new proposal replays its new profile's
discussion. Closing the proposal discussion cancels the reversible proposal.

Accepted hires persist the source identity and greeting variant. Save/load
restarts their reveal without repeating the fee or drawing another variant.
Simulation waits until acknowledgment. The close button uses the same advance
behavior. Rights/planning discussions before project acceptance are UI state;
loading a company cancels them without charging. The original queue is not
serialized into modern saves. All 92 locally imported entries are guarded by
the supported executable hash; older hire-only imports still support hiring.

## Remaining conversations

This is partial conversation restoration. Secretary advice/hiring/automation,
graduate-hire callers, remaining event
speakers and the automatic timed presenter require further tracing. M50 restores the original planning illustration; other meeting scenes and the
automatic timed queue/window sequencing remain unresolved.

Module1.46 has a December/weeks ≤3 contest reminder (`009B15F0 +0430..04A6`)
and February/March renewal advice. Its invocation schedule and policy state are
not established. The actual December/week 4 contest comes from a different
calendar caller (`009DB4F4 +0567..05F8`). Module1.46 also has verified manual
callers in `fMainVisual` (`009C66BC +0741`) and `fGMHisyoSet` (`0097743C`).
The secretary click rectangle and initial portrait are known, but the full advice
queue must be mapped before enabling it. See secretary-initialization.md.

Domain tests cover reveal/click/mouth transitions, Unicode, all seven meeting
branches and persisted hires. Headless Godot checks cover save/reload and modal
clock blocking, multiple discussions, proposal cancellation and the manga loop.
Original Windows focus/timer behavior and graphical comparison remain untested.

## M51 development and shipment conversations

The type and genre introductions now precede their existing choice panels;
the completion introduction precedes release/debugging choices. The shared
compact portrait window uses the original generic secretary speaker. Detailed
type/genre meeting modes 4/5 were not reconstructed by M51; M52 adds their
discussions and confirmations below. Sequel checkpoints retain their existing
automatic inheritance behavior.

| Verified caller | Supported executable anchor |
| --- | --- |
| Type introduction, secretary and modal helper | Module2 `00A0283C +A807..A820` |
| Genre introduction | Same procedure `+ABA1..ABBA` |
| Completion message, one-based room number | Same procedure `+ADC5..ADE6` |
| Adaptation/popularity, balance and quality remarks | fGMSyukka `009C9CB8 +0455..0D35` |
| Price comments: <4000, ≥20000, ≥10000, ≥8000, ≥7000 | Same procedure `+0DB8..0F54` |
| Mandatory magazine introduction before reviews | Same procedure `+1215..1250` |
| Product index increments before optional market summary | Same procedure `+101F..102B`, then `+10B1..10C8` |
| New-company market slots cleared | Module1 `009DD234 +1403..1411` |

`tools/verify_core_dialogue_evidence.py` checks bounded private anchors without
exporting dialogue tables. Japanese bodies are extracted only into the player's
hash-guarded cache (schema 3); English contains unofficial authored translations.
Schema 1/2 imports remain readable; reimport adds Japanese checkpoint/shipment
bodies. No original text or artwork is published.

Shipment order is adaptation remark when applicable, balance outcome when its
roll selects a remark, eligible novelty/fun/graphics outcomes, optional price
comment, then magazine introduction and the existing four reviews. There are at
most seven messages. No extra random draws are made for presentation. The first
two products skip quality-specific rolls as before. The source's music remark
actually tests graphics; its later graphics thresholds remain unreachable.
Price comments do not change the already reconstructed sales formula.

The optional later market-summary branches read the next product slot after
incrementing the product index. Fresh-company slots are cleared, so these lines
are normally absent. They are not substituted with the just-shipped game's
factor. Dirty unused slots from an original save have not been tested; original
save compatibility remains unresolved. This is static/data-flow evidence, not
an original Windows runtime comparison.

Checkpoint acknowledgments only mark the introduction read; stage, selection
fees and experience rewards remain under the existing domain services. Each
shipment stores stable message IDs and a read cursor after calculating the
market and reviews once. Completing a page advances one cursor. Review
acknowledgment is blocked until all messages are read; simulation clocks stay
paused through the existing pending-review guard. Loading restarts reveal at
the unread page without charging or rerolling. These persistence fields are
modern infrastructure; original queue serialization is not claimed.

Old modern saves lacking a shipment queue go directly to their saved reviews;
the lost lottery branches cannot be reconstructed reliably. Validation rejects
unknown message IDs, duplicate/out-of-order stages, missing final review notice,
invalid cursors and unread queues marked as review-acknowledged.

Regression checks cover every balance roll, accepted/rejected quality branches,
price/popularity boundaries, invalid/legacy saves and acknowledgment guards.
Godot checks the actual introduction order, avoids repeating a read checkpoint
after loading, freezes sales/accounting during shipment, resumes midway without
changes to money/market/review scores, and completes ordinary/sequel/manga sales.
The original displays remarks while calculating shipment; OpenGDS2 calculates
the transaction once before displaying its persisted remarks. Normal resulting
rules/RNG order are preserved; original mid-dialogue interruption/save behavior
is not claimed.


## M50 planning illustration and cancellation

The ordinary planning form loads MDIForm1.ImageFormG[0] through getter `032C`
(`009A6DDC +00A0..00CB`). Control metadata identifies ImageFormG as member 13;
the serialized array record at file offset `18B188` is a 256×160 BMP, preceded
by its checked control name/index. The importer writes it only to the user's
`original/Graphic/Embedded/PlanningMeeting.bmp`. No illustration is published.

A separate classic child window shows that original image at native size beside
the compact conversation. It remains present across discussion pages, then closes
before the proposal. Window placement and reversible close behavior are authored
Godot presentation; original desktop positioning/focus parity is not claimed.
Older caches lacking the image still allow discussion; reimport restores it.
Local dimension-preserving artwork overrides apply through the existing loader.
Other source meeting modes use different images and are not inferred from this
one mapping.

## M52 alpha and beta meetings

Meeting mode 4 opens fGMalpha for type selection, and mode 5 opens fGMbeta for
genre selection. Both use ImageFormG[0], the existing locally imported planning
illustration. The image is now present during checkpoint discussion, selection
and confirmation. It is removed when the meeting completes or a different screen
loads. English uses authored translations; Japanese comes from a local,
hash-guarded schema-4 cache. Older schema 1–3 caches remain readable; reimport
adds the new bodies and meeting captions. Original text/assets remain local.

| Verified behavior | Supported executable anchor |
| --- | --- |
| Type/genre introductions only when product count ≤2 | Module2 `00A0283C +A7E0..A7EB`, `+AB7A..AB85` |
| Alpha/beta meeting captions and ImageFormG[0] | fGMKikakuKaigi `009A6DDC +036B..0414` |
| Modal type/genre selection forms | Meeting timer `009EFA40 +1CA9..1D1C` |
| Candidate remarks, six variants with variant 3 silent | fGMalpha load `009D2F68 +05F3..0908` |
| Type choice, then confirmation/publicity | fGMalpha confirm `009ACF14 +0761..08B5` |
| Beta discussion and recognition thresholds ≤10/≤30 | fGMbeta load `009C7860 +017F..03B4` |
| Genre choice, affinity response and +20 Fun | fGMbeta confirm `009B00D8 +0767..099B` |

The type meeting keeps the recovered candidate list, including source order and
duplicates. It evaluates a three-way speech roll for each candidate, then a
six-way variant roll when eligible. Variant 3 has no message. Other variants
suggest the candidate or connect it with the target audience. The discussion
does not choose a type or change fees/quality. Preparing it once saves its
message IDs and candidate arguments; loading never redraws the list or remarks.

The beta meeting has four pages: beta completion, recognition advice selected
at thresholds 10/30, a request to choose a genre, and a reminder of the selected
type. Recognition advice is descriptive; it adds no advertising transaction.
Type confirmation reports the choice and the resulting recognition from the
existing fame/3 rule. Genre confirmation follows the existing audience/type
affinity test: a compatible choice reports the pairing and the +20 Fun bonus;
otherwise it has one ordinary acknowledgment. Source speakers are the generic
employee, with the secretary reporting the affinity bonus. Named staff speakers
or voting mechanics are not invented.

The Godot selection panels use a bounded scrolling list, preview and separate
Confirm button. Previewing an option never charges funds. Confirming delegates
to the existing selection rules once, then records the follow-up queue.
Unpaid/locked options cannot confirm. Simulation, payroll and sales stay paused
while a checkpoint decision or unread confirmation exists, including after the
selection cleared PendingDecision. Loading a confirmation resumes its unread
page. Completing the last confirmation clears the meeting state and resumes
normal development. Completed pre-choice queues remain saved until selection,
so an entirely silent type discussion also cannot reroll on load.

Save validation checks phase/stage consistency, stable message IDs, cursors,
candidate references, required type arguments, and fixed confirmation order.
Legacy projects with no meeting state can prepare a new discussion at their
current selection checkpoint; no past charge or bonus is reconstructed.

This is partial visual/interaction parity. The original example-title column and
its cosmetic random draws, hover/radio details, original desktop focus/position,
and the full abandon-project/stress transaction remain unfinished. Godot uses
authored list arrangement and fee labels. Selection effects are committed before
their persisted confirmations, whereas the original applies some effects between
dialogue calls. Normal final results are retained; original interruption/save
timing during those calls has not been compared on Windows. The automatic timed
presenter and secretary/event conversations are separate remaining work.

`tools/verify_checkpoint_meeting_evidence.py` checks bounded source anchors.
Regression checks cover silent variants, duplicate candidate order, recognition
boundaries, repeated/failed choices, malformed saves and stopped clocks after
confirmation. Headless Godot checks the actual imported picture, preview before
charge, saved confirmation cursors and the ordinary/sequel/manga core loop.

Cancelling through either window, loading or changing screens removes the scene
and stops the typing timer before its controls are removed. This fixes a stale
control reference found during the presentation pass. Private headless UI checks
compare the actual texture with the local import, cancel during typing, wait
through multiple timer intervals, verify no fee/project was created, then run a
fresh complete discussion/proposal. The fixture-free core loop still works
without the private illustration.

## M53: manual secretary advice

The office secretary's inclusive native click rectangle and a Company/Settings
entry now open the original conditional advice queue. Fourteen authored English
translations accompany locally imported Japanese pages. Current imported
campaigns retain initial secretary source 1/portrait 2; older saves keep unknown
identity. Message IDs and read cursors persist without rerolling, spending money
or running office/sales/calendar ticks. Recommendations do not perform or unlock
purchases. See [exact conditions, queue order, source mappings and remaining
presentation limits](secretary-advice.md). Secretary recruitment/policies, actual
name captions, event speakers and automatic timed presentation remain unresolved.

## M54: secretary replacement and named captions

Secretary replacement charges before a modal farewell, changes the identity and
office tile before the modal greeting, and persists both phases. Advice and
replacement use the current secretary's locally imported Japanese name or
provisional English reading. Original generic window captions and named strips
are distinct. The local dialogue cache is now schema 6, 109 entries. Automatic
timed dismissal, original focus and exact placement remain unresolved. See
[secretary management](secretary-management.md).
