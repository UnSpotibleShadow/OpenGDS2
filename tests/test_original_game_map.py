"""Accounting/graph regressions for the research map; no original required."""
import copy
import json
from pathlib import Path
import struct
import sys
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))
import map_original_game as mapping


class OriginalMapTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.inventory = mapping.read_json(mapping.INVENTORY)
        cls.catalog = mapping.read_json(mapping.CATALOG)

    def test_complete_accounting_and_repository_links(self):
        self.assertEqual([], mapping.validate(self.inventory, self.catalog))

    def test_unassigned_object_fails(self):
        catalog = copy.deepcopy(self.catalog)
        for workflow in catalog["workflows"]:
            workflow["objects"] = [o for o in workflow["objects"] if o != "object:fGMHelp"]
        self.assertTrue(any("without workflow disposition" in e for e in mapping.validate(self.inventory, catalog)))

    def test_dangling_control_event_fails(self):
        inventory = copy.deepcopy(self.inventory)
        inventory["events"][0]["procedure"] = "procedure:0xDEADBEEF"
        self.assertTrue(any("broken procedure link" in e for e in mapping.validate(inventory, self.catalog)))

    def test_duplicate_id_fails_even_when_count_is_unchanged(self):
        inventory = copy.deepcopy(self.inventory)
        inventory["controls"][1]["id"] = inventory["controls"][0]["id"]
        self.assertIn("duplicate stable ID", mapping.validate(inventory, self.catalog))

    def test_missing_menu_disposition_fails(self):
        catalog = copy.deepcopy(self.catalog)
        catalog["menu_dispositions"].pop()
        self.assertTrue(any("menu dispositions" in e for e in mapping.validate(self.inventory, catalog)))

    def test_missing_reference_is_not_silently_counted_as_complete(self):
        inventory = copy.deepcopy(self.inventory)
        inventory["references"].pop()
        self.assertTrue(any("references: expected" in e for e in mapping.validate(inventory, self.catalog)))

    def test_menu_cycle_or_wrong_depth_fails(self):
        inventory = copy.deepcopy(self.inventory)
        inventory["menus"][0]["parent"] = inventory["menus"][0]["id"]
        self.assertTrue(any("menu hierarchy depth" in e for e in mapping.validate(inventory, self.catalog)))

    def test_runtime_claim_cannot_advance_without_evidence_schema_change(self):
        catalog = copy.deepcopy(self.catalog)
        catalog["workflows"][0]["runtime"] = "verified"
        self.assertTrue(any("runtime claim requires" in e for e in mapping.validate(self.inventory, catalog)))

    def test_wrong_source_hash_fails_before_pe_inspection(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "authored.exe"
            path.write_bytes(b"MZ authored test fixture")
            with self.assertRaisesRegex(mapping.ParseError, "unsupported original"):
                mapping.build(path, {"source_sha256": "0" * 64})

    @staticmethod
    def menu_fixture():
        # Authored synthetic class-19 records; no original bytes are used.
        def record(name, ordinal, children=False):
            marker = struct.pack("<H", len(name)) + name.encode("ascii") + b"\0"
            body = b"\x13\x03\x01\0X\0" + (b"\x07\xff\xff" if children else b"\xff")
            return b"\x02" + struct.pack("<I", 5 + len(marker) + len(body)) + bytes([ordinal]) + marker + body
        names = [("mRoot", 1), ("mChild", 2), ("mSecond", 3)]
        data = b"authored-prefix" + record("mRoot", 1, True) + record("mChild", 2) + b"\x03" + record("mSecond", 3, True) + b"\x03\x03"
        objects = [{"name": "MDIForm1", "controls": [{"name": name, "index": ordinal} for name, ordinal in names]}]
        return data, objects

    def test_menu_hierarchy_uses_delimiters_not_names(self):
        data, objects = self.menu_fixture()
        menus = mapping.recover_menus(data, objects)
        self.assertEqual([None, "menu:mRoot", None], [m["parent"] for m in menus])
        self.assertEqual([0, 1, 0], [m["depth"] for m in menus])

    def test_menu_metadata_mismatch_fails(self):
        data, objects = self.menu_fixture()
        objects[0]["controls"][1]["index"] = 9
        with self.assertRaisesRegex(mapping.ParseError, "ordinal disagrees"):
            mapping.recover_menus(data, objects)

    def test_menu_unknown_delimiter_fails(self):
        data, objects = self.menu_fixture()
        data = data.replace(b"\x03\x02", b"\x08\x02", 1)
        with self.assertRaisesRegex(mapping.ParseError, "unknown menu delimiter"):
            mapping.recover_menus(data, objects)

    def test_source_address_query_finds_backup_purchase_and_event_reads(self):
        result = mapping.query(self.inventory, self.catalog, "0xA112E8")
        self.assertEqual(4, len(result["references"]))
        self.assertEqual({"procedure:0x0098B1F4", "procedure:0x009E3830"}, {r["procedure"] for r in result["references"]})

    def test_report_is_reproducible(self):
        self.assertEqual(mapping.REPORT.read_text(), mapping.render(self.inventory, self.catalog))

    def test_standalone_view_is_reproducible_and_escapes_script_data(self):
        from original_game_map_view import render_view
        self.assertEqual(mapping.VIEW.read_text(), render_view(self.inventory, self.catalog))
        catalog = copy.deepcopy(self.catalog)
        catalog["workflows"][0]["title"] = "</script><script>not executable</script>"
        rendered = render_view(self.inventory, catalog)
        self.assertNotIn("</script><script>not executable", rendered)
        payload = rendered.split('id="data">', 1)[1].split('</script>', 1)[0]
        self.assertEqual(catalog["workflows"][0]["title"], json.loads(payload)["catalog"]["workflows"][0]["title"])


if __name__ == "__main__":
    unittest.main()
