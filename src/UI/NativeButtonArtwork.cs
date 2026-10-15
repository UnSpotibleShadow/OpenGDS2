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
        if(!arrow&&image.GetSize()==new Vector2I(39,16)){ApplyTab(button,offset,offset);return;}
        bool small=!arrow&&image.GetSize()==new Vector2I(23,22);
        if(!small&&image.GetSize()!=(arrow?new Vector2I(16,16):new Vector2I(63,22)))return;
        using var glyph=image.GetRegion(arrow?new Rect2I(4,4,8,8):new Rect2I(3,1,small?17:14,20));glyph.Convert(Image.Format.Rgba8);
        Color background=image.GetPixel(arrow?3:20,arrow?3:2);
        for(int y=0;y<glyph.GetHeight();y++)for(int x=0;x<glyph.GetWidth();x++)
        {var pixel=glyph.GetPixel(x,y);if(pixel.R==background.R&&pixel.G==background.G&&pixel.B==background.B)glyph.SetPixel(x,y,new(pixel.R,pixel.G,pixel.B,0));}
        button.Icon=ImageTexture.CreateFromImage(glyph);button.TextureFilter=CanvasItem.TextureFilterEnum.Nearest;
        button.AddThemeFontSizeOverride("font_size",10);if(arrow)button.Text="";
    }

    public static void ApplyTab(Button button,int normalOffset,int pressedOffset)
    {
        StyleBoxTexture? Skin(int offset)
        {
            var texture=OriginalAssetStore.LoadTexture($"Graphic/Embedded/Button-{offset:X6}.bmp") ?? OriginalAssetStore.LoadTexture($"Graphic/Recovered/resource-{offset:X6}.bmp");
            if(texture is null)return null;
            using var image=texture.GetImage();if(image.GetSize()!=new Vector2I(39,16))return null;
            // Retain the source bevel; the existing localized Button caption supplies text.
            // Native raster letters must not stretch when English needs wider controls.
            image.FillRect(new Rect2I(2,2,35,12),image.GetPixel(2,2));
            var style=new StyleBoxTexture {Texture=ImageTexture.CreateFromImage(image),ContentMarginLeft=3,ContentMarginRight=3,ContentMarginTop=1,ContentMarginBottom=1};
            style.SetTextureMarginAll(2);return style;
        }
        var normal=Skin(normalOffset);var pressed=Skin(pressedOffset);
        if(normal is null||pressed is null)return;
        foreach(string state in new[]{"normal","hover","disabled"})button.AddThemeStyleboxOverride(state,normal);
        button.AddThemeStyleboxOverride("pressed",pressed);
        button.AddThemeStyleboxOverride("focus",new StyleBoxFlat {DrawCenter=false,ContentMarginLeft=3,ContentMarginRight=3,ContentMarginTop=1,ContentMarginBottom=1});
        button.TextureFilter=CanvasItem.TextureFilterEnum.Nearest;button.AddThemeFontSizeOverride("font_size",10);
    }
}
