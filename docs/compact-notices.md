# Compact text-only management notices

Consumer Investigation and similar short notices previously inherited a generic
670×520 management window, leaving most of the window empty and stacking wide
action buttons. Text-only recall, employee dismissal, legacy roster, financial
failure, calendar inspector/event and production-block notices now fit their
content. Native canvases, charts, lists and illustrated forms keep their layouts.

The fitter runs after a named notice is built and only accepts a body containing
labels and buttons. It wraps labels within a 420-pixel content width, places the
existing actions in a single row of 26-pixel buttons and measures the required
height. Height is capped to the workspace; longer content can still scroll.
Status changes such as Saved or a longer save error trigger a fresh fit. Existing
positions are preserved and clamped to the workspace. Queued work verifies that
its window/content are still current before touching them, including after a
Load, Continue or rapid scene change.

Callbacks, refund accounting, save/recovery and acknowledgement rules are
unchanged. A receipt displays an already committed result; Save, Load and
Continue cannot repeat the charge. The close action retains its existing
pending-event handling. This is an owner-requested presentation redesign, not a
claim to have recovered a new original native layout.

Rendered English/Japanese checks cover investigation and refund/no-refund result
notices, compact dimensions and button bounds, Save/Load feedback, grow/shrink
with a long status, Continue, persisted acknowledgement and unchanged funds.
Original artwork and captures remain outside git. No export/release changes;
Windows final-package acceptance remains pending.
