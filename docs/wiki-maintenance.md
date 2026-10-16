# Maintaining the player wiki

Canonical page source is in [docs/wiki](wiki). Page filenames are GitHub Wiki
slugs; `_Sidebar.md` and `_Footer.md` supply shared navigation. Internal links
omit `.md` because they target wiki pages, not repository file URLs. Use the
[repository index](player-guide.md) when reading the guide before wiki publication.

Keep the guide about current player behavior. Link research/implementation details
instead of copying historical milestone claims as current facts. Preserve the
original archive requirement, save boundaries, technology-action distinction,
uncapped recall liability and explicit validation limits. Do not promise a
published download or architecture that is not actually listed in Releases.
No original screenshots, archives, extracted text or assets are bundled here.

Publication uses the separate `OpenGDS2.wiki.git` repository. Clone/read its current
head first, copy only the reviewed page manifest, commit, push without force and
read back the result. Preserve unrelated pages. Keep write intents and resulting
SHAs in an external recovery record. Do not delete another editor's new content.

The owner initialized Home on 16 October 2026. The twelve reviewed files were
published on top of that commit, preserving its history. The wiki is live at
https://github.com/UnSpotibleShadow/OpenGDS2/wiki . Future edits must clone or
fetch the current wiki head first; do not push the earlier disconnected staging
history or force-push over other editors. Check all ten public page URLs and
sidebar/footer after publishing.
