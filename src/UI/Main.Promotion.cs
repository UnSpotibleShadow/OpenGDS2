using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;
public partial class Main
{
    private void ShowPromotionSelector(GameState state,DevelopmentProjectState project)
    {
        var allowed=PromotionService.Check(state,project,0);
        ClearContent();CompactManagementWindow("PromotionSelectorWindow",new(470,390),450);
        AddHeading(Tr("PAID_PROMOTION"),22);
        if(allowed is not (PromotionResult.Success or PromotionResult.InsufficientFunds))
        {AddHeading(Tr($"PROMOTION_RESULT_{allowed}"),16);AddAction("CANCEL",()=>ShowProject(state,project));return;}
        int selected=-1;
        var table=new ItemList {Name="PromotionMedia",CustomMinimumSize=new(420,200)};
        for(int i=0;i<PromotionService.Media.Count;i++)table.AddItem($"{Tr($"PROMOTION_MEDIA_{i}")} — {PromotionService.Media[i].Fee:N0}");
        _content.AddChild(table);
        var message=new Label {Name="PromotionStatus",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(message);
        var confirm=new Button {Name="ConfirmPromotion",Text=Tr("CONFIRM"),Disabled=true};_content.AddChild(confirm);
        table.ItemSelected+=i=>{selected=(int)i;confirm.Disabled=false;message.Text="";};
        confirm.Pressed+=()=>
        {
            if(selected<0)return;
            var result=PromotionService.Start(state,project,selected);
            if(result==PromotionResult.Success)ShowPendingPromotion(state);
            else message.Text=Tr($"PROMOTION_RESULT_{result}");
        };
        AddAction("CANCEL",()=>ShowProject(state,project));
    }
    private bool ShowPendingPromotion(GameState state)
    {
        if(state.PendingPromotion is not { } meeting)return false;
        ClearContent();CompactManagementWindow("PromotionMeetingWindow",new(420,380),400);
        AddHeading(Tr("PAID_PROMOTION"),22);
        _content.AddChild(Picture("PromotionMeetingPicture",OriginalAssetStore.LoadTexture("Graphic/Embedded/PlanningMeeting.bmp"),new(256,160)));
        var report=new Label {Name="PromotionMeetingResult",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(report);
        var close=new Button {Name="AcknowledgePromotion",Text=Tr("CONTINUE")};_content.AddChild(close);
        close.Pressed+=()=>
        {
            if(!PromotionService.Acknowledge(state))return;
            ShowProject(state,state.DevelopmentProjects.Single(p=>p.RoomIndex==meeting.RoomIndex));
        };
        void Refresh()
        {
            close.Disabled=meeting.Callbacks<27;
            report.Text=meeting.Gain is double gain ? string.Format(Tr("PROMOTION_COMPLETED"),gain,PromotionService.Media[meeting.MediaIndex].Fee) : Tr("PROMOTION_IN_PROGRESS");
        }
        Refresh();if(meeting.Callbacks<27)AdvancePromotionMeeting(state,meeting,_dialog!,Refresh);
        return true;
    }
    private async void AdvancePromotionMeeting(GameState state,PromotionMeetingState meeting,DesktopWindow window,Action refresh)
    {
        while(meeting.Callbacks<27)
        {
            await ToSignal(GetTree().CreateTimer(.025),SceneTreeTimer.SignalName.Timeout);
            if(!ReferenceEquals(_session.Current,state) || !ReferenceEquals(state.PendingPromotion,meeting) ||
                !ReferenceEquals(_dialog,window) || !GodotObject.IsInstanceValid(window) || !window.IsInsideTree())return;
            PromotionService.Advance(state,_session.Random);refresh();
        }
    }
}
