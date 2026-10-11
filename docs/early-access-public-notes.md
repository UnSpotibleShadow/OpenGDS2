# OpenGDS2 @VERSION@ — Early Access

This is an experimental community-tested reconstruction of Game Dev Story 2 DX.
Expect bugs, missing systems and presentation differences. Full native Windows
acceptance is incomplete; the included checklist guides community testing.

## Playable scope

Hire and train employees, assign desks, upgrade PCs, develop original games,
sequels and manga adaptations, hold meetings, debug and ship. Follow reviews,
sales and reprints, calendar events, loans and the implemented facilities.
Mystery-lab experiments have seven source-backed outcomes and saved progress/results.
Develop and launch one own console, then use it for software planning, production
and sales while its market grows. Design/media and technology flags persist.
Modern saves include previous-valid-save recovery. English and Japanese
interfaces and audio controls are available.

Beginner ends after Year 2, December, week 4. Normal is experimental ongoing
play: bankruptcy and full company progression are incomplete, and negative
funds can persist. Casino gameplay and original-save
import are deferred. See KNOWN_ISSUES.md for the remaining boundaries.

This release includes the payroll-screen fix: visiting ordinary technology
purchase, training or application screens preserves an established payroll
snapshot rather than clearing it and potentially stopping month-end progress.
This does not complete every original payroll scratch writer.

## Start playing

Extract the entire ZIP and run OpenGDS2.exe or OpenGDS2.console.exe. No editor
or separate .NET installation is needed. Import your legally obtained
GameDev2DX.lzh from the title; the self-extracting installer is not supported
as an import input. Original game files are not bundled.

Data is stored in `%APPDATA%\@DATA_DIRECTORY@`. Start a fresh company for testing
and back up modern saves before upgrading. Original saves are unsupported.

## Report problems

Report bugs at: @REPORT_URL@

Include the version and source commit from BUILD_MANIFEST.json, Windows/display
settings, steps to reproduce, and expected/actual results. Crashes, save errors,
duplicate charges and stuck workflows are especially useful reports. Review
logs before posting. Never upload the LZH, original executables/assets/databases
or original saves. Share modern reproduction saves privately only when asked.

## Package information

BUILD_MANIFEST.json identifies the source, assembly digest and bundled runtime;
SHA256SUMS lists package file hashes. Those records establish package identity,
not native Windows playthrough acceptance. TEST_CHECKLIST.md records untested
rows as untested.

Code and documentation use the MIT License. Community music has separate
distribution terms. See LICENSE, notice.md and licenses/ for the code, engine,
runtime and music notices.
