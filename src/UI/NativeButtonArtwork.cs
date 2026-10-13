using Godot;
using OpenGDS2.Core;
namespace OpenGDS2.UI;
public static class NativeButtonArtwork
{
    public static void Apply(Button button,int offset,bool arrow=false)
    {
        var source=OriginalAssetStore.LoadTexture($"Graphic/Embedded/Button-{offset:X6}.bmp") ??
            OriginalAssetStore.LoadTexture($"Graphic/Recovered/resource-{offset:X6}.bmp");
        if(source is null)return;
        using var image=source.GetImage();
        if(image.GetSize()!=(arrow?new Vector2I(16,16):new Vector2I(63,22)))return;
        using var glyph=image.GetRegion(arrow?new Rect2I(4,4,8,8):new Rect2I(3,1,14,20));glyph.Convert(Image.Format.Rgba8);
        Color background=image.GetPixel(arrow?3:20,arrow?3:2);
        for(int y=0;y<glyph.GetHeight();y++)for(int x=0;x<glyph.GetWidth();x++)
        {var pixel=glyph.GetPixel(x,y);if(pixel.R==background.R&&pixel.G==background.G&&pixel.B==background.B)glyph.SetPixel(x,y,new(pixel.R,pixel.G,pixel.B,0));}
        button.Icon=ImageTexture.CreateFromImage(glyph);button.TextureFilter=CanvasItem.TextureFilterEnum.Nearest;
        button.AddThemeFontSizeOverride("font_size",10);if(arrow)button.Text="";
    }
}
