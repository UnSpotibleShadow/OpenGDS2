namespace OpenGDS2.Domain;

/// <summary>Locally imported Module1.26 records. Advertised salary is a display column;
/// no recurring debit is established by the recovered calendar.</summary>
public sealed class SecretaryDefinition
{
    public int SourceIndex {get;set;}
    public string Name {get;set;}=string.Empty;
    public int RankCode {get;set;}
    public long ContractFee {get;set;}
    public long AdvertisedSalary {get;set;}
    public int PortraitId {get;set;}
    public string Description {get;set;}=string.Empty;
    public string Rank=>RankCode switch {0=>"S",1=>"A",2=>"B",3=>"C",_=>throw new InvalidDataException("Invalid secretary rank.")};
}
public enum SecretaryHireResult { Success, StateUnknown, AlreadyEmployed, InsufficientFunds, EventPending }
public enum SecretaryReplacementPhase { Farewell, Greeting }
public sealed class SecretaryReplacementState
{
    public int PreviousSourceIndex {get;set;}
    public int NextSourceIndex {get;set;}
    public SecretaryReplacementPhase Phase {get;set;}
}

/// <summary>fGMHisyoK 0098ADF4: debit, old farewell, identity change, new greeting.</summary>
public static class SecretaryManagementRules
{
    public static bool HasCatalog(OriginalCatalog? catalog)=>catalog?.Secretaries is {Count:5} rows &&
        rows.All(x=>x is not null && x.SourceIndex is >=0 and <=4 && x.RankCode is >=0 and <=3 &&
            x.ContractFee>=0 && x.AdvertisedSalary>=0 && !string.IsNullOrWhiteSpace(x.Name) &&
            !string.IsNullOrWhiteSpace(x.Description) && x.PortraitId==SecretaryAdviceRules.PortraitId(x.SourceIndex)) &&
        rows.Select(x=>x.SourceIndex).Distinct().Count()==5;

    public static bool OtherEventPending(GameState state)=>state.PendingHireGreeting is not null ||
        state.PendingSecretaryAdvice is not null || CalendarService.HasPending(state) ||
        state.DevelopmentProjects.Any(p=>p.PendingDecision.HasValue || CheckpointMeetingRules.HasPending(p)) ||
        state.ReleasedGames.Any(p=>p.ReviewPending) || RecallService.HasPending(state) || SalesMilestoneService.HasPending(state);

    public static SecretaryHireResult CheckHire(GameState state,OriginalCatalog? catalog,int sourceIndex)
    {
        if (sourceIndex is <0 or >4) throw new ArgumentOutOfRangeException(nameof(sourceIndex));
        if (state.PendingSecretaryReplacement is not null || OtherEventPending(state)) return SecretaryHireResult.EventPending;
        if (!HasCatalog(catalog) || state.SecretarySourceIndex is not (>=0 and <=4) || state.Money is null)
            return SecretaryHireResult.StateUnknown;
        // Recruitment form 009AC5B8 +0450..0480 excludes the current secretary.
        if (state.SecretarySourceIndex==sourceIndex) return SecretaryHireResult.AlreadyEmployed;
        return state.Money<catalog!.Secretaries.Single(x=>x.SourceIndex==sourceIndex).ContractFee
            ? SecretaryHireResult.InsufficientFunds : SecretaryHireResult.Success;
    }
    public static SecretaryHireResult TryHire(GameState state,OriginalCatalog? catalog,int sourceIndex)
    {
        var result=CheckHire(state,catalog,sourceIndex);
        if (result!=SecretaryHireResult.Success) return result;
        var change=FinancialRules.Prepare(state,-catalog!.Secretaries.Single(x=>x.SourceIndex==sourceIndex).ContractFee);
        var pending=new SecretaryReplacementState {PreviousSourceIndex=state.SecretarySourceIndex!.Value,NextSourceIndex=sourceIndex};
        change.Commit(state);state.PendingSecretaryReplacement=pending;
        return SecretaryHireResult.Success;
    }
    public static bool Acknowledge(GameState state)
    {
        if (state.PendingSecretaryReplacement is not { } pending) return false;
        Validate(state);
        if (pending.Phase==SecretaryReplacementPhase.Farewell)
        {
            state.SecretarySourceIndex=pending.NextSourceIndex;
            pending.Phase=SecretaryReplacementPhase.Greeting;
        }
        else state.PendingSecretaryReplacement=null;
        return true;
    }
    public static void Validate(GameState state)
    {
        if (state.PendingSecretaryReplacement is not { } p) return;
        if (p.PreviousSourceIndex is <0 or >4 || p.NextSourceIndex is <0 or >4 ||
            p.PreviousSourceIndex==p.NextSourceIndex || !Enum.IsDefined(p.Phase) || OtherEventPending(state) ||
            state.SecretarySourceIndex!=(p.Phase==SecretaryReplacementPhase.Farewell ? p.PreviousSourceIndex : p.NextSourceIndex))
            throw new InvalidDataException("Invalid pending secretary replacement.");
    }
}
