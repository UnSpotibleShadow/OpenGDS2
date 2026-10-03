namespace OpenGDS2.Domain;

/// <summary>Verified arithmetic only. Do not schedule without the complete weekly pipeline.</summary>
public static class RecallRules
{
    public const int InvestigationWeeks=5;
    public static bool Investigate(int bugs,Random random)
    {
        if (bugs<0) throw new ArgumentOutOfRangeException(nameof(bugs));
        // VB6 evaluates all three Rnd expressions, even when their bug predicate
        // is false. Signed ranges use floor(Rnd*range), including zero/negative.
        double first=random.NextDouble(),second=random.NextDouble(),third=random.NextDouble();
        return bugs==0 && Math.Floor(first*317)==1 ||
            bugs is >=1 and <=10 && Math.Floor(second*(25.0*bugs))==1 ||
            bugs>=11 && Math.Floor(third*(50.0-bugs))<=5;
    }
    public static bool CheckEligible(int demand,int originalRecallState)=>demand<16 && originalRecallState==0;
    public static int NextCountdown(int countdown)
    {
        if (countdown<0) throw new ArgumentOutOfRangeException(nameof(countdown));
        // A zero/empty countdown does not itself trigger mode 12 in the source.
        return countdown>=1 ? countdown-1 : 0;
    }
    public static long Refund(long sold,long stock,long price,int resultRoll)
    {
        if (resultRoll is <0 or >3 || stock<0 || price<0) throw new ArgumentOutOfRangeException(nameof(resultRoll));
        // Sold can be negative because the original sales formula has no floor.
        if (resultRoll==3) return 0;
        return checked(checked(sold+stock)*(resultRoll==0 ? price : 1000));
    }
}
