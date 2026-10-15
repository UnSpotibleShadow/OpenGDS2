using System;
using System.IO;
using System.Linq;

namespace OpenGDS2.Core;

/// <summary>Local artwork replacements require the matching imported original file.</summary>
public static class LocalAssetOverlay
{
    public static string Resolve(string originalRoot, string overlayRoot, string relative)
    {
        string normalized = relative.Replace('\\', '/');
        if (Path.IsPathRooted(normalized) || normalized.Contains(':') || normalized.Contains('\0') ||
            normalized.Split('/').Any(x => x is "" or "." or ".."))
            throw new ArgumentException("Asset paths must remain inside the local cache.", nameof(relative));
        string original = Path.Combine(originalRoot, normalized.Replace('/', Path.DirectorySeparatorChar));
        // Overrides never establish ownership or replace extracted rules/metadata.
        string extension = Path.GetExtension(normalized);
        if (!File.Exists(original) || !normalized.StartsWith("Graphic/", StringComparison.Ordinal) ||
            !(extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase) || extension.Equals(".png", StringComparison.OrdinalIgnoreCase))) return original;
        string overlay = Path.Combine(overlayRoot, normalized.Replace('/', Path.DirectorySeparatorChar));
        string png = Path.ChangeExtension(overlay, ".png");
        return File.Exists(png) ? png : File.Exists(overlay) ? overlay : original;
    }
}
