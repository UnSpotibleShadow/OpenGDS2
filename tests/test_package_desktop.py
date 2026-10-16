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

    def public_fixture(self, version="0.1.0-ea.1", directory="OpenGDS2-EarlyAccessTesting"):
        (self.source / "project.godot").write_text(
            f'config/version="{version}"\nconfig/use_custom_user_dir=true\n'
            f'config/custom_user_dir_name="{directory}"\n')
        docs = self.source / "docs"
        docs.mkdir(exist_ok=True)
        for name in ["testing", "known-issues", "release-notes", "decisions"]:
            (docs / ("early-access-" + name + ".md")).write_text("Authored historical candidate fixture")
        (docs / "early-access-public-notes.md").write_text(
            "Public @VERSION@\nReport: @REPORT_URL@\nData: @DATA_DIRECTORY@\n")

    def public_package(self, **kwargs):
        args = dict(early_access=True, source_commit="b" * 40, report_url="https://example.test/bugs")
        args.update(kwargs)
        with patch.dict(os.environ, {"NUGET_PACKAGES": str(self.nuget)}):
            desktop.package(self.source, self.export, "windows", self.notices, self.output, **args)

    def test_public_ea_contains_provenance_reporting_notices_and_verified_checksums(self):
        self.public_fixture(directory="OpenGDS2-AuthoredTest")
        self.public_package()
        with zipfile.ZipFile(self.output) as archive:
            prefix = "OpenGDS2-windows-0.1.0-ea.1/"
            manifest = json.loads(archive.read(prefix + "BUILD_MANIFEST.json"))
            self.assertFalse(manifest["private_early_access_candidate"])
            self.assertTrue(manifest["public_early_access"])
            self.assertEqual(manifest["source_commit"], "b" * 40)
            self.assertEqual(manifest["data_directory"], "OpenGDS2-AuthoredTest")
            self.assertEqual(manifest["community_report_url"], "https://example.test/bugs")
            self.assertEqual(manifest["native_windows_acceptance"], "incomplete; community testing")
            self.assertEqual(manifest["assembly_sha256"], hashlib.sha256(archive.read(prefix + "OpenGDS2.dll")).hexdigest())
            notes = archive.read(prefix + "RELEASE_NOTES.md").decode()
            self.assertIn("Public 0.1.0-ea.1", notes)
            self.assertIn("https://example.test/bugs", notes)
            self.assertNotIn("@DATA_DIRECTORY@", notes)
            self.assertIn("OpenGDS2-AuthoredTest", archive.read(prefix + "GETTING_STARTED.txt").decode())
            self.assertIn(prefix + "licenses/GODOT_LICENSE.txt", archive.namelist())
            self.assertIn(prefix + "licenses/microsoft.netcore.app.runtime.win-x64-8.0.0-license.txt", archive.namelist())
            for line in archive.read(prefix + "SHA256SUMS").decode().splitlines():
                digest, name = line.split("  ", 1)
                self.assertEqual(hashlib.sha256(archive.read(prefix + name)).hexdigest(), digest)

    def test_public_documents_use_current_source_copy_and_working_links(self):
        self.public_fixture(directory="OpenGDS2-AuthoredTest")
        repository = Path(__file__).parents[1]
        for name in ["testing", "known-issues", "decisions", "public-notes"]:
            filename = f"early-access-{name}.md"
            (self.source / "docs" / filename).write_text((repository / "docs" / filename).read_text())
        self.public_package()
        with zipfile.ZipFile(self.output) as archive:
            prefix = "OpenGDS2-windows-0.1.0-ea.1/"
            names = set(archive.namelist())
            for filename in ["KNOWN_ISSUES.md", "TEST_CHECKLIST.md", "DECISIONS.md", "RELEASE_NOTES.md"]:
                text = archive.read(prefix + filename).decode()
                self.assertIn("Packaged build: 0.1.0-ea.1", text)
                self.assertIn("%APPDATA%\\OpenGDS2-AuthoredTest", text)
                self.assertIn("https://example.test/bugs", text)
                self.assertNotIn("@VERSION@", text)
                self.assertNotIn("@DATA_DIRECTORY@", text)
                for destination in desktop.re.findall(r"\]\(([^)\s]+)\)", text):
                    if not destination.startswith(("https://", "http://", "#")):
                        self.assertIn(prefix + destination.split("#", 1)[0], names)
            issues = archive.read(prefix + "KNOWN_ISSUES.md").decode()
            checklist = archive.read(prefix + "TEST_CHECKLIST.md").decode()
            self.assertIn("# Known limitations — 0.1.0-ea.1", issues)
            self.assertIn("](TEST_CHECKLIST.md)", issues)
            self.assertIn("](DECISIONS.md)", issues)
            self.assertNotIn("as a private Early Access test candidate", checklist)
            self.assertNotIn("RC.4 baseline", checklist)
            self.assertIn("must match BUILD_MANIFEST.json", checklist)

    def test_public_known_issue_heading_handles_the_historical_source_shape(self):
        self.public_fixture()
        (self.source / "docs" / "early-access-known-issues.md").write_text(
            "# Known limitations — current source / historical RC.4\n\n"
            "Historical evidence stays intact. See [evidence](financial-failure-m84.md#details).\n")
        self.public_package()
        with zipfile.ZipFile(self.output) as archive:
            text = archive.read("OpenGDS2-windows-0.1.0-ea.1/KNOWN_ISSUES.md").decode()
            self.assertIn("# Known limitations — 0.1.0-ea.1", text)
            self.assertNotIn("# Known limitations — current source / historical RC.4", text)
            self.assertIn("Historical evidence stays intact.", text)
            self.assertIn("https://github.com/UnSpotibleShadow/OpenGDS2/blob/" + "b" * 40 + "/docs/financial-failure-m84.md#details", text)

    def test_public_ea_rejects_rc_version_debug_missing_commit_and_invalid_reporting_url(self):
        self.public_fixture(version="0.1.0-ea.1-rc.4")
        with self.assertRaisesRegex(ValueError, "final EA version"):
            self.public_package()
        self.public_fixture()
        for kwargs in [{"configuration": "debug"}, {"source_commit": None},
                       {"report_url": None}, {"report_url": "http://example.test/bugs"},
                       {"report_url": "https://example.test/bug report"}, {"candidate": True}]:
            with self.subTest(kwargs=kwargs), self.assertRaises(ValueError):
                self.public_package(**kwargs)
        self.assertFalse(self.output.exists())

    def test_public_ea_requires_an_explicit_data_directory(self):
        self.public_fixture()
        (self.source / "project.godot").write_text('config/version="0.1.0-ea.1"\n')
        with self.assertRaisesRegex(ValueError, "data directory"):
            self.public_package()
        self.assertFalse(self.output.exists())

    def test_public_ea_rejects_original_imports(self):
        self.public_fixture()
        (self.export / "GameDev2DX.lzh").write_bytes(b"Authored exclusion fixture")
        with self.assertRaisesRegex(ValueError, "Original-game content"):
            self.public_package()
        self.assertFalse(self.output.exists())

    def test_public_ea_replaces_private_candidate_handoff_when_reusing_export_folder(self):
        self.public_fixture(version="0.1.0-ea.1-rc.4")
        with patch.dict(os.environ, {"NUGET_PACKAGES": str(self.nuget)}):
            desktop.package(self.source, self.export, "windows", self.notices,
                            self.output, candidate=True, source_commit="a" * 40)
        self.public_fixture()
        self.public_package()
        with zipfile.ZipFile(self.output) as archive:
            prefix = "OpenGDS2-windows-0.1.0-ea.1/"
            self.assertNotIn("PRIVATE EARLY ACCESS", archive.read(prefix + "GETTING_STARTED.txt").decode())
            self.assertNotIn("historical candidate", archive.read(prefix + "RELEASE_NOTES.md").decode())
            self.assertFalse(json.loads(archive.read(prefix + "BUILD_MANIFEST.json"))["private_early_access_candidate"])
