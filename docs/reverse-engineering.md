# Reverse-engineering notes — GameDev2DX

## 1. Distribution structure

The supplied raw archive contains 48 entries. The important high-level layout is:

- `GameDev2DX.exe` — actual game executable, 6,365,184 bytes after extraction.
- `GameRank.GDS` — 61-byte CSV-like ranking/data file.
- `Graphic/` — BMP assets.
- `Graphic/Human/` — 30 small character sprites, normally 64×64.
- `Sound/` — five PCM WAV effects.

The separate ~2 MB `GameDev2DX.exe` download is a self-extracting wrapper; it is not the same binary as the ~6.36 MB game executable stored inside the LZH.

## 2. Executable technology

Static PE inspection identifies the game as a 32-bit Visual Basic 6 application:

- imports `MSVBVM60.DLL`;
- contains the VB executable marker `VB5!`;
- contains `#VB6JP.DLL` and a VB6 type-library path;
- imports VB runtime machinery such as `MethCallEngine` and `ProcCallEngine`;
- imports `winmm.dll`, including multimedia functions;
- uses GDI-era Windows drawing APIs.

This is useful for modernization: the original is not an opaque custom engine. It is a VB6 desktop application with compiled forms, controls, embedded designer resources, and native/runtime glue.

## 3. Recoverable UI/localization data

A heuristic scan of 16-bit-length-prefixed CP932 strings recovered 673 unique candidates from the executable. There are false positives, but hundreds are clearly genuine UI strings.

Examples found include:

- メニュー(&M)
- 新しいゲーム(&N)
- ゲームの保存(&S)
- ゲームの読込(&L)
- ゲームの終了(&E)
- ゲームの開発
- オリジナルゲーム開発
- ゲームソフト完成
- ゲームコンテスト
- クロスレビュー
- 社員雇用
- テクノロジー購入
- ハードライセンス取得
- ゲームハード開発
- 全日本ゲームコンテスト

The text is therefore substantially extractable without OCR and without running the program.

## 4. Recovered VB6 object/form names

The VB metadata exposes a large screen/form inventory. Representative names include:

- `MDIForm1`
- `MainForm16`
- `MainForm3`
- `fGMKaihatu`
- `fGMKikakusyo`
- `fGMGameNew`
- `fGMGameSave`
- `fGMGameLoad`
- `fGMGameHardInfo`
- `fGMGameDexSn`
- `fGMGameDexKs`
- `fGMRank100`
- `fGMTenantCasino`
- `fGMTenantOnsen`
- `fGMDevW1`
- `fGMDevW2`
- `fGMDevH`
- `fGMGameKaishi`
- `fGMHelp`

This is enough to start treating the application as a collection of screens/state machines rather than one monolithic binary.

## 5. External graphics

Observed dimensions:

- `Title.bmp`: 558×385 RGB
- `Kabe.bmp`: 1920×1280 paletted BMP
- `Beibazyo*.bmp`: roughly 421 px wide
- `KeibaC.bmp`, `KeibaM.bmp`: 233×316
- most `Graphic/Human/*.bmp`: 64×64 paletted BMP

These are conventional BMPs and can be imported losslessly by modern engines.

## 6. Audio

The WAV files are ordinary PCM audio; for example `FIRE.wav` is 16-bit mono, 22.05 kHz. They do not require a proprietary decoder.

## 7. Recommended modernization path

### Milestone A — preservation/import layer

- deterministic LZH extraction;
- file hashes/manifests;
- BMP/WAV importers;
- CP932 string extraction;
- form/object inventory.

### Milestone B — localization layer

Create stable localization IDs independent of byte offsets, for example:

`menu.new_game`, `menu.save`, `menu.load`, `dev.original_game`, `contest.grand_prix`.

Keep Japanese source text locally for matching, and maintain English text in a separate community-authored file.

### Milestone C — modern shell

Recreate the MDI-era shell first: title screen, main window, menus, scaling, audio, and one representative dialog. Do not try to recreate every gameplay rule at once.

### Milestone D — gameplay/state model

Map each VB6 form to a modern screen/controller and progressively document state variables, save data, progression rules, random events, staff data, game-development calculations, and economy logic.

### Milestone E — compatibility tests

For each reconstructed mechanic, create input/state fixtures and compare outputs to observations from the original build. This turns reverse engineering into repeatable regression tests.

## 8. Engine choice

A practical default is Godot or C#/.NET. Both make Unicode localization, modern windowing, audio, image import, high-DPI scaling, controller support, and cross-platform builds straightforward. The extracted VB6 form names and strings can be used as reconstruction clues regardless of engine.

## 9. Public-repository hygiene

A public repository should preferably contain:

- community-written source code;
- extraction/import tools;
- format documentation;
- community-authored English localization;
- tests and hashes;

and should not bundle the original Kairosoft EXE, BMPs, WAVs, or raw extracted Japanese assets unless permission clearly allows redistribution. Users can supply the original free Kairosoft download locally.
## 10. M4 hiring-form correction

The serialized designer blocks distinguish two similarly named forms:

- `fGMSKoyou` — `社員雇用` (employee hiring)
- `fGMSKoyouKz` — `契約更新` (contract renewal)

The hiring form directly exposes the PC game's employee stat labels: level, fun, novelty, accuracy,
graphics, music, development speed, inspiration and contract fee. See `docs/milestone-4.md` and the
reproducible output under `generated/vb6/`.

## M54 secretary management

Seven-argument Module1.26 records feed secretary recruitment; fGMHisyoK charges
a contract once, presents old farewell, updates current index/office crop and
presents the new greeting. fGMHisyoSet stores production/break/return globals.
Main-loop policy consumers prove 300m/100m/manual production, 100/80/60 or
voluntary rest, and 40/20/0 return. This corrects M16's mode-based threshold
attribution. Salary remains a display column with no recurring debit proven.
See [procedure anchors, persistence model and unresolved behavior](secretary-management.md)
and `tools/verify_secretary_management_evidence.py`. Private full traces and
original names/biographies remain outside the repository.
