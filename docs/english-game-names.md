# English game names

The award ceremony previously inserted raw imported rival titles into the award
board, winner dialogue and announcement reel. Sales rankings did the same.
Development content-choice tooltips also retained their Japanese example names.

The English interface now supplies authored display names for:

- All 30 opening rival games and 35 later rival releases, including their sequels.
- All ten rival publishers shown alongside those games.
- Both example games and the example magazine for each of the 51 content types:
  102 game names and 51 magazine names.

The existing 31 manga title translations continue to work. Shared manga/rival
names use the same English wording. The names preserve the original's fictional
parodies rather than replacing them with the real commercial titles they evoke.
These are community English localizations, not claims of official English names.

`GameNameLocalization` resolves a saved rival against its imported owner/title
identity. Runtime chart and award indices are not translation IDs: new player
releases can change those indices, and sales totals change during play. Only
presentation changes; no import or save schema migration is needed. Existing
saves receive the English names immediately.

Player-owned games and the player's company name remain untouched, including
player titles identical to a Japanese rival name. Japanese uses the imported
names. Unknown rivals, unsupported catalogs, missing or blank translations retain
their original display text. Rival publisher aliases require the ten-entry native
publisher table, and development examples require the 51-entry content table.

The award board, its initial construction, host result dialogue, scrolling
announcement reel and sales ranking rows all use the shared title resolver.
Full ranking titles/publishers remain available in tooltips. The chart loads its
catalog before its first refresh, so its initial display is also localized.
Development example tooltips use content source IDs and example positions.

Validation includes 565 domain checks with the private supported original,
a clean application build, translation coverage/key accounting, rendered
English/Japanese award/chart/example flows, existing-save display and save/load
checks, player-title collision checks, award continuation and the core loop.
The native-backed checks cover all 65 rival titles and ten publishers in the
rankings; the domain checks cover every one of the 153 content examples.
Original catalogs, extracted names and rendered captures remain outside git.

The localization-key accounting check also recognizes the 49 existing review
comment keys whose Japanese text is imported locally, just like cancellation
text. That repairs the existing accounting exception without bundling source
Japanese strings or loosening the check for new untranslated authored keys.
