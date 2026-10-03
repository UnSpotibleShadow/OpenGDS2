namespace OpenGDS2.Domain;

/// <summary>
/// Monthly arithmetic: 009DB4F4 +06B7..0770. Staff-list scratch writer:
/// 009B0B60 +0051..00AA. Other scratch writers and live scheduling remain unresolved.
/// </summary>
public static class OriginalPayrollRules
{
    public static long? MonthlyDebit(GameState state)
        => MonthlyDebit(state.EmployeeSources,state.OriginalPayrollScratchIndex);

    public static long? MonthlyDebit(IReadOnlyList<RecruitmentSourceRecord> sources,int? originalScratchIndex)
    {
        int employed=sources.Count(x=>x.GateValue==1);
        if (employed==0) return 0;
        if (originalScratchIndex is null) return null;
        if (originalScratchIndex<0) throw new ArgumentOutOfRangeException(nameof(originalScratchIndex));
        // The source repeatedly reads BDC[scratch], never BDC[loop index].
        var row=sources.SingleOrDefault(x=>x.SourceIndex==originalScratchIndex);
        if (row?.Employee.MonthlySalary is not long amount) return null;
        return checked(amount*employed);
    }

    /// <summary>
    /// The original staff form clears 3018, then increments it for every employed
    /// source. A partial modern source table cannot establish the resulting count.
    /// This is called on entry to the staff screen, never on hire/dismiss/load.
    /// </summary>
    public static bool CaptureStaffList(GameState state)
    {
        if (state.SourceUpperBound is not int upper || upper<=0 ||
            state.EmployeeSources.Count!=upper ||
            !state.EmployeeSources.Select(x=>x.SourceIndex).Order().SequenceEqual(Enumerable.Range(0,upper)))
        {
            state.OriginalPayrollScratchIndex=null;
            return false;
        }
        state.OriginalPayrollScratchIndex=state.EmployeeSources.Count(x=>x.GateValue==1);
        return true;
    }

    // Technology selection also writes 3018, but its complete lifecycle is not mapped.
    // Clear the projection rather than retaining a count from the staff form.
    public static void InvalidateScratch(GameState state) => state.OriginalPayrollScratchIndex=null;

    /// <summary>009B0B60 +00BF sums each employed row's own BDC salary for display.</summary>
    public static long? StaffSalaryTotal(IReadOnlyList<RecruitmentSourceRecord> sources)
    {
        long total=0;
        foreach (var row in sources.Where(x=>x.GateValue==1))
        {
            if (row.Employee.MonthlySalary is not long salary) return null;
            total=checked(total+salary);
        }
        return total;
    }
}
