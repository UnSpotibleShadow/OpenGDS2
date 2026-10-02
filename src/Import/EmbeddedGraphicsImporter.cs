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
