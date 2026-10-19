extends SceneTree

# Synthetic domain/UI fixture; original assets and strings are not needed.
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

func office_input(button_index: int,pixel: Vector2) -> void:
    var office=main.find_child("OriginalOfficeBackground",true,false)
    check(office!=null,"Office input surface exists")
    if office==null:return
    var scale: float=min(office.size.x/466.0,office.size.y/290.0)
    var click=InputEventMouseButton.new();click.button_index=button_index;click.pressed=true
    click.position=(office.size-Vector2(466,290)*scale)/2.0+pixel*scale
    office.gui_input.emit(click);await process_frame

func press_node(name: String) -> void:
    if name=="TogglePause":
        check(main.call("SetSimulationPaused",not main.call("GetSimulationPaused")),"Harness simulation pause")
        await process_frame
        return
    if name=="ToolbarSTART_GAME_DEVELOPMENT":
        await office_input(MOUSE_BUTTON_LEFT,Vector2(192,150))
        await press_node("RoomOriginalGame")
        return
    if name=="CompanyFacilities":
        await office_input(MOUSE_BUTTON_LEFT,Vector2(80,220))
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
    if name in ["CompletionRelease","ProjectRelease"]:
        for page in range(50):
            var message=main.find_child("ConversationMessage",true,false)
            if message==null:break
            var key=InputEventKey.new();key.keycode=KEY_ENTER;key.pressed=true
            message.gui_input.emit(key);await process_frame
            if is_instance_valid(message) and message.is_inside_tree():
                await create_timer(.03).timeout;message.gui_input.emit(key);await process_frame
        if main.find_child("CloseMediaInformation",true,false)!=null:await press_node("CloseMediaInformation")

func press(key: String) -> void:
    for popup in desktop_popups():
        for item in range(popup.item_count):
            if popup.get_item_text(item)==tr(key):
                popup.about_to_popup.emit()
                check(not popup.is_item_disabled(item),"Menu command enabled: "+key)
                if not popup.is_item_disabled(item):popup.id_pressed.emit(popup.get_item_id(item))
                await process_frame
                return
    for button in main.find_children("*", "Button", true, false):
        if button.is_visible_in_tree() and (button.text == tr(key) or button.name=="Toolbar"+key):
            check(not button.disabled, key)
            if not button.disabled:
                button.pressed.emit()
            await process_frame
            return
    check(false,"Missing localized button " + key)

func fixture(kind: int) -> Dictionary:
    var employee := {"Name":"Synthetic trainee","SourceIndex":0,"PortraitId":1,"RawParameter34":2,"Level":1,"Experience":17,"Stress":80,
        "TechnologyLevels":{"0":1}}
    return {"Format":"OpenGDS2","SchemaVersion":1,"State":{
        "Money":100000000,"Year":3,"Month":5,"Week":1,"CompanyActivityTicks":0,"CompanyActivityPerformed":false,
        "SourceUpperBound":1,"MinimumSourceIndex":0,"MaximumSourceIndex":0,"AvailableHardware":[0],"HardwareLicenses":[0],"ProductionPerformedThisWeek":false,
        "Company":{"Name":"Facility test","PresidentName":"Tester","ProductionCapacity":50,"WeeklyNet":11,"MonthlyNet":22,"YearlyNet":33},
        "Calendar":{"HardwareStage":0,"ShipmentsSinceHardwareTrigger":0,
            "Hardware":[{"SourceIndex":0,"Availability":1,"LaunchWeeks":0,"RetirementWeeks":0,"Market":40,"GrowthWeeks":0,"GrowthBoost":0}],
            "Loans":[{"OriginalType":0},{"OriginalType":0},{"OriginalType":0}],
            "Tenants":[{"OriginalState":0,"Weeks":0},{"OriginalState":0,"Weeks":0}],"OriginalSlots0444":[kind,-1,-1]},
        "Employees":[employee],"EmployeeSources":[{"SourceIndex":0,"GateValue":1,"Employee":employee}],
        "ReleasedGames":[{"ProductIndex":0,"Title":"Synthetic stock","Stock":7,"UnitsSold":9,"Price":1000}]}}

func _initialize() -> void:
    call_deferred("run")

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("facility_management_smoke requires disposable XDG_DATA_HOME")
        quit(1)
        return
    root.size = Vector2i(1280,720)
    write_json("user://original/catalog.json",{"SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6"})
    for locale in ["en","ja"]:
        var language := ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg") == OK,"Locale fixture " + locale)
        for kind in [1,2,3,7]:
            write_json("user://saves/slot1.json",fixture(kind))
            main=load("res://scenes/Main.tscn").instantiate()
            root.add_child(main)
            await process_frame
            await press("LOAD_GAME")
            check(main.find_child("GameDesktop",true,false) != null,"Load facility fixture")
            await press_node("TogglePause")
            await press_node("CompanyFacilities")
            check(main.find_child("OpenFacility1_4",true,false).disabled,"Unfinished casino cannot be purchased")
            var action := "UseHotSpring0" if kind == 1 else ("RecycleStock0" if kind == 2 else ("TechnologyRoster0" if kind == 3 else "ExpandProduction0"))
            await press_node(action)
            if kind == 3:
                await press_node("StudyCapacity0")
            await press_node("ToolbarSAVE_GAME")
            var before := saved()
            await press("CANCEL")
            await press_node("ToolbarSAVE_GAME")
            check(saved() == before,"Declining service leaves cash and effects unchanged")
            await press_node(action)
            if kind == 3:
                await press_node("StudyCapacity0")
            await press_node("ConfirmFacilityService")
            check(main.find_child("FacilityResultWindow",true,false) != null,"Service result window")
            await press_node("ToolbarSAVE_GAME")
            var after := saved()
            check(after.PendingFacilityReceipt != null,"Saved result snapshot")
            if kind == 1:
                check(after.Money == 92000000 and after.Employees[0].Stress == 80 and after.EmployeeSources[0].Employee.Stress == 80,"Hot spring debit precedes secretary acknowledgement")
            elif kind == 2:
                check(after.Money == 100000000 and after.ReleasedGames[0].Stock == 0 and after.ReleasedGames[0].UnitsSold == 9,"Stock disposal precedes proceeds acknowledgement")
            elif kind == 3:
                check(after.Money == 60000000 and after.Employees[0].RawParameter34 == 3 and after.EmployeeSources[0].Employee.RawParameter34 == 3,"Study capacity and source synchronization")
                check(after.OriginalPayrollScratchIndex == 1,"Technology selector scratch writer")
            else:
                check(after.Money == 100000000 and after.Company.ProductionCapacity == 50,"Production waits for result acknowledgement")
                check(after.Company.WeeklyNet == 11,"Production debit does not change financial accumulator")
            var result_text: String = main.find_child("FacilityServiceResult",true,false).text
            check(not result_text.begins_with("FACILITY_"),"Translated service result")
            await create_timer(0.2).timeout
            await press_node("ToolbarLOAD_GAME")
            check(main.find_child("FacilityServiceResult",true,false).text == result_text,"Reload retains exact result")
            await press_node("ToolbarSAVE_GAME")
            var reloaded := saved()
            if reloaded != after:
                for key in after:
                    if reloaded.get(key) != after[key]:
                        print("RELOAD_DIFFERENCE ",key," before=",after[key]," after=",reloaded.get(key))
            check(reloaded == after,"Reload does not reroll or reapply service")
            check(main.find_child("ToolbarHIRE_EMPLOYEES",true,false).disabled,"Mutating toolbar action blocked by result")
            await press_node("AcknowledgeFacilityReceipt")
            await press_node("CompanyFacilities")
            await press_node("CloseFacility0")
            await press_node("ConfirmFacilityClosure")
            await press_node("ToolbarSAVE_GAME")
            var closed := saved()
            check(closed.Calendar.OriginalSlots0444[0] == -1 and closed.Money == after.Money,"Facility closure gives no refund")
            await press_node("OpenFacility0_1")
            await press_node("ToolbarSAVE_GAME")
            check(saved().Calendar.OriginalSlots0444[0] == 1 and saved().Money == after.Money-60000000,"Purchase uses exact hot spring price")
            main.queue_free()
            await process_frame
            await process_frame
    quit(1 if failures else 0)

func desktop_popups() -> Array:
    var popups=[]
    for menu in main.find_children("*","MenuButton",true,false):
        var popup=menu.get_popup()
        popups.append(popup)
        popups.append_array(popup.find_children("*","PopupMenu",true,false))
    return popups
