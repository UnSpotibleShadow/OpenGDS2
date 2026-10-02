namespace OpenGDS2.Domain;

public enum OfficeAssignmentResult { Success, UnknownWorkspace, Occupied, AlreadyAssigned, NoLoungeSeat }

/// <summary>Development-seat subset of Module2 10 (009AF6BC). Training remains separate.</summary>
public static class OfficeAssignmentService
{
    public static OfficeAssignmentResult TryAssignDesk(GameState state, EmployeeState employee, int action)
    {
        if (action is <6 or >17) throw new ArgumentOutOfRangeException(nameof(action));
        if (!state.Employees.Contains(employee) || employee.Office is not OfficeEmployeeState office)
            return OfficeAssignmentResult.UnknownWorkspace;
        if (office.Action == action || office.SeatedAction == action) return OfficeAssignmentResult.AlreadyAssigned;
        if (state.Employees.Any(x => x != employee && x.Office?.Action == action))
            return OfficeAssignmentResult.Occupied;
        StartRoute(office, action, null);
        return OfficeAssignmentResult.Success;
    }

    public static OfficeAssignmentResult TryTakeBreak(GameState state, EmployeeState employee, Random random)
    {
        if (!state.Employees.Contains(employee) || employee.Office is not OfficeEmployeeState office)
            return OfficeAssignmentResult.UnknownWorkspace;
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
    }
}
