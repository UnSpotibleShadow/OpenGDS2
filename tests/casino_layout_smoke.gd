extends "res://tests/window_artwork_smoke.gd"

func casino_value() -> Dictionary:
    var v=complete_value();v.State.Calendar.OriginalSlots0444=[4,-1,-1]
    v.State.Casino={"Coins":50,"Bet":0,"PendingPayout":0,"ChainBonus":0,"PokerOpen":false,"Poker":{"Phase":0,"Tick":0,"Outcome":0,"Cards":[],"UsedCards":[],"Holds":[0,0,0,0,0],"Revealed":[false,false,false,false,false],"RevealOrder":[],"DealEnabled":false,"AwardReady":false,"AwardTick":0,"ChosenCard":null,"TieTick":0,"TieRedeal":false}}
    return v

func open_casino(game: String) -> void:
    await press_node("CompanyFacilities");await press_node("EnterCasino0");await press_node(game)

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,800);blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true);setup_catalog()
    var catalog=JSON.parse_string(FileAccess.get_file_as_string("user://original/catalog.json"));catalog.Horses=[];catalog.Jockeys=[]
    for i in range(44):catalog.Horses.append({"SourceIndex":i,"Name":"Runner %02d"%i,"RawWeight":450,"RawKind":3,"RawParameter":13,"Strength":i+.13})
    for i in range(10):catalog.Jockeys.append({"SourceIndex":i,"Name":"Rider %d"%i,"RawValue":5})
    write_json("user://original/catalog.json",integer_tokens(catalog))
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);check(language.save("user://interface.cfg")==OK,"Casino layout locale")
        await load_fixture(casino_value());await press_node("TogglePause")
        await open_casino("CasinoPoker");await press_node("PokerBetCollect");await press_node("PokerDealDouble")
        for frame in range(110):await process_frame
        await press_node("PokerCard0");await window("CasinoPokerWindow",430,350)
        check(main.find_child("PokerCard0",true,false).get_meta("revealed") and main.find_child("PokerCard0",true,false).get_meta("held"),"Native card reveal and hold state")
        await press_node("ClosePoker")
        await open_casino("CasinoBlackjack");await press_node("BlackjackBetStayCollect");await press_node("BlackjackDealMoreDouble")
        for frame in range(240):
            if main.find_child("BlackjackDealerCard0",true,false).is_visible_in_tree() and main.find_child("BlackjackPlayerCard0",true,false).is_visible_in_tree():break
            await process_frame
        await window("CasinoBlackjackWindow",430,350)
        check(main.find_child("BlackjackDealerCard0",true,false).is_visible_in_tree() and main.find_child("BlackjackPlayerCard0",true,false).is_visible_in_tree(),"Ordinary blackjack hands use composed cards")
        await press_node("CloseBlackjack")
        await open_casino("CasinoCardChain");await press_node("CardChainBetCollect");await press_node("CardChainDealDouble");await window("CasinoCardChainWindow",430,378)
        check(main.find_child("ChainDeckRemaining",true,false).text=="47","Card Chain shows actual remaining cards")
        await press_node("CloseCardChain")
        await open_casino("CasinoHorse");await window("CasinoHorseWindow",594,460)
        check(main.find_child("HorseRoster",true,false).get_root().get_child_count()==8 and main.find_child("HorseBettingMatrix",true,false).get_child_count()==8,"Race roster and wager matrix")
        await press_node("HorseStart")
        for frame in range(5):await process_frame
        await window("CasinoHorseWindow",594,460)
        check(main.find_child("HorseRaceTrack",true,false).is_visible_in_tree(),"Race uses dedicated track compositor")
        await dispose()
    print("Casino layout smoke failures: ",failures)
    quit(0 if failures==0 else 1)
