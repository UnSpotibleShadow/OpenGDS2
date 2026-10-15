namespace OpenGDS2.Domain;

/// <summary>Engine-owned state. Keep this independent from Godot UI nodes so rules can be regression-tested.</summary>
public sealed class GameState
{
    // Native members0140/01DC participate in save/load. Null preserves legacy uncertainty.
    public NumberGrouping? NumberDisplayGrouping {get;set;}
    public bool? OfficeGraphicsEnlarged {get;set;}
    // Native member 02CC resets to Empty/zero and participates in GD2 save/load.
    // Earlier modern versions had no purchase path, so their absent flag is false.
    public bool BackupDeviceOwned {get;set;}
    public long PoachingOpportunityTicks {get;set;}
    public bool PoachingEmptyRosterObserved {get;set;}
    public bool PoachingUnknownTargetObserved {get;set;}
    public PoachingState? PendingPoaching {get;set;}
    public int BackupDeviceAnimationTicks {get;set;}
    public bool BackupDeviceAlternateFrame {get;set;}
    public BackupDevicePurchaseState? PendingBackupDevice {get;set;}
    public VirusEventState? PendingVirusEvent {get;set;}
    public CompanyEventContinuation? CompanyEventContinuation {get;set;}
    public LevelUpPresentationState? PendingLevelUpResult {get;set;}

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
    public CasinoState Casino {get;set;}=new();
    public FinancialFailureState? FinancialFailure {get;set;}
    public Dictionary<int,int>? TechnologyOwnershipLevels { get; set; }
    public int? DevelopmentComputerIndex { get; set; }
    public HashSet<int>? HardwareLicenses { get; set; }
    public HashSet<int>? AvailableHardware { get; set; }
    // Null in pre-calendar saves: historical countdowns cannot be reconstructed
    // from lifetime releases or a frozen displayed date.
    public CompanyCalendarState? Calendar { get; set; }
    public CompetitionState? Competition {get;set;}
    public List<RankingAchievement> PendingRankings {get;set;}=[];

    public List<DevelopmentProjectState> DevelopmentProjects { get; set; } = [];
    public List<ReleasedGameState> ReleasedGames { get; set; } = [];
    // A143FC is shared across Hall windows; legacy phase remains unknown.
    public int? HallCelebrationTicks {get;set;}
    public CompanyActivityPresentation? PendingCompanyActivity {get;set;}
    public uint? SimulationRandomState {get;set;}
    public int? ProgrammingActorPasses {get;set;}
    public ProgrammingChallengeState? PendingProgrammingChallenge {get;set;}
    public OfficeActorContinuationState? OfficeActorContinuation {get;set;}
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
    // Continuous Module2 frame local -04A4, assigned only by non-speed work
    // before debugging. Modern saves retain it; older saves leave it unknown.
    public int? RetainedDevelopmentEligibility {get;set;}

    public GameMode Mode { get; set; } = GameMode.Normal;
    public GameModeRules Rules { get; set; } = GameModeRules.For(GameMode.Normal);
    public InitialConditions Initial { get; set; } = new();
    public CompanyState Company { get; set; } = new();
    // Modern presentation snapshot; older saves start a fresh flame phase.
    public OfficeBurstFlameState OfficeBurstFlames {get;set;}=new();
    public OfficeRoomPresentationState OfficeRoomPresentation {get;set;}=new();
    public List<EmployeeState> Employees { get; set; } = [];
    public RecruitmentState Recruitment { get; set; } = new();
    public HireGreetingState? PendingHireGreeting { get; set; }
    public BankLoanReceipt? PendingBankLoanReceipt {get;set;}
    // Original member 0890. Unknown in earlier modern saves; never infer it on load.
    public int? SecretarySourceIndex { get; set; }
    public SecretaryAdviceState? PendingSecretaryAdvice { get; set; }
    public SecretaryReplacementState? PendingSecretaryReplacement {get;set;}
    public PromotionMeetingState? PendingPromotion {get;set;}
    public ProjectCancellationDialogueState? PendingProjectCancellation {get;set;}
    public FacilityReceipt? PendingFacilityReceipt {get;set;}
    public FeeNegotiationState? PendingFeeNegotiation {get;set;}
    // Before M82 modern saves had no experiment workflow; absent fields migrate
    // to idle, independently of unsupported original-save importing.
    public OwnHardwareState OwnHardware {get;set;}=new();
    public MysteryLabState MysteryLab {get;set;}=new();
    public MysteryLabResult? PendingMysteryLabResult {get;set;}
    public SecretaryPolicyState? SecretaryPolicies {get;set;}
}
