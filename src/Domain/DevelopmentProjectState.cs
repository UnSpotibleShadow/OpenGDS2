namespace OpenGDS2.Domain;

public enum DevelopmentWorkCategory { Fun=0, Novelty=1, Graphics=2, Music=3, Speed=4, Bugs=5, Debugging=6 }
public enum DevelopmentWorkTier { Small=1, Medium=2, Burst=3 }
public enum DevelopmentMilestone { SelectType, SelectGenre, StartDebugging }
public sealed record DevelopmentFocus(int Fun, int Novelty, int Graphics, int Music);
public sealed record DevelopmentQuality(double Fun=0, double Novelty=0, double Graphics=0, double Music=0);

/// <summary>Verified new-project fields and persisted selection checkpoints.</summary>
public sealed class DevelopmentProjectState
{
    public int RoomIndex {get;set;}
    public int HardwareSourceIndex {get;set;}
    public int AudienceSourceIndex {get;set;}
    public int? GameTypeSourceIndex {get;set;}
    public int? GenreSourceIndex {get;set;}
    public int OriginalRoomState {get;set;}=1;
    public int OriginalStage {get;set;}
    public int OriginalMode {get;set;}
    // Modern label, not a claim about the original title-entry stage.
    public string Title {get;set;}=string.Empty;
    public double RequiredPoints {get;set;}
    public double Progress {get;set;}
    public DevelopmentQuality Quality {get;set;}=new();
    public DevelopmentQuality? PolishingTargets {get;set;}
    public int Bugs {get;set;}
    public double Recognition {get;set;}
    public double DataSize {get;set;}
    public DevelopmentFocus? Focus {get;set;}
    public DevelopmentMilestone? PendingDecision {get;set;}
    public List<int>? TypeCandidates {get;set;}
}
