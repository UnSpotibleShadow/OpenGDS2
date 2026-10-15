import json
from pathlib import Path
import struct
import sys
import unittest

sys.path.insert(0,str(Path(__file__).resolve().parents[1]/'tools'))
from verify_serialized_properties import parse


class SerializedPropertyTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.schemas=json.loads((Path(__file__).resolve().parents[1]/'tools/vb6_property_types.json').read_text())['classes']

    def test_timer_design_coordinates_are_individual_int32_values(self):
        raw=b'\x03'+struct.pack('<I',100)+b'\x07'+struct.pack('<i',120)+b'\x08'+struct.pack('<i',240)+b'\xff'
        parsed=parse(raw,0,len(raw),'Timer',self.schemas)
        self.assertEqual([f['value'] for f in parsed['fields']],[100,120,240])
        self.assertEqual(parsed['end'],len(raw))

    def test_mdi_root_begins_with_client_rectangle_without_form_class_byte(self):
        raw=bytes([53])+struct.pack('<iiii',100,200,12000,9000)+b'\xff'
        parsed=parse(raw,0,len(raw),'Form',self.schemas)
        self.assertEqual(parsed['fields'][0]['name'],'ClientLeft')
        self.assertEqual(parsed['fields'][0]['rectangle_twips'],[100,200,12000,9000])

    def test_object_and_reserved_zero_markers_stay_explicitly_opaque(self):
        raw=bytes([67,0,99,0,50,2,255]);parsed=parse(raw,0,len(raw),'Pix',self.schemas)
        self.assertEqual([p['index'] for p in parsed['opaque']],[67,99])
        self.assertEqual(parsed['fields'][-1]['name'],'Align')
        for raw in [bytes([67,1,255]),bytes([99,1,255])]:
            with self.assertRaises(ValueError):parse(raw,0,len(raw),'Pix',self.schemas)

    def test_variable_payloads_cannot_escape_the_record_or_hide_a_terminator(self):
        for raw in [b'\x01\x08\x00short',b'\x21\x10\x00\x00\x00short',b'\x03\x64\x00\x00\x00']:
            with self.assertRaises(ValueError):parse(raw,0,len(raw),'Form' if raw[0]!=3 else 'Timer',self.schemas)


if __name__=='__main__':unittest.main()
