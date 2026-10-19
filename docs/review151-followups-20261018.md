# Review #151 follow-ups — 18 October 2026

This batch addresses [#152](https://github.com/UnSpotibleShadow/OpenGDS2/issues/152),
[#153](https://github.com/UnSpotibleShadow/OpenGDS2/issues/153) and
[#154](https://github.com/UnSpotibleShadow/OpenGDS2/issues/154), plus the owner's
application/taskbar icon and startup update-notice requests. Integration targets
`develop`. #151 remains the owner's acceptance gate; no version bump, release,
tag, main promotion or desktop package is included.

## Menus, audio and credits (#152)

The accepted Return to Title entry remains. Extra Actions and the extra
Information entries are removed. Information follows the comparison screenshot:
Staff, Products, Top 100, Platforms, Our consoles, Time records. Other windows
lists numbered child-window titles with the front window checked instead of a fixed
Employees/Products list. The original number choices are restored as the examples
`100万0000` and `1,000,000`, verified in the two serialized menu records at file
offsets 2005494 and 2005531 of the supported EXE.

Sound On/Off controls both audio channels. Independent effects/music switches
and a compact volume dialog remain available. Saving music preferences now
preserves the effects section of audio.cfg. About retains the original credit
and website button, and adds community-port attribution and a repository button.

## Racing names (#153)

The ticket body reports untranslated names; it does not report a further scenery
or camera defect. All 44 runner and ten jockey display names now have English
community translations/readings. The UI resolves a name only when its source
identity and saved name match the locally imported catalog. Japanese mode,
unknown identities, imported data and saved race names remain unchanged. These
are community readings, not claimed official English names.

## Office and development (#154)

Supported executable SHA-256:
`4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.

* `fMainVisual` MouseDown `009C66BC +093F..0B2E` supplies the five study-desk
  diamonds; `+0FF2..1055` supplies the rest-area request. Right-click assignment
  now handles those destinations as well as development desks, using the
  existing assignment, break-policy and route services.
* Initializer `009F651C +331D..34F9` binds study actions 1–5 to sprite IDs 45–49.
  Main loop `00A0283C +2374..246C` supplies their positions/source rectangles;
  `+2717..27AE` applies the model/task source offsets through member 2218.
  Occupied study PCs now project from those imported tiles, preserving depth.
* The PC-area panel in the screenshot is the company research page: owned
  technologies, a technology-purchase button and eligible genres. The serialized
  fGMInfo headings at file offsets 0x2A28CD/0x2A2922 and Command2 identify those
  controls. This remains a nonmodal company tool page, with live ownership/genre
  refresh. It is not an employee course picker. The office training-area hit
  (`009C66BC +06BD..071B`) opens that page. The dedicated employee training
  button is established separately by fGMInfo.Command6 (`0098232C`): actions
  1–5 open fGMWSTech, otherwise its message asks for a study workstation.
  The owner's subsequent screenshot clarification establishes the remaining
  presentation target: the narrow company technology/genre panel, not the
  employee course picker. Clicking either an occupied study PC or the surrounding
  research area now opens that same nonmodal panel. Its 172×305 window contains
  compact green/orange headings, two lists and Buy Technology between them.
  The overview Training button remains removed; the separate Staff training route
  still opens the existing course picker. PC clicks use the rendered rectangles,
  including portions outside the broad research-area diamond.
  Existing training/application costs and timing are unchanged.
* Planning's green summary contains hardware, audience and cost. Work and its
  green marker sit below the distribution. Bugs use a common red strip with a
  white number; development points are labelled Progress. Progress bars share
  the classic green fill/blue remainder treatment.
* Meeting Confirm sits above Cancel in the left command column. The 19 October
  follow-up removes the stale 86-pixel minimum from its 70-pixel slot and shares
  the compact Cancel styling, retaining the imported glyph. Both alpha and beta
  commands are checked against the adjacent quality region in English/Japanese. Completion
  metadata and quality have shared colored regions. Release opens secretary
  dialogue, then the compact media table, then shipment finalization. The new
  introduction is authored English/Japanese presentation text, not a claim to
  have recovered the exact original sentence. Opening the sequence spends no
  funds and produces no copies.
* Additional production uses the yellow/cyan bordered cost and receipts rows.
  Company overview no longer has the extra Facilities/ellipsis buttons;
  facilities remain accessible from their office plots.

## Application identity

Windows presets previously disabled PE resource modification, leaving the
engine executable's icon intact. They now enable the authored fallback ICO.
The main-build workflow installs hash-pinned rcedit 2.0.0 and its Wine runtime
for Linux-hosted Windows resource editing; the import step uses `--import`.
This workflow still packages only after an eligible develop-to-main merge.
No export was run for this batch.

At runtime, Windows also receives a local native ICO generated from the imported
original icon and a stable AppUserModelID. Original pixels stay in user data.
The generic authored fallback remains available before import. Godot's embedded
editor game host is a separate native window: its editor-controlled icon is not
proof of the standalone game's icon. Native Windows titlebar/taskbar and pinned
shortcut acceptance still require the owner's standalone run; a pinned shortcut
may retain the icon of its old executable.

## Startup release notice

A non-blocking GitHub releases-list request includes EA prereleases and excludes
drafts. Semantic comparison orders numeric identifiers correctly (ea.10 > ea.9)
and stable releases above prereleases. The request has an eight-second timeout,
a bounded response, a daily successful-response cache, an hourly retry limit and quiet failure handling.
The small notice links only to this repository's release pages, can be dismissed,
and records the notified version. The setting can be disabled from title or
in-game Settings. There is no downloader, installer or automatic restart.
Headless tests never make the request; authored cached metadata exercises the UI.

## Validation and remaining acceptance

Source checks cover version selection, original study hit regions and PC depth;
bilingual UI fixtures cover menus/audio/About, office study/rest interactions,
employee training charges, release ordering, compact bounds and notification persistence.
Existing casino, save continuation and main core-loop coverage remain applicable.
Exact check results are recorded in the PR. Original Windows execution and the
owner's final side-by-side visual acceptance remain unrun here. Original EXE,
artwork, dialogue dumps, screenshots and imported catalogs stay private.

## Owner re-review — 19 October 2026

#152 is accepted by the owner; #153 was accepted and closed. #154 remains open
only for the PC panel destination/presentation to be visually rechecked; the
meeting command column and other review items are accepted.
The owner accepts Consumer Investigation for #151 because it cannot be reliably
triggered in their play session. Treat this as a manual-test waiver; it adds no
new native execution evidence and changes no refund or receipt rules.

The follow-up passes a clean application build, 574 domain checks and bilingual
study-PC input, course charge/cancellation, icon-bearing alpha/beta command
bounds and management-layout checks. The source validation workflow runs these
UI regressions on the PR and integrated develop commit. The full local core-loop
check completes with zero failures and a clean exit. Confirm uses one action
handler; an initial duplicate no-op subscription was removed before final checks.

The later #154 comment (6262361440) confirms that opening the course picker from
an occupied PC was still the wrong destination. The current regression checks
explicitly reject TrainingWindow on PC clicks, require the two screenshot headings
and narrow bounds, and retain the panel/position across its purchase child and
close/reopen. Merely opening the panel cannot spend funds/experience or begin a
course. This supersedes PR156's direct-course shortcut interpretation.
