using System.Text.Json.Serialization;
namespace OpenGDS2.Domain;

public sealed class BlackjackRoundState
{
    [JsonRequired] public int Phase {get;set;}
    [JsonRequired] public int Tick {get;set;}
    [JsonRequired] public List<CasinoCard> Cards {get;set;}=[]; // Player nine, then dealer nine.
    [JsonRequired] public HashSet<int> UsedCards {get;set;}=[];
    [JsonRequired] public int PlayerCount {get;set;}
    [JsonRequired] public int DealerCount {get;set;}
    [JsonRequired] public int PlayerTotal {get;set;}
    [JsonRequired] public int DealerTotal {get;set;}
    [JsonRequired] public bool DealEnabled {get;set;}
    [JsonRequired] public bool Natural {get;set;}
    [JsonRequired] public bool Win {get;set;}
    [JsonRequired] public int AwardIndex {get;set;}
    [JsonRequired] public int AwardTick {get;set;}
    [JsonRequired] public bool AwardReady {get;set;}
    [JsonRequired] public int CollectTick {get;set;}
    [JsonRequired] public List<CasinoCard> DoubleCards {get;set;}=[];
    [JsonRequired] public List<int> Marks {get;set;}=[0,0,0,0,0];
    [JsonRequired] public int? FirstChoice {get;set;}
    [JsonRequired] public int? SecondChoice {get;set;}
    [JsonRequired] public int TieTick {get;set;}
    [JsonRequired] public bool ReopenedWin {get;set;}
    [JsonRequired] public bool BoundaryReached {get;set;}
}
public static class CasinoBlackjackService
{
    public const double TickSeconds=.050;
    public static readonly IReadOnlyList<int> Multipliers=Array.AsReadOnly(new[]{1,2,4,6,20,25,100});
    public static CasinoResult Open(GameState s)
    {
        if (!CasinoPokerService.OwnsCasino(s)) return CasinoResult.NotOwned;
        if (FacilityService.HasPendingDecision(s)) return CasinoResult.EventPending;
        if (s.Casino.Coins<=0) return CasinoResult.NoCoins;
        // 009951FC resets bet, phase, timer, blink and lose flag, not shared payout/bonus.
        s.Casino.Bet=0;s.Casino.Poker=new();s.Casino.Blackjack=ResetForm(s.Casino.Blackjack);s.Casino.BlackjackOpen=true;return CasinoResult.Success;
    }
    public static BlackjackRoundState ResetForm(BlackjackRoundState previous)=>new()
    {Win=previous.Win,AwardIndex=previous.AwardIndex,AwardTick=previous.AwardTick,ReopenedWin=previous.Win};
    public static bool Close(GameState s)
    {
        if (!s.Casino.BlackjackOpen) return false;
        s.Casino.BlackjackOpen=false;s.CompanyActivityPerformed=true;return true;
    }
    public static bool CanCommand(GameState s,int command)
    {
        var c=s.Casino;var b=c.Blackjack;
        if (!c.BlackjackOpen || b.BoundaryReached) return false;
        if (command==0) return true;
        if (b.Win) return (b.AwardReady || b.ReopenedWin && command==2) && (command==1 && c.PendingPayout>0 && c.PendingPayout<=long.MaxValue/2 || command==2 && c.Coins<=long.MaxValue-c.PendingPayout);
        return command switch {1=>b.Phase==0 && b.DealEnabled || b.Phase==2,2=>b.Phase==0 && c.Coins>0 && c.Bet<50 || b.Phase==2,_=>false};
    }
    public static CasinoResult Command(GameState s,int command,Random random)
    {
        var c=s.Casino;var b=c.Blackjack;
        if (!c.BlackjackOpen) return CasinoResult.InvalidAction;
        // Unlike poker, the blackjack command entry has no unused Rnd call.
        if (command==0) {Close(s);return CasinoResult.Success;}
        if (b.Win && (b.AwardReady || b.ReopenedWin) && (command==1 && c.PendingPayout>long.MaxValue/2 || command==2 && c.Coins>long.MaxValue-c.PendingPayout)) return CasinoResult.NumericLimit;
        if (!CanCommand(s,command)) return CasinoResult.InvalidAction;
        if (b.Win)
        {
            b.AwardReady=false;
            if (command==1) PrepareDouble(b,random);else {b.Phase=4;b.CollectTick=0;}
        }
        else if (b.Phase==0 && command==2) {c.Coins--;c.Bet++;b.DealEnabled=true;}
        else if (b.Phase==0)
        {
            b.Phase=1;b.Tick=0;b.ReopenedWin=false;b.PlayerCount=b.DealerCount=0;b.Natural=false;b.DoubleCards=[];b.Cards=[];b.UsedCards=[];b.DealEnabled=false;
            for (int side=0;side<2;side++) for (int i=0;i<9;i++) b.Cards.Add(CasinoPokerService.Draw(b.UsedCards,random));
        }
        else if (command==1)
        {
            if (b.PlayerCount==9) {b.BoundaryReached=true;return CasinoResult.InvalidAction;}
            b.Phase=3;b.Tick=0;
        }
        else
        {
            int award=StayAward(b.Cards.Take(b.PlayerCount).ToArray());
            if (award>=0) Award(c,b,award);else {b.Phase=8;b.Tick=0;}
        }
        return CasinoResult.Success;
    }
    public static int InitialTotal(CasinoCard first,CasinoCard second)
    {
        int a=first.Rank==0 ? 11 : Math.Min(first.Rank+1,10);
        int d=second.Rank==0 && a<11 ? 11 : Math.Min(second.Rank+1,10);
        return a+d;
    }
    public static int Score(IReadOnlyList<CasinoCard> cards)
    {
        // Native retry downgrades all marked eleven-valued aces together.
        int sum=cards.Sum(c=>c.Rank==0 ? 11 : Math.Min(c.Rank+1,10));
        if (sum>21) sum-=10*cards.Count(c=>c.Rank==0);
        return sum;
    }
    public static int StayAward(IReadOnlyList<CasinoCard> cards)=>cards.Count switch
    {5=>2,6=>3,7=>4,3 when cards.All(c=>c.Rank==5)=>5,>=8=>6,_=>-1};
    public static int DealerOutcome(int player,int dealer)=>dealer>21 ? 1 : dealer==player ? 0 : dealer>player ? -1 : dealer<=16 ? -2 : 1;
    private static void Award(CasinoState c,BlackjackRoundState b,int index)
    {b.Win=true;b.ReopenedWin=false;b.AwardIndex=index;b.AwardTick=1;b.AwardReady=false;c.PendingPayout=checked(c.Bet*(long)Multipliers[index]);}
    private static void Lose(CasinoState c,BlackjackRoundState b)
    {b.Phase=0;c.Bet=0;b.Win=false;b.ReopenedWin=false;b.AwardReady=false;b.DealEnabled=false;}
    private static void PrepareDouble(BlackjackRoundState b,Random random)
    {
        b.Phase=5;b.Tick=0;b.ReopenedWin=false;b.TieTick=0;b.DoubleCards=[];b.UsedCards=[];b.Marks=[0,0,0,0,0];b.FirstChoice=b.SecondChoice=null;
        for (int i=0;i<5;i++) b.DoubleCards.Add(CasinoPokerService.Draw(b.UsedCards,random));
    }
    public static bool SelectCard(GameState s,int index)
    {
        var b=s.Casino.Blackjack;
        if (!s.Casino.BlackjackOpen || index is <0 or >4 || b.DoubleCards.Count!=5) return false;
        // Handler contains no distinct-index guard; the UI hides the first chosen card.
        if (b.Phase==5) {b.FirstChoice=index;b.Marks[index]=1;b.Phase=6;return true;}
        if (b.Phase==6) {b.SecondChoice=index;b.Marks[index]=2;b.Phase=7;b.Tick=0;return true;}
        return false;
    }
    public static void Tick(GameState s,Random random)
    {
        var c=s.Casino;var b=c.Blackjack;
        if (!c.BlackjackOpen || b.BoundaryReached) return;
        // Sequential source blocks matter: a resolved comparison can enter phase 3 below.
        if (b.Phase==1 && !b.Win)
        {
            b.Tick++;
            if (b.Tick is 18 or 36) b.PlayerCount++;
            if (b.Tick==36) {b.PlayerTotal=InitialTotal(b.Cards[0],b.Cards[1]);b.Natural=b.PlayerTotal==21;}
            if (b.Tick==54) b.DealerCount++;
            if (b.Tick==75) {if (b.Natural) Award(c,b,1);else {b.Phase=2;b.Tick=0;}}
        }
        if (b.Phase==3 && !b.Win)
        {
            if (++b.Tick==10)
            {
                b.PlayerCount++;b.PlayerTotal=Score(b.Cards.Take(b.PlayerCount).ToArray());b.Phase=2;
                if (b.PlayerTotal>21) Lose(c,b);
            }
        }
        if (b.Phase==4 && c.PendingPayout>=1 && ++b.CollectTick>=2)
        {
            if (c.Coins==long.MaxValue) {b.CollectTick=1;b.BoundaryReached=true;return;}
            b.CollectTick=0;c.PendingPayout--;c.Coins++;
            if (c.PendingPayout==0) Lose(c,b);
        }
        if (b.Phase==7 && b.TieTick==0)
        {
            b.Tick++;
            if (b.Tick is 9 or 18 or 27)
            {int i=b.Marks.FindIndex(x=>x==0);if (i>=0) b.Marks[i]=3;}
            if (b.Tick==36 && b.SecondChoice is int second) b.Marks[second]=4;
            if (b.Tick==38)
            {
                int a=b.DoubleCards[b.FirstChoice!.Value].HighRank,d=b.DoubleCards[b.SecondChoice!.Value].HighRank;
                if (a>d) Lose(c,b);
                else if (a==d) b.TieTick=1;
                else {c.PendingPayout=checked(c.PendingPayout*2);b.Phase=3;b.AwardReady=true;}
            }
        }
        if (b.Phase==8 && !b.Win)
        {
            if (++b.Tick==10)
            {
                if (b.DealerCount==9) {b.BoundaryReached=true;return;}
                b.DealerCount++;b.DealerTotal=Score(b.Cards.Skip(9).Take(b.DealerCount).ToArray());
                int result=DealerOutcome(b.PlayerTotal,b.DealerTotal);
                if (result==-2) b.Tick=-9;
                else if (result==-1) Lose(c,b);
                else Award(c,b,result);
            }
        }
        if (b.TieTick>=1 && ++b.TieTick==30) PrepareDouble(b,random);
        if (b.Win && !b.AwardReady && b.AwardTick<30 && b.Phase is not (4 or 5 or 6 or 7) && ++b.AwardTick==30) b.AwardReady=true;
    }
    public static void Validate(GameState s)
    {
        if (s.Casino is not { } c || c.Blackjack is not { } b || c.PokerOpen && c.BlackjackOpen ||
            b.Phase is <0 or >8 || b.Tick is <-9 or >75 || b.PlayerCount is <0 or >9 || b.DealerCount is <0 or >9 || b.PlayerTotal is <0 or >31 || b.DealerTotal is <0 or >31 ||
            b.AwardIndex is <0 or >6 || b.AwardTick is <0 or >30 || b.CollectTick is <0 or >1 || b.TieTick is <0 or >=30 ||
            b.Cards is null || b.UsedCards is null || b.DoubleCards is null || b.Marks is null || b.Marks.Count!=5 || b.Marks.Any(x=>x is <0 or >4) ||
            b.Cards.Count is not (0 or 18) || b.DoubleCards.Count is not (0 or 5) || b.FirstChoice is <0 or >4 || b.SecondChoice is <0 or >4 ||
            b.UsedCards.Count>18 || b.UsedCards.Any(i=>i is <0 or >51) || !ValidCards(b.Cards) || !ValidCards(b.DoubleCards) ||
            b.Phase is 1 or 2 or 8 && (b.Cards.Count!=18 || c.Bet==0) || b.Phase==3 && !b.Win && (b.Cards.Count!=18 || c.Bet==0) || b.Phase is 5 or 6 or 7 && b.DoubleCards.Count!=5 ||
            b.Phase is 6 or 7 && b.FirstChoice is null || b.Phase==7 && b.SecondChoice is null ||
            b.ReopenedWin && !b.Win || b.TieTick>0 && (b.Phase!=7 || b.Tick!=38) || b.AwardReady && (!b.Win || c.PendingPayout==0 && !b.ReopenedWin) ||
            b.Phase is 4 or 5 or 6 or 7 && (!b.Win || c.PendingPayout==0 && !(b.Phase==4 && b.ReopenedWin)) ||
            b.Phase is 5 or 6 or 7 && c.PendingPayout>long.MaxValue/2 || b.Phase==4 && c.Coins>long.MaxValue-c.PendingPayout ||
            b.UsedCards.Count!=(b.DoubleCards.Count==5 ? 5 : b.Cards.Count) ||
            (b.DoubleCards.Count==5 ? b.DoubleCards : b.Cards).Any(x=>!b.UsedCards.Contains(x.Identifier)) ||
            c.BlackjackOpen && (!CasinoPokerService.OwnsCasino(s) || FacilityService.HasPendingDecision(s,ignoreCasino:true)))
            throw new InvalidDataException("Invalid casino blackjack state.");
    }
    private static bool ValidCards(List<CasinoCard> cards)=>cards.All(c=>c.Suit is >=0 and <=3 && c.Rank is >=0 and <=12) && cards.Select(c=>c.Identifier).Distinct().Count()==cards.Count;
}
