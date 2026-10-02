# Public repository hygiene

OpenGDS2 is intended to be distributable without Kairosoft's original game binaries or assets.

Do not commit or publish:

- `GameDev2DX.lzh` or the original `GameDev2DX.exe`.
- Files extracted from the original archive such as BMP/WAV assets.
- Bulk dumps of original Japanese game text such as `generated/cp932_string_candidates.tsv`.
- Save files or other user-local imported data.
- The local `generated/` analysis directory, including legacy extracted form strings and full instruction traces. Regenerate reports locally with the tools; publish only necessary structural findings in authored documentation.

Safe project content should consist of independently written engine code, import/reverse-engineering tools, authored translations/localization identifiers, documentation, and small structural metadata needed to reproduce the analysis.

The runtime importer is designed so each user supplies their own copy of the official original download and imports assets locally into `user://`.
