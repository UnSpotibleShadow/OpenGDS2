# Programming challenge audit and Hall celebration — M99 / #95

Baseline: M98 main `2507317c6ea5c1b5a2fde10c0698642cc8927564`.
Static tracing resolves the challenge's trigger, actor/room identity, reel,
stop/hold timing, result effects and interrupted caller return. This is not an
ordinary burst or a technology application. The challenge itself is not wired
into the modern game yet; its persisted mid-employee-pass continuation must be
implemented together with its UI. This batch implements the independently
recovered Hall of Fame celebration below. Ticket #95 stays open.

## Correction to the M98 census

Failure dispatch is **meeting mode 6**, not 9. `0098F68C +01AA` loads literal 6;
success/super-success at +01EB/+022C load 10/17. `0097BDB8` passes the argument
directly to member 38D0. The corresponding timer branches at `009EFA40 +1D2A`,
+3381 and +6478 confirm these meanings. M98's mode-9 mention was a transcription
error and has been corrected. Mode 9 belongs to a different event.

## Trigger and selected actor

Within the active seated-worker branch, `Module2 00A0283C +50B7..516B` requires
software room state A1270C=1 and stage A12974 in 0/1/2. The shared global A112A8
increments **per qualifying actor pass**, not per room, calendar week or wall
second. Trigger requires counter >25000×A149B8 and floor(Rnd×10000×A149B8)=1.
The random expression is evaluated before the Boolean AND: qualifying passes
consume that draw even below the threshold. A149B8 is the existing native timing
factor; this is not a new office-time policy or resolution of #94.

At +5173..5184 the counter resets to 0, A14548 captures the current workspace
actor slot and A14558 captures the current development room. Employee source
identity and portrait resolve through A11E38[actor slot], not the current mouse
selection, employee-list index or a random employee. Introductory secretary
messages use the actor's inspiration bands; +5469 opens fGMKoudo modally.

## Reel and interaction

Load `009A19A0` initializes mode/index/slow counter to zero, then selects the
following ten-position reel from that actor's inspiration. Values are
0=failure, 1=success, 2=super-success. Order matters: this is a timed start/stop
reel, not a draw from an unordered weighted bag.

| Inspiration | Reel positions 0–9 | Failure / success / super slots |
| --- | --- | --- |
| >=80 | 2,0,1,0,1,1,2,0,1,0 | 4 / 4 / 2 |
| >=60 | 1,0,1,0,2,1,0,0,1,0 | 5 / 4 / 1 |
| >=40 | 0,0,1,0,2,0,1,0,0,1 | 6 / 3 / 1 |
| >=20 | 0,1,0,0,1,0,0,1,0,0 | 7 / 3 / 0 |
| <20 | 0,0,1,0,0,0,1,0,0,0 | 8 / 2 / 0 |

Button `00980EB8`: first click changes mode 0→1 and its caption to Stop.
Second click changes 1→2, disables the button and consumes one draw to select
five or six remaining reel advances. Timer `0098F68C` increments/wraps the index
0–9 on every spinning callback. While stopping, every third callback increments
it, decrements the remaining advances and enters mode 3 when zero. Mode 3 reuses
that zeroed counter for a thirteen-callback result hold. It then reads the final
reel slot, closes the reel window and opens the corresponding result meeting.
No result-selection random draw replaces the player's timing.

## Result effects and message order

| Outcome | Native result meeting | Mutation | Additional draws |
| --- | --- | --- | --- |
| Failure | 6 | Room bugs +60; room size A129AC +6 | One employee-comment choice from four |
| Success | 10 | One quality +40; actor experience +30, capped at stored A11C68 | Quality choice from four, then employee comment from three |
| Super-success | 17 | One quality +100; same +30 capped experience | Quality choice from four, then employee comment from three |

Quality ordering is fun, novelty, graphics, music. Successful cases report the
outcome, choose/report the affected quality, write the quality, report the actor's
comment, then write/clamp experience. Failure reports the outcome and bug increase,
reports the actor comment, then commits bugs and size. No fee, recognition bonus,
progress-point increase, level-up or technology grant occurs in these branches.
The port's existing EmployeeLevelUpService supplies the recovered current-level
threshold; do not substitute remaining experience or a cumulative lifetime cap.

## Return and save integration requirements

Reel load calls the common pause helper 0097B500; its unload calls resume helper
0097B410. These change the MDI timer-enabled flag. They do not reset the actor's
work or transfer it to another room. `0097BDB8` opens the result meeting modally;
its completion closes the meeting. The original office caller resumes at +5471
in the **same actor pass**, continuing later event/ordinary-work branches.
Re-running the top of that employee pass is not equivalent: it can repeat stress,
idle animation, the challenge counter and random draws. Modern GameSession has
no saved mid-pass cursor today. A faithful integration must save actor/room,
reel/mode/index/remaining/slow/hold counters, result choices, dialogue cursor and
which mutations already committed, and retain the office continuation position.
Legacy saves lacking a shared challenge counter must remain unknown rather than
be assigned guessed historical actor passes. Native Windows callback intervals,
held-frame state and whole-session VB6 RNG equivalence remain runtime limits.

## Hall of Fame celebration implemented in this batch

Qualifying new review acknowledgment now queues a separate celebration while
retaining the existing one-time Hall flag/counter transaction. Two 256×160 Image1
sources are locally extracted from offsets 2FD305 (index 1) and 2F2EA1 (index 2).
Image0 is the initially empty destination. Callback 00982814 tests counter 3 to
copy image 1, >=5 to copy image 2 and reset to zero, then increments. The original A143FC counter has no load reset and carries between Hall windows.
The modern scene retains the shared counter as well as the per-window destination
frame across save/load and freezes office ticks. Legacy saves with an unknown shared
phase start new celebrations at a documented modern phase zero; existing old
Hall flags are never replayed.
Closing clears presentation only; it never increments Hall count again. Old saves
with an existing Hall flag do not invent a pending historical celebration.
Reimport the original archive for these pictures; missing caches show a readable
fallback. Display callbacks use modern 25ms pacing, without claiming native timer
or exact font/window parity. The original secretary/sequel message sequence is
still a separate presentation task; this change does not claim that entire
sequence is restored.

`verify_programming_challenge_evidence.py` verifies nine procedure fingerprints,
36 bounded anchors, all fifty ordered reel writes and result modes 6/10/17.
The existing M98 resource fingerprints cover the two Hall source BMPs. No original
pixels, message bodies, executables or full traces are committed. Relevant domain
and bilingual Hall checks are batched after implementation; publication does not
repeat successful checks.

Validation completed: 495 domain checks; clean application build and import;
bilingual Hall animation/save-load/acknowledgment checks and existing review/event
regressions. The shared-counter refinement was verified after its implementation;
there are no publication-only test reruns.
