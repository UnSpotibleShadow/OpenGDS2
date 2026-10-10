# Owner test checklist — 0.1.0-ea.1-rc.4

This is an unpublished Windows x86_64 test candidate. Record pass/fail, Windows version, resolution/scaling, language and exact steps. Report the visible version and source commit from BUILD_MANIFEST.json. Do not mark an untested row as passed.

## Start safely

1. Extract the entire ZIP into a fresh folder. Keep OpenGDS2.pck and data_OpenGDS2_windows_x86_64 beside both executables.
2. Run OpenGDS2.console.exe. The title should identify 0.1.0-ea.1-rc.4 as a private Early Access test candidate. No editor or separate .NET install is required.
3. Candidate imports, settings and saves use `%APPDATA%\OpenGDS2-EarlyAccessTesting`. Ordinary `%APPDATA%\Godot\app_userdata\OpenGDS2` data remains separate. Do not move your ordinary save into this candidate for the fresh-start test.
4. Import your supported GameDev2DX.lzh locally. Start a new company. Read KNOWN_ISSUES.md before testing.
5. Capture diagnostics from a terminal in the extracted folder: `.\OpenGDS2.console.exe --log-file opengds2-test.log`.

## Acceptance matrix

| Test | Expected outcome | Result |
| --- | --- | --- |
| Fresh launch without import | Title, settings and quit work; gameplay guides you to import | Untested on Windows |
| Fresh import and restart | Supported LZH imports locally; assets/catalogs persist after restart | Untested on Windows |
| Import failure | Invalid/unsupported archive reports an error without breaking the title | Untested on Windows |
| Settings | English/Japanese, mute and volume persist after restart | Untested on Windows |
| Office | Hire staff, assign all desk orientations, upgrade PC; PCs sit above desks with no atlas-border marks | Untested on Windows |
| Facilities | Purchase/close supported facilities in each plot; occupied artwork updates/reloads; hot spring/recycling/technology/production and lawyer results resume without duplicate effects | Untested on Windows |
| Conversations/meetings | Move a dialogue and meeting window; later pages/windows keep the position; each phase track continues between pages | Untested on Windows |
| Original game loop | Plan, choose focus/type/genre, develop, debug, ship, acknowledge reviews, sell and reprint | Untested on Windows |
| Additional projects | Eligible sequel and manga adaptation complete through shipment without duplicate fees | Untested on Windows |
| Save/reload checkpoints | Quit/relaunch at planning, paid meeting choice, shipment/review, bank receipt and annual event; no lost progress or duplicate money/bonuses | Untested on Windows |
| Cancellation | Confirm/cancel in both rooms; no refund; assigned staff stress/reactions and summary resume after reload | Untested on Windows |
| Save recovery | Save twice; Load Previous Save restores earlier progress; current file stays untouched until explicit Save. With a damaged current slot, the previous valid save remains recoverable | Untested on Windows |
| Scope | Title/mode-selection disclose Beginner ending and experimental Normal; unavailable systems are explained | Untested on Windows |
| Pending development bursts | Another worker triggers Alpha/Beta/Final: the burst survives checkpoint/reload and resumes. Compare immediate shipment (unfinished gain excluded) with Continue Debugging until it finishes (gain included) | Untested on Windows |
| Annual economy | In Normal mode, continue at least three game years; calendar events, payroll/renewal, bank repayments and awards do not stall or double-charge | Untested on Windows |
| Beginner ending | Beginner ends after Year 2, December, week 4; a terminal save/reload preserves the ending and Normal continues into Year 3 | Untested on Windows |
| Display | At 1280×720 and a larger display, test 100% and 150% scaling; text/caret readable, controls and windows reachable | Untested on Windows |
| Stability | Repeated saves, reloads, title returns and an hour of play produce no crash or stuck modal | Untested on Windows |
| Data isolation | Ordinary OpenGDS2 data is unchanged; candidate files stay in its separate directory | Untested on Windows |

A crash, unusable import, corrupted save, duplicate transaction or dead-end in a supported workflow blocks release. Log cosmetic defects separately with screenshots. Keep negative-funds/progression limitations visible; the candidate boundary is documented in DECISIONS.md and must be accepted as part of the exact final release scope.

## Reporting

Use the GitHub Bug report template (the repository is currently private). Include version/source SHA, Windows/display/language settings, numbered reproduction steps, expected/actual outcome and a redacted log excerpt. Logs and modern saves may include names/paths: review them before sharing. Never upload the LZH, original executables/graphics/catalogs or original saves. Modern reproduction saves should be shared privately only when requested.

