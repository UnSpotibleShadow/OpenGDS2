extends "res://tests/casino_poker_smoke.gd"

func chain_round() -> Dictionary:
    var value := casino_fixture()
    value.State.Casino.Coins=10
    value.State.Casino.CardChainOpen=true
    value.State.Casino.CardChain={"ChainCount":0,"PreviousCard":null,"SelectedCard":0,"DeckExhausted":false,"NumericBoundary":false,"EarlyDoubleSelection":false,"Round":value.State.Casino.Poker.duplicate(true)}
    return value

func key(character: int) -> void:
    var event := InputEventKey.new()
    event.unicode=character
    event.pressed=true
    Input.parse_input_event(event)
    await process_frame

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("casino_card_chain_smoke requires disposable XDG_DATA_HOME")
        quit(1)
        return
    root.size=Vector2i(1280,720)
    write_json("user://original/catalog.json",{"SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6"})
    for locale in ["en","ja"]:
        var language := ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Card-chain locale " + locale)
        var value := casino_fixture()
        value.State.Casino.Coins=10
        await load_fixture(value)
        await press_node("TogglePause")
        await press_node("CompanyFacilities")
        await press_node("EnterCasino0")
        check(main.find_child("CasinoCardChain",true,false).text==tr("CASINO_CARD_CHAIN"),"Localized selector")
        await press_node("CasinoCardChain")
        check(main.find_child("CardChainDealDouble",true,false).disabled,"Deal disabled before bet")
        await press_node("CardChainBetCollect")
        await press_node("CardChainDealDouble")
        await press_node("ToolbarSAVE_GAME")
        var hand: Array=saved().Casino.CardChain.Round.Cards.duplicate(true)
        check(hand.size()==5 and saved().Casino.Coins==9 and saved().Casino.Bet==1,"Initial five cards and immediate debit")
        var ticks=saved().CompanyActivityTicks
        await create_timer(0.1).timeout
        await press_node("ToolbarSAVE_GAME")
        check(saved().CompanyActivityTicks==ticks,"Office paused")
        await press_node("ToolbarLOAD_GAME")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Casino.CardChain.Round.Cards==hand,"Saved hand retained")
        await press_node("CardChainCard0")
        await create_timer(0.18).timeout
        await press_node("ToolbarSAVE_GAME")
        check(saved().Casino.CardChain.ChainCount==1 and saved().Casino.CardChain.Round.UsedCards.size()==6,"Independent callback replaces one card")
        await press_node("CloseCardChain")
        await dispose()
        var shortcut := chain_round()
        await load_fixture(shortcut)
        await key(87)
        await press_node("ToolbarSAVE_GAME")
        check(saved().Casino.CardChain.Round.Cards.size()==0,"Uppercase shortcut not silently normalized")
        await key(119)
        await press_node("ToolbarSAVE_GAME")
        check(saved().Casino.Bet==0 and saved().Casino.CardChain.Round.Cards.size()==5,"Lowercase shortcut reaches zero-bet initial deal")
        await key(119)
        await create_timer(0.18).timeout
        await press_node("ToolbarSAVE_GAME")
        check(saved().Casino.CardChain.Round.UsedCards.size()==11 and saved().Casino.CardChain.ChainCount==1,"Copied keyboard branch consumes five plus one")
        await dispose()
        var exhausted := chain_round()
        exhausted.State.Casino.Bet=1
        var r: Dictionary=exhausted.State.Casino.CardChain
        r.ChainCount=47
        r.DeckExhausted=true
        r.Round.Phase=1
        for i in range(52):
            r.Round.UsedCards.append(i)
        for i in range(5):
            r.Round.Cards.append({"Suit":0,"Rank":i})
        await load_fixture(exhausted)
        check(main.find_child("CardChainPhase",true,false).text==tr("CARD_CHAIN_EXHAUSTED"),"Localized exhaustion explanation")
        check(main.find_child("CardChainCard0",true,false).disabled,"Exhausted deck pauses input")
        await press_node("ToolbarSAVE_GAME")
        await press_node("ToolbarLOAD_GAME")
        check(main.find_child("CardChainPhase",true,false).text==tr("CARD_CHAIN_EXHAUSTED"),"Exhaustion safely reloads")
        await press_node("CloseCardChain")
        await dispose()
    await process_frame
    await create_timer(1.0).timeout
    print("Card-chain smoke failures: ",failures)
    quit(0 if failures==0 else 1)
