# Checkpoint choice presentation — M57

The next presentation pass addresses the example-title and selection-layout gaps
recorded after M52. Alpha/type choices now show the locally imported example
names in a separate column; Alpha and Beta both show fees in a separate aligned
column. Headers are inside the scrolling list, so scrollbar width cannot shift
headers away from their rows. The existing compact window size remains in use.

Choice buttons keep their full name and fee in a tooltip when a long name is
clipped. All imported example titles remain in source order and wrap within the
column, with a full tooltip; missing examples display an em dash. The first
choice receives keyboard focus and preview retains its radio selection, including
when invoked through accessibility or a scripted button activation. Unaffordable
or unknown funds permit inspection but keep Confirm disabled.

## Evidence and compatibility boundary

`GameTypeDefinition.ExampleTitles` already holds the three local strings recovered
from members 1324, 1340 and 135C by `OriginalCatalogImporter`. Original source
strings are read only from the user's import; none are committed here. English
and Japanese column headings are authored translations. Imported example names
are displayed unchanged in either language.

Showing all examples is an authored arrangement, not an assertion of exact
original form geometry or cosmetic random-draw parity. Rendering adds no random
calls. Saved candidate order and duplicates, fees, genre eligibility, meeting
queues, selection effects and save schema are unchanged. The original cosmetic
example draws, hover styling, timed presenter and cancellation/stress transaction
remain separate fidelity targets.

## Validation

Direct application compilation passes. The full fixed-60-FPS headless core-loop
suite finishes with zero failures, including example/source ordering, translated
headings, aligned fee cells, initial focus, selected preview, insufficient/unknown
funding guards, no preview charge, saved candidate stability and the existing
ordinary/sequel/manga and meeting-audio regressions. Both OGG integrity checks
pass. The first run found a corrupt generated Beta cache; regeneration from the
verified OGG resolved it. No audio source was changed. `git diff --check` passes.

No desktop package or native Windows run was created; Actions remain paused.
The feature branch builds on M56's verified commit while PR #50 is open.
