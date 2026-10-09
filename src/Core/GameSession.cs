using OpenGDS2.Domain;
using OpenGDS2.Import;

namespace OpenGDS2.Core;

public sealed class GameSession
{
    private readonly Random _random;
    public GameSession(Random? random = null) => _random = random ?? new Random();
    public GameState? Current { get; private set; }
    public ProjectCancellationResult CancelProject(DevelopmentProjectState project)
        =>ProjectCancellationService.TryCancel(Current ?? throw new InvalidOperationException("No current session."),project,_random);
    public GameState StartNewGame(string companyName, string presidentName, GameMode mode, EmployeeDatabase? database = null, OriginalCatalog? catalog = null)
    {
        if (string.IsNullOrWhiteSpace(companyName) || string.IsNullOrWhiteSpace(presidentName))
            throw new ArgumentException("Company and president names are required.");
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        var date = CalendarRules.OpeningDate;
        var state = new GameState
        {
            Mode = mode, Rules = GameModeRules.For(mode),
            Money = mode == GameMode.Beginner ? 1_200_000_000L : 450_000_000L,
            Year = date.Year, Month = date.Month, Week = date.Week,
            TechnologyOwnershipLevels = catalog?.Technologies.Count>0 ? catalog.Technologies.ToDictionary(x=>x.SourceIndex,x=>x.InitialOwnershipLevel) : null,
            DevelopmentComputerIndex = 0,
            HardwareLicenses = [0],
            SalesFee1 = 1500, SalesFee2 = 1500,
            CompanyActivityTicks=0,CompanyActivityPerformed=false,
            ProductionPerformedThisWeek=false,
            AvailableHardware = catalog?.Hardware.Where(x => x.RawMembers["0F1C"] == 1).Select(x => x.SourceIndex).ToHashSet(),
        };
        state.Initial.StartingMoney = state.Money;
        state.Initial.StartingYear = state.Year; state.Initial.StartingMonth = state.Month;
        state.Company.Name = companyName.Trim(); state.Company.PresidentName = presidentName.Trim();
        state.Company.Rank = 0;
        state.Company.Fame = 12;
        state.Company.ProductionCapacity = 50;
        state.Company.OriginalMember90 = 23;
        state.Company.HallOfFameCount = 0;
        state.Company.ActivityCost = 5_000_000;
        // Reset 009DD234 +014D..016A writes Empty, numerically zero.
        state.Company.WeeklyNet=state.Company.MonthlyNet=state.Company.YearlyNet=0;
        if (catalog?.SourceHash==EmployeeDatabaseImporter.ExpectedHash && catalog.Hardware.Count==11 &&
            catalog.Hardware.All(x=>x.RawMembers.ContainsKey("10A4") && x.RawMembers.ContainsKey("10C0")))
            state.Calendar=CalendarService.CreateOpening(catalog);
        if (catalog?.SourceHash==EmployeeDatabaseImporter.ExpectedHash && catalog.InitialRivalProducts.Count==30 && catalog.StagedRivalProducts.Count==35 && catalog.RivalCompanies.Count==10)
            state.Competition=CompetitionService.Opening(catalog);
        if (catalog?.SourceHash == EmployeeDatabaseImporter.ExpectedHash)
            {
                state.SecretarySourceIndex=1; // Completed initializer 009F651C +C0F9.
                // 08B0/08C0 initialize to 2. 08D0 is not reset; a fresh
                // VB global Variant is Empty (numeric 0), not a mode preset.
                state.SecretaryPolicies=new() {Production=Current?.SecretaryPolicies?.Production ?? ProductionPolicy.Funds300Million};
            }
        if (catalog?.SourceHash == EmployeeDatabaseImporter.ExpectedHash)
            state.OriginalTypeRosterMap=Enumerable.Range(0,65).ToDictionary(x=>x,_=>0);
        if (database is not null)
        {
            state.EmployeeSources = database.CreateSources(_random);
            state.MinimumSourceIndex = database.MinimumSourceIndex; state.SourceUpperBound = database.SourceUpperBound;
            if (database.SourceHash==EmployeeDatabaseImporter.ExpectedHash)
                state.MaximumSourceIndex=database.SourceUpperBound-1;
            state.Recruitment.CandidateDatabaseDecoded = true;
            if (database.SourceHash == EmployeeDatabaseImporter.ExpectedHash && database.SourceUpperBound == 60 &&
                state.EmployeeSources.Select(x=>x.SourceIndex).Order().SequenceEqual(Enumerable.Range(0,60)))
            {
                // Reset 009DD234 empties 0..256; initialization writes only 0..59.
                state.OriginalAccuracyCells=Enumerable.Range(99,60).ToDictionary(x=>x,_=>0);
            }
        }
        Current = state;
        return state;
    }
    public FacilityResult ExpandFacilityProduction(int slot)
        =>FacilityService.ExpandProduction(Current ?? throw new InvalidOperationException("No current session."),slot,_random);
    public RecruitmentState OpenRecruitment()
    {
        var state = Current ?? throw new InvalidOperationException("No current session.");
        if (state.PendingBankLoanReceipt is not null || state.PendingHireGreeting is not null || state.PendingSecretaryAdvice is not null || state.PendingSecretaryReplacement is not null || state.PendingProjectCancellation is not null || state.PendingFacilityReceipt is not null) throw new InvalidOperationException("A conversation is pending.");
        state.Recruitment.Candidates.Clear();
        if (state.MinimumSourceIndex is int minimum && state.SourceUpperBound is int upper)
            state.Recruitment.Candidates.AddRange(RecruitmentGenerator.BuildRoster(state.EmployeeSources, minimum, upper, _random));
        return state.Recruitment;
    }
    public HireResult Hire(EmployeeState candidate)
    {
        var state = Current ?? throw new InvalidOperationException("No current session.");
        if (state.PendingBankLoanReceipt is not null || state.PendingHireGreeting is not null || state.PendingSecretaryAdvice is not null || state.PendingSecretaryReplacement is not null || state.PendingProjectCancellation is not null || state.PendingFacilityReceipt is not null) return HireResult.ConversationPending;
        var result = HiringService.TryHire(state, candidate, _random);
        if (result == HireResult.Success && candidate.SourceIndex is int source && source is >=0 and <=59 && candidate.PortraitId is >=1 and <=10)
            state.PendingHireGreeting = new() {EmployeeSourceIndex=source,Variant=(int)Math.Floor(_random.NextSingle()*10)};
        return result;
    }

    public bool AcknowledgeHireGreeting()
    {
        if (Current?.PendingHireGreeting is null) return false;
        Current.PendingHireGreeting = null;
        return true;
    }

    public SecretaryAdviceResult AskSecretary(OriginalCatalog? catalog)
        =>Current?.PendingBankLoanReceipt is not null || Current?.PendingProjectCancellation is not null || Current?.PendingFacilityReceipt is not null ? SecretaryAdviceResult.EventPending :
            SecretaryAdviceRules.Prepare(Current ?? throw new InvalidOperationException("No current session."),catalog,_random);

    public void TickOffice(OriginalCatalog? catalog = null)
    {
        var state = Current ?? throw new InvalidOperationException("No current session.");
        if (state.PendingBankLoanReceipt is not null || state.PendingHireGreeting is not null || state.PendingSecretaryAdvice is not null || state.PendingSecretaryReplacement is not null || state.PendingProjectCancellation is not null || state.PendingFacilityReceipt is not null) return;
        if (state.DevelopmentProjects.Any(x=>x.PendingDecision.HasValue || CheckpointMeetingRules.HasPending(x)) || state.ReleasedGames.Any(x=>x.ReviewPending) || SalesMilestoneService.HasPending(state) || RecallService.HasPending(state)) return;
        if (CalendarService.HasPending(state))
        {
            if (!CalendarService.Resume(state,catalog,_random)) return;
        }
        else CompanyActivityService.Tick(state,_random,catalog);
        if (CalendarService.HasPending(state) || RecallService.HasPending(state)) return;
        if (catalog is not null) SalesService.Tick(state,catalog,_random);
        if (SalesMilestoneService.HasPending(state) || RecallService.HasPending(state)) return;
        foreach (var employee in state.Employees)
        {
            OfficeStaffRules.Tick(employee, _random);
            OfficeBurstFlameRules.Tick(state,employee);
            if (catalog is not null)
            {
                TrainingService.Tick(state,employee,catalog,_random);
                DevelopmentSimulation.TickEmployee(state,employee,catalog,_random);
            }
            SecretaryPolicyRules.TickEmployee(state,employee,_random);
            if (employee.Office is not null) OfficeVisualRules.TickBubble(employee.Office);
            // Original employees refer back to the source arrays. Keep the
            // modern employed copy and source projection consistent.
            var source = state.EmployeeSources.FirstOrDefault(x => x.SourceIndex == employee.SourceIndex);
            if (source is not null) { source.Employee.Stress = employee.Stress; source.Employee.Experience=employee.Experience; }
            if (state.DevelopmentProjects.Any(x=>x.PendingDecision.HasValue)) break;
        }
    }
    public void Restore(GameState state) => Current = state ?? throw new ArgumentNullException(nameof(state));
    public void Clear() => Current = null;
}
