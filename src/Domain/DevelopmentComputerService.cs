namespace OpenGDS2.Domain;

public enum ComputerPurchaseResult { Success, CurrentComputerUnknown, AlreadyOwned, Downgrade, MoneyUnknown, InsufficientFunds }

/// <summary>fGMComK purchase: reject same model/downgrade, charge full price, replace current model.</summary>
public static class DevelopmentComputerService
{
    public static ComputerPurchaseResult TryPurchase(GameState state, OriginalCatalog catalog, int sourceIndex)
    {
        var target = catalog.DevelopmentComputers.SingleOrDefault(x => x.SourceIndex == sourceIndex)
            ?? throw new ArgumentOutOfRangeException(nameof(sourceIndex));
        if (state.DevelopmentComputerIndex is null) return ComputerPurchaseResult.CurrentComputerUnknown;
        if (state.DevelopmentComputerIndex == sourceIndex) return ComputerPurchaseResult.AlreadyOwned;
        var current = catalog.DevelopmentComputers.SingleOrDefault(x => x.SourceIndex == state.DevelopmentComputerIndex)
            ?? throw new InvalidDataException("Current computer is missing from catalog.");
        if (target.BitWidth < 1 || current.BitWidth < 1 || target.PurchasePrice < 0)
            throw new InvalidDataException("Invalid computer catalog.");
        if (target.BitWidth < current.BitWidth) return ComputerPurchaseResult.Downgrade;
        if (state.Money is null) return ComputerPurchaseResult.MoneyUnknown;
        if (state.Money < target.PurchasePrice) return ComputerPurchaseResult.InsufficientFunds;
        state.Money -= target.PurchasePrice;
        state.DevelopmentComputerIndex = sourceIndex;
        return ComputerPurchaseResult.Success;
    }

    // fGMKaihatu confirms this before its funds check; equality is permitted.
    public static bool SupportsHardware(int computerBitWidth, int hardwareBitWidth)
    {
        if (computerBitWidth < 1 || hardwareBitWidth < 1) throw new ArgumentOutOfRangeException();
        return computerBitWidth >= hardwareBitWidth;
    }
}
