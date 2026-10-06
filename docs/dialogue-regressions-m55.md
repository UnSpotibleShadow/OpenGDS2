# M55 dialogue persistence and window positions

Calendar announcements now use the recovered modal dialogue calls, with saved
page cursors. Two-page retirement notices resume their current page after load;
loan repayments debit the account only after the final page is acknowledged.
Graduate hiring saves the greeting before advancing to the next candidate, so
loading cannot hire or charge again. Older transactions without a dialogue
cursor retain their existing direct controls rather than replaying a newly
introduced announcement. Reimport the original LZH for the schema-7, 136-entry
Japanese dialogue cache.

The reported window-position reset came from recreating each management window
and applying the default compact-window position on every page. The desktop now
remembers the shared position of dialogue, quote, proposal, type/genre meeting
and manga-rights windows. Meeting illustrations remember their own position.
Both survive page changes, subsequent conversations and desktop recreation
within the running application. These are UI preferences, not game-save fields;
they reset when the application exits. Restored positions use the existing
workspace bounds so a smaller window cannot strand the caption offscreen.

The synthetic core-loop fixture verifies each hardware announcement, both
retirement pages, loan amounts, modal controls, save/load cursors, single debit
and absence of replay. It moves dialogue and meeting illustration windows,
checks subsequent pages/meetings and reload, and shrinks the desktop to check
position clamping. Its frame-based accelerated phases require `--fixed-fps 60`,
as in the retained validation workflow. Running those phases with an uncapped
frame rate can reach a control before the weekly simulation has advanced.

Local validation: 283 domain checks, the complete headless core-loop suite and
the original-LZH UI smoke suite pass. Two changed local imported music caches
were repaired from MD5-verified artifact entries; the Windows package already
contained the verified copies. These are host-side checks, not native Windows
execution.

The Windows development package replaces the game assembly and translations in
the authenticated M47 debug artifact. Its Windows engine, .NET runtime and 21
imported community audio streams are preserved. It is not a clean Godot export.
The twelve exact seasonal source OGG containers remain missing locally; their
existing GitHub blobs and manifest are preserved. The local source-integrity
gate still fails. Engine-ready audio restoration
does not establish source-file restoration.

The separate automatic timed presenter and capped-quality polishing entry
remain unproven and disabled. GitHub Actions remain paused.
