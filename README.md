# OpenGDS2

Community modernization/reconstruction of the 2001 PC Game Dev Story 2 DX,
targeting **Godot 4 + C#/.NET 8**. M6 is the baseline; this continuation includes M7–M11.

**This is not gameplay-complete.** There is an interactive recruitment/hiring,
staff inspection, manual level-up rules and a save/load loop. Development, sales, awards, office progression
and complete weekly economics are still incomplete and disabled. No substitute
simulation formulas or fabricated employees are supplied.

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
sprites. Work scheduling and release/sales remain incomplete. See
[M11 evidence and integration gaps](docs/development-work.md).

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
applicant → inspect staff → Save game. Reopening recruitment rebuilds its five
slots. Load restores funds, staff, source gates, names and candidate snapshots.
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
[Validation](docs/validation.md) records **33 passing C# tests, 13 Python checks, source-generator compilation
and passing Godot headless UI tests**, plus the temporary host workaround used.
Standard SDK/MSBuild verification is still pending.
The CI workflow is supplied for an ordinary .NET-capable environment and has not
been remotely run.

## Copyright boundary

No Kairosoft executables, graphics, sounds, imported employee database or private
saves are bundled. All imports go into Godot's per-user cache. Do not commit that
cache, locally extracted executables/databases or whole disassemblies. Original
save compatibility remains unsupported.

## License

Community-written code and documentation use the [MIT License](LICENSE). See [notice.md](notice.md) for third-party content exclusions and project disclaimers. Original game files and imported data are not distributed with this repository.
