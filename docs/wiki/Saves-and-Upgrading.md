# Saves and upgrading

## Where data lives

The permanent folder is **OpenGDS2**, regardless of the package filename,
architecture or Early Access label.

| System | Default data location |
| --- | --- |
| Windows | `%APPDATA%\OpenGDS2` |
| Linux | `$XDG_DATA_HOME/OpenGDS2`, normally `~/.local/share/OpenGDS2` |
| macOS | `~/Library/Application Support/OpenGDS2` |

These are data paths, not a promise of published packages for every system.

Modern JSON saves are in `saves`, such as `slot1.json`. The previous valid save
is kept as the matching `.json.bak`. Imported original data, interface/audio
preferences, playlists and artwork overrides also live under the application
profile. Installing a different architecture does not create a separate company;
close the other game before using the shared profile.

## Recovering a previous save

**Load Previous Save** loads the previous valid state into memory. It does not
replace the current slot until you explicitly save. Recovery can therefore lose
progress since that backup. Back up the profile before manual edits or recovery,
and retain matching save/backup pairs.

Original-game save importing is unsupported. Do not copy an original binary save
into the modern saves folder and expect it to load.

## Upgrading from the old test folder

First launch copies eligible data from the neighboring
`OpenGDS2-EarlyAccessTesting` folder once. Close the old version first. The source
folder is preserved, existing destination files are not overwritten, and
conflicting save/backup pairs stay together. Copy failures offer Retry before
starting gameplay.

This is a one-time migration, not ongoing synchronization. Later progress made
in the old folder is not automatically merged. Do not delete migration markers
as a way of combining saves. Back up both folders before transferring a modern
save to an unused supported slot.

Future versions may still require a new company. Read each release's compatibility
notes. [Detailed migration behavior](https://github.com/UnSpotibleShadow/OpenGDS2/blob/main/docs/stable-user-data.md).
