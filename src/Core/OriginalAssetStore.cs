using Godot;
using System.IO;
using OpenGDS2.Import;

namespace OpenGDS2.Core;

public static class OriginalAssetStore
{
    public static string PathFor(string relative)
        => System.IO.Path.Combine(ProjectSettings.GlobalizePath(OriginalGameImporter.CacheRoot), relative.Replace('/', System.IO.Path.DirectorySeparatorChar));

    public static Texture2D? LoadTexture(string relative)
    {
        string path = PathFor(relative);
        if (!File.Exists(path)) return null;
        var image = new Image();
        if (image.Load(path) != Error.Ok) return null;
        return ImageTexture.CreateFromImage(image);
    }
}
