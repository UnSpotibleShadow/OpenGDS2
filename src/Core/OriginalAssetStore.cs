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

    public static Texture2D? LoadOpeningOffice(GameState state)
        => LoadOfficeRenderer()?.CreateTexture(state) ?? LoadTexture("Graphic/Embedded/OfficeBase.bmp");

    public static OriginalOfficeRenderer? LoadOfficeRenderer()
    {
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
        return new(scene, frame.GetData(), frame.GetWidth(), frame.GetHeight(),
            colors.GetData(), masks.GetData(), colors.GetWidth(), colors.GetHeight());
    }
}
