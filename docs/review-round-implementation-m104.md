# M104 — third review implementation

Issue #95 remains open for the next owner playthrough. This implements the approved third review plan; the accepted Magazine Reviews layout is retained.

## Result

- The software toolbar opens Products. Products highlights the whole selected row in pink; Computers retains green selection and Technologies uses pink. The compact sales report has no unsupported dropdown or ellipsis, and both sales-history variants draw twelve independent bars. Sales History and Magazine Reviews remain accessible from the Products menu.
- Native form icons are recovered from 83 child-form headers and bound where the corresponding modern window exists. All nine Company Facilities have their own locally imported artwork and compact entry layout, with compact selection, study, negotiation and hardware-planning subflows. Four computer previews are imported. Original artwork stays in the user's local archive cache.
- Computers and Technologies use their original compact table/description arrangement. Level Up shows the committed seven ability gains and portrait in a separate modal. Saving and loading the result does not upgrade again.
- Dragging a desktop window suspends gameplay time until release, including release outside the caption and lost capture. Manual pause remains independent. UI presentation clocks continue.
- The awards board has no action buttons. A separate secretary/host conversation and announcement reel drive the five categories while the same board retains its position. Saved dialogue pages and reel cursors resume without repeating draws or payments. Existing category ordering, prizes and the worst-game penalty remain unchanged.

## Evidence and verification

`tools/review_round_evidence.json` contains hashes, offsets and form/handler bindings, without original pixels or dialogue. `tools/verify_review_round_evidence.py` checks 83 icons, nine facilities, four computer previews and five procedure fingerprints against the supported executable. Native forms and the owner's original video establish the awards board/conversation split; the owner observed drag suspension directly.

Local checks: clean application build; 522 optional-original domain checks; bilingual focused rendered review suite; complete core-loop regression; bilingual mid-reel/unread-result continuation; native layout containment regression. Review/event and presentation-completion regressions also pass, covering prior reviews, exhibitions and facility result continuations.

## Owner testing

Reimport the original archive after updating the source to populate the new icons, facility images, computer previews and native Japanese awards dialogue (dialogue schema 13, 273 entries). English dialogue is authored translation. Older caches still have fallbacks. No original archive, extracted artwork, screenshots or saves are distributed in this change.

Original Windows fonts, DPI, OS stacking and exact timer pacing are not certified. This is source publication for another owner review, not a new exported release. GitHub Actions and repository visibility remain unchanged.


M105 evidence correction: `MainForm3` serializes no icon. Its former binding
borrowed the next form's ICO, so the corrected importer/evidence has82 valid
child-form bindings. See `presentation-evidence-m105.md`; other M104 artwork
and functionality are unaffected.
