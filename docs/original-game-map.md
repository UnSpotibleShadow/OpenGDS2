# Original-game map

This map starts from the supported original executable; OpenGDS2 is a separate comparison layer.
Stable IDs, hashes and relationships are in [the inventory](original-game-inventory.json); authored workflows, runtime scenarios and gaps are in [the catalog](original-game-workflows.json). Open [the standalone searchable view](original-game-map.html) locally to browse workflows, menus, event bindings, procedures and graph edges without a server or external assets.

Original SHA-256: `4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`. Comparison baseline: `a5abd1de0336ea67ba87aad200899d5dbf7472c0`.

## Coverage means

Every discovered item has an inventory entry, and every compiled object has a workflow disposition. This is **100% accounting of the enumerated sets**, not 100% game understanding or adaptation parity. The official archive is now inventoried and imported resources are checked separately. One MDI root encoding, opaque property meanings, dynamically dispatched targets and native runtime behavior remain open boundaries.

| Set | Accounted | Meaning |
| --- | ---: | --- |
| Compiled objects | 86 / 86 | 84 forms/designers, two modules |
| Metadata control definitions | 920 / 920 | Includes form and menu definitions; arrays are definitions |
| Bound event slots | 572 / 572 | Slot-to-procedure bindings; names not guessed |
| Valid procedure descriptors | 669 / 669 | 167,189 decoded instructions; semantics remain partial |
| Known serialized control instances | 2605 / 2605 | Array instances and decorative controls; excludes menus and form roots |
| Serialized menus | 55 / 55 | Parents recovered from persisted child markers and closing delimiters |
| Known picture properties | 537 / 537 | Includes repeated images and icons; not the full archive |
| Known serialized timers | 36 / 36 | Stored intervals/enabled values and event bindings; dynamic writers remain partial |
| Original runtime workflow acceptance | 0 / 35 | Original was not executed in this environment |

Decoded instruction coverage does not prove call-graph completeness. Pool addresses may denote imports, globals, objects or constants. Member offsets retain unknown receiver identity. Object references do not prove a window opens or is modal.

Recorded graph: **2,829** direct-call/object-reference edges; **18,591** operand-reference sites; **13,140** dynamic-dispatch candidate sites remain unresolved.

## Original menus

Names below are original technical identifiers. Indentation comes from serialization, not English translation or the current port menu. Shortcut/enabled/visible semantics need a further property pass.

Each entry states what is available in OpenGDS2 and where the player can find it. These labels describe access and implementation found in the code; original-game behavior and runtime parity remain unverified.

- **Available:** the action exists at the stated menu or button.
- **Available elsewhere:** the feature exists through the stated alternative path; the original direct menu item is missing.
- **Missing menu access:** related controls exist at the stated location, but the original in-game menu entry is missing. Any uncertainty about equivalent behavior is stated separately.
- **Missing option:** no equivalent player-selectable choice was found. Existing graphics or number rendering may still work without that choice.
- **Missing functionality:** the gameplay action/transaction is not implemented; there is no alternate player path.
- **Unresolved:** the evidence does not yet establish an equivalent action or its behavior; this is not counted as confirmed missing functionality.
- **Menu organization / Separator:** describes a container or divider, not a gameplay feature.

- Menu — `menu:mMenu`: Menu organization: OpenGDS2 uses Game for saving/loading and Back to Title; New Game and Quit are on the title screen. See the individual actions below.
  - New game — `menu:mMenuNewG`: Available: Game > New Game opens a draft without replacing the company until start; cancelling returns to the current company. The title-screen route also remains available. Exact native setup presentation is pending.
  - Separator — `menu:mMenuHaihun1`: Separator: decorative menu divider; no action or gameplay is expected.
  - Save game — `menu:mMenuSave`: Available: Game > Save Game opens the numbered save picker. OpenGDS2 saves JSON; native GD2 compatibility is missing.
  - Load game — `menu:mMenuLoad`: Available: Game > Load Game opens the numbered load picker. OpenGDS2 loads JSON; native GD2 compatibility is missing.
  - Separator — `menu:mMenuHaihun2`: Separator: decorative menu divider; no action or gameplay is expected.
  - Exit game — `menu:mMenuEnd`: Available: Game > Quit closes the application. Native unload/confirmation equivalence remains pending.
- Actions — `menu:mKoudou`: Menu organization: actions remain grouped under Company and Employees; three UP actions are also on the toolbar. Backup purchase and employee poaching are implemented below.
  - Company development — `menu:mKoudouKaisha`: Menu organization: Public relations, Genre research and Expand production exist under Company and as three UP toolbar buttons. The original nested Company development submenu is not reproduced.
    - Public relations — `menu:mKoudouTimeido`: Available: Company > Public relations, or the Public relations UP toolbar button, opens the activity confirmation.
    - Genre development — `menu:mKoudouLibary`: Available: Company > Genre research, or the Genre research UP toolbar button, opens the activity confirmation.
    - Production expansion — `menu:mKoudouZoukyou`: Available: Company > Expand production, or the Expand production UP toolbar button, opens the activity confirmation.
  - Separator — `menu:mKoudouMinasu1`: Separator: decorative menu divider; no action or gameplay is expected.
  - Recruit employees — `menu:mKoudouSShinki`: Available: Employees > Hire Employees, or the recruitment toolbar button, opens recruitment.
  - Poach employees — `menu:mKoudouSHikinuku`: Available: Employees > Poach employee opens the source-eligible target roster and timed negotiation. Automatic defensive offers, donations, contract changes and saved outcomes are implemented; native runtime comparison remains pending.
  - Separator — `menu:mKoudouMinasu3`: Separator: decorative menu divider; no action or gameplay is expected.
  - Secretary settings — `menu:mKoudouHisyoS`: Available: Company > Secretary policies opens the secretary settings window.
  - Change secretary — `menu:mKoudouHisyoK`: Available: Company > Hire secretary, or the secretary toolbar button, opens secretary hiring/change.
  - Separator — `menu:mKoudouMinasu2`: Separator: decorative menu divider; no action or gameplay is expected.
  - Visit bank — `menu:mKoudouBank`: Available: Company > Bank, or the bank toolbar button, opens the bank window.
  - Buy development computer — `menu:mKoudouPCK`: Available: Company > Development Computers, or the computer toolbar button, opens computer purchasing.
  - Buy backup device — `menu:mKoudouBackupK`: Available: Company > Buy backup device implements the one-time gameplay purchase and virus-protection flag. Modern save-file .bak recovery is a separate feature.
  - Acquire hardware license — `menu:mKoudouLicense`: Available: Company > Hardware Licenses, or the license toolbar button, opens license purchasing.
  - Buy technology — `menu:mKoudouTechK`: Available: Company > Technologies, or the technology toolbar button, opens technology purchasing.
  - Go out — `menu:mKoudouGo`: Available: Company > Go out contains the original three direct game choices. The casino facility route also remains available. Existing ownership/coins/modal guards are retained; exact native menu gating remains pending.
    - Go horse racing — `menu:mKoudouGoKeiba`: Available: Company > Go out > Horse racing, or the casino facility selector. Existing catalog/funds/ownership/modal guards still apply.
    - Play poker — `menu:mKoudouGoPoka`: Available: Company > Go out > Poker, or the casino facility selector. Existing ownership/coin/modal guards still apply.
    - Play blackjack — `menu:mKoudouGoBlack`: Available: Company > Go out > Blackjack, or the casino facility selector. Existing ownership/coin/modal guards still apply.
- Information — `menu:mInfo`: Menu organization: the corresponding reports are under Employees, Products, Company and Help. The original Information menu grouping is not reproduced.
  - All staff information — `menu:mInfoGMsStaffAll`: Available: Employees > Staff, or the staff toolbar button, opens the implemented staff roster. The original handler uses local dispatch whose target is unresolved in the direct-call graph; exact native equivalence remains unverified.
  - Own released games — `menu:mInfoGMsZishaGame`: Available: Products > Sales, or the products toolbar button, opens the implemented own-product list. Exact original local-dispatch behavior remains unverified.
  - Top 100 cumulative sales — `menu:mInfoGMsTusan100`: Available: Help > Sales rankings, or the sales-ranking toolbar button, opens the implemented ranking report. Exact original local-dispatch behavior remains unverified.
  - Currently released hardware — `menu:mInfoGMHatubaiHard`: Available: Company > Hardware market opens the released-hardware report.
  - Separator — `menu:mInfoGMMinasu`: Separator: decorative menu divider; no action or gameplay is expected.
  - Own released hardware — `menu:mInfoGMHatubaiZHard`: Available: Products > Own hardware opens the own-hardware report when its state is available.
  - Separator — `menu:mInfoGMMinasu2`: Separator: decorative menu divider; no action or gameplay is expected.
  - Reach-time rankings — `menu:mInfoGMToutatu`: Available: Help > Global rankings opens the reach-time records view.
- Windows — `menu:mWindow`: Menu organization: Windows exposes Office, Company, Employees and Products. That grouping differs from the original Visual, Tool and Other windows entries.
  - Visual window — `menu:mWindowVisual`: Available: Windows > Office reopens the office window. Exact original show/hide behavior remains unverified.
  - Tool window — `menu:mWindowTool`: Available: Windows > Company reopens the company window, the comparison for original fGMInfo. Exact original page/tool-window behavior remains unverified.
  - Other windows — `menu:mWindowOther`: Unresolved: the original entry has no bound handler; dynamic window-list behavior has not been established. OpenGDS2 has explicit Windows entries, but they are not a confirmed equivalent.
- Settings — `menu:mSetup`: Available: in-game Settings exposes the original Sound, Graphic enlargement and Number display submenus. Title-screen Settings retains community music and language preferences.
  - Sound — `menu:mSetupSound`: Available: Settings > Sound controls native effects independently of community music, matching the recovered native dispatcher guard.
    - Sound on — `menu:mSetupSoundOn`: Available: Settings > Sound > On enables original sound effects; community music keeps its separate setting.
    - Sound off — `menu:mSetupSoundOff`: Available: Settings > Sound > Off disables original sound effects; community music keeps its separate setting.
  - Graphic enlargement — `menu:mSetupGraphic`: Available: Settings > Graphic enlargement selects Normal/Enlarged office graphics. The saved choice uses native-size pixels or 2x pixels with a bounded scroll viewport.
    - Normal size — `menu:mSetupGraphicD`: Available: Settings > Graphic enlargement > Normal restores native office resolution and clears the visible scroll position.
    - Enlarged size — `menu:mSetupGraphicK`: Available: Settings > Graphic enlargement > Enlarged doubles office pixels and preserves office input-coordinate mapping. Native panning/focus acceptance remains pending.
  - Number display — `menu:mSetupNumber`: Available: Settings > Number display selects Japanese or comma-separated thousands grouping across the UI; the choice is stored in company saves.
    - Japanese large-number grouping — `menu:mSetupNumber1`: Available: Settings > Number display > Japanese grouping uses four-digit groups with 万/億/兆 and zero-filled lower groups. Modern Int64 values can additionally use 京.
    - Thousands grouping — `menu:mSetupNumber2`: Available: Settings > Number display > Thousands grouping uses comma-separated three-digit groups independently of interface language.
- Help — `menu:mHelp`: Menu organization: Help > Game help opens the implemented topic viewer. The original Version information/About action is unresolved separately below.
  - Help topics — `menu:mHelpHelp`: Available: Help > Game help, or the help toolbar button, opens the five-topic original Help viewer.
  - Separator — `menu:mHelpMinasu1`: Separator: decorative menu divider; no action or gameplay is expected.
  - Version information — `menu:mHelpVersion`: Unresolved: the original opens fMainAbout. An equivalent interactive About/version/link dialog has not been established in OpenGDS2; there is no confirmed alternate player path for that dialog.

## Workflow and adaptation index

All rows retain partial understanding and pending original runtime acceptance. Comparison files and automated tests establish where to investigate; they do not certify native parity.

| Stable ID | Original objects | Adaptation disposition | Evidence |
| --- | --- | --- | --- |
| W01: Start, numbered save/load and start gate | `fGMGameNew`, `fGMGameSave`, `fGMGameLoad`, `fGMGameKaishi` | intentional-difference | [catalogs-and-original-saves](catalogs-and-original-saves.md) · [source-save-integrity](source-save-integrity.md) · [remaining-presentation-m105-step6](remaining-presentation-m105-step6.md) |
| W02: Desktop, toolbar and office input | `MDIForm1`, `fMainVisual` | partial | [native-toolbar-20261015](native-toolbar-20261015.md) · [office-clicks-m39](office-clicks-m39.md) · [office-presentation-m94](office-presentation-m94.md) · [map-backlog-implementation](map-backlog-implementation.md) · [issue118-implementation](issue118-implementation.md) |
| W03: Company information and employee detail pages | `fGMInfo` | comparison-present | [management-presentation-m105-step3](management-presentation-m105-step3.md) · [window-census-m98](window-census-m98.md) |
| W04: Portrait conversations and callback return | `fGMKaiwa`, `fGMKaiwa2`, `fGMKaiwaC` | partial | [checkpoint-speakers-m92](checkpoint-speakers-m92.md) · [secretary-advice](secretary-advice.md) · [workflow-verification-m76](workflow-verification-m76.md) · [issue118-implementation](issue118-implementation.md) |
| W05: Staff roster, recruitment and annual contracts | `fGMSyain`, `fGMSKoyou`, `fGMSKoyouKz`, `fGMKeiyakuS` | comparison-present | [management-presentation-m105-step3](management-presentation-m105-step3.md) · [presentation-handler-audit-m93](presentation-handler-audit-m93.md) |
| W06: Secretary hiring, policy and action choices | `fGMHisyoK`, `fGMHisyoSet`, `fGMKoudou` | comparison-present | [secretary-management](secretary-management.md) · [secretary-initialization](secretary-initialization.md) |
| W07: Technology purchase, training, use and level-up | `fGMTechKau`, `fGMWSTech`, `fGMLevelUP` | comparison-present | [review112-help-technology-audit](review112-help-technology-audit.md) · [training-and-technologies](training-and-technologies.md) · [technology-application](technology-application.md) · [speed-technology-m91](speed-technology-m91.md) · [stress-relief-m90](stress-relief-m90.md) |
| W08: Interactive advanced-programming challenge | `fGMKoudo` | partial | [programming-challenge-m99](programming-challenge-m99.md) · [presentation-completion-m105](presentation-completion-m105.md) |
| W09: Planning, checkpoints, completion and magazine interviews | `fGMKaihatu`, `fGMKaihatuS`, `fGMKikakusyo`, `fGMKikakuKaigi`, `fGMalpha`, `fGMbeta`, `fGMKansei` | comparison-present | [development-planning](development-planning.md) · [checkpoint-choices-m57](checkpoint-choices-m57.md) · [development-panels-m95](development-panels-m95.md) · [issue118-implementation](issue118-implementation.md) |
| W10: Manga content selection | `fGMManga` | comparison-present | [development-planning](development-planning.md) · [management-presentation-m105-step3](management-presentation-m105-step3.md) · [issue118-implementation](issue118-implementation.md) |
| W11: Hall-qualified sequel planning | `fGMZokuhen` | comparison-present | [sequel-development](sequel-development.md) · [review-event-presentation-m96](review-event-presentation-m96.md) · [issue118-implementation](issue118-implementation.md) |
| W12: Both development-room live reports | `fGMDevW1`, `fGMDevW2` | comparison-present | [playthrough-presentation-m103](playthrough-presentation-m103.md) · [office-presentation-m94](office-presentation-m94.md) |
| W13: Initial shipment, reproduction and media information | `fGMSyukka`, `fGMSyukkaSai`, `fGMMedia` | comparison-present | [presentation-handler-audit-m93](presentation-handler-audit-m93.md) · [playthrough-presentation-m103](playthrough-presentation-m103.md) · [development-panels-m95](development-panels-m95.md) · [issue118-implementation](issue118-implementation.md) |
| W14: Magazine reviews, Hall and illustrated milestones | `fGMCrossR`, `fGMCrossRDen`, `fGMOuri` | comparison-present | [review-event-presentation-m96](review-event-presentation-m96.md) · [programming-challenge-m99](programming-challenge-m99.md) · [remaining-presentation-m105-step6](remaining-presentation-m105-step6.md) |
| W15: Product list, compact selling report and sales history | `fGMZisha`, `fGMZishaK`, `fGMZishaKW` | comparison-present | [development-panels-m95](development-panels-m95.md) · [playthrough-presentation-m103](playthrough-presentation-m103.md) · [review112-help-technology-audit](review112-help-technology-audit.md) |
| W16: Game exhibition selection and attendance | `fGMGameDexSn`, `fGMGameDexKs` | comparison-present | [annual-calendar-events](annual-calendar-events.md) · [review-event-presentation-m96](review-event-presentation-m96.md) · [playthrough-presentation-m103](playthrough-presentation-m103.md) · [issue118-implementation](issue118-implementation.md) |
| W17: Annual awards and conversation-driven announcement | `fGMKontesuto`, `fGMKontesutoHp` | comparison-present | [awards-presentation-m102](awards-presentation-m102.md) · [review-round-implementation-m104](review-round-implementation-m104.md) · [annual-calendar-events](annual-calendar-events.md) |
| W18: Computers, hardware licenses, market and launch card | `fGMComK`, `fGMHLicense`, `fGMGameHardInfo`, `fGMHardHatubai` | comparison-present | [hardware-presentation-m105-step4](hardware-presentation-m105-step4.md) · [catalogs-and-original-saves](catalogs-and-original-saves.md) |
| W19: Three company-development UP actions | `fGMSeisanZ` | comparison-present | [native-toolbar-20261015](native-toolbar-20261015.md) · [review112-help-technology-audit](review112-help-technology-audit.md) |
| W20: Facilities, tenants and facility-specific dialogs | `fGMTenaSel`, `fGMTenantOnsen`, `fGMTenantGameS`, `fGMTenantReS`, `fGMTenantTechS`, `fGMTenantTechSel`, `fGMTenantBengo`, `fGMTenantSeisan`, `fGMTenantNazoP` | comparison-present | [facility-artwork-m61](facility-artwork-m61.md) · [lawyer-facility-m71](lawyer-facility-m71.md) · [management-presentation-m105-step3](management-presentation-m105-step3.md) · [issue118-implementation](issue118-implementation.md) |
| W21: Own-hardware development, launch and report | `fGMTenantHard`, `fGMTenantHardS`, `fGMDevH`, `fGMSyukkaH`, `fGMZishaH` | comparison-present | [own-hardware-m83](own-hardware-m83.md) · [hardware-presentation-m105-step4](hardware-presentation-m105-step4.md) |
| W22: Casino entry, game selector and coin purchase | `fGMTenantCasino`, `fGMTenantCasinoSn` | comparison-present | [casino-investigation-m77](casino-investigation-m77.md) · [review112-help-technology-audit](review112-help-technology-audit.md) |
| W23: Poker, holds, payouts and double-up | `fCasinoPoker` | comparison-present | [casino-poker-m85](casino-poker-m85.md) · [casino-fidelity-m78](casino-fidelity-m78.md) |
| W24: Blackjack and shared double-up | `fCasinoBJ` | comparison-present | [casino-blackjack-m86](casino-blackjack-m86.md) · [casino-fidelity-m78](casino-fidelity-m78.md) |
| W25: Card-chain, keyboard and exhaustion boundary | `fCasinoCC` | comparison-present | [casino-card-chain-m87](casino-card-chain-m87.md) · [casino-runtime-boundaries-m79](casino-runtime-boundaries-m79.md) |
| W26: Horse racing, wagers, race and refund | `fKeibaMain`, `fKeibaModosi` | partial | [horse-catalog-m88](horse-catalog-m88.md) · [casino-horse-racing-m89](casino-horse-racing-m89.md) · [casino-presentation-m105-step5](casino-presentation-m105-step5.md) |
| W27: Bank loan and repayment | `fGMBank` | comparison-present | [bank-loans-m58](bank-loans-m58.md) · [financial-failure-m84](financial-failure-m84.md) |
| W28: Original Help viewer | `fGMHelp` | comparison-present | [review112-help-technology-audit](review112-help-technology-audit.md) · [remaining-presentation-m105-step6](remaining-presentation-m105-step6.md) |
| W29: Live sales rankings and reach-time records | `fGMRank100`, `fGMToutatuRank` | comparison-present | [school-rivals-and-rankings](school-rivals-and-rankings.md) · [remaining-presentation-m105-step6](remaining-presentation-m105-step6.md) |
| W30: Poaching and employment offers | `fGMHikinuki`, `fGMHikinukiSel`, `fGMHikinukiKeS`, `fGMHikinukiKnS` | comparison-present | [map-backlog-implementation](map-backlog-implementation.md) · [window-census-m98](window-census-m98.md) · [remaining-presentation-m105-step6](remaining-presentation-m105-step6.md) |
| W31: Reachable opponent-software pane with dormant data | `fGMZishaNet` | unresolved | [window-census-m98](window-census-m98.md) |
| W32: Utility, about and unclassified helper forms | `MainForm16`, `MainForm3`, `fMainAbout` | unresolved | [window-census-m98](window-census-m98.md) · [presentation-evidence-m105](presentation-evidence-m105.md) |
| W33: Initialization, simulation, calendars and failure | `Module1`, `Module2` | partial | [static-recovery-2026-10-10](static-recovery-2026-10-10.md) · [weekly-economics-investigation](weekly-economics-investigation.md) · [calendar-transactions](calendar-transactions.md) · [financial-failure-m84](financial-failure-m84.md) · [workflow-verification-m76](workflow-verification-m76.md) · [issue118-implementation](issue118-implementation.md) |
| W34: Paid room promotion and media selection | `fGMSendenBaitai` | comparison-present | [room-promotion-m97](room-promotion-m97.md) · [presentation-handler-audit-m93](presentation-handler-audit-m93.md) · [issue118-implementation](issue118-implementation.md) |
| W35: Backup device purchase and virus-event protection | `MDIForm1`, `Module2` | comparison-present | [map-backlog-implementation](map-backlog-implementation.md) · [original-backup-device-map](original-backup-device-map.md) |

## Original timer inventory

Values are serialized requests in milliseconds, not observed delivery cadence. `unknown/default` means no explicit Enabled value was recovered; it does not mean disabled. An unbound timer is retained rather than assigned an invented function. Runtime writes can change these values.

| Original control | Interval | Explicit Enabled | Bound procedures |
| --- | ---: | --- | --- |
| `MDIForm1.Timer1` | 75 | 0 | none bound |
| `MDIForm1.Timer2` | 100 | 0 | `procedure:0x0098CD98` |
| `fGMKikakuKaigi.Timer1` | 100 | unknown/default | `procedure:0x009EFA40` |
| `fGMKontesutoHp.Timer1` | 75 | unknown/default | `procedure:0x009BD6F8` |
| `fGMGameDexKs.Timer1` | 100 | unknown/default | `procedure:0x0099AB50` |
| `fGMKaiwa2.Timer1` | 20 | unknown/default | `procedure:0x00998F30` |
| `fGMKaiwaC.Timer1` | 10 | unknown/default | `procedure:0x009B20EC` |
| `fGMZishaH.Timer1` | 100 | unknown/default | `procedure:0x0098CB60` |
| `fGMGameHardInfo.Timer1` | 100 | unknown/default | `procedure:0x00999D04` |
| `fGMKoudo.Timer1` | 80 | unknown/default | `procedure:0x0098F68C` |
| `fGMZishaNet.Timer1` | 100 | unknown/default | `procedure:0x0099C4EC` |
| `fGMLevelUP.Timer1` | 100 | unknown/default | `procedure:0x009B375C` |
| `fGMKontesuto.Timer1` | 75 | unknown/default | `procedure:0x009CEE10` |
| `fGMKansei.Timer1` | 100 | unknown/default | `procedure:0x0099B554` |
| `fGMKaiwa.Timer1` | 20 | unknown/default | `procedure:0x00998AB0` |
| `fGMDevH.Timer1` | 50 | unknown/default | `procedure:0x0099BFB0` |
| `fGMRank100.Timer1` | 100 | unknown/default | `procedure:0x00995924` |
| `fGMZisha.Timer1` | 100 | unknown/default | `procedure:0x009A369C` |
| `fGMZishaK.Timer1` | 100 | unknown/default | none bound |
| `fGMSyukkaSai.Timer1` | 80 | unknown/default | `procedure:0x00997D6C` |
| `fGMZishaKW.Timer1` | 100 | unknown/default | `procedure:0x009C4570` |
| `fGMSyukka.Timer1` | 80 | unknown/default | `procedure:0x009A4D78` |
| `fGMSyain.Timer1` | 100 | unknown/default | `procedure:0x009B0B60` |
| `fGMHikinuki.Timer1` | 100 | unknown/default | `procedure:0x009B7570` |
| `fGMDevW2.Timer1` | 50 | unknown/default | `procedure:0x009AE2A4` |
| `fGMDevW1.Timer1` | 50 | unknown/default | `procedure:0x009AECB0` |
| `fGMInfo.Timer1` | 100 | unknown/default | `procedure:0x009DF294` |
| `fCasinoPoker.Timer1` | 30 | unknown/default | `procedure:0x009D0374` |
| `fCasinoBJ.Timer1` | 50 | unknown/default | `procedure:0x009D7BB8` |
| `fCasinoCC.Timer1` | 40 | unknown/default | `procedure:0x009E13B8` |
| `fGMCrossRDen.Timer1` | 100 | unknown/default | `procedure:0x00982814` |
| `fGMCrossR.Timer1` | 100 | unknown/default | `procedure:0x009CAFCC` |
| `fKeibaMain.Timer3` | 100 | 0 | `procedure:0x009A12F8` |
| `fKeibaMain.Timer2` | 80 | 0 | `procedure:0x009EB860` |
| `fKeibaMain.Timer1` | 100 | unknown/default | `procedure:0x009E6148` |
| `fMainVisual.Timer1` | 30 | 0 | `procedure:0x00976EE8` |

## Crosscutting map and next evidence

### X01: Menu, toolbar, hotspots and keyboard

Original menu hierarchy is freshly recovered from serialized records and metadata bindings. Toolbar/office events are inventoried, but numeric event slots do not by themselves identify Click/KeyDown meanings.

Next: Trace each bound event, array-index switch and keyboard branch; record the current player path separately from missing menu access, missing options, missing functionality and unresolved original behavior.

### X02: Screens, layouts, buttons and window lifetime

All 2605 known control property streams and all 84 roots are structurally bounded. The MDI root omits the normal Form-class byte and begins directly with ClientLeft. Opaque object/reserved marker meanings remain unresolved; there are 537 picture properties.

Next: Resolve opaque marker meanings and unsupported encodings outside the known inventory; runtime values/assignments and keyboard/focus still require comparison.

### X03: Timelines, automatic triggers and ordering

Module1/Module2 and all known serialized timers are inventoried. Direct P-code thunks and object-reference sites are indexed; dynamic calls and state aliases are not a complete scheduler graph.

Next: Label callback slots, resolve timer enable/interval writers and caller continuation; observe actor/weekly/monthly/yearly order, stalls, simultaneous events and window drag.

### X04: Globals, members, arrays and native RNG

Operand references remain indexed with unqualified member receivers kept explicit. The new RNG map resolves 310 native random draw sites and one startup seed site. This is a callsite inventory, not whole-session draw order.

Next: Classify remaining receiver aliases and executed RNG paths; compare native seeds and paired saved sessions.

### X05: Persistence and migration

All 130 native read/write record templates now pair their 448 field slots and array ranks, covering 447 distinct members and 40 loops. Modern numbered JSON slots work; native GD2 interoperability remains unsupported.

Next: Validate Variant subtypes, loop cardinalities, modern meanings and native encoding against genuine GD2 fixtures before enabling compatibility.

### X06: Artwork, icons, patches and external files

The official LZH has 45 files and three directories, with lengths and CRC16s verified. All 422 BMP/GIF property imports match bytes or decoded pixels. There are 114 ICO properties and one CUR; 82 root-icon exports are checked separately. Exhaustive dynamic image use remains unverified. The separate PE resource tree is also inventoried: one icon, one icon group and one version payload.

Next: Resolve the remaining control ICO/CUR and dynamic assignment uses; pair rendered screens with the original runtime.

### X07: Original audio and music

Five original WAVs are recovered. Existing native effect callsites remain wired; in-game Sound On/Off now controls the source effect dispatcher independently of community music. Open.wav has no established trigger; Bakuhatu.wav has a dispatcher branch but no recovered caller.

Next: Keep unused/unproven triggers explicit; validate audio timing and replacement on the original runtime rather than assigning invented cues.

### X08: Tables, formulas and importer coverage

Existing importers recover catalog families from the initialization procedure at 009F651C; metadata/procedure/reference inventory starts from the original, not the current catalog model. Raw members retain unknown semantics.

Next: Reconcile every initialization assignment/table/helper with a schema or explicit unknown; compare source row counts/types with importers and model fields. Existing imported rows are not a denominator for all original tables.

### X09: Archive and original runtime acceptance

The official archive and exact supported PE are recovered. A disposable original installation with VB6 runtime is prepared, but Wine11 exits with host SIGSEGV before a usable game window; a bounded strace confirms the failure. All 245 paired workflow scenarios remain pending.

Next: Use a working original Windows runtime for the 245 comparisons, native focus/timing acceptance and genuine GD2 fixtures. The failed Wine run is not an acceptance pass.

## Catalog and audio source anchors

These are identified families and their current comparison/import paths. Their row counts are recovered subsets, not a denominator for every original table.

| ID | Family | Source initializer | Comparison paths |
| --- | --- | --- | --- |
| T01 | 60 employee records and progression/catalog fields | `procedure:0x009F651C` | [EmployeeDatabaseImporter.cs](../src/Import/EmployeeDatabaseImporter.cs) |
| T02 | 18 genres, seven audiences, 51 game types and type compatibility rows | `procedure:0x009F651C` | [OriginalCatalogImporter.cs](../src/Import/OriginalCatalogImporter.cs) |
| T03 | Four development computers and eleven third-party hardware definitions/media bands | `procedure:0x009F651C` | [OriginalCatalogImporter.cs](../src/Import/OriginalCatalogImporter.cs) |
| T04 | Eight technologies with three-level training/experience/raw tables | `procedure:0x009F651C` | [OriginalCatalogImporter.cs](../src/Import/OriginalCatalogImporter.cs) |
| T05 | Five secretaries and 31 manga rights records | `procedure:0x009F651C` | [OriginalCatalogImporter.cs](../src/Import/OriginalCatalogImporter.cs) |
| T06 | 44 horse runners and ten jockeys | `procedure:0x009F651C` | [HorseCatalogImporter.cs](../src/Import/HorseCatalogImporter.cs) |
| T07 | 30 initial rival products | `procedure:0x009F651C` | [CompetitionCatalogImporter.cs](../src/Import/CompetitionCatalogImporter.cs) |
| T08 | 35 staged rival product launch definitions | `procedure:0x009B5BE0` | [CompetitionCatalogImporter.cs](../src/Import/CompetitionCatalogImporter.cs) |
| T09 | Ten rival company names from the explicit owner-name switch | `procedure:0x0098548C` | [CompetitionCatalogImporter.cs](../src/Import/CompetitionCatalogImporter.cs) |

Audio selector/trigger anchors: [original_audio_evidence.json](../tools/original_audio_evidence.json). Native ordered save records: [native_save_evidence.json](../tools/native_save_evidence.json). Both retain narrower claims than complete audio or persistence mapping.

## Prioritized gap queue

These are evidence tasks or known adaptation differences, not authorization to invent or implement new gameplay.

| Priority | ID | Work remaining |
| --- | --- | --- |
| P0 | G01 | Official archive inventory/import verification is complete. Original runtime acceptance remains blocked on this host by Wine SIGSEGV; execute all 245 paired scenarios on working Windows before claiming complete coverage. |
| P1 | G02 | Poaching is implemented and regression-tested. Compare the four native forms, timer delivery/retained controls, automatic offers and the unresolved empty-roster scratch boundary on the original runtime. |
| P1 | G03 | Missing backup/poaching/direct-game access and sound/graphics/number options are restored. Remaining original menu grouping, exact enable/visibility rules, About and utility access still need reconciliation. |
| P1 | G04 | Classify dynamic dispatch targets, timer interval/enable writers, scheduler order and actor/callback persistence; direct-call graph alone is incomplete. |
| P1 | G05 | All 130 native record templates have paired member/rank references (448 field slots); 310 RNG draw sites and one seed site are mapped. Finish field meanings/types/cardinalities and genuine GD2 cross-load tests; compare whole-session native seed/draw ordering. |
| P1 | G11 | Backup purchase, source ownership/reset/persistence, virus outcomes, office animation and saved continuations are implemented and tested. Paired original runtime acceptance remains pending. |
| P2 | G06 | All 2605 control streams and all 84 roots are structurally bounded. Resolve meanings of opaque zero-object/reserved 98/99 markers and investigate unsupported encodings outside the known inventory; consumed bytes are not understood behaviour. |
| P2 | G07 | All 45 archive files and 422 imported raster properties are verified, with 82 root icons. Resolve remaining control ICO/CUR usage, dynamic assignments and unproven audio triggers; imported files are not exhaustive handler coverage. |
| P2 | G08 | Graduate/renewal artwork aliases are corrected to fGMKeiyakuS/fGMSKoyouKz. Native runtime presentation comparison remains pending. |
| P2 | G09 | Resolve utility forms and fGMZishaNet data initialization/writers before exposing guessed behavior. |
| P2 | G10 | Verify all presented screens, selections, button glyphs, graph blocks, bilingual patches, window retention, focus and drag-time suspension on the original Windows runtime. |

## Reproduce and query

With the supported original executable kept outside the checkout:

```sh
python3 tools/map_original_game.py --exe /private/path/GameDev2DX.exe --check
python3 -m unittest discover -s tests -p 'test_original_game_map.py'
python3 tools/map_original_game.py --query fGMDevW1
```

`--check` rebuilds metadata, menu hierarchy, procedure decoding and reference graph; rechecks every known serialized boundary/hash; and compares the deterministic inventory/report with the committed files. No target code executes. `--write` deliberately refreshes generated files after reviewed structure/catalog edits. Without `--exe`, checks validate catalog accounting and links only. `--query` returns one object's controls, bindings, procedures, resources, callers and workflow IDs.

The structural anchors are build-specific recovered boundaries. Hash verification does not improve a partial property parse. Current known limits:

- The official LZH is now fingerprinted: 45 files and 3 directories with all file CRC16s verified. See docs/original-archive-inventory.json; no archive contents are bundled.
- All 2605 known control property streams and all 84 form roots are structurally bounded. The MDI root has an implicit header beginning with ClientLeft. Zero object markers and reserved 98/99 properties retain unresolved meanings. There are 537 known picture properties, including the additionally recovered office icon.
- Menus are independently recovered as class-19 records. Other unsupported serialized control classes/encodings are not proven absent.
- Procedure decoding covers descriptor bodies with alignment tails reported. An instruction decode is not a decompilation, proof of reachability, or complete recovery of computed calls.
- Dynamic dispatch, event semantic names, receiver identity for member offsets and aliasing require further analysis. Pool reference sites are not all globals.
- Window modality, focus, z-order, timer delivery, drag suspension, native fonts and retained-frame lifetime need Windows runtime evidence.
- No whole-session native RNG equivalence, native GD2 compatibility, exhaustive image-to-handler binding, or complete audio-trigger parity is claimed.
- The original distribution/runtime copy was recovered, but Wine 11 exits with a host SIGSEGV before a usable game window. All 245 original-versus-port acceptance scenarios remain pending; no successful native runtime pass is claimed.

## Runtime acceptance protocol

Execute the catalog's scenarios against a disposable copy of the original, recording executable/archive hashes, scenario ID, starting save hash, actions, observed ordering, captures and state deltas. Pair each recording with the adaptation on an equivalent setup; record differences explicitly. Test normal, cancel, unavailable/failure, repeated input, automatic interruption, save/load and timing paths. A scenario is only accepted after its evidence is reviewed; static fingerprints and a modern smoke test cannot accept it.

Original executables, archives, extracted strings, images/audio, saves, captures and full traces stay private. Public records contain only authored descriptions and technical metadata. Keep presentation issue #95 open.
