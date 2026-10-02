# Original presentation recovery

The intended product is the full PC game's office and development loop in Godot,
retaining the original presentation where practical. The previous M9 screen was
a systems prototype, not a faithful game interface. Neither M9 nor this office
asset correction represents a completed port.

## Verified local graphics

The supported executable contains serialized VB6 form bitmap files. They are
not entries in its PE bitmap resource directory. Validating bitmap headers and
decoding the images confirms these file offsets:

| Offset | Dimensions | Local import name | Observed content |
| --- | --- | --- | --- |
| `0x3B5DC6` | 466 × 290 | `OfficeBase.bmp` | Isometric office floor, walls and fixed background decorations |
| `0x3D7469` | 1600 × 530 | `OfficeSprites.bmp` | Office and character sprite atlas |
| `0x4A6960` | 1600 × 530 | `OfficeMasks.bmp` | Corresponding black/white sprite mask atlas |

The importer validates the existing supported executable hash before recovering
these files into `user://original/Graphic/Embedded/`. No bitmap bytes or original
executable are distributed in source. Existing caches require reimporting.

The office screen displays the original background with nearest filtering and
keeps the implemented management actions alongside it. The importer is opened
when New Game is selected without original data, rather than entering a company
with no database and disabled hiring.

## Still unresolved

The background is only the base layer. It does not implement a populated office.
Sprite source rectangles, masking/compositing, isometric positioning, depth,
employee-slot placement and work animation must be recovered before rendering
those layers. Initial staff remain unverified. Development, reviews, shipment,
sales, technology, hardware development and progression remain incomplete.

Module1 ordinal 67 (`0x00987E78`) allocates an unused tile and writes seven
arguments into members `2484`, `24A0`, `24BC`, `24D8`, `24F4`, `2510`, and
the depth-like `23DC`. The common initializer invokes it repeatedly. The exact
rendering interpretation needs tracing; the argument mapping alone is not
sufficient to invent office placement.

Validation includes compilation and a real Godot image-decoding check for the
466 × 290 texture, plus the existing imported-data management/save checks.
This execution host has no graphical display or Xvfb; headless checks do not
establish visual parity or usability. Compare a rendered Windows/Linux build
against the original before claiming either.
