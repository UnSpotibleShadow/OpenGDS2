# Original toolbar and menu cleanup

Replace the port's mixed toolbar with the original icon sequence: Save, Load,
public relations, genre research, production expansion, hiring, technology
purchase, Bank, development computers, hardware licenses, secretary hiring,
employees, products, sales rankings and Help. The three UP commands now open
`fGMSeisanZ`-style confirmation directly and return to the office on cancel or
completion. The shared weekly budget and existing charge/result conversations
remain unchanged. No activity-overview detour is part of the native command path.

Evidence: the supplied original screenshot and `MDIForm1.BGraphicH`'s recovered
23x22 picture array establish the artwork and visual grouping. The serialized
source indices are 0,1,13,14,15,2,3,4,5,6,7,8,9,10,12; importer IDs are their
reverse (15 minus source index). Source index11 is an unused icon; it is not
assigned a speculative command. `MDIForm1 0099DB78` contains the toolbar click
routes to native forms and the shared three activity handlers. Artwork is locally
imported and is never committed. This corrects M25/M30's provisional mapping and
adds the missing UP/Bank/secretary/ranking/Help shortcuts.

Buttons use the native bitmap dimensions with no second surrounding skin or
invisible text taking layout space. Tooltip/status help stays localized. The
funds readout uses a recessed white field like the reference.

Remove Home, the duplicated planning shortcuts, the aggregate company-action
shortcut, import, settings, calendar inspection and pause from the toolbar.
Remove import/settings/calendar inspection, the port activity overview and the
duplicate planning command from in-game menus as requested. Import and application
preferences remain available before play on the title screen. Existing original
gameplay actions remain accessible through the menus/office; no port-only tools
were relocated into another in-game menu.

The old pause button was also a workflow-test control. The simulation pause state
now has a guarded engine API used by fixtures, with no hidden button, toolbar or
menu item. Tests activate actual menu commands when former port shortcuts are
removed. No new player-facing keyboard shortcut was introduced.

The original complete menu hierarchy has not been recovered in this change; the
existing translated gameplay menu groups remain. This change does not claim an
exact reconstruction of every native menu caption or accelerator.

Validation: clean app build; bilingual native-toolbar checks assert all15 icon
bindings/order and23x22 dimensions, stable hover, absence of removed menu tools,
three direct confirmations, cancel safety, shared weekly gating and reload without
repeat charge. Bilingual #112, management layout, presentation continuation,
financial failure and window/artwork regressions pass, as does the complete
headless core gameplay loop. No domain gameplay service was changed.
