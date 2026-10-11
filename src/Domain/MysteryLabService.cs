using System.Text.Json.Serialization;

namespace OpenGDS2.Domain;

public sealed class MysteryLabState
{
    [JsonRequired] public double Progress {get;set;}
    [JsonRequired] public int Outcome {get;set;}
}
public sealed class MysteryLabResult
{
    [JsonRequired] public int Outcome {get;set;}
    [JsonRequired] public int MessageIndex {get;set;}
    [JsonRequired] public bool Fame99 {get;set;}
}

/// <summary>00987B40 start; 009E3830 +07BF..0940; 009EFA40 mode 15.</summary>
public static class MysteryLabService
{
    public const long ExperimentCost=25_000_000;
    public static readonly IReadOnlyList<string> ImportKeys=new[] {"MYSTERY_LAB_INTRO"}
        .Concat(Enumerable.Range(0,7).Select(i=>$"MYSTERY_LAB_RESULT_{i}"))
        .Append("MYSTERY_LAB_RESULT_4_HIGH").ToArray();

    private static List<RecruitmentSourceRecord>? EmployedSources(GameState state)
    {
        if (state.SourceUpperBound is not int upper || upper<=0 || state.EmployeeSources.Count!=upper ||
            !state.EmployeeSources.Select(x=>x.SourceIndex).Order().SequenceEqual(Enumerable.Range(0,upper))) return null;
        var rows=state.EmployeeSources.Where(x=>x.GateValue==1).ToList();
        if (rows.Count!=state.Employees.Count || !rows.Select(x=>x.SourceIndex).Order().SequenceEqual(
            state.Employees.Select(x=>x.SourceIndex).Order().Select(x=>x ?? -1))) return null;
        return rows;
    }
    public static FacilityResult CheckStart(GameState state,int slot)
    {
        var result=FacilityService.CheckService(state,slot,FacilityKind.MysteryLab);
        if (result!=FacilityResult.Success) return result;
        if (state.MysteryLab.Progress>0) return FacilityResult.ExperimentRunning;
        if (state.Money is null) return FacilityResult.MoneyUnknown;
        // Modern completeness guards; no funds/affordability guard is added.
        if (state.Company.ProductionCapacity is null || state.Company.Fame is null || EmployedSources(state) is null)
            return FacilityResult.StateUnknown;
        return FacilityResult.Success;
    }
    public static FacilityResult Start(GameState state,int slot,Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        var result=CheckStart(state,slot);if (result!=FacilityResult.Success) return result;
        long cash=checked(state.Money!.Value-ExperimentCost);
        float draw=random.NextSingle();
        if (!float.IsFinite(draw) || draw<0 || draw>=1) throw new ArgumentOutOfRangeException(nameof(random));
        state.MysteryLab.Outcome=(int)Math.Floor(draw*7d);state.MysteryLab.Progress=1;
        // This is a direct cash-cell write, not an accounting-helper debit.
        state.Money=cash;return FacilityResult.Success;
    }
    public static bool Tick(GameState state)
    {
        var lab=state.MysteryLab;
        if (FacilityService.HasPendingDecision(state) || lab.Progress<=0 ||
            state.Calendar?.OriginalSlots0444.Contains((int)FacilityKind.MysteryLab)!=true) return false;
        double next=lab.Progress+0.2;
        if (next<=100) {lab.Progress=next;return false;}
        // Resolve all required data/overflow before committing the completion.
        long? cash=null;int? capacity=null,fame=null;List<RecruitmentSourceRecord>? staff=null;
        switch (lab.Outcome)
        {
            case 0: if (state.Money is not long gain) return false;cash=checked((long)Math.Floor(gain*1.5d));break;
            case 1: if (state.Money is not long loss) return false;cash=checked((long)Math.Floor(loss/2d));break;
            case 2: if (state.Company.ProductionCapacity is not int twice) return false;capacity=checked(twice*2);break;
            case 3: if (state.Company.ProductionCapacity is not int reduced) return false;capacity=checked((int)Math.Floor(reduced*0.8d));break;
            case 4: if (state.Company.Fame is not int previous) return false;fame=previous<80 ? 80 : 99;break;
            case 5: case 6: staff=EmployedSources(state);if (staff is null) return false;break;
            default: throw new InvalidDataException("Invalid mystery-lab result.");
        }
        if (cash.HasValue) state.Money=cash.Value;
        if (capacity.HasValue) state.Company.ProductionCapacity=capacity.Value;
        if (fame.HasValue) state.Company.Fame=fame.Value;
        if (staff is not null) foreach (var row in staff)
        {
            double stress=lab.Outcome==5 ? 0 : 90;
            row.Employee.Stress=stress;
            state.Employees.Single(x=>x.SourceIndex==row.SourceIndex).Stress=stress;
        }
        lab.Progress=next;
        state.PendingMysteryLabResult=new() {Outcome=lab.Outcome,Fame99=fame==99};return true;
    }
    public static string MessageKey(MysteryLabResult pending)=>pending.MessageIndex==0 ? "MYSTERY_LAB_INTRO" :
        pending.Outcome==4 && pending.Fame99 ? "MYSTERY_LAB_RESULT_4_HIGH" : $"MYSTERY_LAB_RESULT_{pending.Outcome}";
    public static bool Acknowledge(GameState state)
    {
        if (state.PendingMysteryLabResult is not { } pending) return false;
        if (pending.MessageIndex==0) pending.MessageIndex=1;
        else {state.PendingMysteryLabResult=null;state.MysteryLab.Progress=0;}
        return true;
    }
    public static void Validate(GameState state)
    {
        var lab=state.MysteryLab;
        if (lab is null || !double.IsFinite(lab.Progress) || lab.Progress<0 || lab.Progress>100.200001 || lab.Outcome is <0 or >6)
            throw new InvalidDataException("Invalid mystery-lab experiment.");
        if (state.PendingMysteryLabResult is not { } pending)
        {if (lab.Progress>100) throw new InvalidDataException("Missing mystery-lab result.");return;}
        if (pending.Outcome!=lab.Outcome || pending.MessageIndex is <0 or >1 || lab.Progress<=100 ||
            pending.Fame99 && pending.Outcome!=4 || state.Calendar?.OriginalSlots0444.Contains(6)!=true ||
            state.PendingFacilityReceipt is not null || state.PendingFeeNegotiation is not null || CalendarService.HasPending(state) ||
            state.PendingProjectCancellation is not null || state.PendingHireGreeting is not null ||
            state.PendingSecretaryAdvice is not null || state.PendingSecretaryReplacement is not null || state.PendingBankLoanReceipt is not null)
            throw new InvalidDataException("Invalid pending mystery-lab result.");
    }
}
