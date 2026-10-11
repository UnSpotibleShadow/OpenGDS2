namespace OpenGDS2.Domain;

/// <summary>
/// Monthly arithmetic: 009DB4F4 +06B7..0770. Staff-list scratch writer:
/// 009B0B60 +0051..00AA. The technology-capacity roster also captures its count;
/// ordinary purchase/training/use screens preserve this shared snapshot.
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

    // Explicit uncertainty guard for callers with genuinely incomplete evidence.
    // The capacity-facility selector has a recovered count writer; ordinary
    // technology screens have no established scratch write and must preserve it.
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
