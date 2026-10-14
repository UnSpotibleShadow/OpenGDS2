extends "res://tests/review_round_smoke.gd"

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,800)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    setup_catalog()
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);check(language.save("user://interface.cfg")==OK,"Award continuation locale")
        var v=complete_value();v.State.Month=12;v.State.Week=4
        var own=product();own.ReviewPending=false;own.ReviewDisplayCallbacks=90;own.Quality={"Fun":0,"Novelty":0,"Graphics":0,"Music":0}
        v.State.ReleasedGames=[own]
        v.State.Competition={"Products":[{"Title":own.Title,"Owner":-1,"OwnProduct":own.ProductIndex,"Hardware":0,"Demand":0,"SalesPower":0,"UnitsSold":0},{"Title":"Authored rival game","Owner":0,"Hardware":0,"Demand":0,"SalesPower":0,"UnitsSold":100}],"PlatformAges":[0,0,0,0,0,0,0,0]}
        v.State.Calendar.Pending={"ClosingDate":{"Year":3,"Month":12,"Week":3},"Phase":0,"Index":0,"PendingEvent":8,"Awards":{"Year":3,"Entries":[0],"CategoryIndex":0,"RivalEntries":[]}}
        for i in range(31):v.State.Calendar.Pending.Awards.RivalEntries.append(1)
        await load_fixture(v)
        for frame in range(600):
            if main.find_child("AwardAnnouncementWindow",true,false)!=null:break
            if main.find_child("ConversationWindow",true,false)!=null:await award_page()
            else:await process_frame
        await create_timer(.2).timeout
        await press_node("ToolbarSAVE_GAME")
        var partial=saved();var a=partial.Calendar.Pending.Awards
        check(a.PresentationPhase==2 and a.ReelIndex>0 and a.ReelIndex<13,"Mid-reel snapshot has a bounded saved cursor")
        await press_node("ToolbarLOAD_GAME")
        check(main.find_child("AwardAnnouncementWindow",true,false)!=null,"Loading resumes the announcement popup")
        await press_node("ToolbarSAVE_GAME")
        var restored=saved()
        check(restored.SimulationRandomState==partial.SimulationRandomState and restored.Money==partial.Money,"Mid-reel load repeats neither draws nor payment")
        check(restored.Calendar.Pending.Awards.ReelIndex>=a.ReelIndex,"Mid-reel load does not reset display progress")
        await create_timer(1.3).timeout
        check(main.find_child("ConversationWindow",true,false).get_meta("award_phase")==3,"Reel finishes into unread host result dialogue")
        await press_node("ToolbarSAVE_GAME");var unread=saved()
        await press_node("ToolbarLOAD_GAME")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Calendar.Pending.Awards.PresentationPage==unread.Calendar.Pending.Awards.PresentationPage,"Unread result page is restored without acknowledgment")
        await dispose()
    print("Awards continuation failures: ",failures)
    quit(0 if failures==0 else 1)
