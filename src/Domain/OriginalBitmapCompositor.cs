namespace OpenGDS2.Domain;

/// <summary>The original two BitBlt raster operations, operating on RGB24 bytes.</summary>
public static class OriginalBitmapCompositor
{
    public static byte CompositeChannel(byte destination, byte mask, byte color)
        => (byte)((mask & ~destination) ^ color);

    public static void Blit(byte[] destination, int width, int height, byte[] colors, byte[] masks,
        int atlasWidth, int atlasHeight, OfficeSpriteDefinition sprite)
    {
        if (width <= 0 || height <= 0 || atlasWidth <= 0 || atlasHeight <= 0 ||
            destination.Length != checked(width * height * 3) ||
            colors.Length != checked(atlasWidth * atlasHeight * 3) || masks.Length != colors.Length ||
            sprite.Width < 0 || sprite.Height < 0 || sprite.SourceX < 0 || sprite.SourceY < 0 ||
            sprite.SourceX > atlasWidth - sprite.Width || sprite.SourceY > atlasHeight - sprite.Height)
            throw new ArgumentException("Invalid original bitmap rectangle.");
        int left = Math.Max(0, sprite.X), top = Math.Max(0, sprite.Y);
        int right = (int)Math.Min(width, (long)sprite.X + sprite.Width);
        int bottom = (int)Math.Min(height, (long)sprite.Y + sprite.Height);
        for (int y = top; y < bottom; y++)
        for (int x = left; x < right; x++)
        {
            int target = (y * width + x) * 3;
            int source = ((sprite.SourceY + y - sprite.Y) * atlasWidth + sprite.SourceX + x - sprite.X) * 3;
            for (int channel = 0; channel < 3; channel++)
                destination[target + channel] = CompositeChannel(destination[target + channel], masks[source + channel], colors[source + channel]);
        }
    }
}
