using Godot;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private void ShowBlackjack(GameState state)
    {
        var canvas=CasinoCanvas("CasinoBlackjackWindow","CASINO_BLACKJACK",422,318);
        _dialog!.SetTitle(Tr("CASINO_BLACKJACK"));
        _dialog.CloseRequested=()=>{CasinoBlackjackService.Close(state);ShowOfficeReconstruction(state);};
        var totals=NativeText(canvas,"BlackjackTotals","",new(4,251,414,22),10);
        var message=NativeText(canvas,"BlackjackPhase","",new(4,274,176,40),10);message.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var dealer=NativeText(canvas,"BlackjackDealer","",new(238,36,180,20),10);
        var player=NativeText(canvas,"BlackjackPlayer","",new(4,150,142,20),10);
        var dealerCards=new List<NativeCasinoCard>();var playerCards=new List<NativeCasinoCard>();
        for(int i=0;i<9;i++){dealerCards.Add(PlaceCasinoCard(canvas,"BlackjackDealerCard"+i,8+16*i,38,()=>{}));playerCards.Add(PlaceCasinoCard(canvas,"BlackjackPlayerCard"+i,152+16*i,138,()=>{}));dealerCards[i].Disabled=playerCards[i].Disabled=true;}
        var cards=new List<NativeCasinoCard>();
        for(int i=0;i<5;i++){int choice=i;cards.Add(PlaceCasinoCard(canvas,$"BlackjackCard{i}",12+80*i,132,()=>{CasinoBlackjackService.SelectCard(state,choice);_refreshPoker?.Invoke();}));}
        var more=NativeAction(canvas,"BlackjackDealMoreDouble","POKER_DEAL",new(186,286,70,25),()=>{_session.BlackjackCommand(1);_refreshPoker?.Invoke();});
        var stay=NativeAction(canvas,"BlackjackBetStayCollect","POKER_ADD_BET",new(262,286,76,25),()=>{_session.BlackjackCommand(2);_refreshPoker?.Invoke();});
        NativeAction(canvas,"CloseBlackjack","BACK_TO_OFFICE",new(344,286,74,25),()=>{CasinoBlackjackService.Close(state);ShowOfficeReconstruction(state);}).TooltipText=Tr("BLACKJACK_RULES")+"\n"+Tr("CASINO_CLOSE_NOTE");
        _refreshPoker=()=>
        {
            if (!_pokerInterfaceVisible) return;
            var c=state.Casino;var b=c.Blackjack;
            totals.Text=string.Format(DisplayNumbers.Provider,Tr("POKER_TOTALS"),c.Coins,c.Bet,c.PendingPayout);
            bool doubling=b.Phase is 5 or 6 or 7;
            message.Text=Tr(b.BoundaryReached ? "BLACKJACK_CARD_BOUNDARY" : doubling ? b.TieTick>0 ? "POKER_TIE" : b.Phase==5 ? "BLACKJACK_FIRST" : b.Phase==6 ? "BLACKJACK_SECOND" : "POKER_REVEAL" : b.Phase==4 ? "POKER_COLLECTING" : b.Win ? b.AwardReady ? "POKER_WIN" : "POKER_RESULT_WAIT" : b.Phase==0 ? "POKER_BET" : b.Phase==2 ? "BLACKJACK_DECISION" : "POKER_REVEAL");
            if (b.Win && b.AwardReady && (c.PendingPayout>long.MaxValue/2 || c.Coins>long.MaxValue-c.PendingPayout)) message.Text+="\n"+Tr("CASINO_RESULT_NumericLimit");
            dealer.Text=Tr("BLACKJACK_DEALER_SCORE")+": "+(b.DealerCount==0?"?":CasinoBlackjackService.Score(b.Cards.Skip(9).Take(b.DealerCount).ToArray()).ToString());
            player.Text=Tr("BLACKJACK_PLAYER_SCORE")+": "+(b.PlayerCount==0?"?":b.PlayerTotal.ToString());
            for(int i=0;i<9;i++){dealerCards[i].Visible=!doubling&&i<b.DealerCount;playerCards[i].Visible=!doubling&&i<b.PlayerCount;if(i<b.DealerCount)dealerCards[i].Present(b.Cards[9+i],true);if(i<b.PlayerCount)playerCards[i].Present(b.Cards[i],true);}
            more.Text=Tr(b.Win ? "POKER_DOUBLE" : b.Phase==2 ? "BLACKJACK_MORE" : "POKER_DEAL");
            stay.Text=Tr(b.Win ? "POKER_COLLECT" : b.Phase==2 ? "BLACKJACK_STAY" : "POKER_ADD_BET");
            more.Disabled=!CasinoBlackjackService.CanCommand(state,1);stay.Disabled=!CasinoBlackjackService.CanCommand(state,2);
            for (int i=0;i<5;i++)
            {
                cards[i].Visible=doubling && !(b.Phase==6 && b.FirstChoice==i);
                bool revealed=doubling && b.Marks[i]>=3;
                cards[i].Present(revealed?b.DoubleCards[i]:default,revealed,true);
                cards[i].Disabled=b.Phase is not (5 or 6);
                cards[i].SetMeta("revealed",revealed);
            }
        };
        _pokerElapsed=0;_pokerInterfaceVisible=true;_refreshPoker();RefreshDesktop();
    }
}
