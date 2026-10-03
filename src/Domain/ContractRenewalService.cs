namespace OpenGDS2.Domain;

public enum ContractRenewalResult { Success, MoneyUnknown, FeeUnknown, InsufficientFunds }

/// <summary>
/// Verified employee/finance projection of fGMSKoyouKz.Command1.
/// Dismissal clears supported workspace projections. Calendar scheduling and
/// training/external task cleanup remain separate reconstruction work.
/// </summary>
public static class ContractRenewalService
{
    // The original tick tests month 3 / temporary week 5 after incrementing the week,
    // before its four-week month rollover. Modern dates never store that temporary 5.
    public static bool IsDueAtEndOfWeek(GameDate date)
    {
        CalendarRules.Validate(date);
        return date.Month == 3 && date.Week == 4;
    }

    // Selected IDs are employees whose checkboxes remain checked. The form loads
    // all employed source rows in source-index order, independent of hiring order.
    public static ContractRenewalResult TryRenew(GameState state, IReadOnlySet<int> selectedSourceIds)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(selectedSourceIds);
        if (state.Employees.Any(x => x.SourceIndex is null) ||
            state.Employees.Select(x => x.SourceIndex).Distinct().Count() != state.Employees.Count)
            throw new InvalidDataException("Renewal requires unique original employee identities.");
        var roster = state.Employees.OrderBy(x => x.SourceIndex).ToArray();
        if (selectedSourceIds.Any(id => !roster.Any(x => x.SourceIndex == id)))
            throw new ArgumentException("Selection contains a non-employed source.", nameof(selectedSourceIds));
        // Resolve source records before any mutations, including duplicate-data errors.
        var sources = roster.ToDictionary(x => x.SourceIndex!.Value,
            x => state.EmployeeSources.SingleOrDefault(s => s.SourceIndex == x.SourceIndex));
        long total = 0;
        foreach (var employee in roster.Where(x => selectedSourceIds.Contains(x.SourceIndex!.Value)))
        {
            if (employee.ContractFee is null) return ContractRenewalResult.FeeUnknown;
            if (employee.ContractFee < 0) throw new InvalidDataException("Invalid employee contract fee.");
            total = checked(total + employee.ContractFee.Value);
        }
        if (state.Money is null) return ContractRenewalResult.MoneyUnknown;
        if (state.Money < total) return ContractRenewalResult.InsufficientFunds;

        foreach (var employee in roster.Where(x => !selectedSourceIds.Contains(x.SourceIndex!.Value)))
        {
            EmployeeDismissalService.DismissResolved(state,employee,sources[employee.SourceIndex!.Value]);
        }
        state.Money -= total;
        return ContractRenewalResult.Success;
    }
}
