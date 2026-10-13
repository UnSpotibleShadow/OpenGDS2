# M102: awards presentation follow-ups

This batch addresses two confirmed leftovers after the M101 layout work: the
Japanese GAME CONTEST lettering remained in the English stage picture, and a
winner/payment message clipped at one line.

The English patch accepts only the known decoded RGB fingerprint of the locally
imported 256×160 AwardsStage image. It replaces a 182×18 area inside the existing
title-plate border with authored blue/black pixel lettering. All pixels outside
that rectangle remain identical. It shares the existing local caption cache and
never overwrites or distributes the original. Japanese bypasses it; explicit
user artwork overlays retain precedence. Unknown sources retain the local original.

Award messages use word-aware wrapping and a font-measured height. The white
message panel and window grow vertically as necessary, and acknowledgment stays
below the text. The stage/trophy arrangement remains compact. Long trophy-row
names also retain their full text in a tooltip. This changes presentation only:
category draws, payout values, saved reel picks and acknowledgment order remain
in the existing services.

Validation uses deterministic caption-boundary/source-immutability checks,
optional-original GIF decode/fingerprint/scenery checks, a clean app build/import,
and rendered English/Japanese short-title, long-title and rival-result cases.
The rendered checks assert that every message line is visible, acknowledgment
does not overlap, Japanese scenery remains unchanged and save/reload neither
pays nor draws again. Captures and original inputs stay outside the repository.
Results: 518 optional-original domain checks; clean build/import; six rendered
English/Japanese short/long/rival cases; full core-loop regression.
#95 stays open for further findings. No build export, release or Actions dispatch.
