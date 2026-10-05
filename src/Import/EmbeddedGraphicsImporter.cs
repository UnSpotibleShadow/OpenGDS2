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
        // Meeting mode 0 uses MDIForm1.ImageFormG[0], getter 032C.
        const int meetingOffset=0x18B188;
        if (meetingOffset>executable.Length-54 ||
            !executable.AsSpan(meetingOffset-28,13).SequenceEqual(System.Text.Encoding.ASCII.GetBytes("ImageFormG\0\x18\x01")) ||
            BinaryPrimitives.ReadUInt16LittleEndian(executable.AsSpan(meetingOffset-15,2))!=0)
            throw new InvalidDataException("Unsupported embedded planning illustration.");
        WriteBitmap(executable,meetingOffset,256,160,destination,"PlanningMeeting.bmp");
        // MDIForm1 BGraphicH picture properties, verified in the supplied build.
        int[] toolbar = [0x1AD1, 0x216A, 0x2803, 0x2E9C, 0x3535, 0x3BCC, 0x4265, 0x48FE,
            0x4F97, 0x5630, 0x5CC9, 0x6362, 0x69FB, 0x7094, 0x772D, 0x7DC6];
        for (int index = 0; index < toolbar.Length; index++)
            WriteBitmap(executable, toolbar[index], 23, 22, destination, $"Toolbar{index}.bmp");
        // MDIForm1.ImageHard is serialized in descending array-index order.
        // The quote accesses this array by selected source index (009940B8+B0).
        for (int index = 0; index < 15; index++)
        {
            int offset = 0x1A9F3E + (14-index)*0x3069;
            if (offset > executable.Length-54 ||
                !executable.AsSpan(offset-27,12).SequenceEqual(System.Text.Encoding.ASCII.GetBytes("ImageHard\0\x18\x01")) ||
                BinaryPrimitives.ReadUInt16LittleEndian(executable.AsSpan(offset-15,2)) != index)
                throw new InvalidDataException("Unsupported embedded hardware picture array.");
            WriteBitmap(executable, offset, 64, 64, destination, $"Hardware{index}.bmp");
        }
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
