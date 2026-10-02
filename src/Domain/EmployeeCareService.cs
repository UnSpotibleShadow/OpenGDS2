namespace OpenGDS2.Domain;

public enum EmployeeCareResult { Success, NoEmployees, MoneyUnknown, InsufficientFunds }

/// <summary>Verified hot-spring transaction; tenant availability is not reconstructed.</summary>
public static class EmployeeCareService
{
    public const long HotSpringCost = 8_000_000;
    public static bool StressWarningEligible(bool employed, double stress) => employed && stress >= 85;

    // Called only after the office/tenant system has established availability.
    // No UI exposes this action while its unlock conditions remain unresolved.
    public static EmployeeCareResult TryHotSpring(GameState state)
    {
        if (state.Employees.Count == 0) return EmployeeCareResult.NoEmployees;
        if (state.Money is null) return EmployeeCareResult.MoneyUnknown;
        if (state.Money < HotSpringCost) return EmployeeCareResult.InsufficientFunds;
        state.Money -= HotSpringCost;
        foreach (var employee in state.Employees) employee.Stress = 0;
        foreach (var source in state.EmployeeSources.Where(x => x.GateValue == 1)) source.Employee.Stress = 0;
        return EmployeeCareResult.Success;
    }
}
