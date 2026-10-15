using Godot;
using OpenGDS2.Domain;
using OpenGDS2.Core;
using OpenGDS2.Persistence;
namespace OpenGDS2.UI;
public partial class Main
{
    private void ShowSaveSlots(GameState? state,bool load)
    {
        var canvas=ManagementCanvas(load?"LoadSlotWindow":"SaveSlotWindow",Tr(load?"LOAD_SAVE_SLOTS":"SAVE_SAVE_SLOTS"),new(434,364));
        string directory=ProjectSettings.GlobalizePath("user://saves");var entries=SaveSlotStore.Read(directory,!load);
        var table=ManagementTable(canvas,"SaveSlotList",["SAVE_SLOT_NUMBER","COMPANY_NAME","DATE","FUNDS"],[30,143,78,112],new(4,4,426,145));table.SetColumnExpand(0,false);table.SetColumnExpand(1,true);
        foreach(var entry in entries)
        {
            var row=table.CreateItem(table.GetRoot());row.SetMetadata(0,entry.Slot);row.SetText(0,entry.Slot.ToString());row.SetText(1,entry.State?.Company.Name??Tr(entry.Error is null?"SAVE_SLOT_EMPTY":"SAVE_SLOT_DAMAGED"));
            row.SetText(2,entry.State is { } snapshot?$"{snapshot.Year}/{snapshot.Month}/{snapshot.Week}":"—");row.SetText(3,entry.State?.Money?.ToDisplayNumber()??"—");row.SetTextAlignment(3,HorizontalAlignment.Right);row.SetTooltipText(1,row.GetText(1));row.SetTooltipText(3,row.GetText(3));
        }
        var image=NativePlace(canvas,Picture("SaveSlotOfficePreview",null,new(233,145)),new(4,155,233,145));
        var detail=NativeText(canvas,"SaveSlotDetails","",new(245,155,185,145),10);detail.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var status=NativeText(canvas,"SaveSlotStatus","",new(4,305,426,25),10);status.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        int selected=-1;
        var confirm=NativeAction(canvas,"ConfirmSaveSlot",load?"LOAD_GAME":"SAVE_GAME",new(218,334,103,25),()=>{},false);
        var backup=NativeAction(canvas,"LoadSlotBackup","LOAD_PREVIOUS_SAVE",new(4,334,208,25),()=>{},false);backup.Visible=load;
        NativeAction(canvas,"CancelSaveSlots","CANCEL",new(327,334,103,25),()=>{if(state is null)ShowTitleScreen();else ShowOfficeReconstruction(state);});
        table.ItemSelected+=()=>
        {
            if(table.GetSelected() is not { } row)return;int slot=row.GetMetadata(0).AsInt32();selected=entries.ToList().FindIndex(x=>x.Slot==slot);if(selected<0)return;
            var entry=entries[selected];confirm.Disabled=load?entry.State is null:state is null;backup.Disabled=!entry.BackupAvailable;
            image.Texture=entry.State is { } snapshot?OriginalAssetStore.LoadOpeningOffice(snapshot):null;
            detail.Text=entry.State is { } saved?$"{saved.Company.Name}\n{saved.Company.PresidentName}\n{saved.Year}/{saved.Month}/{saved.Week}\n{Tr("FUNDS")}: ¥{DisplayNumbers.Format(saved.Money)}\n{Tr("STAFF")}: {saved.Employees.Count}":Tr(entry.Error is null?"SAVE_SLOT_EMPTY":"SAVE_SLOT_DAMAGED");detail.TooltipText=detail.Text;
            status.Text=entry.Error is null?"":Tr("SAVE_SLOT_DAMAGED");
        };
        void Load(bool previous)
        {
            if(selected<0)return;try{var entry=entries[selected];var snapshot=previous?SaveStore.LoadBackup(entry.Path):SaveStore.Load(entry.Path);_activeSaveSlot=entry.Slot;_session.Restore(snapshot);ShowOfficeReconstruction(snapshot);}catch(Exception ex){status.Text=Tr("LOAD_FAILED")+": "+ex.Message;}
        }
        confirm.Pressed+=()=>{if(selected<0||confirm.Disabled)return;if(load){Load(false);return;}try{SaveStore.Save(entries[selected].Path,state!);_activeSaveSlot=entries[selected].Slot;ShowOfficeReconstruction(state!);}catch(Exception ex){status.Text=Tr("SAVE_FAILED")+": "+ex.Message;}};
        backup.Pressed+=()=>Load(true);
    }
}
