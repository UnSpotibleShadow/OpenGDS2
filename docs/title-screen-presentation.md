# Compact title screen

The title screen keeps all ten actions in a centered 600-pixel menu. New Game,
Load Game and Settings share a 30-pixel primary row. Six secondary actions use
26-pixel buttons in two columns; Quit and the build version share a footer.
The campaign boundary remains visible. Other forms retain their existing layout.

The imported `Graphic/Title.bmp` displays at its native 558×385 dimensions.
English uses locally generated, authored pixel lettering reading **OPEN GAME /
DEV STORY II** with cream, gold and red outlines. The original sky gradient is
reconstructed within the top 165-pixel caption band (excluding the tower cap). Every pixel below that band,
including the city and mascots, is preserved. This is a new English title treatment,
not a claim of an official localized logo.

Generation requires the exact RGB hash
`4e95e7151a12f32ae00bc6c9afd5e7cefd1b014b15d0d0c708d60d6ae9c74fa2`.
The result stays in `user://localized-artwork/en/title-logo-v2/`; imported artwork
is untouched and no original image is bundled. Unknown variants fall back to
their original image. Japanese uses the original, and explicit user artwork
overrides have priority. Existing imports need no reimport.

Validation covers a fresh title without artwork, English/Japanese at 1280×720,
a tall 1280×1016 window, menu return from Settings, backup availability and
custom override priority. Pixel checks verify the untouched lower scene, original
Japanese and deterministic bounded patching. Private rendered previews stay
outside the repository. Native Windows final-package acceptance remains pending.
