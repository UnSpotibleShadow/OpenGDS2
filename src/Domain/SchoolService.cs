namespace OpenGDS2.Domain;

public enum SchoolResult { Success, CalendarUnknown, EventPending, InvalidSlot, SlotOccupied, AlreadyOwned, MoneyUnknown, InsufficientFunds, NotOwned }

/// <summary>Company label slots 21–23, fGMTenaSel purchase and fGMTenantGameS closure.</summary>
public static class SchoolService
{
    public const long PurchasePrice=600_000_000;
    public static SchoolResult Open(GameState state,int slot)
    {
        var c=state.Calendar;
        if (c is null) return SchoolResult.CalendarUnknown;
        if (FacilityService.HasPendingDecision(state)) return SchoolResult.EventPending;
        if (slot is <0 or >2) return SchoolResult.InvalidSlot;
        if (c.OriginalSlots0444.Contains(0)) return SchoolResult.AlreadyOwned;
        if (c.OriginalSlots0444[slot]!=-1) return SchoolResult.SlotOccupied;
        if (state.Money is not long money) return SchoolResult.MoneyUnknown;
        if (money<PurchasePrice) return SchoolResult.InsufficientFunds;
        // 00988F78 +0123..0134 directly stores cash, without 0097DD04.
        state.Money=checked(money-PurchasePrice);c.OriginalSlots0444[slot]=0;
        return SchoolResult.Success;
    }
    public static SchoolResult Close(GameState state,int slot)
    {
        var c=state.Calendar;
        if (c is null) return SchoolResult.CalendarUnknown;
        if (FacilityService.HasPendingDecision(state)) return SchoolResult.EventPending;
        if (slot is <0 or >2) return SchoolResult.InvalidSlot;
        if (c.OriginalSlots0444[slot]!=0) return SchoolResult.NotOwned;
        c.OriginalSlots0444[slot]=-1;return SchoolResult.Success;
    }
}
