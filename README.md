# OpenGDS2

Community modernization/reconstruction of the 2001 PC Game Dev Story 2 DX,
targeting **Godot 4 + C#/.NET 8**. M6 is the baseline; this continuation includes M7–M18.

The own-game **development → release → sales** loop is playable. Staff develop
projects at their assigned desks; you select a type and genre, debug or release,
then manage inventory and further production. Hiring, employee inspection,
manual level-up, dismissal and modern save/load are also available. Release reviews and sales
milestone announcements are implemented. Awards, office progression and complete
weekly economics remain unfinished.

M8 adds verified March-end contract renewal rules and local importing for seven
audience definitions. See [M8 evidence and limitations](docs/milestone-8.md).
M9 adds development computer upgrades, available hardware license purchases,
eleven hardware records, 51 game types and verified planning quotes. See
[development planning evidence](docs/development-planning.md).

M10 composes the original office from locally imported sprite/mask atlases and
128 recovered tiles, restores calendar glyphs and classic desktop controls, and
animates hired employees entering and resting in the lounge. Movement survives
save/load. Existing caches need reimporting. See
[original presentation recovery](docs/original-presentation.md) for verified
behavior and unresolved work paths.

M11 adds development-domain formulas and planning transitions, a bilingual planning
preview, imported employee portraits, full office route rules and computer-upgrade
sprites. M13 subsequently connects work scheduling and release/sales. See
[M11 evidence and integration gaps](docs/development-work.md).

M12 connects employee reassignment to the office: choose a development desk under
Staff, watch the employee walk and sit, then send them back to the lounge.
Assigned desks display locally imported computers, including the purchased model.
Movement, reservations and seated state survive save/load. See
[M12 evidence and limits](docs/milestone-12.md). M13 now schedules project progress.

M13 connects the **development → release → sales** loop. Hire and assign staff,
start a project, choose its type and genre, debug or ship, and produce more copies
as stock sells. The original sales formula and production size bands drive the
simulation. Save/load preserves work and selection checkpoints. Reimport your
original archive and start a new company for this milestone.

M19 restores the company-wide weekly production allowance. First shipment and
reprints consume it; the sales screen re-enables reproduction at the next
200-tick budget reset. Older saves with an unknown allowance require a new
company. See [production-budget.md](docs/production-budget.md).

M20 records the original current-week and twelve prior weeks of product sales.
Open **Sales History** for the classic graph; signed counts and original scale
boundaries are preserved. See [sales-history.md](docs/sales-history.md).

M21 adds consumer investigations, the original five-week recall countdown,
refund decisions and reproduction prohibitions. Both event dialogs survive
save/load without charging twice. See [consumer-recalls.md](docs/consumer-recalls.md).

M14 adds original review scoring, four-reviewer release results, Hall of Fame
status and sales milestone announcements. Reviews and pending announcements
survive save/load and pause the office clock until acknowledged. See
[review and event evidence](docs/reviews-and-events.md).

M15 adds employee dismissal and supported workspace cleanup for nonrenewal,
including workers walking to a desk or lounge. Payroll and recall arithmetic
have isolated regression coverage while their calendar scheduling remains
unresolved. See [employee dismissal evidence](docs/employee-dismissal.md).

M17 imports the eight technology definitions and initial employee skills, connects
technology purchasing and study at all five training seats, and preserves learned
skills through saving, dismissal and rehiring; see [training evidence](docs/training-and-technologies.md).
M18 connects six timed project technologies with diminishing returns, plus two
stress-relief levels. Speed and top-level stress relief remain unresolved; see
[application evidence](docs/technology-application.md).

M16 connects public relations, genre research and production expansion, including
the shared budget and automatic production growth. Research unlocks more genres;
fame increases future project recognition. See [company activity evidence and
clock scope](docs/company-activities.md).
See [the playable loop, evidence and limits](docs/core-loop.md). Calendar economics,
awards and recall effects are still unfinished. Reviews and sales milestones are connected.

## Run

Install Godot **4.4.1 .NET** and .NET SDK 8, then:

```sh
dotnet build OpenGDS2.csproj
godot --path .
```

Alternatively open `project.godot` in the .NET edition of Godot. The title screen
works without original assets. Use **Import original assets** to choose your
legally obtained `GameDev2DX.lzh`; the importer also extracts employee definitions
from the game in memory. The SFX installer itself is not supported.

New Game → enter company/president → select mode → Hire Employees → choose an
applicant → Staff → Assign a Desk → Start Game Development → choose type/genre →
debug or release → Sales → Produce More Copies. Use the lounge for tired staff.
Reopening recruitment rebuilds its five slots. Save/load preserves funds, staff,
source gates, projects, decisions and released products.
Imported character names remain Japanese proper names; interface language can be
changed under Settings. A calendar inspector previews the reconstructed calendar
without skipping unresolved company simulation.

## Evidence and tests

```sh
python3 -m unittest discover -s tests -p 'test_*.py'
dotnet run --project tests/DomainTests/DomainTests.csproj
```

Original-only evidence tests are opt-in; see [employee-initializer.md](docs/employee-initializer.md).
The C# suite has no Godot or third-party test-framework dependency.

[Milestone 7](docs/milestone-7.md) distinguishes recovered facts from gaps.
[Remaining work](docs/remaining-work.md) lists exact next trace targets.
[Validation](docs/validation.md) records **58 passing local C# checks, 16 Python checks
and passing imported-data Godot UI tests**, including desk assignment and return.
The normal SDK build and fixture-free runtime are also tested in GitHub Actions;
original-fixture checks run locally and do not upload original content.

## Copyright boundary

No Kairosoft executables, graphics, sounds, imported employee database or private
saves are bundled. All imports go into Godot's per-user cache. Do not commit that
cache, locally extracted executables/databases or whole disassemblies. Original
save compatibility remains unsupported.

## License

Community-written code and documentation use the [MIT License](LICENSE). See [notice.md](notice.md) for third-party content exclusions and project disclaimers. Original game files and imported data are not distributed with this repository.
