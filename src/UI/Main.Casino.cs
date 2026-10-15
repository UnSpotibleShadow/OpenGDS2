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
        ClearContent();CompactManagementWindow("CasinoSelector",new Vector2(370,325),350);
        AddHeading(Tr("CASINO_ENTER"),24);AddHeading(string.Format(Tr("CASINO_COINS"),state.Casino.Coins),18);
        AddHeading(Tr("CASINO_AVAILABLE_GAMES"),14);
        var poker=new Button {Name="CasinoPoker",Text=Tr("CASINO_POKER"),Disabled=state.Casino.Coins<=0 || FacilityService.HasPendingDecision(state)};
        _content.AddChild(poker);poker.Pressed+=()=>
        {
            var result=CasinoPokerService.Open(state);
            if (result==CasinoResult.Success) ShowPoker(state);else _status.Text=Tr($"CASINO_RESULT_{result}");
        };
        var blackjack=new Button {Name="CasinoBlackjack",Text=Tr("CASINO_BLACKJACK"),Disabled=state.Casino.Coins<=0 || FacilityService.HasPendingDecision(state)};
        _content.AddChild(blackjack);blackjack.Pressed+=()=>
        {
            var result=CasinoBlackjackService.Open(state);
            if (result==CasinoResult.Success) ShowBlackjack(state);else _status.Text=Tr($"CASINO_RESULT_{result}");
        };
        var chain=new Button {Name="CasinoCardChain",Text=Tr("CASINO_CARD_CHAIN"),Disabled=state.Casino.Coins<=0 || FacilityService.HasPendingDecision(state)};
        _content.AddChild(chain);chain.Pressed+=()=>
        {
            var result=CasinoCardChainService.Open(state);
            if (result==CasinoResult.Success) ShowCardChain(state);else _status.Text=Tr($"CASINO_RESULT_{result}");
        };
        var horse=new Button {Name="CasinoHorse",Text=Tr("CASINO_HORSE"),Disabled=FacilityService.HasPendingDecision(state) || state.Money is null || !CasinoHorseService.HasCatalog(_activeCatalog)};
        _content.AddChild(horse);horse.Pressed+=()=>
        {
            var result=_session.OpenHorse(_activeCatalog);
            if (result==CasinoResult.Success) ShowHorseRacing(state);else _status.Text=Tr($"CASINO_RESULT_{result}");
        };
        if (!CasinoHorseService.HasCatalog(_activeCatalog)) AddHeading(Tr("CASINO_RESULT_CatalogMissing"),12);
        var purchase=new Button {Name="CasinoBuyCoins",Text=Tr("CASINO_BUY_COINS"),Disabled=FacilityService.HasPendingDecision(state) || state.Money is null};
        _content.AddChild(purchase);purchase.Pressed+=()=>ShowCoinPurchase(state);
        _status=new Label {Name="CasinoStatus",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(_status);
        if (state.Casino.Coins<=0) _status.Text=Tr("CASINO_RESULT_NoCoins");
        AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));
    }
    private void ShowCoinPurchase(GameState state)
    {
        ClearContent();CompactManagementWindow("CasinoCoinPurchase",new Vector2(370,225),350);
        AddHeading(Tr("CASINO_BUY_COINS"),20);AddHeading(Tr("CASINO_PURCHASE_CONFIRM"),16);
        _status=new Label {Name="CasinoPurchaseStatus",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(_status);
        bool committed=false;
        var confirm=new Button {Name="ConfirmCasinoCoins",Text=Tr("CONFIRM")};_content.AddChild(confirm);
        confirm.Pressed+=()=>
        {
            if (committed || !ReferenceEquals(_session.Current,state)) return;
            var result=CasinoPokerService.BuyCoins(state);
            if (result==CasinoResult.Success) {committed=true;ShowCasino(state);}
            else _status.Text=Tr($"CASINO_RESULT_{result}");
        };
        AddAction("CANCEL",()=>ShowCasino(state));
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
            totals.Text=string.Format(Tr("POKER_TOTALS"),c.Coins,c.Bet,c.PendingPayout);
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
