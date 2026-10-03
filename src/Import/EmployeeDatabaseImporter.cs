using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using OpenGDS2.Domain;

namespace OpenGDS2.Import;

/// <summary>Targeted constant extraction; never loads or executes the original PE.</summary>
public static class EmployeeDatabaseImporter
{
    public const string ExpectedHash = "4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6";
    public static EmployeeDatabase Extract(byte[] data)
    {
        if (!Convert.ToHexString(SHA256.HashData(data)).Equals(ExpectedHash, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Unsupported GameDev2DX executable. Expected the game inside the archive, not the installer.");
        var pe = new Image(data);
        var result = new EmployeeDatabase { SourceHash = ExpectedHash, MinimumSourceIndex = 35, SourceUpperBound = 60 };
        const int pool = 0x60DF28, start = 0x9F651C;
        for (int n = 0; n < 8; n++)
        {
            int pos = pe.Offset(start + 0x344 + n * 0x2C);
            pe.Require(pos, 0x3A, 0x2C, 0xFF);
            string given = pe.String(pool, pe.U16(pos + 3));
            pe.Require(pos + 11, 0x3A, 0x6C, 0xFF);
            string family = pe.String(pool, pe.U16(pos + 14));
            result.NameComponents.Add(new() { Given = given, Family = family });
        }
        int limit = pe.Offset(start + 0x2500);
        for (int pos = pe.Offset(start + 0x4A4); pos < limit; pos++)
        {
            if (!pe.Matches(pos, 0xFE, 0xC1, 0x3C, 0xFD)) continue;
            int cursor = pos;
            long fee = pe.Literal32(ref cursor, 0xFD3C), raw40 = pe.Literal32(ref cursor, 0xFD5C);
            int raw3C = pe.Literal16(ref cursor, 0xFD7C), level = pe.Literal16(ref cursor, 0xFD9C);
            int raw34 = pe.Literal16(ref cursor, 0xFDBC);
            var attributes = new List<int>();
            foreach (ushort local in new ushort[] { 0xFDDC, 0xFDFC, 0xFE1C, 0xFE3C, 0xFE5C, 0xFE7C, 0xFEAC })
                attributes.Add(pe.Literal16(ref cursor, local));
            string? name = null;
            bool generated = pe.Matches(cursor, 0x28, 0xBC, 0xFE);
            if (generated)
            {
                if (pe.Literal16(ref cursor, 0xFEBC) != -1) throw new InvalidDataException("Unknown name generation marker.");
            }
            else
            {
                pe.Require(cursor, 0x3A, 0x2C, 0xFF);
                name = pe.String(pool, pe.U16(cursor + 3));
                pe.Require(cursor + 5, 0x4E, 0xBC, 0xFE, 0x04, 0xBC, 0xFE);
                cursor += 11;
            }
            int portrait = pe.Literal16(ref cursor, 0xFECC);
            pe.Require(cursor, 0x04, 0x2C, 0xFD, 0x0A, 0xCF, 0x00, 0x3C, 0x00);
            result.Employees.Add(new()
            {
                SourceIndex = result.Employees.Count, Name = name, GeneratedName = generated,
                PortraitId = portrait, Level = level, RawAttributes = attributes,
                ContractFee = fee, RawParameter3C = raw3C, RawParameter34 = raw34,
                RawParameter40 = raw40, InitialGate = 0, InitialSpecialFlag = result.Employees.Count >= 49 ? 1 : 0,
                DefinitionAddress = $"0x{start + cursor - pe.Offset(start) + 3:X8}"
            });
            pos = cursor + 7;
        }
        // Fresh skill cells are Empty (numeric zero), then selected special rows
        // receive direct skill writes following their employee setter call.
        foreach (var row in result.Employees) row.InitialTechnologyLevels=Enumerable.Range(0,8).ToDictionary(x=>x,_=>0);
        int employeeIndex=-1;
        for (int pos=pe.Offset(start+0x4A4);pos<limit;pos++)
        {
            if (employeeIndex+1<result.Employees.Count && pos==pe.Offset(Convert.ToInt32(result.Employees[employeeIndex+1].DefinitionAddress,16))) employeeIndex++;
            if (!pe.Matches(pos,0x28,0x2C,0xFF) || !pe.Matches(pos+5,0x07,0x08,0x00,0x60,0x0A)) continue;
            pe.Require(pos+10,0x28,0x6C,0xFF,0x01,0x00,0xFB,0x9C,0xCC,0xFE,0xFC,0x22,0xF5);
            pe.Require(pos+26,0x07,0x08,0x00,0x58,0x0D,0xA7,0x02,0x00,0xFD,0x86,0x00,0x00);
            pe.Require(pos+24,0x00,0x00);
            int level=pe.U16(pos+3),technology=pe.U16(pos+22);
            if (employeeIndex<0 || technology>7 || level is <1 or >3) throw new InvalidDataException("Initial technology shape changed.");
            result.Employees[employeeIndex].InitialTechnologyLevels![technology]=level;
        }
        if (result.Employees.Count != 60) throw new InvalidDataException("Employee initializer shape changed.");
        return result;
    }

    internal sealed class Image(byte[] data)
    {
        public ushort U16(int off) => BitConverter.ToUInt16(data, off);
        private uint U32(int off) => BitConverter.ToUInt32(data, off);
        public int Offset(int address)
        {
            int header = checked((int)U32(0x3C)), optional = header + 24;
            long rva = address - U32(optional + 28);
            int count = U16(header + 6), table = optional + U16(header + 20);
            for (int n = 0; n < count; n++)
            {
                int section = table + 40 * n;
                uint begin = U32(section + 12), size = U32(section + 16);
                if (rva >= begin && rva < begin + size)
                    return checked((int)(U32(section + 20) + rva - begin));
            }
            throw new InvalidDataException("Address is not file-backed.");
        }
        public bool Matches(int pos, params byte[] expected) => pos >= 0 && pos + expected.Length <= data.Length && data.AsSpan(pos, expected.Length).SequenceEqual(expected);
        public void Require(int pos, params byte[] expected)
        {
            if (!Matches(pos, expected)) throw new InvalidDataException($"Unexpected initializer at file offset {pos:X}.");
        }
        public int Literal16(ref int pos, ushort local)
        {
            Require(pos, 0x28, (byte)local, (byte)(local >> 8));
            int value = BitConverter.ToInt16(data, pos + 3); pos += 5; return value;
        }
        public int Literal32(ref int pos, ushort local)
        {
            Require(pos, 0xFE, 0xC1, (byte)local, (byte)(local >> 8));
            int value = BitConverter.ToInt32(data, pos + 4); pos += 8; return value;
        }
        public string String(int pool, int index)
        {
            int address = checked((int)U32(Offset(pool + index * 4)));
            int size = checked((int)U32(Offset(address - 4)));
            if (size < 0 || size > 4096 || size % 2 != 0) throw new InvalidDataException("Invalid BSTR.");
            return Encoding.Unicode.GetString(data, Offset(address), size);
        }
    }
}
