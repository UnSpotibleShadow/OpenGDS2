using System.Security.Cryptography;
using OpenGDS2.Domain;

namespace OpenGDS2.Import;

public static class OriginalCatalogImporter
{
    public static OriginalCatalog Extract(byte[] data)
    {
        if (!Convert.ToHexString(SHA256.HashData(data)).Equals(EmployeeDatabaseImporter.ExpectedHash, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Unsupported catalog executable.");
        var pe = new EmployeeDatabaseImporter.Image(data);
        var result = new OriginalCatalog { SourceHash = EmployeeDatabaseImporter.ExpectedHash };
        const int start = 0x9F651C, pool = 0x60DF28;
        // Genre helper writes seven arguments to members 1240,125C,11B4,
        // 11D0,11EC,1208,1224. Only the name column is semantically identified.
        for (int index = 0; index < 18; index++)
        {
            int pos = pe.Offset(start + 0x6331 + index * 0x4C);
            long member1224 = pe.Literal32(ref pos, 0xFE1C);
            int member1208 = pe.Literal16(ref pos, 0xFE3C);
            int member11EC = pe.Literal16(ref pos, 0xFE5C);
            int member11D0 = pe.Literal16(ref pos, 0xFE7C);
            pe.Require(pos, 0x3A, 0x1C, 0xFF);
            string name = pe.String(pool, pe.U16(pos + 3));
            pe.Require(pos + 5, 0x4E, 0xAC, 0xFE, 0x04, 0xAC, 0xFE); pos += 11;
            int member125C = pe.Literal16(ref pos, 0xFEBC);
            int member1240 = pe.Literal16(ref pos, 0xFECC);
            pe.Require(pos, 0x04, 0xFC, 0xFD, 0x0A, 0x35, 0x01, 0x20, 0x00);
            result.Genres.Add(new GenreDefinition { SourceIndex = index, Name = name,
                DefinitionAddress = $"0x{start + 0x6360 + index * 0x4C:X8}",
                RawMembers = new() { ["1240"] = member1240, ["125C"] = member125C,
                    ["11D0"] = member11D0, ["11EC"] = member11EC, ["1208"] = member1208, ["1224"] = member1224 } });
        }
        // Audience setter: arguments 10..28 -> 14A8,1400,141C,1438,
        // 1454,148C,1470. The first argument is a numeric identifier,
        // not the description; arguments are pushed in reverse order.
        for (int index = 0; index < 7; index++)
        {
            int pos = pe.Offset(start + 0x9B3E + index * 0x52);
            pe.Require(pos, 0x3A, 0x2C, 0xFE);
            string description = pe.String(pool, pe.U16(pos + 3));
            pe.Require(pos + 5, 0x4E, 0x1C, 0xFE, 0x04, 0x1C, 0xFE); pos += 11;
            long member148C = pe.Literal32(ref pos, 0xFE3C);
            int member1454 = pe.Literal16(ref pos, 0xFE5C);
            int member1438 = pe.Literal16(ref pos, 0xFE7C);
            int member141C = pe.Literal16(ref pos, 0xFEAC);
            pe.Require(pos, 0x3A, 0x2C, 0xFF);
            string name = pe.String(pool, pe.U16(pos + 3));
            pe.Require(pos + 5, 0x4E, 0xBC, 0xFE, 0x04, 0xBC, 0xFE); pos += 11;
            int member14A8 = pe.Literal16(ref pos, 0xFECC);
            pe.Require(pos, 0x04, 0xFC, 0xFD, 0x0A, 0x25, 0x02, 0x20, 0x00);
            result.Audiences.Add(new AudienceDefinition { SourceIndex = index, Name = name, Description = description,
                DefinitionAddress = $"0x{start + 0x9B73 + index * 0x52:X8}",
                RawMembers = new() { ["14A8"] = member14A8, ["141C"] = member141C,
                    ["1438"] = member1438, ["1454"] = member1454, ["148C"] = member148C } });
        }
        for (int index = 0; index < 4; index++)
        {
            int pos = pe.Offset(start + 0xA332 + index * 0x51);
            pe.Require(pos, 0x3A, 0x4C, 0xFE);
            string description = pe.String(pool, pe.U16(pos + 3));
            pe.Require(pos + 5, 0x4E, 0x3C, 0xFE, 0x04, 0x3C, 0xFE); pos += 11;
            long price = pe.Literal32(ref pos, 0xFE5C);
            int bits = pe.Literal16(ref pos, 0xFE7C);
            pe.Require(pos, 0x3A, 0x1C, 0xFF);
            string label = pe.String(pool, pe.U16(pos + 3));
            pe.Require(pos + 5, 0x4E, 0xAC, 0xFE, 0x04, 0xAC, 0xFE); pos += 11;
            pe.Require(pos, 0x3A, 0x2C, 0xFF);
            string name = pe.String(pool, pe.U16(pos + 3));
            pe.Require(pos + 5, 0x4E, 0xBC, 0xFE, 0x04, 0xBC, 0xFE); pos += 11;
            int id = pe.Literal16(ref pos, 0xFECC);
            pe.Require(pos, 0x04, 0x1C, 0xFE, 0x0A, 0x49, 0x02, 0x1C, 0x00);
            if (id != index) throw new InvalidDataException("Computer index changed.");
            result.DevelopmentComputers.Add(new() { SourceIndex = id, Name = name, Description = description,
                BitLabel = label, BitWidth = bits, PurchasePrice = price });
        }
        int[] hardwareCalls = [0x54F3,0x5644,0x5795,0x5905,0x5A58,0x5BA9,0x5CFA,0x5E4B,0x5F9C,0x60ED,0x623E];
        string[] members = ["1034","0F70","0FFC","0FE0","0F54","0F38","1050","0F1C","0FC4","0F00","0FA8","0EE4","0EC8"];
        for (int index = 0; index < hardwareCalls.Length; index++)
        {
            int pos = pe.Offset(start + hardwareCalls[index] - 0x59);
            var hardware = new HardwareDefinition { SourceIndex = index };
            // 009874E8 arguments 48/4C -> members 10A4/10C0. The fourth
            // record builds a concatenated description, so its prefix differs.
            int growthPos=pe.Offset(start+hardwareCalls[index]-(index==3 ? 0x6F : 0x6E));
            hardware.RawMembers["10C0"]=pe.Literal16(ref growthPos,index==3 ? (ushort)0xFBFC : (ushort)0xFC2C);
            hardware.RawMembers["10A4"]=pe.Literal16(ref growthPos,index==3 ? (ushort)0xFC1C : (ushort)0xFD2C);
            foreach (string member in members)
            {
                if (member is "1034" or "0EE4")
                {
                    pe.Require(pos, 0x3A);
                    string value = pe.String(pool, pe.U16(pos + 3));
                    pe.Require(pos + 5, 0x4E);
                    ushort local = pe.U16(pos + 6);
                    pe.Require(pos + 8, 0x04, (byte)local, (byte)(local >> 8)); pos += 11;
                    if (member == "1034") hardware.Description = value; else hardware.Name = value;
                }
                else
                {
                    bool wide = member is "0F70" or "0FFC" or "0FC4";
                    ushort local = pe.U16(pos + (wide ? 2 : 1));
                    hardware.RawMembers[member] = wide ? pe.Literal32(ref pos, local) : pe.Literal16(ref pos, local);
                }
            }
            pe.Require(pos, 0x04); pos += 3;
            pe.Require(pos, 0x0A, 0x0C, 0x01, 0x44, 0x00);
            // Four calls to 0060ED44 populate size/cost tables 10F8/111C.
            int bandOffset=hardwareCalls[index]+(index==3 ? 0x31 : 0x2F);
            for (int band=0;band<4;band++)
            {
                int bp=pe.Offset(start+bandOffset+band*0x2D);
                long unitCost=pe.Literal16(ref bp,0xFEAC);
                int threshold=pe.Literal16(ref bp,0xFEBC);
                pe.Require(bp,0x3A,0x6C,0xFF);
                pe.Require(bp+14,0x0A,0x0E,0x01,0x10,0x00);
                hardware.ProductionBands.Add(new(threshold,unitCost));
            }
            result.Hardware.Add(hardware);
        }
        string[] typeMembers = ["135C","1340","1324","1378","1308","12EC","12D0","12B4","13B0","1394"];
        for (int index = 0; index < 51; index++)
        {
            int pos = pe.Offset(start + 0x6889 + index * 0xF5);
            var type = new GameTypeDefinition { SourceIndex = index };
            var examples = new Dictionary<string,string>();
            foreach (string member in typeMembers)
            {
                if (member is "135C" or "1340" or "1324" or "12B4")
                {
                    pe.Require(pos, 0x3A);
                    string value = pe.String(pool, pe.U16(pos + 3));
                    pe.Require(pos + 5, 0x4E);
                    ushort local = pe.U16(pos + 6);
                    pe.Require(pos + 8, 0x04, (byte)local, (byte)(local >> 8)); pos += 11;
                    if (member == "12B4") type.Name = value; else examples[member] = value;
                }
                else
                {
                    bool wide = member == "1378";
                    ushort local = pe.U16(pos + (wide ? 2 : 1));
                    type.RawMembers[member] = wide ? pe.Literal32(ref pos, local) : pe.Literal16(ref pos, local);
                }
            }
            pe.Require(pos, 0x04); pos += 3;
            pe.Require(pos, 0x0A, 0x4B, 0x01, 0x2C, 0x00);
            type.ExampleTitles = [examples["1324"], examples["1340"], examples["135C"]];
            pos = pe.Offset(start + 0x68FC + index * 0xF5);
            for (int column = 0; column < 16; column++)
            {
                ushort local = pe.U16(pos + 1);
                type.RawTable13CC.Add(pe.Literal16(ref pos, local));
            }
            type.RawTable13CC.Reverse();
            // Helper 0098F178 does not consume argument 48. Argument 4C
            // populates column 14; there is no column 15 write in this helper.
            type.RawUnusedArgument48 = type.RawTable13CC[14];
            type.RawTable13CC.RemoveAt(14);
            pe.Require(pos, 0x04); pos += 3;
            pe.Require(pos, 0x0A, 0x4C, 0x01, 0x44, 0x00);
            result.GameTypes.Add(type);
        }
        // Technology helper 0098BA28: seventeen arguments plus result storage.
        for (int index=0;index<8;index++)
        {
            int pos=pe.Offset(start+0x9D7C+index*0x98);
            pe.Require(pos,0x3A,0x0C,0xFC);
            string description=pe.String(pool,pe.U16(pos+3));
            pe.Require(pos+5,0x4E,0x1C,0xFC,0x04,0x1C,0xFC);pos+=11;
            int ownership=pe.Literal16(ref pos,0xFC2C);
            long fee=pe.Literal32(ref pos,0xFD2C);
            var numeric=new List<int>();
            foreach (ushort local in new ushort[] {0xFD3C,0xFD5C,0xFD7C,0xFD9C,0xFDBC,0xFDDC,
                0xFDFC,0xFE1C,0xFE3C,0xFE5C,0xFE7C,0xFEAC}) numeric.Add(pe.Literal16(ref pos,local));
            numeric.Reverse();
            pe.Require(pos,0x3A,0x2C,0xFF);string name=pe.String(pool,pe.U16(pos+3));
            pe.Require(pos+5,0x4E,0xBC,0xFE,0x04,0xBC,0xFE);pos+=11;
            int identifier=pe.Literal16(ref pos,0xFECC);
            pe.Require(pos,0x04,0xFC,0xFB,0x0A,0x30,0x02,0x48,0x00);
            result.Technologies.Add(new() {SourceIndex=index,Identifier=identifier,Name=name,Description=description,
                InitialOwnershipLevel=ownership,PurchaseFee=fee,RequiredTrainingWork=numeric.GetRange(0,3),
                ExperienceCosts=numeric.GetRange(3,3),RawTable158C=numeric.GetRange(6,3),RawTable1568=numeric.GetRange(9,3)});
        }
        // Module1.26: seven arguments -> name/rank/contract/salary/portrait/description.
        for (int index=0;index<5;index++)
        {
            int pos=pe.Offset(start+0x250B+index*0x4E);
            pe.Require(pos,0x3A,0x4C,0xFE);
            string description=pe.String(pool,pe.U16(pos+3));
            pe.Require(pos+5,0x4E,0x3C,0xFE,0x04,0x3C,0xFE);pos+=11;
            int portrait=pe.Literal16(ref pos,0xFE5C);
            long salary=pe.Literal32(ref pos,0xFE7C);
            long fee=pe.Literal32(ref pos,0xFEAC);
            int rank=pe.Literal16(ref pos,0xFEBC);
            pe.Require(pos,0x3A,0x6C,0xFF);
            string name=pe.String(pool,pe.U16(pos+3));
            pe.Require(pos+5,0x4E,0xCC,0xFE,0x04,0xCC,0xFE,0x04,0x1C,0xFE,0x0A,0xFF,0,0x1C,0);
            result.Secretaries.Add(new() {SourceIndex=index,Name=name,RankCode=rank,ContractFee=fee,
                AdvertisedSalary=salary,PortraitId=portrait,Description=description});
        }
        if (!SecretaryManagementRules.HasCatalog(result)) throw new InvalidDataException("Incomplete secretary catalog.");
        CompetitionCatalogImporter.Extract(pe,result);
        // 31 fixed-width calls to 0060ECA8, whose thunk points to the
        // code-info for Module1 helper 20 (0098128C).
        pe.Require(pe.Offset(0x60ECA8),0xBA,0x1C,0x13,0x98,0);
        for (int index=0;index<31;index++)
        {
            int pos=pe.Offset(start+0xA481+index*0x57);
            long fee=pe.Literal32(ref pos,0xFE3C);
            string ReadString(ushort local)
            {
                pe.Require(pos,0x3A,(byte)local,(byte)(local>>8));
                string value=pe.String(pool,pe.U16(pos+3));
                pe.Require(pos+5,0x4E);ushort stored=pe.U16(pos+6);
                pe.Require(pos+8,0x04,(byte)stored,(byte)(stored>>8));pos+=11;
                return value;
            }
            string publisher=ReadString(0xFE6C);
            int popularity=pe.Literal16(ref pos,0xFE7C);
            string description=ReadString(0xFF1C),author=ReadString(0xFF2C),title=ReadString(0xFF6C);
            pe.Require(pos,0x04,0x1C,0xFE,0x0A,0x57,0x02,0x1C,0);
            if (fee<0 || popularity<0) throw new InvalidDataException("Invalid original manga record.");
            result.Manga.Add(new() {SourceIndex=index,Title=title,Author=author,
                PopularityDescription=description,Popularity=popularity,Publisher=publisher,RightsFee=fee});
        }
        HorseCatalogImporter.Extract(pe,result);
        return result;
    }
}
