using OpenGDS2.Domain;
using OpenGDS2.Import;

namespace OpenGDS2.Core;

// Display-only community readings. Original names remain in simulation/saves.
public static class EmployeeNameLocalization
{
    public static string Display(EmployeeState employee, EmployeeDatabase? database,
        bool english, Func<string, string> translate,IReadOnlyDictionary<int,EmployeeNameReadingOverride>? overrides=null)
    {
        if (!english || database?.SourceHash != EmployeeDatabaseImporter.ExpectedHash ||
            employee.SourceIndex is not int source) return employee.Name;
        var definition = database.Employees.SingleOrDefault(x => x.SourceIndex == source);
        if (definition is null) return employee.Name;
        string normalized = Normalize(employee.Name);
        if (!definition.GeneratedName)
        {
            // A renamed/custom employee must never inherit a source-index alias.
            if (definition.Name is null || Normalize(definition.Name) != normalized) return employee.Name;
            if (overrides?.TryGetValue(source,out var reading)==true && Normalize(reading.OriginalName)==normalized)
                return reading.EnglishName;
            return Read($"EMPLOYEE_NAME_{source}", translate) ?? employee.Name;
        }
        // Generated records independently choose a family and a given name.
        // Match the saved combination, never its randomly populated source slot.
        for (int family = 0; family < database.NameComponents.Count; family++)
        for (int given = 0; given < database.NameComponents.Count; given++)
        {
            if (Normalize(database.NameComponents[family].Family + " " + database.NameComponents[given].Given) != normalized)
                continue;
            if (overrides?.TryGetValue(source,out var reading)==true && Normalize(reading.OriginalName)==normalized)
                return reading.EnglishName;
            var f = Read($"EMPLOYEE_FAMILY_{family}", translate);
            var g = Read($"EMPLOYEE_GIVEN_{given}", translate);
            return f is null || g is null ? employee.Name : f + " " + g;
        }
        return employee.Name;
    }

    private static string? Read(string key, Func<string, string> translate)
    {
        string text = translate(key);
        return string.IsNullOrWhiteSpace(text) || text == key ? null : text;
    }
    private static string Normalize(string text)
        => string.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    // A kana component is phonetic evidence; kanji-only personal names do
    // not prove a unique reading. This audit does not assert official names.
    public static bool HasKanaReadingEvidence(int sourceIndex)=>sourceIndex is 16 or 56 or 57;
    public static Dictionary<int,EmployeeNameReadingOverride> ReadOverrides(string json)
    {
        var file=System.Text.Json.JsonSerializer.Deserialize<EmployeeNameReadingFile>(json);
        if (file?.SchemaVersion!=1 || file.SourceHash!=EmployeeDatabaseImporter.ExpectedHash || file.Readings is null ||
            file.Readings.Count>60 || file.Readings.Any(x=>x is null) || file.Readings.Select(x=>x.SourceIndex).Distinct().Count()!=file.Readings.Count ||
            file.Readings.Any(x=>x.SourceIndex is <0 or >59 || string.IsNullOrWhiteSpace(x.OriginalName) || x.OriginalName.Length>128 ||
                string.IsNullOrWhiteSpace(x.EnglishName) || x.EnglishName.Length>128 || x.EnglishName.Any(char.IsControl)))
            throw new InvalidDataException("Invalid local employee-name readings.");
        return file.Readings.ToDictionary(x=>x.SourceIndex);
    }
}

public sealed class EmployeeNameReadingFile
{
    public int SchemaVersion {get;set;}=1;
    public string SourceHash {get;set;}=string.Empty;
    public List<EmployeeNameReadingOverride> Readings {get;set;}=[];
}
public sealed class EmployeeNameReadingOverride
{
    public int SourceIndex {get;set;}
    public string OriginalName {get;set;}=string.Empty;
    public string EnglishName {get;set;}=string.Empty;
}
