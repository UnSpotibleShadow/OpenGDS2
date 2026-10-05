# M53: manual secretary advice

Click the secretary in the original office, or choose **Ask Secretary** from
Company or Settings. The compact conversation reports the original advice in
source order. It charges no fee and performs no purchase, training, recruitment
or financial transaction. English is authored community translation; Japanese
comes only from the user's local original import. Reimport the required LZH to
obtain the schema-5, 106-entry dialogue cache. Earlier dialogue caches remain
readable, but lack the new Japanese pages and display the reimport notice.

## Verified static behavior

These findings come from targeted P-Code/data-flow analysis of the supported
executable, not original Windows gameplay comparison. The public verifier retains
bounded instruction predicates, branch destinations and setter mappings; original
text, complete procedures, catalogs and artwork remain private.

`fMainVisual` at `009C66BC +071F..0741` tests the inclusive native rectangle
x=104..114, y=80..104 and calls Module1.46 (`009B15F0`, thunk `0060EDE0`).
Godot converts clicks from the displayed texture back to native pixels before
this test. The original settings form also calls the same routine; the modern
Company/Settings entry is a convenient alternative to the office click.

| Advice | Source offsets in 009B15F0 | Original condition |
| --- | --- | --- |
| Develop a game | +0000..0083 | Room-state member 16F0 is zero in both rooms |
| Hot-spring tenant | +0084..0166 | None of three 0444 slots is 1; cash >= 04B4[1]+60 million; floor(Rnd*2)=0 |
| Buy technology | +0167..0371 | Count ownership 14F0 >=1; ordered tiers cash >=150/250/350 million with count <=0/2/4; one eligible floor(Rnd*2)=0 |
| Contracts next month / this month | +0372..042F | Month 2 / month 3, respectively |
| Contest | +0430..04A5 | Month 12 and week <=3 |
| Hardware licenses | +04A6..0648 | Count hardware with 0F1C=1 and 0F8C=1; unconditional floor(Rnd*2) draw; on zero, separate advice for counts 0, 1 or 2 |
| Upgrade computer | +0649..0816 | Current numeric bits 8/16/32 and hardware-stage EB4 >=1/3/5; one eligible floor(Rnd*2)=0 |
| Excess inventory | +0817..08EB | Sum 1E5C for products whose 1DEC=2; total >=200,000; floor(Rnd*2)=0 |
| Funds assessment | +08EC..0A74 | Sum contract fees 0BC0 for employed-gate 0C84=1; cash <=50 million takes precedence, otherwise cash < contract total warns, otherwise healthy funds |

Tenant setter `0097D918 +0014..0026` maps argument 14 to price member 04B4.
Initializer `009F651C +9999` writes 60 million for source 1, so the advice
threshold is **120 million**. This is tenant acquisition advice, not the existing
8-million hot-spring outing. Tenant purchase effects are still unresolved.

The tiered technology and computer tests are ordered branches: a failed random
roll does not fall through to another tier. The license draw happens even with
three or more active licenses, when it produces no message. The other optional
rolls occur only after their eligibility condition passes, in the order above.
Modern `Random.Next(2)` preserves branch count/order; full VB6 seed and global
random-stream equivalence remain unverified.

The stock predicate reads the product status array, not adaptation mode. Module1
public storage base A1101C maps member 1DEC to A12E08 and 1E5C to A12E78.
Supported shipment `009C9CB8 +0FBC..0FD6` copies inventory into A12E78 and writes
2 into A12E08. All current own releases therefore contribute to this advice sum.
Future external jobs, product states or inventory disposal must extend this
projection explicitly; they must not silently be included as own shipments.

All preliminary message calls have presenter argument 28=0, queuing pages. Each
of the three exclusive final funds calls has argument 28=1, presenting the queue.
The December condition is a manual advice page; it proves no automatic reminder
schedule. The routine contains no additional fee or action transaction.

## Presentation and persistence

Fresh campaigns with the supported imported catalog now save the verified initial
secretary source index 1 (initializer +C0F9..C0FE). M44's five portrait mappings
are 1, 2, 4, 5, 6; record 1 therefore uses portrait 2. Presenter Module1.62 resolves
selector -2 through current secretary 0890 and portrait table 0938. `fGMKaiwa`
Form_Load `00998AB0` loads Human{portrait}.bmp and its .1/.2 mouth frames.
Godot uses those locally imported portraits and the existing 20 ms reveal cycle.
The speaker caption remains the localized generic Secretary; actual secretary
names and name readings are still unconnected. Original focus, exact placement
and automatic timed-dismissal behavior remain unfinished.

Modern saves keep stable message IDs and a read cursor in PendingSecretaryAdvice.
Reopening/loading resumes the unread page without rerolling. The office clock,
sales, calendar progression and recruitment remain blocked while advice is unread.
Reading a completed page advances once; the last page clears the queue. Close or
Enter follows the existing reveal/advance behavior. Saving, loading and language
settings remain available. No original queue/save-format compatibility is claimed.

Old modern saves retain null secretary identity, so their advice has a generic
speaker without an inferred portrait. Missing cash/date/calendar, technology,
computer, license or contract-fee data prevents preparing advice and displays an
explicit unknown-data notice. Loading never fabricates those projections.

Secretary recruitment, salaries/contracts, policy defaults and automation are not
implemented by this milestone. Neither advice nor its recommendations unlocks
those systems.

## Validation

Fifteen added public C# cases cover inclusive click boundaries, each numeric
threshold, message and random-draw order, active-license intersection, manual
calendar conditions, contract precedence, initial/legacy secretary identity,
unknown and modal states, malformed queues, overflow-before-draw behavior,
save/load without rerolling, and frozen clocks/recruitment.

Public Godot checks click the actual scaled office surface, display unknown-data
advice, request advice with authored company fixtures, save/reload midway while
paused, and verify no fee or clock movement. Private Godot checks import all
fourteen Japanese pages, use Japanese dialogue and compare secretary mouth frames
with local Human2 artwork. Existing development/release/sales, sequel and manga
regressions remain green. Actions stay paused; no new exports or native Windows
validation were run.
