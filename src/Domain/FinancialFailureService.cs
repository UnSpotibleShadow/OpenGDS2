using System.Text.Json.Serialization;

namespace OpenGDS2.Domain;

// Modern resumable presentation of 00A0283C +CE69..D5FA. It preserves gameplay
// cells and does not claim an original saved bankruptcy flag.
public sealed class FinancialFailureState
{
    [JsonRequired] public int Phase {get;set;}
    [JsonRequired] public int NextSource {get;set;}
    [JsonRequired] public List<FinancialFailureSlot>? Slots {get;set;}
    [JsonRequired] public FinancialFailureReaction? Reaction {get;set;}
}
public sealed class FinancialFailureSlot
{
    [JsonRequired] public int SourceIndex {get;set;}
    [JsonRequired] public bool Employed {get;set;}
    [JsonRequired] public string Name {get;set;}=string.Empty;
    [JsonRequired] public int? PortraitId {get;set;}
}
public sealed class FinancialFailureReaction
{
    [JsonRequired] public int SourceIndex {get;set;}
    [JsonRequired] public int Variant {get;set;}
}
public static class FinancialFailureService
{
    public static string ReactionKey(int variant)=>$"FINANCIAL_FAILURE_REACTION_{variant}";
    public static IEnumerable<string> ImportKeys=>new[]{"FINANCIAL_FAILURE_INTRO_0","FINANCIAL_FAILURE_INTRO_1","FINANCIAL_FAILURE_CLOSING","FINANCIAL_FAILURE_NAMED","FINANCIAL_FAILURE_EMPLOYEE"}
        .Concat(Enumerable.Range(0,8).Select(ReactionKey));
    public static bool IsComplete(GameState state)=>state.FinancialFailure?.Phase==4;
    public static bool Begin(GameState state)
    {
        if (state.FinancialFailure is not null || state.Money is not <0 || FacilityService.HasPendingDecision(state)) return false;
        List<FinancialFailureSlot>? slots=null;
        if (state.SourceUpperBound is int upper && upper is >=0 and <=512 && state.EmployeeSources.Count==upper &&
            state.EmployeeSources.Select(x=>x.SourceIndex).Order().SequenceEqual(Enumerable.Range(0,upper)) &&
            state.EmployeeSources.Where(x=>x.GateValue==1).All(x=>!string.IsNullOrWhiteSpace(x.Employee.Name) && x.Employee.PortraitId is >=1 and <=10))
            slots=state.EmployeeSources.OrderBy(x=>x.SourceIndex).Select(x=>new FinancialFailureSlot
            {SourceIndex=x.SourceIndex,Employed=x.GateValue==1,Name=x.GateValue==1 ? x.Employee.Name : string.Empty,
                PortraitId=x.GateValue==1 ? x.Employee.PortraitId : null}).ToList();
        state.FinancialFailure=new() {Slots=slots};
        return true;
    }
    public static bool Acknowledge(GameState state,Random random)
    {
        if (state.FinancialFailure is not { } failure || failure.Phase==4 || failure.Phase==2 && failure.Slots is null) return false;
        if (failure.Phase==0) {failure.Phase=1;return true;}
        if (failure.Phase==3) {failure.Phase=4;return true;}
        failure.Phase=2;failure.Reaction=null;
        if (failure.Slots is null) return true; // Unknown legacy roster: stop rather than invent slots/draws.
        while (failure.NextSource<failure.Slots.Count)
        {
            var slot=failure.Slots[failure.NextSource++];
            // VB And evaluates the draw even for an unemployed slot.
            int selected=(int)Math.Floor(random.NextSingle()*2);
            if (slot.Employed && selected==1)
            {
                failure.Reaction=new() {SourceIndex=slot.SourceIndex,Variant=(int)Math.Floor(random.NextSingle()*8)};
                return true;
            }
        }
        failure.Phase=3;
        return true;
    }
    public static void Validate(GameState state)
    {
        if (state.FinancialFailure is not { } f) return;
        if (state.Money is not <0 || f.Phase is <0 or >4 || f.NextSource<0 ||
            (f.Slots is null ? f.NextSource!=0 || f.Phase>=3 || f.Reaction is not null :
                f.Slots.Count>512 || f.Slots.Count!=state.SourceUpperBound || f.Slots.Any(x=>x is null) || !f.Slots.Select(x=>x.SourceIndex).SequenceEqual(Enumerable.Range(0,f.Slots.Count)) ||
                f.Slots.Any(x=>x.Employed ? string.IsNullOrWhiteSpace(x.Name) || x.PortraitId is not (>=1 and <=10) : x.Name!=string.Empty || x.PortraitId is not null) ||
                f.NextSource>f.Slots.Count || f.Phase<2 && f.NextSource!=0 || f.Phase>=3 && f.NextSource!=f.Slots.Count) ||
            (f.Phase==2 && f.Slots is not null ? f.Reaction is null : f.Reaction is not null) ||
            f.Reaction is { } r && (r.Variant is <0 or >7 || r.SourceIndex!=f.NextSource-1 || r.SourceIndex<0 || f.Slots is null || !f.Slots[r.SourceIndex].Employed))
            throw new InvalidDataException("Invalid financial-failure continuation.");
        if (OwnHardwareService.HasPending(state) || CalendarService.HasPending(state) || state.PendingBankLoanReceipt is not null ||
            state.PendingHireGreeting is not null || state.PendingSecretaryAdvice is not null || state.PendingSecretaryReplacement is not null ||
            state.PendingProjectCancellation is not null || state.PendingFacilityReceipt is not null || state.PendingFeeNegotiation is not null ||
            state.PendingMysteryLabResult is not null || state.DevelopmentProjects.Any(p=>p.PendingDecision.HasValue || CheckpointMeetingRules.HasPending(p)) ||
            state.ReleasedGames.Any(p=>p.ReviewPending) || SalesMilestoneService.HasPending(state) || RecallService.HasPending(state))
            throw new InvalidDataException("Financial failure overlaps another suspended transaction.");
    }
}
