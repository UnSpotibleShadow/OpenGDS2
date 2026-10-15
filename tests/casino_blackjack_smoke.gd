extends "res://tests/casino_poker_smoke.gd"

func blackjack_round() -> Dictionary:
    return {"Phase":0,"Tick":0,"Cards":[],"UsedCards":[],"PlayerCount":0,"DealerCount":0,
        "PlayerTotal":0,"DealerTotal":0,"DealEnabled":false,"Natural":false,"Win":false,
        "AwardIndex":0,"AwardTick":0,"AwardReady":false,"CollectTick":0,"DoubleCards":[],
        "Marks":[0,0,0,0,0],"FirstChoice":null,"SecondChoice":null,"TieTick":0,"ReopenedWin":false,"BoundaryReached":false}

func award_fixture() -> Dictionary:
    var value := casino_fixture()
    value.State.Casino.Coins=10
    value.State.Casino.Bet=1
    value.State.Casino.PendingPayout=4
    value.State.Casino.BlackjackOpen=true
    value.State.Casino.Blackjack=blackjack_round()
    var b: Dictionary=value.State.Casino.Blackjack
    b.Phase=2
    b.PlayerCount=5
    b.DealerCount=1
    b.PlayerTotal=17
    b.Win=true
    b.AwardReady=true
    b.AwardTick=30
    b.AwardIndex=2
    for i in range(18):
        b.Cards.append({"Suit":int(i/13),"Rank":i%13})
        b.UsedCards.append(i)
    value.State.OriginalPayrollScratchIndex=17
    return value

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("casino_blackjack_smoke requires disposable XDG_DATA_HOME")
        quit(1)
        return
    root.size=Vector2i(1280,720)
    write_json("user://original/catalog.json",{"SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6"})
    for locale in ["en","ja"]:
        var language := ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Blackjack locale " + locale)
        var value := casino_fixture()
        value.State.Casino.Coins=10
        value.State.Casino.Bet=7
        await load_fixture(value)
        await press_node("TogglePause")
        await press_node("CompanyFacilities")
        await press_node("EnterCasino0")
        check(main.find_child("CasinoBlackjack",true,false).text==tr("CASINO_BLACKJACK"),"Localized blackjack selector")
        await press_node("CasinoBlackjack")
        check(main.find_child("BlackjackDealMoreDouble",true,false).disabled,"Deal initially disabled")
        await press_node("BlackjackBetStayCollect")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Casino.Bet==1 and saved().Casino.Coins==9,"Opening resets retained bet; new bet debits once")
        await press_node("BlackjackDealMoreDouble")
        await press_node("ToolbarSAVE_GAME")
        var hand: Array=saved().Casino.Blackjack.Cards.duplicate(true)
        check(hand.size()==18 and saved().Casino.Blackjack.UsedCards.size()==18,"Eighteen predealt unique cards")
        var ticks=saved().CompanyActivityTicks
        await create_timer(0.2).timeout
        await press_node("ToolbarSAVE_GAME")
        check(saved().CompanyActivityTicks==ticks,"Blackjack pauses office")
        check(main.find_child("ToolbarSETTINGS",true,false)==null,"No port settings shortcut in blackjack")
        await press_node("ToolbarLOAD_GAME")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Casino.Blackjack.Cards==hand,"Saved hand resumes without redraw")
        for attempt in range(130):
            await press_node("ToolbarSAVE_GAME")
            if saved().Casino.Blackjack.Phase==2 or saved().Casino.Blackjack.Win:
                break
            await create_timer(0.05).timeout
        check(saved().Casino.Blackjack.PlayerCount==2 and saved().Casino.Blackjack.DealerCount==1,"Initial independent timer reveals two and one")
        await press_node("CloseBlackjack")
        await press_node("ToolbarSAVE_GAME")
        check(not saved().Casino.BlackjackOpen and saved().Casino.Coins==9 and saved().CompanyActivityPerformed,"Close records activity without refund")
        await dispose()
        await load_fixture(award_fixture())
        check(main.find_child("BlackjackBetStayCollect",true,false).text==tr("POKER_COLLECT"),"Localized award controls")
        await press_node("ToolbarSAVE_GAME")
        check(saved().OriginalPayrollScratchIndex==17,"Saved active blackjack preserves payroll scratch")
        await press_node("ToolbarLOAD_GAME")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Casino.PendingPayout==4 and saved().OriginalPayrollScratchIndex==17,"Reload preserves award and scratch")
        await press_node("BlackjackBetStayCollect")
        for attempt in range(20):
            await create_timer(0.05).timeout
            await press_node("ToolbarSAVE_GAME")
            if saved().Casino.PendingPayout==0:
                break
        check(saved().Casino.Coins==14 and saved().Casino.Bet==0,"Award collection credits exact total once")
        await dispose()
        await load_fixture(award_fixture())
        await press_node("BlackjackDealMoreDouble")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Casino.Blackjack.DoubleCards.size()==5,"Double-up generates five fresh cards")
        await press_node("BlackjackCard0")
        check(not main.find_child("BlackjackCard0",true,false).visible,"First choice hidden from normal mouse input")
        await press_node("ToolbarSAVE_GAME")
        await press_node("ToolbarLOAD_GAME")
        check(not main.find_child("BlackjackCard0",true,false).visible,"Hidden choice preserved on reload")
        await press_node("BlackjackCard1")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Casino.Blackjack.Phase==7 and saved().Casino.Blackjack.FirstChoice==0 and saved().Casino.Blackjack.SecondChoice==1,"Two choices enter comparison")
        await press_node("CloseBlackjack")
        await dispose()
    await process_frame
    await create_timer(1.0).timeout
    print("Blackjack smoke failures: ",failures)
    quit(0 if failures==0 else 1)
