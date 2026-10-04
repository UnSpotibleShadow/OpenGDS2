namespace OpenGDS2.Domain;

public sealed class CompanyState
{
    public string Name { get; set; } = string.Empty;
    public string PresidentName { get; set; } = "伊集院 光の助";

    // Fame (company recognition, percent) and rank opening values are statically
    // verified. Development power remains unresolved.
    public int? Fame { get; set; }
    public int? DevelopmentPower { get; set; }
    public int? Rank { get; set; }
    public int? ProductionCapacity { get; set; }
    // Original member 90: genre research, confirmed by fGMSeisanZ.
    public int? OriginalMember90 { get; set; }
    public int? HallOfFameCount { get; set; }
    public long? ActivityCost { get; set; }
    // Original signed financial accumulators 22C, 23C and 24C. Missing older
    // save totals stay unknown; they cannot be reconstructed from current cash.
    public long? WeeklyNet { get; set; }
    public long? MonthlyNet { get; set; }
    public long? YearlyNet { get; set; }
}
