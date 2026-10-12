namespace OpenGDS2.Domain;

/// <summary>Original product member 2000: current week plus twelve prior weeks.</summary>
public static class SalesHistoryRules
{
    public const int CellCount=13;
    public static List<long> NewHistory()=>Enumerable.Repeat(0L,CellCount).ToList();
    public static void Validate(IReadOnlyList<long> history)
    {
        if (history.Count!=CellCount) throw new InvalidDataException("Original weekly sales history requires thirteen cells.");
        // Signed sales are intentional: the original does not clamp negative units.
    }
    public static void AdvanceWeek(GameState state)
    {
        // Validate every known row before shifting any of them.
        foreach (var product in state.ReleasedGames)
            if (product.WeeklySalesHistory is not null) Validate(product.WeeklySalesHistory);
        foreach (var product in state.ReleasedGames)
        {
            var history=product.WeeklySalesHistory;
            if (history is null) continue; // Missing legacy records cannot be reconstructed from total sales.
            for (int i=CellCount-1;i>0;i--) history[i]=history[i-1];
            history[0]=0;
        }
    }
    public static long ChartCeiling(IReadOnlyList<long> history)
    {
        Validate(history);
        long maximum=0;
        // fGMZishaK scans 0..11, although its twelve segments also draw cell 12.
        // Preserve this original oldest-cell omission rather than auto-fit all cells.
        for (int i=0;i<CellCount-1;i++) maximum=Math.Max(maximum,history[i]);
        return maximum>=100000 ? 200000 : maximum>=30000 ? 80000 : 40000;
    }
    /// <summary>fGMZishaKW 009C4570: the narrow individual-product variant.</summary>
    public static long CompactChartCeiling(IReadOnlyList<long> history)
    {
        Validate(history);
        long maximum=0;
        for(int i=0;i<CellCount-1;i++)maximum=Math.Max(maximum,history[i]);
        return maximum<20000 ? 20000 : maximum<40000 ? 40000 : maximum<100000 ? 80000 : maximum<200000 ? 200000 : 400000;
    }
}
