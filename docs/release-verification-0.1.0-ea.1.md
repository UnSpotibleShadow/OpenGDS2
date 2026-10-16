# Early Access 0.1.0-ea.1 — artifact and publication record

**Subsequent publication:** the owner published their own Windows x86_32/x86_64
exports and checksum files in release ID415162340 on 16 October 2026. The repository
is public. The hashes/validation below identify the earlier agent-prepared x64
artifact, not those independently exported public packages. The upload/visibility
blockers below are historical; they no longer describe release availability.
Use the actual published asset names/checksums for downloads.

Prepared 16 October 2026 after owner gameplay sign-off and explicit build/release
authorization. This record does not announce an available public download.

## Immutable identity

- Tag: `v0.1.0-ea.1`, annotated and pushed. Never move or recreate it.
- Built source: `83db494daf01a855fd7d9895142c7928265b58fd` (PR134 merge).
- Tag object: `6fe0c4543340ab9d7b4fe94a6b1272a6ca752b39`.
- Windows x64 ZIP: `OpenGDS2-windows-0.1.0-ea.1.zip`, 112,040,930 bytes.
- ZIP SHA-256: `1ff6636f10a4192b7ec53d83875d0ce8966761d74a4964f40bc2e67df9c4f52f`.
- Game assembly SHA-256: `e9553a102306789c3f7bf36a3d24b3827ffe3a1f51eb04e041df6ff8006d1109`.
- Standard clean Godot 4.4.1 .NET release export, SDK8, self-contained .NET8.0.31.
- Permanent user-data directory: `OpenGDS2`.

## Verification

Clean build/import/export and generated engine/runtime notices passed. The 554
public domain tests and 93 Python tests (17 optional original-fixture skips)
passed. The exact Windows game assembly/PCK passed exported-host startup,
development/shipment/sales core-loop and English/Japanese recovery checks.
Focused fresh original-LZH import, new-company creation, save/restart/load,
English/Japanese reload and legacy-folder migration also passed.

All ZIP CRCs, 201 package file hashes and 272 PCK payload digests passed. All 25
community audio sources matched Git blobs and their imported resources/remaps.
The official Windows release engine matched the previously verified template
hash. Original game files, private saves/caches, debug symbols and host adapters
were excluded. Tracked release source stayed unchanged during export.

The validation host uses matching Linux engine/runtime with unchanged Windows
game assembly/PCK. **Native Windows package acceptance is unrun.** Full sustained,
multi-year and display checks remain community work; owner gameplay sign-off
is recorded separately from this exact artifact's validation.

The optional legacy `tests/ui_smoke.gd` original-data branch failed 25 assertions.
Its schema10/173-entry expectation predates current schema18/374 entries, and
its pixel regions/control assumptions predate UI changes. Later failures have
not all been independently reconciled. The focused import/save/restart checks
passed separately; the older suite is not claimed as passing. Updating that
optional harness remains maintenance work.

The public-content review covered 262 reachable historical commits and 3,547
named objects, plus hosted issues/comments and screenshot references. No flagged
original-file paths, unexpected binaries or credential-pattern matches were
found. This is a bounded scan, not proof against every possible unknown secret.
Owner-provided screenshot references were retained. Actions were not dispatched.

## Publication blocked by integration permissions

Draft release ID **415139616** exists with notes at
https://github.com/UnSpotibleShadow/OpenGDS2/releases/tag/untagged-a238e123dddb00b2b015 .
No assets were uploaded; the release is not published. The repository is private.
Asset upload to uploads.github.com returned HTTP401 Bad credentials; read-back
confirmed an empty asset list. Repository visibility PATCH returned HTTP403
Resource not accessible by integration; read-back confirmed private visibility.
These failures do not revoke the owner's release authorization. Do not retry
unchanged credentials or create another draft/tag.

For manual completion, upload the verified ZIP, its `.zip.sha256` companion and
`OpenGDS2-0.1.0-ea.1-verification.md` to the existing draft. Make the repository
public and publish that draft as an Early Access pre-release. Then verify the
public tag target, anonymous release/issue access and downloaded ZIP SHA-256.
Only after that verification should README announce the actual download URL.
Documentation follow-ups on main do not alter the frozen built source or tag.
