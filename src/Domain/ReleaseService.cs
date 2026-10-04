namespace OpenGDS2.Domain;

public enum ReleaseResult { Success, NotReady, DuplicateTitle, MoneyUnknown, InsufficientFunds, AlreadyProducedThisWeek, StateUnknown, Recalled }

/// <summary>fGMSyukka own-game release and fGMSyukkaSai production transactions.</summary>
public static class ReleaseService
{
    public const long DefaultPrice=5800;
    public const long DefaultCopies=5000;
    public static long UnitProductionCost(HardwareDefinition hardware,double dataSize)
    {
        if (!double.IsFinite(dataSize) || dataSize<0 || hardware.ProductionBands.Count!=4)
            throw new InvalidDataException("Original production size/cost bands are required. Reimport the original distribution.");
        int band=3;
        for (int i=0;i<3;i++) if (Math.Floor(dataSize)<=hardware.ProductionBands[i].SizeThreshold) {band=i;break;}
        return checked(hardware.ProductionBands[band].UnitCost+200);
    }
    public static ReleaseResult TryRelease(GameState state,DevelopmentProjectState project,OriginalCatalog catalog,
        string title,long price,long copies,Random random)
    {
        ValidateInput(title,price,copies);
        if (!state.DevelopmentProjects.Contains(project) || project.OriginalRoomState!=1 || project.OriginalStage!=3 ||
            project.Progress<project.RequiredPoints || project.GameTypeSourceIndex is null || project.GenreSourceIndex is null)
            return ReleaseResult.NotReady;
        title=title.Trim();
        if (state.ReleasedGames.Any(x=>x.Title==title)) return ReleaseResult.DuplicateTitle;
        if (state.ReleasedGames.Any(x=>x.ReviewPending) || SalesMilestoneService.HasPending(state) || RecallService.HasPending(state)) return ReleaseResult.NotReady;
        long cost=checked(UnitProductionCost(catalog.Hardware.Single(x=>x.SourceIndex==project.HardwareSourceIndex),project.DataSize)*copies);
        if (state.Money is null) return ReleaseResult.MoneyUnknown;
        if (state.Money<cost) return ReleaseResult.InsufficientFunds;
        var product=new ReleasedGameState
        {
            ProductIndex=state.ReleasedGames.Count,Title=title,Price=price,Stock=copies,ProductionSpending=cost,SalesMilestoneStage=0,
            WeeklySalesHistory=SalesHistoryRules.NewHistory(),
            Recall=new(),
            HardwareSourceIndex=project.HardwareSourceIndex,AudienceSourceIndex=project.AudienceSourceIndex,
            GameTypeSourceIndex=project.GameTypeSourceIndex.Value,GenreSourceIndex=project.GenreSourceIndex.Value,
            Quality=project.Quality with {},Bugs=project.Bugs,Recognition=project.Recognition,DataSize=project.DataSize,
            MarketFactor=MarketFactor(state.ReleasedGames.Count,project.Quality,random)
        };
        ReviewService.Generate(product,catalog,random);
        FinancialRules.Apply(state,-cost);
        // First shipment sets the flag; its handler does not test the reprint guard.
        state.ProductionPerformedThisWeek=true;
        state.ReleasedGames.Add(product);state.DevelopmentProjects.Remove(project);
        // A new room must not receive an unfinished contribution from the old product.
        foreach (var e in state.Employees.Where(x=>x.Office?.SeatedAction is >=6 and <=17 &&
            (x.Office.SeatedAction-6)/6==project.RoomIndex)) ResetWork(e.Office!);
        return ReleaseResult.Success;
    }
    public static ReleaseResult TryProduce(GameState state,ReleasedGameState product,OriginalCatalog catalog,long copies)
    {
        if (copies<1) throw new ArgumentOutOfRangeException(nameof(copies));
        if (!state.ReleasedGames.Contains(product)) throw new ArgumentException("Product is not in this company.");
        var availability=CheckProduction(state,product);
        if (availability!=ReleaseResult.Success) return availability;
        long cost=checked(UnitProductionCost(catalog.Hardware.Single(x=>x.SourceIndex==product.HardwareSourceIndex),product.DataSize)*copies);
        long stock=checked(product.Stock+copies),spending=checked(product.ProductionSpending+cost);
        if (state.Money is null) return ReleaseResult.MoneyUnknown;
        if (state.Money<cost) return ReleaseResult.InsufficientFunds;
        FinancialRules.Apply(state,-cost);product.Stock=stock;product.ProductionSpending=spending;
        state.ProductionPerformedThisWeek=true;
        return ReleaseResult.Success;
    }
    public static ReleaseResult CheckProduction(GameState state,ReleasedGameState? product=null)
    {
        // Original menu checks recall state 2 before the company production flag.
        if (product is not null)
        {
            if (product.Recall is null) return ReleaseResult.StateUnknown;
            if (product.Recall.OriginalState==2) return ReleaseResult.Recalled;
        }
        if (state.ProductionPerformedThisWeek==true) return ReleaseResult.AlreadyProducedThisWeek;
        if (state.ProductionPerformedThisWeek is null || state.CompanyActivityTicks is null) return ReleaseResult.StateUnknown;
        return ReleaseResult.Success;
    }
    public static double MarketFactor(int productIndex,DevelopmentQuality quality,Random random)
    {
        DevelopmentWorkRules.ValidateQuality(quality);
        if (productIndex<0) throw new ArgumentOutOfRangeException(nameof(productIndex));
        int roll=random.Next(20);
        double factor=1+(roll switch {0=>1.5,1 or 2=>.5,3 or 4 or 5=>-.5,_=>0});
        if (productIndex<2) factor-=.1;
        else
        {
            if (quality.Novelty>200) factor+=random.Next(2)==0 ? 2.5 : -.5;
            else if (quality.Novelty>100) factor+=random.Next(2)==0 ? .5 : -.2;
            if (quality.Fun>200) {if (random.Next(3)==0) factor+=1.1;}
            else if (quality.Fun>100 && random.Next(2)==0) factor+=.6;
            // The subsequent >150 and >200 branches are unreachable in the source.
            if (quality.Music>100 && random.Next(10)==1) factor+=.5;
        }
        return factor<0 ? .1 : factor;
    }
    internal static void ResetWork(OfficeEmployeeState office)
    {
        // Assignment +06DA..070C clears these three cells. Other workspace
        // counters remain until their own start/break branches reset them.
        office.WorkTicks=office.WorkTier=office.BurstTicks=0;
    }
    private static void ValidateInput(string title,long price,long copies)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length>128) throw new ArgumentException("A release title is required.");
        // Modern input validation; the original spinner can produce negative prices.
        if (price<0 || copies<1) throw new ArgumentOutOfRangeException(nameof(price));
    }
}
