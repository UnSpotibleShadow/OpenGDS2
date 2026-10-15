namespace OpenGDS2.Domain;

public sealed class MagazineInterviewState
{
    public int RoomIndex {get;set;}
    // 0 secretary introduction, 1–4 reporter, 5 secretary result already applied.
    public int Page {get;set;}
    public int? ReporterNameIndex {get;set;}
    public int? PortraitId {get;set;}
    public double? RecognitionBefore {get;set;}
    public double? Gain {get;set;}
}

/// <summary>009E3830 +17F0..1A39; meeting mode7, 009EFA40 +2083..22CE.</summary>
public static class MagazineInterviewService
{
    public static readonly IReadOnlyList<string> ImportKeys=Enumerable.Range(0,5).Select(i=>"INTERVIEW_REPORTER_"+i)
        .Concat(Enumerable.Range(0,4).Select(i=>"INTERVIEW_PAGE_"+i))
        .Concat(["INTERVIEW_INTRO","INTERVIEW_RESULT","INTERVIEW_SPEAKER"]).ToArray();
    public static bool Periodic(GameState state,DevelopmentProjectState project,Random random)
    {
        // Null is unrecorded pre-feature history. It cannot establish eligibility.
        if(project.OriginalRoomState!=1||project.InterviewCompleted is null)return true;
        float draw=random.NextSingle();
        if(!float.IsFinite(draw)||draw<0||draw>=1)throw new ArgumentOutOfRangeException(nameof(random));
        double chance=Math.Floor((double)draw*(1500-project.Recognition*15)*OfficeStaffRules.TimingFactor);
        // The random expression precedes all the remaining native Boolean tests.
        if(project.Recognition<=15||chance!=2||project.OriginalStage<2||project.InterviewCompleted==true)return true;
        if(CompanyEventService.HasPending(state)||!state.DevelopmentProjects.Contains(project))throw new InvalidOperationException("A company event is pending.");
        project.InterviewCompleted=true;
        state.PendingMagazineInterview=new() {RoomIndex=project.RoomIndex};return false;
    }
    public static bool Acknowledge(GameState state,Random random)
    {
        if(state.PendingMagazineInterview is not { } scene)return false;
        var project=state.DevelopmentProjects.Single(p=>p.RoomIndex==scene.RoomIndex);
        if(scene.Page==0)
        {
            // Native order is Int(Rnd)*5+1, not Int(Rnd*5)+1: portrait1,
            // still consuming the first draw. The following name draw spans five.
            int portrait=CompanyEventService.Draw(random,1)*5+1;
            int name=CompanyEventService.Draw(random,5);
            scene.PortraitId=portrait;scene.ReporterNameIndex=name;
        }
        if(scene.Page==4)
        {
            double gain=Math.Min(20,100-project.Recognition);
            scene.RecognitionBefore=project.Recognition;scene.Gain=gain;project.Recognition+=gain;
        }
        if(scene.Page==5)state.PendingMagazineInterview=null;else scene.Page++;
        return true;
    }
    public static void Validate(GameState state)
    {
        if(state.PendingMagazineInterview is not { } scene)return;
        var project=state.DevelopmentProjects.SingleOrDefault(p=>p.RoomIndex==scene.RoomIndex&&p.OriginalRoomState==1);
        if(scene.RoomIndex is <0 or >1||scene.Page is <0 or >5||project?.InterviewCompleted!=true||project.OriginalStage<2||
            scene.Page==0&&(scene.ReporterNameIndex is not null||scene.PortraitId is not null)||
            scene.Page>0&&(scene.ReporterNameIndex is not (>=0 and <5)||scene.PortraitId!=1)||
            scene.Page<5&&(scene.Gain is not null||scene.RecognitionBefore is not null)||
            scene.Page==5&&(scene.Gain is not double gain||!double.IsFinite(gain)||scene.RecognitionBefore is not double before||
                !double.IsFinite(before)||gain!=Math.Min(20,100-before)||before+gain!=project.Recognition))
            throw new InvalidDataException("Invalid magazine-interview continuation.");
    }
}
