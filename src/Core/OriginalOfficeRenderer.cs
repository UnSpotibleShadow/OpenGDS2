using Godot;
using System;
using System.Linq;
using OpenGDS2.Domain;

namespace OpenGDS2.Core;

/// <summary>Loaded local graphics; recomposition does not reread imported files.</summary>
public sealed class OriginalOfficeRenderer(OfficeSceneData scene, byte[] background,
    int width, int height, byte[] colors, byte[] masks, int atlasWidth, int atlasHeight)
{
    public ImageTexture CreateTexture(GameState state)
    {
        using var image = Compose(state);
        return ImageTexture.CreateFromImage(image);
    }

    public void UpdateTexture(ImageTexture texture, GameState state)
    {
        using var image = Compose(state);
        texture.Update(image);
    }

    private Image Compose(GameState state)
    {
        var pixels = (byte[])background.Clone();
        var sprites = state.Year is int year && state.Month is int month && state.Week is int week
            ? OfficePresentationRules.ForDate(scene, new(year, month, week))
            : scene.Sprites.Where(x => x.Id != 144 && x.Id != 145 && (long)x.X + x.Width > 0 && x.Y > 0);
        sprites = sprites.Concat(state.Employees.Where(x => x.Office is not null).Select(OfficeStaffRules.Sprite));
        foreach (var sprite in sprites.OrderBy(x => x.SortKey))
            OriginalBitmapCompositor.Blit(pixels, width, height, colors, masks, atlasWidth, atlasHeight, sprite);
        return Image.CreateFromData(width, height, false, Image.Format.Rgb8, pixels);
    }
}
