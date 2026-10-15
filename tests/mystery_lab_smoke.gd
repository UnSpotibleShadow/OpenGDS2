extends SceneTree

# Synthetic fixtures; no original archive, assets or dialogue required.
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
    if name=="TogglePause":
        check(main.call("SetSimulationPaused",not main.call("GetSimulationPaused")),"Harness simulation pause")
        await process_frame
        return
    if name=="ToolbarSTART_GAME_DEVELOPMENT":
        main.find_child("MENU_DEVELOPMENT",true,false).get_popup().id_pressed.emit(0)
        await process_frame
        return
    if name=="ToolbarBACK_TO_TITLE":
        var menu=main.find_child("MENU_GAME",true,false).get_popup()
        menu.id_pressed.emit(menu.item_count-1)
        await process_frame
        return
    var button = main.find_child(name, true, false)
    check(button != null and not button.disabled, name)
    if button != null and not button.disabled:
        button.pressed.emit()
    await process_frame

func press(key: String) -> void:
    var found := false
    for button in main.find_children("*", "Button", true, false):
        if button.is_visible_in_tree() and (button.text == tr(key) or button.name=="Toolbar"+key):
            check(not button.disabled, "Enabled " + key)
            if not button.disabled:
                button.pressed.emit()
            found = true
            break
    if not found:
        for menu in main.find_children("*","MenuButton",true,false):
            var popup=menu.get_popup()
            for item in range(popup.item_count):
                if popup.get_item_text(item)==tr(key):
                    popup.about_to_popup.emit()
                    check(not popup.is_item_disabled(item),"Menu command enabled: "+key)
                    if not popup.is_item_disabled(item):popup.id_pressed.emit(popup.get_item_id(item))
                    found=true
                    break
    check(found, "Localized button " + key)
    await process_frame

func advance_page() -> void:
    var message = main.find_child("ConversationMessage",true,false)
    check(message != null,"Lab result page exists")
    if message == null:
        return
    var accept := InputEventKey.new()
    accept.keycode = KEY_ENTER
    accept.pressed = true
    message.gui_input.emit(accept)
    await process_frame
    if is_instance_valid(message) and message.is_inside_tree():
        await create_timer(0.03).timeout
        message.gui_input.emit(accept)
        await process_frame

func fixture() -> Dictionary:
    var employee := {"Name":"Synthetic worker","SourceIndex":0,"PortraitId":1,"RawParameter34":2,"Level":1,"Experience":17,"Stress":80}
    return {"Format":"OpenGDS2","SchemaVersion":1,"State":{
        "Money":10000000,"Year":3,"Month":5,"Week":1,"CompanyActivityTicks":0,"CompanyActivityPerformed":false,
        "SourceUpperBound":1,"MinimumSourceIndex":0,"MaximumSourceIndex":0,"AvailableHardware":[0],"HardwareLicenses":[0],"ProductionPerformedThisWeek":false,
        "Company":{"Name":"Lab test","PresidentName":"Tester","Fame":12,"ProductionCapacity":50,"WeeklyNet":11,"MonthlyNet":22,"YearlyNet":33},
        "Calendar":{"HardwareStage":0,"ShipmentsSinceHardwareTrigger":0,
            "Hardware":[{"SourceIndex":0,"Availability":1,"LaunchWeeks":0,"RetirementWeeks":0,"Market":40,"GrowthWeeks":0,"GrowthBoost":0}],
            "Loans":[{"OriginalType":0},{"OriginalType":0},{"OriginalType":0}],
            "Tenants":[{"OriginalState":0,"Weeks":0},{"OriginalState":0,"Weeks":0}],"OriginalSlots0444":[6,-1,-1]},
        "MysteryLab":{"Progress":0,"Outcome":0},"Employees":[employee],"EmployeeSources":[{"SourceIndex":0,"GateValue":1,"Employee":employee}]}}

func load_fixture(value: Dictionary) -> void:
    write_json("user://saves/slot1.json",value)
    main=load("res://scenes/Main.tscn").instantiate()
    root.add_child(main)
    await process_frame
    await press("LOAD_GAME")

func dispose() -> void:
    main.queue_free()
    main=null
    await process_frame

func _initialize() -> void:
    call_deferred("run")

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("mystery_lab_smoke requires disposable XDG_DATA_HOME")
        quit(1)
        return
    root.size=Vector2i(1280,720)
    write_json("user://original/catalog.json",{"SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6"})
    for locale in ["en","ja"]:
        var language := ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg") == OK,"Locale fixture " + locale)
        await load_fixture(fixture())
        await press_node("TogglePause")
        await press_node("CompanyFacilities")
        await press_node("StartMysteryExperiment0")
        await press("CANCEL")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Money==10000000 and saved().MysteryLab.Progress==0,"Declining start preserves cash/progress")
        await press_node("StartMysteryExperiment0")
        await press_node("ConfirmFacilityService")
        await press_node("ToolbarSAVE_GAME")
        var experiment: Dictionary = saved().MysteryLab.duplicate()
        check(saved().Money==-15000000 and experiment.Progress==1,"Start accepts negative balance and debits once")
        check(saved().Company.WeeklyNet==11 and saved().Company.MonthlyNet==22 and saved().Company.YearlyNet==33,"Direct debit preserves accounting snapshots")
        await press_node("CompanyFacilities")
        check(main.find_child("StartMysteryExperiment0",true,false).disabled,"Second experiment disabled")
        await press_node("ToolbarLOAD_GAME")
        await press_node("ToolbarSAVE_GAME")
        check(saved().MysteryLab==experiment and saved().Money==-15000000,"Reload preserves selected outcome without charging again")
        await dispose()
        # Exercise the actual session ticker into the result modal.
        var completing := fixture()
        completing.State.MysteryLab={"Progress":99.9,"Outcome":2}
        await load_fixture(completing)
        for attempt in range(15):
            if main.find_child("ConversationMessage",true,false)!=null:
                break
            await create_timer(0.05).timeout
        await press_node("ToolbarSAVE_GAME")
        check(saved().Company.ProductionCapacity==100 and saved().PendingMysteryLabResult!=null,"Active simulation completes and applies capacity result")
        await dispose()
        for outcome in range(7):
            var result := fixture()
            result.State.MysteryLab={"Progress":100.0000000001,"Outcome":outcome}
            result.State.PendingMysteryLabResult={"Outcome":outcome,"MessageIndex":0,"Fame99":outcome==4}
            result.State.Money=12345678
            if outcome==4:
                result.State.Company.Fame=99
            await load_fixture(result)
            for cursor in range(2):
                var message = main.find_child("ConversationMessage",true,false)
                check(message!=null and not message.text.begins_with("MYSTERY_LAB_"),"Translated result " + locale + "/" + str(outcome) + "/" + str(cursor))
                if message==null:
                    break
                var page: String=message.text
                await press_node("ToolbarSAVE_GAME")
                check(saved().PendingMysteryLabResult.MessageIndex==cursor,"Saved result cursor")
                await press_node("ToolbarLOAD_GAME")
                check(main.find_child("ConversationMessage",true,false).text==page,"Reload restores current result page")
                main.find_child("CompanyFacilities",true,false).pressed.emit()
                await process_frame
                check(main.find_child("ConversationMessage",true,false).text==page,"Pending result blocks facility navigation")
                check(saved().Money==12345678,"Reload does not repeat result cash effects")
                await advance_page()
            await press_node("ToolbarSAVE_GAME")
            check(saved().PendingMysteryLabResult==null and saved().MysteryLab.Progress==0,"Last page resets progress")
            check(saved().MysteryLab.Outcome==outcome and saved().Money==12345678,"Last page retains outcome without changing cash")
            await dispose()
    await create_timer(1.0).timeout
    print("mystery laboratory UI failures: ",failures)
    quit(0 if failures==0 else 1)
