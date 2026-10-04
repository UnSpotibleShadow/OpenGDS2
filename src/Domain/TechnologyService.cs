namespace OpenGDS2.Domain;

public enum TechnologyPurchaseResult { Success, StateUnknown, AlreadyOwned, InsufficientFunds }

/// <summary>Purchase form 00986614; selecting an already owned row is disabled by 00984CEC.</summary>
public static class TechnologyService
{
    public static TechnologyPurchaseResult TryPurchase(GameState state,TechnologyDefinition technology)
    {
        if (technology.PurchaseFee<0) throw new InvalidDataException("Invalid technology price.");
        if (state.TechnologyOwnershipLevels is null || !state.TechnologyOwnershipLevels.TryGetValue(technology.SourceIndex,out int level) || state.Money is null)
            return TechnologyPurchaseResult.StateUnknown;
        if (level>=1) return TechnologyPurchaseResult.AlreadyOwned;
        if (state.Money<technology.PurchaseFee) return TechnologyPurchaseResult.InsufficientFunds;
        FinancialRules.Apply(state,-technology.PurchaseFee);
        state.TechnologyOwnershipLevels[technology.SourceIndex]=3;
        return TechnologyPurchaseResult.Success;
    }
}
