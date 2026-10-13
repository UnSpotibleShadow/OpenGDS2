extends "res://tests/review_event_smoke.gd"

# Authored fixtures exercise transaction cursors without redistributing originals.
func neutral() -> Dictionary:
    var value := stress_fixture()
    value.SchemaVersion=2
    value.State.DevelopmentProjects=[]
    value.State.Company.ActivityCost=5000000
    value.State.Company.HallOfFameCount=0
    value.State.SalesFee1=1500
    value.State.SalesFee2=1500
    return value

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("presentation_completion_smoke requires disposable data")
        quit(1)
        return
    root.size=Vector2i(1280,720)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    panel_catalog()
    artwork_fixture()
    for i in range(1,5):
        blank_bmp("user://original/Graphic/Embedded/SalesAchievement%d.bmp" % i,256,160,false)
    for i in range(1,3):
        blank_bmp("user://original/Graphic/Embedded/HallCelebration%d.bmp" % i,256,160,false)
    blank_bmp("user://original/Graphic/Embedded/HelpTitle.bmp",250,171,false)
    write_json("user://original/help.json",{"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6","Paragraphs":[{"Topic":0,"Top":0,"Text":"Synthetic locally imported help"}]})
    for locale in ["en","ja"]:
        DirAccess.remove_absolute(ProjectSettings.globalize_path("user://saves/slot2.json"))
        DirAccess.remove_absolute(ProjectSettings.globalize_path("user://saves/slot2.json.bak"))
        var language := ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Completion locale " + locale)
        var challenge := stress_fixture()
        challenge.SchemaVersion=2
        challenge.State.DevelopmentProjects[0].OriginalStage=0
        challenge.State.DevelopmentProjects[0].Progress=0
        challenge.State.Employees[1].Inspiration=80
        challenge.State.PendingProgrammingChallenge={"Inspiration":80,"IntroPage":2,"Mode":0}
        challenge.State.OfficeActorContinuation={"ActorSlot":1,"EmployeeSourceIndex":1,"RoomIndex":0}
        await load_fixture(challenge)
        check(main.find_child("ProgrammingReel",true,false).get_child_count()==10,"Challenge shows all ten ordered slots")
        await press_node("ProgrammingCommand")
        await press_node("ProgrammingCommand")
        await press_node("ToolbarSAVE_GAME")
        var stopped := saved()
        check(stopped.PendingProgrammingChallenge.Mode==2,"Stopped challenge saves deceleration")
        await press_node("ToolbarLOAD_GAME")
        for frame in range(120):
            if main.find_child("ConversationWindow",true,false)!=null:
                break
            await process_frame
        check(main.find_child("ConversationWindow",true,false)!=null,"Challenge reaches saved result dialogue")
        await press_node("ToolbarSAVE_GAME")
        var result := saved()
        await press_node("ToolbarLOAD_GAME")
        await advance_page()
        await advance_page()
        await press_node("ToolbarSAVE_GAME")
        var applied := saved()
        check(applied.PendingProgrammingChallenge.ResultPage==2,"Effect page retains an explicit commit cursor")
        await press_node("ToolbarLOAD_GAME")
        await press_node("ToolbarSAVE_GAME")
        check(saved().DevelopmentProjects[0].Quality==applied.DevelopmentProjects[0].Quality,"Reload cannot repeat programming quality reward")
        await advance_page()
        await press_node("ToolbarSAVE_GAME")
        check(saved().PendingProgrammingChallenge==null,"Final programming result dismisses the scene")
        check(int(result.PendingProgrammingChallenge.Outcome) in [0,1,2],"Challenge result is saved before presentation")
        await dispose()

        var review := neutral()
        var p := product()
        p.ReviewScores=[8,8,8,8]
        p.ReviewDisplayCallbacks=90
        review.State.ReleasedGames=[p]
        await load_fixture(review)
        await press_node("AcknowledgeReviews")
        await press_node("ToolbarSAVE_GAME")
        check(saved().ReleasedGames[0].ReviewSummaryPage==0 and saved().Company.HallOfFameCount==0,"Secretary assessment precedes Hall transaction")
        await advance_page()
        check(main.find_child("HallCelebrationWindow",true,false)!=null,"Assessment opens Hall banner")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Company.HallOfFameCount==1 and saved().ReleasedGames[0].ReviewSummaryPage==1,"Hall count commits once before sequel page")
        await press_node("ToolbarLOAD_GAME")
        await press_node("AcknowledgeHallCelebration")
        await advance_page()
        await press_node("ToolbarSAVE_GAME")
        check(saved().Company.HallOfFameCount==1 and saved().ReleasedGames[0].ReviewSummaryPage==null,"Hall sequel sequence completes without duplicate count")
        await dispose()

        var milestone := neutral()
        p=product()
        p.ReviewPending=false
        p.ReviewDisplayCallbacks=90
        p.ReviewScores=[4,4,4,4]
        p.UnitsSold=100000
        p.Recall={"OriginalState":1,"WeeksRemaining":0}
        p.SalesMilestoneStage=0
        p.PendingSalesMilestones=[1]
        milestone.State.ReleasedGames=[p]
        await load_fixture(milestone)
        check(main.find_child("SalesAchievementPicture",true,false).texture!=null,"Milestone loads its own illustration")
        await press_node("AcknowledgeSalesMilestone")
        await press_node("ToolbarSAVE_GAME")
        check(saved().ReleasedGames[0].SalesMilestonePage==1,"Milestone secretary page is resumable")
        await press_node("ToolbarLOAD_GAME")
        await advance_page()
        await press_node("ToolbarSAVE_GAME")
        check(saved().ReleasedGames[0].PendingSalesMilestones.is_empty(),"Milestone completes once")
        await press_node("ProduceProduct0")
        main.find_child("ProductionCopies",true,false).value=1200
        check(main.find_child("ProductionTotalCost",true,false).text=="¥600,000","Reproduction displays actual per-copy breakdown")
        await press("MEDIA_INFORMATION")
        check(main.find_child("MediaCapacityTable",true,false)!=null,"Media dialog contains capacity bands")
        await press("CANCEL")
        check(main.find_child("ProductionCopies",true,false).value==1200,"Media round trip preserves reproduction quantity")
        await dispose()

        var activity := neutral()
        activity.State.PendingCompanyActivity={"Activity":0}
        await load_fixture(activity)
        check(main.find_child("ActivityConfirmationWindow",true,false)!=null,"Company activity asks for confirmation")
        await press_node("ConfirmCompanyActivity")
        await press_node("ToolbarSAVE_GAME")
        var paid := saved()
        check(paid.Money==5000000 and paid.PendingCompanyActivity.Confirmed,"Activity charges only on confirmation")
        await press_node("ToolbarLOAD_GAME")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Money==paid.Money and saved().Company.Fame==paid.Company.Fame,"Result reload cannot repeat fee or fame")
        await advance_page()
        await press("BACK_TO_OFFICE")
        main.find_child("MENU_HELP",true,false).get_popup().id_pressed.emit(0)
        await process_frame
        var help=main.find_child("HelpTopicBody",true,false)
        check(help!=null and not help.text.is_empty(),"Help topic opens in both locales")
        if locale=="ja":
            check(help.text=="Synthetic locally imported help","Japanese help comes from the local import")
        var selector=main.find_child("HelpTopicSelector",true,false)
        selector.item_selected.emit(3)
        await process_frame
        check(main.find_child("HelpTitlePicture",true,false).texture!=null,"About topic uses the separate title illustration")
        await press("BACK_TO_OFFICE")
        main.find_child("MENU_GAME",true,false).get_popup().id_pressed.emit(0)
        await process_frame
        var save_slots=main.find_child("SaveSlotList",true,false)
        check(save_slots!=null,"Save menu opens numbered selection")
        save_slots.item_selected.emit(save_slots.item_count-1)
        await process_frame
        await press_node("ConfirmSaveSlot")
        check(FileAccess.file_exists("user://saves/slot2.json"),"Saving creates a second independent slot")
        # Save/load selection is also reachable from title, independent of quick-save.
        await press_node("ToolbarSAVE_GAME")
        await press_node("ToolbarBACK_TO_TITLE")
        await press("LOAD_SAVE_SLOTS")
        var slots=main.find_child("SaveSlotList",true,false)
        check(slots!=null and slots.item_count>0,"Numbered save picker lists existing modern slots")
        slots.item_selected.emit(slots.item_count-1)
        await process_frame
        check(not main.find_child("ConfirmSaveSlot",true,false).disabled and main.find_child("SaveSlotDetails",true,false).text.contains("Lab test"),"Selected slot has a usable company preview")
        await press_node("ConfirmSaveSlot")
        await dispose()

        await load_fixture(neutral())
        await press_node("ToolbarSAVE_GAME")
        var start_ticks: int=int(saved().CompanyActivityTicks)
        for frame in range(120):
            await process_frame
        await press_node("ToolbarSAVE_GAME")
        var elapsed_ticks: int=int(saved().CompanyActivityTicks)-start_ticks
        check(elapsed_ticks>=49 and elapsed_ticks<=51,"Integrated normal office clock runs 25 ticks per second")
        await dispose()

        var chart := neutral()
        chart.State.Competition={"PlatformAges":[0,0,0,0,0,0,0,0],"Products":[
            {"Title":"Synthetic leader","Owner":0,"Hardware":0,"UnitsSold":100,"Demand":0,"SalesPower":0},
            {"Title":"Synthetic rising game","Owner":1,"Hardware":0,"UnitsSold":0,"Demand":99,"SalesPower":100}]}
        await load_fixture(chart)
        main.find_child("MENU_HELP",true,false).get_popup().id_pressed.emit(3)
        await process_frame
        var table=main.find_child("SalesRankingTable",true,false)
        check(table.get_root().get_child_count()==2,"Live chart has no phantom container row")
        table.get_root().get_child(1).select(0)
        Engine.time_scale=100
        for frame in range(30):
            await process_frame
        Engine.time_scale=1
        check(table.get_root().get_child(0).get_text(1)=="Synthetic rising game","Sales chart updates its order while open")
        check(int(table.get_selected().get_metadata(0))==1,"Live reorder preserves selected product")
        await dispose()

        var hardware := neutral()
        hardware.State.Calendar.Pending={"ClosingDate":{"Year":3,"Month":4,"Week":4},"Phase":4,"Index":0,"PendingEvent":3}
        await load_fixture(hardware)
        check(main.find_child("HardwareAnnouncementPicture",true,false).texture!=null,"Hardware launch opens its separate device card")
        await press_node("AcknowledgeCalendar")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Calendar.Pending.HardwareLaunchPage==1,"Hardware acknowledgment page persists")
        await press_node("ToolbarLOAD_GAME")
        await advance_page()
        await press_node("ToolbarSAVE_GAME")
        check(saved().Calendar.Pending==null or saved().Calendar.Pending.PendingEvent!=3,"Hardware launch advances its existing calendar transaction once")
        await dispose()
    print("Presentation completion smoke failures: ",failures)
    quit(0 if failures==0 else 1)
