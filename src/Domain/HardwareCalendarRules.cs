namespace OpenGDS2.Domain;

/// <summary>Bounded weekly branches 009DB4F4 +0777..1479 and +1548..1668.</summary>
public static class HardwareCalendarRules
{
    // These are eligibility predicates, not a reordered timeline. Source record
    // order matters, including the two original stage-six alternatives.
    private static (int Id,int Stage,long Funds,int[] Retire,int[] Weeks)[] Gates =
    [
        (3,0,150_000_000,[],[]), (4,1,300_000_000,[0],[14]),
        (2,2,450_000_000,[1],[14]), (10,3,70_000_000,[],[]),
        (5,4,1_000_000_000,[8],[14]), (9,5,1_200_000_000,[3],[14]),
        (6,6,1_400_000_000,[4,2],[14,17]), (7,6,1_600_000_000,[],[])
    ];
    public static bool TryTrigger(GameState state,OriginalCatalog catalog,HardwareCalendarState hardware)
    {
        var calendar=state.Calendar ?? throw new InvalidOperationException("Unknown calendar state.");
        int id=catalog.Hardware.Single(x=>x.SourceIndex==hardware.SourceIndex).Identifier;
        if (!Gates.Any(x=>x.Id==id)) return false;
        var gate=Gates.Single(x=>x.Id==id);
        if (calendar.ShipmentsSinceHardwareTrigger<4 || calendar.HardwareStage!=gate.Stage ||
            state.Money is not long money || money<gate.Funds || hardware.LaunchWeeks!=0) return false;
        var retire=gate.Retire.Select((identifier,i)=>(Hardware:calendar.Hardware.Single(
            h=>catalog.Hardware.Single(x=>x.SourceIndex==h.SourceIndex).Identifier==identifier),Weeks:gate.Weeks[i])).ToArray();
        foreach (var item in retire) item.Hardware.RetirementWeeks=item.Weeks;
        calendar.ShipmentsSinceHardwareTrigger=0;hardware.LaunchWeeks=24;
        return true;
    }
    public static double NextMarket(double current,int growthWeeks,int boost,long originalMember0F54)
    {
        if (!double.IsFinite(current) || current<0 || growthWeeks<0 || boost<0 || originalMember0F54<0)
            throw new InvalidDataException("Invalid hardware market cells.");
        double next=growthWeeks==0 ? current : current+((double)boost*boost+growthWeeks)*(originalMember0F54/1.5)/900;
        if (!double.IsFinite(next)) throw new OverflowException("Hardware market overflow.");
        return next;
    }
}
