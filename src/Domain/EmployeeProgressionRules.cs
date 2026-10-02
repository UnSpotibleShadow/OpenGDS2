namespace OpenGDS2.Domain;

/// <summary>Recovered progression arithmetic, separated from unresolved work/task scheduling.</summary>
public static class EmployeeProgressionRules
{
    // Explicit draws allow evidence fixtures independent of a modern RNG implementation.
    // Original Rnd is single precision. No claim of bit-for-bit VB6 RNG compatibility.
    public static int SuperBonus(float draw)
    {
        ValidateDraw(draw);
        return (int)Math.Floor((double)draw * 10) == 0 ? 10 : 0;
    }
    public static int AttributeGain(int current, int growthCoefficient, int bonus, float draw)
    {
        ValidateDraw(draw);
        if (current is < 0 or > 99 || growthCoefficient < 0 || bonus is not (0 or 10))
            throw new ArgumentOutOfRangeException(nameof(current));
        int gain = checked((int)Math.Floor((double)draw * growthCoefficient) + growthCoefficient / 3 + bonus);
        return Math.Min(gain, 99 - current);
    }
    // Module2's work branches add 3.7 or 10, then clamp against the employee's
    // stored current-level threshold. Their scheduling/eligibility remains unresolved.
    public static double AccrueExperience(double current, double gain, int level)
    {
        if (!double.IsFinite(current) || !double.IsFinite(gain) || current < 0 || gain < 0)
            throw new ArgumentOutOfRangeException(nameof(current));
        return Math.Min(current + gain, EmployeeLevelUpService.RequiredExperience(level));
    }
    private static void ValidateDraw(float draw)
    {
        if (!float.IsFinite(draw) || draw < 0 || draw >= 1) throw new ArgumentOutOfRangeException(nameof(draw));
    }
}
