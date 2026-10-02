namespace OpenGDS2.Domain;

/// <summary>
/// Mode behavior that is documented for the original PC release.
/// Exact starting-money amounts are deliberately left unresolved until the P-Code path is decoded.
/// </summary>
public sealed record GameModeRules(
    bool TutorialEnabled,
    int? TimeLimitYears,
    bool HasExtraStartingFunds)
{
    public static GameModeRules For(GameMode mode) => mode switch
    {
        GameMode.Beginner => new GameModeRules(
            TutorialEnabled: true,
            TimeLimitYears: 3,
            HasExtraStartingFunds: true),
        _ => new GameModeRules(
            TutorialEnabled: false,
            TimeLimitYears: null,
            HasExtraStartingFunds: false),
    };
}
