# M68: project cancellation

The live development screen now offers **Cancel project**. A paused confirmation
explains the consequences. **Keep developing** changes nothing; confirmation
discards the project, adds staff stress once and makes the room reusable. New
games, sequels and manga adaptations share this transaction. No desktop package
or GitHub Actions run is needed for this change.

## Original evidence

The official executable SHA-256 is
`4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.
Static inspection covers both complete BGraphic click handlers:

| Form | Procedure / size | Constant pool | Cancellation index |
| --- | --- | --- | --- |
| fGMDevW1 | `009AAA8C` / 2108 bytes | `005FEE38` | 6 |
| fGMDevW2 | `009AA1B8` / 2076 bytes | `005FE5FC` | 6 |

Room 1 offsets below are mirrored 32 bytes earlier in room 2:

- `+01E6..020B`: confirmation helper, followed by its result check; declining
  branches directly to the exit before the staff loop or room-state write.
- `+020E..023B`: selected room, employed count `A1358C`, and employee-slot
  assigned action `A13770`. The loop runs from zero through count minus one.
- `+023F..032B`: assigned actions 6–11 for room index 0, 12–17 for index 1.
  This tests assignment, not seated action, movement or engagement.
- `+032E..06E1`: one random seven-way staff reaction per affected employee.
  These are dialogue choices, not different stress rewards.
- `+06E2..073F`: employed slot → source index `A11E38`, then stress `A11C84`;
  add 20, and replace values greater than 99 with 99. Even stress 100 becomes 99.
- `+0749..0814`: two room-specific summary messages.
- `+0815..0822`: write zero to room state `A1270C`, then unload the form.

The cancellation path has no finance call, refund, shipment, source employment
gate or desk-assignment write. Existing room totals are abandoned; they do not
become a released product. The employee work loop's verified empty-room cleanup
clears pending work and technology applications. The modern service performs
that cleanup immediately so accepting another project cannot revive them.
See also [the release boundary evidence](release-burst-boundary-m67.md).

Reproduce the bounded checks against a privately held original:

```sh
python3 tools/verify_project_cancellation_evidence.py /private/GameDev2DX.exe
```

The checker fingerprints both inspected handlers, verifies global mappings and
small transaction anchors, and outputs conclusions. It neither runs the original
nor emits original strings or full traces. Opcode meanings use the primary
[VB P-Code table](https://www.vb-decompiler.org/vb_pcode_table.htm).

## Compatibility and scope

No save schema or catalog change. Cancellation removes the existing project and
updates employed stress plus its recruitment-source mirror where available.
Desk reservations remain. The other room, money/net accumulators, product list,
production flag and sequel-parent flags remain untouched. Repeating a stale
confirmation cannot apply stress twice.

Unknown legacy workspaces or affected stress block cancellation before mutation.
An unaffected employee's unknown stress does not block it. Active checkpoint
choices must finish first. Polishing room state 2 remains outside the verified
playable workflow. The modern confirmation and result use authored English and
Japanese translations. M69 now restores original randomized staff reactions and ordered summaries with
a saved cursor; see [cancellation dialogue](cancellation-dialogue-m69.md).

## Validation

Synthetic domain regressions cover every desk in both rooms, walking assignments,
fractional stress and cap boundaries, all three own-project modes, no refund,
source synchronization, unknown data, checkpoint guards, repeated confirmation,
corrupt source identities and save/reload followed by immediate room reuse.
The Godot cancellation smoke drives real project buttons, decline, confirmation,
paused time, room isolation and saved results in both interface languages.
