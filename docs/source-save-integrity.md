# M31 — integrity of saved employee source records

Modern saves include inactive employee source records so dismissal, recruitment
and rehiring retain progression. Validation previously checked finite/nonnegative
experience and stress and levels 1–10 for active staff and applicants, while
source records had only partial validation. A malformed inactive record could
therefore reach later recruitment.

Apply the same existing employee-record checks to all three collections before
serialization and after deserialization. This is modern-format integrity, not a
new original-game rule. Nullable unknown progression remains allowed; no missing
value is initialized or inferred. Valid original imports retain their values.

Four public regressions cover negative/nonfinite source experience and stress,
invalid source levels, malformed loaded source progression, refusal before
overwriting a readable slot, and unchanged nullable legacy values.
The local execution environment disconnected after M30's local checks; M31 is
validated using standard SDK/Godot and desktop fixtures in GitHub Actions.

The [M31 workflow](https://github.com/UnSpotibleShadow/OpenGDS2/actions/runs/37146196040)
passes 134 public C# checks, 19 public Python checks, the normal SDK build and all
Godot/editor/packaged desktop fixtures. Optional original-data reruns remain
pending until local execution is restored; full original-save compatibility is
not established by these modern-format checks.
