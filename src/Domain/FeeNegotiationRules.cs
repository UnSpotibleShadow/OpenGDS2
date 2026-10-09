namespace OpenGDS2.Domain;

/// <summary>
/// Fee-result branches of original meeting modes 13 and 14. The live facility service owns entry and saved dialogue sequencing.
/// </summary>
public static class FeeNegotiationRules
{
    public static int OutcomeDrawRange(long currentFee) => currentFee switch
    {
        1300 => 2,
        1100 => 4,
        900 => 8,
        700 => 16,
        _ => 0
    };

    // This resolves only the outcome branch. It does not purchase a consultation,
    // mutate either company fee or consume the dialogue helper's random draws.
    public static long Resolve(long currentFee, Random outcomeRandom)
    {
        ArgumentNullException.ThrowIfNull(outcomeRandom);
        if (currentFee == 1500) return 1300;
        int range = OutcomeDrawRange(currentFee);
        if (range == 0) return currentFee;
        double draw = outcomeRandom.NextDouble();
        if (!double.IsFinite(draw) || draw < 0 || draw >= 1)
            throw new ArgumentOutOfRangeException(nameof(outcomeRandom), "Outcome draw must lie in [0, 1).");
        return Math.Floor(draw * range) == 0 ? currentFee - 200 : currentFee;
    }
}
