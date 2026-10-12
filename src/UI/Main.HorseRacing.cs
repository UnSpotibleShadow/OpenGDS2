using Godot;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private double _horseElapsed;
    private void TickHorseInterface(GameState state,double delta)
    {
        var r=state.Casino.Horse;_horseElapsed+=delta;
        double interval=r.Stage switch {0=>CasinoHorseService.BetSeconds,1=>CasinoHorseService.RaceSeconds,_=>CasinoHorseService.ResultSeconds};
        if(_horseElapsed<interval) return;
        _horseElapsed=0;
        if(r.Stage==0) CasinoHorseService.TickBet(state);
        else if(r.Stage==1) _session.TickHorseRace();
        else if(r.Stage==2) _session.TickHorseResult(_activeCatalog);
        _refreshPoker?.Invoke();
    }
    private void ShowHorseRacing(GameState state)
    {
        ClearContent();CompactManagementWindow("CasinoHorseWindow",new Vector2(665,560),645);
        _dialog!.SetTitle(Tr("CASINO_HORSE"));
        _dialog.CloseRequested=()=>{CasinoHorseService.Close(state);ShowOfficeReconstruction(state);};
        var totals=new Label {Name="HorseTotals",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(totals);
        var message=new Label {Name="HorsePhase",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(message);
        var runners=new List<Label>();
        for(int i=0;i<8;i++) {var line=new Label {Name=$"HorseRunner{i}",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(line);runners.Add(line);}
        var selectors=new HBoxContainer();_content.AddChild(selectors);
        var kind=new OptionButton {Name="HorseWagerKind"};kind.AddItem(Tr("HORSE_SINGLE"),0);kind.AddItem(Tr("HORSE_PAIR"),1);selectors.AddChild(kind);
        var first=new OptionButton {Name="HorseFirst",SizeFlagsHorizontal=SizeFlags.ExpandFill};selectors.AddChild(first);
        var second=new OptionButton {Name="HorseSecond",SizeFlagsHorizontal=SizeFlags.ExpandFill};selectors.AddChild(second);
        kind.ItemSelected+=i=>{state.Casino.Horse.PairSelected=i==1;_refreshPoker?.Invoke();};
        first.ItemSelected+=i=>
        {
            var r=state.Casino.Horse;r.SelectedFirst=(int)i;
            if(r.SelectedSecond==r.SelectedFirst) r.SelectedSecond=r.SelectedFirst==0 ? 1 : 0;
            r.HeldAdjustment=0;_refreshPoker?.Invoke();
        };
        second.ItemSelected+=i=>{state.Casino.Horse.SelectedSecond=second.GetItemId((int)i);state.Casino.Horse.HeldAdjustment=0;_refreshPoker?.Invoke();};
        var wager=new Label {Name="HorseSelectedWager"};_content.AddChild(wager);
        var actions=new HBoxContainer();_content.AddChild(actions);
        var add=new Button {Name="HorseAddWager",Text=Tr("HORSE_ADD"),SizeFlagsHorizontal=SizeFlags.ExpandFill};actions.AddChild(add);
        var remove=new Button {Name="HorseRemoveWager",Text=Tr("HORSE_REMOVE"),SizeFlagsHorizontal=SizeFlags.ExpandFill};actions.AddChild(remove);
        add.ButtonDown+=()=>state.Casino.Horse.HeldAdjustment=1;remove.ButtonDown+=()=>state.Casino.Horse.HeldAdjustment=-1;
        add.ButtonUp+=()=>state.Casino.Horse.HeldAdjustment=0;remove.ButtonUp+=()=>state.Casino.Horse.HeldAdjustment=0;
        var start=new Button {Name="HorseStart",Text=Tr("HORSE_START"),SizeFlagsHorizontal=SizeFlags.ExpandFill};actions.AddChild(start);
        start.Pressed+=()=>{_session.StartHorse();_horseElapsed=0;_refreshPoker?.Invoke();};
        var accept=new Button {Name="HorseAcceptResult",Text=Tr("HORSE_ACCEPT_RESULT")};_content.AddChild(accept);
        accept.Pressed+=()=>
        {
            if(state.Casino.Horse.Stage==3) _session.AcceptHorsePayout(_activeCatalog);else _session.ResumeHorseCatalog(_activeCatalog);
            _horseElapsed=0;_refreshPoker?.Invoke();
        };
        AddHeading(Tr("HORSE_ORIGINAL_DEFECTS"),12);AddHeading(Tr("HORSE_CLOSE_NOTE"),12);
        var close=new Button {Name="CloseHorse",Text=Tr("BACK_TO_OFFICE")};_content.AddChild(close);
        close.Pressed+=()=>{CasinoHorseService.Close(state);ShowOfficeReconstruction(state);};
        List<RaceRunner>? roster=null;int lastFirst=-1;
        _refreshPoker=()=>
        {
            if(!_pokerInterfaceVisible) return;
            var r=state.Casino.Horse;
            totals.Text=string.Format(Tr("HORSE_TOTALS"),state.Money,r.DraftCash,r.RaceCounter+1);
            message.Text=Tr(r.NumericBoundary ? "CASINO_RESULT_NumericLimit" : r.CatalogBoundary ? "CASINO_RESULT_CatalogMissing" : r.Stage switch {0=>"HORSE_BETTING",1=>"HORSE_RUNNING",2=>"HORSE_RESULTS_WAIT",_=>"HORSE_PAYOUT"});
            if(r.Stage>=2) message.Text+="\n"+string.Format(Tr("HORSE_FINISH"),string.Join(" → ",r.Arrival.Select(i=>(i+1).ToString())));
            if(r.Stage==3) message.Text+="\n"+string.Format(Tr("HORSE_AWARD"),r.DisplayedAward);
            for(int i=0;i<8;i++)
            {
                var h=r.Runners[i];string forecasts=string.Join("/",r.ForecastRanks.Select(row=>(row[i]+1).ToString()));
                runners[i].Text=string.Format(Tr("HORSE_RUNNER"),i+1,h.Name,h.JockeyName,forecasts,r.WinOdds[i],r.SingleStakes[i]);
                if(r.Stage>0) runners[i].Text+="  "+(r.RacePhase<2 ? r.Positions[i] : r.FinishCoordinates[i]).ToString("0.0");
            }
            kind.Select(r.PairSelected ? 1 : 0);
            bool changed=!ReferenceEquals(roster,r.Runners);
            if(changed) {first.Clear();for(int i=0;i<8;i++) first.AddItem($"{i+1}: {r.Runners[i].Name}",i);}
            if(changed || lastFirst!=r.SelectedFirst)
            {second.Clear();for(int i=0;i<8;i++) if(i!=r.SelectedFirst) second.AddItem($"{i+1}: {r.Runners[i].Name}",i);}
            roster=r.Runners;lastFirst=r.SelectedFirst;
            first.Select(r.SelectedFirst);second.Select(CasinoHorseService.CompressedColumn(r.SelectedFirst,r.SelectedSecond));second.Visible=r.PairSelected;
            long stake=r.PairSelected ? r.PairStakes[r.SelectedFirst][r.SelectedSecond] : r.SingleStakes[r.SelectedFirst];
            double odds=r.PairSelected ? r.PairOdds[r.SelectedFirst][CasinoHorseService.CompressedColumn(r.SelectedFirst,r.SelectedSecond)] : r.WinOdds[r.SelectedFirst];
            wager.Text=string.Format(Tr("HORSE_SELECTED_WAGER"),stake,odds);
            bool draft=r.Stage==0 && !r.NumericBoundary && !r.CatalogBoundary;
            kind.Disabled=first.Disabled=second.Disabled=!draft;add.Disabled=!draft || r.DraftCash<=0;
            remove.Disabled=!draft || (r.PairSelected ? r.PairStakes[r.SelectedFirst][CasinoHorseService.CompressedColumn(r.SelectedFirst,r.SelectedSecond)] : r.SingleStakes[r.SelectedFirst])<=0;
            start.Disabled=!draft;accept.Visible=r.Stage==3 || r.CatalogBoundary;
            accept.Disabled=!CasinoHorseService.HasCatalog(_activeCatalog) || r.NumericBoundary;
        };
        _horseElapsed=0;_pokerInterfaceVisible=true;_refreshPoker();RefreshDesktop();
    }
}
