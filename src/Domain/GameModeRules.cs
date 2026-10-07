namespace OpenGDS2.Domain;

/// <summary>
/// Mode behavior verified from the supported original executable.
/// The ending is an exact calendar predicate, not an elapsed-year duration.
/// </summary>
public sealed record GameModeRules(
    bool TutorialEnabled,
    int? TimeLimitYears,
    bool HasExtraStartingFunds)
{
    public GameDate? EndingClosingDate { get; init; }
    // TimeLimitYears is retained for old modern saves. Do not use the historical
    // three-year description as a gameplay deadline; saves recompute these rules.
    public static GameModeRules For(GameMode mode) => mode switch
    {
        GameMode.Beginner => new GameModeRules(
            TutorialEnabled: true,
            TimeLimitYears: null,
            HasExtraStartingFunds: true) { EndingClosingDate = CalendarRules.BeginnerEndingClosingDate },
        _ => new GameModeRules(
            TutorialEnabled: false,
            TimeLimitYears: null,
            HasExtraStartingFunds: false),
    };
}
