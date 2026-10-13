namespace OpenGDS2.Domain;

public sealed class OfficeActorContinuationState
{
    public int ActorSlot {get;set;}
    public int EmployeeSourceIndex {get;set;}
    public int RoomIndex {get;set;}
}
public sealed class ProgrammingChallengeState
{
    public int Inspiration {get;set;}
    public int IntroPage {get;set;}
    public int Mode {get;set;}
    public int Index {get;set;}
    public int SlowTicks {get;set;}
    public int Remaining {get;set;}
    public int? Outcome {get;set;}
    public int? QualityIndex {get;set;}
    public int? CommentVariant {get;set;}
    public int ResultPage {get;set;}
}
/// <summary>00A0283C +50B7..5471, fGMKoudo and meeting modes 6/10/17.</summary>
public static class ProgrammingChallengeService
{
    private static readonly int[][] Reels=[ [2,0,1,0,1,1,2,0,1,0],[1,0,1,0,2,1,0,0,1,0],
        [0,0,1,0,2,0,1,0,0,1],[0,1,0,0,1,0,0,1,0,0],[0,0,1,0,0,0,1,0,0,0] ];
    public static int Band(int inspiration)=>inspiration>=80?0:inspiration>=60?1:inspiration>=40?2:inspiration>=20?3:4;
    public static int Display(ProgrammingChallengeState scene)=>Reels[Band(scene.Inspiration)][scene.Index];
    public static bool HasPending(GameState state)=>state.PendingProgrammingChallenge is not null || state.OfficeActorContinuation is not null;
    private static int Draw(Random random,int span)
    {float v=random.NextSingle();if(!float.IsFinite(v)||v<0||v>=1)throw new ArgumentOutOfRangeException(nameof(random));return (int)Math.Floor((double)v*span);}
    public static bool TryTrigger(GameState state,EmployeeState employee,DevelopmentProjectState project,Random random)
    {
        if(state.ProgrammingActorPasses is not int passes || HasPending(state) || project.OriginalRoomState!=1 || project.OriginalStage>=3 ||
            employee.Office?.SlotIndex is not int slot || employee.SourceIndex is not int source || employee.Inspiration is not int inspiration)return false;
        int next=checked(passes+1);int draw=Draw(random,20_000);state.ProgrammingActorPasses=next;
        // Native timing factor is 2. The draw is eager even below the threshold.
        if(next<=50_000 || draw!=1)return false;
        state.ProgrammingActorPasses=0;state.OfficeActorContinuation=new() {ActorSlot=slot,EmployeeSourceIndex=source,RoomIndex=project.RoomIndex};
        state.PendingProgrammingChallenge=new() {Inspiration=inspiration};return true;
    }
    public static bool AcknowledgeIntro(GameState state)
    {if(state.PendingProgrammingChallenge is not {IntroPage:<2} scene)return false;scene.IntroPage++;return true;}
    public static bool Command(GameState state,Random random)
    {
        if(state.PendingProgrammingChallenge is not {IntroPage:2} scene)return false;
        if(scene.Mode==0){scene.Mode=1;return true;}
        if(scene.Mode!=1)return false;
        scene.Remaining=5+Draw(random,2);scene.Mode=2;return true;
    }
    public static bool Advance(GameState state,Random random)
    {
        if(state.PendingProgrammingChallenge is not {IntroPage:2} scene || scene.Mode is 0 or 4)return false;
        if(scene.Mode==1)scene.Index=(scene.Index+1)%10;
        else if(scene.Mode==2)
        {
            if(++scene.SlowTicks==3){scene.SlowTicks=0;scene.Index=(scene.Index+1)%10;if(--scene.Remaining==0)scene.Mode=3;}
        }
        else if(++scene.Remaining==13)
        {
            int outcome=Display(scene);int? quality=outcome==0?null:Draw(random,4);
            int comment=Draw(random,outcome==0?4:3);
            scene.Outcome=outcome;scene.QualityIndex=quality;scene.CommentVariant=comment;scene.Mode=4;
        }
        return true;
    }
    public static bool AcknowledgeResult(GameState state)
    {
        if(state.PendingProgrammingChallenge is not {Mode:4} scene || state.OfficeActorContinuation is not { } cursor)return false;
        var project=state.DevelopmentProjects.Single(p=>p.RoomIndex==cursor.RoomIndex);
        var employee=state.Employees.Single(e=>e.SourceIndex==cursor.EmployeeSourceIndex && e.Office?.SlotIndex==cursor.ActorSlot);
        if(scene.ResultPage==1 && scene.Outcome!=0)
        {
            double gain=scene.Outcome==1?40:100;var q=project.Quality;
            project.Quality=scene.QualityIndex switch {0=>q with {Fun=q.Fun+gain},1=>q with {Novelty=q.Novelty+gain},
                2=>q with {Graphics=q.Graphics+gain},3=>q with {Music=q.Music+gain},_=>throw new InvalidDataException("Unknown challenge quality.")};
        }
        if(scene.ResultPage++<2)return true;
        if(scene.Outcome==0){int bugs=checked(project.Bugs+60);double size=project.DataSize+6;project.Bugs=bugs;project.DataSize=size;}
        else employee.Experience=EmployeeProgressionRules.AccrueExperience(employee.Experience!.Value,30,employee.Level!.Value);
        var source=state.EmployeeSources.FirstOrDefault(s=>s.SourceIndex==employee.SourceIndex);if(source is not null)source.Employee.Experience=employee.Experience;
        // Retain the cursor until the rest of the same actor pass executes.
        state.PendingProgrammingChallenge=null;return true;
    }
    public static void Validate(GameState state)
    {
        if(state.ProgrammingActorPasses<0)throw new InvalidDataException("Invalid challenge counter.");
        var cursor=state.OfficeActorContinuation;var scene=state.PendingProgrammingChallenge;
        if(cursor is null){if(scene is not null)throw new InvalidDataException("Missing office continuation.");return;}
        var actor=state.Employees.SingleOrDefault(e=>e.SourceIndex==cursor.EmployeeSourceIndex && e.Office?.SlotIndex==cursor.ActorSlot);
        var project=state.DevelopmentProjects.SingleOrDefault(p=>p.RoomIndex==cursor.RoomIndex);
        if(actor?.Office is not {WorkEngaged:true,MovementState:0,SeatedAction:>=6 and <=17} office || (office.SeatedAction-6)/6!=cursor.RoomIndex ||
            actor.Level is not (>=1 and <=10) || actor.Experience is null || project is not {OriginalRoomState:1,OriginalStage:<3} ||
            project.PendingDecision is not null || state.PendingPromotion is not null || CalendarService.HasPending(state))
            throw new InvalidDataException("Invalid office continuation target.");
        if(scene is null)return;
        if(scene.Inspiration is <0 or >99 || scene.IntroPage is <0 or >2 || scene.Mode is <0 or >4 || scene.Index is <0 or >9 ||
            scene.SlowTicks is <0 or >2 || scene.Remaining is <0 or >13 || scene.ResultPage is <0 or >2 ||
            scene.IntroPage<2 && scene.Mode!=0 || scene.Mode==2 && scene.Remaining is <1 or >6 || scene.Mode==3 && scene.Remaining>12 ||
            scene.Mode==4 && (scene.Outcome!=Display(scene) || scene.CommentVariant is not int comment || comment<0 || comment>=(scene.Outcome==0?4:3) ||
                (scene.Outcome==0 ? scene.QualityIndex is not null : scene.QualityIndex is not (>=0 and <=3))) ||
            scene.Mode!=4 && (scene.Outcome is not null || scene.QualityIndex is not null || scene.CommentVariant is not null || scene.ResultPage!=0))
            throw new InvalidDataException("Invalid programming challenge phase.");
    }
}
