# M38 — first screenshot-feedback fixes

This pass implements the straightforward presentation/audio changes from the
[2026-10-04 feedback](user-feedback-2026-10-04.md). It does not establish exact
original presentation or add unproven simulation behavior.

## Implemented

- The supplied title recording is converted to audio-only Ogg Vorbis (stereo
  48 kHz, quality 4) and loops on the main title. It is independent of the office
  playlist, including a local playlist override. One player switches contexts;
  title and office audio do not overlap. Existing mute/volume preferences apply.
- White text fields use a black caret, dark selection with white selected text,
  and a visible focus border. Employee/table selection uses a green highlight
  with black text, following the supplied original recruitment reference.
- Enlarging the game adds desktop workspace without stretching a 1280 × 720
  canvas. Imported office pixels remain at native resolution; existing explicit
  image scaling/hit-coordinate conversion still works. Child windows clamp back
  into the workspace after shrinking and maximized windows follow its size.
- Recruitment has a compact candidate table, one selected portrait/level/chart,
  speed/inspiration bars, learned-technology descriptions and one hire/cancel
  pair. Row selection neither rebuilds the roster nor charges money. Existing
  roster generation, funds checks and hire transactions are preserved.
- Desk assignment is a smaller two-column selector: rooms across columns,
  desks down each column. Occupied desks remain disabled; original room/action
  IDs and transactions are unchanged. This is a usability correction, not a
  claim to have recovered the original assignment gesture.

The title audio is an explicitly requested community addition, not original
Kairosoft audio and not covered by the code MIT grant. Its source and attribution
are recorded in `audio/community/NOTICE.md`. Original assets still require local
LZH import, and no original screenshots or extracted assets are included.

## Validation

Local direct C# compilation passes with zero warnings/errors. The unchanged
domain suite passes all 198 checks (196 public and two private fixtures); all
27 Python checks pass with the supported local executable. Expanded Godot
core-loop and original-import UI suites pass with zero failures.

UI checks cover title-theme selection/looping even when an office override
exists, office playlist switching/mute/volume, caret/selection colors, candidate
selection preserving funds and the full roster, one selected portrait/hire
action, desk assignment/occupied-seat rejection, 1920 × 1080 workspace geometry,
window reachability after shrinking, and the existing full campaign workflows.
Headless tests set their native window size explicitly after initialization;
the dummy display otherwise starts at 64 × 64 with stretch disabled.

Standard SDK builds, exported Linux/Windows packages and native Windows CI all
pass in [Validate run 37223533010](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37223533010).
PR #32 is merged; the SDK build reports zero warnings/errors, and public Godot
plus packaged Linux/native Windows core-loop suites report zero failures.
This host has no graphical desktop, so actual caret visibility, DPI appearance,
recruitment chart text and pixel quality still require graphical comparison.

## Still pending

Sprite-edge artifact diagnosis, tenant plots and occupied-facility artwork,
development-room action popup, original quote/meeting separation, compact
conversation/greeting/bubble presentation, English staff-name readings, remaining
original management-screen layouts, and broader graphical/DPI verification.
No employee names or original gameplay rules were guessed to fill these gaps.
