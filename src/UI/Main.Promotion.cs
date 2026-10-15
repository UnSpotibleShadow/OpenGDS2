using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;
public partial class Main
{
    private DesktopWindow? _promotionSelector;
    private void ClosePromotionSelector()
    {
        if(_promotionSelector is { } window&&GodotObject.IsInstanceValid(window))
        {_dialogPositions["PromotionSelectorWindow"]=window.Position;window.GetParent()?.RemoveChild(window);window.QueueFree();}
        _promotionSelector=null;
    }
    private void ShowPromotionSelector(GameState state,DevelopmentProjectState project)
    {
        var allowed=PromotionService.Check(state,project,0);
        ClearContent();var canvas=NativeWindow("PromotionSelectorWindow",Tr("PAID_PROMOTION"),new(306,204));
        if(allowed is not (PromotionResult.Success or PromotionResult.InsufficientFunds))
        {
            NativeText(canvas,"PromotionStatus",Tr($"PROMOTION_RESULT_{allowed}"),new(4,4,298,145),11).AutowrapMode=TextServer.AutowrapMode.WordSmart;
            NativeAction(canvas,"CancelPromotion","CANCEL",new(204,172,98,25),()=>ShowProject(state,project));return;
        }
        int selected=-1;
        var table=ManagementTable(canvas,"PromotionMedia",["PROMOTION_METHOD","CONTRACT_FEE"],[166,120],new(4,4,298,140));
        StyleSelection(table,new(.72f,1,.72f));
        for(int i=0;i<PromotionService.Media.Count;i++)
        {var row=table.CreateItem(table.GetRoot());row.SetMetadata(0,i);row.SetText(0,Tr($"PROMOTION_MEDIA_{i}"));row.SetText(1,"¥"+DisplayNumbers.Format(PromotionService.Media[i].Fee));row.SetTextAlignment(1,HorizontalAlignment.Right);}
        var message=NativeText(canvas,"PromotionStatus","",new(4,146,298,22),10);
        var confirm=NativeAction(canvas,"ConfirmPromotion","CONFIRM",new(100,172,98,25),()=>{},false);
        NativeAction(canvas,"CancelPromotion","CANCEL",new(204,172,98,25),()=>ShowProject(state,project));
        table.ItemSelected+=()=>{selected=table.GetSelected().GetMetadata(0).AsInt32();confirm.Disabled=false;message.Text="";};
        confirm.Pressed+=()=>
        {
            if(selected<0)return;
            var result=PromotionService.Start(state,project,selected);
            if(result!=PromotionResult.Success){message.Text=Tr($"PROMOTION_RESULT_{result}");return;}
            ClosePromotionSelector();_promotionSelector=_dialog;
            foreach(var button in _promotionSelector!.FindChildren("*","Button",true,false).OfType<Button>())button.Disabled=true;
            table.MouseFilter=Control.MouseFilterEnum.Ignore;_promotionSelector.CloseRequested=()=>{};
            _dialog=null;_dialogPositionKey=null;ShowPendingPromotion(state);
        };
    }
    private bool ShowPendingPromotion(GameState state)
    {
        if(state.PendingPromotion is not { } meeting)return false;
        var picture=OriginalSceneArtwork.Promotion(meeting.MediaIndex);
        if(meeting.Callbacks>=27)
        {
            string text=string.Format(DisplayNumbers.Provider,Tr("PROMOTION_FINISHED_DIALOGUE"),Tr($"PROMOTION_MEDIA_{meeting.MediaIndex}"),meeting.Gain);
            ShowConversation(Tr("PAID_PROMOTION"),[new(SecretaryCaption(state.SecretarySourceIndex),text,SecretaryAdviceRules.PortraitId(state.SecretarySourceIndex))],()=>
            {
                if(!ReferenceEquals(state.PendingPromotion,meeting)||!PromotionService.Acknowledge(state))return;
                ClosePromotionSelector();ShowProject(state,state.DevelopmentProjects.Single(p=>p.RoomIndex==meeting.RoomIndex));
            },scene:picture);return true;
        }
        ClearContent();var canvas=NativeWindow("PromotionMeetingWindow",Tr($"PROMOTION_MEDIA_{meeting.MediaIndex}"),new(256,160));
        NativePlace(canvas,Picture("PromotionMeetingPicture",picture,new(256,160)),new(0,0,256,160));
        _dialog!.CloseRequested=()=>{};AdvancePromotionMeeting(state,meeting,_dialog);return true;
    }
    private async void AdvancePromotionMeeting(GameState state,PromotionMeetingState meeting,DesktopWindow window)
    {
        while(meeting.Callbacks<27)
        {
            await ToSignal(GetTree().CreateTimer(.025),SceneTreeTimer.SignalName.Timeout);
            if(!ReferenceEquals(_session.Current,state)||!ReferenceEquals(state.PendingPromotion,meeting)||
                !ReferenceEquals(_dialog,window)||!GodotObject.IsInstanceValid(window)||!window.IsInsideTree())return;
            PromotionService.Advance(state,_session.Random);
        }
        ShowPendingPromotion(state);
    }
}
