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
        if (!_pokerInterfaceVisible || _session.Current is not { } state || !state.Casino.CardGameOpen) return;
        _pokerElapsed+=delta;
        if (_pokerElapsed>=(state.Casino.BlackjackOpen ? CasinoBlackjackService.TickSeconds : CasinoPokerService.TickSeconds))
        {
            // One callback, no elapsed-time catch-up burst. Native callbacks rearm after return.
            _pokerElapsed=0;if (state.Casino.BlackjackOpen) _session.TickBlackjack();else _session.TickPoker();_refreshPoker?.Invoke();
        }
    }
    private void ShowCasino(GameState state)
    {
        if (state.Casino.PokerOpen) {ShowPoker(state);return;}
        if (state.Casino.BlackjackOpen) {ShowBlackjack(state);return;}
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
        ClearContent();CompactManagementWindow("CasinoPokerWindow",new Vector2(430,365),410);
        _dialog!.SetTitle(Tr("CASINO_POKER"));
        _dialog.CloseRequested=()=>{CasinoPokerService.Close(state);ShowOfficeReconstruction(state);};
        var totals=new Label {Name="PokerTotals",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(totals);
        var message=new Label {Name="PokerPhase",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(message);
        var row=new HBoxContainer {Name="PokerHand"};_content.AddChild(row);
        var cards=new List<Button>();
        for (int index=0;index<5;index++)
        {
            int chosen=index;
            var card=new Button {Name=$"PokerCard{index}",CustomMinimumSize=new Vector2(72,95),SizeFlagsHorizontal=SizeFlags.ExpandFill};
            row.AddChild(card);cards.Add(card);
            card.Pressed+=()=>{if (CasinoPokerService.SelectCard(state,chosen)) _refreshPoker?.Invoke();};
        }
        var actions=new HBoxContainer();_content.AddChild(actions);
        var bet=new Button {Name="PokerBetCollect",SizeFlagsHorizontal=SizeFlags.ExpandFill};actions.AddChild(bet);
        var deal=new Button {Name="PokerDealDouble",SizeFlagsHorizontal=SizeFlags.ExpandFill};actions.AddChild(deal);
        bet.Pressed+=()=>{_session.PokerCommand(0);_refreshPoker?.Invoke();};
        deal.Pressed+=()=>{_session.PokerCommand(1);_refreshPoker?.Invoke();};
        AddHeading(Tr("POKER_PAY_TABLE"),12);
        AddHeading(Tr("CASINO_CLOSE_NOTE"),12);
        var close=new Button {Name="ClosePoker",Text=Tr("BACK_TO_OFFICE")};_content.AddChild(close);
        close.Pressed+=()=>{CasinoPokerService.Close(state);ShowOfficeReconstruction(state);};
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
                cards[i].Text=face+(p.Phase==1 && p.Holds[i]==1 ? "\n"+Tr("POKER_HELD") : "");
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
