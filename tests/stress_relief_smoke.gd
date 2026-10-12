extends "res://tests/mystery_lab_smoke.gd"

func stress_fixture(mapped: bool=true) -> Dictionary:
    var value := fixture()
    value.State.Calendar.OriginalSlots0444=[-1,-1,-1]
    value.State.SourceUpperBound=2
    value.State.MaximumSourceIndex=1
    var first: Dictionary=value.State.Employees[0]
    first.Name="Synthetic first slot"
    first.Level=10
    first.Office={"SlotIndex":0 if mapped else 2,"Action":0,"SeatedAction":0,"MovementState":0,"WorkEngaged":false}
    first.TechnologyLevels={"5":0}
    var selected := first.duplicate(true)
    selected.Name="Synthetic selected"
    selected.SourceIndex=1
    selected.Stress=77.7
    selected.Experience=100
    selected.TechnologyLevels={"5":3}
    selected.Office={"SlotIndex":1,"Action":6,"SeatedAction":6,"MovementState":0,"WorkEngaged":true}
    value.State.Employees=[first,selected]
    value.State.EmployeeSources=[{"SourceIndex":0,"GateValue":1,"Employee":first},{"SourceIndex":1,"GateValue":1,"Employee":selected}]
    value.State.DevelopmentProjects=[{"Title":"Synthetic project","RoomIndex":0,"HardwareSourceIndex":0,"AudienceSourceIndex":0,
        "OriginalRoomState":1,"OriginalMode":0,"OriginalStage":3,"RequiredPoints":100,"Progress":100,"GameTypeSourceIndex":0,"GenreSourceIndex":0,"DataSize":100,"Bugs":0,"Focus":{"Fun":25,"Novelty":25,"Graphics":25,"Music":25}}]
    return value

func open_selected_use() -> void:
    await press("STAFF")
    for button in main.find_children("*","Button",true,false):
        if button.text==tr("USE_TECHNOLOGY") and not button.disabled:
            button.pressed.emit()
            await process_frame
            return
    check(false,"Selected worker technology action exists")

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("stress_relief_smoke requires disposable data")
        quit(1)
        return
    write_json("user://original/catalog.json",{"SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6",
        "Technologies":[{"SourceIndex":5,"Identifier":5,"Name":"Synthetic stress technique","RawTable1568":[5,10,15],"RawTable158C":[2,3,4]}]})
    for locale in ["en","ja"]:
        var language := ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Stress locale " + locale)
        await load_fixture(stress_fixture())
        await press_node("TogglePause")
        await open_selected_use()
        var note=main.find_child("StressReliefSourceTarget",true,false)
        check(note!=null and note.text==tr("STRESS_RELIEF_SLOT_ZERO").format(["Synthetic first slot"],"{_}"),"Localized actual source target")
        await press_node("UseTechnology5")
        await press_node("ToolbarSAVE_GAME")
        var people: Array=saved().Employees
        check(people[0].Stress==0 and people[1].Stress==77.7 and people[1].Experience==85,"Slot-zero effect and selected experience cost")
        await press_node("ToolbarLOAD_GAME")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Employees[0].Stress==0 and saved().Employees[1].Experience==85,"Stress result survives reload")
        await dispose()
        await load_fixture(stress_fixture(false))
        await press_node("TogglePause")
        await open_selected_use()
        check(main.find_child("UseTechnology5",true,false).disabled,"Unknown workspace mapping disables use")
        check(main.find_child("StressReliefSourceTarget",true,false).text==tr("STRESS_RELIEF_MAPPING_UNKNOWN"),"Localized missing mapping boundary")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Employees[1].Experience==100,"Unknown mapping does not consume experience")
        await dispose()
    await process_frame
    await create_timer(1.0).timeout
    print("Stress relief smoke failures: ",failures)
    quit(0 if failures==0 else 1)
