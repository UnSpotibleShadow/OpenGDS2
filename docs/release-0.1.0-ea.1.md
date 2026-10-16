# Open Game Dev Story II — 0.1.0-ea.1 (Early Access)

The first Early Access release of OpenGDS2, an unofficial community reconstruction
of the original PC Game Dev Story 2 DX in Godot and C#.

Run your own studio: hire and train staff, develop games, hold meetings, debug,
ship and follow reviews and sales. Explore facilities, promotions, exhibitions,
awards, loans, casino games and own-console development. English and Japanese
interfaces are available, alongside modern saves and previous-save recovery.

Recent work restores facility-specific artwork and conversations, all six
promotion scenes, award dialogue, review speech bubbles and sales block charts.
English rival game names, the translated title artwork, original toolbar routes,
Company summary sections and more compact windows round out the presentation.

## Getting started

1. Extract the entire Windows x64 ZIP and run `OpenGDS2.exe` or
   `OpenGDS2.console.exe`. Keep all accompanying files together.
2. Import your legally obtained `GameDev2DX.lzh` from the title screen.
   The self-extracting EXE is not a supported import input.
3. Start a company. No Godot editor or separate .NET installation is needed
   for the self-contained Windows package.

**Original game files are required and are not bundled.**

## Saves and upgrading

Data stays in `%APPDATA%\OpenGDS2`, independent of version or release label.
Close older versions before launching. Previous `OpenGDS2-EarlyAccessTesting`
imports, settings and modern saves are copied once without overwriting existing
destination files. Back up saves before upgrading. Original-game saves cannot
be imported.

## Early Access limitations

Expect bugs, missing systems and presentation differences. Beginner ends after
Year 2, December, week 4; Normal is experimental ongoing play. Negative funds
trigger company dissolution. Full original campaign, visual and RNG parity are
not claimed. Some bitmap text and provisional name readings still need work.
Technologies are employee actions, not universal passive quality bonuses.
Casino modes retain documented original quirks. Full sustained, multi-year and
display testing remains community work.

See the included `KNOWN_ISSUES.md`, `TEST_CHECKLIST.md` and `DECISIONS.md`.
`BUILD_MANIFEST.json` identifies the actual source/runtime; `SHA256SUMS` lists
package file hashes. These records do not themselves prove Windows acceptance.

Report bugs at https://github.com/UnSpotibleShadow/OpenGDS2/issues with your
version/source commit, reproduction steps and expected/actual behavior.
Crashes, save errors and blocked progress are priorities. Never attach original
archives, executables, assets, databases or original saves.

Thank you to everyone helping test and compare this reconstruction with the
original. Code uses the MIT License; community music has separate terms.
See the included licenses and notices.
