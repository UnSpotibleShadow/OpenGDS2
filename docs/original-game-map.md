# Original-game map

This map starts from the supported original executable; OpenGDS2 is a separate comparison layer.
Stable IDs, hashes and relationships are in [the inventory](original-game-inventory.json); authored workflows, runtime scenarios and gaps are in [the catalog](original-game-workflows.json). Open [the standalone searchable view](original-game-map.html) locally to browse workflows, menus, event bindings, procedures and graph edges without a server or external assets.

Original SHA-256: `4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`. Comparison baseline: `2eb7c0bf7de32b33b76e0bb1dc80840f406188e1`.

## Coverage means

Every discovered item has an inventory entry, and every compiled object has a workflow disposition. This is **100% accounting of the enumerated sets**, not 100% game understanding or adaptation parity. The complete archive, all VB property encodings, dynamically dispatched targets and native runtime behavior are open boundaries.

| Set | Accounted | Meaning |
| --- | ---: | --- |
| Compiled objects | 86 / 86 | 84 forms/designers, two modules |
| Metadata control definitions | 920 / 920 | Includes form and menu definitions; arrays are definitions |
| Bound event slots | 572 / 572 | Slot-to-procedure bindings; names not guessed |
| Valid procedure descriptors | 669 / 669 | 167,189 decoded instructions; semantics remain partial |
| Known serialized control instances | 2605 / 2605 | Array instances and decorative controls; excludes menus and form roots |
| Serialized menus | 55 / 55 | Parents recovered from persisted child markers and closing delimiters |
| Known picture properties | 536 / 536 | Includes repeated images and icons; not the full archive |
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
  - New game — `menu:mMenuNewG`: Available elsewhere: title screen > New Game. During play, use Game > Back to Title first; the original direct New Game menu item is missing. Equivalent reset/confirmation behavior remains unverified.
  - Separator — `menu:mMenuHaihun1`: Separator: decorative menu divider; no action or gameplay is expected.
  - Save game — `menu:mMenuSave`: Available: Game > Save Game opens the numbered save picker. OpenGDS2 saves JSON; native GD2 compatibility is missing.
  - Load game — `menu:mMenuLoad`: Available: Game > Load Game opens the numbered load picker. OpenGDS2 loads JSON; native GD2 compatibility is missing.
  - Separator — `menu:mMenuHaihun2`: Separator: decorative menu divider; no action or gameplay is expected.
  - Exit game — `menu:mMenuEnd`: Available elsewhere: title screen > Quit closes the application; Game > Back to Title returns there during play. The original direct Exit Game menu item and its confirmation flow are not reproduced.
- Actions — `menu:mKoudou`: Menu organization: actions are distributed across Company and Employees, the three UP toolbar buttons and casino-facility controls. This does not mean every original action exists; poaching and backup-device purchase are missing below.
  - Company development — `menu:mKoudouKaisha`: Menu organization: Public relations, Genre research and Expand production exist under Company and as three UP toolbar buttons. The original nested Company development submenu is not reproduced.
    - Public relations — `menu:mKoudouTimeido`: Available: Company > Public relations, or the Public relations UP toolbar button, opens the activity confirmation.
    - Genre development — `menu:mKoudouLibary`: Available: Company > Genre research, or the Genre research UP toolbar button, opens the activity confirmation.
    - Production expansion — `menu:mKoudouZoukyou`: Available: Company > Expand production, or the Expand production UP toolbar button, opens the activity confirmation.
  - Separator — `menu:mKoudouMinasu1`: Separator: decorative menu divider; no action or gameplay is expected.
  - Recruit employees — `menu:mKoudouSShinki`: Available: Employees > Hire Employees, or the recruitment toolbar button, opens recruitment.
  - Poach employees — `menu:mKoudouSHikinuku`: Missing functionality: employee-poaching offers, negotiation/contract transactions and the menu action are not implemented. There is no alternate player path.
  - Separator — `menu:mKoudouMinasu3`: Separator: decorative menu divider; no action or gameplay is expected.
  - Secretary settings — `menu:mKoudouHisyoS`: Available: Company > Secretary policies opens the secretary settings window.
  - Change secretary — `menu:mKoudouHisyoK`: Available: Company > Hire secretary, or the secretary toolbar button, opens secretary hiring/change.
  - Separator — `menu:mKoudouMinasu2`: Separator: decorative menu divider; no action or gameplay is expected.
  - Visit bank — `menu:mKoudouBank`: Available: Company > Bank, or the bank toolbar button, opens the bank window.
  - Buy development computer — `menu:mKoudouPCK`: Available: Company > Development Computers, or the computer toolbar button, opens computer purchasing.
  - Buy backup device — `menu:mKoudouBackupK`: Missing functionality: the purchasable gameplay backup device and its virus-protection ownership/transaction are not implemented. There is no alternate player path; modern save-file backups are a separate feature.
  - Acquire hardware license — `menu:mKoudouLicense`: Available: Company > Hardware Licenses, or the license toolbar button, opens license purchasing.
  - Buy technology — `menu:mKoudouTechK`: Available: Company > Technologies, or the technology toolbar button, opens technology purchasing.
  - Go out — `menu:mKoudouGo`: Available elsewhere: open an existing casino through Company facilities or its office tenant, then choose a game. The original Actions > Go out submenu is missing; its original visibility/eligibility rules remain unverified.
    - Go horse racing — `menu:mKoudouGoKeiba`: Available elsewhere: casino facility > Casino > Horse racing opens the implemented horse-racing game. The original direct Go horse racing menu item is missing. Catalog, funds and pending-decision guards can disable entry.
    - Play poker — `menu:mKoudouGoPoka`: Available elsewhere: casino facility > Casino > Poker opens the implemented poker game. The original direct Play poker menu item is missing. Coins and pending-decision guards can disable entry.
    - Play blackjack — `menu:mKoudouGoBlack`: Available elsewhere: casino facility > Casino > Blackjack opens the implemented blackjack game. The original direct Play blackjack menu item is missing. Coins and pending-decision guards can disable entry.
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
- Settings — `menu:mSetup`: Missing menu access: there is no in-game Settings menu. Title screen > Settings contains language, sound-effect and music controls. The original graphic-size and number-grouping choices are missing options below.
  - Sound — `menu:mSetupSound`: Missing menu access: there is no in-game Settings > Sound submenu. Title screen > Settings provides separate sound-effect and music toggles/volumes; equivalence to the original single sound setting remains unverified.
    - Sound on — `menu:mSetupSoundOn`: Missing menu access: enable sound effects and/or music through title screen > Settings. The original in-game Sound > On command is missing; whether it controlled the same sounds remains unverified.
    - Sound off — `menu:mSetupSoundOff`: Missing menu access: disable sound effects and/or music through title screen > Settings. The original in-game Sound > Off command is missing; whether it muted the same sounds remains unverified.
  - Graphic enlargement — `menu:mSetupGraphic`: Missing option: no Normal/Enlarged office-graphics selector was found in OpenGDS2. The office is rendered, but the original selectable enlargement behavior has no confirmed equivalent.
    - Normal size — `menu:mSetupGraphicD`: Missing option: there is no player-selectable Normal office-graphics command. Existing default office rendering is not proof of equivalent original sizing; the source references fMainVisual.
    - Enlarged size — `menu:mSetupGraphicK`: Missing option: there is no player-selectable Enlarged office-graphics command or confirmed alternate enlargement control. The source references fMainVisual; exact original scaling remains unverified.
  - Number display — `menu:mSetupNumber`: Missing option: there is no player-selectable number-grouping mode. OpenGDS2 already renders numbers with N0 formatting, but does not expose the original two-format choice.
    - Japanese large-number grouping — `menu:mSetupNumber1`: Missing option: no selector for Japanese large-number grouping was found. Existing number rendering and interface-language selection do not implement this original format choice.
    - Thousands grouping — `menu:mSetupNumber2`: Missing option: no selector for thousands grouping was found. Current N0-formatted numbers may already use thousands separators, but the player cannot choose this original display mode independently.
- Help — `menu:mHelp`: Menu organization: Help > Game help opens the implemented topic viewer. The original Version information/About action is unresolved separately below.
  - Help topics — `menu:mHelpHelp`: Available: Help > Game help, or the help toolbar button, opens the five-topic original Help viewer.
  - Separator — `menu:mHelpMinasu1`: Separator: decorative menu divider; no action or gameplay is expected.
  - Version information — `menu:mHelpVersion`: Unresolved: the original opens fMainAbout. An equivalent interactive About/version/link dialog has not been established in OpenGDS2; there is no confirmed alternate player path for that dialog.

## Workflow and adaptation index

All rows retain partial understanding and pending original runtime acceptance. Comparison files and automated tests establish where to investigate; they do not certify native parity.

| Stable ID | Original objects | Adaptation disposition | Evidence |
| --- | --- | --- | --- |
| W01: Start, numbered save/load and start gate | `fGMGameNew`, `fGMGameSave`, `fGMGameLoad`, `fGMGameKaishi` | intentional-difference | [catalogs-and-original-saves](catalogs-and-original-saves.md) · [source-save-integrity](source-save-integrity.md) · [remaining-presentation-m105-step6](remaining-presentation-m105-step6.md) |
| W02: Desktop, toolbar and office input | `MDIForm1`, `fMainVisual` | partial | [native-toolbar-20261015](native-toolbar-20261015.md) · [office-clicks-m39](office-clicks-m39.md) · [office-presentation-m94](office-presentation-m94.md) |
| W03: Company information and employee detail pages | `fGMInfo` | comparison-present | [management-presentation-m105-step3](management-presentation-m105-step3.md) · [window-census-m98](window-census-m98.md) |
| W04: Portrait conversations and callback return | `fGMKaiwa`, `fGMKaiwa2`, `fGMKaiwaC` | partial | [checkpoint-speakers-m92](checkpoint-speakers-m92.md) · [secretary-advice](secretary-advice.md) · [workflow-verification-m76](workflow-verification-m76.md) |
| W05: Staff roster, recruitment and annual contracts | `fGMSyain`, `fGMSKoyou`, `fGMSKoyouKz`, `fGMKeiyakuS` | comparison-present | [management-presentation-m105-step3](management-presentation-m105-step3.md) · [presentation-handler-audit-m93](presentation-handler-audit-m93.md) |
| W06: Secretary hiring, policy and action choices | `fGMHisyoK`, `fGMHisyoSet`, `fGMKoudou` | comparison-present | [secretary-management](secretary-management.md) · [secretary-initialization](secretary-initialization.md) |
| W07: Technology purchase, training, use and level-up | `fGMTechKau`, `fGMWSTech`, `fGMLevelUP` | comparison-present | [review112-help-technology-audit](review112-help-technology-audit.md) · [training-and-technologies](training-and-technologies.md) · [technology-application](technology-application.md) · [speed-technology-m91](speed-technology-m91.md) · [stress-relief-m90](stress-relief-m90.md) |
| W08: Interactive advanced-programming challenge | `fGMKoudo` | partial | [programming-challenge-m99](programming-challenge-m99.md) · [presentation-completion-m105](presentation-completion-m105.md) |
| W09: Planning, proposal, alpha/beta and completion | `fGMKaihatu`, `fGMKaihatuS`, `fGMKikakusyo`, `fGMKikakuKaigi`, `fGMalpha`, `fGMbeta`, `fGMKansei` | comparison-present | [development-planning](development-planning.md) · [checkpoint-choices-m57](checkpoint-choices-m57.md) · [development-panels-m95](development-panels-m95.md) |
| W10: Manga content selection | `fGMManga` | comparison-present | [development-planning](development-planning.md) · [management-presentation-m105-step3](management-presentation-m105-step3.md) |
| W11: Hall-qualified sequel planning | `fGMZokuhen` | comparison-present | [sequel-development](sequel-development.md) · [review-event-presentation-m96](review-event-presentation-m96.md) |
| W12: Both development-room live reports | `fGMDevW1`, `fGMDevW2` | comparison-present | [playthrough-presentation-m103](playthrough-presentation-m103.md) · [office-presentation-m94](office-presentation-m94.md) |
| W13: Initial shipment, reproduction and media information | `fGMSyukka`, `fGMSyukkaSai`, `fGMMedia` | comparison-present | [presentation-handler-audit-m93](presentation-handler-audit-m93.md) · [playthrough-presentation-m103](playthrough-presentation-m103.md) · [development-panels-m95](development-panels-m95.md) |
| W14: Magazine reviews, Hall and illustrated milestones | `fGMCrossR`, `fGMCrossRDen`, `fGMOuri` | comparison-present | [review-event-presentation-m96](review-event-presentation-m96.md) · [programming-challenge-m99](programming-challenge-m99.md) · [remaining-presentation-m105-step6](remaining-presentation-m105-step6.md) |
| W15: Product list, compact selling report and sales history | `fGMZisha`, `fGMZishaK`, `fGMZishaKW` | comparison-present | [development-panels-m95](development-panels-m95.md) · [playthrough-presentation-m103](playthrough-presentation-m103.md) · [review112-help-technology-audit](review112-help-technology-audit.md) |
| W16: Game exhibition selection and attendance | `fGMGameDexSn`, `fGMGameDexKs` | comparison-present | [annual-calendar-events](annual-calendar-events.md) · [review-event-presentation-m96](review-event-presentation-m96.md) · [playthrough-presentation-m103](playthrough-presentation-m103.md) |
| W17: Annual awards and conversation-driven announcement | `fGMKontesuto`, `fGMKontesutoHp` | comparison-present | [awards-presentation-m102](awards-presentation-m102.md) · [review-round-implementation-m104](review-round-implementation-m104.md) · [annual-calendar-events](annual-calendar-events.md) |
| W18: Computers, hardware licenses, market and launch card | `fGMComK`, `fGMHLicense`, `fGMGameHardInfo`, `fGMHardHatubai` | comparison-present | [hardware-presentation-m105-step4](hardware-presentation-m105-step4.md) · [catalogs-and-original-saves](catalogs-and-original-saves.md) |
| W19: Three company-development UP actions | `fGMSeisanZ` | comparison-present | [native-toolbar-20261015](native-toolbar-20261015.md) · [review112-help-technology-audit](review112-help-technology-audit.md) |
| W20: Facilities, tenants and facility-specific dialogs | `fGMTenaSel`, `fGMTenantOnsen`, `fGMTenantGameS`, `fGMTenantReS`, `fGMTenantTechS`, `fGMTenantTechSel`, `fGMTenantBengo`, `fGMTenantSeisan`, `fGMTenantNazoP` | comparison-present | [facility-artwork-m61](facility-artwork-m61.md) · [lawyer-facility-m71](lawyer-facility-m71.md) · [management-presentation-m105-step3](management-presentation-m105-step3.md) |
| W21: Own-hardware development, launch and report | `fGMTenantHard`, `fGMTenantHardS`, `fGMDevH`, `fGMSyukkaH`, `fGMZishaH` | comparison-present | [own-hardware-m83](own-hardware-m83.md) · [hardware-presentation-m105-step4](hardware-presentation-m105-step4.md) |
| W22: Casino entry, game selector and coin purchase | `fGMTenantCasino`, `fGMTenantCasinoSn` | comparison-present | [casino-investigation-m77](casino-investigation-m77.md) · [review112-help-technology-audit](review112-help-technology-audit.md) |
| W23: Poker, holds, payouts and double-up | `fCasinoPoker` | comparison-present | [casino-poker-m85](casino-poker-m85.md) · [casino-fidelity-m78](casino-fidelity-m78.md) |
| W24: Blackjack and shared double-up | `fCasinoBJ` | comparison-present | [casino-blackjack-m86](casino-blackjack-m86.md) · [casino-fidelity-m78](casino-fidelity-m78.md) |
| W25: Card-chain, keyboard and exhaustion boundary | `fCasinoCC` | comparison-present | [casino-card-chain-m87](casino-card-chain-m87.md) · [casino-runtime-boundaries-m79](casino-runtime-boundaries-m79.md) |
| W26: Horse racing, wagers, race and refund | `fKeibaMain`, `fKeibaModosi` | partial | [horse-catalog-m88](horse-catalog-m88.md) · [casino-horse-racing-m89](casino-horse-racing-m89.md) · [casino-presentation-m105-step5](casino-presentation-m105-step5.md) |
| W27: Bank loan and repayment | `fGMBank` | comparison-present | [bank-loans-m58](bank-loans-m58.md) · [financial-failure-m84](financial-failure-m84.md) |
| W28: Original Help viewer | `fGMHelp` | comparison-present | [review112-help-technology-audit](review112-help-technology-audit.md) · [remaining-presentation-m105-step6](remaining-presentation-m105-step6.md) |
| W29: Live sales rankings and reach-time records | `fGMRank100`, `fGMToutatuRank` | comparison-present | [school-rivals-and-rankings](school-rivals-and-rankings.md) · [remaining-presentation-m105-step6](remaining-presentation-m105-step6.md) |
| W30: Poaching and employment offers | `fGMHikinuki`, `fGMHikinukiSel`, `fGMHikinukiKeS`, `fGMHikinukiKnS` | missing | [window-census-m98](window-census-m98.md) · [remaining-presentation-m105-step6](remaining-presentation-m105-step6.md) |
| W31: Reachable opponent-software pane with dormant data | `fGMZishaNet` | unresolved | [window-census-m98](window-census-m98.md) |
| W32: Utility, about and unclassified helper forms | `MainForm16`, `MainForm3`, `fMainAbout` | unresolved | [window-census-m98](window-census-m98.md) · [presentation-evidence-m105](presentation-evidence-m105.md) |
| W33: Initialization, simulation, calendars and failure | `Module1`, `Module2` | partial | [static-recovery-2026-10-10](static-recovery-2026-10-10.md) · [weekly-economics-investigation](weekly-economics-investigation.md) · [calendar-transactions](calendar-transactions.md) · [financial-failure-m84](financial-failure-m84.md) · [workflow-verification-m76](workflow-verification-m76.md) |
| W34: Paid room promotion and media selection | `fGMSendenBaitai` | comparison-present | [room-promotion-m97](room-promotion-m97.md) · [presentation-handler-audit-m93](presentation-handler-audit-m93.md) |
| W35: Backup device purchase and virus-event protection | `MDIForm1`, `Module2` | missing | [original-backup-device-map](original-backup-device-map.md) |

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

All 84 forms have known layout-region anchors. Known non-menu instances and picture-property hashes are bound to owners; two root and 228 instance property streams are partial.

Next: Resolve stopped property streams; distinguish dynamic controls/default properties; verify dimensions, fonts, tooltip/focus, dialog owner/modality and drag/reopen/selection position on Windows.

### X03: Timelines, automatic triggers and ordering

Module1/Module2 and all known serialized timers are inventoried. Direct P-code thunks and object-reference sites are indexed; dynamic calls and state aliases are not a complete scheduler graph.

Next: Label callback slots, resolve timer enable/interval writers and caller continuation; observe actor/weekly/monthly/yearly order, stalls, simultaneous events and window drag.

### X04: Globals, members, arrays and native RNG

Operand reference sites are mechanically indexed. Pool addresses retain unclassified roles; member offsets retain unknown receiver identity. Existing local RNG evidence does not establish whole-session draw ordering.

Next: Classify pool cells and receiver-qualified members; map readers/writers, aliases and seeds/draw ordering using startup and saved checkpoints.

### X05: Persistence and migration

Native save/load spans 009D9824/009D60E0 are fingerprinted and 669 procedures are indexed. Modern numbered JSON slots and migrations are implemented; native GD2 interoperability remains unsupported.

Next: Complete ordered native field/type/array map and transient/nonpersistent cells; obtain genuine GD2 fixtures and cross-load/save comparisons before claiming compatibility.

### X06: Artwork, icons, patches and external files

Known property payloads bind 400 BMP, 22 GIF, 113 ICO and one CUR occurrence. Repetition counts are not distinct artwork counts. Source-handler use and dynamic picture assignment remain partial; no fresh LZH denominator exists.

Next: Supply original archive; inventory every entry/hash/type; map all picture writers and use-sites, sprite subdivisions/masks, English patch regions and per-window icon/button identity.

### X07: Original audio and music

The evidence manifest identifies four asynchronous single-voice effect selectors and known call sites; selector 3 has no call in the known dispatcher references. Community/meeting/seasonal music paths are a separate comparison. Current sound-effect and music controls are available through title screen > Settings; the original in-game Sound On/Off menu entries are missing access points, not evidence that audio toggling is unimplemented.

Next: Inventory archive audio and every direct/indirect playback site; establish selector/context, interruption/overlap/mute/loop/stop and resume on the original. Do not create a trigger for an unused selector.

### X08: Tables, formulas and importer coverage

Existing importers recover catalog families from the initialization procedure at 009F651C; metadata/procedure/reference inventory starts from the original, not the current catalog model. Raw members retain unknown semantics.

Next: Reconcile every initialization assignment/table/helper with a schema or explicit unknown; compare source row counts/types with importers and model fields. Existing imported rows are not a denominator for all original tables.

### X09: Archive and original runtime acceptance

Original PE is available and freshly hash-verified. A private Wine 11 binary exists, but no complete original distribution or established runnable game installation is available. Native Windows acceptance and all 245 paired workflow scenarios remain pending.

Next: Use a disposable Windows install with the matching original distribution. Record original archive/runtime versions and execute catalog scenarios with captures/state checkpoints, then compare the adaptation.

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
| P0 | G01 | Obtain original archive and runtime evidence; complete external-file denominator and pair 245 scenario recordings. This blocks a claim of complete game coverage. |
| P1 | G02 | Recover the four-form poaching transaction and its periodic/menu callbacks; adaptation workflow is missing. |
| P1 | G03 | Reconcile original menu/toolbar/hotspot/keyboard access with the concrete paths listed above. Casino games and title-screen audio controls exist elsewhere; direct casino and in-game Sound entries are missing. Graphic-size/number-grouping selectors are missing options. Backup purchase and poaching are missing gameplay; unresolved behavior must remain separate. |
| P1 | G04 | Classify dynamic dispatch targets, timer interval/enable writers, scheduler order and actor/callback persistence; direct-call graph alone is incomplete. |
| P1 | G05 | Complete native GD2 field/type/array mapping and whole-session RNG seed/draw-order comparisons as separate compatibility projects. |
| P1 | G11 | Implement the source-backed backup-device purchase/ownership and corresponding virus protection only after tracing event mutations/persistence; this is absent from the adaptation and distinct from JSON backup files. |
| P2 | G06 | Decode two stopped root and 228 stopped control-property streams; determine whether unsupported classes/streams contain additional controls or resources. |
| P2 | G07 | Reconcile every original artwork/audio/table use, including archive-only race sheets and dynamic assignments. 536 known picture properties do not prove complete resource coverage. |
| P2 | G08 | Reconcile NativeWindowArtwork graduate/renewal aliases with the source forms; icon association does not establish workflow identity. |
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

- The original LZH is absent from this workspace. External graphics, audio and other archive entries have no complete fresh archive denominator; cache contents are not a substitute.
- Known serialized anchors comprise 84 layout regions, 2,605 non-menu control instances and 536 picture properties. Two root property parses and 228 control property parses stop early. Unknown property encodings may contain further data.
- Menus are independently recovered as class-19 records. Other unsupported serialized control classes/encodings are not proven absent.
- Procedure decoding covers descriptor bodies with alignment tails reported. An instruction decode is not a decompilation, proof of reachability, or complete recovery of computed calls.
- Dynamic dispatch, event semantic names, receiver identity for member offsets and aliasing require further analysis. Pool reference sites are not all globals.
- Window modality, focus, z-order, timer delivery, drag suspension, native fonts and retained-frame lifetime need Windows runtime evidence.
- No whole-session native RNG equivalence, native GD2 compatibility, exhaustive image-to-handler binding, or complete audio-trigger parity is claimed.

## Runtime acceptance protocol

Execute the catalog's scenarios against a disposable copy of the original, recording executable/archive hashes, scenario ID, starting save hash, actions, observed ordering, captures and state deltas. Pair each recording with the adaptation on an equivalent setup; record differences explicitly. Test normal, cancel, unavailable/failure, repeated input, automatic interruption, save/load and timing paths. A scenario is only accepted after its evidence is reviewed; static fingerprints and a modern smoke test cannot accept it.

Original executables, archives, extracted strings, images/audio, saves, captures and full traces stay private. Public records contain only authored descriptions and technical metadata. Keep presentation issue #95 open.
