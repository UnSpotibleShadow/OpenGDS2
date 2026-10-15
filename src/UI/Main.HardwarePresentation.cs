using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private Texture2D? HardwareDesignPicture(int? design,int? medium)=>design is >=0 and <=1&&medium is >=0 and <=1?OriginalAssetStore.LoadTexture($"Graphic/Embedded/Hardware{11+design*2+medium}.bmp"):null;
    private void ShowHardwareMarket(GameState state)
    {
        ClearContent();var canvas=NativeWindow("HardwareMarketWindow",Tr("HARDWARE_MARKET"),new(406,203));
        NativeFill(canvas,"HardwareMarketInstructionBox",new(4,4,398,22),Colors.White,true);
        NativeText(canvas,"HardwareMarketInstruction",Tr("HARDWARE_MARKET_NOTE"),new(7,4,392,22),10,HorizontalAlignment.Center);
        var table=ManagementTable(canvas,"HardwareMarketTable",["TARGET_HARDWARE","LICENSE_COLUMN","HARDWARE_SHIPPED","HARDWARE_MAKER"],[133,57,92,100],new(4,33,398,130));
        NativeAction(canvas,"CloseHardwareMarket","CANCEL",new(294,174,108,25),()=>ShowOfficeReconstruction(state));
        string previous="";
        void Refresh()
        {
            var catalog=OwnHardwareService.Catalog(state,_activeCatalog!);
            var hardware=catalog.Hardware.Where(h=>state.AvailableHardware?.Contains(h.SourceIndex)==true).OrderBy(h=>h.SourceIndex).ToArray();
            string snapshot=string.Join("|",hardware.Select(h=>$"{h.SourceIndex}:{h.Name}:{state.HardwareLicenses?.Contains(h.SourceIndex)}:{state.Calendar?.Hardware.SingleOrDefault(m=>m.SourceIndex==h.SourceIndex)?.Market}"));
            if(snapshot==previous&&table.GetRoot().GetChildCount()>0)return;previous=snapshot;
            int? selected=table.GetSelected()?.GetMetadata(0).AsInt32();var scrollbar=table.GetChildren(true).OfType<VScrollBar>().FirstOrDefault();double scroll=scrollbar?.Value??0;
            table.Clear();table.CreateItem();
            foreach(var h in hardware)
            {
                var row=table.CreateItem(table.GetRoot());row.SetMetadata(0,h.SourceIndex);row.SetText(0,CatalogName(catalog,"HARDWARE",h.SourceIndex,h.Name));
                row.SetText(1,state.HardwareLicenses is null?"?":state.HardwareLicenses.Contains(h.SourceIndex)?Tr("LICENSE_OWNED_SHORT"):"—");
                var market=state.Calendar?.Hardware.SingleOrDefault(m=>m.SourceIndex==h.SourceIndex);row.SetText(2,market is null?"?":(Math.Floor(market.Market)*10_000).ToString("N0"));row.SetTextAlignment(2,HorizontalAlignment.Right);
                row.SetText(3,h.SourceIndex==11?state.Company.Name:h.RawMembers.TryGetValue("0FA8",out long maker)&&maker is >=0 and <=9?DialogueText("CALENDAR_MAKER_"+maker):"?");
                if(h.SourceIndex==selected)row.Select(0);
            }
            if(scrollbar is not null)scrollbar.Value=scroll;
        }
        _activeCatalog=_importer.LoadCatalog();_clockRunning=true;_refreshCoreStatus=Refresh;Refresh();
    }
    private void ShowOwnHardwareReport(GameState state)
    {
        if(state.OwnHardware.Launched is not { } hardware){ShowOfficeReconstruction(state);return;}
        ClearContent();var canvas=NativeWindow("OwnHardwareReportWindow",hardware.Name,new(312,83));
        NativePlace(canvas,Picture("OwnHardwareReportPicture",HardwareDesignPicture(state.OwnHardware.LaunchedDesign,state.OwnHardware.LaunchedMedium),new(64,64)),new(4,7,64,64));
        NativeText(canvas,"OwnHardwareReportName",hardware.Name,new(76,4,232,20));
        var design=NativeText(canvas,"OwnHardwareReportDesign","",new(76,25,232,17),10);
        var medium=NativeText(canvas,"OwnHardwareReportMedium","",new(76,43,232,17),10);
        NativeText(canvas,"OwnHardwareUnitsCaption",Tr("HARDWARE_SHIPPED"),new(76,61,126,18),10);
        var units=NativeText(canvas,"OwnHardwareUnits","",new(204,61,104,18),11,HorizontalAlignment.Right);
        void Refresh()
        {
            design.Text=state.OwnHardware.LaunchedDesign is int d?Tr("HARDWARE_FORM_"+d):"?";
            medium.Text=state.OwnHardware.LaunchedMedium is int m?Tr("HARDWARE_MEDIA_"+m):"?";
            var market=state.Calendar?.Hardware.SingleOrDefault(h=>h.SourceIndex==11);units.Text=market is null?"?":(Math.Floor(market.Market)*10_000).ToString("N0");
        }
        _clockRunning=true;_activeCatalog=_importer.LoadCatalog();_refreshCoreStatus=Refresh;Refresh();
    }
    private void ShowNativeHardwareProject(GameState state,DevelopmentProjectState project)
    {
        CloseRetainedProjectReport(project.RoomIndex);_closingProjectReport=_projectReportRoom==project.RoomIndex;
        try{ClearContent();}finally{_closingProjectReport=false;}
        var canvas=NativeWindow("HardwareProjectWindow",Tr("OWN_HARDWARE_PROJECT"),new(376,174),"DevelopmentProjectWindow:"+project.RoomIndex);
        var design=project.HardwareDesign!;
        NativeText(canvas,"HardwareDesignCaption",Tr("OWN_HARDWARE_DESIGN"),new(4,4,102,18),10);
        NativeText(canvas,"HardwareDesignValue",Tr("HARDWARE_FORM_"+design.Design),new(109,4,263,18),10);
        NativeText(canvas,"HardwareMediumCaption",Tr("OWN_HARDWARE_MEDIUM"),new(4,25,102,18),10);
        NativeText(canvas,"HardwareMediumValue",Tr("HARDWARE_MEDIA_"+design.Medium),new(109,25,263,18),10);
        string[] keys=["STAT_FUN","STAT_NOVELTY","STAT_GRAPHICS","STAT_MUSIC"];
        var quality=new Label[4];var flags=new Label[4];
        for(int i=0;i<4;i++)
        {
            NativeText(canvas,"HardwareQualityCaption"+i,Tr(keys[i]),new(4,50+i*18,70,18),10);
            quality[i]=NativeText(canvas,"HardwareQualityValue"+i,"",new(75,50+i*18,80,18),10,HorizontalAlignment.Right);
            int id=OwnHardwareService.RequiredTechnologies[i];NativeText(canvas,"HardwareTechnology"+id,HardwareTechnologyName(id),new(164,50+i*18,166,18),10);
            flags[i]=NativeText(canvas,"HardwareTechnologyFlag"+id,"",new(334,50+i*18,38,18),10,HorizontalAlignment.Right);
        }
        var note=NativeText(canvas,"HardwareLifecycle",Tr("HARDWARE_REQUIREMENTS_NOTE"),new(4,124,368,28),10);note.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var report=NativeText(canvas,"HardwareReport","",new(4,153,255,20),10);
        var progress=NativePlace(canvas,new ProgressBar {Name="HardwareProgress",MinValue=0,MaxValue=2500,ShowPercentage=false},new(266,158,106,9));
        void Refresh()
        {
            double[] values=[project.Quality.Fun,project.Quality.Novelty,project.Quality.Graphics,project.Quality.Music];
            for(int i=0;i<4;i++){quality[i].Text=$"{values[i]:N0}/800";flags[i].Text=design.TechnologyFlags[OwnHardwareService.RequiredTechnologies[i]]==1?"on":"off";}
            report.Text=$"{Tr("DEVELOPMENT_POINTS")}: {project.Progress:N0}/2,500";progress.Value=project.Progress;
            
        }
        _projectReportRoom=project.RoomIndex;_projectReportRefresh=Refresh;_dialog!.CloseRequested=()=>{_closingProjectReport=true;try{ShowOfficeReconstruction(state);}finally{_closingProjectReport=false;}};
        _activeCatalog=_importer.LoadCatalog();_clockRunning=true;_refreshCoreStatus=Refresh;Refresh();
    }
    private void NativeHardwareScene(Control canvas,string name)
    {
        var scene=OriginalAssetStore.LoadTexture("Graphic/Embedded/HardwareLaunchScene.bmp");
        float left=(canvas.CustomMinimumSize.X-260)/2;
        NativeFill(canvas,name+"Frame",new(left,4,260,164),Colors.White,true);
        NativePlace(canvas,Picture(name,scene,new(256,160)),new(left+2,6,256,160));
        if(scene is null)NativeText(canvas,name+"Missing",Tr("EVENT_ARTWORK_REIMPORT"),new(left+6,60,246,45),10).AutowrapMode=TextServer.AutowrapMode.WordSmart;
    }
    private void ShowNativeHardwareLaunch(GameState state,DevelopmentProjectState project)
    {
        ClearContent();var canvas=NativeWindow("HardwareLaunchWindow",Tr("OWN_HARDWARE_LAUNCH"),new(322,290));NativeHardwareScene(canvas,"HardwareLaunchScene");
        var design=project.HardwareDesign!;
        NativePlace(canvas,Picture("HardwareLaunchDevice",HardwareDesignPicture(design.Design,design.Medium),new(64,64)),new(4,175,64,64));
        NativeText(canvas,"HardwareLaunchNameCaption",Tr("OWN_HARDWARE_NAME"),new(77,174,241,20),10);
        var name=NativePlace(canvas,new LineEdit {Name="HardwareLaunchName",Text=design.LaunchName??Tr("OWN_HARDWARE_DEFAULT_NAME"),MaxLength=128},new(77,198,241,24));name.AddThemeFontSizeOverride("font_size",11);
        name.TextChanged+=value=>design.LaunchName=value;
        var gate=NativeText(canvas,"HardwareLaunchGate","",new(4,242,314,20),10);
        NativeAction(canvas,"LaunchOwnHardware","OWN_HARDWARE_LAUNCH",new(145,261,173,25),()=>
        {
            if(!ReferenceEquals(_session.Current,state))return;var result=OwnHardwareService.Launch(state,project,name.Text);
            if(result==OwnHardwareResult.Success){PersistPendingRankings(state);ShowOfficeReconstruction(state);}else gate.Text=Tr("OWN_HARDWARE_RESULT_"+result);
        });
        NativeAction(canvas,"SaveHardwareLaunch","SAVE_GAME",new(4,261,133,25),()=>{try{Persistence.SaveStore.Save(SavePath,state);gate.Text=Tr("SAVE_OK");}catch(Exception ex){gate.Text=Tr("SAVE_FAILED")+": "+ex.Message;}});
    }
    private void ShowNativeHardwareAnnouncement(GameState state,CalendarTransaction pending,HardwareDefinition hardware,OriginalCatalog catalog)
    {
        ClearContent();var canvas=NativeWindow("HardwareAnnouncementWindow",Tr("CALENDAR_EVENT_HardwareLaunch"),new(322,324));NativeHardwareScene(canvas,"HardwareAnnouncementScene");
        NativePlace(canvas,Picture("HardwareAnnouncementPicture",HardwarePicture(state,hardware.SourceIndex),new(64,64)),new(4,175,64,64));
        NativeText(canvas,"HardwareAnnouncementName",CatalogName(catalog,"HARDWARE",hardware.SourceIndex,hardware.Name),new(77,174,241,21),11);
        string maker=hardware.RawMembers.TryGetValue("0FA8",out long id)&&id is >=0 and <=9?DialogueText("CALENDAR_MAKER_"+id):"?";
        NativeText(canvas,"HardwareAnnouncementMaker",maker,new(77,197,241,18),10);
        NativeText(canvas,"HardwareAnnouncementFee",Tr("LICENCE_FEE")+": "+(hardware.RawMembers.TryGetValue("0FC4",out long fee)?$"¥{fee:N0}":"?"),new(77,218,241,18),10);
        NativeFill(canvas,"HardwareAnnouncementDescriptionBox",new(77,241,241,47),Colors.White,true);
        var description=new RichTextLabel {Name="HardwareAnnouncementDescription",ScrollActive=true};description.AddThemeFontSizeOverride("normal_font_size",10);NativePlace(canvas,description,new(80,244,235,41));
        description.Text=TranslationServer.GetLocale().StartsWith("en")?string.Format(Tr("HARDWARE_ANNOUNCEMENT_DESCRIPTION"),CatalogName(catalog,"HARDWARE",hardware.SourceIndex,hardware.Name),hardware.RawMembers.GetValueOrDefault("0F00")):hardware.Description;
        NativeAction(canvas,"AcknowledgeCalendar","CONTINUE",new(107,295,108,25),()=>{if(!ReferenceEquals(state.Calendar?.Pending,pending))return;pending.HardwareLaunchPage=1;ShowHardwareLaunchCard(state,pending);});
    }
}
