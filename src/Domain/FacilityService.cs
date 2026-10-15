namespace OpenGDS2.Domain;

public enum FacilityKind { School, HotSpring, Recycling, TechnologySchool, Casino, Lawyer, MysteryLab, ProductionLab, HardwareLab }
public enum FacilityResult { Success, CalendarUnknown, EventPending, InvalidSlot, SlotOccupied, AlreadyOwned, MoneyUnknown, InsufficientFunds, NotOwned, StateUnknown, NoEmployees, NotEmployed, ExperimentRunning }

/// <summary>Authored result snapshot; applying a service and acknowledging it are separate operations.</summary>
public sealed class FacilityReceipt
{
    [System.Text.Json.Serialization.JsonRequired]
    public FacilityKind Kind {get;set;}
    [System.Text.Json.Serialization.JsonRequired]
    public long Amount {get;set;}
    public int? Gain {get;set;}
    public string? EmployeeName {get;set;}
}

/// <summary>fGMTenaSel, fGMTenantReS, fGMTenantTechSel and fGMTenantSeisan verified transactions.</summary>
public static class FacilityService
{
    private static readonly long[] Prices=[600_000_000,60_000_000,80_000_000,100_000_000,500_000_000,250_000_000,300_000_000,150_000_000,1_000_000_000];
    public const long TechnologyStudyCost=40_000_000;
    public const long ProductionExpansionCost=10_000_000;
    public const long RecyclingPrice=500;
    public static long PurchasePrice(FacilityKind kind)=>Prices[ValidKind(kind)];
    public static bool ServiceAvailable(FacilityKind kind)=>kind is FacilityKind.School or FacilityKind.HotSpring or FacilityKind.Recycling or FacilityKind.TechnologySchool or FacilityKind.ProductionLab or FacilityKind.Lawyer or FacilityKind.MysteryLab or FacilityKind.HardwareLab or FacilityKind.Casino;
    private static int ValidKind(FacilityKind kind)=>Enum.IsDefined(kind) ? (int)kind : throw new ArgumentOutOfRangeException(nameof(kind));
    public static bool HasPendingDecision(GameState state,bool ignoreCasino=false)=>CompanyEventService.HasPending(state) || (!ignoreCasino && state.Casino?.ActivityOpen==true) || state.FinancialFailure is not null || OwnHardwareService.HasPending(state) || (state.PendingFacilityReceipt is not null || state.PendingFeeNegotiation is not null || state.PendingMysteryLabResult is not null) || CalendarService.HasPending(state) ||
        state.PendingProjectCancellation is not null || state.PendingSecretaryAdvice is not null || state.PendingSecretaryReplacement is not null ||
        state.PendingBankLoanReceipt is not null || state.PendingHireGreeting is not null || RecallService.HasPending(state) ||
        SalesMilestoneService.HasPending(state) || state.DevelopmentProjects.Any(p=>p.PendingDecision is not null || CheckpointMeetingRules.HasPending(p)) ||
        state.ReleasedGames.Any(p=>p.ReviewPending || p.ShipmentMessages is { } messages && p.ShipmentMessageIndex<messages.Count);
    private static FacilityResult CheckSlot(GameState state,int slot)
    {
        if (state.Calendar is null) return FacilityResult.CalendarUnknown;
        if (HasPendingDecision(state)) return FacilityResult.EventPending;
        if (slot is <0 or >2) return FacilityResult.InvalidSlot;
        if (state.Calendar.OriginalSlots0444 is not {Count:3} cells || cells.Any(x=>x is <-1 or >8)) return FacilityResult.StateUnknown;
        return FacilityResult.Success;
    }
    public static FacilityResult Open(GameState state,int slot,FacilityKind kind)
    {
        int type=ValidKind(kind);var result=CheckSlot(state,slot);if (result!=FacilityResult.Success) return result;
        var cells=state.Calendar!.OriginalSlots0444;
        // The original affordability test precedes the uniqueness scan.
        if (state.Money is not long money) return FacilityResult.MoneyUnknown;
        if (money<Prices[type]) return FacilityResult.InsufficientFunds;
        if (cells.Contains(type)) return FacilityResult.AlreadyOwned;
        if (cells[slot]!=-1) return FacilityResult.SlotOccupied;
        state.Money=checked(money-Prices[type]);cells[slot]=type;
        return FacilityResult.Success;
    }
    public static FacilityResult Close(GameState state,int slot,FacilityKind expectedKind)
    {
        int type=ValidKind(expectedKind);var result=CheckSlot(state,slot);if (result!=FacilityResult.Success) return result;
        if (state.Calendar!.OriginalSlots0444[slot]!=type) return FacilityResult.NotOwned;
        state.Calendar.OriginalSlots0444[slot]=-1;return FacilityResult.Success;
    }
    internal static FacilityResult CheckService(GameState state,int slot,FacilityKind kind)
    {
        var result=CheckSlot(state,slot);if (result!=FacilityResult.Success) return result;
        return state.Calendar!.OriginalSlots0444[slot]==(int)kind ? FacilityResult.Success : FacilityResult.NotOwned;
    }
    public static FacilityResult HotSpring(GameState state,int slot)
    {
        var result=CheckService(state,slot,FacilityKind.HotSpring);if (result!=FacilityResult.Success) return result;
        var care=EmployeeCareService.TryHotSpring(state);
        if (care!=EmployeeCareResult.Success) return care switch
        {EmployeeCareResult.NoEmployees=>FacilityResult.NoEmployees,EmployeeCareResult.MoneyUnknown=>FacilityResult.MoneyUnknown,_=>FacilityResult.InsufficientFunds};
        state.PendingFacilityReceipt=new() {Kind=FacilityKind.HotSpring,Amount=EmployeeCareService.HotSpringCost};
        return FacilityResult.Success;
    }
    public static FacilityResult Recycle(GameState state,int slot)
    {
        var result=CheckService(state,slot,FacilityKind.Recycling);if (result!=FacilityResult.Success) return result;
        if (state.Money is null) return FacilityResult.MoneyUnknown;
        long stock=StockForRecycling(state),amount=checked(stock*RecyclingPrice);
        var finance=FinancialRules.Prepare(state,amount);
        foreach (var product in state.ReleasedGames) product.Stock=0;
        finance.Commit(state);state.PendingFacilityReceipt=new() {Kind=FacilityKind.Recycling,Amount=amount};
        return FacilityResult.Success;
    }
    public static long StockForRecycling(GameState state)
    {
        long stock=0;
        foreach (var product in state.ReleasedGames)
        {if (product.Stock<0) throw new InvalidDataException("Negative product stock.");stock=checked(stock+product.Stock);}
        return stock;
    }
    // Opening the original selector builds the gate-1 source roster and writes
    // its count into the shared payroll scratch cell, even when study is cancelled.
    public static IReadOnlyList<EmployeeState>? OpenTechnologyRoster(GameState state,int slot)
    {
        if (CheckService(state,slot,FacilityKind.TechnologySchool)!=FacilityResult.Success ||
            state.SourceUpperBound is not int upper || upper<=0 || state.EmployeeSources.Count!=upper ||
            !state.EmployeeSources.Select(x=>x.SourceIndex).Order().SequenceEqual(Enumerable.Range(0,upper))) return null;
        var sources=state.EmployeeSources.Where(x=>x.GateValue==1).OrderBy(x=>x.SourceIndex).ToList();
        var employees=new List<EmployeeState>();
        foreach (var source in sources)
        {
            var matches=state.Employees.Where(x=>x.SourceIndex==source.SourceIndex).ToList();
            if (matches.Count!=1 || matches[0].MaximumTechnologies is null || matches[0].MaximumTechnologies!=source.Employee.MaximumTechnologies) return null;
            employees.Add(matches[0]);
        }
        if (employees.Count!=state.Employees.Count) return null;
        state.OriginalPayrollScratchIndex=employees.Count;return employees;
    }
    public static FacilityResult Study(GameState state,int slot,EmployeeState employee)
    {
        var result=CheckService(state,slot,FacilityKind.TechnologySchool);if (result!=FacilityResult.Success) return result;
        if (!state.Employees.Contains(employee)) return FacilityResult.NotEmployed;
        var sources=state.EmployeeSources.Where(x=>x.SourceIndex==employee.SourceIndex).ToList();
        if (employee.MaximumTechnologies is not int capacity || capacity<0 || sources.Count!=1 || sources[0].GateValue!=1 ||
            sources[0].Employee.MaximumTechnologies!=capacity) return FacilityResult.StateUnknown;
        if (state.Money is not long money) return FacilityResult.MoneyUnknown;
        if (money<TechnologyStudyCost) return FacilityResult.InsufficientFunds;
        int next=checked(capacity+1);var finance=FinancialRules.Prepare(state,-TechnologyStudyCost);
        employee.RawParameter34=next;sources[0].Employee.RawParameter34=next;finance.Commit(state);
        state.PendingFacilityReceipt=new() {Kind=FacilityKind.TechnologySchool,Amount=TechnologyStudyCost,Gain=next,EmployeeName=employee.Name};
        return FacilityResult.Success;
    }
    public static FacilityResult ExpandProduction(GameState state,int slot,Random random)
    {
        var result=CheckService(state,slot,FacilityKind.ProductionLab);if (result!=FacilityResult.Success) return result;
        if (state.Money is not long money) return FacilityResult.MoneyUnknown;
        if (state.Company.ProductionCapacity is not int capacity || capacity<0) return FacilityResult.StateUnknown;
        float draw=random.NextSingle();
        if (!float.IsFinite(draw) || draw<0 || draw>=1) throw new ArgumentOutOfRangeException(nameof(random));
        int gain=15+(int)Math.Floor(draw*5d),next=checked(capacity+gain);
        long cash=checked(money-ProductionExpansionCost);
        // This branch neither checks affordability nor calls the accumulator.
        state.Company.ProductionCapacity=next;state.Money=cash;
        state.PendingFacilityReceipt=new() {Kind=FacilityKind.ProductionLab,Amount=ProductionExpansionCost,Gain=gain};
        return FacilityResult.Success;
    }
    public static void ValidateReceipt(FacilityReceipt receipt)
    {
        bool valid=receipt.Kind switch
        {
            FacilityKind.HotSpring=>receipt.Amount==EmployeeCareService.HotSpringCost && receipt.Gain is null && receipt.EmployeeName is null,
            FacilityKind.Recycling=>receipt.Amount>=0 && receipt.Amount%RecyclingPrice==0 && receipt.Gain is null && receipt.EmployeeName is null,
            FacilityKind.TechnologySchool=>receipt.Amount==TechnologyStudyCost && receipt.Gain>0 && !string.IsNullOrWhiteSpace(receipt.EmployeeName),
            FacilityKind.ProductionLab=>receipt.Amount==ProductionExpansionCost && receipt.Gain is >=15 and <=19 && receipt.EmployeeName is null,
            _=>false
        };
        if (!valid) throw new InvalidDataException("Invalid facility result.");
    }
}
