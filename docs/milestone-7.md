# Milestone 7 — employee import, recruitment, funds, calendar and saves

This is a larger reconstruction milestone, **not a completed game**. It implements
an interactive recruit → hire → inspect staff → save → load loop. Development,
sales, awards and full weekly processing remain disabled rather than invented.

## Opening-state corrections

Designer records immediately preceding the `fGMGameKaishi.Command1` declarations
encode **Normal=0, Beginner=1, Cancel=2**. M3 only verified comparisons 0/1;
its prose presented the modes in display order and did not resolve the indices.
The new evidence tool checks the serialized index bytes and captions directly.

The common initializer at `0x009F651C` stores **450,000,000** into funds member
0x20. It stores year=1, month=4 and week=1 into members 0x40/0x50/0x60.
The Beginner branch of `fGMGameKaishi.Command1` overrides funds with
**1,200,000,000** at `+0x014C`, after the company-setup modal returns. Normal
branches around that override. M7 uses these static values.

Do not mistake the later unreachable/debug initializer block at `+0xC14A` for
opening state: an ExitProc at `+0xC149` precedes it.

Company recognition starts at 12% and rank at 0 (Small company), verified by
initializer/reset, information-form pools and matching caption/value coordinates.
Development power and the documented initial employee are still not binary-verified. A new reconstruction session has no synthesized employee. This
is an acknowledged missing initial-state behavior, not a claim that the original
starts with zero staff. The secretary/one-employee historical account remains
corroboration; it is not substituted with invented identities.

## Calendar

`Module1` procedure start `0x009DB4F4`, size 7,180:

- `+0x01AE` increments week.
- `+0x067D` tests week >=5 and resets to 1 while incrementing month.
- `+0x0715` tests month=13 and resets month/week to 1, incrementing year.

There are four weeks per month and 48 per year. The UI exposes a separate
calendar inspector without mutating the company calendar, because the same
original routine also processes finances/events/contracts that remain unresolved.

## Implemented

- Sixty employee source definitions imported from the verified distribution.
- Eight name-component pairs, fourteen generated-name records; seven named stats.
- Source bounds 35..59, employed gates, five-slot roster reconstruction on reopen.
- Hire affordability, fee charge, employee identity, independent snapshots and gate update.
- Explicit modern guards against duplicate hires, negative fees, malformed data and stalled RNG.
- Versioned JSON saves, atomic write, bounded load, Japanese round-trip, collection restoration.
- English/Japanese settings and scrollable office/recruitment UI.
- Separate calendar rules; manual level-up with experience thresholds, reset, stat gains and fee increase.
- Eighteen genre records imported with unidentified numeric columns retained structurally.
- Isolated hot-spring recovery rules; tenant access remains disabled until unlock rules are known.
- LHA size/CRC checks and full archive extraction verification.
- Python evidence tests, C# domain regression suite, optional local data parity test, CI build/smoke commands.

## Limits

C# compilation, 20 C# tests, 12 Python checks and Godot headless UI tests passed
using a temporary environment workaround; see validation.md. Standard SDK/MSBuild
execution and graphical layout verification still require a normal host. No Windows/Wine behavioral comparison or original save fixture
was available. Original save compatibility is unsupported. C# checks must be run
on a normal .NET-capable environment before a release.
