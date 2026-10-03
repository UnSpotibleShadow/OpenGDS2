# Original / OpenGDS2 comparison — M32

This audit compares the supplied GameDev2DX distribution and original desktop
screenshot with OpenGDS2 through M31, then records the first correction in M32.
The executable is the supported hash in `source-hashes.txt`. Original files,
extracted images, catalog tables and full procedure traces remain local.

## How the comparison was made

- Inspect the user's original desktop screenshot and the locally rendered
  OpenGDS2 office at its native 466 x 290 pixels, including a hired worker,
  lounge rest and a development desk.
- Compare the current Godot controls and domain services with bounded original
  P-Code, serialized form/control metadata and locally imported catalogs.
- Run local original-data/domain and real Godot scene-tree fixtures. These
  exercise OpenGDS2; they do not execute the original Windows program.
- Cross-check the original feature scope against the
  [official PC game page](https://kairosoft.net/game/pc/gamedev2.html).
  Its description establishes planning, beta/debugging, shipment, technology
  research, hardware licensing and eventual own-hardware development. It does
  not supply the missing simulation formulas.

The local host has neither Wine nor a desktop display server. This is a
screenshot/static-source comparison plus OpenGDS2 runtime testing, not a claim
of side-by-side original runtime verification. The screenshot's April week 2
and visible employee are an observed state, not proof of the opening roster.

## Differences and priorities

| Area | Original evidence | OpenGDS2 status / difference | Follow-up |
| --- | --- | --- | --- |
| Desktop and office | Supplied screenshot shows simultaneous office, company, staff and product windows; local atlas/tiles recover the office | M25–M30 restore these windows and native scene; chrome is community-authored and complete visual comparison is pending | Compare layout, clipping, fonts and all dialogs on an actual display |
| Employee selection | `fMainVisual 009C66BC +0758..08CB` scans workspace slots and uses inclusive inset sprite bounds | M31 office artwork had no input; table activation opened the whole roster. M32 selects the clicked/activated employee | Extend remaining original pointer interactions only after tracing their guards |
| Toolbar | Sixteen original images imported locally; original screenshot uses images | M29–M30 use imported images and fallback pictograms with translated help; some original command associations remain unproven | Recover command binding and local bitmap translations |
| Company information | Screenshot includes weekly/monthly/yearly financial rows and tenant information | Current company window has funds/fame/research/capacity/rank/date; period accounts and tenants are missing | Map accounting members 22C/23C/24C and their reset ordering |
| Opening staff | Screenshot contains an employee; its displayed stat pattern is a lead for imported source 29 | New companies begin without a proven initial employee assignment | Trace assignment/call sites; do not infer an opening hire from this screenshot |
| Calendar | Weekly routine advances weeks/months, triggers March-end renewals and further economics | Work/sales/activity ticks run; displayed company date remains at opening | Highest gameplay priority: complete ordered calendar transactions and modal resumption |
| Payroll/contracts | Isolated renewal formulas and apparent shared-index payroll behavior recovered | Annual renewal projection exists; live payroll and renewal scheduling remain disabled | Establish scratch lifecycle and runtime behavior before enabling charges |
| Hiring/progression | Database, five-slot recruitment, costs, dismissal and level-up recovered | Connected, including study and six project technologies; capacity edge cases and original random stream remain incomplete | Compare original roster/placement runtime and special source workflows |
| Development | Planning/type/genre/debugging and ordinary contribution formulas recovered | Own-game loop works; polishing, sequels, outside jobs and several presentation paths are missing | Recover acceptance/completion branches for each unsupported workflow |
| Work presentation | Main loop contains separate visual notification fields and atlas rectangles | Walking/rest/seating render; several work/burst/event bubbles are missing | Separate notification lifecycle from work tier and recover supported event writers |
| Release/sales | Shipment, production bands, fees, signed sales formula, reviews and recall branches recovered | Core loop, reprints, reviews, graph, milestones and recalls connected | Compare original full cycle; connect recovered consultations only once entry/cost lifecycle is known |
| Hardware | Availability transitions and license requirements; own hardware described officially | Initial licenses/computers work; live launches, retirement and own-hardware development are missing | Resolve weekly hardware counters and progression gates |
| Awards/company growth | Original event and contest forms plus office/tenant purchase branches exist | Awards, upgrades, complete ranking and tenant progression missing | Trace eligibility, charges and effects before exposing actions |
| Bank/bankruptcy | Principal amounts, one-year repayment block and negative-funds branch identified | Loans, repayment and complete company dissolution not connected | Recover complete branch ordering and state cleanup |
| Saves | Original sequential Write/Input operations identified | Modern atomic JSON works; original-save compatibility unsupported | Obtain/verify local original save fixtures and complete serialization order |
| Translation | Original strings and bitmap text are Japanese | 99 catalog labels translated; character readings and embedded image text still Japanese | Establish readings and use local-only artwork patches; no guessed identities |
| Audio | Original SFX imported locally; no music in the supplied distribution | Eight authorized optional community recordings added | Intentional modern option, separately controlled; no simulation bonus |
| RNG | VB6 shared Rnd sequence | Modern Random and partial draw scheduling | Original seed/draw-sequence compatibility remains unresolved |

## M32 correction: select the visible employee

The original left-click handler iterates workspace slots upwards and stops at
the first matching rectangle. Its hit test is inclusive:

`X + offsetX + 2 <= pointerX <= X + offsetX + 14`

`Y + offsetY + 2 <= pointerY <= Y + offsetY + 30`

Offsets correspond to the rendered seated pose; walking uses current positions.
Overlap selection follows slot order, not sprite depth or modern roster order.
`OfficeInteractionRules` implements this pure selection. The Godot office maps
its centered/scaled image coordinates back to original pixels and opens the
selected employee's existing management controls. Staff-table activation also
opens that row's controls. Blank pixels and unreconstructed right-click
assignment do not open this screen. Existing decision dialogs cannot be replaced
by clicking the office behind them. Selection itself changes no employee rules,
funds or save fields, and the management screen pauses simulation.

Original zoom/scroll state and right-click workspace assignment remain
unimplemented. Godot image-coordinate mapping and the focused management layout
are runtime integration, separate from the recovered original hit test.

## Corrected reverse-engineering interpretation

M30 called A14530 a work tier. The bounded renderer at
`00A0283C +C940..C986` instead copies it into A139F4 and clears the pending
field; it is a visual notification. The actual work tier is A138A4. Notification
4 selects the speed-related visual rectangle, so this write does not prove a
speed multiplier. The speed idle-state local also has a bypassed assignment;
its lifetime still requires tracing. See [technology-application.md](technology-application.md).

## Validation

Local direct compilation, 140 C# checks (138 public, two original fixtures),
all 27 Python checks (19 public, eight original fixtures), the imported-original
UI suite and the complete core-loop UI suite pass. Added coverage includes
inclusive hit-box edges, overlap order, moving/seated coordinates, unknown
workspaces, actual office input with image scaling/letterboxing, row activation,
unchanged funds/progression and management pause behavior. Standard SDK/export
validation is recorded in [validation.md](validation.md).

This audit is a work queue, not a completion claim. Unknown mechanics remain
explicitly unresolved; imported originals and private QA images are excluded
from the repository and downloadable builds.
