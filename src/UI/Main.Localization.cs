using Godot;
using OpenGDS2.Domain;
using OpenGDS2.Import;

namespace OpenGDS2.UI;

public partial class Main
{
    private const string InterfaceSettingsPath = "user://interface.cfg";

    private void LoadInterfaceLanguage()
    {
        var settings = new ConfigFile();
        string locale = settings.Load(InterfaceSettingsPath) == Error.Ok
            ? settings.GetValue("language", "locale", "en").AsString() : "en";
        TranslationServer.SetLocale(locale is "en" or "ja" ? locale : "en");
    }

    private void SaveInterfaceLanguage(string locale)
    {
        TranslationServer.SetLocale(locale);
        var settings = new ConfigFile(); settings.SetValue("language", "locale", locale);
        var result = settings.Save(InterfaceSettingsPath);
        if (result != Error.Ok) GD.PushWarning($"Language preference could not be saved: {result}");
    }

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
