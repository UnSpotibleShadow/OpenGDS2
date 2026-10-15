extends "res://tests/window_artwork_smoke.gd"

func snapshot(label: String) -> void:
    var message=main.find_child("ConversationMessage",true,false)
    if message!=null and message.visible_characters>=0 and message.visible_characters<message.text.length():
        var enter=InputEventKey.new();enter.keycode=KEY_ENTER;enter.pressed=true;message.gui_input.emit(enter)
    await process_frame;await RenderingServer.frame_post_draw
    var folder=OS.get_environment("OPENGDS2_PRIVATE_UI_CAPTURE");DirAccess.make_dir_recursive_absolute(folder)
    check(root.get_texture().get_image().save_png(folder.path_join("en-"+label+".png"))==OK,"Private before view: "+label)

func project_value() -> Dictionary:
    var v=complete_value();var p=stress_fixture().State.DevelopmentProjects[0]
    p.Title="Temple Rumble";p.PromotionUseCount=0;p.Recognition=50;p.Quality={"Fun":17,"Novelty":28,"Graphics":39,"Music":41}
    v.State.DevelopmentProjects=[p];v.State.Money=500000000;v.State.SalesFee1=1500;v.State.SalesFee2=1500;v.State.Company.ProductionCapacity=200;v.State.SecretarySourceIndex=1
    return v

func room_action(action: String) -> void:
    var click=InputEventMouseButton.new();click.button_index=MOUSE_BUTTON_LEFT;click.pressed=true;click.position=Vector2(192,148)
    main.find_child("OriginalOfficeBackground",true,false).gui_input.emit(click);await process_frame;await press_node(action)

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,800);blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true);setup_catalog()
    var portrait_source=OS.get_environment("OPENGDS2_PRIVATE_NATIVE_ART").path_join("Graphic/Human")
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("user://original/Graphic/Human"))
    for filename in DirAccess.get_files_at(portrait_source):DirAccess.copy_absolute(portrait_source.path_join(filename),ProjectSettings.globalize_path("user://original/Graphic/Human/"+filename))
    var catalog=JSON.parse_string(FileAccess.get_file_as_string("user://original/catalog.json"))
    catalog.Manga=[{"SourceIndex":0,"Title":"Authored comic","Author":"Authored author","Publisher":"Authored publisher","PopularityDescription":"Popular","Popularity":10,"RightsFee":100}]
    write_json("user://original/catalog.json",integer_tokens(catalog))
    var settings=ConfigFile.new();settings.set_value("language","locale","en");settings.set_value("display","number_grouping",1);settings.save("user://interface.cfg")
    var v=project_value();v.State.DevelopmentProjects[0].PromotionUseCount=1;v.State.DevelopmentProjects[0].Recognition=67
    v.State.PendingPromotion={"RoomIndex":0,"MediaIndex":2,"Callbacks":27,"RecognitionBefore":50,"Gain":17}
    await load_fixture(v);main.call("SetSimulationPaused",true);await snapshot("promotion");await dispose()
    v=project_value();v.State.DevelopmentProjects[0].PendingDecision=2;v.State.DevelopmentProjects[0].DecisionIntroductionAcknowledged=2
    await load_fixture(v);main.call("SetSimulationPaused",true);await snapshot("completion");await press_node("CompletionRelease");await snapshot("shipment");await dispose()
    v=complete_value();v.State.DevelopmentProjects=[];var p=product();p.Title="Skyline Dreams";p.HallOfFame=true;p.ReviewPending=false;p.ReviewScores=[9,9,9,9];p.ReviewDisplayCallbacks=90;p.Recall={"OriginalState":1,"WeeksRemaining":0};p.Recognition=50;v.State.ReleasedGames=[p]
    await load_fixture(v);main.call("SetSimulationPaused",true);await room_action("RoomSequel");await snapshot("sequel");await dispose()
    v=complete_value();v.State.DevelopmentProjects=[]
    await load_fixture(v);main.call("SetSimulationPaused",true);await room_action("RoomManga");await snapshot("manga");await press_node("ConfirmManga");await snapshot("publisher");await dispose()
    v=complete_value();v.State.DevelopmentProjects=[];p=product();p.ReviewPending=false;p.ReviewDisplayCallbacks=90;p.Recall={"OriginalState":1,"WeeksRemaining":0};v.State.ReleasedGames=[p];v.State.Month=10;v.State.Week=1
    v.State.Calendar.Pending={"ClosingDate":{"Year":3,"Month":9,"Week":4},"Phase":7,"Index":0,"PendingEvent":7,"Exhibition":{"EligibleProducts":[0],"SelectedProducts":[0],"Visitors":14000,"ReportedAwarenessGain":28,"DisplayCallbacks":90,"PreviousVisitorRecord":0}}
    await load_fixture(v);main.call("SetSimulationPaused",true);await snapshot("exhibition");await dispose()
    v=project_value();v.State.DevelopmentProjects[0].InterviewCompleted=true;v.State.DevelopmentProjects[0].OriginalStage=2
    v.State.PendingMagazineInterview={"RoomIndex":0,"Page":2,"ReporterNameIndex":3,"PortraitId":1}
    v.State.CompanyEventContinuation={"PoachingProcessed":true,"NextInterviewRoom":1}
    await load_fixture(v);main.call("SetSimulationPaused",true);await snapshot("interview");await dispose()
    for kind in [7,5]:
        v=complete_value();v.State.DevelopmentProjects=[];v.State.Calendar.OriginalSlots0444=[kind,-1,-1];v.State.SecretarySourceIndex=1;v.State.SalesFee1=1500;v.State.SalesFee2=1500
        await load_fixture(v);main.call("SetSimulationPaused",true)
        var click=InputEventMouseButton.new();click.button_index=MOUSE_BUTTON_LEFT;click.pressed=true;click.position=Vector2(90,215)
        main.find_child("OriginalOfficeBackground",true,false).gui_input.emit(click);await process_frame
        if kind==7:await press_node("ExpandProduction0");await snapshot("facility")
        else:await press_node("NegotiateRetail0");await press_node("ConfirmFeeNegotiation");await snapshot("lawyer")
        await dispose()
    print("Review capture failures: ",failures);quit(0 if failures==0 else 1)
