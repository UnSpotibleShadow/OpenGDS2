using System.Text.Json.Serialization;

namespace OpenGDS2.Domain;

public enum ExhibitionResult { Success, NoSelection, TooManyProducts, InvalidSelection, MoneyUnknown, InsufficientFunds, NotPending }
public sealed class ExhibitionState
{
    [JsonRequired] public List<int> EligibleProducts { get; set; } = [];
    // -1/-2 identify the two development rooms; nonnegative values are products.
    [JsonRequired] public List<int> SelectedProducts { get; set; } = [];
    public int? Visitors { get; set; }
    public double? ReportedAwarenessGain { get; set; }
}
public enum GameAward { Music, Graphics, Design, Worst, GrandPrize }
public sealed class AwardCeremonyState
{
    [JsonRequired] public int Year { get; set; }
    [JsonRequired] public List<int> Entries { get; set; } = [];
    [JsonRequired] public int CategoryIndex { get; set; }
    public int? WinningProduct { get; set; }
    // Null before the current category's draw; true afterwards. A null winning
    // product then means a competitor won. Null RivalEntries preserves legacy
    // saves whose original competitor catalog is unknown.
    public bool? ResultReady { get; set; }
    public List<int>? RivalEntries {get;set;}
    public int? WinningRival {get;set;}
    public List<int>? AnimationProducts {get;set;}
}
public sealed class GraduateEventState
{
    [JsonRequired] public List<int> Candidates { get; set; } = [];
    [JsonRequired] public int CandidateIndex { get; set; }
}

/// <summary>fGMGameDexSn selection and fGMGameDexKs visitor/awareness transactions.</summary>
public static class ExhibitionService
{
    public const long ProductFee = 10_000_000;
    public static List<int> Eligible(GameState state) => state.DevelopmentProjects.Where(p=>p.OriginalRoomState==1)
        .OrderBy(p=>p.RoomIndex).Select(p=>-p.RoomIndex-1)
        .Concat(state.ReleasedGames.OrderByDescending(p=>p.ProductIndex).Select(p=>p.ProductIndex)).ToList();
    public static ExhibitionResult Participate(GameState state,IReadOnlySet<int> selection,Random random)
    {
        var t=state.Calendar?.Pending;var e=t?.Exhibition;
        if (t?.PendingEvent!=CalendarEvent.Exhibition || e is null || e.Visitors is not null) return ExhibitionResult.NotPending;
        if (selection.Count==0) return ExhibitionResult.NoSelection;
        if (selection.Count>5) return ExhibitionResult.TooManyProducts;
        if (selection.Any(id=>!e.EligibleProducts.Contains(id)) || !e.EligibleProducts.SequenceEqual(Eligible(state))) return ExhibitionResult.InvalidSelection;
        if (state.Money is null) return ExhibitionResult.MoneyUnknown;
        long fee=checked(selection.Count*ProductFee);
        if (state.Money<fee) return ExhibitionResult.InsufficientFunds;
        var finance=FinancialRules.Prepare(state,-fee);
        // Selection rebuilds the original 379C list in room order, then reverse
        // release order, regardless of the order in which boxes were clicked.
        var selected=e.EligibleProducts.Where(selection.Contains).ToList();
        int visitors=0;
        // Timer 0099AB50: 90 ticks; +0092 runs every fourth tick (22 draws).
        for (int tick=4;tick<=90;tick+=4) visitors=checked(visitors+random.Next(350)+450);
        double gain=visitors/500;
        var changes=new List<(int Id,double Value)>();
        foreach (int id in selected)
        {
            double value=Awareness(state,id);
            // The original reuses this local: a room's 100 cap reduces the
            // gain for all later entries, including released games (uncapped).
            if (id<0 && value+gain>100) gain=100-value;
            changes.Add((id,value+gain));
        }
        finance.Commit(state);
        foreach (var change in changes) SetAwareness(state,change.Id,change.Value);
        e.SelectedProducts=selected;e.Visitors=visitors;e.ReportedAwarenessGain=gain;
        var c=state.Calendar!;
        if (c.ExhibitionVisitorRecord is int record && visitors>record) c.ExhibitionVisitorRecord=visitors;
        return ExhibitionResult.Success;
    }
    public static bool Close(GameState state)
    {
        var t=state.Calendar?.Pending;
        if (t?.PendingEvent!=CalendarEvent.Exhibition) return false;
        t.Exhibition=null;t.PendingEvent=null;t.Phase=CalendarPhase.Loans;
        return true;
    }
    private static double Awareness(GameState state,int id)=>id<0 ?
        state.DevelopmentProjects.Single(p=>p.RoomIndex==-id-1).Recognition : state.ReleasedGames.Single(p=>p.ProductIndex==id).Recognition;
    private static void SetAwareness(GameState state,int id,double value)
    {
        if (id<0) state.DevelopmentProjects.Single(p=>p.RoomIndex==-id-1).Recognition=value;
        else state.ReleasedGames.Single(p=>p.ProductIndex==id).Recognition=value;
    }
}

/// <summary>Verified own-company branches of 009BD6F8 and 009CEE10.</summary>
public static class AwardService
{
    public static readonly IReadOnlyList<GameAward> Order=Array.AsReadOnly(new[] {GameAward.Music,GameAward.Graphics,GameAward.Design,GameAward.Worst,GameAward.GrandPrize});
    public static long Prize(GameAward award)=>award switch
    {GameAward.Music or GameAward.Graphics=>100_000_000,GameAward.Design=>300_000_000,GameAward.Worst=>-200_000_000,GameAward.GrandPrize=>500_000_000,_=>throw new ArgumentOutOfRangeException(nameof(award))};
    public static int OriginalCode(GameAward award)=>award switch
    {GameAward.Music=>3,GameAward.Graphics=>2,GameAward.Design=>4,GameAward.Worst=>1,GameAward.GrandPrize=>5,_=>throw new ArgumentOutOfRangeException(nameof(award))};
    public static int? DrawWinner(GameAward award,IReadOnlyList<ReleasedGameState> entries,Random random)
    {
        int? winner=null;
        foreach (var product in entries)
        {
            var q=product.Quality;
            bool wins=award switch
            {
                // Ascending ElseIf tests make the higher thresholds unreachable.
                GameAward.Music=>q.Music>=70 && random.Next(10)==1,
                GameAward.Graphics=>q.Graphics>=70 && random.Next(10)==1,
                GameAward.Design=>(q.Fun+q.Novelty)/2>=80 && random.Next(10)==1,
                GameAward.Worst=>(q.Fun+q.Novelty)/2<=50 && random.Next(10)==1,
                GameAward.GrandPrize=>Grand(product,random),
                _=>throw new ArgumentOutOfRangeException(nameof(award))
            };
            if (wins) winner=product.ProductIndex;
        }
        if (award==GameAward.Worst)
        {
            // +0900 clears the first pass's winner, then draws for every entry
            // through non-short-circuit And, even when Fun > 30.
            winner=null;
            foreach (var product in entries)
            {int roll=random.Next(2);if (product.Quality.Fun<=30 && roll==0) winner=product.ProductIndex;}
        }
        return winner;
    }
    private static bool Grand(ReleasedGameState p,Random random)
    {
        var q=p.Quality;
        bool wins=(q.Fun+q.Novelty+q.Music+q.Graphics)/4>=120 && random.Next(10)==1;
        // One-million sales overrides the roll, without skipping its draw.
        return wins || p.UnitsSold>=1_000_000;
    }
    public static bool Reveal(GameState state,Random random)
    {
        var t=state.Calendar?.Pending;var a=t?.Awards;
        if (t?.PendingEvent!=CalendarEvent.Awards || a is null || a.ResultReady is not null || a.CategoryIndex>=Order.Count || state.Money is null) return false;
        var entries=a.Entries.Select(id=>state.ReleasedGames.Single(p=>p.ProductIndex==id)).ToList();
        // Thirteen name-animation draws precede selection in 009BD6F8.
        var animation=a.RivalEntries is null ? null : Enumerable.Range(0,13).Select(_=>a.RivalEntries[random.Next(31)]).ToList();
        var category=Order[a.CategoryIndex];int? winner=DrawWinner(category,entries,random);
        int? rival=winner is null && a.RivalEntries is not null ? a.RivalEntries[random.Next(31)] : null;
        FinancialRules.Change? finance=winner is not null ? FinancialRules.Prepare(state,Prize(category)) : null;
        if (winner is int id)
        {
            state.ReleasedGames.Single(p=>p.ProductIndex==id).OriginalAwardCode=OriginalCode(category);
            if (category==GameAward.GrandPrize && state.Calendar!.FirstGrandPrizeDate is null)
                state.Calendar.FirstGrandPrizeDate=new(a.Year,12,4);
        }
        finance?.Commit(state);a.WinningProduct=winner;a.WinningRival=rival;a.AnimationProducts=animation;a.ResultReady=true;
        if (winner is not null && category==GameAward.GrandPrize) RankingService.Queue(state,RankingKind.GrandPrize);
        return true;
    }
    public static bool Continue(GameState state)
    {
        var t=state.Calendar?.Pending;var a=t?.Awards;
        if (t?.PendingEvent!=CalendarEvent.Awards || a?.ResultReady!=true) return false;
        a.CategoryIndex++;a.ResultReady=null;a.WinningProduct=null;a.WinningRival=null;a.AnimationProducts=null;
        if (a.CategoryIndex==Order.Count)
        {t.Awards=null;t.PendingEvent=null;t.Phase=CalendarPhase.TenantsAndPayroll;}
        return true;
    }
}

public static class GraduateService
{
    public static List<int> DrawCandidates(GameState state,Random random)
    {
        if (state.MinimumSourceIndex is not int min || state.MaximumSourceIndex is not int max || state.SourceUpperBound is not int upper ||
            !state.EmployeeSources.Select(p=>p.SourceIndex).Order().SequenceEqual(Enumerable.Range(0,upper)))
            throw new InvalidOperationException("Unknown graduate source bounds.");
        var source=state.EmployeeSources.Where(p=>p.SourceIndex>=min && p.SourceIndex<=max).OrderBy(p=>p.SourceIndex).ToList();
        int eligible=source.Count(p=>p.GateValue==0);
        for (int pass=0;pass<100_000;pass++)
        {
            var candidates=new List<int>();
            foreach (var row in source)
            {int roll=random.Next(5);if (row.GateValue==0 && roll==0) candidates.Add(row.SourceIndex);}
            if (eligible<=2 || candidates.Count>=2) return candidates;
        }
        throw new InvalidOperationException("Graduate lottery stalled; no partial candidate list was accepted.");
    }
    public static HireResult Hire(GameState state,Random random)
    {
        var g=state.Calendar?.Pending?.Graduates;
        if (state.Calendar?.Pending?.PendingEvent!=CalendarEvent.Graduates || g is null || g.CandidateIndex>=g.Candidates.Count) return HireResult.NoActiveCandidate;
        var source=state.EmployeeSources.Single(p=>p.SourceIndex==g.Candidates[g.CandidateIndex]);
        var result=HiringService.TryHire(state,source.Employee,random);
        if (result==HireResult.Success)
        {
            // fGMKeiyakuS's graduate greeting draws one of four after copying.
            _=random.Next(4);
            Next(state);
        }
        return result;
    }
    public static bool Decline(GameState state)=>Next(state);
    private static bool Next(GameState state)
    {
        var t=state.Calendar?.Pending;var g=t?.Graduates;
        if (t?.PendingEvent!=CalendarEvent.Graduates || g is null || g.CandidateIndex>=g.Candidates.Count) return false;
        if (++g.CandidateIndex==g.Candidates.Count)
        {t.Graduates=null;t.PendingEvent=null;t.Phase=CalendarPhase.Complete;}
        return true;
    }
}

internal static class AnnualCalendarValidation
{
    public static void Validate(GameState state)
    {
        var c=state.Calendar!;var t=c.Pending;
        if (c.ExhibitionVisitorRecord<0) throw new InvalidDataException("Invalid exhibition record.");
        if (c.FirstGrandPrizeDate is GameDate record) Date(record);
        if (t is null) return;
        if ((t.PendingEvent==CalendarEvent.Exhibition)!=(t.Exhibition is not null) ||
            (t.PendingEvent==CalendarEvent.Awards)!=(t.Awards is not null) ||
            (t.PendingEvent==CalendarEvent.Graduates)!=(t.Graduates is not null))
            throw new InvalidDataException("Annual event payload and calendar event disagree.");
        if (t.Exhibition is ExhibitionState e)
        {
            if (e.EligibleProducts is null || e.SelectedProducts is null ||
                !e.EligibleProducts.SequenceEqual(ExhibitionService.Eligible(state)) ||
                e.SelectedProducts.Count>5 || !e.SelectedProducts.SequenceEqual(e.EligibleProducts.Where(e.SelectedProducts.Contains)) ||
                e.Visitors is null && (e.SelectedProducts.Count!=0 || e.ReportedAwarenessGain is not null) ||
                e.Visitors is not null && (e.Visitors is <9900 or >17578 || e.SelectedProducts.Count==0 ||
                    e.ReportedAwarenessGain is not double gain || !double.IsFinite(gain) || gain>e.Visitors/500))
                throw new InvalidDataException("Invalid exhibition selection or saved result.");
        }
        if (t.Awards is AwardCeremonyState a)
        {
            var products=state.Competition?.Products;
            if (a.RivalEntries is not null && (products is null || a.RivalEntries.Count!=31 ||
                a.RivalEntries.Any(id=>id<0 || id>=products.Count || products[id].Owner<0)) ||
                a.WinningRival is int rival && (a.ResultReady!=true || a.WinningProduct is not null || a.RivalEntries?.Contains(rival)!=true) ||
                a.AnimationProducts is not null && (a.ResultReady!=true || a.AnimationProducts.Count!=13 || a.AnimationProducts.Any(id=>a.RivalEntries?.Contains(id)!=true)) ||
                a.RivalEntries is not null && a.ResultReady==true && (a.AnimationProducts is null || a.WinningProduct is null && a.WinningRival is null))
                throw new InvalidDataException("Invalid rival award entries or animation.");
            if (a.Year!=t.ClosingDate.Year || a.Entries is null || a.CategoryIndex is <0 or >4 || a.ResultReady==false ||
                state.ReleasedGames.Any(p=>p.ReleasedOn is null) ||
                !a.Entries.SequenceEqual(state.ReleasedGames.Where(p=>p.ReleasedOn!.Value.Year==a.Year).OrderBy(p=>p.ProductIndex).Select(p=>p.ProductIndex)) ||
                a.WinningProduct is int winner && (!a.Entries.Contains(winner) || a.ResultReady!=true ||
                    state.ReleasedGames.Single(p=>p.ProductIndex==winner).OriginalAwardCode!=AwardService.OriginalCode(AwardService.Order[a.CategoryIndex])))
                throw new InvalidDataException("Invalid award ceremony cursor or winner.");
        }
        if (t.Graduates is GraduateEventState g)
        {
            if (g.Candidates is null || g.Candidates.Count==0 || g.CandidateIndex<0 || g.CandidateIndex>=g.Candidates.Count ||
                state.MinimumSourceIndex is not int min || state.MaximumSourceIndex is not int max ||
                !g.Candidates.SequenceEqual(g.Candidates.Distinct().Order()) ||
                g.Candidates.Any(id=>id<min || id>max || !state.EmployeeSources.Any(p=>p.SourceIndex==id)) ||
                state.EmployeeSources.Single(p=>p.SourceIndex==g.Candidates[g.CandidateIndex]).GateValue!=0)
                throw new InvalidDataException("Invalid graduate candidate queue.");
        }
    }
    internal static void Date(GameDate date)
    {
        try {CalendarRules.Validate(date);} catch (ArgumentOutOfRangeException ex) {throw new InvalidDataException("Invalid annual event date.",ex);}
    }
}
