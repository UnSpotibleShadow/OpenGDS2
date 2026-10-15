using Godot;
using OpenGDS2.Domain;
using OpenGDS2.Core;
namespace OpenGDS2.UI;
public partial class Main
{
    private bool ShowPendingProgrammingChallenge(GameState state)
    {
        if(state.PendingProgrammingChallenge is not { } scene)return false;
        var cursor=state.OfficeActorContinuation!;
        var actor=state.Employees.Single(e=>e.SourceIndex==cursor.EmployeeSourceIndex);
        if(scene.IntroPage<2)
        {
            string text=scene.IntroPage==0 ? string.Format(DisplayNumbers.Provider,Tr("PROGRAMMING_INTRO"),EmployeeName(actor)) :
                string.Format(DisplayNumbers.Provider,Tr("PROGRAMMING_INSPIRATION"),actor.Inspiration);
            ShowConversation(SecretaryCaption(state.SecretarySourceIndex),[new(SecretaryCaption(state.SecretarySourceIndex),text)],()=>
            {if(ReferenceEquals(_session.Current,state)){ProgrammingChallengeService.AcknowledgeIntro(state);ShowPendingProgrammingChallenge(state);}});
            return true;
        }
        if(scene.Mode==4)
        {
            string text=scene.ResultPage switch
            {
                0=>string.Format(DisplayNumbers.Provider,Tr($"PROGRAMMING_RESULT_{scene.Outcome}"),EmployeeName(actor)),
                1=>scene.Outcome==0 ? Tr("PROGRAMMING_FAILURE_EFFECT") : string.Format(DisplayNumbers.Provider,Tr("PROGRAMMING_QUALITY_EFFECT"),
                    Tr(new[]{"STAT_FUN","STAT_NOVELTY","STAT_GRAPHICS","STAT_MUSIC"}[scene.QualityIndex!.Value]),scene.Outcome==1?40:100),
                _=>Tr($"PROGRAMMING_COMMENT_{(scene.Outcome==0?"FAILURE":"SUCCESS")}_{scene.CommentVariant}")
            };
            string speaker=scene.ResultPage==2?EmployeeName(actor):SecretaryCaption(state.SecretarySourceIndex);
            ShowConversation(Tr("PROGRAMMING_CHALLENGE"),[new(speaker,text,scene.ResultPage==2?actor.PortraitId:null)],()=>
            {
                if(!ReferenceEquals(_session.Current,state))return;
                ProgrammingChallengeService.AcknowledgeResult(state);
                if(!ShowPendingProgrammingChallenge(state))ShowOfficeReconstruction(state);
            });return true;
        }
        ClearContent();var canvas=NativeWindow("ProgrammingChallengeWindow",Tr("PROGRAMMING_CHALLENGE"),new(294,207));
        NativePlace(canvas,Picture("ProgrammingActorPortrait",OriginalAssetStore.LoadEmployeePortrait(actor.PortraitId),new(64,64)),new(216,5,64,64));
        NativeText(canvas,"ProgrammingActorName",EmployeeName(actor),new(124,74,166,28),10).AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var slots=new Label[10];
        for(int i=0;i<10;i++)slots[i]=NativeText(canvas,$"ProgrammingSlot{i}","",new(5,5+i*18,113,18),11);
        var command=NativeAction(canvas,"ProgrammingCommand","PROGRAMMING_START",new(124,174,166,27),()=>{});
        command.Pressed+=()=>{ProgrammingChallengeService.Command(state,_session.Random);Refresh();};
        void Refresh()
        {
            for(int i=0;i<10;i++)
            {
                var probe=new ProgrammingChallengeState {Inspiration=scene.Inspiration,Index=i};int result=ProgrammingChallengeService.Display(probe);
                slots[i].Text=(i==scene.Index?"▶ ":"   ")+Tr($"PROGRAMMING_REEL_{result}");
                slots[i].AddThemeColorOverride("font_color",result==0?new(.7f,0,0):result==1?new(0,.45f,0):new(0,.2f,.6f));
            }
            command.Text=Tr(scene.Mode==0?"PROGRAMMING_START":"PROGRAMMING_STOP");command.Disabled=scene.Mode>=2;
        }
        Refresh();AdvanceProgrammingScene(state,scene,_dialog!,Refresh);return true;
    }
    private async void AdvanceProgrammingScene(GameState state,ProgrammingChallengeState scene,DesktopWindow window,Action refresh)
    {
        while(ReferenceEquals(state.PendingProgrammingChallenge,scene))
        {
            await ToSignal(GetTree().CreateTimer(.025),SceneTreeTimer.SignalName.Timeout);
            if(!ReferenceEquals(_session.Current,state)||!ReferenceEquals(_dialog,window)||!GodotObject.IsInstanceValid(window)||!window.IsInsideTree())return;
            ProgrammingChallengeService.Advance(state,_session.Random);
            if(scene.Mode==4){ShowPendingProgrammingChallenge(state);return;}
            refresh();
        }
    }
}
