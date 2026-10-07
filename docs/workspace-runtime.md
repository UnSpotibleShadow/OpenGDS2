# Local continuation build

This restricted workspace has no `/proc`, which prevents the ordinary .NET CLI
from starting correctly. `tools/workspace_thread_entry.c` runs main on a pthread
with discoverable stack metadata. `tools/workspace_roslyn_build.py` invokes the
installed .NET 8 compiler directly, including the Godot source generator for the
app. This fallback is not a substitute for a clean SDK/MSBuild release export.

`tools/restore_packaged_audio.py` restores the engine-ready, MD5-verified audio
resources from a trusted desktop debug artifact. It does not restore the source
OGG containers or modify `seasonal-manifest.json`. The source audio integrity
test must remain failing until all twelve exact source files are available
locally. The existing GitHub source-audio blobs are preserved during publication.

`tools/workspace_debug_package.py` creates a Windows (`--platform windows`) or
Linux (default) development package using the
artifact's native engine, runtime and imported community audio, replacing the
game assembly and translations with the current locally built versions. Its
`WORKSPACE_BUILD.txt` identifies this provenance. Never include original game
files or private import caches in this package. On Windows, extract the complete
ZIP and launch `OpenGDS2.console.exe`; on Linux, use `OpenGDS2.sh`. Windows packages
preserve the artifact's Windows engine and runtime. Windows execution cannot be
verified in this workspace without a Windows runtime.

GitHub Actions remain paused. No remote workflow is dispatched by these tools.

The M61 packager verifies every base PCK payload against its stored MD5 before reusing it. It adds current meeting OGGs, import descriptors, engine-ready resources and their manifest, updates the PCK entry count, and records `--source-commit` and `--build-label` in BUILD_MANIFEST.json. Always copy the trusted base into a separate staging folder; the tool modifies that staging PCK in place. Pass `--platform windows` for the owner's requested development builds. Validate the resulting ZIP, PCK entry digests, meeting resource decoding, assembly hash and SHA256SUMS. Do not claim a native Windows run from Linux checks.
