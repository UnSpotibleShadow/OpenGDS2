using OpenGDS2.Domain;
using OpenGDS2.Import;

namespace OpenGDS2.Core;

public sealed class GameSession
{
    private readonly Random _random;
    public GameSession(Random? random = null) => _random = random ?? new Random();
    public GameState? Current { get; private set; }
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
    public RecruitmentState OpenRecruitment()
    {
        var state = Current ?? throw new InvalidOperationException("No current session.");
        state.Recruitment.Candidates.Clear();
        if (state.MinimumSourceIndex is int minimum && state.SourceUpperBound is int upper)
            state.Recruitment.Candidates.AddRange(RecruitmentGenerator.BuildRoster(state.EmployeeSources, minimum, upper, _random));
        return state.Recruitment;
    }
    public HireResult Hire(EmployeeState candidate)
        => HiringService.TryHire(Current ?? throw new InvalidOperationException("No current session."), candidate, _random);

    public void TickOffice(OriginalCatalog? catalog = null)
    {
        var state = Current ?? throw new InvalidOperationException("No current session.");
        if (state.DevelopmentProjects.Any(x=>x.PendingDecision.HasValue) || state.ReleasedGames.Any(x=>x.ReviewPending) || SalesMilestoneService.HasPending(state) || RecallService.HasPending(state)) return;
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
            if (catalog is not null)
            {
                TrainingService.Tick(state,employee,catalog,_random);
                DevelopmentSimulation.TickEmployee(state,employee,catalog,_random);
            }
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
