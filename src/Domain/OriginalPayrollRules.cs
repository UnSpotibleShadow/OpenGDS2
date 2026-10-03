namespace OpenGDS2.Domain;

/// <summary>009DB4F4 +06B7..0770 arithmetic. Scratch-state scheduling is unresolved.</summary>
public static class OriginalPayrollRules
{
    public static long? MonthlyDebit(IReadOnlyList<RecruitmentSourceRecord> sources,int? originalScratchIndex)
    {
        int employed=sources.Count(x=>x.GateValue==1);
        if (employed==0) return 0;
        if (originalScratchIndex is null) return null;
        if (originalScratchIndex<0) throw new ArgumentOutOfRangeException(nameof(originalScratchIndex));
        // The source repeatedly reads BDC[scratch], never BDC[loop index].
        var row=sources.SingleOrDefault(x=>x.SourceIndex==originalScratchIndex);
        if (row?.Employee.RawParameter40 is not long amount) return null;
        return checked(amount*employed);
    }
}
