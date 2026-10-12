# Paid room promotion — M97 / #95

Implements the room branch of meeting mode 1 under the documented fresh-Empty
local Variant premise. Six authored media descriptions and source fees/bases/spans
come from M93, not the exhibition screenshot. Source references: report click
009AAA8C index 3 (stage >=1), media selection 00980A38, confirmation 00982064,
initializer 009F651C +A252..A31F, and meeting timer 009EFA40 +0DB5..17D5.
Proposal acceptance 0099BA80 +02A7 resets A12AC4 to zero for new projects.

Selection/cancel cost nothing. Confirmation checks funds and opens a saved
meeting. On callback 27 one Single random draw supplies base + floor(draw*span),
using the project's use count across all media. Gain is capped against 99 minus
current recognition, including negative adjustment above 99. The count increments
and the fee passes through the existing financial accumulator once. Result
acknowledgment resumes the project; save/reload retains phase/result and never
rerolls a completed transaction. Office progression is paused during the meeting.

Legacy projects without the newly saved count retain unknown history and cannot
receive guessed diminishing returns. New accepted software proposals initialize
zero. No clean released-product advertising selector is invented: native unused
branches use inconsistent room/product indices. Local frame reuse still requires
native comparison. Modern display uses 25ms callbacks, without claiming native
Windows delivery, cosmetic animation/RNG, or font/window parity. The meeting uses
the existing locally imported planning illustration and authored bilingual UI text.

Ticket #95 remains open. Office pacing #94 and exhaustive artwork-family review
#97 remain separate and deferred. M97 is a milestone name, not closure of issue #97.

Validation: 493 domain checks; clean application build/import; bilingual promotion
selector/cancel and partial/completed meeting save-reload checks; M93 bounded
original handler verifier (32 fingerprints, 60 anchors, 15 events, four resources,
78 numeric literals). No publication-only test reruns.
