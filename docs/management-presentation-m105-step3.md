# M105 Step 3 — everyday management presentation

The owner authorized Step 3 separately. This work stays local on
`codex/m105-presentation-evidence`; issue #95 remains open. Step 4 (hardware
presentation) needs the owner's next approval. Original audio remains Step 7.

## Implemented layouts

The recovered designer and handler fingerprints are recorded in
`tools/presentation_layout_evidence.json` and `docs/presentation-evidence-m105.md`.
Twip rectangles establish the original arrangement, not exact modern-font parity.
English clients are slightly wider where full names and financial values need it.

- **Hardware licences (`fGMHLicense`)**: compact scrollable hardware, licence,
  shipped-units and manufacturer columns; selected device/bit width, description
  and licence fee below. The fourth native column is manufacturer, not fee. Market
  units use the existing calendar projection; unknown ownership/market stays
  unknown. Selection and purchase retain the same window identity/position.
- **Additional production (`fGMSyukkaSai`)**: white nine-row cost/distribution
  table, one production total, device, quantity spinner/held controls and readonly
  retail price. Existing media-information detour preserves quantity and position.
  Production still checks recall, weekly allowance, capacity and available funds.
- **Training (`fGMWSTech`)**: three course columns, selected description/progress,
  experience/capacity and study-seat shortcuts. Source course costs, ownership,
  level/capacity and study-seat gates still use the existing domain services.
- **Technology use (`fGMInfo`)**: bounded learned-technique list with use costs,
  scrollable explanation and actual saved work/speed status. Native speed/stress
  boundaries remain visible; using a technique retains its established charge,
  recipient and retained-frame requirements. Save/load and desk access remain.
- **Staff/employee information (`fGMSyain`, `fGMInfo`)**: bounded salary roster,
  selected portrait/compact ability chart and employee actions; employee technology
  page uses a white name/level table. The active pressed tab survives employee
  switching, alongside Step 2's retained window coordinates. Payroll capture still
  runs on roster opening; nullable source salaries remain unknown.
- **Secretary hiring (`fGMHisyoK`)**: three visible native columns in name,
  contract-fee, monthly-salary order; the designer's rank heading was hidden.
  Portrait/biography preview and explicit candidate selection remain. Replacement
  still charges once and resumes the saved farewell/greeting conversation.
- **Secretary policies (`fGMHisyoSet`)**: boxed production, break and return groups
  in the recovered stacked arrangement, with separate advice/confirm/cancel.
  Radio controls have compact, stable margins. Only Confirm applies the draft;
  advice closes the draft and opens the existing advice conversation.
- **Bank (`fGMBank`)**: three loan rows with due date, principal and repayment,
  four exclusive borrowing choices, terms and eligibility status. The fourth choice
  was hidden in the static designer; a matching runtime visibility gate was not
  established, so existing supported borrowing choices remain available. Receipt
  acknowledgment and saved transaction behavior are unchanged.
- **Company information (`fGMInfo`)**: supplementary compact information window,
  opened by the Company pane's ellipsis, restores confirmed secretary, distribution
  fees, development-computer processing, three tenant slots and best-selling game
  fields without replacing the accepted Company summary. It uses the native icon.

This changes presentation and navigation, not domain pricing, training work,
secretary policy values, borrowing terms, simulation draws or accounting rules.
Original pixels/captions/captures remain in the user's local import/private scratch
cache; new English/Japanese labels and public test fixtures are authored.

## Verification

`tests/management_layout_smoke.gd` covers both languages, client containment,
licence payment/ownership, training seat/charge/active course, employee tab state,
Company fields, secretary column order/boxed policies and reproduction totals.
Existing core-loop, speed/stress use and window/artwork regressions cover saved
management transactions, source-target boundaries, stable positions and button art.
Final receipts and private rendered crops are under `/workspace/scratch/m105-step3-*`.

Final results: clean app build (0 warnings/errors), bilingual management layout,
headless full core-loop, bilingual speed/stress use and bilingual window/artwork
regressions all pass. No domain service changed, so the existing 522 domain-test
result is historical rather than a new Step 3 run.

The first render caught table minimum-width overflow and policy-control spacing;
both were corrected. The fee assertion was corrected to compare numeric JSON
licence IDs rather than mixed integer/float arrays. The core-loop launch initially
omitted its established fixed frame rate; that attempt was stopped at 109 seconds,
and the corrected check passed in 33 seconds. Speed/stress fixtures now explicitly
select their worker in the bounded roster. The window regression caught and fixed
a disposed-spinner read on return from media information; it now verifies draft
quantity as well as window position. Only affected failed checks were rerun.

Exact native font rendering and Windows stacking still need the owner's review.
No original assets, screenshots, build/export or remote publication were added.
