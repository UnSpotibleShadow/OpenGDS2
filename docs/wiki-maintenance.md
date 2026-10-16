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

GitHub currently has the wiki feature enabled but returns Repository not found
for its uninitialized Git repository. If it cannot be initialized through Git,
the owner must create the first Home page in the GitHub Wiki web UI. A placeholder
is enough; then reconcile its head and publish the prepared pages. The source
pages and repository guide remain usable while that external step is pending.
