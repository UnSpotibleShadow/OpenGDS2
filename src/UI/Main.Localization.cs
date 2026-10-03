using Godot;
using OpenGDS2.Domain;
using OpenGDS2.Import;

namespace OpenGDS2.UI;

public partial class Main
{
    // Translation affects presentation only. Saves and imported catalogs keep
    // their original names and stable source indices.
    private string CatalogName(OriginalCatalog? catalog, string category, int sourceIndex, string original)
    {
        if (!TranslationServer.GetLocale().StartsWith("en", StringComparison.OrdinalIgnoreCase) ||
            catalog?.SourceHash != EmployeeDatabaseImporter.ExpectedHash) return original.Trim();
        string key = category == "TECHNOLOGY" ? $"TECHNOLOGY_NAME_{sourceIndex}" : $"CATALOG_{category}_{sourceIndex}";
        string translated = Tr(key);
        return translated == key || string.IsNullOrWhiteSpace(translated) ? original.Trim() : translated;
    }
}
