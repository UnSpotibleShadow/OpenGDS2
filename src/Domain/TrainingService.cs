namespace OpenGDS2.Domain;

public enum TrainingResult { Success, NotEmployed, StateUnknown, NotAtStudySeat, Unavailable, InsufficientExperience, CapacityReached }

public sealed class EmployeeTrainingState
{
    public int TechnologySourceIndex { get; set; }
    public int Progress { get; set; }
}

/// <summary>Study acceptance 00993A2C and main loop 00A0283C +410C..4717.</summary>
public static class TrainingService
{
    public static TrainingResult CheckStart(GameState state,EmployeeState employee,TechnologyDefinition technology)
    {
        Validate(technology);
        if (!state.Employees.Contains(employee)) return TrainingResult.NotEmployed;
        if (employee.Office is not OfficeEmployeeState office || employee.TechnologyLevels is null ||
            employee.MaximumTechnologies is null || employee.Experience is null || state.TechnologyOwnershipLevels is null ||
            !state.TechnologyOwnershipLevels.TryGetValue(technology.SourceIndex,out int ownership) ||
            !employee.TechnologyLevels.TryGetValue(technology.SourceIndex,out int level)) return TrainingResult.StateUnknown;
        if (office.Action is <1 or >5) return TrainingResult.NotAtStudySeat;
        if (level>=3 || ownership<=level) return TrainingResult.Unavailable;
        if (employee.Experience<technology.ExperienceCosts[level]) return TrainingResult.InsufficientExperience;
        int distinct=employee.TechnologyLevels.Count(x=>x.Value>=1)-(level>=1 ? 1 : 0);
        if (distinct>=employee.MaximumTechnologies) return TrainingResult.CapacityReached;
        return TrainingResult.Success;
    }
    public static TrainingResult TryStart(GameState state,EmployeeState employee,TechnologyDefinition technology)
    {
        var result=CheckStart(state,employee,technology);
        if (result!=TrainingResult.Success) return result;
        // Resolve malformed duplicate source rows before spending experience.
        _=state.EmployeeSources.SingleOrDefault(x=>x.SourceIndex==employee.SourceIndex);
        int level=employee.TechnologyLevels![technology.SourceIndex];
        employee.Experience-=technology.ExperienceCosts[level];
        employee.Office!.Training=new() {TechnologySourceIndex=technology.SourceIndex};
        SynchronizeSource(state,employee);
        return TrainingResult.Success;
    }
    public static bool Tick(GameState state,EmployeeState employee,OriginalCatalog catalog,Random random)
    {
        var office=employee.Office;
        if (office?.Training is not EmployeeTrainingState training || office.Action is <1 or >5 ||
            office.MovementState!=0 || employee.Stress is not double stress || stress>=100 ||
            employee.DevelopmentSpeed is not int speed || employee.TechnologyLevels is null ||
            !employee.TechnologyLevels.TryGetValue(training.TechnologySourceIndex,out int level) || level>=3) return false;
        var technology=catalog.Technologies.Single(x=>x.SourceIndex==training.TechnologySourceIndex);Validate(technology);
        if (office.ActionTicks==14) office.Frame=14+3*(office.Facing-1);
        if (office.ActionTicks>=28) {office.Frame=15+3*(office.Facing-1);office.ActionTicks=0;}
        office.ActionTicks++;
        office.TrainingIdleTicks=checked(office.TrainingIdleTicks+1);
        if (office.TrainingIdleTicks>=2+(100-speed)/10.0)
        {
            training.Progress=checked(training.Progress+(random.Next(5)+3)/2);
            office.TrainingIdleTicks=0;
        }
        employee.Stress=Math.Min(100,stress+(random.Next(3)+1)/30.0);
        if (training.Progress<technology.RequiredTrainingWork[level]) return false;
        training.Progress=technology.RequiredTrainingWork[level];
        employee.TechnologyLevels[technology.SourceIndex]=level+1;
        office.Training=null;
        SynchronizeSource(state,employee);
        return true;
    }
    public static void SynchronizeSource(GameState state,EmployeeState employee)
    {
        var source=state.EmployeeSources.SingleOrDefault(x=>x.SourceIndex==employee.SourceIndex);
        if (source is null) return;
        source.Employee.Experience=employee.Experience;
        source.Employee.TechnologyLevels=employee.TechnologyLevels is null ? null : new(employee.TechnologyLevels);
    }
    private static void Validate(TechnologyDefinition technology)
    {
        if (technology.SourceIndex is <0 or >7 || technology.RequiredTrainingWork.Count!=3 || technology.ExperienceCosts.Count!=3 ||
            technology.RequiredTrainingWork.Any(x=>x<=0) || technology.ExperienceCosts.Any(x=>x<0))
            throw new InvalidDataException("Incomplete imported training tables. Reimport the original distribution.");
    }
}
