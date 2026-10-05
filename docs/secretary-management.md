# M54: secretary replacement and policies

Open **Company → Hire secretary** to replace the current secretary, or
**Company → Secretary policies** to configure production and rest automation.
Both are also available through Settings. Reimport the required original LZH
for the five locally extracted secretary records and 109-entry Japanese dialogue
cache. The public repository contains authored translations and rule code;
original names, biographies, Japanese dialogue and portraits remain local.

## Verified static reconstruction

| Evidence | Behavior |
| --- | --- |
| Module1.26 `009814E4 +0000..0077` | Arguments map to name 08E4, rank 0900, contract fee 0970, advertised salary 091C, portrait 0938 and biography 0954; count 0880 increments |
| Initializer `009F651C +250B..268E` | Five seven-argument definitions; portraits 1,2,4,5,6 |
| `fGMHisyoK` load `009AC5B8 +042D..0488` | Walk source order and exclude the current secretary |
| Same `+0566..066D` | Rank codes 0/1/2/3 display S/A/B/C |
| Same `+066E..072A` | Display each row's contract fee and advertised salary |
| Hiring `0098ADF4 +0000..0087` | Reject fee greater than funds; debit the selected contract fee through the existing financial helper |
| Same `+0088..00EF`, `+00F0..0128`, `+0129..017A` | Old farewell, change current index/tile crop, then new greeting; each conversation is modal |
| Presenter `009933DC +00EE..0113`, `+01E1..0274` | Portrait selector -2 resolves the current secretary; name strip uses the named secretary caption |
| Settings confirm `009894DC +0000..0185` | Store the three radio groups in globals A118EC, A118CC and A118DC |
| Settings load `00991EC0 +0129..02A7` | Restore those radio choices |
| Initializer `009F651C +0050..0065` | Break and return globals both set to 2: stress 80 entry, stress 0 return |
| Main loop `00A0283C +0098..0109` | From budget tick 160, unused production budget reads policy 0/1 at funds ≥300m/100m; policy 2 is manual |
| Same `+B163..B25D` | Policy ≥1 enables automatic entry at stress ≥100/80/60; excludes the unsupported tier-5/tick-3 operation; writes flag and remembered action before routing to a lounge |
| Same `+B25E..B30D` | With automatic entry enabled, stress ≤40/20/0 clears the flag and requests the remembered action |
| Native assignment `009C66BC +1055..1111` | Work/study reassignment requires stress ≤entry threshold, unless voluntary; accepted requests clear automatic return intent |

The readable settings screenshot omitted the stress-40 return option in the
earlier feedback summary. The execution code establishes all three values.
M54 corrects M16's difficulty-based automatic production threshold: **the
secretary's policy controls it**, independent of Beginner/Normal mode.

No reset/initializer write to production policy 08D0 was found in the decoded
procedures; a fresh VB global Variant is Empty, numerically 0. Fresh supported
campaigns therefore start at 300m. This opening value follows the static global
initialization model, not an original Windows runtime observation. Starting a
new campaign in the same active session retains its known production selection;
break/return reset to their initializer values. Cross-title/process preference
persistence and original save compatibility remain unverified.

## Transactions, routing and saves

Replacement prepares all accounting before mutation, charges once, and persists
old/new source IDs plus Farewell/Greeting phase. The current identity remains old
until farewell acknowledgment, then becomes new before greeting. Save/load does
not charge again. Office/calendar/sales ticks and other company transactions
pause while either page is unread. Reopening a former secretary is supported;
rehiring the current one is excluded by the roster and domain guard.

Automatic rest retains the existing source route/sitting animations, stress
recovery and lounge-seat lottery. If all seats are occupied, the source still
sets its automatic flag; it does not reroll every frame. If the former desk has
been occupied, return clears the flag but does not steal the desk. The original
assignment helper ignores request 0. Voluntary policy disables both automatic
entry and return, while keeping any existing remembered action/flag. A manual
break clears automatic return intent.

Policies and per-employee automatic flag/remembered action are nullable in old
modern saves. Loading never fills them from guesses. Existing modern saves can
select all three policies explicitly; old employee workspaces with unknown rest
fields remain inert, while newly hired staff have known reset fields. Reimporting
adds the secretary roster without inventing a missing current secretary in a
legacy save. Unknown current identity blocks replacement.

## Localization and limits

English secretary names are **provisional community readings**, particularly
kanji-only personal names. They are display labels, not verified official
pronunciations. Japanese uses the locally imported names. Biographies, greetings
and farewell have authored English translations. Secretary identity changes
update the office sprite crop as well as the conversation portrait/name strip.

The recruitment form advertises salary member 091C. The recovered procedures
reference it for display, definitions, reset and save serialization; no recurring
secretary debit or annual secretary renewal was established. M54 does **not** add
one. Staff payroll and employee contracts retain their separately reconstructed
behavior. Any remaining original indirect salary/renewal caller must be traced
before it can become a simulation transaction.

Exact original window geometry, radio ordering, focus, automatic timed
conversation presentation, VB6 RNG stream equivalence, unsupported external
jobs/tier-5 work and native Windows audiovisual comparison remain open.

## Validation

Local checks cover paid replacement accounting, both persisted phases, old/new
portraits and sprite crops, rehire/current-secretary guards, malformed queues,
overflow-before-charge, modal clock/recruitment guards, policy/difficulty
independence, inclusive stress thresholds, voluntary behavior, full lounges,
occupied former desks, manual assignment and legacy unknown state. Godot tests
exercise the actual selection and policy controls, save/load during both pages,
biography translation and cancel-without-change behavior. Private checks extract
all five original records, Japanese captions and portraits locally.

GitHub Actions remain paused. No new desktop export or native Windows validation
is claimed.
