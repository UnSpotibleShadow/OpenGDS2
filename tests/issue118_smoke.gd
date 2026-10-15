extends "res://tests/window_artwork_smoke.gd"

func setup_issue118() -> void:
    root.size=Vector2i(1280,800)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    setup_catalog()
    var catalog=JSON.parse_string(FileAccess.get_file_as_string("user://original/catalog.json"))
    catalog.Manga=[{"SourceIndex":0,"Title":"Authored comic","Author":"Authored author","Publisher":"Authored publisher","PopularityDescription":"Popular","Popularity":10,"RightsFee":100}]
    catalog.GameTypes[0].ExampleTitles=["Example A","Example B","Authored specialist magazine"]
    write_json("user://original/catalog.json",integer_tokens(catalog))
    var humans="user://original/Graphic/Human"
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(humans))
    var private_art=OS.get_environment("OPENGDS2_PRIVATE_NATIVE_ART")
    if not private_art.is_empty():
        for filename in DirAccess.get_files_at(private_art.path_join("Graphic/Human")):
            check(DirAccess.copy_absolute(private_art.path_join("Graphic/Human").path_join(filename),ProjectSettings.globalize_path(humans.path_join(filename)))==OK,"Copy local portrait fixture")
    else:
        for portrait in range(1,11):blank_bmp(humans.path_join("Human%d.bmp"%portrait),64,64,true)
    for i in range(6):
        if not FileAccess.file_exists("user://original/Graphic/Embedded/Promotion%d.bmp"%i):blank_bmp("user://original/Graphic/Embedded/Promotion%d.bmp"%i,256,152 if i in [1,2] else 160,i%2==0)
    if not FileAccess.file_exists("user://original/Graphic/Embedded/PublisherMeeting.bmp"):blank_bmp("user://original/Graphic/Embedded/PublisherMeeting.bmp",256,160,true)
    for name in ["BusinessMeeting","Facility-ProductionLab","Facility-Recycling"]:
        if not FileAccess.file_exists("user://original/Graphic/Embedded/"+name+".png"):png("user://original/Graphic/Embedded/"+name+".png",Vector2i(256,160),Color.LIGHT_BLUE)

func project_value(stage: int=2) -> Dictionary:
    var v=complete_value();var p=stress_fixture().State.DevelopmentProjects[0]
    p.OriginalStage=stage;p.Progress=50 if stage<3 else 100;p.PendingDecision=null;p.PromotionUseCount=0;p.InterviewCompleted=false
    p.Recognition=50;p.Quality={"Fun":17,"Novelty":28,"Graphics":39,"Music":41}
    v.State.DevelopmentProjects=[p];v.State.Money=500000000;v.State.SecretarySourceIndex=0
    v.State.SalesFee1=1500;v.State.SalesFee2=1500;v.State.Company.ProductionCapacity=200
    return v

func pause_load(v: Dictionary) -> void:
    await load_fixture(v);main.call("SetSimulationPaused",true)

func capture_desktop(label: String) -> void:
    var output=OS.get_environment("OPENGDS2_PRIVATE_UI_CAPTURE")
    if output.is_empty() or DisplayServer.get_name()=="headless":return
    var message=main.find_child("ConversationMessage",true,false)
    if message!=null and message.visible_characters>=0 and message.visible_characters<message.text.length():
        var enter=InputEventKey.new();enter.keycode=KEY_ENTER;enter.pressed=true;message.gui_input.emit(enter)
    await process_frame;await RenderingServer.frame_post_draw
    DirAccess.make_dir_recursive_absolute(output)
    check(root.get_texture().get_image().save_png(output.path_join(TranslationServer.get_locale()+"-"+label+".png"))==OK,"Private review image "+label)

func promotion_cases() -> void:
    for medium in range(6):
        Engine.time_scale=0
        await pause_load(project_value());await press_node("OpenProject0");await press_node("ProjectPromotion")
        await window("PromotionSelectorWindow",320,240)
        var media=main.find_child("PromotionMedia",true,false)
        check(media.columns==2 and media.get_root().get_child_count()==6,"Six media in separate method/fee columns")
        var row=media.get_root().get_child(medium);row.select(0);media.item_selected.emit()
        var selector=main.find_child("PromotionSelectorWindow",true,false);selector.position=Vector2(820,420)
        await press_node("ConfirmPromotion")
        check(main.find_child("PromotionMeetingPicture",true,false).texture!=null,"Medium-specific scene "+str(medium))
        check(is_instance_valid(selector) and selector.position==Vector2(820,420),"Selector stays in place behind promotion")
        await press_node("ToolbarSAVE_GAME");var before=saved()
        check(before.PendingPromotion.Callbacks==0 and before.Money==500000000,"Promotion waits without charging")
        await press_node("ToolbarLOAD_GAME");Engine.time_scale=100
        for i in range(100):
            await process_frame
            if main.find_child("ConversationWindow",true,false)!=null:break
        Engine.time_scale=1
        check(main.find_child("PlanningDiscussionPicture",true,false)!=null,"Promotion result has a separate scene")
        await press_node("ToolbarSAVE_GAME");var after=saved()
        check(after.PendingPromotion.Callbacks==27 and after.DevelopmentProjects[0].PromotionUseCount==1,"Promotion outcome committed once")
        if medium==2:await capture_desktop("promotion")
        await press_node("ToolbarLOAD_GAME");await press_node("ToolbarSAVE_GAME")
        check(saved().Money==after.Money and saved().PendingPromotion.Gain==after.PendingPromotion.Gain,"Reload never repeats promotion charge or gain")
        await award_page();check(main.find_child("DevelopmentProjectWindow",true,false)!=null,"Secretary acknowledgement returns to development")
        await dispose()

func form_cases() -> void:
    var v=project_value(3);v.State.DevelopmentProjects[0].PendingDecision=2;v.State.DevelopmentProjects[0].DecisionIntroductionAcknowledged=2
    await pause_load(v);await window("CompletionWindow",380,352)
    check(main.find_child("CompletionMetadata0",true,false)!=null and main.find_child("ProjectQuality1",true,false).text=="28","Completion has separate metadata cells and native quality order")
    await capture_desktop("completion");await press_node("CompletionRelease");await window("ShipmentWindow",434,374)
    var shipment=main.find_child("ShipmentWindow",true,false);shipment.position=Vector2(650,200)
    main.find_child("ReleaseCopies",true,false).value=10100
    main.find_child("ReleasePrice",true,false).value=6400
    main.find_child("ReleaseTitle",true,false).text="Authored shipment"
    check(main.find_child("ShipmentAllSold",true,false).text=="¥34,340,000","Ruled table recalculates receipts")
    var edit=main.find_child("ReleaseCopies",true,false).get_line_edit()
    await process_frame
    edit.release_focus()
    check(edit.text.replace(",","").replace(".","").contains("10100"),"Five-digit copies are formatted completely")
    check(edit.get_theme_font("font").get_string_size(edit.text,HORIZONTAL_ALIGNMENT_LEFT,-1,edit.get_theme_font_size("font_size")).x<=edit.size.x-6,"Five-digit copies fit inside the field")
    await capture_desktop("shipment")
    await press_node("ShipmentMediaInformation");await window("MediaInformationWindow",300,290)
    await press_node("CloseMediaInformation")
    check(main.find_child("ReleaseTitle",true,false).text=="Authored shipment" and main.find_child("ReleaseCopies",true,false).value==10100,"Media return preserves title and quantity draft")
    check(main.find_child("ShipmentWindow",true,false).position==Vector2(650,200),"Shipment retains dragged position")
    await dispose()
    v=complete_value();v.State.DevelopmentProjects=[]
    var game=product();game.ReviewPending=false;game.ReviewDisplayCallbacks=90;game.HallOfFame=true;game.ReviewScores=[9,9,9,9];game.Recall={"OriginalState":1,"WeeksRemaining":0};game.Recognition=50
    v.State.ReleasedGames=[game]
    await pause_load(v);await room_action("RoomSequel");await window("SequelSelectionWindow",420,344)
    check(main.find_child("SequelPredecessors",true,false).columns==3 and main.find_child("SequelQuality1",true,false).text=="80","Sequel list and selected qualities match native fields")
    check(not main.find_child("SequelDetail0",true,false).text.is_empty(),"Sequel selected audience is shown")
    await capture_desktop("sequel");await dispose()
    v=complete_value();v.State.DevelopmentProjects=[]
    await pause_load(v);await room_action("RoomManga");await window("MangaSelectionWindow",420,315)
    check(main.find_child("MangaDetails3",true,false).text==(tr("CATALOG_MANGA_AUTHOR_0") if TranslationServer.get_locale().begins_with("en") else "Authored author") and not main.find_child("MangaDetails4",true,false).text.is_empty(),"Manga author and publisher are separate cyan cells")
    await capture_desktop("manga");await press_node("ConfirmManga")
    check(main.find_child("PlanningDiscussionPicture",true,false).texture!=null,"Manga rights uses publisher building artwork")
    check(main.find_child("ConversationPortrait",true,false).visible and main.find_child("ConversationPortrait",true,false).texture!=null,"Publisher representative uses the recovered random-staff portrait binding")
    await capture_desktop("publisher");await dispose()

func room_action(action: String) -> void:
    var input=InputEventMouseButton.new();input.button_index=MOUSE_BUTTON_LEFT;input.pressed=true;input.position=Vector2(192,148)
    main.find_child("OriginalOfficeBackground",true,false).gui_input.emit(input);await process_frame;await press_node(action)

func event_cases() -> void:
    var v=project_value();v.State.DevelopmentProjects[0].InterviewCompleted=true
    v.State.PendingMagazineInterview={"RoomIndex":0,"Page":2,"ReporterNameIndex":3,"PortraitId":1}
    v.State.CompanyEventContinuation={"PoachingProcessed":true,"NextInterviewRoom":1,"NextVirusRoom":0}
    await pause_load(v)
    check(main.find_child("PlanningDiscussionPicture",true,false).texture!=null,"Interview has the native business scene")
    await capture_desktop("interview");await press_node("ToolbarSAVE_GAME");await press_node("ToolbarLOAD_GAME")
    await award_page();await award_page();await award_page();await press_node("ToolbarSAVE_GAME")
    check(saved().PendingMagazineInterview.Page==5 and saved().DevelopmentProjects[0].Recognition==70,"Interview applies gain after final reporter page")
    await press_node("ToolbarLOAD_GAME");await award_page();await press_node("ToolbarSAVE_GAME")
    check(saved().PendingMagazineInterview==null and saved().DevelopmentProjects[0].Recognition==70,"Interview final receipt never repeats gain")
    await dispose()
    v=complete_value();v.State.DevelopmentProjects=[]
    var shown=product();shown.ReviewPending=false;shown.ReviewDisplayCallbacks=90;shown.Recall={"OriginalState":1,"WeeksRemaining":0};v.State.ReleasedGames=[shown]
    v.State.Month=10;v.State.Week=1
    v.State.Calendar.Pending={"ClosingDate":{"Year":3,"Month":9,"Week":4},"Phase":7,"Index":0,"PendingEvent":7,"Exhibition":{"EligibleProducts":[0],"SelectedProducts":[0],"Visitors":14000,"ReportedAwarenessGain":28,"DisplayCallbacks":90,"PreviousVisitorRecord":0}}
    await pause_load(v)
    check(main.find_child("ExhibitionAttendanceWindow",true,false)!=null and main.find_child("ConversationWindow",true,false)!=null,"Exhibition shows attendance and secretary windows together")
    await capture_desktop("exhibition");await press_node("ToolbarSAVE_GAME");await press_node("ToolbarLOAD_GAME");await award_page()
    await press_node("ToolbarSAVE_GAME");check(saved().Calendar.Pending.Exhibition.ResultAcknowledged,"Exhibition result acknowledgement is saved")
    check(not main.find_child("CloseExhibition",true,false).disabled,"Attendance close enables after secretary result")
    await dispose()

func facility_cases() -> void:
    var v=complete_value();v.State.DevelopmentProjects=[];v.State.Calendar.OriginalSlots0444=[7,-1,-1]
    await pause_load(v)
    var click=InputEventMouseButton.new();click.button_index=MOUSE_BUTTON_LEFT;click.pressed=true;click.position=Vector2(90,215)
    main.find_child("OriginalOfficeBackground",true,false).gui_input.emit(click);await process_frame
    var owner=main.find_child("FacilityProductionLabWindow",true,false);owner.position=Vector2(720,330)
    await press_node("ExpandProduction0")
    check(is_instance_valid(owner) and owner.position==Vector2(720,330),"Full facility window remains at its dragged position behind the secretary")
    check(main.find_child("ConversationWindow",true,false)!=null,"Facility action begins with a secretary conversation")
    await capture_desktop("facility");await award_page();await window("FacilityConfirmationWindow",344,162)
    await press_node("ConfirmFacilityService");await press_node("ToolbarSAVE_GAME");var paid=saved()
    check(paid.PendingFacilityReceipt!=null and paid.Money==v.State.Money-10000000,"Facility action charges once before result")
    await press_node("ToolbarLOAD_GAME");await award_page();await press_node("ToolbarSAVE_GAME")
    check(saved().PendingFacilityReceipt==null and saved().Money==paid.Money,"Facility receipt reload does not charge twice")
    main.find_child("OriginalOfficeBackground",true,false).gui_input.emit(click);await process_frame
    await press_node("CloseFacility0");await award_page();await window("FacilityClosureWindow",344,162)
    await press_node("CancelFacilityService");check(main.find_child("FacilityProductionLabWindow",true,false).position==Vector2(720,330),"Cancelled confirmation restores the owner position");await press_node("ToolbarSAVE_GAME")
    check(saved().Calendar.OriginalSlots0444[0]==7,"Cancellation preserves facility")
    await press_node("CloseFacility0");await award_page();await press_node("ConfirmFacilityClosure");await press_node("ToolbarSAVE_GAME")
    check(saved().Calendar.OriginalSlots0444[0]==-1 and saved().Money==paid.Money,"Closure frees plot without refund")
    await dispose()

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    setup_issue118()
    for locale in ["en","ja"]:
        var settings=ConfigFile.new();settings.set_value("language","locale",locale);settings.set_value("display","number_grouping",1);check(settings.save("user://interface.cfg")==OK,"Issue118 locale "+locale)
        await promotion_cases();await form_cases();await event_cases();await facility_cases()
    print("Issue118 smoke failures: ",failures);quit(0 if failures==0 else 1)
