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

## Desktop development exports (M23)

The Validate workflow exports Linux and Windows x86_64 packages after domain and
editor checks pass. Its **OpenGDS2-desktop-development** artifact contains both
ZIPs, with the community game assembly, Godot/.NET runtimes, project notices,
dependency license texts and SHA-256 manifests. No original files or local caches
are included. Builds are development checkpoints; the full port is unfinished.

The Linux and native Windows packages run startup and the full synthetic
development/release/sales UI fixture outside the source checkout. Whole-window
graphical testing and original Windows behavioral comparison remain pending.
The committed solution file is required by Godot's C# exporter; a successful
editor build alone does not establish a successful native export.

For a local export, install the matching **Godot 4.4.1 .NET export templates**
through Editor → Manage Export Templates, then:

```sh
mkdir -p builds/linux builds/windows
godot --headless --path . --export-release "Linux Desktop" builds/linux/OpenGDS2.x86_64
godot --headless --path . --export-release "Windows Desktop" builds/windows/OpenGDS2.exe
godot --headless --path . --script res://tools/export_licenses.gd -- /tmp/opengds2-notices
python3 tools/package_desktop.py --export builds/linux --platform linux --notices /tmp/opengds2-notices --output builds/OpenGDS2-linux-x86_64.zip
python3 tools/package_desktop.py --export builds/windows --platform windows --notices /tmp/opengds2-notices --output builds/OpenGDS2-windows-x86_64.zip
```

Extract the complete ZIP and keep the executable, .pck and runtime directory
together. The title screen starts without imports. Import your legally obtained
GameDev2DX.lzh locally to enable the reconstructed gameplay and original office.

The packaging helper obtains .NET notices from the actual published NuGet runtime
pack. The Godot notice script uses the exporting engine's license/copyright APIs;
these notices are generated into the build, not copied from the original game.
