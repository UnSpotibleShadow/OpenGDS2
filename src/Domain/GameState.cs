namespace OpenGDS2.Domain;

/// <summary>Engine-owned state. Keep this independent from Godot UI nodes so rules can be regression-tested.</summary>
public sealed class GameState
{
    // Unknown original values stay nullable. Do not turn missing reverse-engineering evidence into zeroes.
    public int? Year { get; set; }
    public int? Week { get; set; }
    public List<RecruitmentSourceRecord> EmployeeSources { get; set; } = [];
    public int? MinimumSourceIndex { get; set; }
    // Original member A80, initialized to A60 - 1. Graduate bounds are inclusive.
    public int? MaximumSourceIndex { get; set; }
    public int? SourceUpperBound { get; set; }
    public int? Month { get; set; }
    // Shared original scratch A14034/member 3018; not a live employed-count cache.
    public int? OriginalPayrollScratchIndex { get; set; }
    public long? Money { get; set; }
    public Dictionary<int,int>? TechnologyOwnershipLevels { get; set; }
    public int? DevelopmentComputerIndex { get; set; }
    public HashSet<int>? HardwareLicenses { get; set; }
    public HashSet<int>? AvailableHardware { get; set; }
    // Null in pre-calendar saves: historical countdowns cannot be reconstructed
    // from lifetime releases or a frozen displayed date.
    public CompanyCalendarState? Calendar { get; set; }

    public List<DevelopmentProjectState> DevelopmentProjects { get; set; } = [];
    public List<ReleasedGameState> ReleasedGames { get; set; } = [];
    public int SalesTicks { get; set; }
    public int? CompanyActivityTicks { get; set; }
    public bool? CompanyActivityPerformed { get; set; }
    // Original global A110EC/member B0: shared by all game production.
    public bool? ProductionPerformedThisWeek { get; set; }
    public long? SalesFee1 { get; set; }
    public long? SalesFee2 { get; set; }
    // Global member 31A8: shared across type-selection forms, not per project.
    public Dictionary<int,int>? OriginalTypeRosterMap { get; set; }
    // Sparse original sourceIndex+99 cells; old modern saves keep these unknown.
    public Dictionary<int,int>? OriginalAccuracyCells { get; set; }

    public GameMode Mode { get; set; } = GameMode.Normal;
    public GameModeRules Rules { get; set; } = GameModeRules.For(GameMode.Normal);
    public InitialConditions Initial { get; set; } = new();
    public CompanyState Company { get; set; } = new();
    public List<EmployeeState> Employees { get; set; } = [];
    public RecruitmentState Recruitment { get; set; } = new();
}
