using System.Text.Json.Serialization;

namespace OpenGDS2.Domain;

public sealed class RivalProduct
{
    [JsonRequired] public string Title {get;set;}=string.Empty;
    [JsonRequired] public int Owner {get;set;}
    public int? OwnProduct {get;set;}
    [JsonRequired] public int Hardware {get;set;}
    [JsonRequired] public long UnitsSold {get;set;}
    [JsonRequired] public int Demand {get;set;}
    [JsonRequired] public int SalesPower {get;set;}
    public RivalProduct Copy()=>new() {Title=Title,Owner=Owner,OwnProduct=OwnProduct,Hardware=Hardware,UnitsSold=UnitsSold,Demand=Demand,SalesPower=SalesPower};
}
public sealed class RivalReleaseDefinition
{
    public int TriggerPlatform {get;set;}
    public int Age {get;set;}
    public RivalProduct Product {get;set;}=new();
}
public sealed class CompetitionState
{
    [JsonRequired] public List<RivalProduct> Products {get;set;}=[];
    [JsonRequired] public List<int> PlatformAges {get;set;}=[];
}
public static class CompetitionService
{
    public static CompetitionState Opening(OriginalCatalog catalog)=>new()
    {Products=catalog.InitialRivalProducts.Select(p=>p.Copy()).ToList(),PlatformAges=Enumerable.Repeat(0,8).ToList()};
    public static void AdvanceWeek(GameState state,OriginalCatalog catalog)
    {
        var c=state.Competition;if (c is null || state.Calendar is null) return;
        for (int platform=0;platform<8;platform++)
        {
            if (state.Calendar.HardwareStage<platform) continue;
            int age=c.PlatformAges[platform];
            c.Products.AddRange(catalog.StagedRivalProducts.Where(p=>p.TriggerPlatform==platform && p.Age==age).Select(p=>p.Product.Copy()));
            c.PlatformAges[platform]=checked(age+1);
        }
    }
    public static void AppendOwn(GameState state,ReleasedGameState product)
    {
        state.Competition?.Products.Add(new() {Title=product.Title,Owner=-1,OwnProduct=product.ProductIndex,Hardware=0,UnitsSold=0,Demand=0,SalesPower=0});
    }
    public static long RivalSales(RivalProduct product,double market,int noise)
    {
        if (noise is <0 or >19 || !double.IsFinite(market) || market<0) throw new ArgumentOutOfRangeException(nameof(noise));
        // Module2 +09A1..0A2E: floor before noise and again after market scaling.
        return checked((long)Math.Floor((Math.Floor(product.SalesPower/2.0*product.Demand*product.Demand/700.0)+noise)*(1+market/80)));
    }
    public static void SellRivals(GameState state,OriginalCatalog catalog,Random random)
    {
        if (state.Competition is not CompetitionState c) return;
        foreach (var product in c.Products.Where(p=>p.Demand>0))
        {
            double market=state.Calendar?.Hardware.Single(p=>p.SourceIndex==product.Hardware).Market ?? catalog.Hardware.Single(p=>p.SourceIndex==product.Hardware).RawMembers["1050"];
            long sales=RivalSales(product,market,random.Next(20));
            long units=checked(product.UnitsSold+sales);
            product.Demand=Math.Max(0,product.Demand-2);product.UnitsSold=units;
        }
    }
    public static IReadOnlyList<int> Chart(GameState state)
    {
        if (state.Competition is not CompetitionState c) return [];
        // Insertion sort only swaps strictly greater sales: ties retain append order.
        return c.Products.Select((p,i)=>(p,i)).OrderByDescending(x=>x.p.UnitsSold).Select(x=>x.i).Take(100).ToList();
    }
    public static void UpdateOwnChart(GameState state)
    {
        if (state.Competition is not CompetitionState c) return;
        foreach (var p in c.Products.Where(p=>p.OwnProduct is not null)) p.UnitsSold=state.ReleasedGames.Single(g=>g.ProductIndex==p.OwnProduct).UnitsSold;
        if (Chart(state).FirstOrDefault(-1) is int first && first>=0 && c.Products[first].Owner==-1)
            RankingService.Queue(state,RankingKind.NumberOneSales);
    }
    public static List<int> DrawContestEntries(GameState state,Random random)
    {
        var products=state.Competition!.Products;
        // The original multiplies by count-1, excluding the newest catalog row.
        if (products.Count<2 || !products.Take(products.Count-1).Any(p=>p.Owner>=0)) throw new InvalidOperationException("No eligible rival contest pool.");
        var entries=new List<int>();
        for (int attempts=0;entries.Count<31 && attempts<100_000;attempts++)
        {int id=random.Next(products.Count-1);if (products[id].Owner>=0) entries.Add(id);}
        if (entries.Count!=31) throw new InvalidOperationException("Rival contest lottery stalled.");
        return entries;
    }
    public static void Validate(GameState state)
    {
        var c=state.Competition;if (c is null) return;
        if (c.PlatformAges is null || c.PlatformAges.Count!=8 || c.PlatformAges.Any(a=>a<0) || c.Products is null || c.Products.Any(p=>p is null ||
            string.IsNullOrWhiteSpace(p.Title) || p.Owner is <-1 or >9 || p.Hardware is <0 or >10 || p.Demand<0 || p.SalesPower<0 ||
            (p.Owner==-1)!=(p.OwnProduct is not null) || p.OwnProduct is int id && !state.ReleasedGames.Any(g=>g.ProductIndex==id && g.Title==p.Title)) ||
            !c.Products.Where(p=>p.Owner==-1).Select(p=>p.OwnProduct!.Value).Order().SequenceEqual(state.ReleasedGames.Select(p=>p.ProductIndex).Order()))
            throw new InvalidDataException("Invalid rival catalog or own-product links.");
    }
}
