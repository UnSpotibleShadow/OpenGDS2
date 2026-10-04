# M39 — development-room and tenant-plot clicks

This pass connects the original office click targets requested in the supplied
screenshots. Domain hit tests operate on native 466 × 290 office coordinates;
the UI converts mouse positions through the existing scale/letterbox mapping.
No original screenshots, graphics or text catalogs are published.

## Recovered hit regions

`fMainVisual 009C66BC` is the Form's MouseDown event (event slot 19). Its left
button branch begins at +00BF. After original zoom/scroll coordinate handling,
the regions compare `u = y + x/2` and `v = y - x/2`, each narrowed to a Single.
The bounds are strict, unlike the inclusive employee hit rectangles.

| Target | u bounds | v bounds | Source |
| --- | --- | --- | --- |
| Tenant slot 0 | 190 < u < 274 | 124 < v < 208 | +00D1..013D; label index 21 |
| Tenant slot 1 | 270 < u < 354 | 124 < v < 208 | +0142..01AE; label index 22 |
| Tenant slot 2 | 398 < u < 482 | −2 < v < 80 | +01B3..021E; label index 23 |
| Development room 0 | 228 < u < 260 | 34 < v < 69 | +0223..0268; selected-room write +026B |
| Development room 1 | 324 < u < 356 | 34 < v < 69 | +0470..04B5; selected-room write +04B8 |

The tenant tests are independent and overlap at `270 < u < 274`. The original
modal return sequence for this strip has not been dynamically verified, so
OpenGDS2 leaves it inactive rather than choosing an invented slot priority.
Other points map to their established slot. Source byte-anchor checks and domain
edge cases cover the rectangles, strict boundaries and nonfinite inputs.

For empty development rooms, the original opens `fGMKaihatuS`. Its handlers
`0097C19C`, `0097C26C`, and `0097BF94` write mode 0/1/2 to global `A14014`
and open the next modal form. The readable screenshot establishes these as
original game, sequel and manga adaptation. These mode values are separate from
the work engine's room-state fields; their meanings must not be conflated.

## Implemented paths

- An empty desk interior opens a compact room-action chooser. Original Game
  enters the existing verified own-game planner with the clicked room retained;
  the room dropdown is hidden on this contextual path. Toolbar/menu planning
  remains available with its room selector.
- Sequel and manga entries are displayed but disabled with an unresolved-behavior
  tooltip. They are not aliases for ordinary new-game development.
- Clicking a supported active own-game room opens its existing project screen.
  Original window visibility toggling and own-hardware room forms are not yet
  reproduced.
- A tenant plot opens a compact facility view focused on that slot. The known
  school purchase/closure services remain unchanged, and legacy unknown calendar
  state stays explicit. Other facilities and occupied-plot artwork are pending.
- The sales top-100 table now follows the supplied original column order:
  rank/title, units sold, company. Rank and sales values are right aligned.

Pending calendar forms still block office actions; employee selection and its
arrow/pane remain available elsewhere. Preview/cancel does not start a project,
spend funds or buy a facility. Original quote/meeting separation is still the
next planning-presentation task.

## Validation and limits

Direct C# compilation passes. The local domain suite passes 200 checks (198 public
plus two private original fixtures), including two new hit-region regressions.
All 27 Python checks pass, including the expanded original anchors. Godot's full
core-loop and original-import UI suites report zero failures. Private UI checks
exercise both room clicks (one at 2× with letterboxing), carried room identity,
disabled unreconstructed modes, all three tenant slots and zero preview charges.
Standard SDK/export/native Windows checks all pass in
[Validate run 37224787621](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37224787621);
PR #33 is merged. The SDK build has zero warnings/errors and public Godot plus
packaged Linux/native Windows fixtures have zero failures.

The reported sprite-edge artifact remains unresolved. The inspected original
reception employee/computer/chair rectangles contain no pure red line or
white-mask/nonwhite-color mismatch. This does not explain the owner's captured
frame or establish a fix; matching runtime state/cache/overrides and graphical
comparison remain necessary. Verified raster operations have not been changed.
