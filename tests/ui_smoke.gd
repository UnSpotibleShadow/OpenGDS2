extends SceneTree
# Run on a disposable user-data directory. Save checks require explicit opt-in.
var main: Node
var failures := 0

func _initialize() -> void:
    call_deferred("run")

func nodes_of_type(type_name: String, parent: Node = main) -> Array[Node]:
    var found: Array[Node] = []
    for child in parent.get_children():
        if child.is_class(type_name):
            found.append(child)
        found.append_array(nodes_of_type(type_name, child))
    return found

func check(condition: bool, message: String) -> void:
    if not condition:
        failures += 1
        push_error("FAIL UI: " + message)

func button(key: String) -> Button:
    for node in nodes_of_type("Button"):
        if node.text == tr(key) and node.is_visible_in_tree():
            return node
    check(false, "Button missing: " + key)
    return null

func press(key: String) -> void:
    var target := button(key)
    if target != null:
        check(not target.disabled, "Button disabled: " + key)
        if not target.disabled:
            target.pressed.emit()
    await process_frame

func run() -> void:
    main = load("res://scenes/Main.tscn").instantiate()
    root.add_child(main)
    await process_frame
    check(button("NEW_GAME") != null, "Title C# initialization")
    await press("SETTINGS")
    if DirAccess.get_files_at("user://music").is_empty():
        check(not main.find_child("MusicPlayer",true,false).playing,"Title does not play background music")
        check(not button("NEXT_MUSIC_TRACK").disabled,"Bundled playlist enables next track")
        var durations := [182.0,144.0,64.96,60.6,65.16,151.16,132.28,131.16]
        for index in range(durations.size()):
            var track = load("res://audio/community/office-%02d.ogg" % (index+1))
            check(track != null and abs(track.get_length()-durations[index]) < 0.05,"Bundled music duration %d" % (index+1))
    await press("日本語")
    check(TranslationServer.get_locale() == "ja", "Japanese setting")
    check(tr("DEVELOPMENT_COMPUTERS") == "開発パソコン", "Japanese computer translation loaded")
    await press("BACK_TO_TITLE")
    await press("SETTINGS")
    await press("English")
    check(tr("HARDWARE_LICENSES") == "Hardware Licenses", "English license translation loaded")
    check(tr("DEVELOPMENT_PLANNING") == "Development Planning", "English planning translation loaded")
    await press("BACK_TO_TITLE")
    var fixture := OS.get_environment("OPENGDS2_ORIGINAL_LZH")
    if not fixture.is_empty():
        await press("IMPORT_BUTTON")
        nodes_of_type("FileDialog")[0].file_selected.emit(fixture)
        await process_frame
        check(FileAccess.file_exists("user://original/employees.json"), "Database import")
        var catalog = JSON.parse_string(FileAccess.get_file_as_string("user://original/catalog.json"))
        check(catalog != null and catalog.Genres.size() == 18, "Genre catalog import")
        check(catalog != null and catalog.Audiences.size() == 7, "Audience catalog import")
        check(catalog != null and catalog.DevelopmentComputers.size() == 4, "Computer catalog import")
        check(catalog != null and catalog.Hardware.size() == 11, "Hardware catalog import")
        check(catalog != null and catalog.GameTypes.size() == 51, "Game type catalog import")
        check(not FileAccess.file_exists("user://original/GameDev2DX.exe"), "Executable excluded from cache")
    await press("NEW_GAME")
    if fixture.is_empty():
        check(nodes_of_type("LineEdit").is_empty(), "New game requires local original data")
        check(button("IMPORT_BUTTON") != null, "Missing original data opens importer")
        print("UI smoke checks complete (without original data); failures=", failures)
        quit(0 if failures == 0 else 1)
        return
    await press("CONFIRM")
    check(nodes_of_type("LineEdit").size() == 2, "Empty name rejected")
    var fields := nodes_of_type("LineEdit")
    if fields.size() == 2:
        fields[0].text = "Smoke Company"
        fields[1].text = "Smoke President"
    await press("CONFIRM")
    await press("NORMAL_MODE")
    for name in ["OfficeWindow", "CompanyWindow", "EmployeeWindow", "ProductWindow"]:
        check(main.find_child(name,true,false) != null,"Original desktop window: " + name)
    check(main.find_child("OriginalDesktopWallpaper",true,false) != null,"Imported original desktop wallpaper")
    check(not button("START_GAME_DEVELOPMENT").disabled, "Development loop is enabled")
    await press("DEVELOPMENT_PLANNING")
    check(main.find_child("PlanningHardware", true, false).item_count == 3, "Planning hardware availability")
    check(main.find_child("PlanningAudience", true, false).item_count == 7, "Planning audience catalog")
    check(main.find_child("PlanningQuote", true, false).text.contains("20,000,000"), "Initial planning cost")
    check(main.find_child("PlanningQuote", true, false).text.contains("400"), "Initial planning points")
    await press("PLANNING_NEW_PROPOSAL")
    var focus_total := 0
    for label in main.find_child("PlanningFocus", true, false).get_children():
        if label.text.ends_with("%"):
            focus_total += int(label.text.trim_suffix("%"))
    check(focus_total == 100, "Planning focus totals one hundred")
    await press("CANCEL")
    if not fixture.is_empty():
        var office_background = main.find_child("OriginalOfficeBackground", true, false)
        check(office_background != null and office_background.texture != null, "Original office background decoded")
        if office_background != null:
            if OS.get_environment("OPENGDS2_UI_EXPORT_OFFICE") == "1":
                office_background.texture.get_image().save_png("user://office-test.png")
            check(office_background.texture.get_width() == 466 and office_background.texture.get_height() == 290, "Original office bitmap dimensions")
            check(office_background.size == Vector2(466,290), "Office uses original-size pixels")
            check(office_background.get_global_rect().end.x <= main.get_viewport_rect().end.x, "Office fits the viewport")
            check(button("BACK_TO_TITLE").get_global_rect().end.x <= main.get_viewport_rect().end.x, "Toolbar fits the viewport")
        check(FileAccess.file_exists("user://original/Graphic/Embedded/OfficeSprites.bmp"), "Embedded sprite sheet imported locally")
        await press("HARDWARE_LICENSES")
        var license_buttons: Array[Node] = []
        for node in nodes_of_type("Button"):
            if node.text == tr("LICENSE_BUY"):
                license_buttons.append(node)
        check(license_buttons.size() == 2, "Only released, unlicensed hardware is offered")
        if license_buttons.size() == 2:
            license_buttons[1].pressed.emit()
            await process_frame
        await press("CANCEL")
        await press("DEVELOPMENT_COMPUTERS")
        var buy_buttons: Array[Node] = []
        for node in nodes_of_type("Button"):
            if node.text == tr("COMPUTER_BUY"):
                buy_buttons.append(node)
        check(buy_buttons.size() == 3, "Opening computer already owned")
        if buy_buttons.size() == 3:
            buy_buttons[0].pressed.emit()
            await process_frame
        await press("CANCEL")
        await press("HIRE_EMPLOYEES")
        var portraits = main.find_children("EmployeePortrait", "TextureRect", true, false)
        check(portraits.size() == 5, "Five imported applicant portraits")
        for portrait in portraits:
            check(portrait.texture.get_width() == 64 and portrait.texture.get_height() == 64, "Original portrait dimensions")
        var hire_buttons: Array[Node] = []
        for node in nodes_of_type("Button"):
            if node.text == tr("HIRE_CONFIRM"):
                hire_buttons.append(node)
        check(hire_buttons.size() == 5, "Five applicant cards")
        if hire_buttons.size() == 5:
            hire_buttons[0].pressed.emit()
            await process_frame
            # Exercise the actual Godot clock and compositor, rather than only
            # testing a domain update detached from the running scene.
            await create_timer(4.5).timeout
    if OS.get_environment("OPENGDS2_UI_TEST_SAVE") == "1":
        check(not OS.get_environment("XDG_DATA_HOME").is_empty(), "Save tests require isolated XDG_DATA_HOME")
        if not OS.get_environment("XDG_DATA_HOME").is_empty():
            await press("SAVE_GAME")
            var saved = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
            check(saved != null, "Modern save written")
            if saved != null:
                check(saved.State.Company.Name == "Smoke Company", "Save company")
                check(saved.State.Company.Fame == 12 and saved.State.Company.Rank == 0, "Opening recognition/rank")
                check(saved.State.Year == 1 and saved.State.Month == 4 and saved.State.Week == 1, "Opening date")
                check(saved.State.DevelopmentProjects.is_empty(), "Planning preview does not start a project")
                check(saved.State.Employees.size() == (1 if not fixture.is_empty() else 0), "Hired staff persisted")
                if not fixture.is_empty() and saved.State.Employees.size() == 1:
                    check(saved.State.Employees[0].Office != null and saved.State.Employees[0].Office.MovementState == 6, "Hired employee walks to lounge and rests in Godot")
                check(saved.State.DevelopmentComputerIndex == (1 if not fixture.is_empty() else 0), "Computer upgrade persisted")
                check(saved.State.HardwareLicenses.size() == (2 if not fixture.is_empty() else 1), "Hardware license persisted")
            await press("LOAD_GAME")
            if OS.get_environment("OPENGDS2_UI_EXPORT_OFFICE") == "1":
                # Dummy headless rendering does not expose texture.Update pixel
                # changes through get_image. Reload forces fresh composition.
                main.find_child("OriginalOfficeBackground", true, false).texture.get_image().save_png("user://office-hired-test.png")
    await press("STAFF")
    check(main.find_children("EmployeePortrait", "TextureRect", true, false).size() == 1, "Imported staff portrait")
    await press("ASSIGN_DESK")
    var desk = main.find_child("AssignDesk6", true, false)
    check(desk != null and not desk.disabled, "Empty development desk can be selected")
    if desk != null:
        desk.pressed.emit()
        await process_frame
        await create_timer(8.0).timeout
        await press("SAVE_GAME")
        var seated = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
        if seated != null:
            var workspace = seated.State.Employees[0].Office
            check(workspace.Action == 6 and workspace.SeatedAction == 6 and workspace.WorkEngaged, "Staff reaches development desk in Godot")
            check(workspace.MovementState == 0 and workspace.LoungeSeat == null, "Desk assignment releases lounge reservation")
        await press("LOAD_GAME")
        if OS.get_environment("OPENGDS2_UI_EXPORT_OFFICE") == "1":
            main.find_child("OriginalOfficeBackground", true, false).texture.get_image().save_png("user://office-desk-test.png")
        await press("STAFF")
        await press("ASSIGN_DESK")
        check(main.find_child("AssignDesk6", true, false).disabled, "Occupied desk cannot be assigned twice")
        await press("CANCEL")
        await press("TAKE_BREAK")
        await create_timer(8.0).timeout
        await press("SAVE_GAME")
        var resting = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
        if resting != null:
            var workspace = resting.State.Employees[0].Office
            check(workspace.MovementState == 6 and not workspace.WorkEngaged and workspace.SeatedAction == 0, "Staff returns from desk to lounge")
        await press("STAFF")
    await press("CANCEL")
    await press("CALENDAR_INSPECTOR")
    await press("NEXT_WEEK")
    await press("CANCEL")
    print("UI smoke checks complete; failures=", failures)
    quit(0 if failures == 0 else 1)
