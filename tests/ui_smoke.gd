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
    root.size = Vector2i(1280,720)
    main = load("res://scenes/Main.tscn").instantiate()
    root.add_child(main)
    await process_frame
    check(button("NEW_GAME") != null, "Title C# initialization")
    await press("SETTINGS")
    if DirAccess.get_files_at("user://music").is_empty():
        var music = main.find_child("MusicPlayer",true,false)
        check(music.stream != null and music.stream.resource_path.ends_with("title-theme.ogg"),"Title selects dedicated community theme")
        check(music.stream.loop,"Title theme loops")
        check(not button("NEXT_MUSIC_TRACK").disabled,"Bundled playlist enables next track")
        var durations := [182.0,144.0,64.96,60.6,65.16,151.16,132.28,131.16]
        for index in range(durations.size()):
            var track = load("res://audio/community/office-%02d.ogg" % (index+1))
            check(track != null and abs(track.get_length()-durations[index]) < 0.05,"Bundled music duration %d" % (index+1))
    await press("日本語")
    check(TranslationServer.get_locale() == "ja", "Japanese setting")
    check(tr("DEVELOPMENT_COMPUTERS") == "開発パソコン", "Japanese computer translation loaded")
    var preferences := ConfigFile.new()
    check(preferences.load("user://interface.cfg") == OK and preferences.get_value("language","locale") == "ja","Language preference is saved outside simulation state")
    main.queue_free()
    await process_frame
    main = load("res://scenes/Main.tscn").instantiate()
    root.add_child(main)
    await process_frame
    check(TranslationServer.get_locale() == "ja","Japanese language survives restart")
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
        check(FileAccess.file_exists("user://original/dialogue.json"),"Original Japanese dialogue imported locally")
        var catalog = JSON.parse_string(FileAccess.get_file_as_string("user://original/catalog.json"))
        check(catalog != null and catalog.Genres.size() == 18, "Genre catalog import")
        check(catalog != null and catalog.Audiences.size() == 7, "Audience catalog import")
        check(catalog != null and catalog.DevelopmentComputers.size() == 4, "Computer catalog import")
        check(catalog != null and catalog.Hardware.size() == 11, "Hardware catalog import")
        check(catalog != null and catalog.GameTypes.size() == 51, "Game type catalog import")
        check(not FileAccess.file_exists("user://original/GameDev2DX.exe"), "Executable excluded from cache")
        for index in range(16):
            check(FileAccess.file_exists("user://original/Graphic/Embedded/Toolbar%d.bmp" % index),"Original toolbar bitmap imported locally: %d" % index)
        for index in range(15):
            check(FileAccess.file_exists("user://original/Graphic/Embedded/Hardware%d.bmp" % index),"Original hardware picture imported locally: %d" % index)

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
    check(main.find_child("ToolbarSAVE_GAME",true,false).icon != null,"Original toolbar save icon is loaded")
    for mapping in [["SAVE_GAME",15],["LOAD_GAME",14],["STAFF",7],["HARDWARE_LICENSES",9],["SALES",6],["COMPANY_ACTIVITIES",8]]:
        var expected = Image.load_from_file("user://original/Graphic/Embedded/Toolbar%d.bmp" % mapping[1])
        var displayed = main.find_child("Toolbar" + mapping[0],true,false).icon.get_image()
        check(displayed.get_data() == expected.get_data(),"Toolbar action uses the intended local bitmap: " + mapping[0])
    check(not button("START_GAME_DEVELOPMENT").disabled, "Development loop is enabled")
    await press("DEVELOPMENT_PLANNING")
    check(main.find_child("PlanningHardware", true, false).get_root().get_child_count() == 3, "Planning hardware availability")
    check(main.find_child("PlanningAudience", true, false).get_root().get_child_count() == 7, "Planning audience catalog")
    for dropdown_name in ["PlanningHardware", "PlanningAudience"]:
        var dropdown = main.find_child(dropdown_name,true,false)
        var category = "HARDWARE" if dropdown_name == "PlanningHardware" else "AUDIENCE"
        for index in range(dropdown.get_root().get_child_count()):
            var row = dropdown.get_root().get_child(index)
            check(row.get_text(0).begins_with(tr("CATALOG_%s_%d" % [category,row.get_metadata(0)])),"Original catalog label translated: " + dropdown_name)
    check(main.find_child("PlanningQuoteWindow",true,false).size.x <= 450,"Quote uses a compact child window")
    check(main.find_child("PlanningTitle",true,false) == null and main.find_child("PlanningFocus",true,false) == null,"Quote precedes title/focus proposal controls")
    var hardware_rows = main.find_child("PlanningHardware",true,false).get_root()
    check(hardware_rows.get_child(0).is_selectable(0) and not hardware_rows.get_child(1).is_selectable(0) and not hardware_rows.get_child(2).is_selectable(0),"Available unlicensed hardware is grey and not selectable")
    check(hardware_rows.get_child(0).get_text(3) == "150,000","Quote shipment display uses floor(original market) times 10000")
    check(main.find_child("PlanningQuote", true, false).text.contains("20,000,000"), "Initial planning cost")
    check(main.find_child("PlanningQuote", true, false).text.contains("400"), "Initial planning points")
    var planning_picture = main.find_child("PlanningHardwarePicture",true,false)
    var expected_picture = Image.load_from_file(ProjectSettings.globalize_path("user://original/Graphic/Embedded/Hardware0.bmp"))
    expected_picture.convert(Image.FORMAT_RGB8)
    check(planning_picture.texture != null and planning_picture.texture.get_width() == 64 and planning_picture.texture.get_height() == 64,"Quote shows original 64-pixel hardware picture")
    if planning_picture.texture != null:
        var displayed_picture = planning_picture.texture.get_image()
        displayed_picture.convert(Image.FORMAT_RGB8)
        check(displayed_picture.get_data() == expected_picture.get_data(),"Opening quote picture matches imported source index zero")

    await press("PLANNING_MEETING")
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
            for room in range(2):
                var scale = 1.0 if room == 0 else 2.0
                office_background.size = Vector2(466,290) if room == 0 else Vector2(932,600)
                var event = InputEventMouseButton.new()
                event.button_index = MOUSE_BUTTON_LEFT
                event.pressed = true
                event.position = (Vector2(192,148) if room == 0 else Vector2(288,196))*scale + Vector2(0,0 if room == 0 else 10)
                office_background.gui_input.emit(event)
                await process_frame
                check(main.find_child("DevelopmentRoomActions",true,false) != null,"Desk interior opens room action chooser: %d" % room)
                check(main.find_child("RoomSequel",true,false).disabled and main.find_child("RoomManga",true,false).disabled,"Unreconstructed workflows are visibly unavailable")
                main.find_child("RoomOriginalGame",true,false).pressed.emit()
                await process_frame
                var selected_room = main.find_child("PlanningRoom",true,false)
                check(selected_room.get_selected_id() == room and not selected_room.visible,"Office-selected room is carried into planning: %d" % room)
                await press("CANCEL")
            office_background.size = Vector2(466,290)
            var tenant_pixels = [Vector2(66,199),Vector2(146,239),Vector2(401,239)]
            for slot in range(3):
                var event = InputEventMouseButton.new()
                event.button_index = MOUSE_BUTTON_LEFT
                event.pressed = true
                event.position = tenant_pixels[slot]
                office_background.gui_input.emit(event)
                await process_frame
                check(main.find_child("TenantFacilityWindow",true,false) != null,"Tenant plot opens focused facility window: %d" % slot)
                check(main.find_child("OpenSchool%d" % slot,true,false) != null,"Clicked plot carries matching school slot: %d" % slot)
                var other_slot = (slot+1)%3
                check(main.find_child("OpenSchool%d" % other_slot,true,false) == null,"Focused facility chooser excludes other slots")
                await press("BACK_TO_OFFICE")
            await press("SAVE_GAME")
            var after_room_clicks = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
            check(after_room_clicks.State.DevelopmentProjects.is_empty() and after_room_clicks.State.Money == 450000000,"Office action previews do not start work or spend funds")
            main.find_child("MENU_HELP",true,false).get_popup().id_pressed.emit(2)
            await process_frame
            var sales_ranking = main.find_child("SalesRankingTable",true,false)
            check(sales_ranking != null and sales_ranking.get_column_title(2) == tr("UNITS_SOLD") and sales_ranking.get_column_title(3) == tr("COMPANY_NAME"),"Sales rankings follow original units-before-company columns")
            await press("BACK_TO_OFFICE")
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
        await press("DEVELOPMENT_PLANNING")
        var selected_hardware = main.find_child("PlanningHardware",true,false)
        var owned_row = selected_hardware.get_root().get_child(2)
        check(owned_row.is_selectable(0),"Purchased license enables quote selection")
        owned_row.select(0)
        await process_frame
        var selected_picture = main.find_child("PlanningHardwarePicture",true,false)
        var selected_expected = Image.load_from_file(ProjectSettings.globalize_path("user://original/Graphic/Embedded/Hardware%d.bmp" % owned_row.get_metadata(0)))
        selected_expected.convert(Image.FORMAT_RGB8)
        var displayed_selected = selected_picture.texture.get_image()
        displayed_selected.convert(Image.FORMAT_RGB8)
        check(displayed_selected.get_data() == selected_expected.get_data(),"Quote selection switches to matching local hardware picture")
        await press("CANCEL")
        await press("HIRE_EMPLOYEES")
        var portraits = main.find_children("EmployeePortrait", "TextureRect", true, false)
        check(portraits.size() == 1, "Selected applicant has one imported portrait")
        for portrait in portraits:
            check(portrait.texture.get_width() == 64 and portrait.texture.get_height() == 64, "Original portrait dimensions")
        var hire_buttons: Array[Node] = []
        for node in nodes_of_type("Button"):
            if node.text == tr("HIRE_CONFIRM"):
                hire_buttons.append(node)
        var recruitment = main.find_child("RecruitmentTable",true,false)
        check(recruitment != null and recruitment.get_root().get_child_count() == 5,"Five candidates in compact recruitment table")
        check(hire_buttons.size() == 1, "One selected-candidate hire action")
        if recruitment != null:
            for row_index in range(recruitment.get_root().get_child_count()):
                var display_name: String = recruitment.get_root().get_child(row_index).get_text(0)
                check(display_name.to_ascii_buffer().get_string_from_ascii() == display_name,"Recruitment names have English community readings")

        if recruitment != null:
            await press("SAVE_GAME")
            var before_selection = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
            recruitment.get_root().get_child(1).select(0)
            await process_frame
            await press("SAVE_GAME")
            var after_selection = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
            check(before_selection.State.Money == after_selection.State.Money,"Candidate selection never charges funds")
            check(before_selection.State.Recruitment.Candidates == after_selection.State.Recruitment.Candidates,"Candidate selection never regenerates roster")
            check(main.find_child("RecruitmentAbilityChart",true,false) != null,"Selected candidate chart shown")
            recruitment.get_root().get_child(0).select(0)
            await process_frame
        if hire_buttons.size() == 1:
            hire_buttons[0].pressed.emit()
            await process_frame
            var conversation = main.find_child("ConversationWindow",true,false)
            check(conversation != null and conversation.size.x <= 350 and conversation.size.y <= 165,"Hire opens compact conversation")
            check(main.find_child("ConversationNameStrip",true,false) != null and main.find_child("ConversationPortrait",true,false) != null,"Conversation has name strip and imported portrait")
            await press("SAVE_GAME")
            var greeting_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
            check(greeting_save.State.PendingHireGreeting != null,"Pending greeting survives save")
            await create_timer(0.2).timeout
            await press("SAVE_GAME")
            var greeting_wait = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
            check(greeting_wait.State.CompanyActivityTicks == greeting_save.State.CompanyActivityTicks and greeting_wait.State.Money == greeting_save.State.Money,"Greeting suspends time and never repeats hire fee")
            await press("LOAD_GAME")
            check(main.find_child("ConversationWindow",true,false) != null,"Loading resumes pending notification")
            var message = main.find_child("ConversationMessage",true,false)
            if message != null:
                var click := InputEventMouseButton.new()
                click.button_index=MOUSE_BUTTON_LEFT;click.pressed=true
                message.gui_input.emit(click)
                await process_frame
            check(main.find_child("ConversationWindow",true,false) == null,"Clicking message dismisses conversation")
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
            if saved != null and saved.State.Employees.size() == 1:
                # Pause movement before testing actual office-input binding.
                var pause = main.find_child("TogglePause",true,false)
                pause.pressed.emit()
                await press("SAVE_GAME")
                var frozen = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
                var office_image = main.find_child("OriginalOfficeBackground",true,false)
                var workspace = frozen.State.Employees[0].Office
                var pixel = Vector2(workspace.X+workspace.OffsetX+8,workspace.Y+workspace.OffsetY+16)
                var event := InputEventMouseButton.new()
                event.button_index = MOUSE_BUTTON_RIGHT
                event.pressed = true
                event.position = pixel
                office_image.gui_input.emit(event)
                check(main.find_child("SelectedEmployee",true,false) == null,"Unreconstructed right-click assignment remains unavailable")
                event.button_index = MOUSE_BUTTON_LEFT
                event.position = Vector2.ZERO
                office_image.gui_input.emit(event)
                check(main.find_child("SelectedEmployee",true,false) == null,"Blank office pixels do not open employee controls")
                # KeepAspectCentered input must account for letterboxing and scale.
                office_image.size = Vector2(932,600)
                event.position = pixel*2+Vector2(0,10)
                office_image.gui_input.emit(event)
                await process_frame
                var selected = main.find_child("SelectedEmployee",true,false)
                check(selected != null and selected.get_meta("employee_source") == saved.State.Employees[0].SourceIndex,"Office click opens the hit employee at scaled coordinates")
                var arrow = main.find_child("EmployeeSelectionArrow",true,false)
                check(arrow != null and arrow.size == Vector2(16,20),"Employee selection arrow scales with imported office pixels")
                var expected_arrow = Vector2(workspace.X+workspace.OffsetX+4,workspace.Y+workspace.OffsetY-12)*2+Vector2(0,10)
                check(arrow != null and arrow.position.distance_to(expected_arrow) < 0.01,"Arrow sits above the selected employee with letterboxing")
                var details = main.find_child("EmployeeDetailsWindow",true,false)
                check(details != null and details.size.x <= 240 and details.size.y <= 410,"Imported employee portrait uses compact pane")
                check(main.find_child("ManagementWindow",true,false) == null,"Office selection keeps office unobstructed")
                check(main.find_child("EmployeeAbilityChart",true,false) != null,"Compact imported employee pane has five-axis ability chart")
                await press("SAVE_GAME")
                var focused_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
                check(focused_save.State.CompanyActivityTicks == frozen.State.CompanyActivityTicks,"Explicit pause stays active while inspecting an employee")
                check(focused_save.State.Money == frozen.State.Money and focused_save.State.Employees[0].Experience == frozen.State.Employees[0].Experience,"Selecting a visible employee does not charge funds or progression")
                await press("CANCEL")
                check(main.find_child("EmployeeSelectionArrow",true,false) == null,"Closing details removes selected employee arrow")
                office_image.size = Vector2(466,290)
                pause.pressed.emit()
            if OS.get_environment("OPENGDS2_UI_EXPORT_OFFICE") == "1":
                # Dummy headless rendering does not expose texture.Update pixel
                # changes through get_image. Reload forces fresh composition.
                main.find_child("OriginalOfficeBackground", true, false).texture.get_image().save_png("user://office-hired-test.png")
    await press("STAFF")
    check(main.find_children("EmployeePortrait", "TextureRect", true, false).size() == 1, "Imported staff portrait")
    await press("ASSIGN_DESK")
    var assignment = main.find_child("DeskAssignmentWindow",true,false)
    check(assignment != null and assignment.size.x <= 430 and assignment.size.y <= 370,"Desk assignment stays compact")
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
