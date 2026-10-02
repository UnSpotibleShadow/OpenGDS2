namespace OpenGDS2.Domain;

public readonly record struct DevelopmentQuote(long Cost, int Points);

/// <summary>fGMKaihatu's verified hardware/audience calculation; not a complete development workflow.</summary>
public static class DevelopmentPlanningRules
{
    public static DevelopmentQuote Quote(HardwareDefinition hardware, AudienceDefinition audience, int originalMode, long? mode2AdditionalCost = null)
    {
        if (originalMode is < 0 or > 2) throw new ArgumentOutOfRangeException(nameof(originalMode));
        long audienceCost = audience.RawMembers["148C"];
        long audiencePoints = audience.RawMembers["1454"];
        if (hardware.PlanningCost < 0 || audienceCost < 0 || hardware.PlanningPoints < 0 || audiencePoints < 0)
            throw new InvalidDataException("Invalid planning catalog.");
        long additional = originalMode == 2 ? mode2AdditionalCost ?? throw new InvalidOperationException("Mode 2 requires its original additional cost.") : 0;
        if (additional < 0) throw new InvalidDataException("Invalid additional cost.");
        return new(checked(hardware.PlanningCost + audienceCost + additional), checked((int)(hardware.PlanningPoints + audiencePoints)));
    }
}
