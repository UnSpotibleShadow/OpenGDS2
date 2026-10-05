# M47 — optional seasonal soundtrack and crossfades

This is a community presentation feature, not reconstructed original behavior.
Seasonal music defaults to enabled; Settings → Background music can restore the
full office playlist. Mute, volume and the seasonal preference live in
`user://audio.cfg`, independently of simulation saves.

| In-game months | Season | Bundled recordings |
| --- | --- | --- |
| March–May | Spring | spring/office-01 through office-04 |
| June–August | Summer | existing office-01 through office-08 |
| September–November | Autumn / fall | autumn/office-01 through office-04 |
| December–February | Winter | winter/office-01 through office-04 |

Spring, autumn and winter use the twelve recordings supplied on 2026-10-05.
Summer uses all eight existing office recordings. The new uncompressed WAVs
totaled 277,728,960 bytes; audio-only Ogg Vorbis quality 4 at stereo 48 kHz totals
22,143,224 bytes (about 92% smaller). Source WAVs remain untouched outside the
repository. All conversions were decoded with FFmpeg and their durations checked
against the supplied WAVs. Encoded sizes and checksums appear in
`audio/community/seasonal-manifest.json`; the separate audio notice still applies.
The dedicated title theme continues
to loop separately, regardless of the selected season.

## Local playlists

The resolved music folder appears in Settings. Add Ogg Vorbis, WAV or MP3 files
to its `spring`, `summer`, `autumn` or `winter` subfolder, then restart. Tracks
are ordered by filename and wrap within that season. Subfolders are optional:

1. Valid tracks in the current season's folder take precedence.
2. Otherwise a valid playlist directly in the music folder is used.
3. Otherwise the season's bundled playlist is used, with the full bundled
   playlist as fallback if that season's recordings are unavailable.

Disabling seasonal music uses the general local playlist, or all twenty bundled
office recordings when no general local playlist exists.
An unknown calendar month also uses that general playlist. Custom playlists
remain local; this feature does not upload or copy audio to the repository.

## Calendar integration and limits

Opening a company or loading a save immediately selects its current season.
Office calendar ticks refresh the selection before presenting any calendar or
development decision. Entering a different playlist selects its first recording;
same-season updates, dialogs and saving preserve the current recording. If two
seasons share the general fallback playlist, that playlist continues uninterrupted.
December and January are the same season. A muted season transition selects the
new playlist without unmuting it. Next Track and the player finished signal wrap
within the active playlist. Audio playback position is not stored in the
simulation save format.

## Crossfade

A season transition during enabled office playback overlaps the outgoing and
incoming tracks for two real-time seconds. Equal-power sine/cosine gains are
multiplied by the user's configured volume; changes to that volume apply to both
voices. The outgoing voice starts at the previous recording's playback position.
The fade continues while the simulation is manually paused or a dialog is open.
It does not use the accelerated simulation clock.

At completion, the outgoing player is stopped and its stream released. Muting,
Next Track, leaving gameplay or returning to the title cancel that voice. A
muted season transition does not start a fade. Rapid season changes replace the
older fading voice and retain at most two players. Finishing the outgoing voice
completes its fade without advancing the incoming playlist. First entering
gameplay selects the recording directly; the title theme remains separate.

The soundtrack reads the game month but never changes calendar rules, RNG,
money or domain state. Original assets and the LZH import requirement are unchanged.

## Validation

Pure C# checks cover all twelve months, invalid/unknown months and the four
bundled playlists and equal-power crossfade gains. Godot fixtures use authored silent WAVs to check date-based save
loading, same-season continuity, seasonal advancement/wrapping, missing-season
fallback, muted transitions, winter across New Year and ordinary-playlist mode.
The core-loop fixture also checks a real May-to-June calendar boundary, both fade
voices, mid-fade volume, fade completion while paused, mute/title cleanup and
rapid retargeting. The UI fixture checks the durations and wrapping of all twenty
bundled recordings and title-theme preservation. Python checks verify all twelve
compressed-file hashes and that no community WAVs are bundled. Headless
tests establish resource/control behavior; they do not establish listening quality.
