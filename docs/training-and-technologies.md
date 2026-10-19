# Technologies and study — M17

All evidence below is static P-Code evidence for the supported executable hash.
Original Windows runtime comparison is still outstanding. Names, descriptions,
prices and level tables are imported locally; no original database is bundled.

## Recovered catalog and initial skills

Module1 initializer `009F651C +9DE3..A20B` calls technology setter `0098BA28`
eight times. Arguments `10..50` write the following tables at the current
technology count; the helper then increments that count.

| Member | Meaning | Evidence |
| --- | --- | --- |
| 161C | Numeric identifier | Setter argument 10; distinct from list scratch member 1600 |
| 14D4, 150C | Name and description | Setter arguments 14 and 50 |
| 1544 | Required training work, levels 1–3 | Study form and main-loop completion comparison |
| 15B0 | Training experience cost, levels 1–3 | Study acceptance subtracts the selected next-level cell |
| 158C | Technology application counter table | Use form copies selected learned-level cell to workspace 28DC; effects remain separate |
| 1568 | Technology application experience cost | Employee use handler compares and subtracts the learned-level cell |
| 1528 | Purchase fee | Purchase handler finance call |
| 14F0 | Company ownership level | Purchase sets 3; study list permits ownership greater than learned level |

Fresh employee skill cells at member `0D58` are cleared to Empty (numeric zero)
by `009DD234 +08E2..0918`. Initializer writes override skills for six special
source rows following their employee definition calls. Both importers recover
those writes structurally, rather than embedding their values in the project.

Employee argument 34/member `0D3C` is the maximum number of **distinct** learned
technologies. It is now exposed as `MaximumTechnologies`; the raw save field is
retained for compatibility. Existing modern saves lacking skills/ownership keep
those values null and cannot silently acquire training capabilities.

## Purchase and acceptance

`fGMTechKau 00986614` requires funds at least the purchase fee, subtracts the fee
once and sets ownership to 3. The selection handler `00984CEC +0000..0020`
disables rows whose ownership is already at least 1.

The study list `009ABC7C +0396..03F3` includes a technology only when its learned
level is below 3 and company ownership exceeds that learned level. This compares
source-indexed tables; member 1600 is the filtered list, not a definition ID.

Study acceptance `00993A2C`:

1. Requires experience at least the next-level training cost.
2. Counts all skills at least level 1. Subtracts one when upgrading a known skill.
3. Rejects if that adjusted count is at least the employee's distinct-skill limit.
4. Subtracts experience immediately, records the selected technology, clears
   progress and sets workspace task state 2.

Starting a replacement course repeats this transaction, including its experience
charge and progress reset. Reassigning a seat does not refund experience or clear
the course; it pauses study until the employee returns to a study seat. The UI
explains the replacement cost and supports resuming by returning to the office.

## Study tick

`00A0283C +40C9..4717` requires movement state zero, action 1–5, task state 2 and
stress below 100. Study seats use the same proven navigation/seating block as
workstations. The independent study idle counter increments before comparing
against `2 + (100 − developmentSpeed)/10`; this threshold is **not** multiplied
by the timing factor.

On reaching that threshold:

- Progress increases by `floor((floor(Rnd×5)+3)/2)` and idle resets to zero.
- The progress draw occurs before that tick's stress draw.

Every active study tick adds `(floor(Rnd×3)+1)/30` stress with timing factor 2,
capped at 100. Frames change at ticks 14 and 28 as in the original study branch.
At required work, progress is capped, the learned level increments and task state
returns to zero. Completion spends no additional experience and awards no
invented stat bonus. Skills remain in source records through dismissal, applicant
cloning and rehiring; workspace courses disappear when employment ends.

## Scope still incomplete

Technology **application** was not enabled in M17; M18 connects six timed project effects and stress-relief levels 1–2. See [application evidence](technology-application.md). The separate
employee use handler `009A98E8` establishes its costs and distinguishes immediate
stress relief, temporary speed boosts and timed project effects. Speed and top-level stress relief remain unresolved. Buying or studying a
technology does not itself apply a permanent stat bonus.

The 18 October #154 follow-up restores occupied study PC tiles and office click
routes from the original assignment hit regions and member-2218 sprite mappings;
see [review evidence](review151-followups-20261018.md). Course-progress artwork,
completion-message timing and native Windows behavioral acceptance remain
separate limitations. Other workstation task states and external jobs are not
established by the study-PC change.

The 19 October owner review adds direct course selection by clicking the occupied
study PC. The target is the PC's assigned employee, even while someone else is
selected. The employee-overview Training button is removed; empty research-area
space still opens the company research page. Opening/cancelling course selection
never charges experience. Confirming a course retains the existing eligibility,
cost and continuation rules. This PC shortcut is the requested port interaction;
it is not claimed as newly established native executable behavior.
