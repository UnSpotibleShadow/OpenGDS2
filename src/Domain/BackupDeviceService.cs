namespace OpenGDS2.Domain;

public enum BackupPurchaseResult { Success, AlreadyOwned, InsufficientFunds, StateUnknown }
public sealed class BackupDevicePurchaseState
{
    public int Page {get;set;}
    public BackupPurchaseResult? Result {get;set;}
}
public sealed class VirusStaffReaction
{
    public int ActorSlot {get;set;}
    public int? SourceIndex {get;set;}
    public int? PortraitId {get;set;}
    public string Name {get;set;}=string.Empty;
}
public sealed class VirusEventState
{
    public int RoomIndex {get;set;}
    public string Title {get;set;}=string.Empty;
    public bool Protected {get;set;}
    public int Page {get;set;}
    public int StaffIndex {get;set;}
    public int? Reaction {get;set;}
    public List<VirusStaffReaction> Staff {get;set;}=[];
}
public sealed class CompanyEventContinuation
{
    public bool PoachingProcessed {get;set;}
    public int NextVirusRoom {get;set;}
    public int NextInterviewRoom {get;set;}
}

/// <summary>MDIForm1 0098B1F4, native member 02CC and its guarded financial debit.</summary>
public static class BackupDeviceService
{
    public const long Price=100_000_000;
    public static bool Begin(GameState state)
    {
        if(FacilityService.HasPendingDecision(state) || state.PendingCompanyActivity is not null)return false;
        state.PendingBackupDevice=new() {Page=state.BackupDeviceOwned?2:0,
            Result=state.BackupDeviceOwned?BackupPurchaseResult.AlreadyOwned:null};
        return true;
    }
    public static bool Acknowledge(GameState state)
    {
        if(state.PendingBackupDevice is not { } scene || scene.Page==1)return false;
        if(scene.Page==0)scene.Page=1;else state.PendingBackupDevice=null;
        return true;
    }
    public static bool Cancel(GameState state)
    {if(state.PendingBackupDevice is not {Page:1})return false;state.PendingBackupDevice=null;return true;}
    public static BackupPurchaseResult? Confirm(GameState state)
    {
        if(state.PendingBackupDevice is not {Page:1} scene)return null;
        var result=state.BackupDeviceOwned?BackupPurchaseResult.AlreadyOwned:
            state.Money is null?BackupPurchaseResult.StateUnknown:
            state.Money<Price?BackupPurchaseResult.InsufficientFunds:BackupPurchaseResult.Success;
        if(result==BackupPurchaseResult.Success)
        {
            var change=FinancialRules.Prepare(state,-Price);
            state.BackupDeviceOwned=true;change.Commit(state);
        }
        scene.Result=result;scene.Page=2;return result;
    }
}

/// <summary>Resumable periodic virus branch 009E3830 +1BF7..27E7. Other periodic events join this same pass.</summary>
public static class CompanyEventService
{
    public static readonly IReadOnlyList<string> ImportKeys=new[]{"BACKUP_DEVICE_EXPLANATION","BACKUP_DEVICE_RESULT_AlreadyOwned",
        "BACKUP_DEVICE_RESULT_InsufficientFunds","BACKUP_DEVICE_RESULT_Success","VIRUS_INTRO","VIRUS_PROTECTED","VIRUS_STRESS","VIRUS_BUY_BACKUP"}
        .Concat(Enumerable.Range(0,7).Select(i=>$"VIRUS_LOSS_REACTION_{i}"))
        .Concat(Enumerable.Range(0,7).Select(i=>$"VIRUS_PROTECTED_REACTION_{i}")).ToArray();
    public static bool HasPending(GameState state)=>state.PendingBackupDevice is not null || state.PendingVirusEvent is not null || state.PendingPoaching is not null || state.PendingMagazineInterview is not null;
    public static bool Resume(GameState state,Random random)
    {
        if(HasPending(state))return false;
        var cursor=state.CompanyEventContinuation??=new();
        if(!cursor.PoachingProcessed)
        {
            cursor.PoachingProcessed=true;
            if(!PoachingService.Periodic(state,random))return false;
        }
        while(cursor.NextInterviewRoom<2)
        {
            int room=cursor.NextInterviewRoom++;
            var project=state.DevelopmentProjects.SingleOrDefault(p=>p.RoomIndex==room&&p.OriginalRoomState==1);
            if(project is not null&&!MagazineInterviewService.Periodic(state,project,random))return false;
        }
        if(state.ReleasedGames.Count>=5)
        {
            while(cursor.NextVirusRoom<2)
            {
                int room=cursor.NextVirusRoom++;
                // VB's And evaluates the random expression even for an empty room.
                int draw=Draw(random,500_000);
                var project=state.DevelopmentProjects.SingleOrDefault(p=>p.RoomIndex==room && p.OriginalRoomState==1);
                if(project is not null && draw==33)
                {
                    BeginVirus(state,project);return false;
                }
            }
        }
        if(state.BackupDeviceOwned)
        {
            if(state.BackupDeviceAnimationTicks==10)state.BackupDeviceAlternateFrame=false;
            else if(state.BackupDeviceAnimationTicks==18){state.BackupDeviceAlternateFrame=true;state.BackupDeviceAnimationTicks=0;}
            state.BackupDeviceAnimationTicks++;
        }
        state.CompanyEventContinuation=null;return true;
    }
    internal static int Draw(Random random,int span)
    {
        float value=random.NextSingle();
        if(!float.IsFinite(value)||value<0||value>=1)throw new ArgumentOutOfRangeException(nameof(random));
        return (int)Math.Floor((double)value*span);
    }
    public static void BeginVirus(GameState state,DevelopmentProjectState project)
    {
        if(HasPending(state) || !state.DevelopmentProjects.Contains(project) || project.OriginalRoomState!=1)
            throw new InvalidOperationException("The virus event has no available software project.");
        if(state.Employees.Any(e=>e.Office is null))throw new InvalidOperationException("Employee workspace assignments are unknown.");
        var staff=state.Employees.Where(e=>e.Office!.Action>=6+project.RoomIndex*6 && e.Office.Action<=11+project.RoomIndex*6)
            .OrderBy(e=>e.Office!.SlotIndex).ToArray();
        if(staff.Any(e=>e.Stress is null || !double.IsFinite(e.Stress.Value)))throw new InvalidOperationException("Employee stress is unknown.");
        state.PendingVirusEvent=new() {RoomIndex=project.RoomIndex,Title=project.Title,Protected=state.BackupDeviceOwned,
            Staff=staff.Select(e=>new VirusStaffReaction {ActorSlot=e.Office!.SlotIndex,SourceIndex=e.SourceIndex,
                PortraitId=e.PortraitId is >=1 and <=10?e.PortraitId:null,Name=e.Name}).ToList()};
    }
    public static bool AcknowledgeVirus(GameState state,Random random)
    {
        if(state.PendingVirusEvent is not { } scene)return false;
        if(scene.Page==0 && scene.Protected){scene.Page=1;return true;}
        if(scene.Page is 0 or 1){NextStaff();return true;}
        if(scene.Page==2)
        {
            if(!scene.Protected)
            {
                var reaction=scene.Staff[scene.StaffIndex];
                var employee=state.Employees.Single(e=>e.Office?.SlotIndex==reaction.ActorSlot && e.SourceIndex==reaction.SourceIndex);
                employee.Stress=Math.Min(99,employee.Stress!.Value+20);
                var source=state.EmployeeSources.SingleOrDefault(e=>e.SourceIndex==employee.SourceIndex);
                if(source is not null)source.Employee.Stress=employee.Stress;
            }
            scene.StaffIndex++;NextStaff();return true;
        }
        if(scene.Page==3){scene.Page=4;return true;}
        var project=state.DevelopmentProjects.Single(p=>p.RoomIndex==scene.RoomIndex);
        // Native writes room state zero only after the final secretary message.
        // This is not the voluntary cancellation transaction or its extra work resets.
        state.DevelopmentProjects.Remove(project);state.PendingVirusEvent=null;return true;
        void NextStaff()
        {
            if(scene.StaffIndex<scene.Staff.Count){scene.Page=2;scene.Reaction=Draw(random,7);}
            else if(scene.Protected)state.PendingVirusEvent=null;
            else {scene.Page=3;scene.Reaction=null;}
        }
    }
    public static OfficeSpriteDefinition ProjectBackup(OfficeSpriteDefinition sprite,GameState state)=>sprite.Id==142 && state.BackupDeviceOwned
        ?sprite with {X=18,Y=109,SourceX=state.BackupDeviceAlternateFrame?403:379,SourceY=357}:sprite;
    public static void Validate(GameState state)
    {
        if(HasPending(state) && (state.PendingCompanyActivity is not null || CalendarService.HasPending(state) ||
            state.PendingProgrammingChallenge is not null || state.PendingPromotion is not null || state.Casino.ActivityOpen ||
            state.PendingFacilityReceipt is not null || state.PendingFeeNegotiation is not null || state.PendingBankLoanReceipt is not null ||
            state.FinancialFailure is not null || state.PendingHireGreeting is not null || state.PendingProjectCancellation is not null ||
            state.PendingSecretaryAdvice is not null || state.PendingSecretaryReplacement is not null || OwnHardwareService.HasPending(state)))
            throw new InvalidDataException("Overlapping company-event decisions.");
        if(new object?[]{state.PendingBackupDevice,state.PendingPoaching,state.PendingVirusEvent,state.PendingMagazineInterview}.Count(x=>x is not null)>1||
            state.CompanyEventContinuation?.NextInterviewRoom is <0 or >2)throw new InvalidDataException("Overlapping company events or invalid interview cursor.");
        if(state.BackupDeviceAnimationTicks is <0 or >18 || state.CompanyEventContinuation?.NextVirusRoom is <0 or >2)
            throw new InvalidDataException("Invalid periodic company-event cursor.");
        if(state.PendingBackupDevice is { } purchase && (purchase.Page is <0 or >2 || (purchase.Page==2)!=(purchase.Result is not null) ||
            purchase.Result is { } result && !Enum.IsDefined(result) || purchase.Page<2 && state.BackupDeviceOwned ||
            purchase.Result is BackupPurchaseResult.Success or BackupPurchaseResult.AlreadyOwned && !state.BackupDeviceOwned))
            throw new InvalidDataException("Invalid backup-device purchase.");
        if(state.PendingVirusEvent is not { } virus)return;
        if(state.PendingBackupDevice is not null || virus.RoomIndex is <0 or >1 || virus.Page is <0 or >4 ||
            virus.Protected!=state.BackupDeviceOwned || virus.Protected && virus.Page>2 || !virus.Protected && virus.Page==1 ||
            virus.Staff is null || virus.StaffIndex<0 || virus.StaffIndex>virus.Staff.Count || virus.Reaction is <0 or >6 ||
            virus.Page==2 && (virus.Reaction is null || virus.StaffIndex==virus.Staff.Count) ||
            virus.Page!=2 && virus.Reaction is not null || !state.DevelopmentProjects.Any(p=>p.RoomIndex==virus.RoomIndex && p.OriginalRoomState==1) ||
            virus.Staff.Any(e=>e is null || string.IsNullOrWhiteSpace(e.Name) || e.PortraitId is not (null or >=1 and <=10) ||
                !state.Employees.Any(actor=>actor.Office?.SlotIndex==e.ActorSlot && actor.SourceIndex==e.SourceIndex)) ||
            virus.Staff.Select(e=>e.ActorSlot).Distinct().Count()!=virus.Staff.Count)
            throw new InvalidDataException("Invalid virus-event continuation.");
    }
}
