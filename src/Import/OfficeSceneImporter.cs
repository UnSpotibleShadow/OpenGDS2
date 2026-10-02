using System.IO;
using System.Security.Cryptography;
using OpenGDS2.Domain;

namespace OpenGDS2.Import;

public static class OfficeSceneImporter
{
    public static OfficeSceneData Extract(byte[] executable)
    {
        if (!Convert.ToHexString(SHA256.HashData(executable)).Equals(EmployeeDatabaseImporter.ExpectedHash, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Unsupported original executable.");
        var pe = new EmployeeDatabaseImporter.Image(executable);
        var result = new OfficeSceneData { SourceHash = EmployeeDatabaseImporter.ExpectedHash };
        const int start = 0x9F651C;
        // The initializer's 128 literal calls to tile allocator 0060EEDC.
        // Search a bounded known code range; each candidate must have all seven
        // literal arguments and the expected 32-byte call signature.
        for (int offset = 0x2BEF; offset <= 0x53F5; offset++)
        {
            int call = pe.Offset(start + offset);
            if (executable[call] != 0x0A || executable[call + 1] != 0x6A ||
                executable[call + 2] != 0 || executable[call + 3] != 0x20 || executable[call + 4] != 0) continue;
            int position = call - 38;
            int[] args = new int[7];
            for (int argument = 6; argument >= 0; argument--)
            {
                ushort local = pe.U16(position + 1);
                args[argument] = pe.Literal16(ref position, local);
            }
            pe.Require(position, 0x04, 0xFC, 0xFD);
            result.Sprites.Add(new(result.Sprites.Count + 23, args[0], args[1], args[2], args[3], args[4], args[5], args[6]));
        }
        if (result.Sprites.Count != 128) throw new InvalidDataException("Office initializer changed.");
        return result;
    }
}
