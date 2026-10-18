extends SceneTree

# Authored synthetic fixture: no original archive, graphics or strings required.
var main: Node
var failures := 0

func advance_page() -> void:
    var message = main.find_child("ConversationMessage",true,false)
    check(message != null,"Cancellation conversation page exists")
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
    if not found and key=="START_GAME_DEVELOPMENT":
        await office_input(MOUSE_BUTTON_LEFT,Vector2(192,150))
        if main.find_child("RoomOriginalGame",true,false)!=null:await press_node("RoomOriginalGame")
        found=true
    if not found and key=="SALES":
        main.find_child("MENU_INFORMATION",true,false).get_popup().id_pressed.emit(1);found=true
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
    var private_dialogue := OS.get_environment("OPENGDS2_LOCAL_DIALOGUE")
    if not private_dialogue.is_empty():
        DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("user://original"))
        check(DirAccess.copy_absolute(private_dialogue,ProjectSettings.globalize_path("user://original/dialogue.json")) == OK,"Optional local original dialogue cache")
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
        check(int(after.PendingProjectCancellation.MessageIndex) == 0 and after.PendingProjectCancellation.Staff.size() == 1,"Saved reaction and cursor")
        var reaction_key := "CANCELLATION_REACTION_%d" % int(after.PendingProjectCancellation.Staff[0].Variant)
        var window = main.find_child("ConversationWindow",true,false)
        check(window != null,"Staff reaction uses the compact conversation presenter")
        window.position = Vector2(350,170)
        check(main.find_child("ToolbarSTART_GAME_DEVELOPMENT",true,false)==null,"New project action blocked during cancellation messages")
        await create_timer(0.5).timeout
        await press_node("ToolbarSAVE_GAME")
        check(saved() == after,"Pending dialogue pauses all work and time")
        for page in range(3):
            await press_node("ToolbarSAVE_GAME")
            var page_save := saved()
            await press_node("ToolbarLOAD_GAME")
            check(main.find_child("ConversationWindow",true,false) != null,"Reload resumes cancellation conversation")
            var key := reaction_key if page == 0 else ("CANCELLATION_STRESS_SUMMARY" if page == 1 else "CANCELLATION_DISCARD_SUMMARY")
            var expected: String = tr(key) if locale == "en" else tr("CANCELLATION_REIMPORT_REQUIRED")
            if locale == "ja" and not private_dialogue.is_empty():
                expected = JSON.parse_string(FileAccess.get_file_as_string("user://original/dialogue.json")).Entries.get(key,tr("CANCELLATION_REIMPORT_REQUIRED"))
            expected = expected.replace("{room}","1")
            check(main.find_child("ConversationMessage",true,false).text == expected,"Exact selected reaction or ordered summary")
            await press_node("ToolbarSAVE_GAME")
            check(saved() == page_save,"Reload never rerolls or repeats penalties")
            window = main.find_child("ConversationWindow",true,false)
            window.position = Vector2(350,170)
            await advance_page()
            await press_node("ToolbarSAVE_GAME")
            var advanced := saved()
            if page < 2:
                check(int(advanced.PendingProjectCancellation.MessageIndex) == page+1,"Only acknowledged page advances")
                check(main.find_child("ConversationWindow",true,false).position == Vector2(350,170),"Moved conversation position persists")
            else:
                check(advanced.PendingProjectCancellation == null and main.find_child("ConversationWindow",true,false) == null,"Final summary completes the sequence")
                # Stop resumed ordinary simulation before checking the saved result.
                await press_node("TogglePause")
            check(advanced.Employees[0].Stress == after.Employees[0].Stress,"Acknowledgement never reapplies stress")
        await press_node("ToolbarLOAD_GAME")
        check(main.find_child("OpenProject0",true,false) == null and main.find_child("OpenProject1",true,false) != null, "Saved cancellation survives reload")
        # Loading the confirmed save cannot repeat the stress transaction.
        await press_node("ToolbarSAVE_GAME")
        check(saved().Employees[0].Stress == after.Employees[0].Stress, "Reload never reapplies cancellation stress")
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
