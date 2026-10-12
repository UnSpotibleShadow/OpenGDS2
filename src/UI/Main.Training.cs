using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private string TechnologyName(TechnologyDefinition technology)
        => CatalogName(_activeCatalog,"TECHNOLOGY",technology.SourceIndex,technology.Name);

    private void ShowTechnologies(GameState state)
    {
        ClearContent();AddHeading(Tr("TECHNOLOGIES"),28);
        AddHeading($"{Tr("FUNDS")}: ¥{state.Money:N0}",20);
        var catalog=_importer.LoadCatalog();
        if (catalog is not null)
        foreach (var technology in catalog.Technologies)
        {
            var panel=new VBoxContainer();_content.AddChild(panel);
            panel.AddChild(new Label {Text=$"{TechnologyName(technology)} — ¥{technology.PurchaseFee:N0}"});
            var button=new Button {Name=$"PurchaseTechnology{technology.SourceIndex}",Text=Tr("PURCHASE_TECHNOLOGY"),
                Disabled=state.TechnologyOwnershipLevels is null ||
                    !state.TechnologyOwnershipLevels.TryGetValue(technology.SourceIndex,out int owned) || owned>=1};
            panel.AddChild(button);
            button.Pressed+=()=>
            {
                var result=TechnologyService.TryPurchase(state,technology);
                ShowTechnologies(state);AddHeading(Tr($"TECHNOLOGY_PURCHASE_{result}"),16);
            };
        }
        AddSaveActions(state);
        AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));
    }

    private void ShowTraining(GameState state,EmployeeState employee)
    {
        ClearContent();AddHeading(Tr("TRAINING"),28);AddEmployeeCard(employee);
        AddHeading($"{Tr("EXPERIENCE")}: {employee.Experience:0.0} / {Tr("MAXIMUM_TECHNOLOGIES")}: {employee.MaximumTechnologies}",18);
        var seats=new HBoxContainer();_content.AddChild(seats);
        for (int action=1;action<=5;action++)
        {
            int studyAction=action;
            var button=new Button {Name=$"AssignStudySeat{action}",Text=$"{Tr("STUDY_SEAT")} {action}",
                Disabled=state.Employees.Any(x=>x.Office?.Action==studyAction)};
            seats.AddChild(button);
            button.Pressed+=()=>
            {
                var result=OfficeAssignmentService.TryAssignStudySeat(state,employee,studyAction);
                ShowTraining(state,employee);
                if (result!=OfficeAssignmentResult.Success) AddHeading(Tr($"ASSIGNMENT_RESULT_{result}"),16);
            };
        }
        var catalog=_importer.LoadCatalog();
        if (catalog is not null)
        foreach (var technology in catalog.Technologies)
        {
            if (employee.TechnologyLevels?.TryGetValue(technology.SourceIndex,out int level)!=true) continue;
            AddHeading($"{TechnologyName(technology)} — {Tr("LEVEL")} {level}",18);
            if(technology.Identifier==5 && level==3)
            {
                var target=TechnologyApplicationService.LevelThreeStressTarget(state);
                _content.AddChild(new Label {Name="StressReliefSourceTarget",AutowrapMode=TextServer.AutowrapMode.WordSmart,
                    Text=target is null ? Tr("STRESS_RELIEF_MAPPING_UNKNOWN") : string.Format(Tr("STRESS_RELIEF_SLOT_ZERO"),EmployeeName(target))});
            }
            if (employee.Office?.Training?.TechnologySourceIndex==technology.SourceIndex)
            {
                AddHeading($"{Tr("TRAINING_PROGRESS")}: {employee.Office.Training.Progress} / {technology.RequiredTrainingWork[level]}",16);
                AddHeading(Tr("TRAINING_RESUME_NOTE"),16);
            }
            if (level>=3) continue;
            var button=new Button {Name=$"TrainTechnology{technology.SourceIndex}",
                Text=$"{Tr("TRAINING")} {Tr("LEVEL")} {level+1} — {technology.ExperienceCosts[level]} {Tr("EXPERIENCE")}",
                Disabled=TrainingService.CheckStart(state,employee,technology)!=TrainingResult.Success};
            _content.AddChild(button);
            button.Pressed+=()=>
            {
                var result=TrainingService.TryStart(state,employee,technology);
                if (result==TrainingResult.Success) ShowOfficeReconstruction(state);
                else {ShowTraining(state,employee);AddHeading(Tr($"TRAINING_RESULT_{result}"),16);}
            };
        }
        AddHeading(Tr("TRAINING_NOTE"),16);
        AddSaveActions(state);
        AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));
    }
    private void ShowTechnologyUse(GameState state,EmployeeState employee)
    {
        ClearContent();AddHeading(Tr("USE_TECHNOLOGY"),28);AddEmployeeCard(employee);
        AddHeading($"{Tr("EXPERIENCE")}: {employee.Experience:0.0}",18);
        var catalog=_importer.LoadCatalog();
        if (catalog is not null)
        foreach (var technology in catalog.Technologies)
        {
            if (employee.TechnologyLevels?.TryGetValue(technology.SourceIndex,out int level)!=true || level<1) continue;
            var result=TechnologyApplicationService.CheckUse(state,employee,technology);
            AddHeading($"{TechnologyName(technology)} — {Tr("LEVEL")} {level}",18);
            if (technology.Identifier==4)
            {
                _content.AddChild(new Label {Name="SpeedTechnologyNote",Text=Tr("SPEED_TECHNOLOGY_NOTE"),AutowrapMode=TextServer.AutowrapMode.WordSmart});
                if(employee.Office is {WorkTier:4,WorkTicks:>0})
                    _content.AddChild(new Label {Name="SpeedTechnologyInterruptedWork",Text=Tr("SPEED_TECHNOLOGY_INTERRUPTS_APPLICATION"),AutowrapMode=TextServer.AutowrapMode.WordSmart});
            }
            if(technology.Identifier==5 && level==3)
            {
                var target=TechnologyApplicationService.LevelThreeStressTarget(state);
                _content.AddChild(new Label {Name="StressReliefSourceTarget",AutowrapMode=TextServer.AutowrapMode.WordSmart,
                    Text=target is null ? Tr("STRESS_RELIEF_MAPPING_UNKNOWN") : string.Format(Tr("STRESS_RELIEF_SLOT_ZERO"),EmployeeName(target))});
            }
            if (technology.RawTable1568.Count!=3) continue;
            var button=new Button {Name=$"UseTechnology{technology.SourceIndex}",
                Text=$"{Tr("USE_TECHNOLOGY")} — {technology.RawTable1568[level-1]} {Tr("EXPERIENCE")}",
                Disabled=result!=TechnologyApplicationResult.Success};
            _content.AddChild(button);
            if (result!=TechnologyApplicationResult.Success) AddHeading(Tr($"TECHNOLOGY_USE_{result}"),16);
            button.Pressed+=()=>
            {
                var outcome=TechnologyApplicationService.TryUse(state,employee,technology);
                if (outcome==TechnologyApplicationResult.Success) ShowOfficeReconstruction(state);
                else {ShowTechnologyUse(state,employee);AddHeading(Tr($"TECHNOLOGY_USE_{outcome}"),16);}
            };
        }
        AddHeading(Tr("TECHNOLOGY_USE_NOTE"),16);
        if (employee.Office?.TechnologyApplication is TechnologyApplicationState active)
        {
            if(active.IsSpeedBoost)
                _content.AddChild(new Label {Name="SpeedTechnologyRemaining",Text=string.Format(Tr("SPEED_TECHNOLOGY_REMAINING"),employee.Office.SpeedBoostTicks*OfficeStaffRules.TickSeconds)});
            else AddHeading($"{Tr("TECHNOLOGY_REMAINING_WORK")}: {active.RemainingUnits}",16);
        }
        AddAction("ASSIGN_DESK",()=>ShowDeskAssignment(state,employee));
        AddSaveActions(state);
        AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));
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
