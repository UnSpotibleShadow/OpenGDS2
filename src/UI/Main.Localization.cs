using Godot;
using OpenGDS2.Domain;
using OpenGDS2.Import;
using OpenGDS2.Core;

namespace OpenGDS2.UI;

public partial class Main
{
    private const string InterfaceSettingsPath = "user://interface.cfg";
    private EmployeeDatabase? _activeEmployeeDatabase;
    private IReadOnlyDictionary<int,EmployeeNameReadingOverride>? _employeeNameReadings;

    private string EmployeeName(EmployeeState employee) => EmployeeNameLocalization.Display(employee,
        _activeEmployeeDatabase, TranslationServer.GetLocale().StartsWith("en", StringComparison.OrdinalIgnoreCase),
        key => Tr(key),_employeeNameReadings);

    private void LoadInterfaceLanguage()
    {
        var settings = new ConfigFile();
        string locale = settings.Load(InterfaceSettingsPath) == Error.Ok
            ? settings.GetValue("language", "locale", "en").AsString() : "en";
        TranslationServer.SetLocale(locale is "en" or "ja" ? locale : "en");
        _officeGraphicScale=settings.GetValue("display","office_scale",1).AsInt32()==2?2:1;
        DisplayNumbers.Grouping=settings.GetValue("display","number_grouping",1).AsInt32()==0?NumberGrouping.Japanese:NumberGrouping.Thousands;
        string readings=ProjectSettings.GlobalizePath("user://employee-name-readings.json");
        if (File.Exists(readings))
        {
            try {_employeeNameReadings=EmployeeNameLocalization.ReadOverrides(File.ReadAllText(readings));}
            catch (Exception error) {GD.PushWarning("Local employee-name readings could not be loaded: "+error.Message);}
        }
    }

    private void SaveInterfaceLanguage(string locale)
    {
        TranslationServer.SetLocale(locale);
        var settings = new ConfigFile();settings.Load(InterfaceSettingsPath);settings.SetValue("language", "locale", locale);
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
