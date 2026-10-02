namespace OpenGDS2.Domain;

/// <summary>Engine-owned state. Keep this independent from Godot UI nodes so rules can be regression-tested.</summary>
public sealed class GameState
{
    // Unknown original values stay nullable. Do not turn missing reverse-engineering evidence into zeroes.
    public int? Year { get; set; }
    public int? Week { get; set; }
    public List<RecruitmentSourceRecord> EmployeeSources { get; set; } = [];
    public int? MinimumSourceIndex { get; set; }
    public int? SourceUpperBound { get; set; }
    public int? Month { get; set; }
    public long? Money { get; set; }
    public int? DevelopmentComputerIndex { get; set; }
    public HashSet<int>? HardwareLicenses { get; set; }
    public HashSet<int>? AvailableHardware { get; set; }

    public List<DevelopmentProjectState> DevelopmentProjects { get; set; } = [];
    // Sparse original sourceIndex+99 cells; old modern saves keep these unknown.
    public Dictionary<int,int>? OriginalAccuracyCells { get; set; }

    public GameMode Mode { get; set; } = GameMode.Normal;
    public GameModeRules Rules { get; set; } = GameModeRules.For(GameMode.Normal);
    public InitialConditions Initial { get; set; } = new();
    public CompanyState Company { get; set; } = new();
    public List<EmployeeState> Employees { get; set; } = [];
    public RecruitmentState Recruitment { get; set; } = new();
}
