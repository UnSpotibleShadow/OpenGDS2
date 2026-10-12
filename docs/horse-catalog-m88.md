# M88: Native runner/jockey catalog and entry confirmation

Recovered 2026-10-12 before porting the remaining race workflow. This milestone imports data locally; horse racing is still unavailable. Original names, executable and traces are not published. Source SHA-256 remains `4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6`.

## Initialization evidence

An initial direct-global reference search found reads but no writes for runner count `00A13B40`, strength `00A13CC8` and jockey count `00A13D18`. That was not an absent-data finding: these addresses alias company members relative to base `00A1101C`. Their members are `2B24`, `2CAC` and `2CFC` respectively. The complete decoded inventory includes member references as well as global references.

Reset `009DD234` empties these members; it is not the record initializer. Runner append helper `00980690` writes name/weight/kind through members `2C58/2C74/2C90`, writes strength as **current runner count + argument 4 /100** to member `2CAC`, then increments `2B24`. Jockey helper `0097C570` writes members `2D10/2D2C` and increments `2CFC`.

Startup `009F651C` calls the runner helper **44 times**, at offsets `B6A3..BF66`; it calls the jockey helper **10 times**, at `BF90..C0E6`. Runner index 5 uses an R8 literal 15.4 rather than an I2 literal; its resulting strength is 5.154. The importer preserves numeric operation order and retains unidentified columns under raw names. No modern strength formula or fictional runner dataset is substituted. Names are read from the original constant pool only on the user's local import.

The helpers resolve through thunks `0060ED2C -> 0098070C` and `0060ED38 -> 0097C5B0` (code-info addresses). The original initializer uses pool `0060DF28`. Exact call addresses and fingerprints are in the asset-free evidence manifest.

## Casino entry and further race defects

The selector command index 0 routes to horse racing. Its serialized Command1 index-0 record at file offset 2326512 has no Enabled-property override; native CommandButton defaults leave it enabled. The record fingerprint is verified, independently of the event branch. Earlier missing-data concerns are resolved by the member-alias initialization above, not by assuming that enabled controls populate records.

Additional race tracing confirms a betting typo beyond the previously recorded compressed odds mismatch: adding/removing the pair for first runner 4 and second runner 3 writes wager cell `[4,4]`, while its local display coordinates retain `[4,3]`. Removal/clamp paths also use compressed selection columns in places. The race port must preserve or explicitly label these behaviors, alongside the already recovered normal-path winnings overwrite. Do not normalize the matrices or advertise repaired settlement as unchanged fidelity.

## Integration and verification

`OriginalCatalog` gains optional Horses/Jockeys lists. Old modern catalog JSON remains readable with empty lists; a fresh LZH import extracts the records. Own-console catalog composition preserves the new lists. Names stay in the local original cache and are excluded from repository/package content. Horse UI implementation will require complete locally imported records and guide older caches to reimport.

The domain suite passes 433 checks with the optional private original fixture: all 44/10 records, source indices, nonempty local names, the exact strength expression/R8 case and preservation through own-console composition are checked. A clean application build validates integration. Run the separate static verifier against a private original executable:

```sh
python3 tools/verify_horse_catalog_evidence.py /local/path/GameDev2DX.exe
```

It verifies two helper fingerprints, all 54 initialization call sites and the selector control without printing original names or dialogue. Native Windows entry/playthrough acceptance remains community work. Next: port ordinary horse racing from M77–M79 with the documented wager, odds and settlement defects explicit; do not add an unsupported horse-management mode.
