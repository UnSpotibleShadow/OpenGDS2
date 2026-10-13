using System.Buffers.Binary;
using System.Text.Json;
namespace OpenGDS2.Import;
public sealed record SerializedArtworkRecord(int Offset,int Width,int Height,string Format,string File);
/// <summary>Bounded serialized BMP/GIF inventory; assets remain in the private cache.</summary>
public static class SerializedArtworkImporter
{
    public static IReadOnlyList<SerializedArtworkRecord> Extract(byte[] bytes,string destination)
    {
        Directory.CreateDirectory(destination);var records=new List<SerializedArtworkRecord>();
        for(int offset=4;offset<bytes.Length-54;offset++)
        {
            var data=bytes.AsSpan(offset);bool bmp=data[0]==66&&data[1]==77;
            bool gif=data[..6].SequenceEqual("GIF87a"u8)||data[..6].SequenceEqual("GIF89a"u8);
            if(!bmp&&!gif)continue;
            int width,height,size;uint compression=0;int bits=0;
            if(bmp)
            {
                size=BinaryPrimitives.ReadInt32LittleEndian(data[2..]);
                if(BinaryPrimitives.ReadInt32LittleEndian(data[14..])!=40||size<54||size>bytes.Length-offset)continue;
                width=BinaryPrimitives.ReadInt32LittleEndian(data[18..]);height=BinaryPrimitives.ReadInt32LittleEndian(data[22..]);
                bits=BinaryPrimitives.ReadUInt16LittleEndian(data[28..]);compression=BinaryPrimitives.ReadUInt32LittleEndian(data[30..]);
                if(bits is not (8 or 24 or 32)||compression is not (0 or 1))continue;
            }
            else
            {
                size=BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(offset-4));
                if(size<13||size>bytes.Length-offset||bytes[offset+size-1]!=0x3b)continue;
                width=BinaryPrimitives.ReadUInt16LittleEndian(data[6..]);height=BinaryPrimitives.ReadUInt16LittleEndian(data[8..]);
            }
            if(width is <1 or >4096 ||height is 0 or <-4096 or >4096)continue;
            string extension=bmp?(compression==1?"png":"bmp"):"gif";
            string file=$"resource-{offset:X6}.{extension}";byte[] asset=data[..size].ToArray();
            if(bmp&&compression==1)asset=LocalRasterDecoder.Png(LocalRasterDecoder.Rle8(asset,width,Math.Abs(height)));
            File.WriteAllBytes(Path.Combine(destination,file),asset);
            records.Add(new(offset,width,Math.Abs(height),bmp?"BMP":"GIF",file));
        }
        File.WriteAllText(Path.Combine(destination,"index.json"),JsonSerializer.Serialize(records));return records;
    }
}
