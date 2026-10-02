import hashlib, json, os, struct, sys, unittest
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/'tools'))
from inspect_vb6 import PeImage, ParseError
from trace_hiring_pcode import Instruction, _decode_one, pool_index
from recover_employee_database import recover, bstr
from verify_m7_evidence import verify
from recover_genre_catalog import recover as recover_genres
from verify_office_evidence import verify as verify_office
from verify_development_evidence import verify as verify_development
from verify_core_loop_evidence import verify as verify_core_loop

def synthetic_pe():
    data=bytearray(1024);data[:2]=b'MZ';struct.pack_into('<I',data,0x3c,0x80)
    data[0x80:0x84]=b'PE\0\0';struct.pack_into('<H',data,0x86,1);struct.pack_into('<H',data,0x94,0xe0)
    struct.pack_into('<H',data,0x98,0x10b);struct.pack_into('<I',data,0x98+28,0x400000)
    section=0x178;data[section:section+8]=b'.data\0\0\0'
    struct.pack_into('<IIII',data,section+8,0x200,0x1000,0x200,0x200)
    return data

class PublicTests(unittest.TestCase):
    def test_pe_rejects_non_pe(self):
        with self.assertRaises(ParseError):PeImage(b'xx'+b'\0'*200)
    def test_va_mapping(self):
        pe=PeImage(synthetic_pe());self.assertEqual(pe.va_to_offset(0x401080),0x280)
        with self.assertRaises(ParseError):pe.va_to_offset(0x402000)
    def test_virtual_zero_fill_is_not_file_data(self):
        data=synthetic_pe()+bytes(2048)
        struct.pack_into('<I',data,0x178+8,0x400)
        with self.assertRaises(ParseError):PeImage(data).va_to_offset(0x401250)
        struct.pack_into('<II',data,0x178+16,0,0)
        with self.assertRaises(ParseError):PeImage(data).u32(0x401000)
    def test_reads_cannot_cross_section_or_silently_truncate(self):
        pe=PeImage(synthetic_pe())
        with self.assertRaises(ParseError):pe.bytes_at(0x4011F8,16)
        with self.assertRaises(ParseError):pe.bytes_at(0x401080,-1)
    def test_literal32_opcode(self):
        i,pos=_decode_one(bytes.fromhex('fe c1 3c fd 00 2d 31 01'),0)
        self.assertEqual(pos,8);self.assertEqual(i.mnemonic,'LitVarI4')
        self.assertEqual(struct.unpack_from('<i',i.raw,4)[0],20_000_000)
    def test_truncated_opcode(self):
        with self.assertRaises(ParseError):_decode_one(b'\xfe\xc1\x00',0)
    def test_prefixed_pool_operand(self):
        i,pos=_decode_one(bytes.fromhex('fd b6 11 00'),0);self.assertEqual(pool_index(i),0x11)
    def test_wrapper_or_unknown_hash_rejected(self):
        with self.assertRaises(ParseError):recover(b'MZ'+bytes(200))
        with self.assertRaises(ParseError):verify_office(b'MZ'+bytes(200))
        with self.assertRaises(ParseError):verify_development(b'MZ'+bytes(200))
        with self.assertRaises(ParseError):verify_core_loop(b'MZ'+bytes(200))
    def test_localization_key_parity(self):
        import re
        keys=[]
        for language in ['en','ja']:
            text=(ROOT/'localization'/f'{language}.po').read_text()
            ids=re.findall(r'^msgid "(.*)"$',text,re.M)
            self.assertEqual(len(ids),len(set(ids)),language+' duplicate msgid')
            keys.append(set(ids))
        self.assertEqual(keys[0],keys[1])
    def test_public_tree_no_original_files(self):
        for path in ROOT.rglob('*'):
            if any(part in ['.git','.godot','bin','obj','__pycache__'] for part in path.parts):continue
            if path.is_file():self.assertNotIn(path.suffix.lower(),['.exe','.bmp','.wav','.lzh','.gds','.dll'])

@unittest.skipUnless(os.environ.get('OPENGDS2_ORIGINAL_EXE'),'Original fixture is opt-in and never redistributed')
class OriginalTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):cls.data=Path(os.environ['OPENGDS2_ORIGINAL_EXE']).read_bytes();cls.database=verify(cls.data)
    def test_database_shape(self):
        d=self.database;self.assertEqual(len(d['Employees']),60);self.assertEqual(d['MinimumSourceIndex'],35)
        self.assertEqual(sum(x['GeneratedName'] for x in d['Employees']),14)
        self.assertEqual(len(d['NameComponents']),8)
    def test_office_behavior_anchors(self):
        self.assertTrue(verify_office(self.data))
    def test_development_behavior_anchors(self):
        self.assertTrue(verify_development(self.data))
    def test_core_loop_behavior_anchors(self):
        self.assertTrue(verify_core_loop(self.data))
    def test_complete_and_isolated_rows(self):
        rows=self.database['Employees'];self.assertEqual([x['SourceIndex'] for x in rows],list(range(60)))
        self.assertTrue(all(len(x['RawAttributes'])==7 and x['ContractFee']>=0 for x in rows))
        self.assertEqual(rows[35]['RawAttributes'],[80,82,79,76,83,88,81])
        self.assertEqual(rows[35]['ContractFee'],20_000_000)
        self.assertEqual(rows[59]['Level'],2)
        self.assertEqual(sum(x['InitialSpecialFlag'] for x in rows),11)
        self.assertEqual(rows[48]['InitialSpecialFlag'],0);self.assertEqual(rows[49]['InitialSpecialFlag'],1)
    def test_tampered_fixture_rejected(self):
        data=bytearray(self.data);data[-1]^=1
        with self.assertRaises(ParseError):recover(data)
    def test_genre_catalog(self):
        genres=recover_genres(self.data)['Genres'];self.assertEqual(len(genres),18)
        self.assertEqual([x['SourceIndex'] for x in genres],list(range(18)))
        self.assertTrue(all(len(x['RawMembers'])==6 and x['Name'] for x in genres))
        self.assertEqual(genres[0]['RawMembers']['1224'],8_000_000)
        audiences=recover_genres(self.data)['Audiences']
        self.assertEqual(len(audiences),7)
        self.assertEqual([row['RawMembers']['14A8'] for row in audiences],list(range(1,8)))
        self.assertTrue(all(row['Name'] and row['Description'] and len(row['RawMembers'])==5 for row in audiences))
        catalog=recover_genres(self.data)
        self.assertEqual([row['BitWidth'] for row in catalog['DevelopmentComputers']],[8,16,32,64])
        self.assertEqual(len(catalog['Hardware']),11)
        self.assertEqual([row['SourceIndex'] for row in catalog['Hardware'] if row['RawMembers']['0F1C']==1],[0,2,4])
        self.assertEqual(len(catalog['GameTypes']),51)
        self.assertTrue(all(len(row['RawTable13CC'])==15 and len(row['ExampleTitles'])==3 and len(row['RawMembers'])==6 for row in catalog['GameTypes']))
if __name__=='__main__':unittest.main()
