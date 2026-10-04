using OpenGDS2.Domain;
using OpenGDS2.Import;

namespace OpenGDS2.Core;

// Display-only community readings. Original names remain in simulation/saves.
public static class EmployeeNameLocalization
{
    public static string Display(EmployeeState employee, EmployeeDatabase? database,
        bool english, Func<string, string> translate)
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
            return Read($"EMPLOYEE_NAME_{source}", translate) ?? employee.Name;
        }
        // Generated records independently choose a family and a given name.
        // Match the saved combination, never its randomly populated source slot.
        for (int family = 0; family < database.NameComponents.Count; family++)
        for (int given = 0; given < database.NameComponents.Count; given++)
        {
            if (Normalize(database.NameComponents[family].Family + " " + database.NameComponents[given].Given) != normalized)
                continue;
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
}
