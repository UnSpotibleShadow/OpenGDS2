namespace OpenGDS2.Domain;

public enum CompanyActivity { PublicRelations, GenreResearch, ProductionExpansion }
public enum CompanyActivityResult { Success, AlreadyActed, StateUnknown, InsufficientFunds }

/// <summary>fGMSeisanZ 0099ED68, menu guards and main-loop automatic expansion.</summary>
public static class CompanyActivityService
{
    public const int TicksPerWeek=200;
    public static CompanyActivityResult TryPerform(GameState state,CompanyActivity activity,Random random)
    {
        if (!Enum.IsDefined(activity)) throw new ArgumentOutOfRangeException(nameof(activity));
        if (state.CompanyActivityPerformed==true) return CompanyActivityResult.AlreadyActed;
        if (state.CompanyActivityPerformed is null || state.CompanyActivityTicks is null || state.Company.ActivityCost is null || state.Money is null)
            return CompanyActivityResult.StateUnknown;
        int? current=activity switch
        {
            CompanyActivity.PublicRelations=>state.Company.Fame,
            CompanyActivity.GenreResearch=>state.Company.OriginalMember90,
            _=>state.Company.ProductionCapacity
        };
        if (current is null) return CompanyActivityResult.StateUnknown;
        if (current<0 || state.Company.ActivityCost<0) throw new InvalidDataException("Invalid business activity state.");
        if (state.Money<state.Company.ActivityCost) return CompanyActivityResult.InsufficientFunds;
        int gain=activity switch
        {
            CompanyActivity.PublicRelations=>1+random.Next(3),
            CompanyActivity.GenreResearch=>2+random.Next(3),
            _=>random.Next(12)==1 ? 17+random.Next(8) : 4+random.Next(4)
        };
        int next=activity==CompanyActivity.ProductionExpansion ? checked(current.Value+gain) : Math.Min(100,checked(current.Value+gain));
        var finance=FinancialRules.Prepare(state,-state.Company.ActivityCost.Value);
        switch (activity)
        {
            case CompanyActivity.PublicRelations:state.Company.Fame=next;break;
            case CompanyActivity.GenreResearch:state.Company.OriginalMember90=next;break;
            default:state.Company.ProductionCapacity=next;break;
        }
        finance.Commit(state);state.CompanyActivityPerformed=true;
        return CompanyActivityResult.Success;
    }
    public static void Tick(GameState state,Random random,OriginalCatalog? catalog=null)
    {
        if (CalendarService.HasPending(state) || state.PendingFacilityReceipt is not null) return;
        // This recovered budget phase is separate from the incomplete calendar
        // economics. Older modern saves leave phase/flag unknown.
        if (state.CompanyActivityTicks is not int ticks || state.CompanyActivityPerformed is null) return;
        ticks++;
        if (ticks>=160 && state.CompanyActivityPerformed==false &&
            SecretaryPolicyRules.ProductionThreshold(state.SecretaryPolicies) is long threshold && state.Money>=threshold)
            TryPerform(state,CompanyActivity.ProductionExpansion,random);
        if (ticks==TicksPerWeek)
        {
            if (state.Calendar is not null)
            {
                CalendarService.BeginWeek(state,catalog,random);
                return;
            }
            // Weekly procedure +0119..01A5 shifts the history before the
            // periodic sales call; a boundary batch enters the new week.
            SalesHistoryRules.AdvanceWeek(state);
            ticks=0;state.CompanyActivityPerformed=false;
            // Weekly procedure 009DB4F4 +000B clears the company production flag.
            state.ProductionPerformedThisWeek=false;
            RecallService.AdvanceWeek(state,random);
        }
        state.CompanyActivityTicks=ticks;
    }
}
