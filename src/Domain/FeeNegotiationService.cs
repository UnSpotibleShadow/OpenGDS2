namespace OpenGDS2.Domain;

public enum FeeNegotiationTarget { Transport, Retail }

/// <summary>Applied outcome and resumable cursor over meeting modes 13/14.</summary>
public sealed class FeeNegotiationState
{
    [System.Text.Json.Serialization.JsonRequired] public FeeNegotiationTarget Target {get;set;}
    [System.Text.Json.Serialization.JsonRequired] public long Before {get;set;}
    [System.Text.Json.Serialization.JsonRequired] public long After {get;set;}
    public int MessageIndex {get;set;}
}

public static class FeeNegotiationService
{
    // Original checks affordability but does not debit this displayed amount.
    public const long RequiredFunds=15_000_000;
    public static string Key(FeeNegotiationTarget target,string message)=>$"NEGOTIATION_{target}_{message}";
    public static readonly IReadOnlyList<string> ImportKeys=Enum.GetValues<FeeNegotiationTarget>()
        .SelectMany(target=>new[]{"INTRO","SPEAKER","FIRST_GREETING","GREETING","QUESTION","FAILURE_SUMMARY"}.Select(message=>Key(target,message)))
        .Concat(new[]{"NEGOTIATION_FIRST_SUCCESS","NEGOTIATION_SUCCESS","NEGOTIATION_FAILURE","NEGOTIATION_FLOOR","NEGOTIATION_SUCCESS_SUMMARY","NEGOTIATION_FLOOR_SUMMARY"}).ToArray();
    public static long? CurrentFee(GameState state,FeeNegotiationTarget target)=>target switch
    {FeeNegotiationTarget.Transport=>state.SalesFee2,FeeNegotiationTarget.Retail=>state.SalesFee1,_=>throw new ArgumentOutOfRangeException(nameof(target))};
    public static FacilityResult Begin(GameState state,int slot,FeeNegotiationTarget target,Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        _=CurrentFee(state,target);
        var result=FacilityService.CheckService(state,slot,FacilityKind.Lawyer);
        if (result!=FacilityResult.Success) return result;
        if (state.Money is not long money) return FacilityResult.MoneyUnknown;
        if (money<RequiredFunds) return FacilityResult.InsufficientFunds;
        if (CurrentFee(state,target) is not long before || before<0) return FacilityResult.StateUnknown;
        // The secretary uses portrait -2 and the manager portrait 4: neither
        // message helper selects a random employee. Only the outcome draws Rnd.
        long after=FeeNegotiationRules.Resolve(before,new SingleOutcomeRandom(random));
        var pending=new FeeNegotiationState {Target=target,Before=before,After=after};Validate(pending);
        if (target==FeeNegotiationTarget.Transport) state.SalesFee2=after;else state.SalesFee1=after;
        state.PendingFeeNegotiation=pending;return FacilityResult.Success;
    }
    private sealed class SingleOutcomeRandom(Random source):Random
    {public override double NextDouble()=>source.NextSingle();}
    public static int MessageCount(FeeNegotiationState pending)=>pending.Before is 1500 or 1300 or 1100 or 900 or 700 or 500 ? 5 : 1;
    public static string MessageKey(FeeNegotiationState pending)
    {
        Validate(pending);
        return pending.MessageIndex switch
        {
            0=>Key(pending.Target,"INTRO"),
            1=>Key(pending.Target,pending.Before==1500 ? "FIRST_GREETING" : "GREETING"),
            2=>Key(pending.Target,"QUESTION"),
            3=>pending.Before==1500 ? "NEGOTIATION_FIRST_SUCCESS" : pending.Before==500 ? "NEGOTIATION_FLOOR" : pending.After<pending.Before ? "NEGOTIATION_SUCCESS" : "NEGOTIATION_FAILURE",
            _=>pending.After<pending.Before ? "NEGOTIATION_SUCCESS_SUMMARY" : pending.Before==500 ? "NEGOTIATION_FLOOR_SUMMARY" : Key(pending.Target,"FAILURE_SUMMARY")
        };
    }
    public static bool Acknowledge(GameState state)
    {
        if (state.PendingFeeNegotiation is not { } pending) return false;
        Validate(pending);
        if (++pending.MessageIndex==MessageCount(pending)) state.PendingFeeNegotiation=null;
        return true;
    }
    public static void Validate(FeeNegotiationState pending)
    {
        bool known=pending.Before is 1300 or 1100 or 900 or 700;
        if (!Enum.IsDefined(pending.Target) || pending.Before<0 || pending.After<0 ||
            (pending.Before==1500 ? pending.After!=1300 : known ? pending.After!=pending.Before && pending.After!=pending.Before-200 : pending.After!=pending.Before) ||
            pending.MessageIndex<0 || pending.MessageIndex>=MessageCount(pending))
            throw new InvalidDataException("Invalid fee negotiation snapshot.");
    }
}
