# M29 — catalog translations and toolbar images

## Display translations

English previously translated the interface but displayed most imported catalog
names directly in Japanese. English now uses the supported executable's stable
source indices to select community translations. Japanese uses the original
locally imported names. Neither language changes the catalog, saved employee
names, IDs, fees, probabilities or simulation state.

| Catalog | English entries | Source |
| --- | ---: | --- |
| Genres | 18 | Locally imported genre records |
| Game types | 51 | Locally imported type records |
| Audiences | 7 | Locally imported audience records |
| Hardware | 11 | Locally imported platform records |
| Development computers | 4 | Locally imported computer records |
| Technologies | 8 | Existing translations, now using the same guarded display path |

These are unofficial translations, not recovered official English names. Hardware
and computer labels are transliterations of fictional names. Wording such as
"American football" distinguishes the separate football and soccer entries;
"Drill" retains an ambiguous source term rather than asserting its subject.
The supported executable hash gates the index mapping. Unmapped entries fall
back to their local names. The Japanese PO includes matching catalog keys with
empty translations because their names come from the user's import.

Employee proper names and names generated from the imported family/given-name
pool remain local Japanese names. Their readings have not been established.
No guessed employee identities or original name tables are bundled. Text embedded
in original bitmaps also remains untranslated unless the user supplies a local
artwork override. This milestone does not claim complete localization.

The import dialog, bit-width labels and employee level labels are localized too.
The music settings explain bundled tracks and local playlist precedence.

## Toolbar

The sixteen original 23 × 22 bitmaps are extracted locally from the required LZH.
Existing imports made before M25 may lack them: reimport the same original LZH
to refresh the cache. The empty-project status bar reports missing artwork.
Original bitmaps always take priority over authored fallback pictograms.

Every action now has an image, including Settings, Import and Pause/Resume.
Fallbacks contain shapes rather than initials. Tooltip labels and the bottom
status bar identify actions in the selected language; keyboard focus shows the
same help. Pause/Resume updates both its image and label immediately. Nearest
filtering preserves the original toolbar pixels. This does not establish the
unresolved correspondence between every original icon and VB6 click handler.

## Validation

Local direct C# compilation passes. All 27 Python checks pass with the private
original fixture (19 public checks; eight optional original checks). Public checks
verify unique/matching localization keys and all 91 new English catalog entries.
The imported-original Godot UI fixture reports zero failures and verifies all
sixteen local toolbar bitmaps plus the English platform/audience labels.
The complete core-loop fixture reports zero failures, checks every toolbar image
and tooltip, status help, type/genre labels, English/Japanese switching, and that
catalog names are unchanged on disk. Extracted desktop validation is tracked in
`validation.md`. Full graphical comparison remains outstanding.
