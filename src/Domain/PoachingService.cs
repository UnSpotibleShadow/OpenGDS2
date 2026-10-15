namespace OpenGDS2.Domain;

public enum PoachingPhase { TargetIntroduction, Confirmation, RivalAlert, DefenceIntroduction, Negotiating, DonationAcknowledgement, DecisionIntroduction, DecisionSuspense, DecisionChoice, Result }
public enum PoachingActionResult { Success, NotNegotiating, InvalidAmount, InsufficientFunds, MoneyUnknown }
public sealed class PoachingState
{
    public int SourceIndex {get;set;}
    public bool Defensive {get;set;}
    public PoachingPhase Phase {get;set;}
    // Native Timer1: 100ms, elapsed += 0.5 after processing each callback.
    public int TimerCallbacks {get;set;}
    public long OriginalFee {get;set;}
    public long PlayerOffer {get;set;}
    public long RivalOffer {get;set;}
    public long PlayerDonations {get;set;}
    public long RivalDonations {get;set;}
    public int LastAction {get;set;}
    public int RivalTextTicks {get;set;}
    public int PlayerTextTicks {get;set;}
    public int RivalMessage {get;set;}=-1;
    public int PlayerMessage {get;set;}=-1;
    public int EmployeeMessage {get;set;}
    public bool? PlayerWins {get;set;}
    public long? PendingDonation {get;set;}
}

/// <summary>fGMHikinuki and its three selectors; source comparison uses offer + donations, ties favour the player.</summary>
public static class PoachingService
{
    public static readonly IReadOnlyList<string> ImportKeys=new[]{"POACHING_TARGET_INTRO","POACHING_CONFIRM","POACHING_RIVAL_ALERT","POACHING_DEFENCE_INTRO",
        "POACHING_DONATION_ACK","POACHING_DECISION_INTRO","POACHING_DECISION_SUSPENSE","POACHING_CHOICE_RIVAL","POACHING_CHOICE_PLAYER",
        "POACHING_RESULT_LOST_OFFENSIVE","POACHING_RESULT_LOST_DEFENSIVE","POACHING_RESULT_WON_OFFENSIVE","POACHING_RESULT_WON_DEFENSIVE"}
        .Concat(Enumerable.Range(0,10).Select(i=>"POACHING_RIVAL_MESSAGE_"+i)).Concat(Enumerable.Range(0,4).Select(i=>"POACHING_EMPLOYEE_MESSAGE_"+i)).ToArray();
    public static readonly long[] DonationAmounts=[5_000_000,10_000_000,20_000_000,50_000_000];
    public static readonly long[] ContractIncrements=[10_000_000,20_000_000,50_000_000,100_000_000];
    public static IEnumerable<RecruitmentSourceRecord> Candidates(GameState state)=>state.EmployeeSources
        .Where(s=>s.GateValue==0 && s.Employee.SpecialFlag==1 && s.Employee.ContractFee is >=0 &&
            !state.Employees.Any(e=>e.SourceIndex==s.SourceIndex)).OrderBy(s=>s.SourceIndex);
    public static bool Begin(GameState state,int sourceIndex)
    {
        if(FacilityService.HasPendingDecision(state) || state.PendingCompanyActivity is not null)return false;
        var source=Candidates(state).SingleOrDefault(s=>s.SourceIndex==sourceIndex);
        if(source is null || state.Money is null)return false;
        state.PendingPoaching=Create(source,false);return true;
    }
    private static PoachingState Create(RecruitmentSourceRecord source,bool defensive)=>new()
    {
        SourceIndex=source.SourceIndex,Defensive=defensive,
        Phase=defensive?PoachingPhase.RivalAlert:PoachingPhase.TargetIntroduction,
        OriginalFee=source.Employee.ContractFee!.Value,PlayerOffer=source.Employee.ContractFee.Value,RivalOffer=source.Employee.ContractFee.Value
    };
    public static bool Cancel(GameState state)
    {
        if(state.PendingPoaching is not {Phase:PoachingPhase.TargetIntroduction or PoachingPhase.Confirmation or PoachingPhase.Negotiating})return false;
        // Closing the native form does not refund donations or change employment.
        state.PendingPoaching=null;return true;
    }
    public static bool Confirm(GameState state)
    {
        if(state.PendingPoaching is not {Phase:PoachingPhase.Confirmation} scene)return false;
        scene.Phase=PoachingPhase.Negotiating;return true;
    }
    public static PoachingActionResult Donate(GameState state,long amount)
    {
        if(state.PendingPoaching is not {Phase:PoachingPhase.Negotiating} scene)return PoachingActionResult.NotNegotiating;
        var result=CheckDonation(state,amount);if(result!=PoachingActionResult.Success)return result;
        CommitDonation(state,scene,amount);return PoachingActionResult.Success;
    }
    public static PoachingActionResult QueueDonation(GameState state,long amount)
    {
        if(state.PendingPoaching is not {Phase:PoachingPhase.Negotiating} scene)return PoachingActionResult.NotNegotiating;
        var result=CheckDonation(state,amount);if(result!=PoachingActionResult.Success)return result;
        scene.PendingDonation=amount;scene.Phase=PoachingPhase.DonationAcknowledgement;return PoachingActionResult.Success;
    }
    private static PoachingActionResult CheckDonation(GameState state,long amount)=>!DonationAmounts.Contains(amount)?PoachingActionResult.InvalidAmount:
        state.Money is not long funds?PoachingActionResult.MoneyUnknown:funds<amount?PoachingActionResult.InsufficientFunds:PoachingActionResult.Success;
    private static void CommitDonation(GameState state,PoachingState scene,long amount)
    {
        long next=checked(scene.PlayerDonations+amount);var change=FinancialRules.Prepare(state,-amount);
        scene.PlayerDonations=next;scene.LastAction=1;scene.PlayerTextTicks=0;scene.PlayerMessage=0;change.Commit(state);
    }
    public static PoachingActionResult RaiseOffer(GameState state,long increment)
    {
        if(state.PendingPoaching is not {Phase:PoachingPhase.Negotiating} scene)return PoachingActionResult.NotNegotiating;
        if(!ContractIncrements.Contains(increment))return PoachingActionResult.InvalidAmount;
        scene.PlayerOffer=checked(scene.PlayerOffer+increment);scene.LastAction=2;scene.PlayerTextTicks=0;scene.PlayerMessage=1;
        return PoachingActionResult.Success;
    }
    public static bool Tick(GameState state,Random random)
    {
        if(state.PendingPoaching is not {Phase:PoachingPhase.Negotiating} scene)return false;
        if(scene.RivalTextTicks==15)scene.RivalMessage=-1;
        if(scene.PlayerTextTicks==15)scene.PlayerMessage=-1;
        scene.RivalTextTicks++;scene.PlayerTextTicks++;
        switch(scene.TimerCallbacks)
        {
            case 40:
                int donation=CompanyEventService.Draw(random,3);
                scene.RivalDonations=checked(scene.RivalDonations+new long[]{1_000_000,5_000_000,10_000_000}[donation]);
                scene.LastAction=1;scene.RivalTextTicks=0;scene.RivalMessage=donation;break;
            case 80:
                int offer=CompanyEventService.Draw(random,3);
                scene.RivalOffer=checked(scene.RivalOffer+new long[]{10_000_000,20_000_000,40_000_000}[offer]);
                scene.LastAction=2;scene.RivalTextTicks=0;scene.RivalMessage=3+offer;break;
            case 110:
                int more=CompanyEventService.Draw(random,3);
                scene.RivalDonations=checked(scene.RivalDonations+new long[]{5_000_000,10_000_000,15_000_000}[more]);
                scene.LastAction=1;scene.RivalTextTicks=0;scene.RivalMessage=6+more;break;
            case 160 when scene.RivalOffer<=scene.PlayerOffer:
                int last=CompanyEventService.Draw(random,3);
                scene.RivalOffer=checked(scene.PlayerOffer+new long[]{10_000_000,15_000_000,5_000_000}[last]);
                scene.LastAction=2;scene.RivalTextTicks=0;scene.RivalMessage=9;break;
        }
        if(scene.TimerCallbacks==0)scene.EmployeeMessage=0;
        if(scene.TimerCallbacks==180)scene.EmployeeMessage=1;
        if(scene.LastAction!=0){scene.EmployeeMessage=scene.LastAction+1;scene.LastAction=0;}
        if(scene.TimerCallbacks>=200)scene.Phase=PoachingPhase.DecisionIntroduction;
        scene.TimerCallbacks++;return true;
    }
    public static bool Acknowledge(GameState state,Random random)
    {
        if(state.PendingPoaching is not { } scene)return false;
        switch(scene.Phase)
        {
            case PoachingPhase.TargetIntroduction:scene.Phase=PoachingPhase.Confirmation;break;
            case PoachingPhase.DonationAcknowledgement:
                CommitDonation(state,scene,scene.PendingDonation!.Value);scene.PendingDonation=null;scene.Phase=PoachingPhase.Negotiating;break;
            case PoachingPhase.RivalAlert:scene.Phase=PoachingPhase.DefenceIntroduction;break;
            case PoachingPhase.DefenceIntroduction:scene.Phase=PoachingPhase.Negotiating;break;
            case PoachingPhase.DecisionIntroduction:
                scene.PlayerWins=checked(scene.RivalOffer+scene.RivalDonations)<=checked(scene.PlayerOffer+scene.PlayerDonations);
                scene.Phase=PoachingPhase.DecisionSuspense;break;
            case PoachingPhase.DecisionSuspense:scene.Phase=PoachingPhase.DecisionChoice;break;
            case PoachingPhase.DecisionChoice:scene.Phase=PoachingPhase.Result;break;
            case PoachingPhase.Result:
                var source=state.EmployeeSources.Single(s=>s.SourceIndex==scene.SourceIndex);
                if(scene.PlayerWins==true)
                {
                    if(!scene.Defensive)HiringService.HirePoached(state,source.Employee,random);
                    source.Employee.ContractFee=scene.PlayerOffer;
                    state.Employees.Single(e=>e.SourceIndex==scene.SourceIndex).ContractFee=scene.PlayerOffer;
                }
                else if(scene.Defensive)EmployeeDismissalService.TryDismiss(state,state.Employees.Single(e=>e.SourceIndex==scene.SourceIndex));
                state.PendingPoaching=null;break;
            default:return false;
        }
        return true;
    }
    internal static bool Periodic(GameState state,Random random)
    {
        state.PoachingOpportunityTicks=checked(state.PoachingOpportunityTicks+1);
        int draw=CompanyEventService.Draw(random,7000); // Native And eagerly consumes this below the 30000-tick gate.
        if(state.PoachingOpportunityTicks<=30000 || draw!=1)return true;
        state.PoachingOpportunityTicks=0;
        var staff=state.EmployeeSources.Where(s=>s.GateValue==1).OrderBy(s=>s.SourceIndex).ToArray();
        state.OriginalPayrollScratchIndex=staff.Length;
        float selection=random.NextSingle();
        if(!float.IsFinite(selection)||selection<0||selection>=1)throw new ArgumentOutOfRangeException(nameof(random));
        // With no employees the native code reads a retained scratch-array element.
        // Do not invent an employee/negotiation from absent legacy scratch data.
        if(staff.Length==0){state.PoachingEmptyRosterObserved=true;return true;}
        var target=staff[(int)Math.Floor((double)selection*staff.Length)];
        if(target.Employee.ContractFee is null || !state.Employees.Any(e=>e.SourceIndex==target.SourceIndex))
        {state.PoachingUnknownTargetObserved=true;return true;}
        state.PendingPoaching=Create(target,true);return false;
    }
    public static void Validate(GameState state)
    {
        if(state.PoachingOpportunityTicks<0)throw new InvalidDataException("Invalid poaching opportunity counter.");
        if(state.PendingPoaching is not { } scene)return;
        if(!Enum.IsDefined(scene.Phase) || scene.SourceIndex is <0 or >59 || scene.OriginalFee<0 ||
            scene.PlayerOffer<scene.OriginalFee || scene.RivalOffer<scene.OriginalFee || scene.PlayerDonations<0 || scene.RivalDonations<0 ||
            scene.TimerCallbacks is <0 or >201 || scene.LastAction is <0 or >2 || scene.RivalTextTicks<0 || scene.PlayerTextTicks<0 ||
            scene.RivalMessage is <-1 or >9 || scene.PlayerMessage is <-1 or >1 || scene.EmployeeMessage is <0 or >3 ||
            scene.Defensive && scene.Phase==PoachingPhase.Confirmation || !scene.Defensive && scene.Phase is PoachingPhase.RivalAlert or PoachingPhase.DefenceIntroduction ||
            (scene.Phase>=PoachingPhase.DecisionSuspense)!=(scene.PlayerWins is not null) ||
            (scene.Phase==PoachingPhase.DonationAcknowledgement)!=(scene.PendingDonation is not null) ||
            scene.PendingDonation is long pending && (!DonationAmounts.Contains(pending) || state.Money is not long funds || funds<pending) ||
            !state.EmployeeSources.Any(s=>s is not null && s.Employee is not null && s.SourceIndex==scene.SourceIndex && s.GateValue==(scene.Defensive?1:0) && s.Employee.ContractFee==scene.OriginalFee) ||
            state.Employees.Any(e=>e.SourceIndex==scene.SourceIndex)!=scene.Defensive ||
            state.PendingBackupDevice is not null || state.PendingVirusEvent is not null)
            throw new InvalidDataException("Invalid poaching negotiation.");
    }
}
