using Godot;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    public override void _Input(InputEvent input)
    {
        if (_session.Current is not { } state || !state.Casino.CardChainOpen || !_pokerInterfaceVisible || input is not InputEventKey {Pressed:true,Echo:false} key) return;
        long character=key.Unicode;
        if (character is >=49 and <=53) _session.SelectChainCard((int)character-49,direct:true);
        else if (character==113) _session.CardChainCommand(0,direct:true);
        else if (character==119) _session.CardChainCommand(1,direct:true);
        else return;
        _refreshPoker?.Invoke();GetViewport().SetInputAsHandled();
    }
    private void ShowCardChain(GameState state)
    {
        var canvas=CasinoCanvas("CasinoCardChainWindow","CASINO_CARD_CHAIN",422,346);PokerPayTable(canvas,true);
        _dialog!.SetTitle(Tr("CASINO_CARD_CHAIN"));
        _dialog.CloseRequested=()=>{CasinoCardChainService.Close(state);ShowOfficeReconstruction(state);};
        var totals=NativeText(canvas,"CardChainTotals","",new(4,251,414,22),10);
        var message=NativeText(canvas,"CardChainPhase","",new(4,278,414,32),10);message.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var referenceCard=PlaceCasinoCard(canvas,"CardChainPreviousCard",174,28,()=>{});referenceCard.Size=new(71,96);referenceCard.Disabled=true;
        NativeText(canvas,"ChainDeckCaption",Tr("CARD_CHAIN_REMAINING"),new(255,38,158,28),10).AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var remaining=NativeText(canvas,"ChainDeckRemaining","",new(255,67,158,45),26,HorizontalAlignment.Center);
        var cards=new List<NativeCasinoCard>();
        for(int i=0;i<5;i++){int choice=i;cards.Add(PlaceCasinoCard(canvas,$"CardChainCard{i}",12+80*i,132,()=>{_session.SelectChainCard(choice);_refreshPoker?.Invoke();}));}
        var bet=NativeAction(canvas,"CardChainBetCollect","POKER_ADD_BET",new(194,315,70,25),()=>{_session.CardChainCommand(0);_refreshPoker?.Invoke();});
        var deal=NativeAction(canvas,"CardChainDealDouble","POKER_DEAL",new(270,315,65,25),()=>{_session.CardChainCommand(1);_refreshPoker?.Invoke();});
        NativeAction(canvas,"CloseCardChain","BACK_TO_OFFICE",new(341,315,77,25),()=>{CasinoCardChainService.Close(state);ShowOfficeReconstruction(state);}).TooltipText=Tr("CASINO_CLOSE_NOTE")+"\n"+Tr("CARD_CHAIN_RULES")+"\n"+Tr("CARD_CHAIN_KEYS");
        var previous=NativeText(canvas,"CardChainReference","",new(4,315,184,25),10);
        _refreshPoker=()=>
        {
            if (!_pokerInterfaceVisible) return;
            var c=state.Casino;var r=c.CardChain;var p=r.Round;
            totals.Text=string.Format(DisplayNumbers.Provider,Tr("CARD_CHAIN_TOTALS"),c.Coins,c.Bet,c.PendingPayout,r.ChainCount,c.ChainBonus);
            message.Text=Tr(r.DeckExhausted ? "CARD_CHAIN_EXHAUSTED" : r.NumericBoundary ? "CASINO_RESULT_NumericLimit" : p.Phase==6 && p.ChosenCard==0 ? "CARD_CHAIN_DEALER_KEY" : p.Phase switch
            {0=>c.Bet==50 ? "POKER_RETAINED_BET" : "POKER_BET",1=>"CARD_CHAIN_SELECT",2=>"POKER_REVEAL",3=>"POKER_WIN",4=>c.PendingPayout==0 ? "CARD_CHAIN_ZERO_WAIT" : "POKER_COLLECTING",5=>p.Tick>=18 ? "POKER_CHOOSE_DOUBLE" : "POKER_REVEAL",_=>p.TieRedeal ? "POKER_TIE" : "POKER_REVEAL"});
            if (p.Phase==3 && (c.PendingPayout>long.MaxValue/2 || c.Coins>long.MaxValue-c.PendingPayout)) message.Text+="\n"+Tr("CASINO_RESULT_NumericLimit");
            referenceCard.Present(r.PreviousCard??default,r.PreviousCard.HasValue,r.PreviousCard.HasValue);remaining.Text=(52-p.UsedCards.Count).ToString();
            previous.Text=r.PreviousCard is CasinoCard reference?Tr("CARD_CHAIN_REFERENCE")+" "+CardFace(reference).Replace('\n',' '):"";
            bet.Text=Tr(p.Phase is 3 or 4 ? "POKER_COLLECT" : "POKER_ADD_BET");deal.Text=Tr(p.Phase is >=3 and <=6 ? "POKER_DOUBLE" : "POKER_DEAL");
            bet.Disabled=!CasinoCardChainService.CanCommand(state,0);deal.Disabled=!CasinoCardChainService.CanCommand(state,1);
            for(int i=0;i<5;i++)
            {
                bool revealed=p.Cards.Count==5 && (p.Phase<5 || p.Revealed[i]);
                cards[i].Present(revealed?p.Cards[i]:default,revealed,p.Cards.Count==5);
                cards[i].Disabled=!CasinoCardChainService.CanSelect(state,i);
                cards[i].SetMeta("revealed",revealed);
            }
        };
        _pokerElapsed=0;_pokerInterfaceVisible=true;_refreshPoker();RefreshDesktop();
    }
}
