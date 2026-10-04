using System.Text.Json.Serialization;

namespace OpenGDS2.Domain;

public enum CalendarPhase
{
    EarlyEvents, TenantsAndPayroll, HardwareTriggers, HardwareLaunch,
    HardwareRetirement, Recalls, Markets, Exhibition, Loans, Graduates, Complete
}
public enum CalendarEvent
{
    ContractRenewal, HardwareDevelopment, HardwareSoon, HardwareLaunch,
    HardwareRetiring, HardwareRetired, LoanRepayment, Exhibition, Awards, Graduates, BeginnerEnding
}
public enum CalendarBlock { PayrollUnknown, CatalogUnknown, BeginnerEnding, Awards, Exhibition, Graduates }

/// <summary>Mutable local projections of the original global arrays, never catalog edits.</summary>
public sealed class HardwareCalendarState
{
    [JsonRequired]
    public int SourceIndex { get; set; }
    [JsonRequired]
    public int Availability { get; set; }
    [JsonRequired]
    public int LaunchWeeks { get; set; }
    [JsonRequired]
    public int RetirementWeeks { get; set; }
    [JsonRequired]
    public double Market { get; set; }
    [JsonRequired]
    public int GrowthWeeks { get; set; }
    [JsonRequired]
    public int GrowthBoost { get; set; }
}
public sealed class CalendarLoanState
{
    [JsonRequired]
    public int OriginalType { get; set; }
    public GameDate? BorrowedOn { get; set; }
}
public sealed class CalendarTenantState
{
    [JsonRequired]
    public int OriginalState { get; set; }
    [JsonRequired]
    public int Weeks { get; set; }
}
public sealed class CalendarTransaction
{
    [JsonRequired]
    public GameDate ClosingDate { get; set; }
    [JsonRequired]
    public CalendarPhase Phase { get; set; }
    [JsonRequired]
    public int Index { get; set; }
    public CalendarEvent? PendingEvent { get; set; }
    public CalendarBlock? Block { get; set; }
    public long? Amount { get; set; }
    public ExhibitionState? Exhibition { get; set; }
    public AwardCeremonyState? Awards { get; set; }
    public GraduateEventState? Graduates { get; set; }
}
public sealed class CompanyCalendarState
{
    [JsonRequired]
    public int HardwareStage { get; set; }
    [JsonRequired]
    public int ShipmentsSinceHardwareTrigger { get; set; }
    [JsonRequired]
    public List<HardwareCalendarState> Hardware { get; set; } = [];
    [JsonRequired]
    public List<CalendarLoanState> Loans { get; set; } = [];
    [JsonRequired]
    public List<CalendarTenantState> Tenants { get; set; } = [];
    // Original member 444: startup writes -1 to all three slots. Semantics
    // remain structural; a zero enables the April graduate branch.
    [JsonRequired]
    public List<int> OriginalSlots0444 { get; set; } = [];
    public CalendarTransaction? Pending { get; set; }
    // Unknown in older saves; do not manufacture their previous records.
    public int? ExhibitionVisitorRecord { get; set; }
    public GameDate? FirstGrandPrizeDate { get; set; }
}
