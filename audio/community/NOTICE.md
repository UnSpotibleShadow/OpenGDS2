# Optional community soundtrack

These eight recordings were supplied by project owner UnSpotibleShadow on
2026-10-03 for integration in OpenGDS2, with an assurance that their use is lawful.
The owner explicitly authorized distributing all eight tracks in the public
OpenGDS2 GitHub repository and downloadable game builds on 2026-10-03.
The source files' metadata identifies Suno as their creation tool.
They are additions to the reimplementation, not original Kairosoft game audio.

The supplied recordings do not establish
an independent MIT license for the music. The repository's source-code MIT grant
does not cover these audio files. No separate standalone reuse license has been
specified. Preserve this notice when distributing OpenGDS2 with this soundtrack.

The files were converted to audio-only Ogg Vorbis, quality 4, stereo 48 kHz.
Both supplied recordings named "Office Loop" were retained: their metadata has
separate creation identifiers and their durations differ.

| File | Supplied source |
| --- | --- |
| office-01.ogg | Office Loop (1).mp3 |
| office-02.ogg | Office Loop (2).mp3 |
| office-03.ogg | Office Loop (3).mp3 |
| office-04.ogg | Office Loop (4).mp3 |
| office-05.ogg | Office Loop (5).mp3 |
| office-06.ogg | Office Loop (6).mp3 |
| office-07.ogg | Office Loop.mp3 |
| office-08.ogg | Office Loop.wav |

Music is optional and can be disabled in Settings without changing game rules.

`title-theme.ogg` is the additional recording supplied as
`ゲームのテーマ(2).mp3` on 2026-10-04, explicitly requested for the main-title
screen. Its metadata also identifies Suno as the creation tool. It is distributed
with OpenGDS2 under the same owner authorization and audio notice above, and is
not covered by the source-code MIT grant. The conversion keeps stereo 48 kHz
audio and removes the MP3's embedded cover art. It loops on the title separately
from the office playlist and uses the same mute/volume settings.

## Seasonal recordings supplied on 2026-10-05

The owner supplied twelve additional WAV recordings for the spring, autumn and
winter playlists and requested compression and integration in OpenGDS2. They are
distributed with the project and its game builds under the same owner authorization
and separate audio notice above. They are not original Kairosoft assets and are
not covered by the source-code MIT grant. Summer uses the existing eight office
recordings. Original supplied WAVs are not bundled.

| Bundled files | Supplied sources |
| --- | --- |
| spring/office-01.ogg through office-04.ogg | Spring_Office_1.wav through Spring_Office_4.wav |
| autumn/office-01.ogg through office-04.ogg | Autumn_Office_1.wav through Autumn_Office_4.wav |
| winter/office-01.ogg through office-04.ogg | Winter_Office_1.wav through Winter_Office_4.wav |

These files use audio-only Ogg Vorbis quality 4, stereo 48 kHz, with source metadata
removed. Duration, encoded size and SHA-256 are recorded in
`seasonal-manifest.json`. Conversion reduced 277,728,960 bytes of WAV audio to
22,143,224 bytes of Ogg audio (about 92% smaller). Existing recordings and the
dedicated title theme are retained.

## Meeting recordings supplied on 2026-10-06

The owner supplied four WAV recordings and requested OGG conversion and
integration by meeting phase. They are distributed with OpenGDS2 under the same
owner authorization and separate audio notice above; the source-code MIT grant
does not cover these recordings. The supplied WAVs are not bundled.

| Bundled file | Supplied source | Dialogue phase |
| --- | --- | --- |
| meetings/planning.ogg | Planning Meeting.wav | Planning discussion |
| meetings/alpha.ogg | Alpha Meeting.wav | Type/alpha introduction, discussion and confirmation |
| meetings/beta.ogg | Beta Meeting.wav | Genre/beta introduction, discussion and confirmation |
| meetings/final.ogg | Final Meeting.wav | Completion/debugging introduction |

The files use audio-only Ogg Vorbis quality 4, stereo 48 kHz, with source metadata
removed. Source and encoded hashes, sizes and durations are recorded in
`meetings/manifest.json`. Conversion reduced 61,311,040 WAV bytes to 4,925,290 OGG
bytes (about 92% smaller). These tracks loop continuously through a dialogue
sequence and use the existing music mute/volume settings. Selection/proposal
controls, ordinary office conversations and shipment remarks use the existing
soundtrack rather than the meeting tracks.
