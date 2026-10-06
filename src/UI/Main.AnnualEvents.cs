using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private bool ShowAnnualCalendarEvent(GameState state,CalendarTransaction pending)
    {
        switch (pending.PendingEvent)
        {
            case CalendarEvent.BeginnerEnding:
                AddHeading(Tr("BEGINNER_ENDING_TITLE"),24);
                AnnualMessage("BeginnerEnding",Tr("BEGINNER_ENDING_MESSAGE"));
                AddAction("BACK_TO_TITLE",ShowTitleScreen);
                return true;
            case CalendarEvent.Exhibition:
                ShowExhibition(state,pending.Exhibition!);return true;
            case CalendarEvent.Awards:
                ShowAwards(state,pending.Awards!);return true;
            case CalendarEvent.Graduates:
                ShowGraduate(state,pending.Graduates!);return true;
            default:return false;
        }
    }
    private void AnnualMessage(string name,string text)=>_content.AddChild(new Label {Name=name,Text=text,
        AutowrapMode=TextServer.AutowrapMode.WordSmart,CustomMinimumSize=new Vector2(540,0)});
    private void ShowExhibition(GameState state,ExhibitionState exhibition)
    {
        AddHeading(Tr("GAME_EXHIBITION"),24);
        if (exhibition.Visitors is int visitors)
        {
            AnnualMessage("ExhibitionResult",string.Format(Tr("EXHIBITION_RESULT"),visitors,exhibition.ReportedAwarenessGain));
            AnnualButton("CloseExhibition","CONTINUE",()=>{ExhibitionService.Close(state);ShowOfficeReconstruction(state);});
            return;
        }
        AnnualMessage("ExhibitionSelection",Tr("EXHIBITION_SELECTION"));
        var boxes=new Dictionary<int,CheckBox>();
        var total=new Label {Name="ExhibitionFee",Text=$"{Tr("EXHIBITION_FEE")}: ¥0"};
        foreach (int id in exhibition.EligibleProducts)
        {
            string title=id<0 ? state.DevelopmentProjects.Single(p=>p.RoomIndex==-id-1).Title : state.ReleasedGames.Single(p=>p.ProductIndex==id).Title;
            var box=new CheckBox {Name=$"ExhibitProduct{id}",Text=title};boxes.Add(id,box);_content.AddChild(box);
            box.Toggled+=selected=>
            {
                if (selected && boxes.Values.Count(b=>b.ButtonPressed)>5)
                {box.SetPressedNoSignal(false);_status.Text=Tr("EXHIBITION_TooManyProducts");}
                total.Text=$"{Tr("EXHIBITION_FEE")}: ¥{boxes.Values.Count(b=>b.ButtonPressed)*ExhibitionService.ProductFee:N0}";
            };
        }
        _content.AddChild(total);
        AnnualButton("ParticipateExhibition","EXHIBITION_PARTICIPATE",()=>
        {
            var result=ExhibitionService.Participate(state,boxes.Where(b=>b.Value.ButtonPressed).Select(b=>b.Key).ToHashSet(),Random.Shared);
            if (result==ExhibitionResult.Success) ShowOfficeReconstruction(state);
            else _status.Text=Tr($"EXHIBITION_{result}");
        });
        AnnualButton("SkipExhibition","EXHIBITION_SKIP",()=>{ExhibitionService.Close(state);ShowOfficeReconstruction(state);});
    }
    private void ShowAwards(GameState state,AwardCeremonyState awards)
    {
        AddHeading(Tr("ANNUAL_GAME_AWARDS"),24);
        var category=AwardService.Order[awards.CategoryIndex];
        AddHeading(Tr($"AWARD_{category}"),22);
        AnnualMessage("AwardEntries",string.Format(Tr("AWARD_ENTRIES"),awards.Entries.Count));
        if (awards.ResultReady==true)
        {
            if (awards.WinningProduct is int id)
            {
                string title=state.ReleasedGames.Single(p=>p.ProductIndex==id).Title;
                AnnualMessage("AwardResult",string.Format(Tr("AWARD_OWN_RESULT"),title,AwardService.Prize(category)));
            }
            else if (awards.WinningRival is int rival && state.Competition is CompetitionState competition)
            {
                var product=competition.Products[rival];string company=_activeCatalog?.RivalCompanies.ElementAtOrDefault(product.Owner) ?? Tr("NOT_YET_DECODED");
                AnnualMessage("AwardResult",string.Format(Tr("AWARD_RIVAL_RESULT"),product.Title,company));
            }
            else AnnualMessage("AwardResult",Tr("AWARD_OTHER_COMPANY"));
            AnnualButton("ContinueAwards","CONTINUE",()=>{AwardService.Continue(state);ShowOfficeReconstruction(state);});
        }
        else AnnualButton("RevealAward","AWARD_REVEAL",()=>
        {
            if (AwardService.Reveal(state,Random.Shared))
            {
                ShowOfficeReconstruction(state);
                if (awards.AnimationProducts is not null) _=AnimateAwardReveal(state,awards);
            }
            else _status.Text=Tr("HIRE_RESULT_MoneyUnknown");
        });
    }
    private async System.Threading.Tasks.Task AnimateAwardReveal(GameState state,AwardCeremonyState awards)
    {
        var reel=_content.FindChild("AwardEntries",true,false) as Label;
        var result=_content.FindChild("AwardResult",true,false) as Label;
        var next=_content.FindChild("ContinueAwards",true,false) as Button;
        if (reel is null || result is null || next is null || state.Competition is null) return;
        result.Visible=false;next.Disabled=true;
        foreach (int id in awards.AnimationProducts!)
        {
            if (!GodotObject.IsInstanceValid(reel) || !reel.IsInsideTree()) return;
            var product=state.Competition.Products[id];
            reel.Text=string.Format(Tr("AWARD_REEL_ENTRY"),product.Title,_activeCatalog?.RivalCompanies.ElementAtOrDefault(product.Owner) ?? Tr("NOT_YET_DECODED"));
            // Modern display pacing. The thirteen catalog selections and final
            // winner are already persisted; loading never repeats the lottery.
            await ToSignal(GetTree().CreateTimer(.08),SceneTreeTimer.SignalName.Timeout);
        }
        if (!GodotObject.IsInstanceValid(result) || !result.IsInsideTree()) return;
        reel.Text=string.Format(Tr("AWARD_ENTRIES"),awards.Entries.Count);result.Visible=true;next.Disabled=false;
    }
    private void ShowGraduate(GameState state,GraduateEventState graduates)
    {
        var employee=state.EmployeeSources.Single(p=>p.SourceIndex==graduates.Candidates[graduates.CandidateIndex]).Employee;
        if (graduates.GreetingVariant is int variant)
        {
            ShowConversation(DialogueText("CORE_EMPLOYEE"),[new(DialogueText("GRADUATE_SPEAKER").Replace("{name}",EmployeeName(employee)),
                DialogueText($"GRADUATE_GREETING_{variant}"),employee.PortraitId)],()=>
            {
                if (!ReferenceEquals(state.Calendar?.Pending?.Graduates,graduates)) return;
                GraduateService.AcknowledgeGreeting(state);ShowOfficeReconstruction(state);
            });
            return;
        }
        AddHeading(Tr("GRADUATE_RECRUITMENT"),24);
        AnnualMessage("GraduatePosition",string.Format(Tr("GRADUATE_POSITION"),graduates.CandidateIndex+1,graduates.Candidates.Count));
        AddEmployeeCard(employee);
        AnnualButton("HireGraduate","HIRE_CONFIRM",()=>
        {
            var result=GraduateService.Hire(state,Random.Shared);
            if (result==HireResult.Success) ShowOfficeReconstruction(state);
            else _status.Text=Tr($"HIRE_RESULT_{result}");
        });
        AnnualButton("DeclineGraduate","GRADUATE_DECLINE",()=>{GraduateService.Decline(state);ShowOfficeReconstruction(state);});
    }
    private void AnnualButton(string name,string key,Action action)
    {var button=new Button {Name=name,Text=Tr(key)};_content.AddChild(button);button.Pressed+=action;}
}
