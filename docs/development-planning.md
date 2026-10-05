# Development planning and hardware — M9 evidence

These are static findings from the supported local executable, not observations
of the original Windows runtime. Development task scheduling, completion,
reviews and sales remain unresolved; the development button remains disabled.

## Computers

The reset routine `009DD234 +06A6` sets member 998 (`A119B4`) to Empty/zero,
the current computer index. The common initializer does not overwrite it.
Four computer definitions are direct calls to helper `0098141C` at initializer
offsets `+A368 + index * 51` (hex). Arguments 10..24 map to members
9BC (identifier), 9D8 (name), 9F4 (bit label), A48 (numeric bits), A10 (price),
A2C (description). All fields remain local imported data. C#/Python parity passes.

The purchase handler `fGMComK.Command1`, start `0099037C`, rejects the current
model (`+0000`), rejects a lower bit width (`+0065`), and rejects insufficient
funds (`+00D0`). Success subtracts the full selected price (`+0134..+0151`) and
replaces the current index (`+0158`). There is no trade-in refund in this path.
The remaining writes update the computer's office sprite. The new domain service
implements the financial/current-computer effects; the modern UI has no tile
sprite state. A bilingual computer purchase screen and modern save fields are
implemented. Legacy modern saves with no computer field retain an unknown value.

## Hardware

Eleven hardware definitions use helper `009874E8`, thunk `0060EE1C`.
Arguments 10..4C map to these original members:

| Argument | Member | Recovered meaning |
| --- | --- | --- |
| 10 | 0EC8 | Unresolved numeric identifier; not the source array index |
| 14 | 0EE4 | Hardware name |
| 18 | 0FA8 | Manufacturer identifier, used by name resolver 0060ED8C |
| 1C | 0F00 | Bit width |
| 20 | 0FC4 | License fee |
| 24 | 0F1C | Hardware availability flag |
| 28 | 1050 | Unresolved numeric column |
| 2C | 0F38 | Unresolved numeric column |
| 30 | 0F54 | Unresolved numeric column |
| 34 | 0FE0 | Development planning points |
| 38 | 0FFC | Development planning cost |
| 3C | 0F70 | Unresolved numeric column |
| 40 | 1034 | Description |
| 44 | 1088 | Release message; deliberately omitted from importer |
| 48 | 10A4 | Unresolved numeric column; omitted |
| 4C | 10C0 | Unresolved numeric column; omitted |

The importer reads the thirteen trailing literal arguments at each known call
site, not a speculative whole-program decompilation. One release message is
constructed by concatenation with a runtime value, so it must not be treated
as a direct string literal. Those final three columns are not reconstructed.
All imported fields agree with independent Python extraction.

## Licenses and availability

Initializer `009F651C +6321..+6330` grants hardware source 0's license by setting
member F8C (`A11FA8`) to 1. Later debugger-only code after the first return is
excluded. Initial availability comes from the eleven definitions' member F1C:
source indices 0,2,4 are available. `fGMHLicense.Form_Load`, start `009A9018`,
filters its source list by global `A11F38` (member F1C) at `+0460`.

`fGMHLicense.Command1` (`00989890`) checks whether the source is already
licensed, then invokes meeting modes 2 and 3. `fGMKikakuKaigi.Timer1`, start
`009EFA40`, mode 3 offers the full fee from `A11FE0` (member FC4) at `+1A23`,
checks funds at `+1B08`, charges at `+1B75..+1B92`, and sets the licensed flag
at `+1C17..+1C24`. The insufficient-funds branch returns to the payment prompt;
declining closes the conversation without a license. No random negotiation
discount is established in this transaction. The new UI presents payment as
an explicit button and the service applies these verified effects.

Available hardware and licenses persist independently in modern saves. Future
release/withdrawal events must update availability; their scheduling is not yet
implemented. Older modern saves retain unknown collections, disabling purchase.
Computer bit width is not a license-purchase requirement in the traced handler.

## Planning quote and compatibility gate

### Game types and structural table

51 game-type records now import locally. Their setter is `009836A4` (thunk
`0060EE58`), with calls at initializer `+68D9 + index * F5`; definitions begin
at `+6889 + index * F5`. Arguments 10,14,18,1C,20,24,28,2C,30,34 populate
members 1394,13B0,12B4,12D0,12EC,1308,1378,1324,1340,135C. Member 12B4 holds
the type name and 1324/1340/135C hold three example titles. Six numeric fields
remain structural pending use-site verification.

The accompanying helper `0098F178` writes member 13CC's two-dimensional table
at the just-created type index. Although its call supplies sixteen arguments,
the helper writes **fifteen columns**: arguments 10..44 populate columns 0..13,
argument 48 is unused, and argument 4C populates column 14. Byte-anchor tests
caught this nonsequential mapping; the importer preserves the unused argument
separately. It does not fabricate a sixteenth table column or label these values
as genre compatibility without verifying their uses. C#/Python parity checks
every imported field, example title and table value for all 51 records.

### Quote

`fGMKaihatu` calculation procedure 0, start `00986088`, computes:

- Cost = hardware member FFC + audience member 148C.
- Original mode 2 additionally adds the selected member 408 (`A11424`);
  its wider meaning remains unresolved, so the API requires a supplied value.
- Points = hardware member FE0 + audience member 1454.

The confirmation handler, start `00988A34`, first compares target hardware bits
(`A11F1C`) against current computer bits (`A11A64`) at `+0020..+0034`. Equality
passes. It then checks the computed cost against funds at `+00DA`. It invokes
the planning meeting rather than charging immediately. Acceptance is a separate
`fGMKikakusyo` branch, so the quote API does not mutate company funds or start an
invented development timer.

The planning form (`0099BA80`) also contains sequel carry-over arithmetic,
project initialization and two-room state. Those paths are traced privately but
are not implemented until field meanings and scheduling are verified. Source
trace dumps, original strings and copied assets are excluded from the source archive.

## M48 sequel continuation

The mode-1 acceptance and automatic checkpoint branches are now connected.
See [sequel reconstruction](sequel-development.md) for eligibility, exact 30%
carry-over, inherited type/genre points and the shipment counter quirk. Mode 2
remains unresolved and unavailable.
