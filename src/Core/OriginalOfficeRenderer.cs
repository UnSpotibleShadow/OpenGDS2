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
        var sprites = OfficePresentationRules.ForState(scene,state);
        sprites = sprites.Concat(state.Employees.Where(x => x.Office is not null).Select(OfficeStaffRules.Sprite));
        sprites = sprites.Concat(state.Employees.SelectMany(OfficeVisualRules.Sprites));
        foreach (var sprite in sprites.OrderBy(x => x.SortKey))
            OriginalBitmapCompositor.Blit(pixels, width, height, colors, masks, atlasWidth, atlasHeight, sprite);
        return Image.CreateFromData(width, height, false, Image.Format.Rgb8, pixels);
    }
}
