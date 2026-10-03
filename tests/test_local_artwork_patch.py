"""Use authored bitmaps only; exercise local patch integrity and rollback."""
import copy
import hashlib
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest

from PIL import Image

spec = importlib.util.spec_from_file_location("patch_local_artwork", Path(__file__).parents[1] / "tools/patch_local_artwork.py")
artwork = importlib.util.module_from_spec(spec)
spec.loader.exec_module(artwork)


class LocalArtworkPatchTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.root = Path(self.temporary.name)
        self.original = self.root / "original"
        self.overrides = self.root / "asset-overrides"
        (self.original / "Graphic").mkdir(parents=True)
        self.source = self.original / "Graphic/Authored.bmp"
        Image.new("RGB", (80, 40), (10, 20, 30)).save(self.source)
        self.original_bytes = self.source.read_bytes()
        self.source_hash = "a" * 64
        (self.original / "catalog.json").write_text(json.dumps({"SourceHash": self.source_hash}))
        self.plan = {"format": "OpenGDS2LocalArtworkPatch", "schemaVersion": 1,
                     "sourceHash": self.source_hash, "patches": [{"path": "Graphic/Authored.bmp",
                     "sha256": hashlib.sha256(self.original_bytes).hexdigest(), "size": [80, 40],
                     "operations": [{"rect": [5, 5, 60, 20], "background": "#ffffff",
                                     "text": "English", "fontSize": 12}]}]}

    def tearDown(self):
        self.temporary.cleanup()

    def test_patch_preserves_original_and_unedited_pixels(self):
        outputs = artwork.patch_artwork(self.original, self.overrides, self.plan)
        self.assertEqual(self.source.read_bytes(), self.original_bytes)
        with Image.open(outputs[0]) as image:
            self.assertEqual(image.size, (80, 40))
            self.assertEqual(image.getpixel((0, 0)), (10, 20, 30))
            self.assertEqual(image.getpixel((5, 5)), (255, 255, 255))
            self.assertIn((0, 0, 0), set(image.get_flattened_data()))

    def test_hash_or_catalog_mismatch_creates_no_override(self):
        for changed in ("bitmap", "catalog"):
            plan = copy.deepcopy(self.plan)
            if changed == "bitmap":
                plan["patches"][0]["sha256"] = "0" * 64
            else:
                plan["sourceHash"] = "b" * 64
            with self.assertRaises(ValueError):
                artwork.patch_artwork(self.original, self.overrides, plan)
            self.assertFalse(self.overrides.exists())

    def test_metadata_traversal_and_symlinks_are_rejected(self):
        for relative in ("catalog.json", "Graphic/../catalog.json", "/Graphic/Authored.bmp", "Graphic\\Authored.bmp"):
            plan = copy.deepcopy(self.plan)
            plan["patches"][0]["path"] = relative
            with self.assertRaises(ValueError):
                artwork.patch_artwork(self.original, self.overrides, plan)
        (self.original / "Graphic/Linked.bmp").symlink_to(self.source)
        plan = copy.deepcopy(self.plan)
        plan["patches"][0]["path"] = "Graphic/Linked.bmp"
        with self.assertRaises(ValueError):
            artwork.patch_artwork(self.original, self.overrides, plan)
        self.assertFalse(self.overrides.exists())

    def test_invalid_second_patch_cannot_leave_first_output(self):
        second = self.original / "Graphic/Second.bmp"
        second.write_bytes(self.original_bytes)
        plan = copy.deepcopy(self.plan)
        plan["patches"].append(copy.deepcopy(plan["patches"][0]))
        plan["patches"][1]["path"] = "Graphic/Second.bmp"
        plan["patches"][1]["operations"][0]["rect"] = [79, 0, 2, 10]
        with self.assertRaises(ValueError):
            artwork.patch_artwork(self.original, self.overrides, plan)
        self.assertFalse(self.overrides.exists())

    def test_existing_override_and_original_destination_are_preserved(self):
        outputs = artwork.patch_artwork(self.original, self.overrides, self.plan)
        before = outputs[0].read_bytes()
        with self.assertRaises(ValueError):
            artwork.patch_artwork(self.original, self.overrides, self.plan)
        self.assertEqual(outputs[0].read_bytes(), before)
        with self.assertRaises(ValueError):
            artwork.patch_artwork(self.original, self.original, self.plan)
        self.assertEqual(self.source.read_bytes(), self.original_bytes)


if __name__ == "__main__":
    unittest.main()
