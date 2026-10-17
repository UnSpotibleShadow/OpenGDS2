extends "res://tests/casino_blackjack_smoke.gd"

func chain_boundary(numeric: bool) -> Dictionary:
    var value=casino_fixture();var c=value.State.Casino
    c.Coins=10;c.Bet=1;c.CardChainOpen=true
    var p=c.Poker.duplicate(true);p.Phase=1
    p.Cards=[{"Suit":0,"Rank":0},{"Suit":1,"Rank":1},{"Suit":1,"Rank":3},{"Suit":2,"Rank":5},{"Suit":3,"Rank":7}]
    p.UsedCards=[0,14,16,31,46] if numeric else range(52)
    c.CardChain={"Round":p,"ChainCount":0 if numeric else 47,"PreviousCard":null,"SelectedCard":0,"DeckExhausted":not numeric,"NumericBoundary":numeric,"EarlyDoubleSelection":false}
    return value

func check_boundary(value: Dictionary, label_name: String, key: String, close_name: String, controls: Array, huge: bool=false) -> void:
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("user://saves"))
    var raw=JSON.stringify(value,"",true,true)
    # Keep Int64 edge values exact; GDScript's generic parsed JSON uses doubles.
    if huge:raw=raw.replace('"PendingPayout":4','"PendingPayout":9223372036854775807')
    var file=FileAccess.open("user://saves/slot1.json",FileAccess.WRITE);file.store_string(raw);file.close()
    main=load("res://scenes/Main.tscn").instantiate();root.add_child(main);await process_frame;await press("LOAD_GAME")
    main.set_process(false)
    var label=main.find_child(label_name,true,false)
    check(label!=null,"Boundary warning exists: "+label_name)
    if label==null:await dispose();return
    check(label.text==tr(key),"Correct localized recovery warning: "+label_name)
    check(label.get_line_count()*label.get_line_height()<=label.size.y,"Full recovery warning fits: "+label_name)
    for control in controls:check(main.find_child(control,true,false).disabled,"Boundary stops unsafe action: "+control)
    check(not main.find_child(close_name,true,false).disabled,"Close remains available")
    await press_node("ToolbarSAVE_GAME")
    var before=FileAccess.get_file_as_string("user://saves/slot1.json")
    await press_node("ToolbarLOAD_GAME");await press_node("ToolbarSAVE_GAME")
    check(FileAccess.get_file_as_string("user://saves/slot1.json")==before,"Boundary reload has no financial or random effects")
    check(main.find_child(label_name,true,false).text==tr(key),"Recovery warning survives reload")
    await press_node(close_name);await press_node("ToolbarSAVE_GAME")
    check(not saved().Casino.get("PokerOpen",false) and not saved().Casino.get("BlackjackOpen",false) and not saved().Casino.get("CardChainOpen",false),"Close recovers office access")
    await dispose()

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,720)
    write_json("user://original/catalog.json",{"SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6"})
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);language.save("user://interface.cfg")
        await check_boundary(chain_boundary(false),"CardChainPhase","CARD_CHAIN_EXHAUSTED","CloseCardChain",["CardChainCard0","CardChainBetCollect","CardChainDealDouble"])
        await check_boundary(chain_boundary(true),"CardChainPhase","CASINO_NUMERIC_LIMIT_NOTE","CloseCardChain",["CardChainCard0","CardChainBetCollect","CardChainDealDouble"])
        var b=award_fixture();b.State.Casino.Blackjack.Win=false;b.State.Casino.Blackjack.AwardReady=false;b.State.Casino.Blackjack.PlayerCount=9;b.State.Casino.Blackjack.BoundaryReached=true
        await check_boundary(b,"BlackjackPhase","BLACKJACK_CARD_BOUNDARY","CloseBlackjack",["BlackjackDealMoreDouble","BlackjackBetStayCollect"])
        await check_boundary(award_fixture(),"BlackjackPhase","CASINO_NUMERIC_LIMIT_NOTE","CloseBlackjack",["BlackjackDealMoreDouble","BlackjackBetStayCollect"],true)
        var p=casino_fixture();p.State.Casino.PokerOpen=true;p.State.Casino.Coins=10;p.State.Casino.Bet=1;p.State.Casino.PendingPayout=4
        p.State.Casino.Poker.Phase=3;p.State.Casino.Poker.Outcome=1;p.State.Casino.Poker.AwardReady=true;p.State.Casino.Poker.AwardTick=34
        p.State.Casino.Poker.Cards=[{"Suit":0,"Rank":9},{"Suit":1,"Rank":9},{"Suit":2,"Rank":4},{"Suit":3,"Rank":7},{"Suit":0,"Rank":11}]
        p.State.Casino.Poker.UsedCards=[9,22,30,46,11]
        await check_boundary(p,"PokerPhase","CASINO_NUMERIC_LIMIT_NOTE","ClosePoker",["PokerDealDouble","PokerBetCollect"],true)
    print("Casino boundary smoke failures: ",failures);quit(0 if failures==0 else 1)
