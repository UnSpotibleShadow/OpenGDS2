# Initial secretary portrait — M44

The office importer previously reproduced a temporary tile allocation rather
than the completed initializer. The supported original finishes by selecting
secretary record 1, whose portrait is 2, and rewriting the allocated secretary
tile's source column. M44 restores this final write in locally imported
`office.json`. Reimport the required original LZH to refresh an older cache.
No original artwork, names, descriptions or executable are bundled.

## Verified data flow

| Source | Meaning |
| --- | --- |
| `Module1.26`, `009814E4 +0050..0062` | Setter argument 20 writes portrait array member 0938 at append index 0880. |
| `Module1.69`, `009F651C +2516`, stride 4E | Five secretary records have portrait IDs 1, 2, 4, 5, 6. The 28-byte setter calls start at +253E with the same stride. |
| `009F651C +5125..5147` | Secretary tile allocation saves the allocator's current ID into member 08A0. The imported allocation order identifies tile 141. |
| `009F651C +C0F9..C0FE` | Current secretary member 0890 is explicitly set to 1. The earlier cleared value is intermediate state. |
| `009F651C +C104..C13A` | `48 + (portrait[0890] - 1) * 152` is written into source-X array 24BC at tile index 08A0. |

The completed tile is at (101, 79), source (200, 32), size 16 × 32 and depth
offset 26. The importer verifies the supported executable hash and bounded
instruction shapes before applying the write. The Python evidence verifier
checks the setter, record portraits, selection and source-column arithmetic.
The private domain fixture verifies the resulting tile; the private Godot
fixture verifies the actual imported cache.

## Remaining secretary work

This fixes the initial office picture only. Secretary hiring, later portrait
changes, salary/contract transactions and automation are not yet modeled.
M53 persists source index 1 for fresh imported campaigns; older modern saves
retain unknown identity. This is not campaign-state parity for original saves.

Further tracing established a manual advice entry: the office click handler
`009C66BC +071F..0741` checks native coordinates inclusively in
104..114 × 80..104 and calls `Module1.46` (`009B15F0`). The settings form
also calls that helper. Its message queue includes project, facility,
technology, contract, contest, license, computer, stock and funds advice.
The helper queues preliminary messages and ends with one of three funds
messages, shown through the modal conversation presenter. M53 connects the mapped conditions, random-draw order,
manual click entry and compact translated presentation; see
[advice evidence and limitations](secretary-advice.md). The December/weeks ≤3 reminder is one queue item;
it is not proof of an automatic calendar trigger.

This portrait fix does not diagnose or resolve the owner's red-line artifact.
That separate defect remains documented in
[sprite-artifact-investigation.md](sprite-artifact-investigation.md).
