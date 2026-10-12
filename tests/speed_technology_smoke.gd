extends "res://tests/stress_relief_smoke.gd"

func speed_fixture(known: bool=true, interrupted: bool=false) -> Dictionary:
    var value := stress_fixture()
    value.State.RetainedDevelopmentEligibility=0 if known else null
    var employee: Dictionary=value.State.Employees[1]
    employee.TechnologyLevels={"4":2,"0":1}
    employee.Fun=50
    employee.Novelty=50
    employee.Graphics=50
    employee.Music=50
    employee.Accuracy=50
    employee.DevelopmentSpeed=50 if known else null
    employee.Inspiration=50
    var project: Dictionary=value.State.DevelopmentProjects[0]
    project.OriginalStage=0
    project.RequiredPoints=10000
    project.Progress=0
    project.GameTypeSourceIndex=null
    project.GenreSourceIndex=null
    project.TechnologyApplicationCounts={"0":0,"1":0,"2":0,"3":0,"6":0,"7":0}
    if interrupted:
        employee.Office.WorkTier=4
        employee.Office.WorkTicks=3
        employee.Office.TechnologyApplication={"TechnologySourceIndex":0,"RemainingUnits":2}
    return value

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("speed_technology_smoke requires disposable data")
        quit(1)
        return
    write_json("user://original/catalog.json",{"SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6",
        "Hardware":[{"SourceIndex":0,"Identifier":0,"RawMembers":{"10A4":0,"10C0":0},"ProductionBands":[{"MaximumDataSize":16,"UnitCost":100},{"MaximumDataSize":32,"UnitCost":100},{"MaximumDataSize":64,"UnitCost":100},{"MaximumDataSize":128,"UnitCost":100}]}],
        "Technologies":[{"SourceIndex":4,"Identifier":4,"Name":"Synthetic speed technique","RawTable1568":[5,10,15],"RawTable158C":[]},
        {"SourceIndex":0,"Identifier":0,"Name":"Synthetic ordinary technique","RawTable1568":[5,10,15],"RawTable158C":[2,3,4]}]})
    for locale in ["en","ja"]:
        var language := ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Speed locale " + locale)
        await load_fixture(speed_fixture())
        await press_node("TogglePause")
        await open_selected_use()
        var scope=main.find_child("SpeedTechnologyNote",true,false)
        check(scope!=null,"Speed scope exists")
        if scope==null:
            await dispose()
            quit(1)
            return
        check(scope.text==tr("SPEED_TECHNOLOGY_NOTE"),"Localized speed scope")
        await press_node("UseTechnology4")
        await press_node("ToolbarSAVE_GAME")
        var worker: Dictionary=saved().Employees[1]
        check(worker.Experience==90 and worker.Office.SpeedBoostTicks==100 and worker.Office.TechnologyApplication.IsSpeedBoost,"Level-two timer and selected experience cost")
        await open_selected_use()
        var remaining=main.find_child("SpeedTechnologyRemaining",true,false)
        check(remaining!=null and remaining.text.contains("4.00"),"Speed duration rather than generic units")
        await press("BACK_TO_OFFICE")
        await press_node("TogglePause")
        await create_timer(0.24).timeout
        await press_node("TogglePause")
        await press_node("ToolbarSAVE_GAME")
        var ticks: int=saved().Employees[1].Office.SpeedBoostTicks
        check(ticks<100 and ticks>0,"Office ticks consume the separate timer")
        await press_node("ToolbarLOAD_GAME")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Employees[1].Office.SpeedBoostTicks==ticks,"Saved speed timer survives reload")
        await dispose()
        await load_fixture(speed_fixture(true,true))
        await press_node("TogglePause")
        await open_selected_use()
        var warning=main.find_child("SpeedTechnologyInterruptedWork",true,false)
        check(warning!=null and warning.text==tr("SPEED_TECHNOLOGY_INTERRUPTS_APPLICATION"),"Localized in-flight application warning")
        await press("SAVE_GAME")
        var before_ticks: int=saved().Employees[1].Office.WorkTicks
        await press_node("UseTechnology4")
        await press_node("ToolbarSAVE_GAME")
        worker=saved().Employees[1]
        check(worker.Office.WorkTicks==before_ticks and worker.Office.WorkTier==4 and worker.Office.TechnologyApplication.RemainingUnits==2,"Speed replacement retains the in-flight native work state")
        await dispose()
        await load_fixture(speed_fixture(false))
        await press_node("TogglePause")
        await open_selected_use()
        check(main.find_child("UseTechnology4",true,false).disabled,"Unknown retained frame disables speed")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Employees[1].Experience==100,"Unknown frame consumes no experience")
        await dispose()
    await process_frame
    await create_timer(1.0).timeout
    print("Speed technology smoke failures: ",failures)
    quit(0 if failures==0 else 1)
