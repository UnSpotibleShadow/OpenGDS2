# Employee progression and care

Classification: verified static data flow against the supported original hash.
The original process was not run. Modern random draws do not reproduce VB6 Rnd.

## Manual level-up

`fGMInfo.Command1` handler starts at `009A3E34`, size 704. Branch index 0:

- +000F checks level >=10 and rejects it.
- +0076 checks experience (`A11C4C`) >= threshold table (`A112FC`), indexed by level.
- +00ED clears experience; +00FB increments level once.
- +011A updates stored threshold (`A11C68`) for the new level.
- +0135 opens fGMLevelUP modally, applying seven gain formulas.
- +0213 stores `Int(old contract fee * 1.5)` back into `A11BDC`.

Threshold helper starts at `0097B370`; argument 10 is the index and 14 the value,
writing member 02E0 (`A112FC`). Ten initializer calls at +0204, stride 20:

| Current level | Required experience |
| --- | ---: |
| 1 | 100 |
| 2 | 150 |
| 3 | 200 |
| 4 | 250 |
| 5 | 300 |
| 6 | 350 |
| 7 | 400 |
| 8 | 500 |
| 9 | 600 |
| 10 | 777 (table value; leveling is rejected at the cap) |

Excess experience is discarded, not carried forward. Leveling does not itself
charge company money. The fee becomes the increased amount for future contracts.
Stat draw order: bonus, fun, graphics, novelty, music, accuracy, speed, inspiration.
The modern service prepares changes before mutation and synchronizes source records.

Reset member 0C30 initializes experience to numeric Empty/zero; member 0C68
initializes stress the same way. Employee helper +01DD initializes the separate
stored experience threshold from the level table. Module2 work branches at
+6612, +A605, +A99F and +AD23 add 3.7 or 10 experience, then clamp to the stored
threshold. This confirms fractional storage is necessary. These arithmetic
fragments do not establish task eligibility, tick frequency or training duration;
no generic experience-generating tick is enabled.

## Stress and hot spring

`fGMTenantOnsen` handler starts at `0099E164`, size 1,388. It checks confirmation,
counts employed records, rejects zero employees, checks funds against `A114E8`,
charges that cost once and sets `A11C84` to zero for every employed record at
+025B..0268. Initializer +0150 sets the cost to **8,000,000**, member 04CC.
This is a company-wide cost, not a fee multiplied by staff count.

Module2 periodic handler `009E3830` checks employed flag and stress >=85 at
+0026..0052. A separate warning flag prevents repeated notices until eligibility
ceases. The UI message advises moving the employee to the break room.

`EmployeeCareService` implements the verified hot-spring transaction and the
warning predicate. The hot-spring action remains absent from the UI because
its tenant/office availability is unresolved. Warning scheduling and other
stress accumulation/recovery behavior are also unresolved.
