# M25 — original desktop layout and optional community music

## Evidence and scope

The user-supplied original-game screenshot shows an MDI desktop: a small office
window, a neighboring company summary, an employee table, a product table,
toolbar/menu rows and a bottom status bar. This observation establishes the
presentation pattern; it does not establish every original menu handler, saved
window placement or startup window visibility rule.

The previous 932 × 580 office and large sidebar have been replaced by a 466 × 290
office texture inside community-written child-window chrome. The four principal
windows remain present while management, planning, development decisions,
reviews, release and sales details open as another child window. These details
retain the existing verified domain rules and modal clock pauses.

Initial desktop coordinates approximate the supplied screenshot; they are not
claimed as recovered VB6 defaults. Users can move, minimize, maximize or hide
child windows and reopen the principal windows from the Windows menu. The office
keeps native-size pixels. Window positions persist while navigating the current
desktop, but are not yet part of the save format. Locale changes rebuild the
chrome when returning from Settings. Long lists retain scrollbars.

## Locally imported artwork

`Graphic/Kabe.bmp` provides the original desktop wallpaper. In the supported
executable, sixteen `MDIForm1` / `BGraphicH` serialized picture properties contain
23 × 22 toolbar bitmaps at these file offsets:

| Imported ID | File offset | Imported ID | File offset |
| --- | --- | --- | --- |
| 0 | 0x1AD1 | 8 | 0x4F97 |
| 1 | 0x216A | 9 | 0x5630 |
| 2 | 0x2803 | 10 | 0x5CC9 |
| 3 | 0x2E9C | 11 | 0x6362 |
| 4 | 0x3535 | 12 | 0x69FB |
| 5 | 0x3BCC | 13 | 0x7094 |
| 6 | 0x4265 | 14 | 0x772D |
| 7 | 0x48FE | 15 | 0x7DC6 |

These bitmap shapes match the supplied screenshot. The current toolbar maps
available reconstructed actions to those local icons; exact original click
handler correspondence remains incomplete. No bank/award mechanic is enabled
merely because its icon exists. Existing imports can be refreshed from the LZH
to obtain the toolbar bitmaps; missing icons use community-written text symbols.

The supported executable hash and bitmap header/dimension checks remain required.
The original executable is read in memory and is never copied into the public
project or imported cache. The **LZH archive remains required for importing**;
there is no extracted-directory importer or bundled original-data alternative.

## Local translated artwork

Optional replacements live in `user://asset-overrides`, using the same relative
`Graphic/...` path as the imported bitmap. A PNG with the same basename takes
precedence over an override BMP. For example, a locally edited
`Graphic/Title.png` replaces the local imported `Graphic/Title.bmp`.

The corresponding original must already exist. Overrides cannot supply employee
or catalog JSON or bypass rule extraction. Paths containing parent traversal,
absolute roots, drive names or empty segments are rejected. Image replacements
must retain dimensions; office atlases must retain the verified coordinates and
raster-mask geometry. The imported files remain untouched, so deleting an
override restores the original presentation. Reimports leave overrides alone.

This is a local replacement loader, not a public translated-artwork pack or an
automatic bitmap patch. It makes no claim that a particular translation or
adaptation is licensed. Original-derived output remains excluded from exports.

## Optional soundtrack

Music is an optional modern addition, independent of original game behavior.
Recordings are loaded from `user://music` in filename order (Ogg Vorbis, WAV or
MP3). The resolved folder is displayed in Settings. Place files there and restart.
The public source and desktop packages contain no soundtrack recordings.

For a user-owned ZIP containing audio, run:

```sh
python3 tools/import_music.py "Main Game Background Tracks.zip" --output "/path/from/Settings/music"
```

This requires FFmpeg installed locally. It converts audio only to Ogg Vorbis,
quality 4, stereo 48 kHz; embedded cover images and metadata are excluded. The
script reads only audio members into a temporary directory and refuses to
replace existing numbered output files. It does not import original game assets,
remove the LZH requirement, upload files or contact a music service.

The eight supplied recordings have been converted and decoded locally (total
931.32 seconds, about 14.9 MB). Both supplied recordings named Office Loop have
different durations and were retained. Public redistribution awaits explicit
approval; their use is not covered by the source-code MIT license.
See `audio/community/NOTICE.md` for source provenance.

The playlist starts during gameplay, advances on the audio player's finished
signal, wraps after the last recording and pauses on the title screen. With no
local tracks, the game remains silent and Next Track is disabled. Settings expose
an enable toggle, volume and Next Track. Preferences use `user://audio.cfg` and
do not alter saved simulation state, RNG calls or clocks. Manual simulation pause
and decision dialogs do not change the soundtrack. Headless tests load and inspect
resources but do not start dummy-driver playback.

## Validation limits

Domain tests cover local-overlay precedence, original-file requirements,
rule-data exclusion and unsafe paths. The original-fixture UI suite verifies the
native office dimensions, imported wallpaper, hiring, desk assignment and lounge
return. The complete core-loop UI fixture also verifies simultaneous windows,
window controls, planning that preserves the office, employee-table refresh,
locally generated audio durations, saved audio preferences, playlist advancement
and wrapping. Public tests generate silent WAV fixtures, without supplied music.

These are automated geometry/resource/control checks. A full graphical
comparison of all dialogs against a running original game remains outstanding;
the local host currently has no graphical display server. Music decoding was
checked with FFmpeg, but audible listening quality is not established by a
headless test. Calendar economics, awards and office progression remain
separate unresolved systems.
