extends SceneTree

# Authored synthetic fixture: no original archive, graphics or strings required.
var main: Node
var failures := 0

func check(ok: bool, message: String) -> void:
    print(("PASS " if ok else "FAIL ") + message)
    if not ok:
        failures += 1

func write_json(path: String, value: Dictionary) -> void:
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(path.get_base_dir()))
    var file := FileAccess.open(path, FileAccess.WRITE)
    file.store_string(JSON.stringify(value))

func saved() -> Dictionary:
    return JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json")).State

func press_node(name: String) -> void:
    var button = main.find_child(name, true, false)
    check(button != null and not button.disabled, name)
    if button != null and not button.disabled:
        button.pressed.emit()
    await process_frame

func press(key: String) -> void:
    var found := false
    for button in main.find_children("*", "Button", true, false):
        if button.is_visible_in_tree() and button.text == tr(key):
            check(not button.disabled, "Enabled " + key)
            if not button.disabled:
                button.pressed.emit()
            found = true
            break
    check(found, "Localized button " + key)
    await process_frame

func _initialize() -> void:
    call_deferred("run")

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("project_cancellation_smoke requires disposable XDG_DATA_HOME")
        quit(1)
        return
    root.size = Vector2i(1280,720)
    write_json("user://original/catalog.json", {"SchemaVersion":1,
        "SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6"})
    for locale in ["en", "ja"]:
        var language := ConfigFile.new()
        language.set_value("language", "locale", locale)
        check(language.save("user://interface.cfg") == OK, "Language fixture " + locale)
        var employees := []
        var projects := []
        for room in range(2):
            employees.append({"Name":"Synthetic worker %d" % room,"SourceIndex":room,"PortraitId":1,"Level":1,"Experience":0,"Stress":78 if room == 0 else 11,
                "Fun":50,"Novelty":50,"Graphics":50,"Music":50,"Accuracy":50,"DevelopmentSpeed":50,"Inspiration":50,
                "Office":{"SlotIndex":room,"Action":6+6*room,"SeatedAction":6+6*room,"WorkEngaged":true,"MovementState":0,"Facing":1,
                    "WorkTicks":40 if room == 0 else 0,"WorkTier":3 if room == 0 else 0,"BurstTicks":2,"BurstCount":7}})
            projects.append({"RoomIndex":room,"Title":"Synthetic project %d" % room,"RequiredPoints":1000,"Focus":{"Fun":100,"Novelty":0,"Graphics":0,"Music":0}})
        write_json("user://saves/slot1.json",{"Format":"OpenGDS2","SchemaVersion":1,"State":{
            "Money":1234,"Year":1,"Month":4,"Week":1,"CompanyActivityTicks":0,"ProductionPerformedThisWeek":false,
            "Company":{"Name":"Cancellation UI fixture","PresidentName":"Tester","WeeklyNet":3,"MonthlyNet":4,"YearlyNet":5},
            "Employees":employees,"DevelopmentProjects":projects}})
        main = load("res://scenes/Main.tscn").instantiate()
        root.add_child(main)
        await process_frame
        check(TranslationServer.get_locale().begins_with(locale), "Interface locale " + locale)
        await press("LOAD_GAME")
        if main.find_child("GameDesktop",true,false) == null:
            for label in main.find_children("*","Label",true,false):
                if label.is_visible_in_tree():
                    print(label.text)
            check(false,"Load synthetic cancellation save")
            quit(1)
            return
        await press_node("TogglePause")
        await press_node("OpenProject0")
        await press("CANCEL_PROJECT")
        check(main.find_child("ProjectCancellationWindow",true,false) != null, "Compact cancellation window")
        await press_node("ToolbarSAVE_GAME")
        var before := saved()
        await press("KEEP_PROJECT")
        await press_node("ToolbarSAVE_GAME")
        check(saved() == before, "Declining does not mutate state")
        await press("CANCEL_PROJECT")
        # Unpause globally while the confirmation itself keeps the clock stopped.
        await press_node("TogglePause")
        await create_timer(0.5).timeout
        await press_node("ToolbarSAVE_GAME")
        check(saved() == before, "Confirmation pauses work and time")
        await press_node("ConfirmProjectCancellation")
        await press_node("ToolbarSAVE_GAME")
        var after := saved()
        check(after.DevelopmentProjects.size() == 1 and int(after.DevelopmentProjects[0].RoomIndex) == 1, "Only cancelled room is removed")
        check(is_equal_approx(after.Employees[0].Stress,min(99,before.Employees[0].Stress+20)) and after.Employees[1].Stress == before.Employees[1].Stress, "Stress transaction and unaffected room")
        check(after.Money == before.Money and after.Company.WeeklyNet == before.Company.WeeklyNet, "No refund or financial change")
        check(int(after.Employees[0].Office.Action) == 6 and int(after.Employees[0].Office.SeatedAction) == 6, "Desk assignment stays")
        check(int(after.Employees[0].Office.WorkTicks) == 0 and int(after.Employees[0].Office.WorkTier) == 0, "Pending work cleared")
        await press("BACK_TO_OFFICE")
        await press_node("ToolbarLOAD_GAME")
        check(main.find_child("OpenProject0",true,false) == null and main.find_child("OpenProject1",true,false) != null, "Saved cancellation survives reload")
        # Loading the confirmed save cannot repeat the stress transaction.
        await press_node("TogglePause")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Employees[0].Stress == after.Employees[0].Stress, "Reload never reapplies cancellation stress")
        main.queue_free()
        await process_frame
        await process_frame
    quit(1 if failures else 0)
