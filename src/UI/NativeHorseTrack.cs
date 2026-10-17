using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
/// <summary>Recovered mask/XOR compositor. Reads saved race positions without advancing them.</summary>
public partial class NativeHorseTrack : Control
{
    public HorseRaceState Race {get;set;}=null!;
    private readonly Texture2D? _track=OriginalAssetStore.LoadTexture("Graphic/Beibazyo.bmp")??OriginalAssetStore.LoadTexture("Graphic/Embedded/HorseTrack.bmp");
    private byte[]? _background,_closeup,_finish,_colors,_masks;
    private ImageTexture? _rendered;
    public override void _Ready()
    {
        _background=Pixels(_track,new(421,173));_colors=Pixels(OriginalAssetStore.LoadTexture("Graphic/KeibaC.bmp"),new(233,316));_masks=Pixels(OriginalAssetStore.LoadTexture("Graphic/KeibaM.bmp"),new(233,316));
        _closeup=Pixels(OriginalAssetStore.LoadTexture("Graphic/Beibazyo2.bmp"),new(421,217));
        _finish=Pixels(OriginalAssetStore.LoadTexture("Graphic/Beibazyo3.bmp"),new(421,173));
    }
    private static byte[]? Pixels(Texture2D? texture,Vector2I size)
    {
        if(texture is null)return null;using var image=texture.GetImage();if(image.GetSize()!=size)return null;image.Convert(Image.Format.Rgb8);return image.GetData();
    }
    public override void _Draw()
    {
        if(_background is not null&&_closeup is not null&&_finish is not null&&_colors is not null&&_masks is not null&&Race?.Positions.Count==8)
        {
            using var image=Image.CreateFromData(421,173,false,Image.Format.Rgb8,HorseRasterRules.Compose(_background,_colors,_masks,Race,_closeup,_finish));
            if(_rendered is null)_rendered=ImageTexture.CreateFromImage(image);else _rendered.Update(image);
            DrawTexture(_rendered,Vector2.Zero);
        }
        else
        {
            if(_track is not null)DrawTexture(_track,Vector2.Zero);else DrawRect(new(Vector2.Zero,Size),new(.1f,.55f,.1f));
            DrawString(GetThemeDefaultFont(),new(5,166),Tr("EVENT_ARTWORK_REIMPORT"),HorizontalAlignment.Left,411,10,Colors.White);
        }
    }
}
