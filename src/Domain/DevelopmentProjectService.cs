namespace OpenGDS2.Domain;

public enum DevelopmentPlanningResult { Success, RoomOccupied, ComputerUnknown, HardwareStateUnknown,
    HardwareUnavailable, LicenseRequired, ComputerTooSmall, MoneyUnknown, InsufficientFunds }

/// <summary>New-project, already-licensed acceptance branch of fGMKikakusyo.</summary>
public static class DevelopmentProjectService
{
    public static DevelopmentPlanningResult CheckNewPlan(GameState state,OriginalCatalog catalog,int roomIndex,
        int hardwareSourceIndex,int audienceSourceIndex,out DevelopmentQuote quote)
    {
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
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length>128) throw new ArgumentException("A project label of at most 128 characters is required.");
        DevelopmentWorkRules.SelectCategory(focus,0);
        var result=CheckNewPlan(state,catalog,roomIndex,hardwareSourceIndex,audienceSourceIndex,out var quote);
        if (result!=DevelopmentPlanningResult.Success) return result;
        var project=new DevelopmentProjectState
        {
            RoomIndex=roomIndex,HardwareSourceIndex=hardwareSourceIndex,
            AudienceSourceIndex=audienceSourceIndex,Title=title.Trim(),RequiredPoints=quote.Points,Focus=focus
        };
        state.Money-=quote.Cost;
        state.DevelopmentProjects.Add(project);
        return DevelopmentPlanningResult.Success;
    }
}
