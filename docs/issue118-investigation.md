# Issue #118: original-versus-port investigation

Investigated 15 October 2026 against merged PR #119
(`21c192b93a703b0ee83657d5bc9b89b5cbf66f21`).
[Ticket](https://github.com/UnSpotibleShadow/OpenGDS2/issues/118), last updated
17:15:16 UTC, contains 14 images. All 14 were downloaded and visually inspected
privately. Japanese screenshots are treated as the original, as the owner requests.
The original PE is the supported SHA-256 `4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.
This is an investigation, not completion of the ticket. No gameplay changes are
included. Native runtime timing/focus acceptance is still blocked by the host
Wine failure described in [batch verification](map-backlog-verification.md).

## Findings

| Report / images in ticket order | Confirmed finding | Work required |
| --- | --- | --- |
| Promotion, 1–2 | The secretary message is related: it reports the selected advertising method and awareness gain. The port instead embeds the generic planning-room image and result in one large panel. Its selector is an ItemList rather than the original method/fee columns. | Restore the compact two-column selector, separate medium-specific scene and secretary result. Retain the existing saved charge/result and callback state. |
| Unknown development event, 3 | A specialist-magazine interview, not a development checkpoint or a paid promotion. The source contains its trigger and four reporter pages; the current port has no interview workflow. | Implement the source-backed event with a persisted once-per-project flag, introduction/reporter/result cursor and original scene. |
| Office hover boxes | The current theme does not style TooltipPanel/TooltipLabel. Office mouse motion assigns a generic desk hint instead of choosing useful text for each object. The reported vending/training/room boxes cannot be made complete by changing font color alone. | Add an opaque, high-contrast tooltip theme and source-backed hit-specific office descriptions. Check the room, vending and training targets separately, including enlarged office graphics and screen edges. |
| Game Dex assistant, 4 | The original secretary reports the exhibited games' awareness increase while the booth window remains visible. The port stores the result but exposes it through the visitor label's tooltip; there is no corresponding secretary page. | Present the saved awareness result alongside the booth scene, with a saved acknowledgement so loading does not repeat visitor draws, fees or awareness mutations. |
| Completion, 5 | The correct planning-room artwork and most data already exist. The port uses a centered heading, different field groups/positions and reordered qualities. The original left list is fun/originality/graphics/music; the current summary lists fun/graphics/originality/music. | Reconcile the header strip, image placement, green metadata cells, quality order, separate size/awareness/bug region and two right-hand commands. Keep the existing debugging/release decisions. |
| Shipping, 6 | This is the initial `fGMSyukka` form, not additional production (`fGMSyukkaSai`). The port's 460×490 container, unruled two-column panels and stacked commands differ visibly from the compact original table and right-side commands. | Rebuild initial shipment with the ruled yellow/cyan table, separators, native field grouping and lower-right confirm/cancel pair. Preserve numeric range, one-click stepping, full-width values and media-information return drafts. Review additional production independently. |
| Sequel selection, 7–8 | The original has title/series/shipped-copy columns and a selected game's audience, genre/content and four quality values beside its hardware image. The port adds a review-total column and replaces those selected-game details with a general inheritance note. | Restore the original three columns and complete selected-predecessor panel. Keep eligibility, inheritance and suggested-title rules. Read the runtime resize/load handlers when choosing translated bounds; designer bounds alone are insufficient. |
| Manga selection, 9–10 | The original compact details expose title, popularity and rights fee on the left, author and publisher in cyan cells on the right. The port's lower panel contains only author/publisher and is much taller. Original manga selection is pink; current rows are green. | Restore the compact list/detail layout and screen-specific selection color. Retain the catalog IDs, translated names and rights fees. Do not apply a blanket pink selection style to promotion, whose reference selection is green. |
| Publisher conversation, 11 | The building artwork is already recovered by the generic resource importer as `ImageFormG[2]`; it is not wired to the manga discussion. The current conversation also supplies no representative portrait ID. | Give the image a semantic asset alias and pass it as the scene throughout the rights conversation. Recover the representative portrait binding; do not select an arbitrary employee. Preserve rights acceptance/decline and payment timing. |
| Facility action/closure flow, 12–13 | Image 12 specifically shows production expansion with the secretary, not closure. Image 13 is the lawyer's retailer-fee negotiation; the current five-page negotiation exists but lacks the separate business-meeting scene. Closure and several service confirmations/results still use generic headings/buttons instead of the original conversation plus small confirmation. | Trace purchase, action, removal and cancellation separately for each facility. Reuse confirmed accounting/outcomes while restoring the right scene, speakers, acknowledgement order and small confirmation. Closing a facility view and demolishing the facility must remain distinct actions. |
| Facility pricing, 14 | All nine construction prices agree with the original initializer. The seven visible rows also agree with the screenshot. No price correction is supported. | Retain the values below and verify their displayed grouping and type-to-row mapping during the presentation changes. |

## Recovered scene bindings

Radio and television are 256×152; the other scenes are 256×160. These originals already exist in the local generic resource import.
They need semantic scene use, rather than replacement artwork. Resource offsets
below point to file headers, not the surrounding serialized property header.

| Use | MDI ImageFormG index | Resource file offset |
| --- | ---: | --- |
| Magazine promotion | 6 | `0x0F0704` BMP |
| Radio promotion | 1 | `0x18151E` BMP |
| Television promotion | 8 | `0x0BEA30` BMP |
| Cinema promotion | 5 | `0x10E76E` BMP |
| Internet promotion | 9 | `0x0969C6` BMP |
| Newspaper promotion | 7 | `0x0C869A` BMP |
| Publisher rights discussion | 2 | `0x1634B4` BMP |
| Magazine interview / lawyer negotiation | 12 | `0x08FCD0` GIF |

Meeting form load `009A6DDC` selects promotion artwork at `+00DD..02BC`,
interview mode 7 at `+046D..04C0`, and publisher mode 16 at `+0773..07C9`.
The native screenshot and the recovered scene pixels agree. English patches
should cover embedded Japanese promotional text where necessary, using the
existing local patch mechanism; retain Japanese and external overrides.

## The missing magazine interview

`Module2 009E3830 +1803..1A39` handles this branch for software rooms.
Its conditions include recognition greater than 15, development stage at least 2,
and an unset per-room interview flag `A129C8`. The random condition is
`Int(Rnd × (1500 − recognition × 15) × A149B8) = 2`, where `A149B8` is the
existing native timing factor. Boolean evaluation eagerly consumes the draw;
replacing it with a short-circuit eligibility check would change RNG consumption.

The flag is set before the secretary introduction. Mode 7 is dispatched at
`+194C..1954`. After that call, the caller prepares a gain of 20, reduces it to
`100 − recognition` when necessary, queues the secretary result and adds that
gain. Preserve this exact cap expression, including unusual above-100 inputs.
The reporter sequence is `009EFA40 +2083..22CE` (pool `005FAFCC`, string
indices `A7..B2`). It introduces a subject-specific magazine, discusses the
project's content and promises coverage. This matches image 3.

The native save-field map contains member `19AC`, corresponding to the flag.
Proposal acceptance `0099BA80 +027D` and shipment `009C9CB8 +027A` reference
it too; finish tracing reset/copy semantics before implementing migration.
Current `DevelopmentProjectState` has no equivalent field. The implementation
must join the recovered company-pass order, not run an independent UI-frame
lottery. Persist target, reporter/name selection and all pages; preserve the
source operand order of the cosmetic portrait/name draws as well as the trigger.
Do not invent historical interview state for legacy campaigns.

## Construction and service prices

`FacilityService.Prices` agrees with all nine literals in
`009F651C +9963/+9999/+99CF/+9A05/+9A3B/+9A71/+9AA7/+9ADD/+9B13`.

| Facility | Construction cost |
| --- | ---: |
| Game school | ¥600,000,000 |
| Staff hot spring | ¥60,000,000 |
| Recycling shop | ¥80,000,000 |
| Technology school | ¥100,000,000 |
| Casino | ¥500,000,000 |
| Law office | ¥250,000,000 |
| Mystery laboratory | ¥300,000,000 |
| Production laboratory | ¥150,000,000 |
| Hardware laboratory | ¥1,000,000,000 |

Existing verified service amounts remain distinct: hot spring ¥8m, technology
study ¥40m, production expansion ¥10m, mystery experiment ¥25m, recycling
receipts ¥500 per copy. Lawyer entry checks ¥15m but does not debit it in the
recovered original; both fee-negotiation branches preserve that quirk. Facility
closure has no refund. The screenshot's production secretary amount is ¥10m,
which agrees with the current service constant.

## Implementation order and acceptance

1. **Scene aliases and shared presentation.** Wire the eight scene uses above,
   confirm representative portraits, add English patches where needed, and give
   tooltips an opaque readable style. Import/version compatibility must continue
   to recognize older caches and explain when reimport is required.
2. **Promotion and exhibition conversations.** Restore separate scene/secretary
   windows while retaining already prepared domain outcomes. Save/load during
   reveal and acknowledgement must not repeat payment, gains or random draws.
3. **Completion, shipping, sequel and manga forms.** Reconcile reference field
   groups, full selected-game details, ruled cells, colors, numeric widths and
   command placement. Test English/Japanese, long labels and five-digit quantities;
   preserve dragged positions and draft values through child forms.
4. **Facility conversations.** Trace each facility's action/closure/cancel flow,
   restore its speakers/scenes and confirmation order, then check each supported
   outcome. Retain the verified nine prices and the lawyer payment quirk.
5. **Magazine interview gameplay.** Complete reset/copy/save evidence and add the
   source-ordered trigger, saved conversation and once-only capped awareness gain.
   Exercise rejection, both rooms, eager draws, legacy unknowns and interruption.
6. **Review captures and regressions.** Provide private before/after captures for
   every ticket area, targeted bilingual UI checks and domain checks for changed
   transaction/cursor behavior. Native timing/focus acceptance remains separately
   pending until the original can run. Close #118 only after its fixes are reviewed.

## Investigation verification

All 14 attachment images and the recovered scene contact sheet were inspected.
`verify_facility_evidence.py`, `verify_lawyer_evidence.py` and
`verify_presentation_handler_evidence.py` pass against the pinned original.
The complete structural inventory independently binds the forms/resources and
procedure spans used above. Private images, signed download links, original
captions, full traces and imported artwork are excluded from this document/PR.
No application code changed, so the already-passed PR #119 app/domain/UI checks
were not repeated for this documentation-only investigation.
