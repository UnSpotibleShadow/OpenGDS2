# OpenGDS2

## Support the project

OpenGDS2 is **free of charge**. If you would like to support development, you can do so through Ko-fi. Support is entirely optional.

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/B0P228889O)

**Open Game Dev Story II** is an unofficial community reconstruction of **Game Dev Story 2 DX**, the 2001 PC game, built with **Godot 4 and C#/.NET 8**.

Run a game studio: hire and train employees, develop games, ship them, follow reviews and sales, and reinvest in your company. OpenGDS2 brings the original game into a modern engine, with English and Japanese interfaces and modern save support.

## Early Access

**Early Access `v0.1.0-ea.2` is available.** Download the [Windows release](https://github.com/UnSpotibleShadow/OpenGDS2/releases/tag/v0.1.0-ea.2), with separate **32-bit (`x86_32`)** and **64-bit (`x86_64`)** packages and SHA-256 files.

The first release is intended for **community testing**. The core game loop is playable, but this is an unfinished reconstruction: expect bugs, presentation differences and missing systems. Automated checks and a Windows export are in place; native Windows playthrough validation remains incomplete.

Historical GitHub Actions development packages are outdated; use the published release assets. Source validation runs on pull requests; it does not publish a release.

### What you can play

- Hire, assign, train and manage employees; upgrade their development PCs.
- Plan original games, sequels and manga adaptations; choose types and genres.
- Develop, hold meetings, debug, cancel projects and ship games.
- Read reviews, track sales and sales history, and produce more copies.
- Take loans, participate in calendar events, exhibitions and awards, and use the implemented facilities, including mystery-lab experiments.
- Develop and launch your own console, then make games for its growing market.
- Buy casino coins and play poker, blackjack and card-chain, including their distinct double-up games. Cash-wager horse races are also playable; their original wager and discarded-winnings defects are preserved and disclosed.
- Save and resume supported workflows, with a previous-valid-save recovery option.
- Use English or Japanese text, read the translated original Help, and adjust music and audio settings.
- Follow facility-specific conversations, all six promotion methods, translated rival game names and the recovered award ceremony.

**Beginner mode** ends after Year 2, December, week 4. **Normal mode** supports experimental ongoing play. In either mode, negative funds trigger the recovered company-dissolution sequence and return to the title. Full original presentation and company progression are still under review.

### What is still missing

Original-game save importing remains outside the supported Early Access scope. Casino rules retain documented original quirks; horse results display winnings that the original next-race path discards. Some dialogue, original presentation, Japanese bitmap text and English name readings still need work. Modern saves are separate from the original game's saves.

Recent research has recovered more of the original systems, but recovered evidence does not mean those systems are implemented.

See the [known issues](docs/early-access-known-issues.md) and [scope and research decisions](docs/early-access-decisions.md) for details. Historical milestone documents describe the evidence and older testing decisions; the current release checklist is [here](docs/early-access-release.md).

## Player guide

Read the [player wiki](https://github.com/UnSpotibleShadow/OpenGDS2/wiki) for
installation, saves, development, technologies, facilities, sales, recalls and
troubleshooting. A [repository copy](docs/player-guide.md) is also available.

## Getting started

### Windows release package

Download the matching Windows ZIP from the [release page](https://github.com/UnSpotibleShadow/OpenGDS2/releases/tag/v0.1.0-ea.2), then:

1. Extract the **entire ZIP** into a fresh folder.
2. Run `OpenGDS2.console.exe`, keeping the accompanying files and folders together. The packaged build does not require the Godot editor or a separate .NET installation.
3. On the title screen, select **Import original assets** and choose your legally obtained `GameDev2DX.lzh`.
4. Start a new company and choose Beginner or Normal mode.

Original game: [Kairosoft’s official Game Dev Story 2 DX page](https://kairosoft.net/game/pc/gamedev2.html).

**The original LZH archive is required to play.** Original game files and imported assets are not bundled. The self-extracting `.exe` installer is not a supported import input; choose the `.lzh` archive instead. The title screen and settings work before importing.

Imports, settings and saves use the permanent **`OpenGDS2`** data folder,
independent of the version or Early Access label. On Windows this is
`%APPDATA%\OpenGDS2` (Linux: `$XDG_DATA_HOME/OpenGDS2` or
`~/.local/share/OpenGDS2`; macOS: `~/Library/Application Support/OpenGDS2`).

On the first launch, existing `OpenGDS2-EarlyAccessTesting` imports, preferences,
saves/backups, playlists and artwork overrides are copied automatically. Close
the old game first. The old folder is kept, existing destination files are never
overwritten, and conflicting save/backup pairs are kept together. If copying
fails, the game offers Retry before continuing. See [data migration](docs/stable-user-data.md)
for recovery and manual-transfer details.

### Your first game

Hire employees, assign them to development desks, then start game development in a free room. Choose the project's type and genre, develop and debug it, and ship when ready. Follow sales and use **Produce More Copies** to replenish stock. Send tired staff to the lounge.

Language, mute and volume controls are in **Settings**. Refresh the original import when package notes ask for it.

## Help test Early Access

Useful reports include the release version, Windows version, display resolution/scaling, reproduction steps, and what you expected versus what happened. Save/reload problems, crashes, duplicate charges and stuck workflows are especially useful to report.

Report bugs through [GitHub Issues](https://github.com/UnSpotibleShadow/OpenGDS2/issues). The [Windows testing checklist](docs/early-access-testing.md) is available as a guide for community testers, rather than a requirement that everyone complete every test.

Review logs before sharing them: they may contain names or local paths. **Do not upload the original archive, executables, graphics, databases or original saves.** Back up modern saves before trying a newer build.

## Build from source

Install **Godot 4.4.1 .NET** and the **.NET 8 SDK**:

```sh
dotnet build OpenGDS2.csproj
godot --path .
```

Alternatively, open `project.godot` in the .NET edition of Godot. Playing from source still requires the original LZH import.

See [building instructions](docs/building.md) for setup and exports. Development checks:

```sh
python3 -m unittest discover -s tests -p 'test_*.py'
dotnet run --project tests/DomainTests/DomainTests.csproj
```

The [development checkpoint](docs/development-checkpoint.md), [original-game comparison](docs/original-comparison.md) and [open issues](https://github.com/UnSpotibleShadow/OpenGDS2/issues) contain the technical history and outstanding work.

## License and original content

Community-written code and documentation use the [MIT License](LICENSE). Bundled community music has separate [distribution terms](audio/community/NOTICE.md); it is not covered by the code license.

OpenGDS2 is an unofficial community project. Original game files and imported data remain outside the repository and release packages. See [notice.md](notice.md) for third-party notices and exclusions.

## Development workflow

Changes use topic branches and reviewed PRs into `main`. Published packages are
identified by immutable version tags. See [branches, tags and releases](docs/release-workflow.md).
