# Milestone 2 — New Game flow reconstruction

This milestone replaces the M1 placeholder with startup UI recovered directly from the original VB6 executable.

## Recovered forms

### `fGMGameNew` — New Game / company setup

Recovered from the executable near file offset `0x002357A0`.

Observed controls and captions:

- Form caption: `新しいゲーム` — New Game
- `Text1` — company-name input (blank in the serialized form)
- `Text2` — president-name input, serialized default `伊集院 光の助`
- Label: `会社名` — Company Name
- Label: `社長名` — President Name
- Button: `決定` — Confirm
- Button: `キャンセル` — Cancel

The Godot screen mirrors these semantic fields, not the old fixed-pixel VB6 layout.

### `fGMGameKaishi` — Game Start

Recovered from the executable near file offset `0x002301AC`.

Observed controls and captions:

- Form caption: `ゲーム開始` — Game Start
- Button: `初心者モード` — Beginner Mode
- Button: `通常モード` — Normal Mode
- Button: `キャンセル` — Cancel

The original behavioral differences between Beginner and Normal mode have **not** yet been asserted. OpenGDS2 stores the selected mode but deliberately stops before inventing rule differences.

## New domain structures

- `GameMode` (`Beginner`, `Normal`)
- `CompanyState.PresidentName`
- `GameSession`, which owns the active reconstructed game state independently of Godot UI nodes

## Next reverse-engineering target

Trace the transition after `fGMGameKaishi` into the main office state and recover verified initial values/rules:

- starting cash
- initial company rank/fame
- calendar/date state
- first secretary/tutorial event
- employee/hiring gate
- beginner-vs-normal differences

Until those are verified, M2 intentionally stops at a reconstruction boundary screen.
