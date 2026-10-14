# Next work: third presentation review round

The owner supplied the 14 October 2026 references and requested the completed
analysis and an implementation plan. The owner subsequently approved implementation; completion is recorded in
`docs/review-round-implementation-m104.md`. Issue #95 remains open beyond this review round.

## Confirmed findings

- The native compact sales report, `fGMZishaKW`, has no product dropdown. Product
  selection belongs to the released-products list, `fGMZisha`.
- Compact chart timer `009C4570` iterates twelve independent vertical elements.
  It assigns each element's top and height from its own history cell; it does
  not connect neighboring points. Keep the established compact scale/history
  ordering while changing the renderer.
- Products selection is purple/pink across the entire row. The supplied
  Computers reference uses green; Technologies uses purple/pink. Selection
  treatment must be configured per window.
- All nine facility entry forms bind a distinct 256×160 MDIForm1.ImageFormG
  illustration. Their button sets differ: School has two command controls,
  Law Office four, and the other seven have three. Their downstream actions
  already have recovered gameplay services.
- The four development computer preview BMPs are separate 64×64 Image1 sources
  in `fGMComK`: source indices 0–3 correspond to control indices 1–4. Their raw
  offsets are `278842`, `2777B4`, `27670A`, `27566C`.
- The executable contains form-header ICO resources. The bounded audit maps
  83 child-form header icons, with ten distinct fingerprints; many forms share
  the same default icon. The office, employee/company panel and Products list
  have distinct icons. Embedded Image1 ICO arrays are separate from header
  icons and button skins. The main application icon is a separate PE resource.
- Level Up is a modal presentation in the original. `fGMInfo` handler `009A3E34`
  opens `fGMLevelUP`; timer `009B375C` supplies seven capped stat increases and
  supports a super-level-up bonus. Existing formulas/costs are already recovered.
- Awards board `fGMKontesuto` has no command buttons. Timer `009CEE10` runs
  secretary introduction, ceremony-host dialogue, per-category announcements,
  conditional secretary reactions and closing dialogue. `fGMKontesutoHp` and
  timer `009BD6F8` supply the separate announcement/reel popup. Native category
  order is Music, Graphics, runner-up, Worst, Grand Prize; current domain order
  already agrees. The owner video confirms the persistent board, separate
  dialogue and popup, progressive rows and final return to the office.
- Window dragging pauses gameplay in the owner's original-game observation.
  Static inspection does not establish that every presentation timer pauses;
  suspend gameplay clocks without assuming all UI animations stop.
- Magazine Reviews is accepted in its current improved presentation for this round.

## Facility mapping

| Facility | Original form | ImageFormG index | Primary service |
| --- | --- | ---: | --- |
| School | fGMTenantGameS | 13 | School facility lifecycle |
| Hot Spring | fGMTenantOnsen | 14 | Staff stress relief |
| Recycling Shop | fGMTenantReS | 19 | Recycle inventory |
| Technology School | fGMTenantTechS | 24 | Technology capacity study/employee roster |
| Casino | fGMTenantCasino | 21 | Casino entry |
| Law Office | fGMTenantBengo | 20 | Retail/transport fee negotiation |
| Mystery Lab | fGMTenantNazoP | 17 | Experiment start/progress |
| Production Lab | fGMTenantSeisan | 22 | Production capacity expansion |
| Hardware Lab | fGMTenantHard | 23 | Hardware planning |

The existing facility services, prices, prerequisites, costs and effects remain
authoritative. Restore the native selection/description/action layout around them;
include the facility purchase picker and relevant subordinate selection windows.
Do not use the occupied office-plot atlas sprites as popup illustrations.

## Implementation sequence

1. **Shared window behavior and artwork plumbing.** Add source-bound ICO import
   and title-bar support; map existing windows to their native forms. Add transient
   drag suspension of gameplay clocks. Release, lost capture, window closure and
   focus loss must clear drag suspension without changing manual pause or replaying
   missed ticks. Keep originals private and preserve authored fallback/overlays.
2. **Products and sales.** Route the software toolbar icon to Products. Make
   purple selection cover every column, retaining the selected game during refresh.
   Open individual reports through the list using the recovered activation path.
   Remove the unsupported dropdown row and restore compact report geometry.
   Draw independent weekly bars, preserving values and scale rules.
3. **Computers, Technologies and Level Up.** Rebuild the two purchase screens
   from the supplied compact references, with correct row colors, descriptions,
   previews and action placement. Display Level Up portrait/name and actual stat
   deltas, including capped/bonus outcomes. Capture the successful transaction once;
   showing or reloading the result must not draw again or repeat the upgrade.
4. **Nine facility entry screens.** Apply the mapped illustrations and individual
   layouts, descriptions, fee/progress fields and proven buttons. Include the vacant
   facility picker and existing study/negotiation/hardware subflows. Verify purchase,
   service and closure navigation without changing the recovered economics.
5. **Awards conversation flow.** Keep the awards board as a persistent results
   window. Remove its Announce/Continue controls and drive the source-backed phases
   through portrait dialogue and the separate announcement popup. Preserve moved
   positions, category order, winner outcomes and payments. Persist phase/page/reel
   state so loading cannot repeat a draw or payment or skip an unread message.
6. **Batched verification and handoff.** After related implementation is complete,
   run focused meaningful domain checks, a clean build, bilingual rendered layout
   checks, drag-release/manual-pause checks and workflow/save-reload regressions.
   Reiterate only on failures or relevant changes. Compare against owner references,
   publish the verified work, update #95 and leave it open for further findings.

## Evidence and limits

The source metadata, private decoded artwork, ICO mappings, button bindings and
video frame samples are in `/workspace/scratch/m104-audit/` and
`/workspace/scratch/m104-awards-video/`. No original image or dialogue text is
included here. Existing M7/M61/M71/M93/M96/M98 evidence is reused rather than
recovered again. The implementation still needs rendered English/Japanese width
checks, exact control placement verification, native button semantics and modal
continuation tests. This analysis establishes resource/handler ownership and the
flow; it does not claim Windows pixel/timer parity.
