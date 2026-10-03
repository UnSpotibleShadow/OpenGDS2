"""Local soundtrack import must preserve existing files and fail atomically."""
import importlib.util
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest.mock import patch
import zipfile

spec = importlib.util.spec_from_file_location("import_music", Path(__file__).parents[1] / "tools/import_music.py")
music = importlib.util.module_from_spec(spec)
spec.loader.exec_module(music)


class MusicImportTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.root = Path(self.temporary.name)
        self.archive = self.root / "tracks.zip"
        self.output = self.root / "music"
        with zipfile.ZipFile(self.archive, "w") as bundle:
            bundle.writestr("../../first.mp3", b"authored fixture A")
            bundle.writestr("second.wav", b"authored fixture B")
            bundle.writestr("unrelated.exe", b"not audio")

    def tearDown(self):
        self.temporary.cleanup()

    def test_zip_paths_do_not_escape_local_staging(self):
        def convert(command, check):
            source = Path(command[command.index("-i") + 1])
            self.assertTrue(source.is_relative_to(self.output))
            self.assertIn("-vn", command)
            Path(command[-1]).write_bytes(source.read_bytes())
        with patch.object(music.subprocess, "run", side_effect=convert):
            result = music.import_music(self.archive, self.output)
        self.assertEqual([file.name for file in result], ["office-01.ogg", "office-02.ogg"])
        self.assertEqual(result[0].read_bytes(), b"authored fixture A")
        self.assertFalse((self.root / "first.mp3").exists())

    def test_conversion_failure_leaves_no_partial_playlist(self):
        def convert(command, check):
            if "input-2" in " ".join(command):
                raise subprocess.CalledProcessError(1, command)
            Path(command[-1]).write_bytes(b"converted fixture")
        with patch.object(music.subprocess, "run", side_effect=convert):
            with self.assertRaises(subprocess.CalledProcessError):
                music.import_music(self.archive, self.output)
        self.assertEqual(list(self.output.iterdir()), [])

    def test_existing_recording_is_never_overwritten(self):
        self.output.mkdir()
        existing = self.output / "office-01.ogg"
        existing.write_bytes(b"keep this recording")
        with patch.object(music.subprocess, "run") as convert:
            with self.assertRaises(ValueError):
                music.import_music(self.archive, self.output)
        convert.assert_not_called()
        self.assertEqual(existing.read_bytes(), b"keep this recording")


if __name__ == "__main__":
    unittest.main()
