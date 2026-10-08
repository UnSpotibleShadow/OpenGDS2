# M69: cancellation staff reactions and summaries

Confirmed cancellation now opens the existing compact conversation presenter.
Each assigned staff member reacts once, in employed-list order, followed by a
secretary stress summary and a game-discard summary. The sequence pauses work
and calendar time. Save/load resumes the current page without rerolling a
reaction, cancelling again or adding stress again. M68's +20/cap-99 transaction,
desk reservations, no-refund behavior and room cleanup are unchanged.

## Original evidence and presentation

Both complete room click handlers are fingerprinted in
`tools/verify_project_cancellation_evidence.py` against the supported original
SHA-256 `4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.

| Original branch | Room index 0 | Room index 1 |
| --- | --- | --- |
| Click procedure / pool | `009AAA8C` / `005FEE38` | `009AA1B8` / `005FE5FC` |
| `Int(Rnd * 7)` draw | `+032E..033C` | `+030E..031C` |
| Named employee caption | Pool `16`, employee name, `19` | Pool `14`, employee name, `17` |
| Employee role / portrait | Pool `1B`; employee-source portrait | Pool `19`; employee-source portrait |
| Seven reaction texts | Pool `1C..22` | Pool `1A..20` |
| Stress summary | Pool `24`, room+1, `25` | Pool `22`, room+1, `23` |
| Discard summary | Pool `24`, room+1, `26` | Pool `22`, room+1, `24` |

The staff loop tests **assigned action**, including employees walking to the
room. It queues one seven-way reaction, applies the same stress increment for
every variant, then queues the two room summaries and empties the room. The
checker validates the random draw and matching captions/texts in both pools
without printing or distributing original strings.

`OriginalDialogueImporter` extracts ten additional entries into the user's local
cache: seven reactions, a named caption and two summaries. Dialogue cache schema
8 contains 146 entries. English uses authored translations. Japanese uses the
locally extracted original; reimport the supported LZH to obtain the new entries.
Older dialogue caches 1–7 remain accepted. Missing new Japanese entries display a
reimport notice and can be acknowledged without repeating cancellation.

The modern save stores a room index, staff identity/name/portrait snapshots,
selected variants and a page cursor in `PendingProjectCancellation`. Original
binary text is never stored in that modern sequence. The original transaction
is committed before presentation, as it was in M68; each acknowledgment changes
only the cursor. Preparing all snapshots/random choices before mutation keeps
failed preparation atomic. A single-precision random draw matches the verified
original `Rnd`/`Int` branch. An empty room still gets both summaries with no draw.

The existing 20 ms reveal, portrait animation and moved-window placement are
reused. The session clock, recruitment and direct work are suspended while the
sequence is pending; UI actions permit save/load/settings/return to title.
Acknowledgments guard the captured sequence and cursor against stale callbacks.
After the last page, the office and ordinary work resume.

This adds the verified cancellation sequence, not the still-unproven automatic
conversation queue's scheduling/cadence. Polishing and external/contract work
remain disabled. No save-schema version change, desktop package or Actions run.

## Validation

- 313 synthetic domain tests pass. Added checks cover all seven variants, staff
  order and walking assignments, zero-staff summaries, cursor reload at every
  page, no duplicate penalties, paused clocks/recruitment, bad random values,
  corrupt saved cursors/snapshots and absent fields in older modern saves.
- The original evidence checker and actual local dialogue extraction pass:
  schema 8, 146 entries, all ten cancellation keys.
- Godot C# application compilation passes. The expanded cancellation smoke
  checks English and Japanese controls, exact selected/summary text, page reload,
  moved-window position, modal action gating, frozen work/time and completed
  reload. It runs with new local original dialogue, older schema-7 dialogue and
  no dialogue cache. Optional `OPENGDS2_LOCAL_DIALOGUE` points to a private local
  cache for the original-text comparison; no such file is shipped.

Local UI checks use the workspace host, not native Windows. Optional title audio
is absent in the partial local restoration and emits resource warnings; no audio
validation is claimed. Source publication preserves unrelated packaged assets.

```sh
python3 tools/verify_project_cancellation_evidence.py /private/GameDev2DX.exe
# Run with a disposable user-data directory and the AGENTS.md watchdog:
python3 tools/bounded_process.py --timeout 180 --heartbeat 15 \
  --checkpoint ../checks/m69.json --log ../checks/m69.log \
  --label cancellation-ui --fail-pattern 'SCRIPT ERROR:|Unhandled exception' \
  -- env XDG_DATA_HOME=/tmp/cancellation-check godot --headless --path . \
  --script res://tests/project_cancellation_smoke.gd
```
