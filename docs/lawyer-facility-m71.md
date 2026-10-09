# Lawyer consultations — M71

The lawyer can now be constructed for ¥250 million and used to negotiate either
transport or retail fees. Each successful negotiation reduces that fee by ¥200
per copy. Ordinary sales already subtract both company fees, so this change
improves future receipts without rewriting sales history.

## Supported-original behavior

All anchors refer to the official supported executable SHA-256
`4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.
`tools/verify_lawyer_evidence.py` checks the local executable without executing it
or printing original text. Private disassembly remains outside the repository.

- `fGMTenantBengo` transport `00987678` and retail `00987810` display the
  consultation amount initialized to ¥15 million at `009F651C +016C`, ask for
  confirmation, then compare that amount with funds at `+0093..009B`.
- On acceptance they close the facility window and call `0060ED80` at `+0113`
  with meeting mode 13 or 14. Neither handler debits funds. The dispatcher
  `0097BDB8`, form load `009A6DDC`, meeting outcome arms and called message,
  confirmation and close helpers contain no consultation debit. This original
  quirk is preserved: ¥15 million must be available, but is not charged.
- `fGMKikakuKaigi` timer `009EFA40` mode 13 starts at `+419E`; mode 14 at
  `+4E58`. Transport writes `A1188C` (company `SalesFee2`); retail writes
  `A1187C` (`SalesFee1`). The initial ¥1,500 fee always becomes ¥1,300.
- Fees ¥1,300/1,100/900/700 use `Int(Rnd * 2/4/8/16) == 0`; success subtracts
  ¥200, otherwise the fee is unchanged. ¥500 is a five-message refusal with no
  outcome draw. Unmatched fee values queue only the introduction and remain
  unchanged, preserving the original branch dispatch rather than inventing odds.
- Each matched branch queues introduction, first/repeat greeting, question,
  success/refusal and secretary summary. The helper `009933DC` random-employee
  selection runs only for portrait -1. These arms use secretary -2 and manager
  portrait 4, so their messages consume no employee-selection randomness. The
  live outcome adapter consumes one Single draw only where the original does.

## Persistence and presentation

The fee change and result snapshot are prepared once before the first page,
matching the original queue-building order. Modern saves persist target, before
and after fees and the cursor. Loading never repeats the calculation. Each
acknowledgement advances one page; a stale UI callback cannot advance a newer
cursor. Facility, recruitment and simulation guards pause conflicting actions.
Save/load/settings remain available.

Dialogue import schema 9 adds 18 local-only original keys (164 total). Schemas
1–8 remain readable; Japanese uses authored fallback until local reimport.
English translations and Japanese fallback text in the repository are authored.
The ordinary conversation presenter is used; the original timer animation,
meeting background and portrait timing are not claimed to be reproduced here.
No original archive, executable, text dump or private save is packaged.

## Validation

333 domain tests pass, including affordability, independent targets, Single
outcome draws, floor/unmatched cases, rejected transactions, save/load at every
cursor and tampered snapshots. The app compiles. Bounded headless UI checks cover
English and Japanese, declining, completing, saving/loading all five pages and
blocked facility navigation. Checks also exercise the locally extracted schema 9
cache; existing facility-management UI checks remain passing.

Casino gameplay, delayed mystery-lab results and own-hardware development are
still disabled. The next facility area is the mystery laboratory: recover the
seven delayed outcomes and their calendar dispatch before enabling its service.
