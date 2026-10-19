using Godot;
using OpenGDS2.Core;

namespace OpenGDS2.UI;

public partial class Main
{
    [System.Runtime.InteropServices.DllImport("shell32.dll",CharSet=System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern int SetCurrentProcessExplicitAppUserModelID(string appId);
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
        if(OS.GetName()=="Windows")
        {
            SetCurrentProcessExplicitAppUserModelID("OpenGDS2.Game");
            // WM_SETICON's native ICO path updates both small/title and large/taskbar
            // icons. Original pixels stay in the local user cache, never the repo.
            string path=ProjectSettings.GlobalizePath("user://application-icon.ico");
            try
            {
                byte[] png=icon.SavePngToBuffer();
                using(var stream=System.IO.File.Create(path))
                using(var writer=new System.IO.BinaryWriter(stream))
                {
                    writer.Write((ushort)0);writer.Write((ushort)1);writer.Write((ushort)1);
                    writer.Write((byte)(icon.GetWidth()>=256?0:icon.GetWidth()));writer.Write((byte)(icon.GetHeight()>=256?0:icon.GetHeight()));
                    writer.Write((byte)0);writer.Write((byte)0);writer.Write((ushort)1);writer.Write((ushort)32);
                    writer.Write(png.Length);writer.Write(22);writer.Write(png);
                }
                DisplayServer.SetNativeIcon(path);
            }
            catch(System.IO.IOException){GD.PushWarning("Could not cache the native application icon.");}
            catch(UnauthorizedAccessException){GD.PushWarning("Could not cache the native application icon.");}
        }
    }
}
