#!/usr/bin/env python3
"""Convert the four owner-supplied meeting WAVs without changing the sources."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import tempfile
import wave

TRACKS = {"planning": "Planning Meeting.wav", "alpha": "Alpha Meeting.wav",
          "beta": "Beta Meeting.wav", "final": "Final Meeting.wav"}

def convert(source_directory, output_directory):
    output_directory.mkdir(parents=True, exist_ok=True)
    targets = [output_directory / (phase + ".ogg") for phase in TRACKS]
    manifest_path = output_directory / "manifest.json"
    if any(path.exists() for path in targets + [manifest_path]):
        raise ValueError("Meeting recordings already exist; use a new output directory")
    recordings = []
    with tempfile.TemporaryDirectory(dir=output_directory) as temporary:
        stage = Path(temporary)
        for phase, name in TRACKS.items():
            source = source_directory / name
            with wave.open(str(source), "rb") as wav:
                duration = wav.getnframes() / wav.getframerate()
            target = stage / (phase + ".ogg")
            subprocess.run(["ffmpeg", "-nostdin", "-v", "error", "-i", str(source),
                "-map", "0:a:0", "-vn", "-map_metadata", "-1", "-c:a", "libvorbis",
                "-q:a", "4", "-ar", "48000", "-ac", "2", str(target)], check=True)
            content = target.read_bytes()
            if not content.startswith(b"OggS") or duration <= 0:
                raise ValueError("Invalid converted meeting recording")
            recordings.append({"phase": phase, "source": name,
                "path": "audio/community/meetings/" + target.name,
                "duration": duration, "source_bytes": source.stat().st_size,
                "source_sha256": hashlib.sha256(source.read_bytes()).hexdigest(),
                "bytes": len(content), "sha256": hashlib.sha256(content).hexdigest()})
        written = []
        try:
            for target in targets:
                with target.open("xb") as stream:
                    written.append(target)
                    stream.write((stage / target.name).read_bytes())
            with manifest_path.open("x") as stream:
                written.append(manifest_path)
                json.dump({"encoding": "Ogg Vorbis quality 4, stereo 48000 Hz",
                           "recordings": recordings}, stream, indent=2)
                stream.write("\n")
        except Exception:
            for target in written:
                target.unlink(missing_ok=True)
            raise
    print("Converted four meeting tracks:", sum(r["source_bytes"] for r in recordings),
          "WAV bytes to", sum(r["bytes"] for r in recordings), "OGG bytes.")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source_directory", type=Path)
    parser.add_argument("output_directory", type=Path)
    args = parser.parse_args()
    convert(args.source_directory, args.output_directory)
