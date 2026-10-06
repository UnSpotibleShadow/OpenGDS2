# M56 meeting soundtrack

The four owner-supplied WAVs are converted to Ogg Vorbis quality 4, stereo 48 kHz,
with metadata removed. Only the OGGs, their integrity manifest and the conversion
helper are committed. The original WAVs remain in the upload directory.

| Dialogue | Recording |
| --- | --- |
| Ordinary, sequel and manga planning discussion | planning.ogg |
| Alpha/type introduction, discussion and choice confirmation | alpha.ogg |
| Beta/genre introduction, discussion and choice confirmation | beta.ogg |
| Completion/start-debugging introduction | final.ogg |

Meeting music uses a separate player. It pauses the office recording, cancels
an in-flight seasonal crossfade and loops while the dialogue is active. Page
recreation and saved checkpoint message queues retain the same phase/player,
so consecutive pages do not restart the recording. Closing/cancelling dialogue,
opening selection or proposal controls, leaving for the office/title or loading
another desktop ends the meeting soundtrack. Ordinary secretary/hire/calendar
conversations and shipment commentary do not select a meeting phase.

Mute and volume are shared with the existing soundtrack. Muting pauses meeting
playback; unmuting resumes it. Advancing the office playlist cannot replace the
meeting recording. Same-session dialogue/meeting position retention is preserved.

The two Python tests verify encoded hashes, durations, Vorbis/stereo/48 kHz and
absence of cover art or committed WAVs. The domain test verifies phase mapping.
The real-time Godot audio check starts dummy-driver playback to verify that a
same-phase request does not seek to the beginning, office audio stays silent,
mute/volume work and office/title music returns. The full synthetic core loop
checks phase selection through ordinary/sequel/manga development, checkpoint
reloads, dialogue sequences, cancellation and absence of music during choices.
Headless playback checks do not establish subjective sound quality or native
Windows execution. No desktop debug package or Actions run is produced.

Development publication uses a feature branch followed by a PR into main, as
requested by the owner. This preference and the opt-in-only debug-package rule
are recorded in AGENTS.md.
