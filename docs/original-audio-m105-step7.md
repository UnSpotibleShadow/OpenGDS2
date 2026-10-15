# M105 Step 7 — original effects

`Module1 0098ABF8` dispatches 1=Gold.wav, 2=Up.wav, 3=Bakuhatu.wav,
4=Fire.wav under the original sound-enable gate. Each branch supplies flag 1
(`SND_ASYNC`): a short asynchronous effect on the shared replaceable voice,
without a loop. The complete 669-procedure index identifies 18 direct calls.
`tools/original_audio_evidence.json` and its verifier preserve nine procedure
fingerprints, the call arguments/targets and four flag anchors without audio.

Verified bindings:

- Office actor work classification: tier 2 starts Up; tier 3 starts Fire. Ordinary
  small work and unrelated upgrades/meetings do not get an invented effect.
- Poker initial five-card reveals and four comparison reveals in double-up use
  Up. Replacement-hand drawing and the dealer-only setup branch are silent.
- Card-chain phase-1 selection uses Up; its initial full hand, replacement timer
  and double-up timer have no separate recovered Up call.
- Poker, blackjack and card-chain accepted bet debits and timed payout transfers
  use Gold. Buying the fifty-coin package, generic money changes and horse
  payouts are not substituted for those source callsites.

Explosion is present in the dispatcher but has no direct caller in the indexed
procedures. The old archive inventory records five WAV files; only four paths
are referenced by this dispatcher, and only three cue values have callers. No
trigger is fabricated for an unused file or the explosion selector.

`GameSession` emits transient presentation notifications around the existing
transactions and per-actor work pass. Notifications add no RNG calls or saved
fields. Restore/new-game/clear reset transient playback, without replaying effects
from a rendered state. Modern save continuation retains the existing game cursors.

`OriginalSoundEffects` loads the owner's imported WAVs lazily from the original
cache, accepts filename case variations, disables any WAV loop metadata, and
uses one AudioStreamPlayer. A new effect replaces the old one. Missing local
files remain silent. Original effects have independent enabled/volume settings
in the existing audio.cfg; writes preserve music settings. Title return, muting,
load and teardown stop transient effects. Community music and meeting/seasonal
behavior remain separate. Defaults and modern persistence are port adaptations.
Reimport the original archive if the local cache lacks Sound files. No original
WAV, artwork or private trace is committed.

Checks: clean app build; 528 optional-original domain checks, including cue timing,
silent branches and no replay on restore; rendered authored-PCM playback/resource/
mute/preferences/case/missing-file checks; affected core-loop/casino workflows and
existing meeting-music continuity. Native Windows mixing and subjective playback
of the owner's WAV files still require owner acceptance; authored tones do not
establish their subjective loudness or timbre.
