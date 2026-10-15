namespace OpenGDS2.Domain;

public enum DevelopmentPlanningResult { Success, RoomOccupied, ComputerUnknown, HardwareStateUnknown,
    HardwareUnavailable, LicenseRequired, ComputerTooSmall, MoneyUnknown, InsufficientFunds,
    PredecessorUnavailable, SequelAlreadyDeveloping, MangaUnavailable }

/// <summary>New-project, already-licensed acceptance branch of fGMKikakusyo.</summary>
public static class DevelopmentProjectService
{
    // Rights meeting 16 only verifies willingness and the rights fee. The
    // full fee is paid once by the later mode-2 acceptance transaction.
    public static DevelopmentPlanningResult CheckMangaRights(GameState state,OriginalCatalog catalog,int mangaSourceIndex)
    {
        var manga=catalog.Manga.SingleOrDefault(x=>x.SourceIndex==mangaSourceIndex);
        if (manga is null) return DevelopmentPlanningResult.MangaUnavailable;
        if (manga.RightsFee<0) throw new InvalidDataException("Invalid manga rights fee.");
        if (state.Money is null) return DevelopmentPlanningResult.MoneyUnknown;
        return state.Money<manga.RightsFee ? DevelopmentPlanningResult.InsufficientFunds : DevelopmentPlanningResult.Success;
    }
    public static DevelopmentPlanningResult CheckMangaPlan(GameState state,OriginalCatalog catalog,int roomIndex,
        int hardwareSourceIndex,int audienceSourceIndex,int mangaSourceIndex,out DevelopmentQuote quote)
    {
        var result=CheckNewPlan(state,catalog,roomIndex,hardwareSourceIndex,audienceSourceIndex,out quote);
        var manga=catalog.Manga.SingleOrDefault(x=>x.SourceIndex==mangaSourceIndex);
        if (manga is null) return DevelopmentPlanningResult.MangaUnavailable;
        catalog=OwnHardwareService.Catalog(state,catalog);
        quote=DevelopmentPlanningRules.Quote(catalog.Hardware.Single(x=>x.SourceIndex==hardwareSourceIndex),
            catalog.Audiences.Single(x=>x.SourceIndex==audienceSourceIndex),2,manga.RightsFee);
        if (result is not (DevelopmentPlanningResult.Success or DevelopmentPlanningResult.InsufficientFunds)) return result;
        return state.Money<quote.Cost ? DevelopmentPlanningResult.InsufficientFunds : DevelopmentPlanningResult.Success;
    }
    public static DevelopmentPlanningResult TryAcceptMangaPlan(GameState state,OriginalCatalog catalog,int roomIndex,
        int hardwareSourceIndex,int audienceSourceIndex,int mangaSourceIndex,string title,DevelopmentFocus focus)
        =>TryAcceptPlan(state,catalog,roomIndex,hardwareSourceIndex,audienceSourceIndex,title,focus,null,mangaSourceIndex);
    public static DevelopmentPlanningResult CheckSequelPlan(GameState state,OriginalCatalog catalog,int roomIndex,
        int hardwareSourceIndex,int audienceSourceIndex,int predecessorProductIndex,out DevelopmentQuote quote)
    {
        var result=CheckNewPlan(state,catalog,roomIndex,hardwareSourceIndex,audienceSourceIndex,out quote);
        var predecessor=SequelDevelopmentRules.CheckPredecessor(state,predecessorProductIndex);
        return predecessor!=DevelopmentPlanningResult.Success ? predecessor : result;
    }
    public static DevelopmentPlanningResult TryAcceptSequelPlan(GameState state,OriginalCatalog catalog,int roomIndex,
        int hardwareSourceIndex,int audienceSourceIndex,int predecessorProductIndex,string title,DevelopmentFocus focus)
        =>TryAcceptPlan(state,catalog,roomIndex,hardwareSourceIndex,audienceSourceIndex,title,focus,predecessorProductIndex);
    public static DevelopmentPlanningResult CheckNewPlan(GameState state,OriginalCatalog catalog,int roomIndex,
        int hardwareSourceIndex,int audienceSourceIndex,out DevelopmentQuote quote)
    {
        catalog=OwnHardwareService.Catalog(state,catalog);
        if (roomIndex is <0 or >1) throw new ArgumentOutOfRangeException(nameof(roomIndex));
        var hardware=catalog.Hardware.SingleOrDefault(x=>x.SourceIndex==hardwareSourceIndex)
            ?? throw new ArgumentOutOfRangeException(nameof(hardwareSourceIndex));
        var audience=catalog.Audiences.SingleOrDefault(x=>x.SourceIndex==audienceSourceIndex)
            ?? throw new ArgumentOutOfRangeException(nameof(audienceSourceIndex));
        quote=DevelopmentPlanningRules.Quote(hardware,audience,0);
        if (quote.Points<=0) throw new InvalidDataException("Original planning points must be positive.");
        if (state.DevelopmentProjects.Any(x=>x.RoomIndex==roomIndex)) return DevelopmentPlanningResult.RoomOccupied;
        if (state.DevelopmentComputerIndex is not int computerIndex) return DevelopmentPlanningResult.ComputerUnknown;
        var computer=catalog.DevelopmentComputers.SingleOrDefault(x=>x.SourceIndex==computerIndex)
            ?? throw new InvalidDataException("Unknown development computer.");
        if (state.AvailableHardware is null || state.HardwareLicenses is null) return DevelopmentPlanningResult.HardwareStateUnknown;
        if (!state.AvailableHardware.Contains(hardwareSourceIndex)) return DevelopmentPlanningResult.HardwareUnavailable;
        // The original meeting can arrange a license; callers use the separate license service.
        if (!state.HardwareLicenses.Contains(hardwareSourceIndex)) return DevelopmentPlanningResult.LicenseRequired;
        if (hardware.BitWidth>computer.BitWidth) return DevelopmentPlanningResult.ComputerTooSmall;
        if (state.Money is null) return DevelopmentPlanningResult.MoneyUnknown;
        return state.Money<quote.Cost ? DevelopmentPlanningResult.InsufficientFunds : DevelopmentPlanningResult.Success;
    }
    public static DevelopmentPlanningResult TryAcceptNewPlan(GameState state,OriginalCatalog catalog,int roomIndex,
        int hardwareSourceIndex,int audienceSourceIndex,string title,DevelopmentFocus focus)
        =>TryAcceptPlan(state,catalog,roomIndex,hardwareSourceIndex,audienceSourceIndex,title,focus,null);
    private static DevelopmentPlanningResult TryAcceptPlan(GameState state,OriginalCatalog catalog,int roomIndex,
        int hardwareSourceIndex,int audienceSourceIndex,string title,DevelopmentFocus focus,int? predecessorProductIndex,int? mangaSourceIndex=null)
    {
        catalog=OwnHardwareService.Catalog(state,catalog);
        if (string.IsNullOrWhiteSpace(title) || title.Length>128) throw new ArgumentException("A project label of at most 128 characters is required.");
        DevelopmentWorkRules.SelectCategory(focus,0);
        DevelopmentQuote quote;
        var result=predecessorProductIndex is int parent
            ? CheckSequelPlan(state,catalog,roomIndex,hardwareSourceIndex,audienceSourceIndex,parent,out quote)
            : mangaSourceIndex is int manga ? CheckMangaPlan(state,catalog,roomIndex,hardwareSourceIndex,audienceSourceIndex,manga,out quote)
            : CheckNewPlan(state,catalog,roomIndex,hardwareSourceIndex,audienceSourceIndex,out quote);
        if (result!=DevelopmentPlanningResult.Success) return result;
        var project=new DevelopmentProjectState
        {
            PromotionUseCount=0,InterviewCompleted=false,
            TechnologyApplicationCounts=TechnologyApplicationService.NewProjectCounters(),
            RoomIndex=roomIndex,HardwareSourceIndex=hardwareSourceIndex,
            AudienceSourceIndex=audienceSourceIndex,Title=title.Trim(),RequiredPoints=quote.Points,Focus=focus
        };
        if (predecessorProductIndex is int predecessor)
            SequelDevelopmentRules.Initialize(project,state.ReleasedGames.Single(x=>x.ProductIndex==predecessor));
        if (mangaSourceIndex.HasValue) {project.OriginalMode=2;project.MangaSourceIndex=mangaSourceIndex;}
        FinancialRules.Apply(state,-quote.Cost);
        state.DevelopmentProjects.Add(project);
        return DevelopmentPlanningResult.Success;
    }
}
