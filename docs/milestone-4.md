# Milestone 4 — employee hiring form and PC stat model

Milestone 4 opens the first reconstructed management action: **社員雇用** (employee hiring).
The executable is still inspected statically; no original Windows code is run.

## Important correction from M3

M3's roadmap treated `fGMSKoyouKz` as the new-hire form. The serialized VB6 designer data proves
that this was backwards:

- `fGMSKoyou` at file offset `0x00257B6A` has the window caption `社員雇用` (**Employee Hiring**).
- `fGMSKoyouKz` at `0x0025AA4E` has the caption `契約更新` (**Contract Renewal**).

The M4 implementation and roadmap now use those roles.

## Recovered hiring UI (`fGMSKoyou`)

`tools/scan_form_serialization.py` finds the form block and reads its 16-bit-length-prefixed CP932
strings. High-confidence designer strings include:

| File offset | Original | OpenGDS2 English |
|---|---|---|
| `0x00257B78` | 社員雇用 | Hire Employees |
| `0x0025814C` | キャンセル | Cancel |
| `0x00258191` | 雇用決定 | Hire |
| `0x0025826D` | 名前 | Name |
| `0x0025849E` | 開発速度 | Development Speed |
| `0x002584E6` | ひらめき度 | Inspiration |
| `0x0025887C` | ｸﾞﾗﾌｨｯｸ | Graphics |
| `0x002588C3` | 音楽性 | Music |
| `0x0025890B` | 正確性 | Accuracy |
| `0x00258953` | 意外性 | Novelty |
| `0x0025899B` | おもしろさ | Fun |
| `0x00258B34` | ﾚﾍﾞﾙ | Level |
| `0x0025A883` | 契約金 | Contract Fee |

This is a significant domain correction: the PC game does **not** use the later mobile
Game Dev Story employee schema that had been provisionally sketched in M1. `EmployeeState` now
models the fields actually shown by the 2001 PC form.

The serialized sample name/value strings (`はげ田はげ丸`, `Lv.1`, `66`, `99`, etc.) are treated as
**designer placeholders**, not as reconstructed applicant data.

## Hiring event map

Static VB6 metadata resolves these events:

- `fGMSKoyou.Command1` event slot 0 (the `雇用決定` button):
  - CodeInfo `0x009A0590`
  - P-Code start `0x0099FFC8`
  - size 1480 bytes
- `fGMSKoyou.Command2` event slot 0 (`キャンセル`):
  - CodeInfo `0x0097AE24`
  - P-Code start `0x0097AE0C`
  - size 24 bytes
- form event slot 6:
  - CodeInfo `0x009BB9B0`
  - P-Code start `0x009BABFC`
  - size 3508 bytes

The much larger load/form and hire procedures are the next P-Code targets because they are likely to
contain applicant population and contract-state transitions.

## Contract renewal evidence (`fGMSKoyouKz`)

The second form reuses the same employee detail fields but adds renewal-specific controls/text:

- caption `契約更新` (Contract Renewal)
- `Check1` control array
- `継続` (Continue/Renew)
- `資金` (Funds)
- `合計契約金` (Total Contract Fees)
- instruction `契約を継続したい社員をチェックしてください`

This makes `fGMSKoyouKz` a useful later target for annual payroll/contract logic, not initial hiring.

## M4 runtime behavior

OpenGDS2 now enables **Hire Employees** from the office screen and opens a modern reconstruction of
the verified hiring detail schema. Candidate names, numeric values, RNG and the actual contract
transaction remain unresolved, so the **Hire** button is intentionally disabled rather than filled
with invented data.

A historical walkthrough of the PC release reports that closing/reopening the original hiring window
can show a different applicant set. This is recorded as behavioral evidence only; M4 does not yet
implement the random applicant generator.

Sources used for behavioral cross-checks:

- Vector review (2004): https://www.vector.co.jp/magazine/softnews/040630/n0406303.html
- Sara-net walkthrough (2012): https://www.sara-net.jp/?p=7336

## Reproducible commands

```bash
python tools/inspect_vb6.py /path/to/GameDev2DX.exe --out generated/vb6
python tools/scan_form_serialization.py /path/to/GameDev2DX.exe fGMSKoyou \
  --out generated/vb6/form_fGMSKoyou_strings.tsv
python tools/scan_form_serialization.py /path/to/GameDev2DX.exe fGMSKoyouKz \
  --out generated/vb6/form_fGMSKoyouKz_strings.tsv
```

## Next target

Decode the `fGMSKoyou` form/load and `雇用決定` P-Code far enough to answer four concrete questions:

1. where the applicant records come from;
2. how many applicants are rolled and how refresh/reroll works;
3. how contract cost is checked/deducted;
4. how a selected applicant is copied into the active employee roster.

Once those are recovered, M5 can make the first management action genuinely state-changing.
