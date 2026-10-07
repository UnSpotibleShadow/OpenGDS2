# Beginner ending verification — M64

The supported original executable confirms the existing calendar cutoff.
It initializes **Year 1, April, week 1** and ends Beginner mode when closing
**Year 2, December, week 4**. It does not wait for three elapsed years.
The original routine increments the week first, so its terminal predicate
sees transient week 5. Modern saves retain the valid closing week 4.

## Source evidence

Verified against executable SHA-256
`4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`,
recovered from Kairosoft's official free PC distribution on 2026-10-07.
Only bounded structural facts and the checker are published; binaries,
graphics, archive contents and raw traces remain local.

| Source | Verified behavior |
| --- | --- |
| Common initializer `009F651C +000E..002E` | Members 40/50/60 are year/month/week, initialized to 1/4/1 |
| Weekly routine `009DB4F4 +01C7..01FB` | Exact equality tests year=2, month=12, week=5, joined by AND |
| Weekly routine `+01FC..0213` | Tutorial marker member 3B0C must be nonnegative; a false predicate branches past the ending |
| Mode selection `009886C4 +001E/+0048` | Normal marker is −1; Beginner marker starts at 0 |
| Weekly routine `+04CA` | Terminal exit precedes renewal, payroll, tenants and subsequent weekly processing |

`tools/verify_beginner_ending_evidence.py` verifies the supported hash, field
loads, literal values, equality/AND operations, tutorial-marker gate and exit.
Run it against the locally extracted original executable. It reports only the
opening date, terminal dates, mode markers and ordering.

Historical prose described a three-year tutorial. That description does not
match the supported executable's dates and is not a basis for extending play.
This is static executable verification; no original Windows runtime comparison
is claimed.

## Implementation and compatibility

- Keep the existing deadline, now shared by CalendarRules and GameModeRules.
- Model `EndingClosingDate` explicitly. Retain legacy `TimeLimitYears` for
  reading older modern saves, but set it to null rather than claim three years.
- Save loading already recomputes derived mode rules; an old three-year value
  cannot override the verified ending or extend an already terminal company.
- Correct English/Japanese mode descriptions and expose them as mode-button
  tooltips, including the already verified starting funds.
- Normal mode still crosses into Year 3 without a fixed terminal date.
- Preserve the legacy Normal-mode calendar-block migration used by M35 saves.

Regressions check neighboring dates, exact year equality, Normal bypass,
old mode metadata and terminal save/reload before any later transaction runs.

Validation passed: 297 domain tests, English/Japanese mode tooltip checks and
the full core-loop UI smoke check (zero failures). The UI checks used the native
export runtime with a freshly compiled assembly; no new desktop package was made.
