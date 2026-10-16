# Getting started

## Download and extract

Use a published package from [GitHub Releases](https://github.com/UnSpotibleShadow/OpenGDS2/releases).
A draft or a source-code archive is not a ready-to-play desktop package. If no
published desktop assets are listed yet, a downloadable release is still pending.

Choose the architecture indicated by the asset name. `x86_64` requires 64-bit
Windows; `x86_32` is the 32-bit variant when the maintainer provides one. Each
architecture needs its own export and checks; availability is determined by the
actual release assets, not this guide.

Extract the **entire ZIP** into a fresh folder. Run `OpenGDS2.exe`; the
`OpenGDS2.console.exe` variant also displays diagnostic output. Keep the PCK and
runtime folder together with the executable when supplied. A self-contained
release package does not need Godot or a separate .NET installation.

## Import the original game

1. Obtain the supported original `GameDev2DX.lzh` from an authorized source.
2. Choose the import action on the title screen and select that **LZH archive**.
3. Wait for import to finish, then start a company.

The self-extracting EXE is not a supported import input. Do not rename an EXE or
an unrelated ZIP to `.lzh`. The importer checks the supported original executable;
an incompatible revision is rejected. Original graphics, sounds and catalogs
stay in your local data folder. Never attach that cache or archive to bug reports.

The title and settings work before import, but starting the reconstructed game
requires the original data. Reimport when a newer version asks for newly recovered
resources. [Data location and upgrades](Saves-and-Upgrading).

## Verify a download

A `.sha256` asset is a small text file containing a ZIP's SHA-256 checksum.
Download the matching pair. In PowerShell, run:

```powershell
Get-FileHash ".\your-download.zip" -Algorithm SHA256
```

Compare the hash with the checksum file, ignoring letter case. They must match.
A checksum detects a damaged or different download; it is not a replacement for
using the project's actual release page. You do not need to put the checksum
file in the game folder.
