#!/usr/bin/env python3
"""Convert a user-owned audio ZIP locally; never upload or overwrite recordings."""
import argparse
from pathlib import Path
import shutil
import stat
import subprocess
import tempfile
import zipfile


def import_music(archive, output, ffmpeg="ffmpeg"):
    output = Path(output)
    output.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(archive) as bundle:
        entries = sorted((entry for entry in bundle.infolist()
                          if not entry.is_dir() and
                          Path(entry.filename).suffix.lower() in {".mp3", ".wav", ".ogg"}),
                         key=lambda entry: entry.filename)
        if not entries or len(entries) > 64:
            raise ValueError("Expected between 1 and 64 audio recordings")
        if sum(entry.file_size for entry in entries) > 1024 ** 3:
            raise ValueError("Uncompressed audio exceeds the 1 GiB limit")
        targets = [output / f"office-{index:02}.ogg" for index in range(1, len(entries) + 1)]
        if any(target.exists() or target.is_symlink() for target in targets):
            raise ValueError("Output recordings already exist; use a new folder or back them up first")
        # Stage the whole conversion before making any recordings visible.
        # The ZIP member's path is never used as a filesystem destination.
        with tempfile.TemporaryDirectory(prefix="opengds2-music-", dir=output) as temporary:
            stage = Path(temporary)
            converted = []
            for index, entry in enumerate(entries, 1):
                if stat.S_ISLNK(entry.external_attr >> 16) or entry.flag_bits & 1:
                    raise ValueError("Symlink or encrypted audio members are not supported")
                source = stage / (f"input-{index}" + Path(entry.filename).suffix.lower())
                with bundle.open(entry) as stream, source.open("wb") as file:
                    shutil.copyfileobj(stream, file)
                destination = stage / f"office-{index:02}.ogg"
                subprocess.run([ffmpeg, "-nostdin", "-v", "error", "-i", str(source),
                                "-map", "0:a:0", "-vn", "-map_metadata", "-1",
                                "-c:a", "libvorbis", "-q:a", "4", "-ar", "48000",
                                "-ac", "2", str(destination)], check=True)
                if not destination.is_file() or destination.stat().st_size == 0:
                    raise ValueError("Conversion did not produce an audio file")
                converted.append(destination)
            # Exclusive creation also refuses files added while conversion ran.
            written = []
            try:
                for source, target in zip(converted, targets):
                    with target.open("xb") as file:
                        written.append(target)
                        with source.open("rb") as stream:
                            shutil.copyfileobj(stream, file)
            except Exception:
                for target in written:
                    target.unlink(missing_ok=True)
                raise
        return targets


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("archive", type=Path)
    parser.add_argument("--output", type=Path, required=True,
                        help="Local music folder displayed in OpenGDS2 Settings")
    parser.add_argument("--ffmpeg", default="ffmpeg", help="Local FFmpeg executable")
    args = parser.parse_args()
    try:
        files = import_music(args.archive, args.output, args.ffmpeg)
    except (OSError, ValueError, zipfile.BadZipFile, subprocess.CalledProcessError) as error:
        parser.exit(1, f"Music import failed: {error}\n")
    print(f"Converted {len(files)} local recordings. Restart OpenGDS2 to load them.")
