namespace OpenGDS2.Domain;

/// <summary>Locally imported planning records. Unresolved columns retain structural names.</summary>
public sealed class OriginalCatalog
{
    public int SchemaVersion { get; set; } = 1;
    public string SourceHash { get; set; } = string.Empty;
    public List<GenreDefinition> Genres { get; set; } = [];
    public List<AudienceDefinition> Audiences { get; set; } = [];
    public List<DevelopmentComputerDefinition> DevelopmentComputers { get; set; } = [];
    public List<HardwareDefinition> Hardware { get; set; } = [];
    public List<GameTypeDefinition> GameTypes { get; set; } = [];
}

public sealed class GameTypeDefinition
{
    public int SourceIndex { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> ExampleTitles { get; set; } = [];
    public Dictionary<string, long> RawMembers { get; set; } = [];
    public List<int> RawTable13CC { get; set; } = [];
    public int RawUnusedArgument48 { get; set; }
    public long SelectionFee => RawMembers["1378"];
    public int SelectionPoints => checked((int)RawMembers["1308"]);
    public int AudienceAffinityCode => checked((int)RawMembers["13B0"]);
}

public sealed class HardwareDefinition
{
    public int SourceIndex { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, long> RawMembers { get; set; } = [];
    public int BitWidth => checked((int)RawMembers["0F00"]);
    public long PlanningCost => RawMembers["0FFC"];
    public int PlanningPoints => checked((int)RawMembers["0FE0"]);
    public long LicenseFee => RawMembers["0FC4"];
    public int Identifier => checked((int)RawMembers["0EC8"]);
}

public sealed class DevelopmentComputerDefinition
{
    public int SourceIndex { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BitLabel { get; set; } = string.Empty;
    public int BitWidth { get; set; }
    public long PurchasePrice { get; set; }
}

public sealed class AudienceDefinition
{
    public int SourceIndex { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, long> RawMembers { get; set; } = [];
    public int Identifier => checked((int)RawMembers["14A8"]);
    public string DefinitionAddress { get; set; } = string.Empty;
}

public sealed class GenreDefinition
{
    public int SourceIndex { get; set; }
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, long> RawMembers { get; set; } = [];
    public int Identifier => checked((int)RawMembers["1240"]);
    public long SelectionFee => RawMembers["1224"];
    public int SelectionPoints => checked((int)RawMembers["1208"]);
    public int RequiredCompanyValue => checked((int)RawMembers["125C"]);
    public string DefinitionAddress { get; set; } = string.Empty;
}
