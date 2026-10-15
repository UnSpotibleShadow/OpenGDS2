using Godot;
using System.Buffers.Binary;
using OpenGDS2.Import;
using OpenGDS2.Domain;
namespace OpenGDS2.Core;

/// <summary>Semantic bindings to the locally imported MDI ImageFormG array.</summary>
public static class OriginalSceneArtwork
{
    private static readonly string[] PromotionFiles=["0F0704.bmp","18151E.bmp","0BEA30.bmp","10E76E.bmp","0969C6.bmp","0C869A.bmp"];
    public static Texture2D? Promotion(int medium)=>medium is >=0 and <6 ? Load("Promotion"+medium+".bmp",PromotionFiles[medium]):null;
    public static Texture2D? Publisher()=>Load("PublisherMeeting.bmp","1634B4.bmp");
    public static Texture2D? BusinessMeeting()=>LoadGif("BusinessMeeting.png","08FCD0.gif");
    public static Texture2D? Facility(FacilityKind kind)
    {
        string[] offsets=["08CB31","088E14","04597A","0345EF","040AF6","042598","0653AF","03AF24","0376B5"];
        if(!Enum.IsDefined(kind))return null;
        return kind==FacilityKind.MysteryLab?Load("Facility-MysteryLab.bmp",offsets[(int)kind]+".bmp"):
            LoadGif("Facility-"+kind+".png",offsets[(int)kind]+".gif");
    }
    private static Texture2D? LoadGif(string alias,string resource)
    {
        if(OriginalAssetStore.LoadTexture("Graphic/Embedded/"+alias) is { } scene)return scene;
        string path=OriginalAssetStore.PathFor("Graphic/Recovered/resource-"+resource);
        if(!File.Exists(path))return null;
        using var image=new Image();
        return image.LoadPngFromBuffer(LocalRasterDecoder.Png(LocalRasterDecoder.Gif(File.ReadAllBytes(path),256,160)))==Error.Ok?ImageTexture.CreateFromImage(image):null;
    }
    private static Texture2D? Load(string alias,string resource)
    {
        foreach(string relative in new[]{"Graphic/Embedded/"+alias,"Graphic/Recovered/resource-"+resource})
        {
            if(OriginalAssetStore.LoadTexture(relative) is not { } texture)continue;
            string path=OriginalAssetStore.PathFor(relative);
            string original=Path.Combine(ProjectSettings.GlobalizePath(OriginalGameImporter.CacheRoot),relative.Replace('/',Path.DirectorySeparatorChar));
            // Win32 BI_RGB32 uses reserved bytes, not meaningful alpha. Godot
            // otherwise makes parts of the original Cinema screen transparent.
            // Respect explicit PNG/BMP overrides and generated English patches.
            if(path==original && Path.GetExtension(path).Equals(".bmp",StringComparison.OrdinalIgnoreCase))
            {
                using var file=File.OpenRead(path);byte[] header=new byte[54];
                if(file.Read(header)==54 && header[0]=='B' && header[1]=='M' &&
                    BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(28))==32 &&
                    BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(30))==0)
                {
                    using var image=texture.GetImage();image.Convert(Image.Format.Rgb8);
                    return ImageTexture.CreateFromImage(image);
                }
            }
            return texture;
        }
        return null;
    }
}
