extends "res://tests/issue118_smoke.gd"

var native_names: Dictionary

func expected_title(index: int,locale: String) -> String:
    if locale=="en":return tr("CATALOG_RIVAL_INITIAL_TITLE_%d"%index) if index<30 else tr("CATALOG_RIVAL_STAGED_TITLE_%d"%(index-30))
    return native_names.InitialRivalProducts[index].Title if index<30 else native_names.StagedRivalProducts[index-30].Product.Title

func names_fixture() -> Dictionary:
    var v=complete_value();var own=product();own.Title="Community Game";own.ReviewPending=false;own.ReviewDisplayCallbacks=90;own.UnitsSold=0
    v.State.ReleasedGames=[own]
    var products=native_names.InitialRivalProducts.duplicate(true)
    for release in native_names.StagedRivalProducts:products.append(release.Product.duplicate(true))
    products.append({"Title":own.Title,"Owner":-1,"OwnProduct":own.ProductIndex,"Hardware":0,"Demand":0,"SalesPower":0,"UnitsSold":own.UnitsSold})
    v.State.Competition={"Products":products,"PlatformAges":[0,0,0,0,0,0,0,0]}
    return v

func award_fixture() -> Dictionary:
    var v=names_fixture();v.State.Month=12;v.State.Week=4
    var a={"Year":3,"Entries":[0],"CategoryIndex":4,"ResultReady":true,"WinningRival":59,"PresentationPhase":3,"RivalEntries":[],"AnimationProducts":[],"Outcomes":[
        {"Category":0,"RivalIndex":0},{"Category":1,"RivalIndex":33},{"Category":2,"RivalIndex":13},{"Category":3,"ProductIndex":0},{"Category":4,"RivalIndex":59}]}
    for i in range(31):a.RivalEntries.append([0,33,13,59][i%4])
    for i in range(13):a.AnimationProducts.append(59)
    v.State.Calendar.Pending={"ClosingDate":{"Year":3,"Month":12,"Week":3},"Phase":0,"Index":0,"PendingEvent":8,"Awards":a}
    return v

func run() -> void:
    var private_art=OS.get_environment("OPENGDS2_PRIVATE_NATIVE_ART")
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    if private_art.is_empty():print("SKIP native game-name display checks: private imported catalog required");quit(0);return
    native_names=JSON.parse_string(FileAccess.get_file_as_string(private_art.path_join("catalog.json")))
    setup_issue118()
    var catalog=JSON.parse_string(FileAccess.get_file_as_string("user://original/catalog.json"))
    for key in ["InitialRivalProducts","StagedRivalProducts","RivalCompanies","GameTypes"]:catalog[key]=native_names[key]
    write_json("user://original/catalog.json",integer_tokens(catalog))
    for locale in ["en","ja"]:
        var settings=ConfigFile.new();settings.set_value("language","locale",locale);settings.save("user://interface.cfg")
        var v=award_fixture()
        await pause_load(v)
        for pair in [["Music",0],["Graphics",33],["Design",13],["GrandPrize",59]]:
            var text=main.find_child("AwardWinner"+pair[0],true,false).text
            check(text.contains(expected_title(pair[1],locale)),"Award board uses localized title: "+pair[0]+" "+locale)
        check(main.find_child("AwardWinnerWorst",true,false).text.contains("Community Game"),"Player winner name unchanged")
        var company=tr("CATALOG_RIVAL_COMPANY_9") if locale=="en" else native_names.RivalCompanies[9]
        var message=main.find_child("ConversationMessage",true,false)
        check(message.text.contains(expected_title(59,locale)) and message.text.contains(company),"Host dialogue localizes both title and publisher: "+locale)
        main.find_child("AwardCeremonyWindow",true,false).position=Vector2(340,250)
        main.find_child("ConversationWindow",true,false).position=Vector2(600,80)
        await capture_desktop("award-game-names")
        await press_node("ToolbarSAVE_GAME");var before=saved()
        await press_node("ToolbarLOAD_GAME");await press_node("ToolbarSAVE_GAME");var after=saved()
        check(after.Competition==before.Competition and after.Money==before.Money and after.SimulationRandomState==before.SimulationRandomState,"Award display/save/load preserves source names, funds and random state")
        await dispose()
        v=award_fixture();v.State.Calendar.Pending.Awards.PresentationPhase=2
        await pause_load(v)
        check(main.find_child("AwardAnnouncementText",true,false).text==expected_title(59,locale),"Announcement reel localizes its title immediately: "+locale)
        await capture_desktop("award-reel-names")
        await create_timer(1.3).timeout
        check(main.find_child("ConversationMessage",true,false).text.contains(expected_title(59,locale)),"Reel finishes into localized winner dialogue")
        await dispose()
        v=names_fixture()
        # The player is allowed to choose the exact Japanese name of a rival.
        v.State.ReleasedGames[0].Title=native_names.InitialRivalProducts[0].Title
        v.State.Competition.Products[65].Title=v.State.ReleasedGames[0].Title
        await pause_load(v);await press_node("ToolbarSALES_CHART")
        var table=main.find_child("SalesRankingTable",true,false);var row=table.get_root().get_first_child();var count=0
        while row!=null:
            var index=int(row.get_metadata(0));count+=1
            if index<65:
                check(row.get_text(1)==expected_title(index,locale),"Ranking title "+str(index)+" "+locale)
                var owner=int(v.State.Competition.Products[index].Owner)
                var publisher=tr("CATALOG_RIVAL_COMPANY_%d"%owner) if locale=="en" else native_names.RivalCompanies[owner]
                check(row.get_text(3)==publisher,"Ranking publisher "+str(owner)+" "+locale)
                check(row.get_tooltip_text(1)==row.get_text(1),"Full title available in tooltip")
            else:check(row.get_text(1)==v.State.ReleasedGames[0].Title,"Player title collision remains original in rankings")
            row=row.get_next()
        check(count==66,"All initial, staged and player chart rows checked")
        await capture_desktop("sales-game-names")
        await press_node("ToolbarSAVE_GAME");var persisted=saved().Competition.Products
        for index in range(66):check(persisted[index].Title==v.State.Competition.Products[index].Title,"Sales display never rewrites saved title "+str(index))
        await dispose()
        v=project_value();var p=v.State.DevelopmentProjects[0]
        p.PendingDecision=0;p.DecisionIntroductionAcknowledged=0;p.OriginalStage=0;p.RequiredPoints=300;p.Progress=150;p.GameTypeSourceIndex=null;p.GenreSourceIndex=null
        p.CheckpointMeeting={"Milestone":0,"Lines":[],"MessageIndex":0};p.TypeCandidates=[0,1,2,3,4]
        await pause_load(v)
        for index in range(5):
            var examples=[]
            for j in range(3):examples.append(tr("CATALOG_TYPE_EXAMPLE_%d_%d"%[index,j]) if locale=="en" else native_names.GameTypes[index].ExampleTitles[j])
            check(main.find_child("SelectType%d"%index,true,false).tooltip_text.ends_with("\n".join(examples)),"Development example titles and magazine localized: "+str(index)+" "+locale)
        await dispose()
    print("Game name localization smoke failures: ",failures);quit(0 if failures==0 else 1)
