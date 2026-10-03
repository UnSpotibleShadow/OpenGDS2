namespace OpenGDS2.Domain;

public enum TechnologyApplicationResult { Success, NotEmployed, StateUnknown, NotAtDevelopmentDesk, NoProject,
    DecisionPending, NotLearned, InsufficientExperience, UnresolvedBehavior }
public sealed class TechnologyApplicationState
{
    public int TechnologySourceIndex {get;set;}
    public int RemainingUnits {get;set;}
}

/// <summary>fGMInfo 009A98E8 and Module2 00A0283C +66D8..88B0, own-game mode 1.</summary>
public static class TechnologyApplicationService
{
    public static readonly int[] ProjectTechnologyIdentifiers=[0,1,2,3,6,7];
    public static Dictionary<int,int> NewProjectCounters()=>ProjectTechnologyIdentifiers.ToDictionary(x=>x,_=>0);
    public static TechnologyApplicationResult CheckUse(GameState state,EmployeeState employee,TechnologyDefinition technology)
    {
        if (!state.Employees.Contains(employee)) return TechnologyApplicationResult.NotEmployed;
        if (employee.Office is not OfficeEmployeeState office || employee.TechnologyLevels is null || employee.Experience is null ||
            !employee.TechnologyLevels.TryGetValue(technology.SourceIndex,out int level)) return TechnologyApplicationResult.StateUnknown;
        if (office.SeatedAction is <6 or >17) return TechnologyApplicationResult.NotAtDevelopmentDesk;
        var project=state.DevelopmentProjects.SingleOrDefault(x=>x.RoomIndex==(office.SeatedAction-6)/6);
        if (project is null) return TechnologyApplicationResult.NoProject;
        if (project.OriginalRoomState!=1) return TechnologyApplicationResult.UnresolvedBehavior;
        if (level is <1 or >3) return TechnologyApplicationResult.NotLearned;
        if (technology.RawTable1568.Count!=3 || technology.RawTable1568.Any(x=>x<0) ||
            technology.RawTable158C.Count!=3 || technology.RawTable158C.Any(x=>x<=0)) return TechnologyApplicationResult.StateUnknown;
        if (employee.Experience<technology.RawTable1568[level-1]) return TechnologyApplicationResult.InsufficientExperience;
        if (project.PendingDecision.HasValue) return TechnologyApplicationResult.DecisionPending;
        // Do not replace the unresolved speed scratch/lifetime paths or the
        // level-three stress-relief source index with intended behavior.
        if (technology.Identifier==4 || technology.Identifier==5 && level==3) return TechnologyApplicationResult.UnresolvedBehavior;
        if (technology.Identifier==5) return employee.Stress is null ? TechnologyApplicationResult.StateUnknown : TechnologyApplicationResult.Success;
        if (!ProjectTechnologyIdentifiers.Contains(technology.Identifier)) return TechnologyApplicationResult.UnresolvedBehavior;
        return project.TechnologyApplicationCounts?.ContainsKey(technology.Identifier)==true
            ? TechnologyApplicationResult.Success : TechnologyApplicationResult.StateUnknown;
    }
    public static TechnologyApplicationResult TryUse(GameState state,EmployeeState employee,TechnologyDefinition technology)
    {
        var result=CheckUse(state,employee,technology);if (result!=TechnologyApplicationResult.Success) return result;
        _=state.EmployeeSources.SingleOrDefault(x=>x.SourceIndex==employee.SourceIndex);
        int level=employee.TechnologyLevels![technology.SourceIndex];
        employee.Experience-=technology.RawTable1568[level-1];
        if (technology.Identifier==5)
        {
            double reduction=level==1 ? .5 : .8;
            employee.Stress=Math.Floor(employee.Stress!.Value-employee.Stress.Value*reduction);
            var source=state.EmployeeSources.SingleOrDefault(x=>x.SourceIndex==employee.SourceIndex);
            if (source is not null) source.Employee.Stress=employee.Stress;
        }
        else employee.Office!.TechnologyApplication=new() {TechnologySourceIndex=technology.SourceIndex,RemainingUnits=technology.RawTable158C[level-1]};
        TrainingService.SynchronizeSource(state,employee);
        return TechnologyApplicationResult.Success;
    }
    public static int Percentage(int identifier,int learnedLevel,int previousApplications,Random random)
    {
        if (!ProjectTechnologyIdentifiers.Contains(identifier) || learnedLevel is <1 or >3 || previousApplications<0)
            throw new ArgumentOutOfRangeException(nameof(identifier));
        int basis=previousApplications switch {0=>10+10*learnedLevel,1=>learnedLevel switch {1=>5,2=>10,_=>20},_=>0};
        int width=identifier is 1 or 6 or 7 ? 2*learnedLevel+1 : 3;
        return basis+random.Next(width);
    }
    public static void ApplyCompleted(DevelopmentProjectState project,TechnologyDefinition technology,int level,Random random)
    {
        if (project.TechnologyApplicationCounts is null || !project.TechnologyApplicationCounts.TryGetValue(technology.Identifier,out int previous))
            throw new InvalidDataException("Original project technology counters are unknown.");
        int next=checked(previous+1);
        double factor=Percentage(technology.Identifier,level,previous,random)/100.0;
        var q=project.Quality;
        switch (technology.Identifier)
        {
            case 0: project.DataSize=Math.Max(0,Math.Floor(project.DataSize-project.DataSize*factor));break;
            case 2: project.Bugs=checked((int)Math.Floor(project.Bugs-project.Bugs*factor));break;
            case 1: project.Quality=q with {Graphics=Math.Floor(q.Graphics+q.Graphics*factor)};break;
            case 3: project.Quality=q with {Music=Math.Floor(q.Music+q.Music*factor)};break;
            case 6: project.Quality=q with {Fun=Math.Floor(q.Fun+q.Fun*factor)};break;
            case 7: project.Quality=q with {Novelty=Math.Floor(q.Novelty+q.Novelty*factor)};break;
        }
        project.TechnologyApplicationCounts[technology.Identifier]=next;
    }
    public static void TickWork(DevelopmentProjectState project,EmployeeState employee,OriginalCatalog catalog,Random random)
    {
        var office=employee.Office!;var application=office.TechnologyApplication;
        if (application is null || office.WorkTier!=4 || office.WorkTicks<20) return;
        // Shared work counter resets to 1, then the main loop increments it.
        // Subsequent units therefore take 19 ticks.
        office.WorkTicks=1;application.RemainingUnits--;
        if (application.RemainingUnits>0) return;
        var technology=catalog.Technologies.Single(x=>x.SourceIndex==application.TechnologySourceIndex);
        int level=employee.TechnologyLevels![technology.SourceIndex];
        ApplyCompleted(project,technology,level,random);
        office.WorkTicks=0;office.TechnologyApplication=null;
    }
}
