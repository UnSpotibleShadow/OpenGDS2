# M46 — optional seasonal soundtrack

This is a community presentation feature, not reconstructed original behavior.
Seasonal music defaults to enabled; Settings → Background music can restore the
full office playlist. Mute, volume and the seasonal preference live in
`user://audio.cfg`, independently of simulation saves.

| In-game months | Season | Bundled recordings |
| --- | --- | --- |
| March–May | Spring | office-01, office-02 |
| June–August | Summer | office-03, office-04 |
| September–November | Autumn / fall | office-05, office-06 |
| December–February | Winter | office-07, office-08 |

These are the eight existing authorized recordings allocated in source order.
No new compositions or claims about their musical suitability are introduced.
Their existing audio notice still applies. The dedicated title theme continues
to loop separately, regardless of the selected season.

## Local playlists

The resolved music folder appears in Settings. Add Ogg Vorbis, WAV or MP3 files
to its `spring`, `summer`, `autumn` or `winter` subfolder, then restart. Tracks
are ordered by filename and wrap within that season. Subfolders are optional:

1. Valid tracks in the current season's folder take precedence.
2. Otherwise a valid playlist directly in the music folder is used.
3. Otherwise the season's bundled pair is used, with the full bundled playlist
   as fallback if both recordings are unavailable.

Disabling seasonal music uses only the general local/bundled office playlist.
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
within the active playlist. There is no crossfade or audio playback position in
the simulation save format.

The soundtrack reads the game month but never changes calendar rules, RNG,
money or domain state. Original assets and the LZH import requirement are unchanged.

## Validation

Pure C# checks cover all twelve months, invalid/unknown months and the four
bundled pairs. Godot fixtures use authored silent WAVs to check date-based save
loading, same-season continuity, seasonal advancement/wrapping, missing-season
fallback, muted transitions, winter across New Year and ordinary-playlist mode.
The core-loop fixture also checks a real May-to-June calendar boundary. The UI
fixture checks all four bundled pairs and title-theme preservation. Headless
tests establish resource/control behavior; they do not establish listening quality.
