using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private bool _pokerInterfaceVisible;
    private double _pokerElapsed;
    private Action? _refreshPoker;
    private void TickPokerInterface(double delta)
    {
        if (!_pokerInterfaceVisible || _session.Current is not { } state || !state.Casino.ActivityOpen) return;
        if (state.Casino.HorseOpen) {TickHorseInterface(state,delta);return;}
        _pokerElapsed+=delta;
        if (_pokerElapsed>=(state.Casino.BlackjackOpen ? CasinoBlackjackService.TickSeconds : state.Casino.CardChainOpen ? CasinoCardChainService.TickSeconds : CasinoPokerService.TickSeconds))
        {
            // One callback, no elapsed-time catch-up burst. Native callbacks rearm after return.
            _pokerElapsed=0;if (state.Casino.BlackjackOpen) _session.TickBlackjack();else if (state.Casino.CardChainOpen) _session.TickCardChain();else _session.TickPoker();_refreshPoker?.Invoke();
        }
    }
    private void ShowCasino(GameState state)
    {
        if (state.Casino.PokerOpen) {ShowPoker(state);return;}
        if (state.Casino.BlackjackOpen) {ShowBlackjack(state);return;}
        if (state.Casino.CardChainOpen) {ShowCardChain(state);return;}
        if (state.Casino.HorseOpen) {ShowHorseRacing(state);return;}
        var canvas=ManagementCanvas("CasinoSelector",Tr("CASINO_ENTER"),new(214,276));
        NativeText(canvas,"CasinoCoins",string.Format(DisplayNumbers.Provider,Tr("CASINO_COINS"),state.Casino.Coins),new(8,4,198,22));
        bool cards=state.Casino.Coins>0 && !FacilityService.HasPendingDecision(state);
        void OpenCard(Func<CasinoResult> open,Action show)
        {
            var result=open();if(result==CasinoResult.Success)show();else _status.Text=Tr($"CASINO_RESULT_{result}");
        }
        // fGMTenantCasinoSn: horse, poker, blackjack, card change, coins, cancel.
        NativeAction(canvas,"CasinoHorse","CASINO_HORSE",new(8,30,198,25),()=>OpenCard(()=>_session.OpenHorse(_activeCatalog),()=>ShowHorseRacing(state)),!FacilityService.HasPendingDecision(state) && state.Money is not null && CasinoHorseService.HasCatalog(_activeCatalog));
        NativeAction(canvas,"CasinoPoker","CASINO_POKER",new(8,62,198,25),()=>OpenCard(()=>CasinoPokerService.Open(state),()=>ShowPoker(state)),cards);
        NativeAction(canvas,"CasinoBlackjack","CASINO_BLACKJACK",new(8,94,198,25),()=>OpenCard(()=>CasinoBlackjackService.Open(state),()=>ShowBlackjack(state)),cards);
        NativeAction(canvas,"CasinoCardChain","CASINO_CARD_CHAIN",new(8,126,198,25),()=>OpenCard(()=>CasinoCardChainService.Open(state),()=>ShowCardChain(state)),cards);
        NativeAction(canvas,"CasinoBuyCoins","CASINO_BUY_COINS",new(8,158,198,25),()=>ShowCoinPurchase(state),!FacilityService.HasPendingDecision(state) && state.Money is not null);
        _status=NativeText(canvas,"CasinoStatus",state.Casino.Coins<=0?Tr("CASINO_RESULT_NoCoins"):"",new(8,188,198,48),10);_status.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        if(!CasinoHorseService.HasCatalog(_activeCatalog))_status.Text+= "\n"+Tr("CASINO_RESULT_CatalogMissing");
        NativeAction(canvas,"CasinoCancel","BACK_TO_OFFICE",new(8,242,198,25),()=>ShowOfficeReconstruction(state));
    }
    private void ShowCoinPurchase(GameState state)
    {
        var canvas=ManagementCanvas("CasinoCoinPurchase",Tr("CASINO_BUY_COINS"),new(290,151));
        NativeFill(canvas,"CoinPurchaseMessageBox",new(8,8,274,60),Colors.White,true);
        NativeText(canvas,"CoinPurchaseMessage",Tr("CASINO_PURCHASE_CONFIRM"),new(12,12,266,52)).AutowrapMode=TextServer.AutowrapMode.WordSmart;
        _status=NativeText(canvas,"CasinoPurchaseStatus","",new(8,72,274,37),10);_status.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        bool committed=false;
        NativeAction(canvas,"ConfirmCasinoCoins","CONFIRM",new(48,117,108,25),()=>
        {
            if(committed || !ReferenceEquals(_session.Current,state))return;
            var result=CasinoPokerService.BuyCoins(state);
            if(result==CasinoResult.Success){committed=true;ShowCasino(state);}else _status.Text=Tr($"CASINO_RESULT_{result}");
        });
        NativeAction(canvas,"CancelCasinoCoins","CANCEL",new(164,117,118,25),()=>ShowCasino(state));
    }
    private void ShowPoker(GameState state)
    {
        var canvas=CasinoCanvas("CasinoPokerWindow","CASINO_POKER",422,318);PokerPayTable(canvas);
        _dialog!.SetTitle(Tr("CASINO_POKER"));
        _dialog.CloseRequested=()=>{CasinoPokerService.Close(state);ShowOfficeReconstruction(state);};
        var totals=NativeText(canvas,"PokerTotals","",new(4,251,414,20),10);
        var message=NativeText(canvas,"PokerPhase","",new(4,274,188,40),10);message.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var cards=new List<NativeCasinoCard>();
        for (int index=0;index<5;index++)
        {
            int chosen=index;
            var card=PlaceCasinoCard(canvas,$"PokerCard{index}",12+80*index,132,()=>{if(CasinoPokerService.SelectCard(state,chosen))_refreshPoker?.Invoke();});cards.Add(card);
        }
        var bet=NativeAction(canvas,"PokerBetCollect","POKER_ADD_BET",new(195,286,70,25),()=>{_session.PokerCommand(0);_refreshPoker?.Invoke();});
        var deal=NativeAction(canvas,"PokerDealDouble","POKER_DEAL",new(271,286,65,25),()=>{_session.PokerCommand(1);_refreshPoker?.Invoke();});
        NativeAction(canvas,"ClosePoker","BACK_TO_OFFICE",new(342,286,76,25),()=>{CasinoPokerService.Close(state);ShowOfficeReconstruction(state);}).TooltipText=Tr("CASINO_CLOSE_NOTE");
        _refreshPoker=()=>
        {
            if (!_pokerInterfaceVisible) return;
            var c=state.Casino;var p=c.Poker;
            totals.Text=string.Format(DisplayNumbers.Provider,Tr("POKER_TOTALS"),c.Coins,c.Bet,c.PendingPayout);
            string key=p.Phase switch {0=>c.Bet==50 ? "POKER_RETAINED_BET" : "POKER_BET",1=>p.Tick<45 ? "POKER_REVEAL" : "POKER_HOLD",2=>"POKER_REVEAL",
                3=>p.AwardReady ? "POKER_WIN" : "POKER_RESULT_WAIT",4=>"POKER_COLLECTING",5=>p.Tick<18 ? "POKER_REVEAL" : "POKER_CHOOSE_DOUBLE",_=>p.TieRedeal ? "POKER_TIE" : "POKER_REVEAL"};
            message.Text=Tr(key);
            if (p.Phase==3 && p.AwardReady && (c.PendingPayout>long.MaxValue/2 || c.Coins>long.MaxValue-c.PendingPayout)) message.Text+="\n"+Tr("CASINO_RESULT_NumericLimit");
            if (p.Phase==3) message.Text+="\n"+Tr($"POKER_OUTCOME_{p.Outcome}");
            bet.Text=Tr(p.Phase is 3 or 4 ? "POKER_COLLECT" : "POKER_ADD_BET");
            deal.Text=Tr(p.Phase==1 ? "POKER_DRAW" : p.Phase is >=3 and <=6 ? "POKER_DOUBLE" : "POKER_DEAL");
            bet.Disabled=!CasinoPokerService.CanCommand(state,0);deal.Disabled=!CasinoPokerService.CanCommand(state,1);
            for (int i=0;i<5;i++)
            {
                bool known=p.Cards.Count==5 && p.Revealed[i];
                var card=known ? p.Cards[i] : default;
                string face=known ? CardFace(card) : "?";
                cards[i].Present(card,known,p.Cards.Count==5,p.Phase==1&&p.Holds[i]==1);
                cards[i].Disabled=!(p.Phase==1 || p.Phase==5 && p.Tick>=18 && i>=1);
                cards[i].AddThemeColorOverride("font_color",known && card.Suit is 0 or 2 ? Colors.Red : Colors.Black);
                cards[i].SetMeta("revealed",known);cards[i].SetMeta("held",p.Holds[i]==1);
            }
        };
        _pokerElapsed=0;_pokerInterfaceVisible=true;_refreshPoker();RefreshDesktop();
    }
    private static string CardFace(CasinoCard card)
    {
        string[] ranks=["A","2","3","4","5","6","7","8","9","10","J","Q","K"];
        string[] suits=["♥","♠","♦","♣"];
        return ranks[card.Rank]+"\n"+suits[card.Suit];
    }
}
