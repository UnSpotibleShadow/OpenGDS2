# M61 occupied facility artwork

Opening the school now replaces the vacancy sign with its original occupied-plot artwork. Closing it restores the empty plot immediately. Rendering follows the three saved facility slots, so modern save/load retains the same appearance. The three slot sprites remain separate from facility type; construction does not change accounting or unlock unsupported services.

## Source evidence

The initializer at `009F651C +52C8 +slot*54` stores IDs 146–148 into member 0460. Module2 pools 139/13A point to A11460/A1147C (facility types/member 0444 and sprite IDs/member 0460). Periodic projection at `009E3830 +0594..07A6` reads those arrays, hides an empty slot at X −1000, and selects the occupied position and atlas rectangle.

| Slot | X | Y |
| --- | --- | --- |
| 0 | −16 | 130 |
| 1 | 64 | 170 |
| 2 | 320 | 170 |

All nine source types use Y 409 and the imported 162×111 rectangle. Source X by type 0–8 is 1, 163, 325, 487, 649, 811, 973, 1297, 1135. Type 0 is the existing school. The periodic block does not write drawing depth; projection preserves the allocated sorting key 500. Unknown calendar state or unsupported types remain hidden. Other facility services still require independent evidence for costs, prerequisites and effects.

`ShowFacilities` refreshes the office texture after the existing open/close transaction. No economics, random draws, original data or native engine files are changed by the facility logic.

## Validation and package

Direct app compilation, 295 domain checks, the full fixed-60-FPS core loop, original-LZH UI suite, bounded source-evidence verifier and diff whitespace checks pass. Tests cover all three school positions, open/close, modern save/load, nine source columns and unknown calendar state. Original-data image comparison detects occupied pixels only inside the clipped plot rectangle and confirms exact return to the empty image.

The owner requested a Windows development package after this area. The packaging fallback verifies every reused PCK entry, updates the assembly and translations, includes four meeting OGGs plus descriptors and imported engine resources, and rewrites the PCK entry count. It records source SHA and assembly hash in BUILD_MANIFEST.json and file hashes in SHA256SUMS. This reuses the authenticated M47 Windows engine/.NET runtime and older imported community audio; it is not a clean source export. Native Windows launch remains untested on this Linux host, and Actions remain paused.

Track ongoing work in [GitHub backlog #55](https://github.com/UnSpotibleShadow/OpenGDS2/issues/55).
