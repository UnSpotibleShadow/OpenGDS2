# Release cleanup — 16 October 2026

This pass prepares source version `0.1.0-ea.1`; it does not export a game build,
create a release/tag, change repository visibility or enable Actions. Gameplay,
save formats and the existing `OpenGDS2-EarlyAccessTesting` data folder remain
unchanged. The final owner review and exact-package release gates remain open.

## Changes

- Project, application-window and title versions agree. The title uses a localized
  Early Access label instead of private-candidate wording. Three unreferenced
  applicant/research messages were removed from both language files.
- README, known issues, testing instructions, public notes and release decisions
  describe current implemented scope. Historical candidate records remain
  historical; the announcement still describes a target rather than a download.
- Public package documents identify the exact packaged version and data folder.
  Known-issue headings no longer rely on one obsolete RC.4 literal. Links between
  bundled documents remain local; unbundled evidence links use the exact source
  commit. Tests package the actual source documents as authored-fixture ZIPs.
- Root working instructions are concise. The complete previous handoff is kept
  in [the archive](archive/agent-handoff-through-20261015.md), with historical
  statuses clearly identified. The development checkpoint starts with current
  status before its historical records. Ko-fi and license/music notices remain.
- The current tracked-tree scan found no original/cache/binary/temp paths to
  remove. Required Godot metadata and verification tools were retained. This is
  not a claim that all Git history or hosted attachments have been audited.

## Verification

- Python suite: 92 tests, 75 passed and 17 optional original-file fixtures skipped.
  Includes 12 package checks, current-document/link handling and historical-heading
  regression. Fixture packages contain authored bytes, not game exports.
- Application build: zero warnings/errors. Godot resource import passes.
- English/Japanese title, scope, mode boundary and previous-save recovery pass.
  The test follows the company form transition rather than calling an obsolete
  Godot binding to a domain-state method.
- Fresh-data title/settings/audio startup smoke passes without an original import.
  Earlier shared-fixture attempts and their causes remain in private check logs.
- Whitespace, active document links, preserved handoff, data-folder and Ko-fi
  checks pass. No native Windows or final exported-package acceptance is claimed.

See [release preparation](early-access-release.md) for the next stages.
