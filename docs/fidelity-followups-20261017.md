# Ordered original-game fidelity follow-ups

Owner instruction, 17 October: work in order, finish each step, then ask permission
before starting the next and name that step. Use git-flow, with feature PRs into
develop. Actions are authorized again; no new release is requested. Findings come
from current source and the supported original's static evidence, not a paired
native Windows playthrough. Deliberate adaptations need review before removal.

| Step | Finding | Status |
| --- | --- | --- |
| 1 | Horse-race scenery, camera and finish post | Implemented and locally validated on `feature/horse-race-scenery`; PR targets develop. Awaiting CI/review. |
| 2 | Oversized new-company setup | Awaiting owner permission after step 1. |
| 3 | New-company Enter/default confirmation | Not started. |
| 4 | Missing original About/version/link dialog | Not started. |
| 5 | Original menu organization | Not started. |
| 6 | Facility conversation/transaction ordering | Not started. |
| 7 | Consumer Investigation paraphrasing and extra receipt | Not started. |
| 8 | Original GD2 save compatibility | Not started; a separate compatibility feature, not a UI-only fix. |
| 9 | Persisted active casino rounds | Not started; intentional continuity adaptation to review. |
| 10 | Casino exhaustion and numeric-boundary recovery | Not started; intentional protection against native hangs/errors to review. |

## Step 1: scenery recovery

Supported executable SHA-256:
`4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.
The official LZH contains all three backgrounds and both horse/mask sheets;
the existing importer already copies them. No original bytes are committed.

Original `fKeibaMain 0099061C +009D` loads `Graphic/Beibazyo.bmp` at race start.
Timer `009EB860 +3EA6` loads `Beibazyo2.bmp` at tick 45, then `+3F3C` loads
`Beibazyo3.bmp` at tick 200. The close-up source is 421×217; the visible client
is 421×173. Other backgrounds are 421×173.

The close-up self-copies at `+0331..0562` move bands right by 2 pixels
(y=0, height=77), 6 pixels (y=82, height=91) and 21 pixels (y=173, height=44).
The last band is outside the visible viewport. Each second copy reads the tail
AFTER the first overlapping copy. Its effective horizontal period is therefore
421 minus the band's shift; ordinary modulo-421 wrapping would differ.

Finish-phase `+1218..12D0` similarly shifts the full background right by 10
until camera B reaches −1010. `+1304..13AF` draws the 12×147 finishing post from
sheet (0,169) at (round-even(−B−800),10), using mask AND then colour XOR, before
drawing runners. The renderer previously cloned a fixed starting backdrop and
never used the camera to draw this post.

Rendering now reconstructs the applicable backdrop and cumulative scroll from
the saved phase/tick/camera. The native front-buffer copy precedes the background
scroll; tick increments and scene transitions happen after presentation. Saved
ticks 45 and 200 therefore still display the preceding scene. At tick 200 the
renderer reverses the known position transform for that last close-up frame;
sprite frames likewise use the pre-increment tick. The first visible close-up
scroll is tick 47 and the first visible finishing scroll is tick 202. Repeated
redraws and reloads do not advance scenery, consume randomness, mutate race state
or change wagers/results. Missing
scene files retain the reimport hint. Native timer delivery and final Windows
pixel acceptance remain separate from these recovered drawing rules.

Regression coverage executes an independent sequence of overlapping copies,
checks phase boundaries and the stopped camera, verifies post clipping/layering,
and compares repeated rendering plus JSON restoration without state changes.
CI validates source and casino workflows only; it does not export or publish.

Local validation: application build with zero warnings/errors; clean Godot 4.4.1
.NET editor import; 575 domain checks with the private original executable and
independently recovered employee fixture; 93 Python checks (17 optional skips); English/Japanese casino
layouts and horse save/reload/settlement workflows with zero failures. The source
verifier checks all 32 rendering literals, including 25 new scenery/post anchors.
Private logs and original materials remain outside Git. Native Windows visual
acceptance remains unrun.
