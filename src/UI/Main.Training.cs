using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private string TechnologyName(TechnologyDefinition technology)
        => CatalogName(_activeCatalog,"TECHNOLOGY",technology.SourceIndex,technology.Name);

    private void ShowTechnologies(GameState state)=>ShowTechnologyPurchase(state);

    private DesktopWindow? _companyResearchWindow;
    private Action? _refreshCompanyResearch;
    private void CloseCompanyResearch()
    {
        _refreshCompanyResearch=null;
        if(_companyResearchWindow is { } window&&GodotObject.IsInstanceValid(window))
        {_dialogPositions["CompanyResearchWindow"]=window.Position;_workspace?.RemoveChild(window);window.QueueFree();}
        _companyResearchWindow=null;
        if(_employeeDetails is null&&_windows.TryGetValue("CompanyWindow",out var company)&&GodotObject.IsInstanceValid(company))company.Show();
    }
    private Tree ResearchList(Control canvas,string name,Rect2 rect)
    {
        var table=ManagementTable(canvas,name,["TECHNOLOGY_NAME"],[112],new(rect.Position,new(rect.Size.X-20,rect.Size.Y)));
        table.ColumnTitlesVisible=false;
        foreach(string key in new[]{"v_separation","inner_item_margin_top","inner_item_margin_bottom","draw_guides"})
            table.AddThemeConstantOverride(key,0);
        // Keep the original separate arrow/track rail visible even for an empty list.
        var source=table.GetChildren(true).OfType<VScrollBar>().Single();
        source.Modulate=Colors.Transparent;source.MouseFilter=Control.MouseFilterEnum.Ignore;
        float x=rect.End.X-16;
        var rail=NativePlace(canvas,new VScrollBar {Name=name+"Scroll"},new(x,rect.Position.Y+16,16,rect.Size.Y-32));
        rail.AddThemeStyleboxOverride("scroll",ClassicTheme.CompactInput());
        foreach(string key in new[]{"grabber","grabber_highlight","grabber_pressed"})
            rail.AddThemeStyleboxOverride(key,ClassicTheme.CompactButton());
        var up=NativeAction(canvas,name+"ScrollUp","▲",new(x,rect.Position.Y,16,16),()=>source.Value-=16);
        var down=NativeAction(canvas,name+"ScrollDown","▼",new(x,rect.End.Y-16,16,16),()=>source.Value+=16);
        up.AddThemeFontSizeOverride("font_size",8);down.AddThemeFontSizeOverride("font_size",8);
        bool syncing=false;
        void Sync()
        {
            syncing=true;rail.MaxValue=source.MaxValue;rail.Page=source.Page;
            rail.SetValueNoSignal(source.Value);syncing=false;
        }
        source.Changed+=Sync;source.ValueChanged+=_=>Sync();
        rail.ValueChanged+=value=>{if(!syncing)source.Value=value;};Sync();
        return table;
    }
    private void ShowCompanyResearch(GameState state)
    {
        CloseEmployeeDetails();CloseCompanyResearch();
        var company=_windows["CompanyWindow"];company.Hide();
        var window=new DesktopWindow {Name="CompanyResearchWindow"};_workspace!.AddChild(window);_companyResearchWindow=window;
        window.Initialize(Tr("TECHNOLOGIES"),_dialogPositions.GetValueOrDefault("CompanyResearchWindow",company.Position),new(172,305));
        window.CloseRequested=CloseCompanyResearch;
        var canvas=new Control {Name="NativeDialogCanvas",CustomMinimumSize=new(164,273)};window.Body.AddChild(canvas);
        NativeFill(canvas,"ResearchOwnedHeading",new(6,5,132,13),new(.77f,1,.77f));
        NativeText(canvas,"ResearchOwnedCaption",Tr("OWNED_TECHNOLOGIES"),new(6,5,132,13),10,HorizontalAlignment.Center);
        var owned=ResearchList(canvas,"ResearchOwnedTechnologies",new(6,22,152,97));
        var buy=NativeAction(canvas,"ResearchBuyTechnology","BUY_TECHNOLOGIES",new(6,126,136,25),()=>
        {if(DesktopActionEnabled("TECHNOLOGIES")&&(_dialog is null||_clockRunning))ShowTechnologies(state);});
        NativeFill(canvas,"ResearchGenresHeading",new(6,158,132,13),new(1,.85f,.65f));
        NativeText(canvas,"ResearchGenresCaption",Tr("AVAILABLE_GENRES"),new(6,158,132,13),10,HorizontalAlignment.Center);
        var genres=ResearchList(canvas,"ResearchAvailableGenres",new(6,175,152,95));
        string? signature=null;
        _refreshCompanyResearch=()=>
        {
            buy.Disabled=!DesktopActionEnabled("TECHNOLOGIES")||(_dialog is not null&&!_clockRunning);
            string current=string.Join(",",state.TechnologyOwnershipLevels?.OrderBy(p=>p.Key).Select(p=>$"{p.Key}:{p.Value}")??[])+":"+state.Company.OriginalMember90;
            if(current==signature)return;signature=current;owned.Clear();owned.CreateItem();genres.Clear();genres.CreateItem();
            foreach(var t in _activeCatalog?.Technologies??[])
                if(state.TechnologyOwnershipLevels?.GetValueOrDefault(t.SourceIndex)>0)
                {var row=owned.CreateItem(owned.GetRoot());row.SetText(0,TechnologyName(t));row.SetMetadata(0,t.SourceIndex);}
            foreach(var genre in _activeCatalog?.Genres??[])
                if(state.Company.OriginalMember90 is int research&&DevelopmentSelectionRules.IsGenreEligible(genre,research))
                {var row=genres.CreateItem(genres.GetRoot());row.SetText(0,CatalogName(_activeCatalog,"GENRE",genre.SourceIndex,genre.Name));}
        };
        _refreshCompanyResearch();
    }
    // fGMInfo.Command6 (0098232C) opens fGMWSTech only for study actions 1–5.
    private void BeginEmployeeLearning(GameState state,EmployeeState employee)
    {
        if(_dialog is not null||!DesktopActionEnabled("TRAINING")||!state.Employees.Contains(employee))return;
        _employeeTechnologyPage=true;
        if(_selectedEmployee!=employee)ShowEmployeeDetails(state,employee);
        if(employee.Office?.Action is >=1 and <=5)ShowTraining(state,employee);
        else ShowConversation(Tr("LEARNING_DIALOG"),[new(SecretaryCaption(state.SecretarySourceIndex),Tr("TRAINING_RESULT_NotAtStudySeat"),SecretaryAdviceRules.PortraitId(state.SecretarySourceIndex))],()=>ShowOfficeReconstruction(state),keepEmployeeDetails:true);
    }
    private void ShowTraining(GameState state,EmployeeState employee,int? selectedSource=null)
    {
        if(_selectedEmployee!=employee){_employeeTechnologyPage=true;ShowEmployeeDetails(state,employee);}
        ClearContent(keepEmployeeDetails:true);var canvas=NativeWindow("TrainingWindow",Tr("LEARNING_DIALOG"),new(273,238));
        var catalog=_importer.LoadCatalog();
        var courses=catalog?.Technologies.Where(t=>employee.TechnologyLevels?.TryGetValue(t.SourceIndex,out int level)==true&&level<3&&state.TechnologyOwnershipLevels?.GetValueOrDefault(t.SourceIndex)>level).OrderBy(t=>t.SourceIndex).ToArray()??[];
        NativeText(canvas,"TrainingNameHeading",Tr("TECHNOLOGY_NAME"),new(16,8,102,14),9);
        NativeText(canvas,"TrainingTimeHeading",Tr("LEARNING_TIME"),new(120,8,54,14),9);
        NativeText(canvas,"TrainingExperienceHeading",Tr("REQUIRED_EXPERIENCE"),new(176,8,89,14),9);
        var table=ManagementTable(canvas,"TrainingCourses",["TECHNOLOGY_NAME","LEARNING_TIME","REQUIRED_EXPERIENCE"],[124,56,73],new(8,24,257,113));table.ColumnTitlesVisible=false;
        foreach(string key in new[]{"v_separation","inner_item_margin_top","inner_item_margin_bottom","draw_guides"})table.AddThemeConstantOverride(key,0);
        NativeFill(canvas,"TrainingDescriptionBox",new(8,144,239,45),Colors.White,true);
        var description=new RichTextLabel {Name="TrainingDescription",ScrollActive=true};description.AddThemeFontSizeOverride("normal_font_size",10);NativePlace(canvas,description,new(11,147,233,39));
        if(courses.Length==0)description.Text=Tr(state.TechnologyOwnershipLevels?.Values.Any(v=>v>0)==true?"NO_LEARNABLE_TECHNOLOGIES":"NO_PURCHASED_TECHNOLOGIES");
        NativeText(canvas,"TrainingExperienceCaption",string.Format(DisplayNumbers.Provider,Tr("EMPLOYEE_LEARNING_EXPERIENCE"),EmployeeName(employee)),new(8,192,193,14),10);
        NativeText(canvas,"TrainingExperience",employee.Experience?.ToDisplayNumber()??"?",new(200,192,49,14),10,HorizontalAlignment.Right);
        TechnologyDefinition? selected=null;
        var train=NativeAction(canvas,"TrainSelectedTechnology","CONFIRM_LEARNING",new(80,208,81,25),()=>
        {
            if(selected is null)return;
            var result=TrainingService.TryStart(state,employee,selected);
            if(result==TrainingResult.Success){ShowOfficeReconstruction(state);RefreshEmployeeDetails();return;}
            int source=selected.SourceIndex;
            string message=result switch
            {
                TrainingResult.InsufficientExperience=>Tr("LEARNING_INSUFFICIENT_EXPERIENCE"),
                TrainingResult.CapacityReached when employee.MaximumTechnologies==0=>string.Format(DisplayNumbers.Provider,Tr("LEARNING_CANNOT_LEARN"),EmployeeName(employee)),
                TrainingResult.CapacityReached=>string.Format(DisplayNumbers.Provider,Tr("LEARNING_CAPACITY_REACHED"),EmployeeName(employee),employee.MaximumTechnologies),
                _=>Tr("TRAINING_RESULT_"+result)
            };
            ShowConversation(Tr("LEARNING_DIALOG"),[new(SecretaryCaption(state.SecretarySourceIndex),message,SecretaryAdviceRules.PortraitId(state.SecretarySourceIndex))],()=>ShowTraining(state,employee,source),keepEmployeeDetails:true);
        },false);
        NativeAction(canvas,"CancelTraining","CANCEL",new(168,208,81,25),()=>ShowOfficeReconstruction(state));
        void Select()
        {
            if(table.GetSelected() is not { } row)return;selected=courses.Single(t=>t.SourceIndex==row.GetMetadata(0).AsInt32());
            description.Text=TranslationServer.GetLocale().StartsWith("en")?string.Format(DisplayNumbers.Provider,Tr("TRAINING_DESCRIPTION"),TechnologyName(selected),employee.TechnologyLevels![selected.SourceIndex]+1):selected.Description;
            train.Name="TrainTechnology"+selected.SourceIndex;train.Disabled=TrainingEligibility(state,employee,selected) is TrainingResult.StateUnknown or TrainingResult.NotEmployed or TrainingResult.NotAtStudySeat or TrainingResult.Unavailable;
            train.TooltipText=Tr("TRAINING_RESULT_"+TrainingEligibility(state,employee,selected));
        }
        table.ItemSelected+=Select;
        foreach(var course in courses)
        {
            int level=employee.TechnologyLevels![course.SourceIndex];var row=table.CreateItem(table.GetRoot());row.SetMetadata(0,course.SourceIndex);row.SetText(0,$"{TechnologyName(course)} Lv.{level+1}");row.SetTooltipText(0,row.GetText(0));
            row.SetText(1,course.RequiredTrainingWork.ElementAtOrDefault(level).ToString());row.SetText(2,course.ExperienceCosts.ElementAtOrDefault(level).ToString());
            row.SetTextAlignment(1,HorizontalAlignment.Right);row.SetTextAlignment(2,HorizontalAlignment.Right);if(course.SourceIndex==selectedSource)row.Select(0);
        }
        if(table.GetSelected() is null)table.GetRoot().GetFirstChild()?.Select(0);Select();
    }
    private void ShowTechnologyUse(GameState state,EmployeeState employee)
    {
        ClearContent();var canvas=NativeWindow("TechnologyUseWindow",Tr("USE_TECHNOLOGY"),new(410,386));
        var catalog=_importer.LoadCatalog();
        NativeText(canvas,"TechnologyUseEmployee",$"{EmployeeName(employee)} — {Tr("EXPERIENCE")}: {employee.Experience:0.0}",new(4,2,402,21));
        var scroll=NativePlace(canvas,new ScrollContainer {Name="TechnologyUseList",HorizontalScrollMode=ScrollContainer.ScrollMode.Disabled},new(4,29,402,144));
        var list=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};list.AddThemeConstantOverride("separation",0);scroll.AddChild(list);
        NativeFill(canvas,"TechnologyUseDescriptionBox",new(4,180,402,91),Colors.White,true);
        var detailsScroll=NativePlace(canvas,new ScrollContainer {HorizontalScrollMode=ScrollContainer.ScrollMode.Disabled},new(7,183,396,85));
        var details=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};detailsScroll.AddChild(details);
        var description=new Label {Name="TechnologyUseDescription",AutowrapMode=TextServer.AutowrapMode.WordSmart,SizeFlagsHorizontal=SizeFlags.ExpandFill};description.AddThemeFontSizeOverride("font_size",10);details.AddChild(description);
        void Boundary(string name,string text){var label=new Label {Name=name,Text=text,AutowrapMode=TextServer.AutowrapMode.WordSmart};label.AddThemeFontSizeOverride("font_size",10);details.AddChild(label);}
        if(catalog is not null)
        foreach(var technology in catalog.Technologies)
        {
            if(employee.TechnologyLevels?.TryGetValue(technology.SourceIndex,out int level)!=true||level is <1 or >3)continue;
            var result=TechnologyApplicationService.CheckUse(state,employee,technology);
            var row=new HBoxContainer();list.AddChild(row);
            var select=new Button {Name="SelectUseTechnology"+technology.SourceIndex,Text=$"{TechnologyName(technology)} · {Tr("LEVEL")} {level}",SizeFlagsHorizontal=SizeFlags.ExpandFill,ClipText=true};select.AddThemeFontSizeOverride("font_size",11);row.AddChild(select);
            string note=TranslationServer.GetLocale().StartsWith("en")?Tr("TECHNOLOGY_EFFECT_"+technology.Identifier)+"\n"+Tr("TECHNOLOGY_EFFECT_TIMING"):technology.Description;
            if(technology.Identifier==4){Boundary("SpeedTechnologyNote",Tr("SPEED_TECHNOLOGY_NOTE"));if(employee.Office is {WorkTier:4,WorkTicks:>0})Boundary("SpeedTechnologyInterruptedWork",Tr("SPEED_TECHNOLOGY_INTERRUPTS_APPLICATION"));}
            if(technology.Identifier==4)note+="\n"+Tr("SPEED_TECHNOLOGY_NOTE")+(employee.Office is {WorkTier:4,WorkTicks:>0}?"\n"+Tr("SPEED_TECHNOLOGY_INTERRUPTS_APPLICATION"):"");
            if(technology.Identifier==5&&level==3){var target=TechnologyApplicationService.LevelThreeStressTarget(state);Boundary("StressReliefSourceTarget",target is null?Tr("STRESS_RELIEF_MAPPING_UNKNOWN"):string.Format(DisplayNumbers.Provider,Tr("STRESS_RELIEF_SLOT_ZERO"),EmployeeName(target)));}
            if(result!=TechnologyApplicationResult.Success)note+="\n"+Tr("TECHNOLOGY_USE_"+result);
            select.TooltipText=note;select.Pressed+=()=>description.Text=note;if(description.Text.Length==0)description.Text=note;
            if(technology.RawTable1568.Count!=3)continue;
            var use=new Button {Name="UseTechnology"+technology.SourceIndex,Text=$"{Tr("USE_TECHNOLOGY")} · {technology.RawTable1568[level-1]}",Disabled=result!=TechnologyApplicationResult.Success,TooltipText=note,CustomMinimumSize=new(126,23)};use.AddThemeFontSizeOverride("font_size",10);row.AddChild(use);
            use.Pressed+=()=>{var outcome=TechnologyApplicationService.TryUse(state,employee,technology);if(outcome==TechnologyApplicationResult.Success)ShowOfficeReconstruction(state);else _status.Text=Tr("TECHNOLOGY_USE_"+outcome);};
        }
        string activeText=employee.Office?.TechnologyApplication is TechnologyApplicationState active ? active.IsSpeedBoost?string.Format(DisplayNumbers.Provider,Tr("SPEED_TECHNOLOGY_REMAINING"),employee.Office.SpeedBoostTicks*OfficeStaffRules.TickSeconds):$"{Tr("TECHNOLOGY_REMAINING_WORK")}: {active.RemainingUnits}":Tr("TECHNOLOGY_USE_NOTE");
        _status=NativeText(canvas,employee.Office?.TechnologyApplication?.IsSpeedBoost==true?"SpeedTechnologyRemaining":"TechnologyUseStatus",activeText,new(4,274,402,26),10);_status.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        ManagementSaveActions(canvas,state,357,402);
        NativeAction(canvas,"TechnologyUseDesk","ASSIGN_DESK",new(4,303,205,25),()=>ShowDeskAssignment(state,employee));
        NativeAction(canvas,"CancelTechnologyUse","BACK_TO_OFFICE",new(298,303,108,25),()=>ShowOfficeReconstruction(state));
    }
    private static TrainingResult TrainingEligibility(GameState state,EmployeeState employee,TechnologyDefinition technology)
    {
        try{return TrainingService.CheckStart(state,employee,technology);}
        catch(InvalidDataException){return TrainingResult.StateUnknown;}
    }
    private void AddSaveActions(GameState state)
    {
        _status=new Label();_content.AddChild(_status);
        AddAction("SAVE_GAME",()=>
        {
            try {Persistence.SaveStore.Save(SavePath,state);_status.Text=Tr("SAVE_OK");}
            catch (Exception ex) {_status.Text=$"{Tr("SAVE_FAILED")}: {ex.Message}";}
        });
        AddAction("LOAD_GAME",LoadSession,System.IO.File.Exists(SavePath));
    }
}
