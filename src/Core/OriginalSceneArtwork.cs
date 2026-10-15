using Godot;
using OpenGDS2.Import;
namespace OpenGDS2.Core;

/// <summary>Semantic bindings to the locally imported MDI ImageFormG array.</summary>
public static class OriginalSceneArtwork
{
    private static readonly string[] PromotionFiles=["0F0704.bmp","18151E.bmp","0BEA30.bmp","10E76E.bmp","0969C6.bmp","0C869A.bmp"];
    public static Texture2D? Promotion(int medium)=>medium is >=0 and <6 ? Load("Promotion"+medium+".bmp",PromotionFiles[medium]):null;
    public static Texture2D? Publisher()=>Load("PublisherMeeting.bmp","1634B4.bmp");
    public static Texture2D? BusinessMeeting()
    {
        if(OriginalAssetStore.LoadTexture("Graphic/Embedded/BusinessMeeting.png") is { } scene)return scene;
        string path=OriginalAssetStore.PathFor("Graphic/Recovered/resource-08FCD0.gif");
        if(!File.Exists(path))return null;
        using var image=new Image();
        return image.LoadPngFromBuffer(LocalRasterDecoder.Png(LocalRasterDecoder.Gif(File.ReadAllBytes(path),256,160)))==Error.Ok?ImageTexture.CreateFromImage(image):null;
    }
    private static Texture2D? Load(string alias,string resource)
        =>OriginalAssetStore.LoadTexture("Graphic/Embedded/"+alias) ?? OriginalAssetStore.LoadTexture("Graphic/Recovered/resource-"+resource);
}
