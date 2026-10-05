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
        ApplyInitialSecretary(pe, result, start);
        return result;
    }

    private static void ApplyInitialSecretary(EmployeeDatabaseImporter.Image pe, OfficeSceneData scene, int start)
    {
        // The literal tile allocation is only an intermediate state. At the end
        // of Module1.69 the selected secretary rewrites this tile's SourceX.
        var portraits = new int[5];
        for (int index = 0; index < portraits.Length; index++)
        {
            int position = pe.Offset(start + 0x2516 + index * 0x4E);
            portraits[index] = pe.Literal16(ref position, 0xFE5C);
            pe.Require(pe.Offset(start + 0x253E + index * 0x4E), 0x0A, 0xFF, 0x00, 0x1C, 0x00);
        }
        // The allocator's current ID (2388) is saved as secretary tile (08A0).
        pe.Require(pe.Offset(start + 0x5125), 0x0A, 0x6A, 0x00, 0x20, 0x00);
        pe.Require(pe.Offset(start + 0x5142), 0x07, 0x08, 0x00, 0x88, 0x23,
            0xFD, 0xF0, 0x08, 0x00, 0xA0, 0x08);
        int selectionPosition = pe.Offset(start + 0xC0F9);
        int selected = pe.Literal16(ref selectionPosition, 0xFF6C);
        pe.Require(selectionPosition, 0xFD, 0xE6, 0x08, 0x00, 0x90, 0x08);
        // 48 + (portrait[0890] - 1) * 152 -> SourceX[08A0].
        pe.Require(pe.Offset(start + 0xC104), 0x28, 0x1C, 0xFF, 0x30, 0x00,
            0x07, 0x08, 0x00, 0x90, 0x08, 0xFC, 0x22, 0x07, 0x08, 0x00, 0x38, 0x09,
            0xFC, 0x96, 0x28, 0x6C, 0xFF, 0x01, 0x00, 0xFB, 0x9C, 0xCC, 0xFE,
            0x28, 0x2C, 0xFF, 0x98, 0x00, 0xFB, 0xB4, 0xBC, 0xFE, 0xFB, 0x94, 0xAC, 0xFE,
            0x55, 0x07, 0x08, 0x00, 0xA0, 0x08, 0xFC, 0x22,
            0x07, 0x08, 0x00, 0xBC, 0x24, 0xA2);
        if (selected < 0 || selected >= portraits.Length || portraits[selected] is < 1 or > 10)
            throw new InvalidDataException("Initial secretary selection changed.");
        int spriteIndex = scene.Sprites.FindIndex(x => x.Id == 141);
        if (spriteIndex < 0) throw new InvalidDataException("Secretary tile is missing.");
        scene.Sprites[spriteIndex] = scene.Sprites[spriteIndex] with
        {
            SourceX = 48 + (portraits[selected] - 1) * 152
        };
    }
}
