"""Desktop package diagnostics and content boundaries use authored fixtures."""
import importlib.util
import hashlib
import json
import os
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
import zipfile

spec = importlib.util.spec_from_file_location("package_desktop", Path(__file__).parents[1] / "tools/package_desktop.py")
desktop = importlib.util.module_from_spec(spec)
spec.loader.exec_module(desktop)


class DesktopPackageTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.root = Path(self.temporary.name)
        self.source = self.root / "source"
        self.export = self.root / "export"
        self.notices = self.root / "notices"
        self.nuget = self.root / "nuget"
        for directory in [self.source, self.export, self.notices]:
            directory.mkdir()
        for name in ["LICENSE", "notice.md"]:
            (self.source / name).write_text("Community fixture notice")
        for name in ["GODOT_LICENSE.txt", "GODOT_THIRD_PARTY.txt"]:
            (self.notices / name).write_text("Engine fixture notice")
        for name in ["OpenGDS2.exe", "OpenGDS2.pck", "OpenGDS2.dll"]:
            (self.export / name).write_bytes(b"Authored community fixture")
        (self.export / "OpenGDS2.deps.json").write_text(json.dumps({"libraries": {
            "runtimepack.Microsoft.NETCore.App.Runtime.win-x64/8.0.0": {}}}))
        pack = self.nuget / "microsoft.netcore.app.runtime.win-x64" / "8.0.0"
        pack.mkdir(parents=True)
        for name in ["LICENSE.TXT", "THIRD-PARTY-NOTICES.TXT"]:
            (pack / name).write_text("Runtime fixture notice")
        self.output = self.root / "package.zip"

    def tearDown(self):
        self.temporary.cleanup()

    def package(self, configuration="release"):
        with patch.dict(os.environ, {"NUGET_PACKAGES": str(self.nuget)}):
            desktop.package(self.source, self.export, "windows", self.notices, self.output, configuration)

    def test_debug_package_requires_and_preserves_game_symbols(self):
        with self.assertRaisesRegex(ValueError, "symbols"):
            self.package("debug")
        self.assertFalse(self.output.exists())
        (self.export / "OpenGDS2.pdb").write_bytes(b"Authored managed-symbol fixture")
        self.package("debug")
        with zipfile.ZipFile(self.output) as archive:
            prefix = "OpenGDS2-windows-x86_64-debug/"
            self.assertEqual(archive.read(prefix + "OpenGDS2.pdb"), b"Authored managed-symbol fixture")
            self.assertIn("--log-file", archive.read(prefix + "GETTING_STARTED.txt").decode())
            self.assertIn("OpenGDS2.pdb", archive.read(prefix + "SHA256SUMS").decode())

    def test_release_package_retains_its_existing_layout_without_symbols(self):
        self.package()
        with zipfile.ZipFile(self.output) as archive:
            self.assertIn("OpenGDS2-windows-x86_64/OpenGDS2.exe", archive.namelist())
            self.assertFalse(any("-debug/" in name for name in archive.namelist()))

    def test_debug_package_rejects_imported_original_cache(self):
        (self.export / "OpenGDS2.pdb").write_bytes(b"Authored symbol fixture")
        cache = self.export / "original"
        cache.mkdir()
        (cache / "dialogue.json").write_text("Private fixture must not enter package")
        with self.assertRaisesRegex(ValueError, "private/cache"):
            self.package("debug")
        self.assertFalse(self.output.exists())

    def test_candidate_records_source_and_handoff_in_checked_archive(self):
        (self.source / "project.godot").write_text('config/version="0.1.0-ea.1-rc.4"\n')
        docs = self.source / "docs"
        docs.mkdir()
        for name in ["testing", "known-issues", "release-notes", "decisions"]:
            (docs / ("early-access-"+name+".md")).write_text("Authored candidate fixture")
        commit = "a" * 40
        with patch.dict(os.environ, {"NUGET_PACKAGES": str(self.nuget)}):
            desktop.package(self.source, self.export, "windows", self.notices,
                            self.output, candidate=True, source_commit=commit)
        with zipfile.ZipFile(self.output) as archive:
            prefix = "OpenGDS2-windows-0.1.0-ea.1-rc.4/"
            manifest = json.loads(archive.read(prefix+"BUILD_MANIFEST.json"))
            self.assertEqual(manifest["source_commit"], commit)
            self.assertEqual(manifest["runtime_packs"], ["microsoft.netcore.app.runtime.win-x64/8.0.0"])
            self.assertEqual(manifest["native_windows_acceptance"], "pending owner testing")
            self.assertEqual(archive.read(prefix+"DECISIONS.md"), b"Authored candidate fixture")
            for line in archive.read(prefix+"SHA256SUMS").decode().splitlines():
                digest, name = line.split("  ", 1)
                self.assertEqual(hashlib.sha256(archive.read(prefix+name)).hexdigest(), digest)

    def test_candidate_rejects_debug_and_unidentified_source(self):
        (self.source / "project.godot").write_text('config/version="0.1.0-ea.1-rc.4"\n')
        for configuration, commit in [("debug", "a" * 40), ("release", None)]:
            with self.assertRaisesRegex(ValueError, "candidate requires"):
                desktop.package(self.source, self.export, "windows", self.notices,
                                self.output, configuration, True, commit)
        self.assertFalse(self.output.exists())
