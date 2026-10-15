using Godot;
using OpenGDS2.Domain;
using OpenGDS2.Core;
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
        ClearContent();var canvas=NativeWindow("CasinoHorseWindow",Tr("CASINO_HORSE"),new(586,428));
        _dialog!.SetTitle(Tr("CASINO_HORSE"));
        _dialog.CloseRequested=()=>{CasinoHorseService.Close(state);ShowOfficeReconstruction(state);};
        var totals=NativeText(canvas,"HorseTotals","",new(4,4,578,22),10);
        var message=NativeText(canvas,"HorsePhase","",new(4,382,390,42),10);message.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var table=ManagementTable(canvas,"HorseRoster",["HORSE_NUMBER","EMPLOYEE_NAME","HORSE_JOCKEY","HORSE_FORECAST","HORSE_ODDS"],[34,208,114,112,77],new(4,31,578,177));
        var rosterRows=new TreeItem[8];for(int i=0;i<8;i++){rosterRows[i]=table.CreateItem(table.GetRoot());rosterRows[i].SetMetadata(0,i);}
        table.ItemSelected+=()=>{if(state.Casino.Horse.Stage==0&&table.GetSelected() is { } selected){state.Casino.Horse.SelectedFirst=selected.GetMetadata(0).AsInt32();if(state.Casino.Horse.SelectedSecond==state.Casino.Horse.SelectedFirst)state.Casino.Horse.SelectedSecond=(state.Casino.Horse.SelectedFirst+1)%8;state.Casino.Horse.HeldAdjustment=0;_refreshPoker?.Invoke();}};
        var track=NativePlace(canvas,new NativeHorseTrack {Name="HorseRaceTrack",Race=state.Casino.Horse,TextureFilter=TextureFilterEnum.Nearest,Visible=false,ClipContents=true},new(82,33,421,173));
        var selectors=NativePlace(canvas,new HBoxContainer(),new(4,213,578,26));
        var kind=new OptionButton {Name="HorseWagerKind"};kind.AddItem(Tr("HORSE_SINGLE"),0);kind.AddItem(Tr("HORSE_PAIR"),1);selectors.AddChild(kind);
        var first=new OptionButton {Name="HorseFirst",SizeFlagsHorizontal=SizeFlags.ExpandFill,ClipText=true};selectors.AddChild(first);
        var second=new OptionButton {Name="HorseSecond",SizeFlagsHorizontal=SizeFlags.ExpandFill,ClipText=true};selectors.AddChild(second);
        foreach(var field in new[]{kind,first,second})field.AddThemeFontSizeOverride("font_size",10);
        var matrix=NativePlace(canvas,new GridContainer {Name="HorseBettingMatrix",Columns=4},new(4,244,578,65));matrix.AddThemeConstantOverride("h_separation",3);matrix.AddThemeConstantOverride("v_separation",2);
        var matrixButtons=new Button[8];
        for(int i=0;i<8;i++){int pick=i;var button=new Button {Name="HorseMatrix"+i,Icon=OriginalAssetStore.LoadTexture($"Graphic/Embedded/HorseMarker{i}.bmp"),CustomMinimumSize=new(141,29),ClipText=true};button.AddThemeFontSizeOverride("font_size",10);matrix.AddChild(button);matrixButtons[i]=button;button.Pressed+=()=>{var race=state.Casino.Horse;if(race.Stage!=0)return;if(race.PairSelected){race.SelectedSecond=pick==race.SelectedFirst?(pick+1)%8:pick;}else{race.SelectedFirst=pick;if(race.SelectedSecond==pick)race.SelectedSecond=(pick+1)%8;}race.HeldAdjustment=0;_refreshPoker?.Invoke();};}
        kind.ItemSelected+=i=>{state.Casino.Horse.PairSelected=i==1;_refreshPoker?.Invoke();};
        first.ItemSelected+=i=>
        {
            var r=state.Casino.Horse;r.SelectedFirst=(int)i;
            if(r.SelectedSecond==r.SelectedFirst) r.SelectedSecond=r.SelectedFirst==0 ? 1 : 0;
            r.HeldAdjustment=0;_refreshPoker?.Invoke();
        };
        second.ItemSelected+=i=>{state.Casino.Horse.SelectedSecond=second.GetItemId((int)i);state.Casino.Horse.HeldAdjustment=0;_refreshPoker?.Invoke();};
        var wager=NativeText(canvas,"HorseSelectedWager","",new(4,315,578,22),10);
        var actions=NativePlace(canvas,new HBoxContainer(),new(4,341,578,28));
        var add=new Button {Name="HorseAddWager",Text=Tr("HORSE_ADD"),SizeFlagsHorizontal=SizeFlags.ExpandFill};actions.AddChild(add);
        var remove=new Button {Name="HorseRemoveWager",Text=Tr("HORSE_REMOVE"),SizeFlagsHorizontal=SizeFlags.ExpandFill};actions.AddChild(remove);
        add.ButtonDown+=()=>state.Casino.Horse.HeldAdjustment=1;remove.ButtonDown+=()=>state.Casino.Horse.HeldAdjustment=-1;
        add.ButtonUp+=()=>state.Casino.Horse.HeldAdjustment=0;remove.ButtonUp+=()=>state.Casino.Horse.HeldAdjustment=0;
        var start=new Button {Name="HorseStart",Text=Tr("HORSE_START"),SizeFlagsHorizontal=SizeFlags.ExpandFill};actions.AddChild(start);
        start.Pressed+=()=>{_session.StartHorse();_horseElapsed=0;_refreshPoker?.Invoke();};
        var accept=NativeAction(canvas,"HorseAcceptResult","HORSE_ACCEPT_RESULT",new(402,378,180,25),()=>{});
        accept.Pressed+=()=>
        {
            if(state.Casino.Horse.Stage==3) _session.AcceptHorsePayout(_activeCatalog);else _session.ResumeHorseCatalog(_activeCatalog);
            _horseElapsed=0;_refreshPoker?.Invoke();
        };
        var close=NativeAction(canvas,"CloseHorse","BACK_TO_OFFICE",new(402,402,180,23),()=>{});close.TooltipText=Tr("HORSE_ORIGINAL_DEFECTS")+"\n"+Tr("HORSE_CLOSE_NOTE");
        close.Pressed+=()=>{CasinoHorseService.Close(state);ShowOfficeReconstruction(state);};
        List<RaceRunner>? roster=null;int lastFirst=-1;
        _refreshPoker=()=>
        {
            if(!_pokerInterfaceVisible) return;
            var r=state.Casino.Horse;
            totals.Text=string.Format(DisplayNumbers.Provider,Tr("HORSE_TOTALS"),state.Money,r.DraftCash,r.RaceCounter+1);
            message.Text=Tr(r.NumericBoundary ? "CASINO_RESULT_NumericLimit" : r.CatalogBoundary ? "CASINO_RESULT_CatalogMissing" : r.Stage switch {0=>"HORSE_BETTING",1=>"HORSE_RUNNING",2=>"HORSE_RESULTS_WAIT",_=>"HORSE_PAYOUT"});
            if(r.Stage>=2) message.Text+="\n"+string.Format(DisplayNumbers.Provider,Tr("HORSE_FINISH"),string.Join(" → ",r.Arrival.Select(i=>(i+1).ToString())));
            if(r.Stage==3) message.Text+="\n"+string.Format(DisplayNumbers.Provider,Tr("HORSE_AWARD"),r.DisplayedAward);
            for(int i=0;i<8;i++)
            {
                var h=r.Runners[i];string forecasts=string.Join("/",r.ForecastRanks.Select(row=>(row[i]+1).ToString()));
                var entry=rosterRows[i];entry.SetText(0,(i+1).ToString());entry.SetText(1,h.Name);entry.SetText(2,h.JockeyName);entry.SetText(3,forecasts);entry.SetText(4,r.WinOdds[i].ToString("0.0"));entry.SetTextAlignment(4,HorizontalAlignment.Right);
                if(r.PairSelected&&i==r.SelectedFirst){matrixButtons[i].Text="—";matrixButtons[i].Disabled=true;}
                else{double cellOdds=r.PairSelected?r.PairOdds[r.SelectedFirst][CasinoHorseService.CompressedColumn(r.SelectedFirst,i)]:r.WinOdds[i];long cellStake=r.PairSelected?r.PairStakes[r.SelectedFirst][i]:r.SingleStakes[i];matrixButtons[i].Text=$"{(r.PairSelected?(r.SelectedFirst+1)+"–":"")}{i+1}: {cellOdds:0.0} · ¥{DisplayNumbers.Format(cellStake)}";matrixButtons[i].Disabled=r.Stage!=0;}
            }
            table.Visible=r.Stage!=1;track.Visible=r.Stage==1;track.Race=r;track.QueueRedraw();
            kind.Select(r.PairSelected ? 1 : 0);
            bool changed=!ReferenceEquals(roster,r.Runners);
            if(changed) {first.Clear();for(int i=0;i<8;i++) first.AddItem($"{i+1}: {r.Runners[i].Name}",i);}
            if(changed || lastFirst!=r.SelectedFirst)
            {second.Clear();for(int i=0;i<8;i++) if(i!=r.SelectedFirst) second.AddItem($"{i+1}: {r.Runners[i].Name}",i);}
            roster=r.Runners;lastFirst=r.SelectedFirst;
            first.Select(r.SelectedFirst);second.Select(CasinoHorseService.CompressedColumn(r.SelectedFirst,r.SelectedSecond));second.Visible=r.PairSelected;
            long stake=r.PairSelected ? r.PairStakes[r.SelectedFirst][r.SelectedSecond] : r.SingleStakes[r.SelectedFirst];
            double odds=r.PairSelected ? r.PairOdds[r.SelectedFirst][CasinoHorseService.CompressedColumn(r.SelectedFirst,r.SelectedSecond)] : r.WinOdds[r.SelectedFirst];
            wager.Text=string.Format(DisplayNumbers.Provider,Tr("HORSE_SELECTED_WAGER"),stake,odds);
            bool draft=r.Stage==0 && !r.NumericBoundary && !r.CatalogBoundary;
            kind.Disabled=first.Disabled=second.Disabled=!draft;add.Disabled=!draft || r.DraftCash<=0;
            remove.Disabled=!draft || (r.PairSelected ? r.PairStakes[r.SelectedFirst][CasinoHorseService.CompressedColumn(r.SelectedFirst,r.SelectedSecond)] : r.SingleStakes[r.SelectedFirst])<=0;
            start.Disabled=!draft;accept.Visible=r.Stage==3 || r.CatalogBoundary;
            accept.Disabled=!CasinoHorseService.HasCatalog(_activeCatalog) || r.NumericBoundary;
        };
        _horseElapsed=0;_pokerInterfaceVisible=true;_refreshPoker();RefreshDesktop();
    }
}
