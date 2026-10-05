namespace OpenGDS2.Domain;

// Values match original globals 08D0,08B0,08C0, not visual radio order.
public enum ProductionPolicy { Funds300Million=0, Funds100Million=1, Manual=2 }
public enum BreakPolicy { Voluntary=0, Stress100=1, Stress80=2, Stress60=3 }
public enum ReturnPolicy { Stress40=0, Stress20=1, Stress0=2 }
public sealed class SecretaryPolicyState
{
    public ProductionPolicy Production {get;set;}
    public BreakPolicy Break {get;set;}=BreakPolicy.Stress80;
    public ReturnPolicy Return {get;set;}=ReturnPolicy.Stress0;
}

/// <summary>Module2.11 +00C1..0109 and +B163..B30B. Original routing helper
/// semantics for supported study/development/lounge actions are reused.</summary>
public static class SecretaryPolicyRules
{
    public static long? ProductionThreshold(SecretaryPolicyState? policy)=>policy?.Production switch
    {
        null or ProductionPolicy.Manual=>null,
        ProductionPolicy.Funds300Million=>300_000_000,
        ProductionPolicy.Funds100Million=>100_000_000,
        _=>throw new InvalidDataException("Invalid production policy.")
    };
    public static int? BreakThreshold(BreakPolicy policy)=>policy switch
    {BreakPolicy.Voluntary=>null,BreakPolicy.Stress100=>100,BreakPolicy.Stress80=>80,BreakPolicy.Stress60=>60,_=>throw new ArgumentOutOfRangeException(nameof(policy))};
    public static int ReturnThreshold(ReturnPolicy policy)=>policy switch
    {ReturnPolicy.Stress40=>40,ReturnPolicy.Stress20=>20,ReturnPolicy.Stress0=>0,_=>throw new ArgumentOutOfRangeException(nameof(policy))};

    public static bool TrySet(GameState state,SecretaryPolicyState policy)
    {
        ValidatePolicy(policy);
        if (state.PendingSecretaryReplacement is not null || SecretaryManagementRules.OtherEventPending(state)) return false;
        state.SecretaryPolicies=new() {Production=policy.Production,Break=policy.Break,Return=policy.Return};
        return true;
    }
    public static void TickEmployee(GameState state,EmployeeState employee,Random random)
    {
        if (state.SecretaryPolicies is not { } policy || employee.Office is not { } office ||
            office.AutomaticBreak is null || employee.Stress is not double stress) return;
        int? threshold=BreakThreshold(policy.Break);
        if (threshold is null) return; // Voluntary disables both automatic branches.
        if (stress>=threshold && office.AutomaticBreak==false && !(office.WorkTier==5 && office.WorkTicks==3))
        {
            // Flag/remembered action are written before trying to find a lounge
            // seat. A failed seat search must not retry on every frame.
            office.AutomaticBreak=true;office.BeforeAutomaticBreakAction=office.Action;
            OfficeAssignmentService.TryTakeBreak(state,employee,random,automatic:true);
        }
        if (stress<=ReturnThreshold(policy.Return) && office.AutomaticBreak==true)
        {
            int action=office.BeforeAutomaticBreakAction!.Value;
            office.AutomaticBreak=false;
            if (action is >=1 and <=5) OfficeAssignmentService.TryAssignStudySeat(state,employee,action);
            else if (action is >=6 and <=17) OfficeAssignmentService.TryAssignDesk(state,employee,action);
            else if (action is >=18 and <=20) OfficeAssignmentService.TryTakeBreak(state,employee,random);
            // Request 0 is ignored by original assignment helper +01A1..01AF.
        }
    }
    public static void ValidatePolicy(SecretaryPolicyState policy)
    {
        if (!Enum.IsDefined(policy.Production) || !Enum.IsDefined(policy.Break) || !Enum.IsDefined(policy.Return))
            throw new InvalidDataException("Invalid secretary policies.");
    }
    public static void Validate(GameState state)
    {
        if (state.SecretaryPolicies is { } p) ValidatePolicy(p);
        foreach (var e in state.Employees.Concat(state.EmployeeSources.Select(x=>x.Employee)))
            if (e.Office is { } o && (o.BeforeAutomaticBreakAction is <0 or >20 ||
                o.AutomaticBreak==true && o.BeforeAutomaticBreakAction is null))
                throw new InvalidDataException("Invalid automatic break state.");
    }
}
