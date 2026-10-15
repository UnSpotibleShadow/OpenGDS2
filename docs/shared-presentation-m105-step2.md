# M105 — Step 2: shared window and artwork handling

The owner authorized Step 2 after the native evidence checkpoint. Source changes
remain local on `codex/m105-presentation-evidence`; publication is Step 8. Keep
#95 open and request permission before Step 3.

## Changes

- Older management screens now identify their window and restore a separate
  session position: licences, staff, training/use, production, histories,
  rankings, facility overview/closure, company activities, settings, calendar
  notices and other unconverted notices. `CompactManagementWindow` uses the same
  identity/restore helper. Existing conversation and per-room position keys
  remain intact.
- Employee details remember their moved position when changing employees,
  closing/reopening, or returning after a modal such as Level Up. This preserves
  position; it does not change which screen owns the active transaction.
- Bank, save/load, activity confirmation and planning discussion scenes receive
  their imported native title icons. Named older screens use corresponding
  established form icons; authored utility notices do not acquire guessed ones.
- Native button artwork supports 23×22 small action skins and 39×16 tab skins,
  in addition to 63×22 controls and 16×16 arrows. Level Up and dismissal use their
  evidenced glyphs. Ability/technology tabs retain native bevel variants and use
  existing localized captions, preventing raster text from stretching when
  English requires wider controls. Hover/focus content margins remain stable.
- Local overrides accept PNG source assets as well as BMP. Ownership still
  requires the matching extracted file; metadata overrides remain excluded.
  Explicit replacements take precedence over generated English patches, and
  the image loader retains its original dimension check/fallback.

## Verification

The focused English/Japanese window/artwork check covers distinct window names
and title icons, moved-window reopening, employee switching and Level Up return,
production's media-information detour, small button/tab textures, stable hover
margins, PNG precedence and wrong-sized override fallback. Fixtures are authored;
optional imported artwork remains private. Catalog/recall omissions in the first
test fixture were corrected before the successful pass.

Clean application build: zero warnings/errors. Optional-original domain suite:
522 tests pass, including PNG ownership/path cases. Headless core-loop and rendered native
layout regressions pass with zero failures. The rendered full core-loop attempt
reached its 180-second deadline without reporting an assertion failure; the
workflow check was completed using its established headless route. External
checkpoints and the active AGENTS handoff record these distinct results.

The screen layouts scheduled for Steps 3–6 are unchanged in this step. Original
audio integration remains Step 7. No export, release, Actions run, visibility
change or remote publication is included.
