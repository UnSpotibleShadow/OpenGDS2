# OpenGDS2

An unofficial community reconstruction of **Game Dev Story 2 DX**, the 2001 PC game, built with **Godot 4 and C#/.NET 8**.

Run a game studio: hire and train employees, develop games, ship them, follow reviews and sales, and reinvest in your company. OpenGDS2 brings the original game into a modern engine, with English and Japanese interfaces and modern save support.

## Early Access

**Target release: Friday, 16 October 2026, at 19:00 CEST.** This is a release target, not an available download announcement. The current private candidate is `0.1.0-ea.1-rc.4`.

The first release is intended for **community testing**. The core game loop is playable, but this is an unfinished reconstruction: expect bugs, presentation differences and missing systems. Automated checks and a Windows export are in place; native Windows playthrough validation remains incomplete.

Release downloads will be announced here when available. Historical GitHub Actions development packages are outdated; automatic builds are currently paused.

### What you can play

- Hire, assign, train and manage employees; upgrade their development PCs.
- Plan original games, sequels and manga adaptations; choose types and genres.
- Develop, hold meetings, debug, cancel projects and ship games.
- Read reviews, track sales and sales history, and produce more copies.
- Take loans, participate in calendar events, exhibitions and awards, and use the implemented facilities, including mystery-lab experiments.
- Save and resume supported workflows, with a previous-valid-save recovery option.
- Use English or Japanese text and adjust music and audio settings.

**Beginner mode** ends after Year 2, December, week 4. **Normal mode** supports experimental ongoing play; bankruptcy and full company/office progression are incomplete, and negative funds can persist.

### What is still missing

Casino gameplay, own-hardware development and original-game save importing are outside the first supported Early Access scope. Some dialogue, original presentation, Japanese bitmap text and English name readings still need work. Modern saves are separate from the original game's saves.

Recent research has recovered more of the original systems, but recovered evidence does not mean those systems are implemented.

See the [known issues](docs/early-access-known-issues.md) and [scope and research decisions](docs/early-access-decisions.md) for details. Older documents may still describe the previous owner-testing release gates; the current plan is a community-tested Early Access release.

## Getting started

### Windows release package

Once a release is available:

1. Extract the **entire ZIP** into a fresh folder.
2. Run `OpenGDS2.console.exe`, keeping the accompanying files and folders together. The packaged build does not require the Godot editor or a separate .NET installation.
3. On the title screen, select **Import original assets** and choose your legally obtained `GameDev2DX.lzh`.
4. Start a new company and choose Beginner or Normal mode.

**The original LZH archive is required to play.** Original game files and imported assets are not bundled. The self-extracting `.exe` installer is not a supported import input; choose the `.lzh` archive instead. The title screen and settings work before importing.

The current RC.4 candidate keeps imports, settings and saves in `%APPDATA%\OpenGDS2-EarlyAccessTesting`, separate from ordinary OpenGDS2 data. Follow the final release's package notes for its data location and save compatibility.

### Your first game

Hire employees, assign them to development desks, then start game development in a free room. Choose the project's type and genre, develop and debug it, and ship when ready. Follow sales and use **Produce More Copies** to replenish stock. Send tired staff to the lounge.

Language, mute and volume controls are in **Settings**. Refresh the original import when package notes ask for it.

## Help test Early Access

Useful reports include the release version, Windows version, display resolution/scaling, reproduction steps, and what you expected versus what happened. Save/reload problems, crashes, duplicate charges and stuck workflows are especially useful to report.

Report bugs through [GitHub Issues](https://github.com/UnSpotibleShadow/OpenGDS2/issues) once the repository becomes public before release. The [Windows testing checklist](docs/early-access-testing.md) is available as a guide for community testers, rather than a requirement that everyone complete every test.

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

The [development checkpoint](docs/development-checkpoint.md), [original-game comparison](docs/original-comparison.md) and [continuation backlog](https://github.com/UnSpotibleShadow/OpenGDS2/issues/55) contain the technical history and outstanding work.

## Support the project

OpenGDS2 is **free of charge**. If you would like to support development, you can do so through Ko-fi. Support is entirely optional.

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/B0P228889O)

## License and original content

Community-written code and documentation use the [MIT License](LICENSE). Bundled community music has separate [distribution terms](audio/community/NOTICE.md); it is not covered by the code license.

OpenGDS2 is an unofficial community project. Original game files and imported data remain outside the repository and release packages. See [notice.md](notice.md) for third-party notices and exclusions.
