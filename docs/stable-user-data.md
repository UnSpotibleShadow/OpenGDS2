# Stable OpenGDS2 user data

The data directory is named **OpenGDS2**, independently of game version,
Early Access status, debug/release mode or package filename. Project settings
use that same custom name. Windows uses `%APPDATA%\OpenGDS2`; Linux uses
`$XDG_DATA_HOME/OpenGDS2` (normally `~/.local/share/OpenGDS2`); macOS uses
`~/Library/Application Support/OpenGDS2`.

## First launch after the folder change

Close the previous game before launching the updated one. Before reading
preferences, initializing audio or displaying the ordinary title, the app looks
for the sibling `OpenGDS2-EarlyAccessTesting` directory and copies:

- All save slots and previous-save backups under `saves`.
- The imported `original` cache, local `music` playlists and `asset-overrides`.
- `interface.cfg`, `audio.cfg`, `rankings.json` and `employee-name-readings.json`.

The old folder is never deleted or modified. Existing destination files are
never overwritten. If either member of an existing save/backup pair conflicts
with its legacy counterpart, that entire slot is left alone: a new save must not
receive an unrelated old backup, or vice versa. Other unoccupied slots can copy.
Identical partially copied slots can resume their missing backups.

Each file is copied to a unique temporary file in its destination directory,
flushed and renamed without replacement. The completion record is written only
after success. Interrupted/failed attempts can retry without overwriting files
already copied. Locks protect concurrent migration attempts. Automatic migration
refuses symbolic links/junctions rather than traversing them.

A copy failure shows localized recovery guidance and Retry/Quit before ordinary
game services start. Check free disk space/access permissions and close other
copies of the game; Retry reloads startup and resumes the copy. The original
folder remains the recovery source. Logs and generated localization caches are
not copied; required localized artwork is regenerated from the imported cache.

A successful attempt records `.legacy-data-migration-v1.complete`, including
fresh installations with no old profile. Subsequent launches never reimport old
progress or resurrect deleted destination saves. Running an old version later
will not automatically merge its newer progress into the stable profile.

## Existing destination data and manual recovery

Back up both folders and close both versions before any manual transfer. If a
slot already exists in OpenGDS2, it wins; the untouched legacy folder remains
available for inspection. To keep both companies, transfer a desired modern
save to an unused supported slot and keep its matching `.json`/`.json.bak` pair
together. Do not mix unrelated backups or overwrite your only copy of progress.
The game does not import original-game saves or change the modern save schema.

An invalid completion record or symbolic-link layout needs manual inspection;
keep backups before correcting it. Do not remove the completion record merely
to pick up later play from an old version: transfer the intended files explicitly.
Custom development/testing data roots with names other than OpenGDS2 do not
trigger this neighboring-folder migration. Automated tests use disposable OS
data roots, not the player's profile.

## Validation and release boundary

Five domain regressions cover first copy, all migrated categories, source
preservation, destination/save-family conflicts, resumable failure, one-time and
fresh-profile behavior, custom roots, invalid markers and symbolic links. The
full domain suite with the supported original passes 570 checks. Godot checks
exercise actual first-start language/audio, imported-file retention, loading a
migrated modern save, restart protection and the failure/Retry route. Packaging
records the configured directory in both private-candidate and public modes;
version-label changes do not select another data folder.

Native Windows migration acceptance remains part of final-package testing.
This change creates no public release or downloadable game build.
