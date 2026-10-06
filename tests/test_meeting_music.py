"""Integrity and format checks for the owner-supplied meeting soundtrack."""
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import unittest

class MeetingMusicTests(unittest.TestCase):
    def test_manifest_and_encoded_recordings(self):
        root = Path(__file__).parents[1]
        manifest = json.loads((root / "audio/community/meetings/manifest.json").read_text())
        self.assertEqual([r["phase"] for r in manifest["recordings"]],
                         ["planning", "alpha", "beta", "final"])
        for entry in manifest["recordings"]:
            with self.subTest(phase=entry["phase"]):
                content = (root / entry["path"]).read_bytes()
                self.assertTrue(content.startswith(b"OggS"))
                self.assertEqual(len(content), entry["bytes"])
                self.assertEqual(hashlib.sha256(content).hexdigest(), entry["sha256"])
                self.assertLess(len(content), entry["source_bytes"] / 5)
                self.assertGreater(entry["duration"], 0)
        self.assertFalse(list((root / "audio/community/meetings").glob("*.wav")))

    @unittest.skipUnless(shutil.which("ffprobe"), "FFprobe is optional")
    def test_vorbis_stereo_rate_duration_and_no_cover_art(self):
        root = Path(__file__).parents[1]
        manifest = json.loads((root / "audio/community/meetings/manifest.json").read_text())
        for entry in manifest["recordings"]:
            with self.subTest(phase=entry["phase"]):
                info = json.loads(subprocess.check_output(["ffprobe", "-v", "error",
                    "-show_streams", "-show_format", "-of", "json", str(root / entry["path"])]))
                self.assertEqual(len(info["streams"]), 1)
                stream = info["streams"][0]
                self.assertEqual(stream["codec_name"], "vorbis")
                self.assertEqual(stream["channels"], 2)
                self.assertEqual(stream["sample_rate"], "48000")
                self.assertAlmostEqual(float(info["format"]["duration"]), entry["duration"], places=2)
