namespace OpenGDS2.Domain;

public enum ProjectCancellationResult { Success, ProjectUnavailable, DecisionPending, StateUnknown }

/// <summary>fGMDevW1/W2 BGraphic index 6: discard the project and stress its assigned staff.</summary>
public static class ProjectCancellationService
{
    public static ProjectCancellationResult Check(GameState state,DevelopmentProjectState project)
    {
        if (!state.DevelopmentProjects.Contains(project) || project.RoomIndex is <0 or >1 ||
            project.OriginalRoomState!=1 || project.OriginalMode is not (0 or 1 or 2))
            return ProjectCancellationResult.ProjectUnavailable;
        if (project.PendingDecision.HasValue || CheckpointMeetingRules.HasPending(project))
            return ProjectCancellationResult.DecisionPending;
        // An absent legacy workspace cannot establish which room gets the penalty.
        if (state.Employees.Any(x=>x.Office is null) || Assigned(state,project).Any(x=>
            x.Stress is not double stress || !double.IsFinite(stress) || stress<0))
            return ProjectCancellationResult.StateUnknown;
        return ProjectCancellationResult.Success;
    }

    public static ProjectCancellationResult TryCancel(GameState state,DevelopmentProjectState project)
    {
        var result=Check(state,project);
        if (result!=ProjectCancellationResult.Success) return result;
        // Resolve source identities before any mutation. Modern legacy saves may
        // have no recruitment source rows; known employed stress remains usable.
        var affected=Assigned(state,project).Select(employee=>(Employee:employee,
            Source:employee.SourceIndex is int sourceIndex
                ? state.EmployeeSources.SingleOrDefault(x=>x.SourceIndex==sourceIndex) : null)).ToList();
        foreach (var (employee,source) in affected)
        {
            // Original +06E2..073F (room 1): +20, then cap at 99, including 100 -> 99.
            employee.Stress=Math.Min(99,employee.Stress!.Value+20);
            if (source is not null) source.Employee.Stress=employee.Stress;
        }
        state.DevelopmentProjects.Remove(project);
        // Project removal represents the original room-state-zero write. Clear
        // its seated work/application now so immediate reuse cannot inherit it.
        // Stress uses assigned Action; outstanding work uses SeatedAction.
        foreach (var employee in state.Employees.Where(x=>BelongsToRoom(x.Office!.SeatedAction,project.RoomIndex)))
        {
            employee.Office!.TechnologyApplication=null;
            ReleaseService.ResetWork(employee.Office);
        }
        return ProjectCancellationResult.Success;
    }

    private static IEnumerable<EmployeeState> Assigned(GameState state,DevelopmentProjectState project)
        =>state.Employees.Where(x=>x.Office is not null && BelongsToRoom(x.Office.Action,project.RoomIndex));
    private static bool BelongsToRoom(int action,int room)=>action>=6+room*6 && action<=11+room*6;
}
