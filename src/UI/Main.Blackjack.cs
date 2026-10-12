using Godot;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private void ShowBlackjack(GameState state)
    {
        ClearContent();CompactManagementWindow("CasinoBlackjackWindow",new Vector2(610,420),590);
        _dialog!.SetTitle(Tr("CASINO_BLACKJACK"));
        _dialog.CloseRequested=()=>{CasinoBlackjackService.Close(state);ShowOfficeReconstruction(state);};
        var totals=new Label {Name="BlackjackTotals"};_content.AddChild(totals);
        var message=new Label {Name="BlackjackPhase",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(message);
        var dealer=new Label {Name="BlackjackDealer",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(dealer);
        var player=new Label {Name="BlackjackPlayer",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(player);
        var row=new HBoxContainer();_content.AddChild(row);var cards=new List<Button>();
        for (int i=0;i<5;i++)
        {
            int choice=i;var card=new Button {Name=$"BlackjackCard{i}",CustomMinimumSize=new Vector2(80,90)};row.AddChild(card);cards.Add(card);
            card.Pressed+=()=>{CasinoBlackjackService.SelectCard(state,choice);_refreshPoker?.Invoke();};
        }
        var actions=new HBoxContainer();_content.AddChild(actions);
        var more=new Button {Name="BlackjackDealMoreDouble",SizeFlagsHorizontal=SizeFlags.ExpandFill};actions.AddChild(more);
        var stay=new Button {Name="BlackjackBetStayCollect",SizeFlagsHorizontal=SizeFlags.ExpandFill};actions.AddChild(stay);
        more.Pressed+=()=>{_session.BlackjackCommand(1);_refreshPoker?.Invoke();};
        stay.Pressed+=()=>{_session.BlackjackCommand(2);_refreshPoker?.Invoke();};
        AddHeading(Tr("BLACKJACK_RULES"),12);AddHeading(Tr("CASINO_CLOSE_NOTE"),12);
        var close=new Button {Name="CloseBlackjack",Text=Tr("BACK_TO_OFFICE")};_content.AddChild(close);
        close.Pressed+=()=>{CasinoBlackjackService.Close(state);ShowOfficeReconstruction(state);};
        _refreshPoker=()=>
        {
            if (!_pokerInterfaceVisible) return;
            var c=state.Casino;var b=c.Blackjack;
            totals.Text=string.Format(Tr("POKER_TOTALS"),c.Coins,c.Bet,c.PendingPayout);
            bool doubling=b.Phase is 5 or 6 or 7;
            message.Text=Tr(b.BoundaryReached ? "BLACKJACK_CARD_BOUNDARY" : doubling ? b.TieTick>0 ? "POKER_TIE" : b.Phase==5 ? "BLACKJACK_FIRST" : b.Phase==6 ? "BLACKJACK_SECOND" : "POKER_REVEAL" : b.Phase==4 ? "POKER_COLLECTING" : b.Win ? b.AwardReady ? "POKER_WIN" : "POKER_RESULT_WAIT" : b.Phase==0 ? "POKER_BET" : b.Phase==2 ? "BLACKJACK_DECISION" : "POKER_REVEAL");
            if (b.Win && b.AwardReady && (c.PendingPayout>long.MaxValue/2 || c.Coins>long.MaxValue-c.PendingPayout)) message.Text+="\n"+Tr("CASINO_RESULT_NumericLimit");
            dealer.Text=string.Format(Tr("BLACKJACK_DEALER"),string.Join("  ",b.Cards.Skip(9).Take(b.DealerCount).Select(c=>CardFace(c).Replace('\n',' '))),b.DealerCount==0 ? "?" : CasinoBlackjackService.Score(b.Cards.Skip(9).Take(b.DealerCount).ToArray()).ToString());
            player.Text=string.Format(Tr("BLACKJACK_PLAYER"),string.Join("  ",b.Cards.Take(b.PlayerCount).Select(c=>CardFace(c).Replace('\n',' '))),b.PlayerCount==0 ? "?" : b.PlayerTotal.ToString());
            more.Text=Tr(b.Win ? "POKER_DOUBLE" : b.Phase==2 ? "BLACKJACK_MORE" : "POKER_DEAL");
            stay.Text=Tr(b.Win ? "POKER_COLLECT" : b.Phase==2 ? "BLACKJACK_STAY" : "POKER_ADD_BET");
            more.Disabled=!CasinoBlackjackService.CanCommand(state,1);stay.Disabled=!CasinoBlackjackService.CanCommand(state,2);
            for (int i=0;i<5;i++)
            {
                cards[i].Visible=doubling && !(b.Phase==6 && b.FirstChoice==i);
                bool revealed=doubling && b.Marks[i]>=3;
                cards[i].Text=revealed ? CardFace(b.DoubleCards[i]) : "?";
                cards[i].Disabled=b.Phase is not (5 or 6);
                cards[i].SetMeta("revealed",revealed);
            }
        };
        _pokerElapsed=0;_pokerInterfaceVisible=true;_refreshPoker();RefreshDesktop();
    }
}
