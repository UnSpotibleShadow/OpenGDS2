# M103: second playthrough presentation review

Issue #95 remains open for further original-game findings. This batch addresses
checkpoint name clipping, media columns, shipment controls and numeric fields,
review bubbles and score padding, compact sales graphs, exhibition alignment,
boxed trophies, renewal layout, window positions and toolbar hover spacing.

Development reports remain visible behind meetings and ancillary dialogs. Their
actions are disabled while another dialog owns the interaction. Per-room positions
are retained when reopened, and reports close when the project is removed or the
player explicitly closes them. Product switching and award announcement/continuation
retain the moved window position. These are session UI preferences, not save fields.

The original room-report Timer1 procedures (`009AECB0`, `009AE2A4`) test the room
state at entry and unload on an empty room. Other confirmed unload sites include
shipment, reset and the global close routine. The owner's original checkpoint
screens show the report beneath the selectors. No recovered evidence requires
closing it merely because a checkpoint meeting opens. This establishes lifetime
behavior; it does not establish exact Windows stacking or font parity.

The shipment Timer1 serialization at file offset `266735` contains interval
`0x50` (80 ms). The original does support holding its arrow controls. Ordinary
clicks now apply one 100-copy or 10-price-unit step, with native hold escalation
at the recovered cadence and no extra step upon releasing a repeated hold.
The former 25 ms cadence was too fast. Numeric fields reserve space beyond four
digits and keep the existing production cap and input validation.

Reviewer reactions now have speech-bubble outlines, with padded score artwork.
English portrait patches accept only the four known decoded RGB fingerprints,
replace only the bottom 16 pixels with authored “REVIEWER 1” through “REVIEWER 4”
role labels, and preserve all face pixels above that boundary. These labels are
not recovered personal-name translations. Japanese and explicit user overlays
retain their original paths. Original imports are never overwritten or published.

The original review timer (`009CAFCC`) uses independent draws from a shared
score-band comment pool (band counts 9/8/11/9/8/4). Identical comments are possible,
including the repeated comment in the owner's playthrough. Existing independent
selection is retained; uniqueness would change the original behavior.

The importer now recovers serialized BGraphic button skins and the shipment
arrow bitmaps from the supported local executable. Runtime glyph crops are applied
only to proven bindings: development promotion/cancel/ship, checkpoint cancel and
confirmation, product shipment and shipment adjustment arrows. Japanese caption
pixels from button skins are excluded; text remains localized. Window ICOs are
not button images. Other audited forms use ordinary text command buttons; raw
recovered staff skins do not imply unproven modern action bindings. Reimporting
recovers dedicated files; older recovered resource files are also supported.

`tools/playthrough_presentation_evidence.json` records four procedure fingerprints,
ten button bitmap fingerprints/dimensions and the serialized timer anchor. Verify
against the owner's executable using `tools/verify_playthrough_presentation_evidence.py`.
Public fixtures are authored; optional original artwork and rendered captures stay
outside the repository. Focused bilingual checks cover actual report-to-meeting
transitions, modal action blocking, five-digit shipment input, click/hold cadence,
review art boundaries, product/award position retention and renewal selection.

Validation completed: 521 optional-original domain checks, clean build with zero
warnings/errors, bilingual rendered focused/layout sweeps, complete core-loop,
review/event and presentation-continuation suites, and the source evidence verifier.
The tests verify geometry, interaction and continuation; exact original-game pixel
parity remains subject to further playthrough review in open issue #95.
