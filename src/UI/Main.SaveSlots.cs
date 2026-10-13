using Godot;
using OpenGDS2.Domain;
using OpenGDS2.Core;
using OpenGDS2.Persistence;
namespace OpenGDS2.UI;
public partial class Main
{
    private void ShowSaveSlots(GameState? state,bool load)
    {
        ClearContent();CompactManagementWindow(load?"LoadSlotWindow":"SaveSlotWindow",new(660,510),640);
        AddHeading(Tr(load?"LOAD_SAVE_SLOTS":"SAVE_SAVE_SLOTS"),24);
        string directory=ProjectSettings.GlobalizePath("user://saves");var entries=SaveSlotStore.Read(directory,!load);
        var list=new ItemList {Name="SaveSlotList",CustomMinimumSize=new(620,150)};_content.AddChild(list);
        foreach(var item in entries)list.AddItem($"{item.Slot}: "+(item.State?.Company.Name ?? Tr(item.Error is null?"SAVE_SLOT_EMPTY":"SAVE_SLOT_DAMAGED")));
        var preview=new HBoxContainer();_content.AddChild(preview);
        var image=Picture("SaveSlotOfficePreview",null,new(233,145));preview.AddChild(image);
        var detail=new Label {Name="SaveSlotDetails",AutowrapMode=TextServer.AutowrapMode.WordSmart,SizeFlagsHorizontal=SizeFlags.ExpandFill};preview.AddChild(detail);
        var status=new Label {Name="SaveSlotStatus",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(status);
        int selected=-1;var confirm=new Button {Name="ConfirmSaveSlot",Text=Tr(load?"LOAD_GAME":"SAVE_GAME"),Disabled=true};_content.AddChild(confirm);
        var backup=new Button {Name="LoadSlotBackup",Text=Tr("LOAD_PREVIOUS_SAVE"),Visible=load,Disabled=true};_content.AddChild(backup);
        list.ItemSelected+=index=>
        {
            selected=(int)index;var entry=entries[selected];confirm.Disabled=load?entry.State is null:state is null;
            backup.Disabled=!entry.BackupAvailable;
            image.Texture=entry.State is { } snapshot?OriginalAssetStore.LoadOpeningOffice(snapshot):null;
            detail.Text=entry.State is { } saved?$"{saved.Company.Name}\n{saved.Company.PresidentName}\n{saved.Year}/{saved.Month}/{saved.Week}\n{Tr("FUNDS")}: ¥{saved.Money:N0}\n{Tr("STAFF")}: {saved.Employees.Count}":Tr(entry.Error is null?"SAVE_SLOT_EMPTY":"SAVE_SLOT_DAMAGED");
            status.Text=entry.Error is null?"":Tr("SAVE_SLOT_DAMAGED");
        };
        void Load(bool previous)
        {
            if(selected<0)return;
            try
            {
                var entry=entries[selected];var snapshot=previous?SaveStore.LoadBackup(entry.Path):SaveStore.Load(entry.Path);
                _activeSaveSlot=entry.Slot;_session.Restore(snapshot);ShowOfficeReconstruction(snapshot);
            }
            catch(Exception ex){status.Text=$"{Tr("LOAD_FAILED")}: {ex.Message}";}
        }
        confirm.Pressed+=()=>
        {
            if(selected<0||confirm.Disabled)return;
            if(load){Load(false);return;}
            try{SaveStore.Save(entries[selected].Path,state!);_activeSaveSlot=entries[selected].Slot;ShowOfficeReconstruction(state!);}
            catch(Exception ex){status.Text=$"{Tr("SAVE_FAILED")}: {ex.Message}";}
        };
        backup.Pressed+=()=>Load(true);
        AddAction("CANCEL",()=>{if(state is null)ShowTitleScreen();else ShowOfficeReconstruction(state);});
    }
}
