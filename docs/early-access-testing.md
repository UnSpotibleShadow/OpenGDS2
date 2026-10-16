# Windows community testing guide — Early Access

Test the version and source commit in the accompanying BUILD_MANIFEST.json.
Record Windows version, resolution/scaling, language, exact steps and pass/fail.
Unrun checks remain untested. The full matrix guides community testing; the
release gate is basic final-package launch/import/save/reload plus fixes for
known crashes, save loss, duplicate transactions and supported-workflow dead ends.

## Start safely

1. Extract the entire ZIP into a fresh folder. Keep both executables, the PCK and managed-runtime directory together.
2. Run OpenGDS2.exe or OpenGDS2.console.exe. The title/window version must match BUILD_MANIFEST.json. No editor or separate .NET installation is required.
3. Read GETTING_STARTED.txt for the package's exact data folder. Back up existing modern saves before upgrading or transferring them. This testing pass must not silently overwrite your ordinary or private-review data.
4. Import your legally obtained GameDev2DX.lzh locally. Start a fresh company for the fresh-start check and read KNOWN_ISSUES.md. Test an existing compatible modern save separately using a copy. Original saves are unsupported.
5. To capture diagnostics, run `.\OpenGDS2.console.exe --log-file opengds2-test.log` from a terminal in the extracted directory.

The permanent folder is `OpenGDS2`, including source builds. The first launch
copies existing `OpenGDS2-EarlyAccessTesting` data once, keeping the old folder and
existing destination files. Test that migration with copies of a profile; include
save backups and confirm language/audio preferences apply on the first startup.
Changing the version label must not change the folder. See
[data migration](stable-user-data.md) for conflicts and retry behavior.

## Community test matrix

| Test | Expected outcome | Result |
| --- | --- | --- |
| Fresh launch without import | Title, settings and quit work; gameplay guides you to import | Untested on Windows |
| Fresh import and restart | Supported LZH imports locally; assets/catalogs persist after restart | Untested on Windows |
| Import failure | Invalid/unsupported archive reports an error without breaking the title | Untested on Windows |
| Settings and audio | English/Japanese, mute and volume persist; appropriate music/sound cues play | Untested on Windows |
| Office | Hire staff, assign desks, upgrade PC; artwork/hit regions remain usable | Untested on Windows |
| Facilities | Purchase/use/cancel/close all supported types; artwork, selector ownership and facility-specific results resume without duplicate effects | Untested on Windows |
| Conversations and windows | Drag/resize pauses time until release; window positions persist through meetings, selections and later pages | Untested on Windows |
| Original game loop | Plan, choose focus/type/genre, develop, debug, ship, acknowledge reviews, sell and reprint | Untested on Windows |
| Promotions | All six media show their own scene/result and charge/increase recognition once through save/reload | Untested on Windows |
| Additional projects | Eligible sequel and manga adaptation complete through shipment without duplicate fees | Untested on Windows |
| Save/reload checkpoints | Planning, paid choices, shipment/review, bank receipts and annual events resume without lost progress or duplicate effects | Untested on Windows |
| Cancellation | Both rooms preserve no-refund behavior and saved staff reactions/results | Untested on Windows |
| Save recovery | Save twice; Load Previous Save restores the previous slot without replacing the current file until Save, including when the current file is damaged | Untested on Windows |
| Technology actions | Training returns staff; each learned action uses its documented target/cost/effect and does not imply a passive boost | Untested on Windows |
| Pending development bursts | Checkpoint/reload retains a pending gain; immediate shipment excludes unfinished gain while continued debugging can finish it | Untested on Windows |
| Annual events and economy | Payroll/renewals, loans, exhibitions and award conversations continue without stalls or duplicate effects; English rival names appear throughout awards/rankings | Untested on Windows |
| Campaign boundaries | Beginner ends after Year 2, December, week 4; Normal continues; negative funds trigger dissolution and return to title | Untested on Windows |
| Display | Test 1280×720/larger displays and 100%/150% scaling; text, controls and windows remain reachable | Untested on Windows |
| Stability and data | Sustained play, repeated reloads and title returns do not crash; data stays in the documented folder and other installations remain untouched | Untested on Windows |

## Reporting

Use the reporting URL in the package's release notes. Public issue access must
be verified when publication happens; these source instructions do not establish
that it is already accessible. Include version/source SHA, Windows/display/
language settings, numbered steps, expected/actual results and a redacted log.

Logs and modern saves can contain names and local paths. Never upload the LZH,
original executables, graphics, catalogs or original saves. Share modern
reproduction saves privately only when requested. Keep cosmetic reports distinct
from crashes, save corruption and blocked progress.
