namespace OpenGDS2.Domain;

/// <summary>
/// Employee fields recovered from the original PC GameDev2DX hiring/renewal forms.
/// These are intentionally not the later mobile Game Dev Story stat model.
/// Unknown values stay nullable until the candidate/employee database is decoded.
/// </summary>
public sealed class EmployeeState
{
    public OfficeEmployeeState? Office { get; set; }
    public int? SourceIndex { get; init; }
    public int? PortraitId { get; init; }
    // Literal order: inspiration, speed, accuracy, music, graphics, novelty, fun.
    public List<int> OriginalAttributes { get; init; } = [];
    public int? RawParameter34 { get; init; }
    [System.Text.Json.Serialization.JsonIgnore]
    public int? MaximumTechnologies => RawParameter34;
    // Null means unknown in older modern saves; keys are source indices, not identifiers.
    public Dictionary<int,int>? TechnologyLevels { get; set; }
    public int? RawParameter3C { get; init; }
    public long? RawParameter40 { get; init; }
    public int? SpecialFlag { get; init; }
    public required string Name { get; init; }
    public int? Level { get; set; }
    public double? Experience { get; set; }
    public double? Stress { get; set; }
    public int? Fun { get; set; }              // おもしろさ
    public int? Novelty { get; set; }          // 意外性
    public int? Accuracy { get; set; }         // 正確性
    public int? Graphics { get; set; }         // グラフィック
    public int? Music { get; set; }            // 音楽性
    public int? DevelopmentSpeed { get; set; } // 開発速度
    public int? Inspiration { get; set; }       // ひらめき度
    public long? ContractFee { get; set; }     // 契約金
}
