namespace OpenGDS2.Domain;

public enum LicensePurchaseResult { Success, AvailabilityUnknown, Unavailable, LicensesUnknown, AlreadyOwned, MoneyUnknown, InsufficientFunds }

/// <summary>Confirmed payment branch of fGMKikakuKaigi mode 3; no unproven negotiation bonus.</summary>
public static class HardwareLicenseService
{
    public static LicensePurchaseResult TryPurchase(GameState state, OriginalCatalog catalog, int sourceIndex)
    {
        var hardware = catalog.Hardware.SingleOrDefault(x => x.SourceIndex == sourceIndex)
            ?? throw new ArgumentOutOfRangeException(nameof(sourceIndex));
        if (state.AvailableHardware is null) return LicensePurchaseResult.AvailabilityUnknown;
        if (!state.AvailableHardware.Contains(sourceIndex)) return LicensePurchaseResult.Unavailable;
        if (state.HardwareLicenses is null) return LicensePurchaseResult.LicensesUnknown;
        if (state.HardwareLicenses.Contains(sourceIndex)) return LicensePurchaseResult.AlreadyOwned;
        if (hardware.LicenseFee < 0) throw new InvalidDataException("Invalid license fee.");
        if (state.Money is null) return LicensePurchaseResult.MoneyUnknown;
        if (state.Money < hardware.LicenseFee) return LicensePurchaseResult.InsufficientFunds;
        state.Money -= hardware.LicenseFee;
        state.HardwareLicenses.Add(sourceIndex);
        return LicensePurchaseResult.Success;
    }
}
