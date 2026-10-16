using Godot;
using OpenGDS2.Core;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ApplyApplicationIdentity()
    {
        GetWindow().Title=ProjectSettings.GetSetting("application/config/name").AsString();
        // Window decorates debug builds with a suffix; use the requested title
        // for the actual native window too. The in-game version stays visible.
        if(DisplayServer.GetName()!="headless")DisplayServer.WindowSetTitle(GetWindow().Title,GetWindow().GetWindowId());
        if(!DisplayServer.HasFeature(DisplayServer.Feature.Icon))return;
        // This imported form icon is pixel-identical to the original EXE's
        // separate application icon. Original pixels remain in the local cache.
        var texture=OriginalAssetStore.LoadTexture("Graphic/Embedded/WindowIcon-fGMGameKaishi.png")
            ??GD.Load<Texture2D>("res://assets/application-icon.svg");
        if(texture is null)return;
        using var icon=texture.GetImage();DisplayServer.SetIcon(icon);
    }
}
