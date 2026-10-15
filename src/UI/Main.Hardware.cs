using Godot;
using OpenGDS2.Domain;
using OpenGDS2.Core;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ShowHardwarePlan(GameState state,int slot)
    {
        var catalog=_activeCatalog;if (catalog is null) {ShowFacilities(state,slot);return;}
        ClearContent();var canvas=NativeWindow("HardwarePlanWindow",Tr("OWN_HARDWARE_START"),new(370,248));
        var room=new OptionButton {Name="HardwareRoom"};for(int i=0;i<2;i++)room.AddItem($"{Tr("DEVELOPMENT_ROOM")} {i+1}",i);
        var design=new OptionButton {Name="HardwareDesign"};design.AddItem(Tr("OWN_HARDWARE_DESIGN_0"),0);design.AddItem(Tr("OWN_HARDWARE_DESIGN_1"),1);design.Select(state.OwnHardware.SelectedDesign);
        var medium=new OptionButton {Name="HardwareMedium"};medium.AddItem(Tr("OWN_HARDWARE_MEDIUM_0"),0);medium.AddItem(Tr("OWN_HARDWARE_MEDIUM_1"),1);medium.Select(state.OwnHardware.SelectedMedium);
        int y=6;foreach(var (key,field) in new[]{("DEVELOPMENT_ROOM",room),("OWN_HARDWARE_DESIGN",design),("OWN_HARDWARE_MEDIUM",medium)})
        {NativeText(canvas,key,Tr(key),new(4,y,108,26),10);field.AddThemeFontSizeOverride("font_size",10);NativePlace(canvas,field,new(114,y,252,26));y+=33;}
        var preview=NativePlace(canvas,Picture("HardwareDesignPicture",null,new(64,64)),new(4,112,64,64));
        var cost=NativeText(canvas,"HardwarePlanCost","",new(78,109,288,24),11);
        var gate=NativeText(canvas,"HardwarePlanStatus","",new(78,139,288,48),10);gate.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var accept=NativeAction(canvas,"StartHardwareDevelopment","CONFIRM",new(168,216,94,26),()=>{});
        NativeAction(canvas,"CancelHardwarePlan","CANCEL",new(270,216,96,26),()=>ShowFacilities(state,slot));
        void Refresh()
        {
            preview.Texture=HardwareDesignPicture(design.GetSelectedId(),medium.GetSelectedId());preview.SetMeta("hardware_picture_index",11+design.GetSelectedId()*2+medium.GetSelectedId());
            cost.Text=$"{Tr("PLANNING_COST")}: ¥{OwnHardwareService.Quote(design.GetSelectedId(),medium.GetSelectedId()):N0}";
            var result=OwnHardwareService.CheckStart(state,catalog,slot,room.GetSelectedId(),design.GetSelectedId(),medium.GetSelectedId());gate.Text=result==OwnHardwareResult.Success?"":Tr("OWN_HARDWARE_RESULT_"+result);accept.Disabled=result!=OwnHardwareResult.Success;
        }
        design.ItemSelected+=_=>{state.OwnHardware.SelectedDesign=design.GetSelectedId();Refresh();};medium.ItemSelected+=_=>{state.OwnHardware.SelectedMedium=medium.GetSelectedId();Refresh();};room.ItemSelected+=_=>Refresh();Refresh();
        accept.Pressed+=()=>{if(accept.Disabled)return;var result=OwnHardwareService.Start(state,catalog,slot,room.GetSelectedId(),design.GetSelectedId(),medium.GetSelectedId(),Tr("OWN_HARDWARE_PROJECT"));if(result==OwnHardwareResult.Success)ShowOfficeReconstruction(state);else{Refresh();gate.Text=Tr("OWN_HARDWARE_RESULT_"+result);}};
    }
    private void ShowHardwareProject(GameState state,DevelopmentProjectState project)
    {
        if (ShowPendingOwnHardware(state)) return;
        ShowNativeHardwareProject(state,project);
    }
    private string HardwareTechnologyName(int identifier)
    {
        var catalog=_activeCatalog;var technology=catalog?.Technologies.SingleOrDefault(x=>x.Identifier==identifier);
        return technology is null ? Tr($"TECHNOLOGY_NAME_{identifier}") : CatalogName(catalog,"TECHNOLOGY",technology.SourceIndex,technology.Name);
    }
    private bool ShowPendingOwnHardware(GameState state)
    {
        var technologyProject=state.DevelopmentProjects.FirstOrDefault(p=>p.HardwareDesign?.PendingTechnology is not null);
        if (technologyProject?.HardwareDesign?.PendingTechnology is int id)
        {
            ShowConversation(Tr("OWN_HARDWARE_PROJECT"),[new(SecretaryCaption(state.SecretarySourceIndex),
                string.Format(Tr("OWN_HARDWARE_TECH_COMPLETED"),HardwareTechnologyName(id),technologyProject.RoomIndex+1))],()=>
            {
                if (!ReferenceEquals(_session.Current,state) || technologyProject.HardwareDesign.PendingTechnology!=id) return;
                OwnHardwareService.AcknowledgeTechnology(state,technologyProject);ShowOfficeReconstruction(state);
            });return true;
        }
        var project=state.DevelopmentProjects.FirstOrDefault(OwnHardwareService.Ready);if (project is null) return false;
        ShowNativeHardwareLaunch(state,project);return true;

    }
}
