using System.Text.Json.Serialization;

namespace OpenGDS2.Domain;

public readonly record struct CasinoCard(int Suit,int Rank)
{
    public int Identifier=>Suit*13+Rank;
    public int HighRank=>Rank==0 ? 13 : Rank;
}
public sealed class CasinoState
{
    [JsonRequired] public long Coins {get;set;}
    // Original shared transient cells. Native form unload does not refund or drain them.
    [JsonRequired] public int Bet {get;set;}
    [JsonRequired] public long PendingPayout {get;set;}
    [JsonRequired] public long ChainBonus {get;set;}
    [JsonRequired] public bool PokerOpen {get;set;}
    [JsonRequired] public PokerRoundState Poker {get;set;}=new();
    // Optional additions permit pre-M86 modern saves to load.
    public bool BlackjackOpen {get;set;}
    public BlackjackRoundState Blackjack {get;set;}=new();
    [JsonIgnore] public bool CardGameOpen=>PokerOpen || BlackjackOpen;
}
public sealed class PokerRoundState
{
    [JsonRequired] public int Phase {get;set;}
    [JsonRequired] public int Tick {get;set;}
    [JsonRequired] public int Outcome {get;set;}
    [JsonRequired] public List<CasinoCard> Cards {get;set;}=[];
    [JsonRequired] public HashSet<int> UsedCards {get;set;}=[];
    [JsonRequired] public List<int> Holds {get;set;}=[0,0,0,0,0];
    [JsonRequired] public List<bool> Revealed {get;set;}=[false,false,false,false,false];
    [JsonRequired] public List<int> RevealOrder {get;set;}=[];
    [JsonRequired] public bool DealEnabled {get;set;}
    [JsonRequired] public bool AwardReady {get;set;}
    [JsonRequired] public int AwardTick {get;set;}
    [JsonRequired] public int? ChosenCard {get;set;}
    [JsonRequired] public int TieTick {get;set;}
    [JsonRequired] public bool TieRedeal {get;set;}
}
public enum CasinoResult { Success, NotOwned, EventPending, NoCoins, MoneyUnknown, InvalidAction, NumericLimit }

public static class CasinoPokerService
{
    public const long CoinPurchasePrice=10_000_000;
    public const int CoinsPerPurchase=50;
    public const double TickSeconds=.030;
    public static readonly IReadOnlyList<int> Multipliers=Array.AsReadOnly(new[]{0,1,2,3,10,15,20,100,500,1000});
    public static bool OwnsCasino(GameState state)=>state.Calendar?.OriginalSlots0444.Contains((int)FacilityKind.Casino)==true;
    public static CasinoResult BuyCoins(GameState state)
    {
        if (!OwnsCasino(state)) return CasinoResult.NotOwned;
        if (FacilityService.HasPendingDecision(state)) return CasinoResult.EventPending;
        if (state.Money is null) return CasinoResult.MoneyUnknown;
        if (state.Casino.Coins>long.MaxValue-CoinsPerPurchase) return CasinoResult.NumericLimit;
        var finance=FinancialRules.Prepare(state,-CoinPurchasePrice);
        finance.Commit(state);state.Casino.Coins+=CoinsPerPurchase;
        return CasinoResult.Success;
    }
    public static CasinoResult Open(GameState state)
    {
        if (!OwnsCasino(state)) return CasinoResult.NotOwned;
        if (FacilityService.HasPendingDecision(state)) return CasinoResult.EventPending;
        if (state.Casino.Coins<=0) return CasinoResult.NoCoins;
        // Native Form_Load resets phase/outcome; retains shared bet and payout.
        state.Casino.Blackjack=CasinoBlackjackService.ResetForm(state.Casino.Blackjack);state.Casino.Poker=new() {Phase=0,Tick=0,Outcome=0};state.Casino.PokerOpen=true;
        return CasinoResult.Success;
    }
    public static bool Close(GameState state)
    {
        if (!state.Casino.PokerOpen) return false;
        state.Casino.PokerOpen=false;state.CompanyActivityPerformed=true; // 00978364, member 01CC.
        return true;
    }
    public static bool CanCommand(GameState state,int command)
    {
        var c=state.Casino;var p=c.Poker;
        if (!c.PokerOpen) return false;
        return command switch
        {
            0=>p.Phase==0 && c.Coins>0 && c.Bet<50 || p.Phase==3 && p.AwardReady && c.Coins<=long.MaxValue-c.PendingPayout,
            1=>p.Phase==0 && p.DealEnabled || p.Phase==1 && p.Tick>=45 || p.Phase==3 && p.AwardReady && c.PendingPayout<=long.MaxValue/2,
            _=>false
        };
    }
    public static CasinoResult Command(GameState state,int command,Random random)
    {
        if (!state.Casino.PokerOpen) return CasinoResult.InvalidAction;
        _=random.NextSingle(); // 009B9DE4 entry, including rejected commands.
        if (state.Casino.Poker.Phase==3 && state.Casino.Poker.AwardReady &&
            (command==0 && state.Casino.Coins>long.MaxValue-state.Casino.PendingPayout || command==1 && state.Casino.PendingPayout>long.MaxValue/2)) return CasinoResult.NumericLimit;
        if (!CanCommand(state,command)) return CasinoResult.InvalidAction;
        var c=state.Casino;var p=c.Poker;
        if (command==0)
        {
            if (p.Phase==0)
            {
                c.Coins--;c.Bet++;p.Outcome=0;p.Holds=[0,0,0,0,0];p.DealEnabled=true;
                p.Revealed=[false,false,false,false,false];
            }
            else {p.Phase=4;p.Tick=0;p.AwardReady=false;}
        }
        else if (p.Phase==0) Deal(p,1,random);
        else if (p.Phase==1)
        {
            p.Phase=2;p.Tick=0;p.DealEnabled=false;p.RevealOrder=[];
            for (int i=0;i<5;i++) if (p.Holds[i]==0)
            {p.Cards[i]=Draw(p.UsedCards,random);p.Revealed[i]=false;p.RevealOrder.Add(i);}
        }
        else
        {
            // A fresh deck on each double-up; award/stake are not debited again.
            PrepareDouble(p);p.AwardReady=false;
        }
        return CasinoResult.Success;
    }
    private static void Deal(PokerRoundState p,int phase,Random random)
    {
        p.Phase=phase;p.Tick=0;p.Cards=[];p.UsedCards=[];p.Holds=[0,0,0,0,0];
        p.Revealed=[false,false,false,false,false];p.RevealOrder=[0,1,2,3,4];
        p.DealEnabled=false;p.ChosenCard=null;p.TieRedeal=false;
        for (int i=0;i<5;i++) p.Cards.Add(Draw(p.UsedCards,random));
    }
    private static void PrepareDouble(PokerRoundState p)
    {
        p.Phase=5;p.Tick=0;p.UsedCards=[];p.Holds=[0,0,0,0,0];p.Revealed=[false,false,false,false,false];
        p.ChosenCard=null;p.TieRedeal=false;p.TieTick=0;p.DealEnabled=false;
    }
    public static CasinoCard Draw(HashSet<int> used,Random random)
    {
        if (used.Count>=52) throw new InvalidOperationException("The used-card map is exhausted.");
        while (true)
        {
            int suit=(int)Math.Floor((double)random.NextSingle()*4),rank=(int)Math.Floor((double)random.NextSingle()*13);
            var card=new CasinoCard(suit,rank);
            if (used.Add(card.Identifier)) return card;
        }
    }
    public static bool SelectCard(GameState state,int index)
    {
        var c=state.Casino;var p=c.Poker;
        if (!c.PokerOpen || index is <0 or >4 || p.Cards.Count!=5) return false;
        if (p.Phase==1) {p.Holds[index]=p.Holds[index]==0 ? 1 : 0;return true;}
        if (p.Phase==5 && p.Tick>=18 && index>=1)
        {p.Holds[index]=1;p.ChosenCard=index;p.Phase=6;p.Tick=0;return true;}
        return false;
    }
    public static void Tick(GameState state,Random random)
    {
        var c=state.Casino;var p=c.Poker;
        if (!c.PokerOpen) return;
        if (p.Phase==1)
        {
            if (p.Tick<46) p.Tick++;
            if (p.Tick<=45 && p.Tick%9==0) p.Revealed[p.Tick/9-1]=true;
        }
        else if (p.Phase==2)
        {
            p.Tick++;
            if (p.Tick%9==0 && p.Tick/9<=p.RevealOrder.Count) p.Revealed[p.RevealOrder[p.Tick/9-1]]=true;
            if (p.RevealOrder.All(i=>p.Revealed[i]))
            {
                p.Phase=3;p.Outcome=Classify(p.Cards);p.AwardReady=false;p.AwardTick=1;
                if (p.Outcome==0) {p.Phase=0;c.Bet=0;p.DealEnabled=false;}
            }
        }
        else if (p.Phase==3 && !p.AwardReady)
        {
            // Six classification blink events, then the source's award counter.
            if (++p.AwardTick==34)
            {c.PendingPayout=checked(c.Bet*(long)Multipliers[p.Outcome]);p.AwardReady=true;}
        }
        else if (p.Phase==4)
        {
            if (++p.Tick>=2 && c.PendingPayout>=1)
            {
                if (c.Coins==long.MaxValue) return;
                p.Tick=0;c.PendingPayout--;c.Coins++;
                if (c.PendingPayout==0) {p.Phase=0;c.Bet=0;p.DealEnabled=false;}
            }
        }
        else if (p.Phase==5)
        {
            if (p.Tick<19) p.Tick++;
            if (p.Tick==18)
            {
                p.Cards=[];
                for (int i=0;i<5;i++) p.Cards.Add(Draw(p.UsedCards,random));
                p.Revealed[0]=true;
            }
        }
        else if (p.Phase==6)
        {
            if (p.TieRedeal)
            {
                if (++p.TieTick==30) PrepareDouble(p);
                return;
            }
            p.Tick++;
            if (p.Tick is 1 or 10 or 19)
            {
                int other=Enumerable.Range(1,4).FirstOrDefault(i=>p.Holds[i]==0,-1);
                if (other>=1) {p.Holds[other]=2;p.Revealed[other]=true;}
            }
            if (p.Tick==28 && p.ChosenCard is int choice)
            {
                p.Revealed[choice]=true;int dealer=p.Cards[0].HighRank,player=p.Cards[choice].HighRank;
                if (player<dealer) {p.Phase=0;c.Bet=0;p.DealEnabled=false;}
                else if (player>dealer)
                {
                    if (c.PendingPayout>long.MaxValue/2) {p.Tick=27;return;}
                    c.PendingPayout*=2;p.Phase=3;p.AwardReady=true;
                }
                else {p.TieRedeal=true;p.TieTick=2;}
            }
        }
    }
    public static int Classify(IReadOnlyList<CasinoCard> cards)
    {
        if (cards.Count!=5 || cards.Any(c=>c.Suit is <0 or >3 || c.Rank is <0 or >12) || cards.Select(c=>c.Identifier).Distinct().Count()!=5)
            throw new ArgumentException("Expected five distinct valid cards.");
        var counts=cards.GroupBy(c=>c.Rank).ToDictionary(g=>g.Key,g=>g.Count());
        bool flush=cards.Select(c=>c.Suit).Distinct().Count()==1;
        var ranks=cards.Select(c=>c.Rank).Order().ToArray();
        bool royal=ranks.SequenceEqual(new[]{0,9,10,11,12});
        bool straight=royal || ranks.Zip(ranks.Skip(1)).All(x=>x.Second==x.First+1);
        if (flush && royal) return 9;
        if (flush && straight) return 8;
        if (counts.Values.Contains(4)) return 7;
        if (counts.Values.Contains(3) && counts.Values.Contains(2)) return 6;
        if (flush) return 5;
        if (straight) return 4;
        if (counts.Values.Contains(3)) return 3;
        if (counts.Values.Count(x=>x==2)==2) return 2;
        if (counts.Any(x=>x.Value==2 && (x.Key==0 || x.Key>=9))) return 1;
        return 0;
    }
    public static void Validate(GameState state)
    {
        CasinoBlackjackService.Validate(state);
        if (state.Casino is not { } c || c.Poker is not { } p || c.Coins<0 || c.Bet is <0 or >50 || c.PendingPayout<0 || c.ChainBonus<0 ||
            p.Phase is <0 or >6 || p.Tick is <0 or >46 || p.Phase==2 && p.Tick>45 || p.Phase==4 && p.Tick>1 || p.Phase==5 && p.Tick>19 || p.Phase==6 && p.Tick>28 || p.Outcome is <0 or >9 || p.AwardTick is <0 or >34 || p.TieTick is <0 or >30 ||
            p.Cards is null || p.UsedCards is null || p.Holds is null || p.Revealed is null || p.RevealOrder is null ||
            p.Cards.Count is not (0 or 5) || p.Holds.Count!=5 || p.Revealed.Count!=5 || p.Holds.Any(x=>x is <0 or >2) ||
            p.Cards.Any(x=>x.Suit is <0 or >3 || x.Rank is <0 or >12) || p.Cards.Select(x=>x.Identifier).Distinct().Count()!=p.Cards.Count ||
            p.UsedCards.Count>10 || p.UsedCards.Any(x=>x is <0 or >51) || (p.Phase!=5 || p.Tick>=18) && p.Cards.Any(x=>!p.UsedCards.Contains(x.Identifier)) ||
            p.RevealOrder.Any(x=>x is <0 or >4) || p.RevealOrder.Distinct().Count()!=p.RevealOrder.Count || p.ChosenCard is <1 or >4 ||
            p.Phase!=0 && (p.Cards.Count!=5 || c.Bet==0) || p.Phase==6 && p.ChosenCard is null ||
            p.Phase is 4 or 5 or 6 && c.PendingPayout==0 ||
            p.Phase==3 && (p.Outcome==0 || !p.AwardReady && p.AwardTick is <1 or >=34) ||
            p.Phase is 5 or 6 && c.PendingPayout>long.MaxValue/2 ||
            p.TieRedeal && (p.Phase!=6 || p.Tick!=28 || p.TieTick is <2 or >=30) || p.AwardReady && (p.Phase!=3 || c.PendingPayout==0) ||
            c.PokerOpen && (!OwnsCasino(state) || FacilityService.HasPendingDecision(state,ignoreCasino:true)))
            throw new InvalidDataException("Invalid casino poker state.");
    }
}
