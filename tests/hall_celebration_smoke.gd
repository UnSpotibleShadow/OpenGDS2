extends "res://tests/review_event_smoke.gd"

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("hall_celebration_smoke requires disposable data")
        quit(1)
        return
    root.size=Vector2i(1280,720)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    blank_bmp("user://original/Graphic/Embedded/HallCelebration1.bmp",256,160,true)
    blank_bmp("user://original/Graphic/Embedded/HallCelebration2.bmp",256,160,false)
    panel_catalog()
    for locale in ["en","ja"]:
        var language := ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Hall locale "+locale)
        var value := stress_fixture()
        value.State.DevelopmentProjects=[]
        value.State.ReleasedGames=[product()]
        value.State.ReleasedGames[0].ReviewScores=[8,8,8,8]
        value.State.ReleasedGames[0].ReviewPending=false
        value.State.ReleasedGames[0].ReviewDisplayCallbacks=90
        value.State.ReleasedGames[0].HallOfFame=true
        value.State.ReleasedGames[0].HallCelebration={"Ticks":3,"Frame":0}
        value.State.Company.HallOfFameCount=1
        Engine.time_scale=0
        await load_fixture(value)
        check(main.find_child("HallCelebrationWindow",true,false)!=null,"Pending Hall window resumes")
        await press_node("ToolbarSAVE_GAME")
        check(saved().ReleasedGames[0].HallCelebration.Ticks==3 and saved().Company.HallOfFameCount==1,"Saving initial banner does not repeat Hall count")
        Engine.time_scale=100
        for frame in range(12):
            await process_frame
        Engine.time_scale=0
        check(main.find_child("HallCelebrationPicture",true,false).texture!=null,"Locally imported banner animation displays")
        await press_node("ToolbarSAVE_GAME")
        var previous: Dictionary = saved().ReleasedGames[0].HallCelebration
        await press_node("ToolbarLOAD_GAME")
        await press_node("ToolbarSAVE_GAME")
        check(saved().ReleasedGames[0].HallCelebration==previous and saved().Company.HallOfFameCount==1,"Reload preserves banner phase without duplicate counter")
        await press_node("AcknowledgeHallCelebration")
        await press_node("ToolbarSAVE_GAME")
        check(saved().ReleasedGames[0].HallCelebration==null and saved().Company.HallOfFameCount==1,"Closing banner clears only presentation")
        await dispose()
    print("Hall celebration smoke failures: ",failures)
    quit(0 if failures==0 else 1)
