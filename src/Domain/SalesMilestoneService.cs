namespace OpenGDS2.Domain;

/// <summary>Module2 009E3830 +1121..1241 and fGMOuri 009B4F34.</summary>
public static class SalesMilestoneService
{
    public static long Threshold(int stage)=>stage switch
    {1=>100_000,2=>250_000,3=>500_000,4=>1_000_000,_=>throw new ArgumentOutOfRangeException(nameof(stage))};
    public static void QueueReached(ReleasedGameState product)
    {
        // Legacy saves do not establish the original announcement counter.
        if (product.SalesMilestoneStage is not int stage) return;
        int next=stage+product.PendingSalesMilestones.Count+1;
        while (next<=4 && product.UnitsSold>=Threshold(next)) product.PendingSalesMilestones.Add(next++);
    }
    public static bool Acknowledge(GameState state,ReleasedGameState product)
    {
        if (!state.ReleasedGames.Contains(product) || product.PendingSalesMilestones.Count==0) return false;
        int stage=product.PendingSalesMilestones[0];
        if (stage>=2) RankingService.Queue(state,(RankingKind)(stage-2));
        product.SalesMilestonePage=0;product.SalesMilestoneStage=stage;product.PendingSalesMilestones.RemoveAt(0);
        return true;
    }
    public static bool HasPending(GameState state)=>state.ReleasedGames.Any(x=>x.PendingSalesMilestones.Count>0);
}
