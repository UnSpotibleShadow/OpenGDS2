using Godot;
using OpenGDS2.Domain;
using OpenGDS2.Core;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ShowHardwarePlan(GameState state,int slot)
    {
        var catalog=_activeCatalog;if (catalog is null) {ShowFacilities(state,slot);return;}
        ClearContent();var canvas=NativeWindow("HardwarePlanWindow",Tr("OWN_HARDWARE_START"),new(310,218));
        var room=new OptionButton {Name="HardwareRoom"};for(int i=0;i<2;i++)room.AddItem($"{Tr("DEVELOPMENT_ROOM")} {i+1}",i);
        var design=new OptionButton {Name="HardwareDesign"};design.AddItem(Tr("OWN_HARDWARE_DESIGN_0"),0);design.AddItem(Tr("OWN_HARDWARE_DESIGN_1"),1);design.Select(state.OwnHardware.SelectedDesign);
        var medium=new OptionButton {Name="HardwareMedium"};medium.AddItem(Tr("OWN_HARDWARE_MEDIUM_0"),0);medium.AddItem(Tr("OWN_HARDWARE_MEDIUM_1"),1);medium.Select(state.OwnHardware.SelectedMedium);
        int y=6;foreach(var (key,field) in new[]{("DEVELOPMENT_ROOM",room),("OWN_HARDWARE_DESIGN",design),("OWN_HARDWARE_MEDIUM",medium)})
        {NativeText(canvas,key,Tr(key),new(4,y,108,26),10);field.AddThemeFontSizeOverride("font_size",10);NativePlace(canvas,field,new(114,y,192,26));y+=33;}
        var cost=NativeText(canvas,"HardwarePlanCost","",new(4,109,302,24),11);
        var gate=NativeText(canvas,"HardwarePlanStatus","",new(4,139,302,40),10);gate.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var accept=NativeAction(canvas,"StartHardwareDevelopment","CONFIRM",new(111,188,94,26),()=>{});
        NativeAction(canvas,"CancelHardwarePlan","CANCEL",new(211,188,95,26),()=>ShowFacilities(state,slot));
        void Refresh()
        {
            cost.Text=$"{Tr("PLANNING_COST")}: ¥{OwnHardwareService.Quote(design.GetSelectedId(),medium.GetSelectedId()):N0}";
            var result=OwnHardwareService.CheckStart(state,catalog,slot,room.GetSelectedId(),design.GetSelectedId(),medium.GetSelectedId());gate.Text=result==OwnHardwareResult.Success?"":Tr("OWN_HARDWARE_RESULT_"+result);accept.Disabled=result!=OwnHardwareResult.Success;
        }
        design.ItemSelected+=_=>Refresh();medium.ItemSelected+=_=>Refresh();room.ItemSelected+=_=>Refresh();Refresh();
        accept.Pressed+=()=>{if(accept.Disabled)return;var result=OwnHardwareService.Start(state,catalog,slot,room.GetSelectedId(),design.GetSelectedId(),medium.GetSelectedId(),Tr("OWN_HARDWARE_PROJECT"));if(result==OwnHardwareResult.Success)ShowOfficeReconstruction(state);else{Refresh();gate.Text=Tr("OWN_HARDWARE_RESULT_"+result);}};
    }
    private void ShowHardwareProject(GameState state,DevelopmentProjectState project)
    {
        if (ShowPendingOwnHardware(state)) return;
        ClearContent();CompactManagementWindow("HardwareProjectWindow",new Vector2(450,380),430);
        AddHeading(Tr("OWN_HARDWARE_PROJECT"),20);
        AnnualMessage("HardwareLifecycle",Tr("OWN_HARDWARE_LIFECYCLE"));
        var progress=new ProgressBar {Name="HardwareProgress",MinValue=0,MaxValue=2500};_content.AddChild(progress);
        var report=new Label {Name="HardwareReport",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(report);
        _refreshCoreStatus=()=>
        {
            progress.Value=project.Progress;var q=project.Quality;
            report.Text=$"{Tr("DEVELOPMENT_POINTS")}: {project.Progress:N1} / 2,500\n"+
                $"{Tr("STAT_FUN")}: {q.Fun:N0} / 800 · {Tr("STAT_NOVELTY")}: {q.Novelty:N0} / 800\n"+
                $"{Tr("STAT_GRAPHICS")}: {q.Graphics:N0} / 800 · {Tr("STAT_MUSIC")}: {q.Music:N0} / 800\n"+
                string.Join("\n",OwnHardwareService.RequiredTechnologies.Select(id=>$"{HardwareTechnologyName(id)}: "+
                    Tr(project.HardwareDesign!.TechnologyFlags[id]==1 ? "OWN_HARDWARE_TECH_APPLIED" : "OWN_HARDWARE_TECH_REQUIRED")));
        };
        _activeCatalog=_importer.LoadCatalog();_clockRunning=true;
        _refreshCoreStatus();AddAction("STAFF",()=>ShowStaff(state));AddSaveActions(state);AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));
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
        ClearContent();CompactManagementWindow("HardwareLaunchWindow",new Vector2(430,300),410);
        AddHeading(Tr("OWN_HARDWARE_LAUNCH"),20);AnnualMessage("HardwareLaunchRules",Tr("OWN_HARDWARE_LAUNCH_RULES"));
        var name=AddField(Tr("OWN_HARDWARE_NAME"),project.HardwareDesign!.LaunchName ?? Tr("OWN_HARDWARE_DEFAULT_NAME"),128);name.Name="HardwareLaunchName";
        name.TextChanged+=value=>project.HardwareDesign.LaunchName=value;
        var gate=new Label {Name="HardwareLaunchGate",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(gate);
        var launch=new Button {Name="LaunchOwnHardware",Text=Tr("OWN_HARDWARE_LAUNCH")};_content.AddChild(launch);
        launch.Pressed+=()=>
        {
            if (!ReferenceEquals(_session.Current,state)) return;
            var result=OwnHardwareService.Launch(state,project,name.Text);
            if (result==OwnHardwareResult.Success) {PersistPendingRankings(state);ShowOfficeReconstruction(state);}
            else gate.Text=Tr($"OWN_HARDWARE_RESULT_{result}");
        };
        AddSaveActions(state);return true;
    }
}
