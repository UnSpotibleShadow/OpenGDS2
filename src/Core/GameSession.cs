using OpenGDS2.Domain;

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
            DevelopmentComputerIndex = 0,
            HardwareLicenses = [0],
            AvailableHardware = catalog?.Hardware.Where(x => x.RawMembers["0F1C"] == 1).Select(x => x.SourceIndex).ToHashSet(),
        };
        state.Initial.StartingMoney = state.Money;
        state.Initial.StartingYear = state.Year; state.Initial.StartingMonth = state.Month;
        state.Company.Name = companyName.Trim(); state.Company.PresidentName = presidentName.Trim();
        state.Company.Rank = 0;
        state.Company.Fame = 12;
        if (database is not null)
        {
            state.EmployeeSources = database.CreateSources(_random);
            state.MinimumSourceIndex = database.MinimumSourceIndex; state.SourceUpperBound = database.SourceUpperBound;
            state.Recruitment.CandidateDatabaseDecoded = true;
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
    public void Restore(GameState state) => Current = state ?? throw new ArgumentNullException(nameof(state));
    public void Clear() => Current = null;
}
