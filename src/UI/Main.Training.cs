using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private string TechnologyName(TechnologyDefinition technology)
        => CatalogName(_activeCatalog,"TECHNOLOGY",technology.SourceIndex,technology.Name);

    private void ShowTechnologies(GameState state)=>ShowTechnologyPurchase(state);

    private void ShowTraining(GameState state,EmployeeState employee,int? selectedSource=null)
    {
        ClearContent();var canvas=NativeWindow("TrainingWindow",Tr("TRAINING"),new(370,352));
        var catalog=_importer.LoadCatalog();
        var courses=catalog?.Technologies.Where(t=>employee.TechnologyLevels?.ContainsKey(t.SourceIndex)==true).OrderBy(t=>t.SourceIndex).ToArray() ?? [];
        NativeText(canvas,"TrainingEmployeeName",EmployeeName(employee),new(4,2,362,20));
        var table=ManagementTable(canvas,"TrainingCourses",["TECHNOLOGY_NAME","TRAINING_WORK","EXPERIENCE"],[174,81,91],new(4,26,362,137));
        NativeFill(canvas,"TrainingDescriptionBox",new(4,170,362,54),Colors.White,true);
        var description=new RichTextLabel {Name="TrainingDescription",ScrollActive=true};description.AddThemeFontSizeOverride("normal_font_size",10);NativePlace(canvas,description,new(7,173,356,48));
        NativeText(canvas,"TrainingExperience",$"{Tr("EXPERIENCE")}: {employee.Experience:0.0} / {Tr("MAXIMUM_TECHNOLOGIES")}: {employee.MaximumTechnologies}",new(4,225,362,20),10);
        var seats=NativePlace(canvas,new HBoxContainer {Name="StudySeats"},new(4,247,166,23));seats.AddThemeConstantOverride("separation",2);
        for(int action=1;action<=5;action++)
        {
            int studyAction=action;var button=new Button {Name=$"AssignStudySeat{action}",Text=action.ToString(),CustomMinimumSize=new(29,23),TooltipText=$"{Tr("STUDY_SEAT")} {action}",Disabled=state.Employees.Any(x=>x.Office?.Action==studyAction)};
            button.AddThemeFontSizeOverride("font_size",10);seats.AddChild(button);
            button.Pressed+=()=>{var result=OfficeAssignmentService.TryAssignStudySeat(state,employee,studyAction);ShowTraining(state,employee,table.GetSelected()?.GetMetadata(0).AsInt32());if(result!=OfficeAssignmentResult.Success)_status.Text=Tr("ASSIGNMENT_RESULT_"+result);};
        }
        TechnologyDefinition? selected=null;
        var train=NativeAction(canvas,"TrainSelectedTechnology","TRAINING",new(178,247,98,25),()=>{},false);
        NativeAction(canvas,"CancelTraining","BACK_TO_OFFICE",new(282,247,84,25),()=>ShowOfficeReconstruction(state));
        _status=NativeText(canvas,"TrainingStatus",Tr("TRAINING_NOTE"),new(4,275,362,42),10);_status.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        void Select()
        {
            if(table.GetSelected() is not { } row)return;selected=courses.Single(t=>t.SourceIndex==row.GetMetadata(0).AsInt32());int level=employee.TechnologyLevels![selected.SourceIndex];
            description.Text=TranslationServer.GetLocale().StartsWith("en")?string.Format(Tr("TRAINING_DESCRIPTION"),TechnologyName(selected)):selected.Description;
            if(employee.Office?.Training?.TechnologySourceIndex==selected.SourceIndex)description.Text+=$"\n{Tr("TRAINING_PROGRESS")}: {employee.Office.Training.Progress} / {selected.RequiredTrainingWork.ElementAtOrDefault(level)}\n{Tr("TRAINING_RESUME_NOTE")}";
            train.Name="TrainTechnology"+selected.SourceIndex;train.Disabled=TrainingEligibility(state,employee,selected)!=TrainingResult.Success;
            train.TooltipText=Tr("TRAINING_RESULT_"+TrainingEligibility(state,employee,selected));
        }
        table.ItemSelected+=Select;
        foreach(var course in courses)
        {
            int level=employee.TechnologyLevels![course.SourceIndex];var row=table.CreateItem(table.GetRoot());row.SetMetadata(0,course.SourceIndex);row.SetText(0,$"{TechnologyName(course)} · {Tr("LEVEL")} {level}");
            row.SetText(1,level is >=0 and <3 && course.RequiredTrainingWork.Count==3?course.RequiredTrainingWork[level].ToString():"—");
            row.SetText(2,level is >=0 and <3 && course.ExperienceCosts.Count==3?course.ExperienceCosts[level].ToString():"—");row.SetTextAlignment(1,HorizontalAlignment.Right);row.SetTextAlignment(2,HorizontalAlignment.Right);if(course.SourceIndex==selectedSource)row.Select(0);
        }
        if(table.GetSelected() is null)table.GetRoot().GetFirstChild()?.Select(0);Select();
        ManagementSaveActions(canvas,state,323,362);
        train.Pressed+=()=>{if(train.Disabled||selected is null)return;var result=TrainingService.TryStart(state,employee,selected);if(result==TrainingResult.Success)ShowOfficeReconstruction(state);else _status.Text=Tr("TRAINING_RESULT_"+result);};
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
            string note=TranslationServer.GetLocale().StartsWith("en")?string.Format(Tr("TECHNOLOGY_USE_DESCRIPTION"),TechnologyName(technology)):technology.Description;
            if(technology.Identifier==4){Boundary("SpeedTechnologyNote",Tr("SPEED_TECHNOLOGY_NOTE"));if(employee.Office is {WorkTier:4,WorkTicks:>0})Boundary("SpeedTechnologyInterruptedWork",Tr("SPEED_TECHNOLOGY_INTERRUPTS_APPLICATION"));}
            if(technology.Identifier==4)note+="\n"+Tr("SPEED_TECHNOLOGY_NOTE")+(employee.Office is {WorkTier:4,WorkTicks:>0}?"\n"+Tr("SPEED_TECHNOLOGY_INTERRUPTS_APPLICATION"):"");
            if(technology.Identifier==5&&level==3){var target=TechnologyApplicationService.LevelThreeStressTarget(state);Boundary("StressReliefSourceTarget",target is null?Tr("STRESS_RELIEF_MAPPING_UNKNOWN"):string.Format(Tr("STRESS_RELIEF_SLOT_ZERO"),EmployeeName(target)));}
            if(result!=TechnologyApplicationResult.Success)note+="\n"+Tr("TECHNOLOGY_USE_"+result);
            select.TooltipText=note;select.Pressed+=()=>description.Text=note;if(description.Text.Length==0)description.Text=note;
            if(technology.RawTable1568.Count!=3)continue;
            var use=new Button {Name="UseTechnology"+technology.SourceIndex,Text=$"{Tr("USE_TECHNOLOGY")} · {technology.RawTable1568[level-1]}",Disabled=result!=TechnologyApplicationResult.Success,TooltipText=note,CustomMinimumSize=new(126,23)};use.AddThemeFontSizeOverride("font_size",10);row.AddChild(use);
            use.Pressed+=()=>{var outcome=TechnologyApplicationService.TryUse(state,employee,technology);if(outcome==TechnologyApplicationResult.Success)ShowOfficeReconstruction(state);else _status.Text=Tr("TECHNOLOGY_USE_"+outcome);};
        }
        string activeText=employee.Office?.TechnologyApplication is TechnologyApplicationState active ? active.IsSpeedBoost?string.Format(Tr("SPEED_TECHNOLOGY_REMAINING"),employee.Office.SpeedBoostTicks*OfficeStaffRules.TickSeconds):$"{Tr("TECHNOLOGY_REMAINING_WORK")}: {active.RemainingUnits}":Tr("TECHNOLOGY_USE_NOTE");
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
