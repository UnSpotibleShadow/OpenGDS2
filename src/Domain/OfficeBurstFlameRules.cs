namespace OpenGDS2.Domain;

/// <summary>Module2 +2E98..3269; animation state belongs to seats, not employees.</summary>
public sealed class OfficeBurstFlameState
{
    public int[] Ticks {get;set;}=new int[12];
    public int[] SourceX {get;set;}=Enumerable.Repeat(319,12).ToArray();
}

public static class OfficeBurstFlameRules
{
    private static readonly (int X,int Y,int SourceY)[] Seats=[
        (200,102,359),(232,118,359),(151,105,384),(210,147,359),(125,134,384),(157,150,384),
        (296,150,359),(328,166,359),(247,153,384),(306,195,359),(221,182,384),(253,198,384)];

    private static bool Visible(OfficeEmployeeState? office)=>office is {WorkEngaged:true,WorkTier:3,SeatedAction:>=6 and <=17};

    public static void Tick(GameState state,EmployeeState employee)
    {
        if (!Visible(employee.Office)) return;
        int seat=employee.Office!.SeatedAction-6;
        var flames=state.OfficeBurstFlames;
        // Source tests the old counter, changes frame, then increments it.
        if (flames.Ticks[seat]==3*OfficeStaffRules.TimingFactor) flames.SourceX[seat]=319;
        else if (flames.Ticks[seat]>=6*OfficeStaffRules.TimingFactor)
        {flames.SourceX[seat]=346;flames.Ticks[seat]=0;}
        flames.Ticks[seat]++;
    }

    public static OfficeSpriteDefinition Project(OfficeSpriteDefinition original,GameState state)
    {
        if (original.Id is <64 or >75) return original;
        int seat=original.Id-64;
        if (!state.Employees.Any(e=>Visible(e.Office) && e.Office!.SeatedAction==seat+6))
            return original with {X=-100,Y=0};
        var p=Seats[seat];
        return original with {X=p.X,Y=p.Y,SourceX=state.OfficeBurstFlames.SourceX[seat],SourceY=p.SourceY,
            Width=25,Height=25,DepthOffset=38};
    }

    public static void Validate(GameState state)
    {
        if (state.OfficeBurstFlames is not {Ticks.Length:12,SourceX.Length:12} f ||
            f.Ticks.Any(t=>t is <0 or >12) || f.SourceX.Any(x=>x is not (319 or 346)))
            throw new InvalidDataException("Invalid office burst flame animation.");
    }
}
