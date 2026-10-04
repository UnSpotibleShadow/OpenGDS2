# Staff-name localization — M40

English uses 46 fixed employee readings and eight family/eight given-name
readings for the independently generated employee names. These are authored,
unofficial community romanizations, not verified phonetic annotations from
Kairosoft. Japanese personal-name readings, especially fictional/pun names,
can be ambiguous; all fixed and given-name readings are provisional pending
kana or other original evidence. They may be corrected without changing saves.
Family/given order follows the imported name. Unresolved pronunciation does
not change employee attributes or recruitment rules.

## Identity and fallback

`EmployeeNameLocalization` matches the supported imported database hash and
source index, then compares the saved original name with its imported definition.
For generated names it resolves the actual family/given combination against the
locally imported components. It never assigns a fixed alias to a random slot.
Whitespace is normalized only for matching; original strings are preserved.
Unknown releases, unmatched/custom names, absent database and absent translations
fall back to the original name. Japanese bypasses readings and shows local text.

The English PO contains only authored Latin readings, indexed by source identity.
The public repository contains no decoded original Japanese name table.
The Japanese PO contains matching fallback keys; Japanese names come from the
required local original import. No name or source ID is rewritten in a save.

## Coverage and validation

Recruitment, staff table, employee window/title, graduate/training cards,
desk occupancy, contract renewal, dismissal, stress and salary notices share
one display helper. Public domain tests cover save round trips, custom names,
missing translations, unknown hashes and all 64 generated combinations using
synthetic names. Python checks enforce translation coverage/key parity.
Private imported-original UI checks require Latin recruitment names while
retaining the original database and original employee/save strings.

Graphical Japanese-font/IME and pronunciation review remain outstanding.
