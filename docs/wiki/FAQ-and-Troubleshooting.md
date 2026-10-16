# FAQ and troubleshooting

## The download is only source code

Use a desktop ZIP asset under a published [release](https://github.com/UnSpotibleShadow/OpenGDS2/releases).
GitHub's automatically generated source archives are for building the project.
A draft is not public availability.

## The game asks for original files

Import the supported `GameDev2DX.lzh`, not a self-extracting EXE. An incompatible
archive/revision can be rejected. Reimport if an update requests newly recovered
resources. [Installation](Getting-Started).

## A runtime DLL or PCK is missing

Extract the whole ZIP into a fresh folder. Keep its executable, PCK when supplied,
and runtime folder together. Do not run the EXE directly inside the ZIP. Choose
the correct architecture. Report the exact error if the complete package still
fails; the console executable can help collect diagnostics.

## Where are my saves?

Look under `%APPDATA%\OpenGDS2\saves` on Windows. Package filenames and x86/x64
labels do not rename the data folder. Check [upgrading and recovery](Saves-and-Upgrading)
for the old test-folder migration. Close the game before making an external backup.

## Training finished, but game quality did not change

Training teaches an employee a technology. Select an eligible learned technology
while that employee works in the intended project room to apply it. Most effects
spend experience and are not permanent passive game-quality bonuses.
[Employee guide](Employees-and-Technologies).

## Time is not advancing

Check pause/speed settings and pending meetings, reviews, award pages, investigation
results or other decisions. Finish/acknowledge the current conversation. Dragging
a window also suspends simulation until you release it. Do not assume every
paused moment is a crash.

## The company suddenly dissolved

Negative funds trigger dissolution in both modes. Review recent spending and
[recall charges](Consumer-Investigations), which can exceed current cash.

## How do I report a bug?

Use [GitHub Issues](https://github.com/UnSpotibleShadow/OpenGDS2/issues). Include:

- Version, architecture and source commit when available in the build manifest.
- Windows version/display scaling, reproduction steps and expected/actual behavior.
- A screenshot or relevant log excerpt, after checking it for personal information.

Never upload the original LZH, executable, imported assets/database/cache or
original saves. Share a modern reproduction save privately only when requested.
The in-game backup device and the modern previous-save `.bak` are different
features; one does not imply ownership of the other.
