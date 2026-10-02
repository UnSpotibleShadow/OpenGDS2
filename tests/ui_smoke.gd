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
        if node.text == tr(key):
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
    await press("日本語")
    check(TranslationServer.get_locale() == "ja", "Japanese setting")
    check(tr("DEVELOPMENT_COMPUTERS") == "開発パソコン", "Japanese computer translation loaded")
    await press("BACK_TO_TITLE")
    await press("SETTINGS")
    await press("English")
    check(tr("HARDWARE_LICENSES") == "Hardware Licenses", "English license translation loaded")
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
    check(button("START_GAME_DEVELOPMENT").disabled, "Unknown simulation remains disabled")
    if not fixture.is_empty():
        var office_background = main.find_child("OriginalOfficeBackground", true, false)
        check(office_background != null and office_background.texture != null, "Original office background decoded")
        if office_background != null:
            check(office_background.texture.get_width() == 466 and office_background.texture.get_height() == 290, "Original office bitmap dimensions")
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
        var hire_buttons: Array[Node] = []
        for node in nodes_of_type("Button"):
            if node.text == tr("HIRE_CONFIRM"):
                hire_buttons.append(node)
        check(hire_buttons.size() == 5, "Five applicant cards")
        if hire_buttons.size() == 5:
            hire_buttons[0].pressed.emit()
            await process_frame
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
                check(saved.State.Employees.size() == (1 if not fixture.is_empty() else 0), "Hired staff persisted")
                check(saved.State.DevelopmentComputerIndex == (1 if not fixture.is_empty() else 0), "Computer upgrade persisted")
                check(saved.State.HardwareLicenses.size() == (2 if not fixture.is_empty() else 1), "Hardware license persisted")
            await press("LOAD_GAME")
    await press("STAFF")
    await press("CANCEL")
    await press("CALENDAR_INSPECTOR")
    await press("NEXT_WEEK")
    await press("CANCEL")
    print("UI smoke checks complete; failures=", failures)
    quit(0 if failures == 0 else 1)
