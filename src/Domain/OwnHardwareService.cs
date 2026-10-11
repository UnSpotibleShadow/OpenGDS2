using System.Text.Json.Serialization;

namespace OpenGDS2.Domain;

public sealed class HardwareDesignState
{
    [JsonRequired] public int Design {get;set;}
    [JsonRequired] public int Medium {get;set;}
    [JsonRequired] public Dictionary<int,int> TechnologyFlags {get;set;}=OwnHardwareService.NewFlags();
    public int? PendingTechnology {get;set;}
    // Modern draft field: preserves the launch-name input across save/reload.
    public string? LaunchName {get;set;}
}
public sealed class OwnHardwareState
{
    [JsonRequired] public int SelectedDesign {get;set;}
    [JsonRequired] public int SelectedMedium {get;set;}
    [JsonRequired] public List<Dictionary<int,int>> RetainedRoomFlags {get;set;}=[OwnHardwareService.NewFlags(),OwnHardwareService.NewFlags()];
    public HardwareDefinition? Launched {get;set;}
    public int? LaunchedDesign {get;set;}
    public int? LaunchedMedium {get;set;}
}
public enum OwnHardwareResult { Success, FacilityUnavailable, EventPending, StateUnknown, AlreadyLaunched, ProjectRunning,
    RoomOccupied, MoneyUnknown, InsufficientFunds, NotReady, InvalidName }

/// <summary>009864AC entry; 00990B70 acceptance; 00A0283C hardware work/completion; 0099F384 launch.</summary>
public static class OwnHardwareService
{
    public static readonly int[] RequiredTechnologies=[1,3,6,7];
    public static Dictionary<int,int> NewFlags()=>RequiredTechnologies.ToDictionary(x=>x,_=>0);
    public static long Quote(int design,int medium)
    {
        if (design is <0 or >1 || medium is <0 or >1) throw new ArgumentOutOfRangeException(nameof(design));
        return (design==0 ? 200_000_000L : 100_000_000L)+(medium==0 ? 200_000_000L : 100_000_000L);
    }
    public static int OwnPictureIndex(int design,int medium) {_=Quote(design,medium);return 11+design*2+medium;}
    public static bool Ready(DevelopmentProjectState p)=>p.OriginalRoomState==2 && p.HardwareDesign is {PendingTechnology:null} h &&
        p.Progress>=p.RequiredPoints && p.PolishingTargets is { } t &&
        p.Quality.Fun>=t.Fun && p.Quality.Novelty>=t.Novelty && p.Quality.Graphics>=t.Graphics && p.Quality.Music>=t.Music &&
        RequiredTechnologies.All(x=>h.TechnologyFlags.GetValueOrDefault(x)==1);
    public static bool HasPending(GameState state)=>state.DevelopmentProjects.Any(p=>p.HardwareDesign?.PendingTechnology is not null || Ready(p));
    public static OwnHardwareResult CheckStart(GameState state,OriginalCatalog catalog,int slot,int room,int design,int medium)
    {
        _=Quote(design,medium);
        if (room is <0 or >1) throw new ArgumentOutOfRangeException(nameof(room));
        if (FacilityService.HasPendingDecision(state)) return OwnHardwareResult.EventPending;
        if (state.Calendar?.OriginalSlots0444 is not { } slots || slot<0 || slot>=slots.Count || slots[slot]!=8)
            return OwnHardwareResult.FacilityUnavailable;
        if (state.OwnHardware.Launched is not null) return OwnHardwareResult.AlreadyLaunched;
        if (state.DevelopmentProjects.Any(p=>p.OriginalRoomState==2)) return OwnHardwareResult.ProjectRunning;
        if (state.Money is not long money) return OwnHardwareResult.MoneyUnknown;
        if (money<Quote(design,medium)) return OwnHardwareResult.InsufficientFunds;
        if (state.DevelopmentProjects.Any(p=>p.RoomIndex==room)) return OwnHardwareResult.RoomOccupied;
        if (!catalog.Hardware.OrderBy(x=>x.SourceIndex).Select(x=>x.SourceIndex).SequenceEqual(Enumerable.Range(0,11)) ||
            !state.Calendar.Hardware.Select(x=>x.SourceIndex).SequenceEqual(Enumerable.Range(0,11)) ||
            state.AvailableHardware is null || state.HardwareLicenses is null) return OwnHardwareResult.StateUnknown;
        return OwnHardwareResult.Success;
    }
    public static OwnHardwareResult Start(GameState state,OriginalCatalog catalog,int slot,int room,int design,int medium,string label)
    {
        var result=CheckStart(state,catalog,slot,room,design,medium);if (result!=OwnHardwareResult.Success) return result;
        if (string.IsNullOrWhiteSpace(label) || label.Length>128) return OwnHardwareResult.InvalidName;
        var finance=FinancialRules.Prepare(state,-Quote(design,medium));
        var project=new DevelopmentProjectState {RoomIndex=room,Title=label,OriginalRoomState=2,OriginalMode=1,
            RequiredPoints=2500,Focus=new(25,25,25,25),PolishingTargets=new(800,800,800,800),
            TechnologyApplicationCounts=TechnologyApplicationService.NewProjectCounters(),
            HardwareDesign=new() {Design=design,Medium=medium,TechnologyFlags=new(state.OwnHardware.RetainedRoomFlags[room])}};
        finance.Commit(state);state.OwnHardware.SelectedDesign=design;state.OwnHardware.SelectedMedium=medium;
        state.DevelopmentProjects.Add(project);return OwnHardwareResult.Success;
    }
    // Ordered inclusive intervals: a full target does not fall through into the next interval.
    public static DevelopmentWorkCategory? SelectCategory(DevelopmentProjectState p,int roll)
    {
        if (p.OriginalRoomState!=2 || p.PolishingTargets is not { } t || roll is <0 or >=400)
            throw new ArgumentOutOfRangeException(nameof(roll));
        var q=p.Quality;
        if (roll<=50) return q.Fun<t.Fun ? DevelopmentWorkCategory.Fun : null;
        if (roll<=100) return q.Novelty<t.Novelty ? DevelopmentWorkCategory.Novelty : null;
        if (roll<=150) return q.Graphics<t.Graphics ? DevelopmentWorkCategory.Graphics : null;
        if (roll<=200) return q.Music<t.Music ? DevelopmentWorkCategory.Music : null;
        return p.Progress<p.RequiredPoints ? DevelopmentWorkCategory.Speed : null;
    }
    public static void QueueTechnology(DevelopmentProjectState p,int identifier)
    {
        if (p.HardwareDesign is not { } h || !RequiredTechnologies.Contains(identifier)) throw new InvalidDataException("Invalid hardware technology.");
        if (h.PendingTechnology is not null) throw new InvalidOperationException("A hardware technology message is pending.");
        h.PendingTechnology=identifier;
    }
    public static bool AcknowledgeTechnology(GameState state,DevelopmentProjectState p)
    {
        if (!state.DevelopmentProjects.Contains(p) || p.HardwareDesign is not {PendingTechnology:int id} h) return false;
        h.TechnologyFlags[id]=1;h.PendingTechnology=null;
        state.OwnHardware.RetainedRoomFlags[p.RoomIndex]=new(h.TechnologyFlags);return true;
    }
    public static HardwareDefinition Definition(string name,int medium)=>new() {SourceIndex=11,Name=name,
        Description="Self-developed hardware",RawMembers=new() {
            ["0EC8"]=11,["0FA8"]=medium==0 ? 3 : 4,["0F00"]=64,["0FC4"]=0,["0F1C"]=1,
            ["1050"]=medium==0 ? 0 : 10,["0F38"]=1,["0F54"]=medium==0 ? 50 : 36,
            ["0FE0"]=medium==0 ? 400 : 600,["0FFC"]=0,["0F70"]=400000,["106C"]=0,["1088"]=0,["10A4"]=240,["10C0"]=10},
        ProductionBands=medium==0 ? [new(64,600),new(128,1000),new(256,1500),new(512,2000)] :
            [new(20,700),new(40,1000),new(60,1300),new(80,1500)]};
    public static OwnHardwareResult Launch(GameState state,DevelopmentProjectState p,string name)
    {
        if (!state.DevelopmentProjects.Contains(p) || !Ready(p)) return OwnHardwareResult.NotReady;
        if (state.OwnHardware.Launched is not null) return OwnHardwareResult.AlreadyLaunched;
        if (string.IsNullOrWhiteSpace(name) || name.Length>128) return OwnHardwareResult.InvalidName;
        if (state.Calendar is null || !state.Calendar.Hardware.Select(x=>x.SourceIndex).SequenceEqual(Enumerable.Range(0,11)) ||
            state.AvailableHardware is null || state.HardwareLicenses is null) return OwnHardwareResult.StateUnknown;
        int medium=p.HardwareDesign!.Medium;var hardware=Definition(name.Trim(),medium);
        var market=new HardwareCalendarState {SourceIndex=11,Availability=1,Market=medium==0 ? 0 : 10,GrowthWeeks=240,GrowthBoost=10};
        state.OwnHardware.Launched=hardware;state.OwnHardware.LaunchedDesign=p.HardwareDesign.Design;state.OwnHardware.LaunchedMedium=medium;
        state.OwnHardware.RetainedRoomFlags[p.RoomIndex]=new(p.HardwareDesign.TechnologyFlags);
        state.Calendar.Hardware.Add(market);state.AvailableHardware.Add(11);state.HardwareLicenses.Add(11);
        state.DevelopmentProjects.Remove(p);RankingService.Queue(state,RankingKind.OwnHardware);
        // Match the existing modern shipment safeguard against stale work entering a new project.
        foreach (var employee in state.Employees.Where(e=>e.Office?.SeatedAction is >=6 and <=17 &&
            (e.Office.SeatedAction-6)/6==p.RoomIndex)) ReleaseService.ResetWork(employee.Office!);
        return OwnHardwareResult.Success;
    }
    public static OriginalCatalog Catalog(GameState state,OriginalCatalog catalog)
    {
        if (state.OwnHardware.Launched is not { } own) return catalog;
        if (catalog.Hardware.Any(x=>ReferenceEquals(x,own))) return catalog;
        return new() {SchemaVersion=catalog.SchemaVersion,SourceHash=catalog.SourceHash,Genres=catalog.Genres,Audiences=catalog.Audiences,
            DevelopmentComputers=catalog.DevelopmentComputers,Hardware=catalog.Hardware.Where(x=>x.SourceIndex!=11).Append(own).ToList(),
            GameTypes=catalog.GameTypes,Technologies=catalog.Technologies,InitialRivalProducts=catalog.InitialRivalProducts,
            StagedRivalProducts=catalog.StagedRivalProducts,RivalCompanies=catalog.RivalCompanies,Manga=catalog.Manga,Secretaries=catalog.Secretaries};
    }
    private static bool ValidFlags(Dictionary<int,int>? flags)=>flags is not null && flags.Keys.Order().SequenceEqual(RequiredTechnologies) && flags.Values.All(x=>x is 0 or 1);
    public static void Validate(GameState state)
    {
        var own=state.OwnHardware;
        if (own is null || own.SelectedDesign is <0 or >1 || own.SelectedMedium is <0 or >1 ||
            own.RetainedRoomFlags is not {Count:2} || own.RetainedRoomFlags.Any(x=>!ValidFlags(x))) throw new InvalidDataException("Invalid own-hardware state.");
        if (state.DevelopmentProjects.Any(x=>x is null)) throw new InvalidDataException("Invalid development rooms.");
        var projects=state.DevelopmentProjects.Where(x=>x.OriginalRoomState==2).ToList();
        if (projects.Count>1 || projects.Count>0 && own.Launched is not null) throw new InvalidDataException("Multiple own consoles.");
        foreach (var p in state.DevelopmentProjects)
        {
            if (p.OriginalRoomState!=2) {if (p.HardwareDesign is not null) throw new InvalidDataException("Hardware design on software.");continue;}
            var h=p.HardwareDesign;
            if (p.Quality is null || h is null || h.Design is <0 or >1 || h.Medium is <0 or >1 || !ValidFlags(h.TechnologyFlags) ||
                h.PendingTechnology is int id && !RequiredTechnologies.Contains(id) || h.LaunchName is {Length:>128} || p.RequiredPoints!=2500 || p.OriginalStage!=0 ||
                p.PolishingTargets!=new DevelopmentQuality(800,800,800,800) || p.PendingDecision is not null || p.CheckpointMeeting is not null ||
                p.PredecessorProductIndex is not null || p.MangaSourceIndex is not null || p.OriginalMode!=1 ||
                p.Quality.Fun>800 || p.Quality.Novelty>800 || p.Quality.Graphics>800 || p.Quality.Music>800)
                throw new InvalidDataException("Invalid hardware development.");
        }
        if (HasPending(state) && (CalendarService.HasPending(state) || state.PendingFacilityReceipt is not null || state.PendingFeeNegotiation is not null ||
            state.PendingMysteryLabResult is not null || state.PendingHireGreeting is not null || state.PendingSecretaryAdvice is not null ||
            state.PendingSecretaryReplacement is not null || state.PendingBankLoanReceipt is not null || state.PendingProjectCancellation is not null ||
            state.DevelopmentProjects.Any(p=>p.PendingDecision.HasValue || CheckpointMeetingRules.HasPending(p)) ||
            state.ReleasedGames.Any(p=>p.ReviewPending) || SalesMilestoneService.HasPending(state) || RecallService.HasPending(state)))
            throw new InvalidDataException("Overlapping hardware modal.");
        if (own.Launched is { } hardware)
        {
            if (own.LaunchedDesign is not (0 or 1) || own.LaunchedMedium is not (0 or 1) || hardware.SourceIndex!=11 ||
                string.IsNullOrWhiteSpace(hardware.Name) || hardware.Name.Length>128 || hardware.RawMembers is null || hardware.ProductionBands is null)
                throw new InvalidDataException("Invalid launched console.");
            var expected=Definition(hardware.Name,own.LaunchedMedium.Value);
            if (hardware.RawMembers.Count!=expected.RawMembers.Count || hardware.RawMembers.Any(x=>!expected.RawMembers.TryGetValue(x.Key,out long v) || v!=x.Value) ||
                !hardware.ProductionBands.SequenceEqual(expected.ProductionBands) || state.HardwareLicenses?.Contains(11)!=true ||
                state.AvailableHardware?.Contains(11)!=true || state.Calendar?.Hardware.Count(x=>x.SourceIndex==11)!=1)
                throw new InvalidDataException("Own console disagrees with market/license state.");
        }
        else if (own.LaunchedDesign.HasValue || own.LaunchedMedium.HasValue || state.HardwareLicenses?.Contains(11)==true || state.AvailableHardware?.Contains(11)==true ||
            state.Calendar?.Hardware.Any(x=>x.SourceIndex==11)==true || state.ReleasedGames.Any(x=>x.HardwareSourceIndex==11) ||
            state.DevelopmentProjects.Any(x=>x.HardwareSourceIndex==11)) throw new InvalidDataException("Missing own console.");
    }
}
