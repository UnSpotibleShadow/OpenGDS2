extends "res://tests/window_artwork_smoke.gd"

func room_action(action: String) -> void:
    var event=InputEventMouseButton.new();event.button_index=MOUSE_BUTTON_LEFT;event.pressed=true;event.position=Vector2(192,148)
    main.find_child("OriginalOfficeBackground",true,false).gui_input.emit(event)
    await process_frame
    await press_node(action)

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,800)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true);setup_catalog()
    var catalog=JSON.parse_string(FileAccess.get_file_as_string("user://original/catalog.json"))
    catalog.Audiences[0].Identifier=1;catalog.Hardware[0].RawMembers["1050"]=80
    catalog.Manga=[{"SourceIndex":0,"Title":"Authored comic","Author":"Authored author","Publisher":"Authored publisher","PopularityDescription":"Popular","Popularity":10,"RightsFee":100}]
    write_json("user://original/catalog.json",integer_tokens(catalog))
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);check(language.save("user://interface.cfg")==OK,"Remaining layouts locale")
        var v=complete_value();v.State.DevelopmentProjects=[];v.State.Recruitment={"CandidateDatabaseDecoded":true}
        await load_fixture(v);await press_node("TogglePause")
        for screen in [["MENU_GAME",0,"SaveSlotWindow",450,400],["MENU_GAME",1,"LoadSlotWindow",450,400],["MENU_HELP",0,"HelpTopicWindow",425,405],["MENU_HELP",2,"ReachTimeRecordsWindow",580,278],["MENU_HELP",3,"SalesRankingWindow",525,323],["MENU_STAFF",1,"RecruitmentWindow",495,414]]:
            await menu_choice(screen[0],screen[1])
            if screen[2] in ["SaveSlotWindow","LoadSlotWindow"]:
                var slots=main.find_child("SaveSlotList",true,false);slots.get_root().get_first_child().select(0);slots.item_selected.emit()
            await window(screen[2],screen[3],screen[4])
            if screen[2]=="HelpTopicWindow":
                var selector=main.find_child("HelpTopicSelector",true,false);selector.select(3);selector.item_selected.emit(3)
                await window("HelpTopicWindow",425,405);check(main.find_child("HelpTitlePicture",true,false).texture!=null,"Help loads source title art")
            await close_window(screen[2])
        await room_action("RoomManga");await window("MangaSelectionWindow",442,318);await close_window("MangaSelectionWindow");await dispose()
        var sequel=complete_value();sequel.State.DevelopmentProjects=[];var parent=product();parent.ReviewPending=false;parent.ReviewDisplayCallbacks=90;parent.HallOfFame=true;parent.HasReleasedSequel=false;parent.ReviewScores=[8,8,8,8];parent.Recall={"OriginalState":1,"WeeksRemaining":0};sequel.State.ReleasedGames=[parent];sequel.State.Company.HallOfFameCount=1
        await load_fixture(sequel);await press_node("TogglePause");await room_action("RoomSequel");await window("SequelSelectionWindow",440,322);await dispose()
        var challenge=stress_fixture();challenge.SchemaVersion=2;challenge.State.DevelopmentProjects[0].OriginalStage=0;challenge.State.DevelopmentProjects[0].Progress=0;challenge.State.PendingProgrammingChallenge={"Inspiration":80,"IntroPage":2,"Mode":0};challenge.State.OfficeActorContinuation={"ActorSlot":1,"EmployeeSourceIndex":1,"RoomIndex":0}
        await load_fixture(challenge);await window("ProgrammingChallengeWindow",310,243);check(main.find_children("ProgrammingSlot*","Label",true,false).size()==10,"Vertical ten-slot reel");await dispose()
        var graduate=complete_value();graduate.State.DevelopmentProjects=[];graduate.State.Year=3;graduate.State.MinimumSourceIndex=0;graduate.State.MaximumSourceIndex=1;graduate.State.EmployeeSources[0].GateValue=0;graduate.State.Month=4;graduate.State.Week=2;graduate.State.Calendar.OriginalSlots0444=[0,-1,-1];graduate.State.Employees=[];graduate.State.EmployeeSources[0].Employee.ContractFee=100;graduate.State.Calendar.Pending={"ClosingDate":{"Year":3,"Month":4,"Week":1},"Phase":9,"Index":0,"PendingEvent":9,"Graduates":{"Candidates":[0],"CandidateIndex":0}}
        await load_fixture(graduate);await window("GraduateRecruitmentWindow",398,275);await dispose()
        for stage in range(1,5):
            var milestone=complete_value();milestone.State.DevelopmentProjects=[];var p=product();p.ReviewPending=false;p.ReviewDisplayCallbacks=90;p.ReviewScores=[4,4,4,4];p.Recall={"OriginalState":1,"WeeksRemaining":0};p.SalesMilestoneStage=stage-1;p.PendingSalesMilestones=[stage];milestone.State.ReleasedGames=[p]
            await load_fixture(milestone);await window("SalesAchievementWindow",290,261)
            var captures=OS.get_environment("OPENGDS2_PRIVATE_UI_CAPTURE")
            if not captures.is_empty():check(DirAccess.copy_absolute(captures.path_join(locale+"-SalesAchievementWindow.png"),captures.path_join(locale+"-SalesAchievement%d.png"%stage))==OK,"Individual milestone capture")
            await dispose()
    print("Remaining layout smoke failures: ",failures);quit(0 if failures==0 else 1)
