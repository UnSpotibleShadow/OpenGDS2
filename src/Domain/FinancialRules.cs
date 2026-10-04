namespace OpenGDS2.Domain;

/// <summary>Signed accumulator 0097DD04; reset cells 22C/23C/24C.</summary>
public static class FinancialRules
{
    public readonly record struct Change(long Money,long? WeeklyNet,long? MonthlyNet,long? YearlyNet)
    {
        public void Commit(GameState state)
        {
            state.Money=Money;state.Company.WeeklyNet=WeeklyNet;
            state.Company.MonthlyNet=MonthlyNet;state.Company.YearlyNet=YearlyNet;
        }
    }

    // Resolve every checked sum before callers change gates, inventory or projects.
    public static Change Prepare(GameState state,long delta)
    {
        if (state.Money is not long money) throw new InvalidOperationException("Company funds are unknown.");
        return new(checked(money+delta),Add(state.Company.WeeklyNet),
            Add(state.Company.MonthlyNet),Add(state.Company.YearlyNet));
        long? Add(long? amount)=>amount is long known ? checked(known+delta) : null;
    }
    public static void Apply(GameState state,long delta)=>Prepare(state,delta).Commit(state);

    /// <summary>Isolated weekly reset ordering; the full calendar caller remains unresolved.</summary>
    public static void ResetAtWeekEnd(GameState state,GameDate closingDate)
    {
        CalendarRules.Validate(closingDate);
        state.Company.WeeklyNet=0;
        if (closingDate.Week==4)
        {
            state.Company.MonthlyNet=0;
            if (closingDate.Month==12) state.Company.YearlyNet=0;
        }
    }
}
