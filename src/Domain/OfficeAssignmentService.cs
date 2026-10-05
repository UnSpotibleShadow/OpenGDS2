namespace OpenGDS2.Domain;

public enum OfficeAssignmentResult { Success, UnknownWorkspace, Occupied, AlreadyAssigned, NoLoungeSeat, RestRequired }

/// <summary>Study and development seat subset of Module2 10 (009AF6BC).</summary>
public static class OfficeAssignmentService
{
    public static OfficeAssignmentResult TryAssignStudySeat(GameState state,EmployeeState employee,int action)
    {
        if (action is <1 or >5) throw new ArgumentOutOfRangeException(nameof(action));
        return TryAssignSeat(state,employee,action);
    }
    public static OfficeAssignmentResult TryAssignDesk(GameState state,EmployeeState employee,int action)
    {
        if (action is <6 or >17) throw new ArgumentOutOfRangeException(nameof(action));
        return TryAssignSeat(state,employee,action);
    }
    private static OfficeAssignmentResult TryAssignSeat(GameState state, EmployeeState employee, int action)
    {
        if (!state.Employees.Contains(employee) || employee.Office is not OfficeEmployeeState office)
            return OfficeAssignmentResult.UnknownWorkspace;
        // Native assignment click 009C66BC +1055..110E: a work/study request
        // is allowed only at/below the entry threshold, or under voluntary policy.
        if (state.SecretaryPolicies is { } policy && SecretaryPolicyRules.BreakThreshold(policy.Break) is int threshold && employee.Stress>threshold)
            return OfficeAssignmentResult.RestRequired;
        if (office.AutomaticBreak is not null) office.AutomaticBreak=false;
        if (office.Action == action || office.SeatedAction == action) return OfficeAssignmentResult.AlreadyAssigned;
        if (state.Employees.Any(x => x != employee && x.Office?.Action == action))
            return OfficeAssignmentResult.Occupied;
        StartRoute(office, action, null);
        return OfficeAssignmentResult.Success;
    }

    public static OfficeAssignmentResult TryTakeBreak(GameState state, EmployeeState employee, Random random, bool automatic=false)
    {
        if (!state.Employees.Contains(employee) || employee.Office is not OfficeEmployeeState office)
            return OfficeAssignmentResult.UnknownWorkspace;
        if (!automatic && office.AutomaticBreak is not null) office.AutomaticBreak=false;
        if (office.Action is >=18 and <=20) return OfficeAssignmentResult.AlreadyAssigned;
        var occupied = state.Employees.Where(x => x.Office?.LoungeSeat is not null)
            .Select(x => x.Office!.LoungeSeat!.Value).ToHashSet();
        if (occupied.Count == 12) return OfficeAssignmentResult.NoLoungeSeat;
        for (int attempt=0; attempt<14; attempt++)
        {
            int seat=random.Next(12);
            if (occupied.Contains(seat)) continue;
            StartRoute(office,18+seat/4,seat);
            return OfficeAssignmentResult.Success;
        }
        return OfficeAssignmentResult.NoLoungeSeat;
    }

    private static void StartRoute(OfficeEmployeeState office, int action, int? loungeSeat)
    {
        // +0684..071E, +0980..0991: no teleport or reverse standing animation.
        // The shifted seated base coordinate is retained; offsets are cleared.
        office.SeatedAction=0;
        office.WorkEngaged=false;
        office.OffsetX=office.OffsetY=office.DepthAdjustment=0;
        office.LoungeSeat=loungeSeat;
        office.Action=action;
        office.MovementState=3;
        office.ActionTicks=0;
        ReleaseService.ResetWork(office);
    }
}
