using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
namespace OpenGDS2.Import;
public sealed record OriginalHelpParagraph(int Topic,int Top,string Text);
public sealed class OriginalHelpCatalog
{
    public string SourceHash {get;set;}=string.Empty;
    public List<OriginalHelpParagraph> Paragraphs {get;set;}=[];
}
/// <summary>Serialized fGMHelp Picture1 containers and Label3/4 captions, locally extracted.</summary>
public static class OriginalHelpImporter
{
    public static OriginalHelpCatalog Extract(byte[] bytes)
    {
        string hash=Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        if(hash!="4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6")throw new InvalidDataException("Unsupported help source.");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);var encoding=Encoding.GetEncoding(932);
        var result=new OriginalHelpCatalog {SourceHash=hash};int parent=0;int[] parents=[2,3,1,4,5];
        for(int offset=0x33E530;offset<0x3618C8-32;offset++)
        {
            if(bytes.AsSpan(offset,16).SequenceEqual(new byte[]{80,105,99,116,117,114,101,49,0,0,1,5,0,0,128,2}))
                parent=BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset+16));
            bool label=bytes.AsSpan(offset,9).SequenceEqual("Label4\0\x01\x01"u8)||bytes.AsSpan(offset,9).SequenceEqual("Label3\0\x01\x01"u8);
            if(!label)continue;
            int length=BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset+9));int end=offset+11+length;
            if(length<1||end+12>=0x3618C8||bytes[end]!=0||bytes[end+1]!=2)throw new InvalidDataException("Invalid help caption binding.");
            int topic=Array.IndexOf(parents,parent);if(topic<0)throw new InvalidDataException("Unknown help container.");
            int layout=end+4;
            // ForeColor (property 4) is present only on colored headings.
            if(bytes[layout]==4)layout+=5;
            if(layout+9>=0x3618C8||bytes[layout]!=5)throw new InvalidDataException("Invalid help layout binding.");
            int top=BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(layout+3));
            result.Paragraphs.Add(new(topic,top,encoding.GetString(bytes,offset+11,length)));
        }
        if(result.Paragraphs.Count!=91)throw new InvalidDataException("Incomplete original help captions.");
        result.Paragraphs=result.Paragraphs.OrderBy(p=>p.Topic).ThenBy(p=>p.Top).ToList();return result;
    }
}
