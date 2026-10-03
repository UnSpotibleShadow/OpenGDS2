using System.Buffers.Binary;
using System.IO;

namespace OpenGDS2.Import;

/// <summary>Known serialized VB6 form bitmaps in the supported executable.
/// These bytes are extracted only into the user's local cache.</summary>
public static class EmbeddedGraphicsImporter
{
    public static void Extract(byte[] executable, string destination)
    {
        WriteBitmap(executable, 0x3B5DC6, 466, 290, destination, "OfficeBase.bmp");
        WriteBitmap(executable, 0x3D7469, 1600, 530, destination, "OfficeSprites.bmp");
        WriteBitmap(executable, 0x4A6960, 1600, 530, destination, "OfficeMasks.bmp");
        // MDIForm1 BGraphicH picture properties, verified in the supplied build.
        int[] toolbar = [0x1AD1, 0x216A, 0x2803, 0x2E9C, 0x3535, 0x3BCC, 0x4265, 0x48FE,
            0x4F97, 0x5630, 0x5CC9, 0x6362, 0x69FB, 0x7094, 0x772D, 0x7DC6];
        for (int index = 0; index < toolbar.Length; index++)
            WriteBitmap(executable, toolbar[index], 23, 22, destination, $"Toolbar{index}.bmp");
    }

    private static void WriteBitmap(byte[] bytes, int offset, int width, int height, string destination, string name)
    {
        if (offset < 0 || offset > bytes.Length - 54) throw new InvalidDataException("Missing embedded bitmap.");
        var header = bytes.AsSpan(offset, 54);
        int size = BinaryPrimitives.ReadInt32LittleEndian(header[2..]);
        int pixelOffset = BinaryPrimitives.ReadInt32LittleEndian(header[10..]);
        if (header[0] != 'B' || header[1] != 'M' || size < 54 || size > bytes.Length - offset ||
            pixelOffset < 54 || pixelOffset >= size ||
            BinaryPrimitives.ReadInt32LittleEndian(header[18..]) != width ||
            BinaryPrimitives.ReadInt32LittleEndian(header[22..]) != height)
            throw new InvalidDataException("Unsupported embedded office graphics.");
        Directory.CreateDirectory(destination);
        File.WriteAllBytes(Path.Combine(destination, name), bytes.AsSpan(offset, size).ToArray());
    }
}
