# Reception sprite artifact investigation — M41

Private defect reference `image(9).png` shows a pure-red vertical line at the
front receptionist's edge and a small blue mark beside the computer. Its
captured build/cache/override state is unavailable. The red selection arrow
above staff is a separate verified feature and is not removed.

Checks on a fresh supported original import:

- The 466 × 290 office background contains no pure-red pixels.
- Reception sprites 130–134, 139–141 and all four computer-model rectangles
  contain no pure-red or pure-blue marks matching the defect.
- All ten employee atlas regions have no white-mask/nonwhite-color mismatch.
  Both white bytes preserve the destination under the verified raster pair.
- Portrait 5 contains legitimate colored face details; global color removal
  would damage original sprites. Static receptionist 139 uses portrait 1.
- Existing compositor and private Godot import/core-loop checks pass.

No source correction is justified by this evidence yet. Cache corruption,
local overrides, a different build or a displayed-frame artifact remain
hypotheses. No pixels/raster operations were modified. The defect stays open;
next step is reproducing the captured line with its build, cache and employee
frame, then comparing the composed bitmap and displayed texture. Original
images/atlas samples are kept private. Headless checks cannot prove graphical
rendering parity on the owner's desktop.
