# M12 — Employee desk assignment and office presentation

This checkpoint connects hired employees to the twelve development seats in the
original office. Staff can be assigned to either room and sent back to the lounge.
It does not yet start or complete a game-development project.

## Static evidence

Evidence is from the supported local executable, not a Windows runtime comparison.
No original graphics, catalog text or whole disassembly is distributed.

| Procedure and offsets | Recovered behavior |
| --- | --- |
| Module2 10, `009AF6BC +01AF..01EA` | Reject the current seated action and occupied target; occupancy array uses -1 for a free desk |
| `+02E8..0375`, `+061D..0674` | Release prior lounge/desk reservation and reserve the new development action |
| `+0684..071E`, `+0980..0991` | Clear seated action, visual offsets, work engagement and work counters; retain base coordinates and current route node; enter route state 3 with action counter zero |
| Module2 11, `00A0283C +0A7D..2358` | Fourteen-node routes to selected actions, including office detours |
| `+27C3..2E98` | Final approach coordinates, facing and transition into sitting state 4 |
| `+3311..3789` | Work-seat frames/offsets at ticks 0, 4 and 6; seated transition at tick 8 |
| `+3675..3788` | Store seated action, shift base position by 8 horizontal and 4 vertical pixels according to facing, set movement 0 and work engagement 1 |
| `+2362..27AE` | Assigned computer position and atlas rectangle, including model stride 74 |

The action helper does not animate an invented reverse standing sequence or
teleport the employee to a new approach point. It clears offsets and walks from
the existing base position, including the shift made when sitting at a desk.
The seated pose is retained until work scheduling is connected; ordinary work
and debugging are not silently substituted with lounge recovery.

Actions 6..11 belong to development room 0; actions 12..17 belong to room 1.
The UI presents these as rooms 1 and 2. Training actions 1..5 remain unavailable.
The UI seat buttons are community-authored controls around the imported office;
the original pointer/drag interaction and selection overlays remain to be ported.

Break requests reuse the recovered fourteen-draw lounge search. A failed search
does not change the current assignment. Occupied desks and unchanged assignments
also leave state untouched. The modern result messages explain these cases.

## Imported computer tiles

Initializer `009F651C +42A9..49F0` associates twelve allocator results with actions
6..17, in the imported scene's IDs 93, 95, ... 115. Each starts at (-1000,-1000),
size 46×35 and depth offset -200. These structures come from the local importer.

The main loop changes destination X/Y and atlas X/Y for an assigned action;
it does not write the original stored depth key in that block. The presentation
projection preserves that allocation key when changing Y. Computer tiles become
visible before the final draw filter, and disappear when an action is released.
Atlas X is 571 or 608 plus 74×computer model; atlas Y is 228 or 318 according to
desk orientation. Training-state row offsets are not synthesized.

Chair member 252C is toggled around sitting, but is not read by the reconstructed
draw loop. It must not be assumed to be renderer visibility. Selection/hit-testing
and chair overlays require their own tracing; the current renderer retains the
imported static chair artwork.

## Save and test coverage

Modern saves add optional `SeatedAction` and `WorkEngaged` fields. Older lounge
saves retain their default zero/false values. Validation rejects conflicting
development reservations, inconsistent seated states and positions off the
original half-grid. A loaded employee resumes the saved route/pose.

Tests exercise all 144 lounge-to-desk-to-lounge combinations, all 132 distinct
desk-to-desk transitions, intermediate poses, atomic failed assignments, original
tile import and model selection, and save/load during walking/sitting.
The real Godot UI check assigns a worker, waits for the office timer, saves and
reloads the seated worker, checks occupied-seat controls, and returns them to rest.
Private composed frames were inspected; they are not distributed.

## Remaining integration

- Ordinary/debugging scheduler, burst cancellation guard and work animations.
- Meeting, training, polishing and project-completion transitions.
- Weekly economics, release/reviews/sales and progression.
- Original pointer interaction, bubbles, selection overlays and chair input flags.
- Original Windows behavioral comparison and original RNG consumption.

Active projects remain unavailable through the UI until their complete loop is
connected. No unknown prices, growth, release results or sales formulas are invented.
