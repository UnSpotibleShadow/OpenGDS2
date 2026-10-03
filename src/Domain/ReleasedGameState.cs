namespace OpenGDS2.Domain;

/// <summary>Own-product release/sales fields, copied from the completed development room.</summary>
public sealed class ReleasedGameState
{
    public int ProductIndex {get;set;}
    public string Title {get;set;}=string.Empty;
    public int HardwareSourceIndex {get;set;}
    public int AudienceSourceIndex {get;set;}
    public int GameTypeSourceIndex {get;set;}
    public int GenreSourceIndex {get;set;}
    public DevelopmentQuality Quality {get;set;}=new();
    public int Bugs {get;set;}
    public double Recognition {get;set;}
    public double DataSize {get;set;}
    public double MarketFactor {get;set;}
    public int Demand {get;set;}=100;
    public long Price {get;set;}
    public long Stock {get;set;}
    public long UnitsSold {get;set;}
    // Modern accounting projections; no invented gameplay bonuses.
    public long NetSalesReceipts {get;set;}
    public long ProductionSpending {get;set;}
    public long LastBatchSold {get;set;}
    // Older modern saves have unknown reviews; loading must not reroll them.
    public List<int>? ReviewScores {get;set;}
    public bool ReviewPending {get;set;}
    public bool HallOfFame {get;set;}
    public int? SalesMilestoneStage {get;set;}
    public List<int> PendingSalesMilestones {get;set;}=[];
}
