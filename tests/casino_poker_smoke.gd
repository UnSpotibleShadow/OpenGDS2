extends "res://tests/mystery_lab_smoke.gd"

func casino_fixture() -> Dictionary:
    var value := fixture()
    value.State.Money=10000001
    value.State.Calendar.OriginalSlots0444=[4,-1,-1]
    value.State.Casino={"Coins":0,"Bet":0,"PendingPayout":0,"ChainBonus":0,"PokerOpen":false,
        "Poker":{"Phase":0,"Tick":0,"Outcome":0,"Cards":[],"UsedCards":[],"Holds":[0,0,0,0,0],
            "Revealed":[false,false,false,false,false],"RevealOrder":[],"DealEnabled":false,"AwardReady":false,
            "AwardTick":0,"ChosenCard":null,"TieTick":0,"TieRedeal":false}}
    return value

func await_phase(phase: int) -> bool:
    for attempt in range(80):
        await press_node("ToolbarSAVE_GAME")
        if saved().Casino.Poker.Phase==phase:
            return true
        await create_timer(0.04).timeout
    return false

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("casino_poker_smoke requires disposable XDG_DATA_HOME")
        quit(1)
        return
    root.size=Vector2i(1280,720)
    write_json("user://original/catalog.json",{"SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6"})
    for locale in ["en","ja"]:
        var language := ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Casino locale " + locale)
        await load_fixture(casino_fixture())
        await press_node("TogglePause")
        await press_node("CompanyFacilities")
        await press_node("EnterCasino0")
        check(main.find_child("CasinoPoker",true,false).disabled,"Card entry requires positive coins")
        await press_node("CasinoBuyCoins")
        await press("CANCEL")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Casino.Coins==0 and saved().Money==10000001,"Declining coin purchase preserves money")
        await press_node("CasinoBuyCoins")
        await press_node("ConfirmCasinoCoins")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Casino.Coins==50 and saved().Money==1,"Accepted coin purchase debits once and adds fifty")
        check(saved().Company.WeeklyNet==-9999989,"Coin purchase updates source accounting")
        await press_node("CasinoPoker")
        check(main.find_child("PokerDealDouble",true,false).disabled,"Initial deal disabled until a bet")
        await press_node("PokerBetCollect")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Casino.Bet==1 and saved().Casino.Coins==49,"Bet spends exactly one coin")
        await press_node("PokerDealDouble")
        await press_node("ToolbarSAVE_GAME")
        var cards: Array=saved().Casino.Poker.Cards.duplicate(true)
        check(cards.size()==5 and saved().Casino.Poker.UsedCards.size()==5,"Unique initial hand generated")
        var ticks=saved().CompanyActivityTicks
        await create_timer(0.2).timeout
        await press_node("ToolbarSAVE_GAME")
        check(saved().CompanyActivityTicks==ticks,"Card activity pauses office simulation")
        check(main.find_child("ToolbarSETTINGS",true,false)==null,"No port setup shortcut in casino")
        await press_node("ToolbarLOAD_GAME")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Casino.Poker.Cards==cards,"Load resumes the same hand without reroll")
        for attempt in range(60):
            if not main.find_child("PokerDealDouble",true,false).disabled:
                break
            await create_timer(0.04).timeout
        check(not main.find_child("PokerDealDouble",true,false).disabled,"Initial reveal eventually enables draw")
        await press_node("PokerCard0")
        check(main.find_child("PokerCard0",true,false).get_meta("held"),"Holding is visible")
        await press_node("ClosePoker")
        await press_node("ToolbarSAVE_GAME")
        check(not saved().Casino.PokerOpen and saved().Casino.Bet==1 and saved().Casino.Coins==49,"Closing keeps spent stake without refund")
        check(saved().CompanyActivityPerformed,"Unload sets original company activity flag")
        await dispose()
        # Controlled authored winning hand: verify actual saved collection rather than a lucky random result.
        var winner := casino_fixture()
        winner.State.Casino.Coins=50
        winner.State.Casino.Bet=1
        winner.State.Casino.PendingPayout=3
        winner.State.Casino.PokerOpen=true
        var round: Dictionary=winner.State.Casino.Poker
        round.Phase=3
        round.Outcome=1
        round.AwardReady=true
        round.AwardTick=34
        round.Cards=[{"Suit":0,"Rank":9},{"Suit":1,"Rank":9},{"Suit":2,"Rank":4},{"Suit":3,"Rank":7},{"Suit":0,"Rank":11}]
        round.UsedCards=[9,22,30,46,11]
        round.Revealed=[true,true,true,true,true]
        await load_fixture(winner)
        check(main.find_child("PokerPhase",true,false).text.contains(tr("POKER_OUTCOME_1")),"Winning category localized " + locale)
        await press_node("PokerBetCollect")
        check(await await_phase(0),"Payout completes through timer callbacks")
        check(saved().Casino.Coins==53 and saved().Casino.PendingPayout==0 and saved().Casino.Bet==0,"Full payout returns total award without an extra stake")
        await press_node("ClosePoker")
        await dispose()
        winner.State.Casino.PokerOpen=true
        winner.State.Casino.Bet=1
        winner.State.Casino.PendingPayout=3
        round.Phase=3
        round.AwardReady=true
        await load_fixture(winner)
        await press_node("PokerDealDouble")
        for attempt in range(40):
            if not main.find_child("PokerCard1",true,false).disabled:
                break
            await create_timer(0.04).timeout
        check(main.find_child("PokerCard0",true,false).disabled and not main.find_child("PokerCard1",true,false).disabled,"Double-up hides dealer selection and enables four choices")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Casino.Poker.Phase==5 and saved().Casino.PendingPayout==3,"Double-up waits with saved award unchanged")
        await press_node("ClosePoker")
        await dispose()
    print("Casino poker smoke failures: ",failures)
    await create_timer(1).timeout
    quit(0 if failures==0 else 1)
