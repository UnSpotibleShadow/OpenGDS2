using System.Text.Json.Serialization;
namespace OpenGDS2.Domain;
public sealed class CardChainRoundState
{
    [JsonRequired] public PokerRoundState Round {get;set;}=new();
    [JsonRequired] public int ChainCount {get;set;}
    [JsonRequired] public CasinoCard? PreviousCard {get;set;}
    [JsonRequired] public int SelectedCard {get;set;}
    [JsonRequired] public bool DeckExhausted {get;set;}
    [JsonRequired] public bool EarlyDoubleSelection {get;set;}
    [JsonRequired] public bool NumericBoundary {get;set;}
}
public static class CasinoCardChainService
{
    public const double TickSeconds=.040;
    public static readonly IReadOnlyList<int> Multipliers=Array.AsReadOnly(new[]{1,2,3,4,6,8,10,13,17,20,25,30,40,50,55,60,70,80,100});
    public static int ChainMultiplier(int count)=>count switch
    {<10=>0,<=13=>1,<=16=>2,<=19=>3,<=22=>4,<=25=>6,<=27=>8,<=29=>10,<=31=>13,<=33=>17,<=35=>20,<=37=>25,<=39=>30,<=41=>40,<=43=>50,<=45=>55,<=47=>60,<=49=>70,<=51=>80,_=>100};
    public static int BonusMultiplier(IReadOnlyList<CasinoCard> cards)=>CasinoPokerService.Classify(cards) switch
    {5=>1,6=>2,7=>5,8 or 9=>20,_=>0};
    public static CasinoResult Open(GameState s)
    {
        if (!CasinoPokerService.OwnsCasino(s)) return CasinoResult.NotOwned;
        if (FacilityService.HasPendingDecision(s)) return CasinoResult.EventPending;
        if (s.Casino.Coins<=0) return CasinoResult.NoCoins;
        s.Casino.Poker=new();s.Casino.Blackjack=CasinoBlackjackService.ResetForm(s.Casino.Blackjack);
        s.Casino.CardChain=new();s.Casino.CardChainOpen=true;return CasinoResult.Success;
    }
    public static bool Close(GameState s)
    {
        if (!s.Casino.CardChainOpen) return false;
        s.Casino.CardChainOpen=false;s.CompanyActivityPerformed=true;return true;
    }
    public static bool CanCommand(GameState s,int command)
    {
        var c=s.Casino;var r=c.CardChain;var p=r.Round;
        if (!c.CardChainOpen || r.DeckExhausted || r.NumericBoundary) return false;
        return command switch {0=>p.Phase==0 && c.Coins>0 && c.Bet<50 || p.Phase==3 && c.Coins<=long.MaxValue-c.PendingPayout,
            1=>p.Phase==0 && p.DealEnabled || p.Phase==3 && c.PendingPayout>0 && c.PendingPayout<=long.MaxValue/2,_=>false};
    }
    public static CasinoResult Command(GameState s,int command,Random random,bool direct=false)
    {
        var c=s.Casino;var r=c.CardChain;var p=r.Round;
        if (!c.CardChainOpen) return CasinoResult.InvalidAction;
        _=random.NextSingle(); // 009C8A8C even rejected commands.
        if (p.Phase==3 && (command==0 && c.Coins>long.MaxValue-c.PendingPayout || command==1 && c.PendingPayout>long.MaxValue/2)) return CasinoResult.NumericLimit;
        if (r.DeckExhausted || r.NumericBoundary || !CanCommand(s,command) && !(direct && command==1 && p.Phase is 0 or 1)) return CasinoResult.InvalidAction;
        if (command==0 && p.Phase==0)
        {
            c.Coins--;c.Bet++;c.ChainBonus=0;p.DealEnabled=true;p.Outcome=0;
        }
        else if (command==0) {p.Phase=4;p.Tick=0;p.AwardReady=false;}
        else if (p.Phase==0)
        {
            p.Phase=1;p.Tick=0;p.Cards=[];p.UsedCards=[];p.Holds=[0,0,0,0,0];p.Revealed=[true,true,true,true,true];p.DealEnabled=false;
            r.ChainCount=0;r.PreviousCard=null;r.EarlyDoubleSelection=false;
            for(int i=0;i<5;i++) p.Cards.Add(CasinoPokerService.Draw(p.UsedCards,random));
            // Initial command +0C1A invokes the same bonus classifier as timer +09C4.
            AddBonus(c,r);
        }
        else if (p.Phase==1)
        {
            // Keyboard w reaches the copied replacement branch despite disabled Deal.
            p.Phase=2;p.Tick=0;p.Outcome=0;
            for(int i=0;i<5;i++) if (p.Holds[i]==0)
            {
                if (p.UsedCards.Count==52) {r.DeckExhausted=true;break;}
                p.Cards[i]=CasinoPokerService.Draw(p.UsedCards,random);
            }
        }
        else {r.EarlyDoubleSelection=false;CasinoPokerService.PrepareDouble(p);p.AwardReady=false;}
        return CasinoResult.Success;
    }
    public static bool CanSelect(GameState s,int index,bool direct=false)
    {
        var c=s.Casino;var r=c.CardChain;var p=r.Round;
        if (!c.CardChainOpen || r.DeckExhausted || r.NumericBoundary || index is <0 or >4 || p.Cards.Count!=5) return false;
        if (p.Phase==1) return r.ChainCount==0 || r.PreviousCard is CasinoCard previous && Matches(previous,p.Cards[index]);
        return p.Phase==5 && (direct || p.Tick>=18 && index>=1);
    }
    public static bool SelectCard(GameState s,int index,bool direct=false)
    {
        if (!CanSelect(s,index,direct)) return false;
        var r=s.Casino.CardChain;var p=r.Round;
        if (p.Phase==1) {r.SelectedCard=index;p.Tick=0;p.Phase=2;}
        else {r.EarlyDoubleSelection=p.Tick<18;p.Holds[index]=1;p.ChosenCard=index;p.Tick=0;p.Phase=6;}
        return true;
    }
    public static bool Matches(CasinoCard previous,CasinoCard next)=>previous.Suit==next.Suit || previous.Rank==next.Rank;
    private static void AddBonus(CasinoState c,CardChainRoundState r)
    {
        int category=CasinoPokerService.Classify(r.Round.Cards);r.Round.Outcome=category==9 ? 8 : category;
        long bonus=c.Bet*(long)BonusMultiplier(r.Round.Cards);
        if (c.ChainBonus>long.MaxValue-bonus) {r.NumericBoundary=true;return;}
        c.ChainBonus+=bonus;
    }
    public static void Tick(GameState s,Random random)
    {
        var c=s.Casino;var r=c.CardChain;var p=r.Round;
        if (!c.CardChainOpen || r.DeckExhausted || r.NumericBoundary) return;
        if (p.Phase==2)
        {
            p.Tick++;
            if (p.Tick==1) r.PreviousCard=p.Cards[r.SelectedCard];
            if (p.Tick==3)
            {
                p.Phase=1;
                if (p.UsedCards.Count==52) {r.DeckExhausted=true;return;}
                p.Cards[r.SelectedCard]=CasinoPokerService.Draw(p.UsedCards,random);r.ChainCount++;AddBonus(c,r);
                if (r.NumericBoundary) return;
                if (!p.Cards.Any(card=>Matches(r.PreviousCard!.Value,card)))
                {
                    if (r.ChainCount>=10 || c.ChainBonus>0)
                    {
                        // Below ten, native code retains pending before adding bonus.
                        long basis=r.ChainCount>=10 ? c.Bet*(long)ChainMultiplier(r.ChainCount) : c.PendingPayout;
                        if (basis>long.MaxValue-c.ChainBonus) {r.NumericBoundary=true;return;}
                        c.PendingPayout=basis+c.ChainBonus;p.Phase=3;p.AwardReady=true;
                    }
                    else {p.Phase=0;c.Bet=0;p.DealEnabled=false;}
                }
            }
        }
        else if (p.Phase==4 && c.PendingPayout>0)
        {
            if (++p.Tick==2)
            {
                int amount=c.PendingPayout>=150 ? 10 : 1;
                if (c.Coins>long.MaxValue-amount) {p.Tick=1;r.NumericBoundary=true;return;}
                p.Tick=0;c.Coins+=amount;c.PendingPayout-=amount;
                if (c.PendingPayout==0) {p.Phase=0;c.Bet=0;p.DealEnabled=false;}
            }
        }
        else if (p.Phase==6 && p.ChosenCard==0)
        {
            // Source comparison iterates choices 1..4; keyboard dealer choice never resolves.
            if (p.Tick<28)
            {
                p.Tick++;
                if (p.Tick is 1 or 10 or 19)
                {
                    int other=Enumerable.Range(1,4).First(i=>p.Holds[i]==0);
                    p.Holds[other]=2;p.Revealed[other]=true;
                }
            }
        }
        else if (p.Phase is 5 or 6)
        {
            CasinoPokerService.TickDouble(c,p,random);
            if (p.Phase==5) r.EarlyDoubleSelection=false;
        }
    }
    public static void Validate(GameState s)
    {
        if (s.Casino is not { } c || c.CardChain is not { } r || r.Round is not { } p ||
            r.ChainCount is <0 or >47 || r.SelectedCard is <0 or >4 || r.PreviousCard is CasinoCard prev && (prev.Suit is <0 or >3 || prev.Rank is <0 or >12) ||
            p.Phase is <0 or >6 || p.Tick is <0 or >28 || p.Phase==2 && p.Tick>2 && !r.DeckExhausted || p.Phase==4 && p.Tick>1 || p.Phase==5 && p.Tick>19 ||
            p.Cards is null || p.UsedCards is null || p.Holds is null || p.Revealed is null || p.RevealOrder is null ||
            p.Cards.Count is not (0 or 5) || p.Cards.Any(x=>x.Suit is <0 or >3 || x.Rank is <0 or >12) || p.Cards.Select(x=>x.Identifier).Distinct().Count()!=p.Cards.Count ||
            p.UsedCards.Count>52 || p.UsedCards.Any(x=>x is <0 or >51) || (p.Phase!=5 || p.Tick>=18) && !r.EarlyDoubleSelection && p.Cards.Any(x=>!p.UsedCards.Contains(x.Identifier)) ||
            p.Holds.Count!=5 || p.Holds.Any(x=>x is <0 or >2) || p.Revealed.Count!=5 || p.ChosenCard is <0 or >4 || p.Phase==6 && p.ChosenCard is null ||
            p.TieTick is <0 or >30 || p.TieRedeal && (p.Phase!=6 || p.Tick!=28 || p.TieTick is <2 or >=30) || p.Outcome is <0 or >9 ||
            p.Phase!=0 && p.Cards.Count!=5 || r.DeckExhausted && p.UsedCards.Count!=52 ||
            r.EarlyDoubleSelection && p.Phase is not (0 or 3 or 6) || p.Phase is 5 or 6 && (c.PendingPayout==0 || c.PendingPayout>long.MaxValue/2) || p.Phase==4 && c.Coins>long.MaxValue-c.PendingPayout ||
            c.CardChainOpen && (!CasinoPokerService.OwnsCasino(s) || FacilityService.HasPendingDecision(s,ignoreCasino:true)))
            throw new InvalidDataException("Invalid casino card-chain state.");
    }
}
