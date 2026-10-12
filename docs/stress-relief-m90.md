# M90: Level-three stress relief follows actor slot 0

Implemented 2026-10-12 using [M76 technology tracing](workflow-verification-m76.md), original `fGMInfo 009A98E8 +0385/+064A..06BE` and the documented VB6 Empty-frame initialization premise. Original binary identity and asset/privacy boundaries are unchanged.

The selected employee pays the recovered level-specific experience cost at +0385. Levels one and two retain their selected-person 50%/80% reductions and flooring. Level three's separate branch reads unassigned local -02B4, uses it to index actor-to-source map `A11E38`, and writes stress zero to that source at +06BE. Under M76's documented Empty/numeric-zero premise, this is office actor slot 0, not employee source index 0 and not necessarily the selected employee.

The port resolves that mapping through the known source-indexed employee whose `Office.SlotIndex` is 0. It clears that person's stress and synchronized source record, spends experience only from the selected employee, and preserves the selected person's stress when they differ. It creates no delayed project application. The use screen names the actual recipient before the action.

If actor slot 0 has no known source mapping, the modern UI disables the action before consuming experience. This is an explicit protective boundary: the native code would attempt its array access after charging experience; the port does not invent a substitute recipient or emulate an invalid native access. A direct write can resolve an unknown target stress to zero without assuming its old value.

Five new domain checks and the revised lower-level regression bring the optional-original suite to 459 checks. They verify distinct selected/recipient identities and costs, list/source-order independence, source synchronization, unknown mapping, direct null-stress resolution and saved results. Clean build and M76's 44 procedure fingerprints/35 payout entries pass. English/Japanese smoke checks cover recipient disclosure, actual effect, experience cost, reload and the unavailable-mapping boundary.

This adopts the stated runtime premise; it is not a native Windows reproduction. Speed technology remains unavailable until its separate 50/100/150 counter and continuous-loop retained eligibility are integrated. Original save importing, conversation scheduling and complete payroll scratch writers remain separate fidelity work. RC.4 is unchanged; final source export remains needed.
