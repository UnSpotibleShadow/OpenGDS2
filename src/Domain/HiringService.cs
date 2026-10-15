namespace OpenGDS2.Domain;

/// <summary>
/// Evidence-backed portion of the original hire transaction.
///
/// The fGMSKoyou Hire handler first compares company funds with the selected applicant's
/// contract fee. On success it calls the employee-copy helper; that helper also negates the
/// selected applicant's fee and passes it to the shared financial accumulator routine.
/// </summary>
public static class HiringService
{
    public static HireResult TryHire(GameState state, EmployeeState? candidate, Random? random = null)
    {
        if (candidate is null)
            return HireResult.NoActiveCandidate;
        if (candidate.SourceIndex is int id && state.Employees.Any(x => x.SourceIndex == id))
            return HireResult.AlreadyEmployed;
        if (candidate.ContractFee < 0) return HireResult.InvalidContractFee;
        if (state.Money is null)
            return HireResult.MoneyUnknown;
        if (candidate.ContractFee is null)
            return HireResult.ContractFeeUnknown;
        if (state.Money.Value < candidate.ContractFee.Value)
            return HireResult.InsufficientFunds;

        CompleteHire(state,candidate,random ?? Random.Shared);
        return HireResult.Success;
    }

    // The negotiation's native winning branch calls the shared hire helper
    // directly, without the ordinary hiring screen's balance guard.
    internal static void HirePoached(GameState state,EmployeeState candidate,Random random)
    {
        if(candidate.ContractFee is null or <0 || state.Employees.Any(e=>e.SourceIndex==candidate.SourceIndex))
            throw new InvalidOperationException("Invalid poaching hire target.");
        CompleteHire(state,candidate,random);
    }
    private static void CompleteHire(GameState state,EmployeeState candidate,Random random)
    {
        var change=FinancialRules.Prepare(state,-candidate.ContractFee!.Value);
        var employee = CloneEmployee(candidate);
        if (candidate.PortraitId is >= 1 and <= 10)
            employee.Office = OfficeStaffRules.CreateHire(state, random);
        change.Commit(state);
        state.Employees.Add(employee);
        var source = state.EmployeeSources.FirstOrDefault(x => x.SourceIndex == candidate.SourceIndex);
        if (source is not null) source.GateValue = 1;
    }

    private static EmployeeState CloneEmployee(EmployeeState source) => new()
    {
        SourceIndex = source.SourceIndex, PortraitId = source.PortraitId,
        OriginalAttributes = [.. source.OriginalAttributes],
        TechnologyLevels = source.TechnologyLevels is null ? null : new(source.TechnologyLevels),
        RawParameter34 = source.RawParameter34, RawParameter3C = source.RawParameter3C,
        RawParameter40 = source.RawParameter40, SpecialFlag = source.SpecialFlag,
        Name = source.Name,
        Level = source.Level,
        Experience = source.Experience, Stress = source.Stress,
        Fun = source.Fun,
        Novelty = source.Novelty,
        Accuracy = source.Accuracy,
        Graphics = source.Graphics,
        Music = source.Music,
        DevelopmentSpeed = source.DevelopmentSpeed,
        Inspiration = source.Inspiration,
        ContractFee = source.ContractFee,
    };
}
