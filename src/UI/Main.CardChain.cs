using Godot;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    public override void _Input(InputEvent input)
    {
        if (_session.Current is not { } state || !state.Casino.CardChainOpen || !_pokerInterfaceVisible || input is not InputEventKey {Pressed:true,Echo:false} key) return;
        long character=key.Unicode;
        if (character is >=49 and <=53) CasinoCardChainService.SelectCard(state,(int)character-49,direct:true);
        else if (character==113) _session.CardChainCommand(0,direct:true);
        else if (character==119) _session.CardChainCommand(1,direct:true);
        else return;
        _refreshPoker?.Invoke();GetViewport().SetInputAsHandled();
    }
    private void ShowCardChain(GameState state)
    {
        ClearContent();CompactManagementWindow("CasinoCardChainWindow",new Vector2(490,390),470);
        _dialog!.SetTitle(Tr("CASINO_CARD_CHAIN"));
        _dialog.CloseRequested=()=>{CasinoCardChainService.Close(state);ShowOfficeReconstruction(state);};
        var totals=new Label {Name="CardChainTotals",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(totals);
        var message=new Label {Name="CardChainPhase",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(message);
        var row=new HBoxContainer();_content.AddChild(row);var cards=new List<Button>();
        for(int i=0;i<5;i++)
        {
            int choice=i;var card=new Button {Name=$"CardChainCard{i}",CustomMinimumSize=new Vector2(78,95),SizeFlagsHorizontal=SizeFlags.ExpandFill};
            row.AddChild(card);cards.Add(card);card.Pressed+=()=>{CasinoCardChainService.SelectCard(state,choice);_refreshPoker?.Invoke();};
        }
        var actions=new HBoxContainer();_content.AddChild(actions);
        var bet=new Button {Name="CardChainBetCollect",SizeFlagsHorizontal=SizeFlags.ExpandFill};actions.AddChild(bet);
        var deal=new Button {Name="CardChainDealDouble",SizeFlagsHorizontal=SizeFlags.ExpandFill};actions.AddChild(deal);
        bet.Pressed+=()=>{_session.CardChainCommand(0);_refreshPoker?.Invoke();};
        deal.Pressed+=()=>{_session.CardChainCommand(1);_refreshPoker?.Invoke();};
        AddHeading(Tr("CARD_CHAIN_RULES"),12);AddHeading(Tr("CARD_CHAIN_KEYS"),12);AddHeading(Tr("CASINO_CLOSE_NOTE"),12);
        var close=new Button {Name="CloseCardChain",Text=Tr("BACK_TO_OFFICE")};_content.AddChild(close);
        close.Pressed+=()=>{CasinoCardChainService.Close(state);ShowOfficeReconstruction(state);};
        _refreshPoker=()=>
        {
            if (!_pokerInterfaceVisible) return;
            var c=state.Casino;var r=c.CardChain;var p=r.Round;
            totals.Text=string.Format(Tr("CARD_CHAIN_TOTALS"),c.Coins,c.Bet,c.PendingPayout,r.ChainCount,c.ChainBonus);
            message.Text=Tr(r.DeckExhausted ? "CARD_CHAIN_EXHAUSTED" : r.NumericBoundary ? "CASINO_RESULT_NumericLimit" : p.Phase==6 && p.ChosenCard==0 ? "CARD_CHAIN_DEALER_KEY" : p.Phase switch
            {0=>c.Bet==50 ? "POKER_RETAINED_BET" : "POKER_BET",1=>"CARD_CHAIN_SELECT",2=>"POKER_REVEAL",3=>"POKER_WIN",4=>c.PendingPayout==0 ? "CARD_CHAIN_ZERO_WAIT" : "POKER_COLLECTING",5=>p.Tick>=18 ? "POKER_CHOOSE_DOUBLE" : "POKER_REVEAL",_=>p.TieRedeal ? "POKER_TIE" : "POKER_REVEAL"});
            if (p.Phase==3 && (c.PendingPayout>long.MaxValue/2 || c.Coins>long.MaxValue-c.PendingPayout)) message.Text+="\n"+Tr("CASINO_RESULT_NumericLimit");
            bet.Text=Tr(p.Phase is 3 or 4 ? "POKER_COLLECT" : "POKER_ADD_BET");deal.Text=Tr(p.Phase is >=3 and <=6 ? "POKER_DOUBLE" : "POKER_DEAL");
            bet.Disabled=!CasinoCardChainService.CanCommand(state,0);deal.Disabled=!CasinoCardChainService.CanCommand(state,1);
            for(int i=0;i<5;i++)
            {
                bool revealed=p.Cards.Count==5 && (p.Phase<5 || p.Revealed[i]);
                cards[i].Text=revealed ? CardFace(p.Cards[i]) : "?";
                cards[i].Disabled=!CasinoCardChainService.CanSelect(state,i);
                cards[i].SetMeta("revealed",revealed);
            }
        };
        _pokerElapsed=0;_pokerInterfaceVisible=true;_refreshPoker();RefreshDesktop();
    }
}
