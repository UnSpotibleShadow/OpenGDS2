# Television artwork correction

The previous English promotion patch treated part of the TV cabinet as the
screen. Its fill continued to y=108, below the original CRT glass, and its flat
lettering did not follow the screen's perspective.

The replacement projects a small authored title card into an inset quadrilateral
measured from the supported original `Promotion2.bmp`: (85,33), (176,40),
(173,92), (88,84). The diagonal red background uses an unlettered strip of the
native screen. The original bezel, cabinet, stand and room pixels are retained.
The patch still requires the exact supported RGB fingerprint and dimensions;
original artwork is generated locally and is not distributed in this repository.

Only Television uses the new `television-screen-v2` English artwork cache.
Existing installations therefore regenerate the corrected TV automatically,
without reimporting or invalidating other scene patches. Japanese artwork and
explicit artwork overrides retain their existing precedence.

Validation:

- Domain regression checks every pixel outside an independently measured glass
  polygon, including cabinet locations overwritten by the previous patch, and
  verifies that the source buffer remains untouched and wrong dimensions fail.
- `tests/television_artwork_smoke.gd` opens the real TV promotion scene in English
  and Japanese. With private original artwork it compares every pixel outside
  the glass, requires byte-identical Japanese pixels, and verifies the rendered
  English texture comes from the revised cache. It also exercises an explicit
  TV artwork override. Private artwork and rendered captures stay outside git.

This is a correction to the authored English patch, verified in the adaptation;
it does not claim a successful original-game runtime comparison on this host.
