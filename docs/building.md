# Build and run

The reproducible target for this continuation is **Godot 4.4.1 .NET + .NET SDK 8**.
M6 referenced a 4.7.2 SDK without a verified toolchain; M7 pins a downloadable
compatible Godot 4 release. Use the .NET edition, not the standard edition.

```sh
dotnet build OpenGDS2.csproj
dotnet run --project tests/DomainTests/DomainTests.csproj
godot --headless --path . --editor --quit
godot --headless --path . --quit-after 5
godot --path .
```

The title screen does not require imports. Import the local LZH from the title
screen to enable employee recruitment. Importing graphics alone from an old M6
cache does not populate the employee database; reimport the archive once.
Only the SHA-256-verified extracted build is supported; other revisions are
rejected rather than assigned speculative data.

Godot stores imported assets/employee JSON under `user://original` and modern
save slot 1 under `user://saves/slot1.json`. No save compatibility with the
original is claimed. Existing game names and nullable unresolved values are
preserved in the JSON format.

The supplied GitHub Actions workflow runs public Python/C# tests plus Godot
build and headless import/launch checks. It needs no copyrighted fixture. Local
original-data parity testing is optional:

```sh
python3 tools/recover_employee_database.py /private/GameDev2DX.exe --out /private/employees.json
dotnet run --project tests/DomainTests/DomainTests.csproj -- /private/GameDev2DX.exe /private/employees.json
```

Direct Roslyn compilation (including Godot source generators), C# tests and
Godot headless UI checks passed using a temporary runtime workaround. The normal
SDK/MSBuild command remains blocked here; see validation.md. No workaround or
downloaded toolchain is bundled.

Headless UI regression test (use a disposable data directory):

```sh
XDG_DATA_HOME=/tmp/opengds2-ui-smoke OPENGDS2_UI_TEST_SAVE=1 \
  godot --headless --path . --script res://tests/ui_smoke.gd
```

Set `OPENGDS2_ORIGINAL_LZH=/private/GameDev2DX.lzh` to also test import,
five applicant cards and hiring. Keep the cache outside the repository.
