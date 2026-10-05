# Conversation reconstruction — M49

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
serialized into modern saves. All 50 locally imported entries are guarded by
the supported executable hash; older hire-only imports still support hiring.

## Remaining conversations

This is partial conversation restoration. Secretary advice/hiring/automation,
checkpoint and shipment introductions, graduate-hire callers, remaining event
speakers and the automatic timed presenter require further tracing. The original
meeting illustration and automatic queue/window sequencing are not yet rendered.

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
