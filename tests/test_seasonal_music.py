"""Integrity checks for the supplied community recordings, not original assets."""
import hashlib
import json
from pathlib import Path
import unittest


class SeasonalMusicTests(unittest.TestCase):
    def test_compressed_recordings_match_verified_manifest(self):
        root = Path(__file__).parents[1]
        manifest = json.loads((root / "audio/community/seasonal-manifest.json").read_text())
        recordings = manifest["recordings"]
        self.assertEqual(len(recordings), 12)
        self.assertEqual(len({entry["path"] for entry in recordings}), 12)
        for entry in recordings:
            with self.subTest(path=entry["path"]):
                content = (root / entry["path"]).read_bytes()
                self.assertEqual(len(content), entry["bytes"])
                self.assertEqual(hashlib.sha256(content).hexdigest(), entry["sha256"])
                self.assertTrue(content.startswith(b"OggS"))
                self.assertLess(len(content), entry["source_bytes"] / 5)
                self.assertGreater(entry["duration"], 0)
        for season in ["spring", "autumn", "winter"]:
            self.assertEqual(sum(f"/{season}/" in entry["path"] for entry in recordings), 4)

    def test_repository_contains_no_uncompressed_community_wavs(self):
        folder = Path(__file__).parents[1] / "audio/community"
        self.assertEqual([p for p in folder.rglob("*") if p.suffix.lower() == ".wav"], [])


if __name__ == "__main__":
    unittest.main()
