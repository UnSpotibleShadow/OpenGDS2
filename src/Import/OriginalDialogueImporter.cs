using System.Security.Cryptography;
using OpenGDS2.Domain;

namespace OpenGDS2.Import;

// Extract text only to the user's local cache. Never executes or ships the PE.
public static class OriginalDialogueImporter
{
    public static OriginalDialogueCatalog Extract(byte[] data)
    {
        if (!Convert.ToHexString(SHA256.HashData(data)).Equals(EmployeeDatabaseImporter.ExpectedHash,
            StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Unsupported dialogue source.");
        var pe = new EmployeeDatabaseImporter.Image(data);
        // fGMSKoyou: floor(Rnd * 10), after employee copy; ten greeting arms.
        pe.Require(pe.Offset(0x99FFC8 + 0xE4),0x27,0x6C,0xFF,0x0A,0x0B,0,4,0,0xF4,0x0A,0xEB,0xB3,0xFB,0xE6);
        var result = new OriginalDialogueCatalog {SourceHash=EmployeeDatabaseImporter.ExpectedHash};
        const int pool = 0x60C264;
        for (int variant = 0; variant < 9; variant++)
            result.Entries.Add($"HIRE_GREETING_{variant}",pe.String(pool,0x11+variant));
        result.Entries.Add("HIRE_GREETING_9",pe.String(pool,0x1A)+"{company}"+pe.String(pool,0x1C));
        result.Entries.Add("NEW_EMPLOYEE",pe.String(pool,0x10));
        return result;
    }
}
