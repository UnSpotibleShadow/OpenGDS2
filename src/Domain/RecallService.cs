namespace OpenGDS2.Domain;

public enum RecallEvent { Investigation, Result }
public sealed class ProductRecallState
{
    public int OriginalState {get;set;}
    public int WeeksRemaining {get;set;}
    public RecallEvent? PendingEvent {get;set;}
    public int? OutcomeRoll {get;set;}
    // Modern saved result/accounting projection; may be signed, like original sales.
    public long? RefundAmount {get;set;}
}

/// <summary>One-time late-demand check and five-week consumer investigation.</summary>
public static class RecallService
{
    public static bool HasPending(GameState state)=>state.ReleasedGames.Any(x=>x.Recall?.PendingEvent is not null);
    public static bool CheckAfterSales(ReleasedGameState product,Random random)
    {
        var recall=product.Recall;
        if (recall is null || !RecallRules.CheckEligible(product.Demand,recall.OriginalState)) return false;
        // +130C marks the product checked before all three non-short-circuit draws.
        recall.OriginalState=1;
        if (!RecallRules.Investigate(product.Bugs,random)) return false;
        recall.WeeksRemaining=RecallRules.InvestigationWeeks;
        recall.PendingEvent=RecallEvent.Investigation;
        return true;
    }
    public static void AdvanceWeek(GameState state,Random random)
    {
        foreach (var product in state.ReleasedGames.OrderBy(x=>x.ProductIndex))
            AdvanceProductWeek(state,product,random);
    }
    internal static void AdvanceProductWeek(GameState state,ReleasedGameState product,Random random)
    {
            var recall=product.Recall;
            if (recall is null || recall.OriginalState!=1 || recall.WeeksRemaining<1 || recall.PendingEvent is not null) return;
            int next=RecallRules.NextCountdown(recall.WeeksRemaining);
            if (next>0) {recall.WeeksRemaining=next;return;}
            if (state.Money is null) return; // Do not invent an unknown financial result.
            int outcome=random.Next(4);
            long refund=RecallRules.Refund(product.UnitsSold,product.Stock,product.Price,outcome);
            var finance=FinancialRules.Prepare(state,checked(-refund));
            recall.WeeksRemaining=0;recall.OutcomeRoll=outcome;recall.RefundAmount=refund;
            recall.OriginalState=outcome==3 ? 3 : 2;
            finance.Commit(state);
            if (outcome!=3) product.Stock=0;
            recall.PendingEvent=RecallEvent.Result;
    }
    public static bool Acknowledge(GameState state,ReleasedGameState product)
    {
        if (!state.ReleasedGames.Contains(product) || product.Recall?.PendingEvent is null) return false;
        product.Recall.PendingEvent=null;return true;
    }
    public static void Validate(ReleasedGameState product)
    {
        var r=product.Recall;
        if (r is null) return;
        if (r.OriginalState is <0 or >3 || r.WeeksRemaining is <0 or >RecallRules.InvestigationWeeks ||
            r.PendingEvent is not null && !Enum.IsDefined(r.PendingEvent.Value) || r.OutcomeRoll is <0 or >3 ||
            r.OriginalState==0 && (r.WeeksRemaining!=0 || r.PendingEvent is not null) ||
            r.OriginalState<=1 && (r.OutcomeRoll is not null || r.RefundAmount is not null) ||
            r.PendingEvent==RecallEvent.Investigation && (r.OriginalState!=1 || r.WeeksRemaining!=5) ||
            r.PendingEvent==RecallEvent.Result && r.OriginalState<2 ||
            r.OriginalState>=2 && (r.WeeksRemaining!=0 || r.OutcomeRoll is null || r.RefundAmount is null) ||
            r.OriginalState==2 && (r.OutcomeRoll==3 || product.Stock!=0) ||
            r.OriginalState==3 && (r.OutcomeRoll!=3 || r.RefundAmount!=0))
            throw new InvalidDataException("Invalid product recall state.");
    }
}
