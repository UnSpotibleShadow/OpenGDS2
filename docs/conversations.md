# Conversation reconstruction — M41

Implemented: the normal recruitment hire greeting, using the small original
portrait/name-strip/message layout seen in private reference 173559. The
portrait is imported from the required original LZH. English uses ten unofficial
community translations. Japanese greeting bodies are extracted to local
`original/dialogue.json`; old imports must reimport to obtain them.
No original text table, artwork or executable is included in the repository.

## Verified source

| Behavior | Supported executable anchor |
| --- | --- |
| Copy employee/charge fee before greeting lottery | `0099FFC8 +00D6..00E1` |
| Ten variants selected by floor(Rnd × 10) | `0099FFC8 +00E4..00F5` |
| Nine fixed bodies; tenth inserts company name | `fGMSKoyou` pool entries 11–19, 1A/1C |
| Hire passes employee portrait/name, new-hire caption and modal flag 1 | `0099FFC8 +0102..0154` and subsequent arms |
| Module1 helper 62 opens fGMKaiwa with Show(1) | `009933DC +02AA..02BF`; pool B4 points to object `005EF954` |
| Portrait/name/message/form key handlers share the advance method | fGMKaiwa control event links; `00977864`, `0097789C`, `00977D6C`, `00977A24`, `00977C54` |
| First advance reveals remaining text; later advance moves queue or unloads | `00981674 +0009..008A` |
| Separate helper 63 can use text length × 3 as close countdown | `009930BC +0122..0165`; fGMKaiwa2 timer `00998F30` |

`tools/verify_dialogue_evidence.py` verifies concise byte anchors privately.
It exports no dialogue. Static analysis does not establish original desktop
behavior under every focus, window-position or timer configuration.

## Modern persistence and presentation

The accepted hire stores source identity and selected variant in a pending
notification. Simulation ticks stop until acknowledgment. Save/load restores the
notification without repeating a financial transaction or drawing another
variant. Closing, clicking the portrait/name/message or pressing Enter dismisses
it. The full text is immediately visible: typewriter speed/mouth animation are
unresolved, so no original timing is claimed. This is an isolated presentation
simplification. Original message queue and automatic presenter are not enabled.

The pending state is additive to the modern save schema; old saves have none.
Normal hiring uses the recovered greeting lottery. Graduate hiring is unchanged
until its caller's dialogue sequence is independently mapped. Domain checks
cover all ten variants, failed hires, clock blocking, invalid state, idempotent
acknowledgment and save round trips. Public Godot checks exercise all ten saved
notifications with synthetic staff; private UI checks import actual dialogue,
hire once, save, reload and click to continue.

## Unresolved conversations

The screenshot's secretary reminder is not yet implemented. Module1.46 has a
December/weeks ≤3 contest message (`009B15F0 +0430..04A6`), plus February/March
renewal advice; its invocation schedule and secretary identity/policy state
remain to be established. The calendar routine also introduces the actual
December/week 4 contest (`009DB4F4 +0567..05F8`). These are different callers.
Do not infer the reminder date or periodic repetition from the screenshot alone.

Other event introductions, speaker selection/RNG, secretary settings, office
speech bubbles, timed notifications and original conversation queue persistence
remain open. This milestone is the first playable conversation restoration,
not completion of all original dialogue.

Additional caller tracing finds Module1.46 invoked from `fMainVisual` click
handler `009C66BC +0741` and `fGMHisyoSet` action `0097743C`.
The secretary-hire/settings menu entries themselves open separate forms
(`0097A20C`, `0097A254`); they are not evidence of an automatic reminder date.
The office click region/coordinate interpretation and selected secretary
initialization still need complete data-flow verification before wiring advice.
