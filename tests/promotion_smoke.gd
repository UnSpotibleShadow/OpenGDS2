extends "res://tests/project_panels_smoke.gd"

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("promotion_smoke requires disposable data")
        quit(1)
        return
    root.size=Vector2i(1280,720)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    blank_bmp("user://original/Graphic/Embedded/PlanningMeeting.bmp",256,160,true)
    panel_catalog()
    for locale in ["en","ja"]:
        var language := ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Promotion locale "+locale)
        var value := stress_fixture()
        value.State.Money=30000000
        value.State.DevelopmentProjects[0].PromotionUseCount=0
        Engine.time_scale=0
        await load_fixture(value)
        await press_node("OpenProject0")
        await press("PAID_PROMOTION")
        check(main.find_child("ConfirmPromotion",true,false).disabled,"No confirmation before media selection")
        var media=main.find_child("PromotionMedia",true,false)
        check(media.item_count==6,"Six source media")
        media.select(0)
        media.item_selected.emit(0)
        await press("CANCEL")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Money==30000000 and saved().PendingPromotion==null,"Selector cancellation costs nothing")
        await press("PAID_PROMOTION")
        media=main.find_child("PromotionMedia",true,false)
        media.select(0)
        media.item_selected.emit(0)
        await press_node("ConfirmPromotion")
        check(main.find_child("PromotionMeetingPicture",true,false).texture!=null,"Local meeting illustration")
        check(main.find_child("AcknowledgePromotion",true,false).disabled,"Meeting waits for callback 27")
        await press_node("ToolbarSAVE_GAME")
        var before := saved()
        check(before.PendingPromotion.Callbacks==0 and before.Money==30000000,"Unfinished meeting has not charged")
        await press_node("ToolbarLOAD_GAME")
        Engine.time_scale=100
        for frame in range(80):
            await process_frame
            if not main.find_child("AcknowledgePromotion",true,false).disabled:
                break
        Engine.time_scale=0
        await press_node("ToolbarSAVE_GAME")
        var after := saved()
        check(after.PendingPromotion.Callbacks==27 and after.Money==25000000 and after.DevelopmentProjects[0].PromotionUseCount==1,"Result charges once and increments count")
        await press_node("ToolbarLOAD_GAME")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Money==25000000 and saved().PendingPromotion.Gain==after.PendingPromotion.Gain,"Reload preserves completed transaction")
        await press_node("AcknowledgePromotion")
        check(main.find_child("DevelopmentProjectWindow",true,false)!=null,"Acknowledgment returns to project")
        await dispose()
    print("Promotion smoke failures: ",failures)
    quit(0 if failures==0 else 1)
