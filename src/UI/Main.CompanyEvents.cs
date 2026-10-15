using Godot;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;

public partial class Main
{
    private void BeginBackupDevice(GameState state)
    {if(ReferenceEquals(_session.Current,state) && BackupDeviceService.Begin(state))ShowPendingCompanyEvent(state);}
    private bool ShowPendingCompanyEvent(GameState state)
    {
        if(ShowPendingMagazineInterview(state))return true;
        if(ShowPendingPoaching(state))return true;
        if(state.PendingBackupDevice is { } purchase)
        {
            if(purchase.Page==1)
            {
                var canvas=ManagementCanvas("BackupDeviceConfirmation",Tr("BUY_BACKUP_DEVICE"),new(330,122));
                NativeText(canvas,"BackupDeviceQuestion",string.Format(DisplayNumbers.Provider,Tr("BACKUP_DEVICE_CONFIRM"),$"¥{DisplayNumbers.Format(BackupDeviceService.Price)}"),new(8,8,314,67),11).AutowrapMode=TextServer.AutowrapMode.WordSmart;
                NativeAction(canvas,"ConfirmBackupDevice","CONFIRM",new(128,88,94,25),()=>
                {
                    if(!ReferenceEquals(state.PendingBackupDevice,purchase))return;
                    BackupDeviceService.Confirm(state);ShowPendingCompanyEvent(state);
                });
                NativeAction(canvas,"CancelBackupDevice","CANCEL",new(228,88,94,25),()=>
                {BackupDeviceService.Cancel(state);ShowOfficeReconstruction(state);});
                return true;
            }
            string key=purchase.Page==0?"BACKUP_DEVICE_EXPLANATION":"BACKUP_DEVICE_RESULT_"+purchase.Result;
            ShowConversation(Tr("BUY_BACKUP_DEVICE"),[new(SecretaryCaption(state.SecretarySourceIndex),purchase.Result==BackupPurchaseResult.StateUnknown?Tr(key):DialogueText(key))],()=>
            {
                if(!ReferenceEquals(state.PendingBackupDevice,purchase))return;
                BackupDeviceService.Acknowledge(state);
                if(!ShowPendingCompanyEvent(state))ShowOfficeReconstruction(state);
            });return true;
        }
        if(state.PendingVirusEvent is not { } virus)return false;
        string text=virus.Page switch
        {
            0=>DialogueText("VIRUS_INTRO").Replace("{room}",(virus.RoomIndex+1).ToString()),
            1=>DialogueText("VIRUS_PROTECTED"),
            2=>DialogueText($"VIRUS_{(virus.Protected?"PROTECTED":"LOSS")}_REACTION_{virus.Reaction}"),
            3=>DialogueText("VIRUS_STRESS").Replace("{room}",(virus.RoomIndex+1).ToString()),
            _=>DialogueText("VIRUS_BUY_BACKUP")
        };
        var actor=virus.Page==2?virus.Staff[virus.StaffIndex]:null;
        string heading=actor is null?Tr("VIRUS_EVENT"):EmployeeName(new EmployeeState {Name=actor.Name,SourceIndex=actor.SourceIndex});
        ShowConversation(heading,[new(actor?.Name??SecretaryCaption(state.SecretarySourceIndex),text,actor?.PortraitId)],()=>
        {
            if(!ReferenceEquals(state.PendingVirusEvent,virus))return;
            CompanyEventService.AcknowledgeVirus(state,_session.Random);
            if(!ShowPendingCompanyEvent(state))ShowOfficeReconstruction(state);
        });return true;
    }
}
