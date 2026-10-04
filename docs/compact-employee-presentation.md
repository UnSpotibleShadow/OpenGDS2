# M37 — Compact employee inspection

Clicking an employee in the office or activating its staff-table row now opens
a narrow child window beside the office, replacing the large management dialog.
A community-drawn red arrow marks the selected employee and follows its current
workspace and sprite offsets. Its position accounts for the office image's
scale and letterboxing. Selecting another employee replaces the panel; closing
it or clicking blank office space removes the arrow and restores company info.
Selection is UI state and is never written into a company save.

The pane follows the supplied original screenshot: imported 64×64 portrait,
level, salary and contract fee, ability/technology tabs, a cyan five-axis chart,
green-on-blue status bars, experience, level-up and dismissal controls. Existing
desk/training/technology/break actions are available from its Actions menu.
Detailed management workflows retain their existing dialogs and guards.
The pane requests 224×376 desktop pixels; its content scrolls when tall fallback
fonts or longer translations require extra space, while action controls remain
visible. English and Japanese labels are provided.

The five stat axes use the original `fGMInfo 009DF294 +057E..0BAA` offsets and
raw-value/100 scaling: fun at the top, novelty at the right, accuracy at the
lower right, music at the lower left and graphics at the left. The original
status-bar code uses 99 as its denominator (+0280 and the corresponding
inspiration/stress blocks). Unknown legacy values show question marks; they
are not replaced with invented stats. Experience uses the existing verified
level threshold and is displayed with the original floor operation.

The new outline, arrow, colors and window dimensions are authored presentation,
not redistributed original pixels or a claim of exact original raster parity.
Portraits and the office still require local original-archive importing.
Inspection is modeless; management and calendar decisions retain their pause
and mutation guards. The original's full pause/window lifecycle and right-click
assignment behavior still need dynamic comparison.

Godot coverage checks table activation, imported portraits, compact dimensions,
tab switching, scaled/letterboxed arrow position, unchanged funds/progression
on selection, and arrow removal. Full rendered desktop comparison remains
outstanding on this headless host.
