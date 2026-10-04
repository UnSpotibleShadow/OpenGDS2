namespace OpenGDS2.Domain;

/// <summary>Module2 009E3830 +0948..142D own-product sales, at twenty office ticks.</summary>
public static class SalesService
{
    public const int TicksPerBatch=20;
    public static double PriceMultiplier(long price)=>price switch
    {
        <3000=>2,<4000=>1.5,<5000=>1.2,<6000=>1,<7000=>.7,
        <8000=>.5,<9000=>.2,<10000=>.1,<20000=>.05,_=>0
    };
    public static long PotentialSales(ReleasedGameState product,OriginalCatalog catalog,int noise)
    {
        if (noise is <0 or >19) throw new ArgumentOutOfRangeException(nameof(noise));
        DevelopmentWorkRules.ValidateQuality(product.Quality);
        if (product.Demand<=0) return 0;
        var hardware=catalog.Hardware.Single(x=>x.SourceIndex==product.HardwareSourceIndex);
        var type=catalog.GameTypes.Single(x=>x.SourceIndex==product.GameTypeSourceIndex);
        int audience=catalog.Audiences.Single(x=>x.SourceIndex==product.AudienceSourceIndex).Identifier;
        double audienceFactor=audience switch {1=>1,2=>.9,3=>.8,4=>.75,5=>.7,6=>.6,7=>.5,_=>throw new InvalidDataException("Invalid audience.")};
        var q=product.Quality;
        double sales=Math.Floor((q.Fun*1.6+q.Novelty*1.2+q.Music*.8+q.Graphics*.7-product.Bugs)/2*
            ((double)product.Demand*product.Demand/800))+noise;
        sales*=1+hardware.RawMembers["1050"]/80.0;
        sales*=1+product.Recognition/40;
        sales*=product.MarketFactor;
        sales*=audienceFactor;
        sales*=.7+type.RawMembers["12EC"]/10.0;
        sales*=PriceMultiplier(product.Price);
        // No lower clamp in the original. Negative demand results are retained,
        // including the rare low-quality/high-bug stock/revenue defect.
        return checked((long)Math.Floor(sales));
    }
    public static int NextDemand(int current,int firstRoll,int? secondRoll=null)
    {
        if (current<0 || firstRoll is <0 or >49 || secondRoll is <0 or >6) throw new ArgumentOutOfRangeException(nameof(current));
        if (firstRoll==0) return current;
        if (secondRoll is null) throw new ArgumentException("A second demand draw is required.");
        return Math.Max(0,checked(current+(secondRoll==0 ? 2 : -2)));
    }
    public static void Tick(GameState state,OriginalCatalog catalog,Random random)
    {
        // Unknown legacy fees must not silently become zero.
        if (state.Money is null || state.SalesFee1 is null || state.SalesFee2 is null ||
            state.ReleasedGames.Any(x=>x.ReviewPending) || SalesMilestoneService.HasPending(state) || RecallService.HasPending(state)) return;
        if (state.SalesTicks+1>=TicksPerBatch)
            foreach (var product in state.ReleasedGames)
                if (product.WeeklySalesHistory is not null) SalesHistoryRules.Validate(product.WeeklySalesHistory);
        if (++state.SalesTicks<TicksPerBatch) return;
        state.SalesTicks=0;
        foreach (var product in state.ReleasedGames.OrderBy(x=>x.ProductIndex))
        {
            long sales=product.Stock>0 && product.Demand>0 ? Math.Min(product.Stock,PotentialSales(product,catalog,random.Next(20))) : 0;
            long receipts=checked(checked(product.Price-state.SalesFee1.Value-state.SalesFee2.Value)*sales);
            long stock=checked(product.Stock-sales),units=checked(product.UnitsSold+sales);
            var finance=FinancialRules.Prepare(state,receipts);
            long net=checked(product.NetSalesReceipts+receipts);
            long? weekly=product.WeeklySalesHistory is not null ? checked(product.WeeklySalesHistory[0]+sales) : null;
            int first=random.Next(50);
            int demand=NextDemand(product.Demand,first,first==0 ? null : random.Next(7));
            finance.Commit(state);product.Stock=stock;product.UnitsSold=units;
            product.NetSalesReceipts=net;product.LastBatchSold=sales;product.Demand=demand;
            if (weekly is long unitsThisWeek) product.WeeklySalesHistory![0]=unitsThisWeek;
            SalesMilestoneService.QueueReached(product);
            RecallService.CheckAfterSales(product,random);
        }
    }
}
