namespace OpenGDS2.Domain;

// Modal calls in Module1.59 (009DB4F4), not the unproven timed presenter.
public static class CalendarDialogueRules
{
    public static int PageCount(CalendarEvent kind)=>kind switch
    {
        CalendarEvent.HardwareRetiring or CalendarEvent.HardwareRetired=>2,
        CalendarEvent.ContractRenewal or CalendarEvent.Awards or CalendarEvent.HardwareDevelopment or
            CalendarEvent.HardwareSoon or CalendarEvent.Exhibition or CalendarEvent.LoanRepayment or CalendarEvent.Graduates=>1,
        _=>0
    };
    public static string MessageKey(GameState state)
    {
        var t=state.Calendar?.Pending ?? throw new InvalidOperationException("No calendar conversation.");
        if (t.PendingEvent is not CalendarEvent kind || t.DialoguePage is not int page || page<0 || page>=PageCount(kind))
            throw new InvalidOperationException("No calendar conversation.");
        return "CALENDAR_DIALOGUE_"+(kind==CalendarEvent.Exhibition ? (state.Month==4 ? "ExhibitionSpring" : "ExhibitionAutumn") :
            kind.ToString()+(page==1 ? "Explanation" : ""));
    }
    public static bool Acknowledge(GameState state)
    {
        var t=state.Calendar?.Pending;
        if (t?.PendingEvent is not CalendarEvent kind || t.DialoguePage is not int page) return false;
        if (page+1<PageCount(kind)) {t.DialoguePage=page+1;return true;}
        // For notifications, commit only after the final page; in particular,
        // repayment text precedes the original debit at +195A.
        if (kind is CalendarEvent.HardwareDevelopment or CalendarEvent.HardwareSoon or
            CalendarEvent.HardwareRetiring or CalendarEvent.HardwareRetired or CalendarEvent.LoanRepayment)
            return CalendarService.Acknowledge(state);
        t.DialoguePage=null;return true;
    }
    public static int HardwareIndex(CalendarTransaction t)=>t.PendingEvent is
        CalendarEvent.HardwareDevelopment or CalendarEvent.HardwareRetiring or CalendarEvent.HardwareRetired ? t.Index-1 : t.Index;
    public static long LoanPrincipal(int type)=>type switch
    {1=>10_000_000,2=>50_000_000,3=>100_000_000,4=>200_000_000,_=>throw new ArgumentOutOfRangeException(nameof(type))};
    public static IEnumerable<string> ImportKeys=>new[] {"ContractRenewal","Awards","HardwareDevelopment","HardwareSoon",
        "HardwareRetiring","HardwareRetiringExplanation","HardwareRetired","HardwareRetiredExplanation",
        "ExhibitionSpring","ExhibitionAutumn","LoanRepayment","Graduates"}.Select(x=>"CALENDAR_DIALOGUE_"+x)
        .Concat(Enumerable.Range(0,4).Select(x=>$"GRADUATE_GREETING_{x}"))
        .Concat(Enumerable.Range(0,10).Select(x=>$"CALENDAR_MAKER_{x}")).Append("GRADUATE_SPEAKER");
    internal static void Validate(GameState state)
    {
        var t=state.Calendar?.Pending;
        if (t?.DialoguePage is not int page) return;
        if (t.PendingEvent is not CalendarEvent kind || page<0 || page>=PageCount(kind) ||
            t.Exhibition?.Visitors is not null || t.Awards is {CategoryIndex:>0} or {ResultReady:true} ||
            t.Graduates is {CandidateIndex:>0} or {GreetingVariant:not null})
            throw new InvalidDataException("Invalid calendar conversation cursor.");
    }
}
