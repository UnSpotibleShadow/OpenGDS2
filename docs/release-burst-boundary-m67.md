# Unfinished bursts at phase changes and release — M67 / issue #61

The owner's follow-up identified a gap in M66: another employee can reach a
checkpoint while a numbered burst is still running. M66 checked a burst that
itself crossed a checkpoint, not immediate shipment with another employee's
unfinished burst. Its result did not rule out this case.

## Result

Alpha/type, Beta/genre and Final/debugging transitions preserve unfinished work.
The checkpoint pauses the office; after its conversation/selection completes,
the pending burst resumes and commits normally. Continue Debugging also resumes
it. Quality remains credited even if progress has already reached its cap.
Type/genre choices can increase required points, reducing the displayed percentage
without subtracting the absolute points or cancelling pending work.

**Immediate shipment is different:** it snapshots completed quality/bug totals
and discards work that has not finished. A visible 37 Graphics counter does not
become +37 Graphics in the released game if shipment occurs before it commits.
This can explain the observation. The supported original's static data flow
supports this behavior; changing shipment to flush pending gains would alter it.
No gameplay formula or release behavior is changed by M67.

## Supported original evidence

Executable SHA-256:
`4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.

| Original source | Bounded verified fact |
| --- | --- |
| Shipment `009C9CB8 +01E9..024C` | Copies the four current room qualities and bugs into separate product fields |
| Shipment `+104D` | Sets the selected development room's state to zero |
| Module2 `00A0283C +4874..4888` | Tests that room state for zero before its empty-room cleanup |
| Module2 `+48D1` | Clears the worker's outstanding burst counter in that empty-room path |

`tools/verify_release_burst_boundary.py` verifies the supported hash, field maps,
copy instructions, room-empty write and counter cleanup. It emits no procedure
traces or original assets. Together with M66's verified completion-only commit,
these anchors support excluding uncommitted counters from the shipped snapshot.
They do not establish an original Windows runtime comparison or exact parity of
all worker animation cleanup timing. Modern cleanup occurs immediately on release
to prevent an old pending animation entering a newly accepted project; the source
counter-clear shown above occurs on the next empty-room worker pass.

## Regression results

Two-employee tests run through GameSession, not just the contribution formula.
The first employee completes small Speed work and triggers each of the three
checkpoints before the second employee's 37 Graphics burst can finish. The tests
verify clock pause, intact counters, checkpoint save/reload, actual type/genre
selection or Continue Debugging, then +37 Graphics exactly once on resumption.

A separate Final-boundary test immediately ships after reload. The product keeps
its completed 30 Graphics instead of receiving the unfinished additional 37;
its pending animation is cleared. Post-shipment reload and a new project receive
no old burst contribution.

Validation: 302 synthetic domain tests pass; the original release-boundary checker
and Python compilation pass. The existing nullable warning at Program.cs:1339 is
unchanged. No UI/runtime code changed, Godot check or desktop package was produced,
and GitHub Actions remain paused.

For manual Windows acceptance, trigger Final while another employee has a visible
numbered burst. Compare immediate shipment with Continue Debugging until the
burst finishes, then shipment. The latter should include its quality gain; the
former should not. This is a fidelity check, not a proposed extra release bonus.
