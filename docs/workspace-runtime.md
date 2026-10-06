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
