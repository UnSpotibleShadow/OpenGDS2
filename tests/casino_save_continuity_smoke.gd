extends "res://tests/casino_poker_smoke.gd"

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,720)
    write_json("user://original/catalog.json",{"SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6"})
    var games=[
        ["Poker","PokerBetCollect","PokerDealDouble","CasinoPokerWindow"],
        ["Blackjack","BlackjackBetStayCollect","BlackjackDealMoreDouble","CasinoBlackjackWindow"],
        ["CardChain","CardChainBetCollect","CardChainDealDouble","CasinoCardChainWindow"]]
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);language.save("user://interface.cfg")
        for game in games:
            var value=casino_fixture();value.State.Casino.Coins=10;value.State.Casino[game[0]+"Open"]=true
            value.State.OriginalPayrollScratchIndex=17
            await load_fixture(value)
            # Freeze callbacks, not the controls, to inspect the exact load boundary.
            main.set_process(false)
            await press_node(game[1]);await press_node(game[2])
            if game[0]=="CardChain":await press_node("CardChainCard0")
            await press_node("ToolbarSAVE_GAME")
            var before=FileAccess.get_file_as_string("user://saves/slot1.json")
            await press_node("ToolbarLOAD_GAME");await press_node("ToolbarSAVE_GAME")
            check(FileAccess.get_file_as_string("user://saves/slot1.json")==before,"Exact saved casino state and RNG restored: "+locale+"/"+game[0])
            check(main.find_child(game[3],true,false)!=null,"Active casino window restored: "+game[0])
            main.set_process(true);await create_timer(0.22).timeout;main.set_process(false)
            await press_node("ToolbarSAVE_GAME")
            var state=saved()
            var round=state.Casino[game[0]]
            if game[0]=="CardChain":
                check(round.ChainCount==1 and round.Round.UsedCards.size()==6,"Restored replacement executes once")
            else:check(round.Tick>0,"Restored reveal timer advances: "+game[0])
            # The replacement can legitimately lose and clear the stake. Coins must
            # still reflect exactly the original one-coin debit in either outcome.
            var expected_bet=0 if game[0]=="CardChain" and round.Round.Phase==0 else 1
            check(state.Casino.Coins==9 and state.Casino.Bet==expected_bet,"Reload preserves the correct stake without another debit: "+game[0])
            check(state.OriginalPayrollScratchIndex==17 and state.CompanyActivityTicks==value.State.CompanyActivityTicks,"Casino continuation preserves office scratch and clock")
            await dispose()
    print("Casino save continuity smoke failures: ",failures);quit(0 if failures==0 else 1)
