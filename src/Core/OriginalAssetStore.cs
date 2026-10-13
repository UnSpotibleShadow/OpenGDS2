using Godot;
using System.IO;
using OpenGDS2.Import;
using OpenGDS2.Domain;
using System;
using System.Linq;
using System.Text.Json;

namespace OpenGDS2.Core;

public static class OriginalAssetStore
{
    public const string OverlayRoot = "user://asset-overrides";
    private static string? _englishArtworkRoot;
    public static string PathFor(string relative)
    {
        string original=ProjectSettings.GlobalizePath(OriginalGameImporter.CacheRoot);
        string path=LocalAssetOverlay.Resolve(original,ProjectSettings.GlobalizePath(OverlayRoot),relative);
        string imported=System.IO.Path.Combine(original,relative.Replace('/',System.IO.Path.DirectorySeparatorChar));
        if(path!=imported || !TranslationServer.GetLocale().StartsWith("en",StringComparison.OrdinalIgnoreCase))return path;
        try {if(LocalSceneArtworkPatches.Ensure(relative,imported) is { } scene)return scene;}
        catch(Exception error){GD.PushWarning("Local English scene patch could not be generated: "+error.Message);}
        if(_englishArtworkRoot is null)return path;
        return LocalAssetOverlay.Resolve(original,_englishArtworkRoot,relative);
    }

    public static Texture2D? LoadTexture(string relative)
    {
        string path = PathFor(relative);
        if (!File.Exists(path)) return null;
        using var image = new Image();
        if (image.Load(path) != Error.Ok) return null;
        if (path != System.IO.Path.Combine(ProjectSettings.GlobalizePath(OriginalGameImporter.CacheRoot), relative.Replace('/', System.IO.Path.DirectorySeparatorChar)))
        {
            using var original = new Image();
            string imported = System.IO.Path.Combine(ProjectSettings.GlobalizePath(OriginalGameImporter.CacheRoot), relative.Replace('/', System.IO.Path.DirectorySeparatorChar));
            if (original.Load(imported) != Error.Ok) return null;
            if (original.GetSize() != image.GetSize())
            {
                GD.PushWarning("Ignoring local artwork override with changed dimensions: " + relative);
                return ImageTexture.CreateFromImage(original);
            }
        }
        return ImageTexture.CreateFromImage(image);
    }

    public static Texture2D? LoadOpeningOffice(GameState state)
        => LoadOfficeRenderer()?.CreateTexture(state) ?? LoadTexture("Graphic/Embedded/OfficeBase.bmp");

    public static Texture2D? LoadEmployeePortrait(int? portraitId,int frame=0)
        => portraitId is >=1 and <=10 && frame is >=0 and <=2
            ? LoadTexture($"Graphic/Human/Human{portraitId}{(frame==0 ? "" : "."+frame)}.bmp") : null;

    public static OriginalOfficeRenderer? LoadOfficeRenderer()
    {
        try {_englishArtworkRoot=LocalOfficeArtworkPatches.EnsureEnglishPatch();}
        catch (Exception error) {GD.PushWarning("Local English artwork patch could not be generated: "+error.Message);_englishArtworkRoot=null;}
        string scenePath = PathFor("office.json");
        if (!File.Exists(scenePath)) return null;
        var scene = JsonSerializer.Deserialize<OfficeSceneData>(File.ReadAllText(scenePath));
        if (scene is null || scene.SourceHash != EmployeeDatabaseImporter.ExpectedHash || scene.Sprites.Count != 128)
            throw new InvalidDataException("Unsupported local office data. Reimport your original archive.");
        using var frame = new Image(); using var colors = new Image(); using var masks = new Image();
        if (frame.Load(PathFor("Graphic/Embedded/OfficeBase.bmp")) != Error.Ok ||
            colors.Load(PathFor("Graphic/Embedded/OfficeSprites.bmp")) != Error.Ok ||
            masks.Load(PathFor("Graphic/Embedded/OfficeMasks.bmp")) != Error.Ok)
            throw new InvalidDataException("Missing local office graphics. Reimport your original archive.");
        frame.Convert(Image.Format.Rgb8); colors.Convert(Image.Format.Rgb8); masks.Convert(Image.Format.Rgb8);
        if (frame.GetSize() != new Vector2I(466, 290) || colors.GetSize() != new Vector2I(1600, 530) || masks.GetSize() != colors.GetSize())
            throw new InvalidDataException("Local office artwork must retain the verified background and atlas dimensions.");
        return new(scene, frame.GetData(), frame.GetWidth(), frame.GetHeight(),
            colors.GetData(), masks.GetData(), colors.GetWidth(), colors.GetHeight());
    }
}
