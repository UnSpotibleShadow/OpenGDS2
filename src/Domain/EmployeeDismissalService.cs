namespace OpenGDS2.Domain;

public enum DismissalResult { Success, NotEmployed, IdentityUnknown }

/// <summary>Own-employee projection of Module1 009981CC. No severance payment.</summary>
public static class EmployeeDismissalService
{
    public static DismissalResult TryDismiss(GameState state,EmployeeState employee)
    {
        if (!state.Employees.Contains(employee)) return DismissalResult.NotEmployed;
        if (employee.SourceIndex is not int sourceIndex) return DismissalResult.IdentityUnknown;
        if (state.Employees.Count(x=>x.SourceIndex==sourceIndex)!=1)
            throw new InvalidDataException("Dismissal requires a unique employee identity.");
        // Resolve duplicate-data errors before changing reservations or gates.
        var source=state.EmployeeSources.SingleOrDefault(x=>x.SourceIndex==sourceIndex);
        DismissResolved(state,employee,source);
        return DismissalResult.Success;
    }
    internal static void DismissResolved(GameState state,EmployeeState employee,RecruitmentSourceRecord? source)
    {
        // Source +0399..03D0 clears employed gate and stress, retaining level/stats.
        employee.Stress=0;
        if (source is not null)
        {
            source.GateValue=0;source.Employee.Stress=0;
            source.Employee.Experience=employee.Experience;
        }
        // Reservations, sprites and pending contributions are projections of
        // employed workspaces. Removing this workspace frees both current and
        // destination seats, including a worker dismissed while walking.
        employee.Office=null;
        state.Employees.Remove(employee);
    }
}
