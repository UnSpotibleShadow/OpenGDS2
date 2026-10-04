using OpenGDS2.Domain;

namespace OpenGDS2.Import;

/// <summary>Bounded literal records; all rival strings stay in the user's local cache.</summary>
internal static class CompetitionCatalogImporter
{
    internal static void Extract(EmployeeDatabaseImporter.Image pe,OriginalCatalog catalog)
    {
        int[] initial=[0xAF0A, 0xAF4A, 0xAF8A, 0xAFCA, 0xB00A, 0xB04A, 0xB08A, 0xB0CA, 0xB10A, 0xB14A, 0xB18A, 0xB1CA, 0xB20A, 0xB24A, 0xB28A, 0xB2C7, 0xB307, 0xB347, 0xB387, 0xB3C7, 0xB404, 0xB444, 0xB484, 0xB4C4, 0xB501, 0xB541, 0xB57E, 0xB5BB, 0xB5FB, 0xB63B];
        foreach (int offset in initial) catalog.InitialRivalProducts.Add(Read(pe,0x9F651C,0x60DF28,offset,false));
        int[] staged=[0x2E, 0x79, 0xC4, 0x10F, 0x15A, 0x1A5, 0x1F0, 0x23B, 0x286, 0x2D1, 0x35F, 0x3AA, 0x3F5, 0x440, 0x48B, 0x519, 0x564, 0x5AF, 0x5FA, 0x645, 0x6D3, 0x71E, 0x769, 0x7F7, 0x842, 0x88D, 0x91B, 0x966, 0x9B1, 0xA3F, 0xA8A, 0xAD5, 0xB63, 0xBAE, 0xBF9];
        int[] ages=[0, 2, 4, 6, 8, 10, 14, 18, 22, 26, 0, 4, 7, 10, 14, 0, 5, 7, 11, 15, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2];
        for (int i=0;i<staged.Length;i++) catalog.StagedRivalProducts.Add(new() {TriggerPlatform=i<10 ? 0 : i<15 ? 1 : i<20 ? 2 : i<23 ? 3 : i<26 ? 4 : i<29 ? 5 : i<32 ? 6 : 7,
            Age=ages[i],Product=Read(pe,0x9B5BE0,0x60DF28,staged[i],true)});
        // Module1 0098548C's explicit owner-name switch: ten imported strings.
        int[] names=[0x18,0x32,0x4C,0x66,0x80,0x9A,0xB4,0xCE,0xE8,0x102];
        foreach (int offset in names)
        {
            int pos=pe.Offset(0x98548C+offset);pe.Require(pos,0x3A,0x5C,0xFF);
            catalog.RivalCompanies.Add(pe.String(0x60DF28,pe.U16(pos+3)));
        }
    }
    private static RivalProduct Read(EmployeeDatabaseImporter.Image pe,int start,int pool,int offset,bool staged)
    {
        int pos=pe.Offset(start+offset);
        ushort[] locals=staged ? [0xFEAC,0xFECC,0xFEEC,0xFF0C,0xFF2C] : [0xFE3C,0xFE5C,0xFE7C,0xFEAC,0xFEBC];
        int demand=pe.Literal16(ref pos,locals[0]),power=pe.Literal16(ref pos,locals[1]),hardware=pe.Literal16(ref pos,locals[2]),owner=pe.Literal16(ref pos,locals[3]);
        // Several initial totals fit in I2; the remainder use LitVarI4.
        long units;
        if (staged) units=pe.Literal16(ref pos,locals[4]);
        else if (pe.U16(pos)==0xC1FE) units=pe.Literal32(ref pos,locals[4]);
        else units=pe.Literal16(ref pos,locals[4]);
        pe.Require(pos,0x3A,0x6C,0xFF);string title=pe.String(pool,pe.U16(pos+3));
        ushort local=staged ? (ushort)0xFF5C : (ushort)0xFECC;
        pe.Require(pos+5,0x4E,(byte)local,(byte)(local>>8),0x04,(byte)local,(byte)(local>>8));
        pe.Require(pos+14,0x0A,0x30,0x00,0x1C,0x00);
        return new() {Title=title,UnitsSold=units,Owner=owner,Hardware=hardware,Demand=demand,SalesPower=power};
    }
}
