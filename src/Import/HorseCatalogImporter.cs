using OpenGDS2.Domain;
namespace OpenGDS2.Import;
// Native append helpers 00980690 (runner) and 0097C570 (jockey).
internal static class HorseCatalogImporter
{
    internal static void Extract(EmployeeDatabaseImporter.Image pe,OriginalCatalog catalog)
    {
        const int start=0x9F651C,pool=0x60DF28;
        pe.Require(pe.Offset(0x60ED2C),0xBA,0x0C,0x07,0x98,0);
        pe.Require(pe.Offset(0x60ED38),0xBA,0xB0,0xC5,0x97,0);
        int[] calls=[0xB6A3,0xB6D7,0xB70B,0xB73F,0xB773,0xB7AE,0xB7E2,0xB816,0xB84A,0xB87E,0xB8B2,0xB8E6,0xB91A,0xB94E,0xB982,0xB9B6,0xB9EA,0xBA1E,0xBA52,0xBA86,0xBABA,0xBAEE,0xBB22,0xBB56,0xBB8A,0xBBBE,0xBBF2,0xBC26,0xBC5A,0xBC8E,0xBCC2,0xBCF6,0xBD2A,0xBD5E,0xBD92,0xBDC6,0xBDFA,0xBE2E,0xBE62,0xBE96,0xBECA,0xBEFE,0xBF32,0xBF66];
        string ReadString(ref int pos)
        {
            pe.Require(pos,0x3A,0x6C,0xFF);string value=pe.String(pool,pe.U16(pos+3));
            pe.Require(pos+5,0x4E,0xCC,0xFE,0x04,0xCC,0xFE);pos+=11;return value;
        }
        for(int index=0;index<calls.Length;index++)
        {
            int pos=pe.Offset(start+calls[index]-(index==5 ? 36 : 29));
            double parameter=index==5 ? pe.LiteralDouble(ref pos,0xFE7C) : pe.Literal16(ref pos,0xFE7C);
            int weight=pe.Literal16(ref pos,0xFEAC),kind=pe.Literal16(ref pos,0xFEBC);
            string name=ReadString(ref pos);pe.Require(pos,0x04,0x5C,0xFE,0x0A,0xC3,0x02,0x14,0);
            // Helper +0042..0060 stores count + argument4 /100 in member 2CAC.
            catalog.Horses.Add(new() {SourceIndex=index,Name=name,RawWeight=weight,RawKind=kind,RawParameter=parameter,Strength=index+parameter/100});
        }
        for(int index=0;index<10;index++)
        {
            int pos=pe.Offset(start+0xBF90+index*0x26-19);int value=pe.Literal16(ref pos,0xFEBC);
            string name=ReadString(ref pos);pe.Require(pos,0x04,0xAC,0xFE,0x0A,0xF0,0x02,0x0C,0);
            catalog.Jockeys.Add(new() {SourceIndex=index,Name=name,RawValue=value});
        }
    }
}
