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
        if(exhibition.Visitors is not null){ExhibitionScene(state,exhibition);return;}
        ClearContent();var canvas=NativeWindow("ExhibitionSelectionWindow",Tr("GAME_EXHIBITION"),new(350,315));
        var instruction=NativeText(canvas,"ExhibitionSelection",Tr("EXHIBITION_SELECTION"),new(4,2,342,30),10);instruction.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var selected=new HashSet<int>();
        var total=NativeText(canvas,"ExhibitionFee",$"{Tr("EXHIBITION_FEE")}: ¥0",new(4,266,342,18));
        _status=NativeText(canvas,"ExhibitionSelectionStatus","",new(4,285,178,26),10);_status.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var developing=CreateTable("ExhibitionDevelopingList",["EXHIBITION_DEVELOPING","EXHIBITION_PROGRESS"],[242,78]);
        NativePlace(canvas,developing,new(4,38,342,77));developing.SelectMode=Tree.SelectModeEnum.Multi;
        var released=CreateTable("ExhibitionReleasedList",["EXHIBITION_RELEASED","PRODUCT_RELEASE_DATE"],[242,78]);
        NativePlace(canvas,released,new(4,123,342,139));released.SelectMode=Tree.SelectModeEnum.Multi;
        foreach(int id in exhibition.EligibleProducts)
        {
            var table=id<0?developing:released;var row=table.CreateItem(table.GetRoot());row.SetMetadata(0,id);
            string title=id<0?Tr("EXHIBITION_ROOM").Replace("{room}",(-id).ToString()):state.ReleasedGames.Single(p=>p.ProductIndex==id).Title;
            row.SetText(0,title);
            if(id<0)row.SetText(1,OfficeRoomPresentationRules.Percentage(state.DevelopmentProjects.Single(p=>p.RoomIndex==-id-1))+"%");
            else {var date=state.ReleasedGames.Single(p=>p.ProductIndex==id).ReleasedOn;row.SetText(1,date is { } d?$"{d.Year}/{d.Month}/{d.Week}":"?");}
            row.SetTextAlignment(1,HorizontalAlignment.Right);
        }
        void Select(TreeItem item,long column,bool enabled)
        {
            int id=item.GetMetadata(0).AsInt32();
            if(enabled && !selected.Contains(id) && selected.Count==5)
            {item.Deselect((int)column);_status.Text=Tr("EXHIBITION_TooManyProducts");return;}
            if(enabled)selected.Add(id);else selected.Remove(id);
            total.Text=$"{Tr("EXHIBITION_FEE")}: ¥{DisplayNumbers.Format(selected.Count*ExhibitionService.ProductFee)}";
        }
        developing.MultiSelected+=Select;released.MultiSelected+=Select;
        NativeAction(canvas,"ParticipateExhibition","EXHIBITION_PARTICIPATE",new(186,283,78,27),()=>
        {
            var result=ExhibitionService.Participate(state,selected,_session.Random);
            if(result==ExhibitionResult.Success)ShowOfficeReconstruction(state);else _status.Text=Tr($"EXHIBITION_{result}");
        });
        NativeAction(canvas,"SkipExhibition","NATIVE_SKIP",new(270,283,76,27),()=>{ExhibitionService.Close(state);ShowOfficeReconstruction(state);});
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
        ClearContent();var canvas=NativeWindow("GraduateRecruitmentWindow",Tr("GRADUATE_RECRUITMENT"),new(382,239));
        NativeText(canvas,"GraduatePosition",string.Format(DisplayNumbers.Provider,Tr("GRADUATE_POSITION"),graduates.CandidateIndex+1,graduates.Candidates.Count),new(4,3,374,23),10);
        EmployeePreview(canvas,employee,"Graduate",new(4,31,374,163));
        _status=NativeText(canvas,"GraduateStatus","",new(4,197,374,14),10);
        NativeAction(canvas,"HireGraduate","HIRE_CONFIRM",new(156,211,106,25),()=>{var result=GraduateService.Hire(state,_session.Random);if(result==HireResult.Success)ShowOfficeReconstruction(state);else _status.Text=Tr("HIRE_RESULT_"+result);});
        NativeAction(canvas,"DeclineGraduate","GRADUATE_DECLINE",new(268,211,110,25),()=>{GraduateService.Decline(state);ShowOfficeReconstruction(state);});
    }
    private void AnnualButton(string name,string key,Action action)
    {var button=new Button {Name=name,Text=Tr(key)};_content.AddChild(button);button.Pressed+=action;}
}
