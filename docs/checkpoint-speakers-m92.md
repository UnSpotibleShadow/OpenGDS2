# M92: Checkpoint speakers and payroll scratch

The random-employee portrait branch of `Module1 009933DC +0033..00E5` is now
connected to alpha/beta checkpoint discussions and confirmations. It rebuilds
payroll scratch member 3018, fills a source-ordered roster of gate-1 employees,
draws `floor(Rnd * count)` and reads the chosen source's portrait. It changes no
funds immediately; later monthly payroll consumes that shared count as its salary
index. This is the original arithmetic, not a sum of the speakers' salaries.

Fourteen direct checkpoint call sites are identified: five suggestion variants in
`009D2F68`, six beta discussion branches in `009C7860`, the type confirmation in
`009ACF14 +07F7`, and genre confirmations in `009B00D8 +088E/+098F`.
Publicity `009ACF14 +086D` and the affinity/fun bonus `009B00D8 +08E9` use secretary
portrait -2 and bypass roster/scratch rebuilding. Publicity is now presented as
the secretary's line, correcting the earlier generic-employee label.

Portrait draws are interleaved with suggestion/variant draws while preparing the
modern checkpoint queue. Each line stores its selected source and portrait, so
redrawing, acknowledgement and modern save/load do not reroll people or rebuild
scratch. English/Japanese speaker labels use locally loaded employee names. The
existing UI prepares a complete saved queue before showing it; this is modern
continuity rather than the original blocking form-call stack. Whole-session RNG
state and native Windows comparison remain separate work.

An incomplete modern source table leaves scratch unknown and produces no random
speaker. A complete table with no employed people establishes count 0 but selects
no person and consumes no portrait draw. The native empty path draws and reads
retained map cell 0; its contents are unestablished here. This explicit modern
boundary avoids inventing a former employee. Legacy meeting lines without speaker
fields retain their generic presentation; loading does not add missing draw history.

The earlier backlog's "technology selector" is specifically
`fGMTenantTechSel 0099F9A4`: the school-capacity roster already connected by M70.
Ordinary technology purchase/training/use does not add a scratch writer; retain
M80's fix. Remaining direct literal -1 portrait callers are planning (37), final
completion (12), awards (15), sales commentary (1) and shipment (2): 67 sites.
The scan found 81 sites total, of which the 14 checkpoint sites are now connected.
Dynamic portrait arguments and secondary-helper calls are separate evidence;
this is not a claim that every dialogue call has been implemented. The periodic
poaching routine has its own count writer and still needs trigger/queue integration.

`tools/verify_checkpoint_speaker_evidence.py` checks the supported binary identity,
six procedure fingerprints, 25 bounded anchors and the 14 checkpoint calls. Only
numeric/structural metadata is committed; original dialogue, names and full traces
stay private. Seven domain checks cover roster order/payroll arithmetic, unknown
and empty maps, draw interleaving, beta portraits, saved identities, secretary
bypass and malformed identities. All 483 optional-original domain checks and a
clean application build/import pass. English/Japanese UI checks cover named
speaker/portrait presentation, payroll count, saved identities and page advance.
The fixture acknowledges the preceding secretary introduction and uses the
source-backed five-entry minimum type roster in save checks. No gameplay/save
validators were weakened. Final regression/publication results are recorded in
issue #55. The M90 ZIP is unchanged; current source is the owner's testing route.
