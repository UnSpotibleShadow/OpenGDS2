namespace OpenGDS2.Domain;

/// <summary>fGMCrossR 009CAFCC +065F..09F2. Cosmetic/comment RNG is not reproduced.</summary>
public static class ReviewService
{
    public static double Basis(ReleasedGameState product,OriginalCatalog catalog)
    {
        DevelopmentWorkRules.ValidateQuality(product.Quality);
        var q=product.Quality;
        var type=catalog.GameTypes.Single(x=>x.SourceIndex==product.GameTypeSourceIndex);
        return (q.Fun+q.Novelty+q.Graphics+q.Music)*product.MarketFactor*(.7+type.RawMembers["12EC"]/10.0);
    }
    public static int Score(double basis,double marketFactor,Random random)
    {
        if (!double.IsFinite(basis) || !double.IsFinite(marketFactor) || marketFactor<0)
            throw new ArgumentOutOfRangeException(nameof(basis));
        int score=basis switch
        {
            <130=>1+random.Next(3),<200=>2+random.Next(3),<250=>3+random.Next(3),
            <300=>4+random.Next(3),<350=>5+random.Next(3),<400=>6+random.Next(3),
            <450=>7+random.Next(2),<800=>8+random.Next(2),_=>8+random.Next(3)
        };
        // The first draw still happens. This is a replacement, not a bonus.
        if (basis>=400 && marketFactor>=2) score=7+random.Next(basis<800 ? 3 : 4);
        return Math.Min(10,score);
    }
    public static void Generate(ReleasedGameState product,OriginalCatalog catalog,Random random)
    {
        if (product.ReviewScores is not null) throw new InvalidOperationException("Reviews already exist.");
        double basis=Basis(product,catalog);
        product.ReviewScores=Enumerable.Range(0,4).Select(_=>Score(basis,product.MarketFactor,random)).ToList();
        product.ReviewPending=true;
    }
    public static bool Acknowledge(GameState state,ReleasedGameState product)
    {
        if (!state.ReleasedGames.Contains(product) || !product.ReviewPending || product.ReviewScores is null || ShipmentDialogueRules.HasPending(product)) return false;
        bool hallOfFame=product.ReviewScores.Sum()>=32;
        int? count=hallOfFame && state.Company.HallOfFameCount is int current ? checked(current+1) : state.Company.HallOfFameCount;
        product.HallOfFame=hallOfFame;product.ReviewPending=false;state.Company.HallOfFameCount=count;
        return true;
    }
}
