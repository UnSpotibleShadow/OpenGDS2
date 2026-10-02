using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenGDS2.Domain;

// Names and records are loaded from the user's own distribution, never bundled.
public sealed class EmployeeDatabase
{
    public int SchemaVersion { get; set; } = 1;
    public string SourceHash { get; set; } = string.Empty;
    public int MinimumSourceIndex { get; set; }
    public int SourceUpperBound { get; set; }
    public List<NameComponents> NameComponents { get; set; } = [];
    public List<EmployeeDefinition> Employees { get; set; } = [];
    public Dictionary<string, long?> OpeningScalars { get; set; } = [];

    public List<RecruitmentSourceRecord> CreateSources(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        if (Employees.Any(x => x.RawAttributes.Count != 7) ||
            (Employees.Any(x => x.GeneratedName) && NameComponents.Count == 0))
            throw new InvalidOperationException("Incomplete employee database.");
        var names = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<RecruitmentSourceRecord>();
        foreach (var row in Employees.OrderBy(x => x.SourceIndex))
        {
            string name = row.Name ?? string.Empty;
            if (row.GeneratedName)
            {
                // The original samples family and given names independently and rejects
                // any full name already in the initialized source database.
                var available = (from family in NameComponents
                                 from given in NameComponents
                                 select family.Family + " " + given.Given).Distinct().Count(x => !names.Contains(x));
                if (available == 0) throw new InvalidOperationException("Name pool exhausted.");
                int attempts = 0;
                do
                {
                    if (++attempts > 100_000) throw new InvalidOperationException("Random name generation stalled.");
                    name = NameComponents[random.Next(NameComponents.Count)].Family + " " +
                           NameComponents[random.Next(NameComponents.Count)].Given;
                } while (names.Contains(name));
            }
            names.Add(name);
            result.Add(new RecruitmentSourceRecord
            {
                SourceIndex = row.SourceIndex, GateValue = row.InitialGate,
                Employee = new EmployeeState
                {
                    SourceIndex = row.SourceIndex, Name = name, Level = row.Level,
                    Experience = 0, Stress = 0, // Module1 reset: members C30 and C68.
                    ContractFee = row.ContractFee, PortraitId = row.PortraitId,
                    OriginalAttributes = [.. row.RawAttributes],
                    Fun = row.RawAttributes[6], Novelty = row.RawAttributes[5],
                    Accuracy = row.RawAttributes[2], Graphics = row.RawAttributes[4],
                    Music = row.RawAttributes[3], DevelopmentSpeed = row.RawAttributes[1],
                    Inspiration = row.RawAttributes[0],
                    RawParameter34 = row.RawParameter34, RawParameter3C = row.RawParameter3C,
                    RawParameter40 = row.RawParameter40, SpecialFlag = row.InitialSpecialFlag
                }
            });
        }
        return result;
    }
}

public sealed class NameComponents
{
    public string Given { get; set; } = string.Empty;
    public string Family { get; set; } = string.Empty;
}

public sealed class EmployeeDefinition
{
    public int SourceIndex { get; set; }
    public string? Name { get; set; }
    public bool GeneratedName { get; set; }
    public int PortraitId { get; set; }
    public List<int> RawAttributes { get; set; } = [];
    public int Level { get; set; }
    public long ContractFee { get; set; }
    public int RawParameter3C { get; set; }
    public int RawParameter34 { get; set; }
    public long RawParameter40 { get; set; }
    public int InitialGate { get; set; }
    public int InitialSpecialFlag { get; set; }
    public string DefinitionAddress { get; set; } = string.Empty;
}
