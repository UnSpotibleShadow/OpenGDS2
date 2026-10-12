# Artwork families and local quick reference

The 1600×530 image shown in chat was the **office sprite atlas**, not all game
artwork. The owner's device and staff examples are separate resources.

| Family | Local import location | Status |
| --- | --- | --- |
| Office background | `Graphic/Embedded/OfficeBase.bmp` | 466×290 base scene |
| Office sprite atlas and mask | `Graphic/Embedded/OfficeSprites.bmp`, `OfficeMasks.bmp` | 1600×530; room/employee/desk/environment pieces |
| Target device pictures | `Graphic/Embedded/Hardware0.bmp` through `Hardware14.bmp` | Fifteen separate 64×64 ImageHard resources |
| Staff portraits | `Graphic/Human/Human1.bmp` through `Human10.bmp`, plus `.1.bmp`/`.2.bmp` frames | Archive files; not tiles on the office atlas |
| Meeting/completion | `Graphic/Embedded/PlanningMeeting.bmp` | 256×160 ImageFormG[0] |
| Proposal document | `Graphic/Embedded/PlanningProposal.bmp` | Separate 256×160 fGMKikakusyo Picture1; M95 imports it |
| Toolbar icons | `Graphic/Embedded/Toolbar0.bmp` through `Toolbar15.bmp` | Separate embedded control pictures |
| Exhibition and awards | ImageFormG[26]/[4] | Known single-frame GIFs; M93 identifies them, local import/render support still pending |
| Reviewer portraits/names, score and reaction images | Serialized fGMCrossR controls | Separate images; resource extraction/presentation still pending |
| Other archive graphics | Imported `Graphic` files | Local index lists whatever is present without guessing their purpose |

For a quick visual reference of **all currently imported images**, run:

```sh
python tools/local_artwork_reference.py --cache /path/to/original --output /private/path/artwork-reference.html
```

Open the generated HTML locally. It groups the separate resource families and
links to original-size images without embedding or copying original pixels.
The cache is the game's `user://original` folder under its application data
directory. Keep generated references private and outside this repository.

Issue #97's later exhaustive review must inspect both the office atlas and the
separate resource families, including serialized controls not yet imported.
An index of the current cache does not prove that every original image has been
recovered, or that every animation/frame has been implemented. Issue #95 stays
open for the native walkthrough's presentation findings.
