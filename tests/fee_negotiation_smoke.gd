extends SceneTree

# Authored synthetic fixture: no original archive, graphics or strings required.
var main: Node
var failures := 0

func advance_page() -> void:
    var message = main.find_child("ConversationMessage",true,false)
    check(message != null,"Negotiation conversation page exists")
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
        main.find_child("AdditionalActionsMenu",true,false).id_pressed.emit(0)
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
        for popup in desktop_popups():
            for item in range(popup.item_count):
                if popup.get_item_text(item)==tr(key):
                    popup.about_to_popup.emit()
                    check(not popup.is_item_disabled(item),"Menu command enabled: "+key)
                    if not popup.is_item_disabled(item):popup.id_pressed.emit(popup.get_item_id(item))
                    found=true
                    break
    check(found, "Localized button " + key)
    await process_frame

func _initialize() -> void:
    call_deferred("run")

func fixture(kind: int) -> Dictionary:
    var employee := {"Name":"Synthetic trainee","SourceIndex":0,"PortraitId":1,"RawParameter34":2,"Level":1,"Experience":17,"Stress":80,
        "TechnologyLevels":{"0":1}}
    return {"Format":"OpenGDS2","SchemaVersion":1,"State":{
        "Money":15000000,"SalesFee1":1500,"SalesFee2":1500,"Year":3,"Month":5,"Week":1,"CompanyActivityTicks":0,"CompanyActivityPerformed":false,
        "SourceUpperBound":1,"MinimumSourceIndex":0,"MaximumSourceIndex":0,"AvailableHardware":[0],"HardwareLicenses":[0],"ProductionPerformedThisWeek":false,
        "Company":{"Name":"Facility test","PresidentName":"Tester","ProductionCapacity":50,"WeeklyNet":11,"MonthlyNet":22,"YearlyNet":33},
        "Calendar":{"HardwareStage":0,"ShipmentsSinceHardwareTrigger":0,
            "Hardware":[{"SourceIndex":0,"Availability":1,"LaunchWeeks":0,"RetirementWeeks":0,"Market":40,"GrowthWeeks":0,"GrowthBoost":0}],
            "Loans":[{"OriginalType":0},{"OriginalType":0},{"OriginalType":0}],
            "Tenants":[{"OriginalState":0,"Weeks":0},{"OriginalState":0,"Weeks":0}],"OriginalSlots0444":[kind,-1,-1]},
        "Employees":[employee],"EmployeeSources":[{"SourceIndex":0,"GateValue":1,"Employee":employee}],
        "ReleasedGames":[{"ProductIndex":0,"Title":"Synthetic stock","Stock":7,"UnitsSold":9,"Price":1000}]}}

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("fee_negotiation_smoke requires disposable XDG_DATA_HOME")
        quit(1)
        return
    root.size = Vector2i(1280,720)
    var private_dialogue := OS.get_environment("OPENGDS2_LOCAL_DIALOGUE")
    if not private_dialogue.is_empty():
        DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("user://original"))
        check(DirAccess.copy_absolute(private_dialogue,ProjectSettings.globalize_path("user://original/dialogue.json")) == OK,"Optional local original dialogue cache")
    write_json("user://original/catalog.json",{"SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6"})
    for locale in ["en","ja"]:
        var language := ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg") == OK,"Locale fixture " + locale)
        for target in ["Transport","Retail"]:
            write_json("user://saves/slot1.json",fixture(5))
            main=load("res://scenes/Main.tscn").instantiate()
            root.add_child(main)
            await process_frame
            await press("LOAD_GAME")
            await press_node("TogglePause")
            await press_node("CompanyFacilities")
            check(main.find_child("OpenFacility1_5",true,false).disabled,"Duplicate lawyer purchase disabled")
            await press_node("Negotiate" + target + "0")
            await press("CANCEL")
            await press_node("ToolbarSAVE_GAME")
            check(saved().PendingFeeNegotiation == null and saved().SalesFee1 == 1500 and saved().SalesFee2 == 1500,"Declining consultation preserves fees")
            await press_node("Negotiate" + target + "0")
            await press_node("ConfirmFeeNegotiation")
            for cursor in range(5):
                var message = main.find_child("ConversationMessage",true,false)
                check(message != null and not message.text.begins_with("NEGOTIATION_"),"Translated negotiation dialogue " + str(cursor))
                var text: String = message.text
                await press_node("ToolbarSAVE_GAME")
                var state := saved()
                check(state.Money == 15000000 and state.Company.WeeklyNet == 11,"Original funds check without debit")
                check(state.PendingFeeNegotiation.MessageIndex == cursor,"Saved negotiation cursor")
                check(state.SalesFee2 == (1300 if target == "Transport" else 1500) and state.SalesFee1 == (1300 if target == "Retail" else 1500),"Only selected fee reduced")
                await create_timer(0.2).timeout
                await press_node("ToolbarLOAD_GAME")
                check(main.find_child("ConversationMessage",true,false).text == text,"Reload retains dialogue and outcome")
                main.find_child("CompanyFacilities",true,false).pressed.emit()
                await process_frame
                check(main.find_child("ConversationMessage",true,false).text == text,"Conversation blocks facility actions")
                await advance_page()
            await press_node("ToolbarSAVE_GAME")
            check(saved().PendingFeeNegotiation == null and saved().Money == 15000000,"Consultation completes without replay")
            main.queue_free()
            await process_frame
    print("lawyer negotiation UI failures: ",failures)
    quit(0 if failures == 0 else 1)

func desktop_popups() -> Array:
    var popups=[]
    for menu in main.find_children("*","MenuButton",true,false):
        var popup=menu.get_popup()
        popups.append(popup)
        popups.append_array(popup.find_children("*","PopupMenu",true,false))
    return popups
