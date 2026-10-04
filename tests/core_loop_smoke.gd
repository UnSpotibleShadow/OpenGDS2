extends SceneTree
# Author-created synthetic data only. Always use disposable XDG_DATA_HOME.
var main: Node
var failures := 0

func check(condition: bool, message: String) -> void:
    if not condition:
        failures += 1
        push_error("FAIL CORE UI: " + message)

func press(key: String) -> void:
    var found := false
    for button in main.find_children("*", "Button", true, false):
        if button.text == tr(key) and button.is_visible_in_tree():
            check(not button.disabled, "Enabled: " + key)
            if not button.disabled:
                button.pressed.emit()
            found = true
            break
    check(found, "Button exists: " + key)
    await process_frame

func press_node(name: String) -> void:
    var button = main.find_child(name, true, false)
    check(button != null and not button.disabled, name)
    if button != null and not button.disabled:
        button.pressed.emit()
    await process_frame

func write_json(path: String, value: Variant) -> void:
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(path.get_base_dir()))
    var file := FileAccess.open(path, FileAccess.WRITE)
    file.store_string(JSON.stringify(normalize_numbers(value)))

func normalize_numbers(value: Variant) -> Variant:
    # Godot parses JSON numbers as doubles; System.Text.Json integer fields
    # require integer tokens when this fixture is written back.
    if value is Dictionary:
        for key in value:
            value[key] = normalize_numbers(value[key])
    elif value is Array:
        for index in range(value.size()):
            value[index] = normalize_numbers(value[index])
    elif typeof(value) == TYPE_FLOAT and value == floor(value):
        return int(value)
    return value

func await_node(name: String) -> bool:
    for frame in range(15000):
        if main.find_child(name, true, false) != null:
            return true
        await process_frame
    check(false, "Timed out waiting for " + name)
    return false

func _initialize() -> void:
    call_deferred("run")

func create_music_fixture() -> void:
    # Community-authored silent WAVs exercise local loading in exported builds.
    # No supplied recordings or original-game audio are required by public CI.
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("user://music"))
    for index in range(8):
        var track := AudioStreamWAV.new()
        track.format = AudioStreamWAV.FORMAT_16_BITS
        track.mix_rate = 8000
        var samples := PackedByteArray()
        samples.resize(16000 * (index + 1))
        track.data = samples
        check(track.save_to_wav("user://music/office-%02d.wav" % (index+1)) == OK,"Write local music fixture")

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("core_loop_smoke requires disposable XDG_DATA_HOME")
        quit(1)
        return
    var raw_type := {"1378":5,"1308":10,"13B0":1,"12EC":3}
    var catalog := {
        "SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6",
        "Hardware":[{"SourceIndex":0,"Name":"Synthetic Hardware","RawMembers":{"0F00":8,"0FE0":20,"0FFC":10,"0EC8":0,"1050":40,"0F54":15,"10A4":0,"10C0":0},
            "ProductionBands":[{"SizeThreshold":16,"UnitCost":300},{"SizeThreshold":32,"UnitCost":700},{"SizeThreshold":48,"UnitCost":1200},{"SizeThreshold":64,"UnitCost":1800}]}],
        "Audiences":[{"SourceIndex":0,"Name":"Synthetic Audience","RawMembers":{"14A8":1,"1454":10,"148C":20}}],
        "DevelopmentComputers":[{"SourceIndex":0,"Name":"Synthetic Computer","BitWidth":8}],
        "GameTypes":[{"SourceIndex":0,"Name":"Synthetic Type A","RawMembers":raw_type,"RawTable13CC":[]},
            {"SourceIndex":1,"Name":"Synthetic Type B","RawMembers":raw_type,"RawTable13CC":[]}],
        "Technologies":[{"SourceIndex":0,"Identifier":0,"Name":"Synthetic Technology","PurchaseFee":50,"InitialOwnershipLevel":0,"RequiredTrainingWork":[40,60,80],"ExperienceCosts":[10,20,30],"RawTable158C":[1,2,3],"RawTable1568":[1,2,3]}],
        "Genres":[{"SourceIndex":0,"Name":"Synthetic Genre","RawMembers":{"1224":7,"1208":10,"1240":1,"125C":0}}]
    }
    # Genre helper validates its fifteen-column table even for broad audience.
    for type in catalog.GameTypes:
        for column in range(15):
            type.RawTable13CC.append(1)
    var roster := {}
    for cell in range(65):
        roster[str(cell)] = 0
    var worker := {"RawParameter34":1,"TechnologyLevels":{"0":0},"Name":"Synthetic Worker","SourceIndex":0,"PortraitId":1,"Level":1,"Experience":0,"Stress":0,
        "Fun":80,"Novelty":80,"Graphics":80,"Music":80,"Accuracy":80,"DevelopmentSpeed":80,"Inspiration":80,
        "Office":{"Action":6,"SeatedAction":6,"WorkEngaged":true,"MovementState":0,"Facing":1,"Frame":13}}
    var state := {"TechnologyOwnershipLevels":{"0":0},"Money":10000000,"Year":1,"Month":4,"Week":1,"DevelopmentComputerIndex":0,"HardwareLicenses":[0],"AvailableHardware":[0],
        "Company":{"Name":"Synthetic UI Company","PresidentName":"Tester","Fame":12,"Rank":0,"ProductionCapacity":50,"OriginalMember90":23,"ActivityCost":5000000,"WeeklyNet":0,"MonthlyNet":0,"YearlyNet":0},
        "Employees":[worker],"OriginalAccuracyCells":{"99":0},"OriginalTypeRosterMap":roster,"SalesFee1":1500,"SalesFee2":1500,"CompanyActivityTicks":0,"CompanyActivityPerformed":false}
    write_json("user://original/catalog.json",catalog)
    write_json("user://saves/slot1.json",{"Format":"OpenGDS2","SchemaVersion":1,"State":state})
    create_music_fixture()
    main = load("res://scenes/Main.tscn").instantiate()
    root.add_child(main)
    await process_frame
    await press("LOAD_GAME")
    var toolbar = main.find_child("DesktopToolbar",true,false)
    for control in toolbar.get_children():
        if control is Button:
            check(control.icon != null,"Toolbar action has an image: " + control.text)
            check(control.tooltip_text == control.text,"Toolbar action has a localized tooltip: " + control.text)
            if control.icon != null:
                check(control.icon.get_size() == Vector2(23,22),"Toolbar image retains native dimensions")
    var save_action = main.find_child("ToolbarSAVE_GAME",true,false)
    save_action.mouse_entered.emit()
    check(main.find_child("OfficeStatus",true,false).text == tr("SAVE_GAME"),"Toolbar hover explains its action in the status bar")
    save_action.mouse_exited.emit()
    for name in ["OfficeWindow", "CompanyWindow", "EmployeeWindow", "ProductWindow"]:
        var window = main.find_child(name,true,false)
        check(window != null and window.visible,"Simultaneous desktop window: " + name)
        check(window.get_global_rect().end.x <= main.get_viewport_rect().end.x,"Desktop window fits viewport: " + name)
    var office = main.find_child("OfficeWindow",true,false)
    var staff_table = main.find_child("DesktopEmployeeTable",true,false)
    check(staff_table.get_root().get_child_count() == 1,"Desktop table shows loaded employee")
    staff_table.get_root().get_first_child().select(0)
    staff_table.item_activated.emit()
    await process_frame
    var selected_employee = main.find_child("SelectedEmployee",true,false)
    check(selected_employee != null and selected_employee.get_meta("employee_source") == 0,"Activating an employee row opens that employee's controls")
    check(main.find_child("DismissEmployee0",true,false) != null,"Selected employee supports existing management actions")
    await press("CANCEL")
    var employee_window = main.find_child("EmployeeWindow",true,false)
    var original_position = employee_window.position
    employee_window.find_child("MinimizeWindow",true,false).pressed.emit()
    await process_frame
    check(not staff_table.is_visible_in_tree(),"Employee window minimizes")
    employee_window.find_child("MinimizeWindow",true,false).pressed.emit()
    await process_frame
    check(staff_table.is_visible_in_tree(),"Employee window restores")
    employee_window.find_child("MaximizeWindow",true,false).pressed.emit()
    await process_frame
    check(employee_window.position == Vector2.ZERO,"Employee window maximizes within desktop")
    employee_window.find_child("MaximizeWindow",true,false).pressed.emit()
    await process_frame
    check(employee_window.position == original_position,"Employee window restores original position")
    employee_window.find_child("CloseWindow",true,false).pressed.emit()
    check(not employee_window.visible,"Employee child window closes")
    main.find_child("MENU_WINDOWS",true,false).get_popup().id_pressed.emit(2)
    await process_frame
    check(employee_window.visible,"Window menu reopens employee list")
    await press("START_GAME_DEVELOPMENT")
    var planning_hardware = main.find_child("PlanningHardware",true,false)
    var planning_audience = main.find_child("PlanningAudience",true,false)
    check(planning_hardware.get_item_text(0).begins_with(tr("CATALOG_HARDWARE_0")),"English platform label uses the imported source index")
    check(planning_audience.get_item_text(0) == tr("CATALOG_AUDIENCE_0"),"English audience label is translated")
    # Changing language changes labels, not the imported data or identifiers.
    await press("SETTINGS")
    await press("日本語")
    await press("BACK_TO_OFFICE")
    await press("START_GAME_DEVELOPMENT")
    check(main.find_child("PlanningHardware",true,false).get_item_text(0).begins_with("Synthetic Hardware"),"Japanese mode uses the untouched local platform name")
    check(main.find_child("PlanningAudience",true,false).get_item_text(0) == "Synthetic Audience","Japanese mode uses the untouched local audience name")
    await press("SETTINGS")
    await press("English")
    await press("BACK_TO_OFFICE")
    await press("START_GAME_DEVELOPMENT")
    var unchanged_catalog = JSON.parse_string(FileAccess.get_file_as_string("user://original/catalog.json"))
    check(unchanged_catalog.Hardware[0].Name == "Synthetic Hardware" and unchanged_catalog.Audiences[0].Name == "Synthetic Audience","Localization never rewrites imported names")
    office = main.find_child("OfficeWindow",true,false)
    check(main.find_child("OfficeWindow",true,false) == office and office.visible,"Planning retains the office window")
    check(not main.find_child("AcceptPlan",true,false).disabled,"Planning can begin")
    main.find_child("PlanningTitle",true,false).text = "Synthetic UI Game"
    await press_node("AcceptPlan")
    check(main.find_child("DevelopmentProgress",true,false) != null,"Live development screen")
    Engine.time_scale = 100
    if not await await_node("SelectType0"):
        quit(1)
        return
    await press("SAVE_GAME")
    var checkpoint = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    for period in ["WeeklyNet","MonthlyNet","YearlyNet"]:
        check(checkpoint.State.Company[period] == checkpoint.State.Money-10000000,"Planning expense feeds financial period: " + period)
    var financial_row = main.find_child("CompanyMetricWEEKLY_NET",true,false)
    check(financial_row != null and financial_row.text != "?","Company window displays recorded weekly finances")

    check(checkpoint.State.DevelopmentProjects[0].PendingDecision == 0,"Type checkpoint persisted")
    var offered_types = checkpoint.State.DevelopmentProjects[0].TypeCandidates
    for index in range(offered_types.size()):
        check(main.find_child("SelectType%d" % index,true,false).text.begins_with(tr("CATALOG_TYPE_%d" % offered_types[index])),"Each randomized type label matches its saved source ID")
    # Reload checkpoint through the actual office controls before selecting.
    await press("LOAD_GAME")
    await await_node("SelectType0")
    await press_node("SelectType0")
    if not await await_node("SelectGenre0"):
        quit(1)
        return
    check(main.find_child("SelectGenre0",true,false).text.begins_with(tr("CATALOG_GENRE_0")),"Genre selection uses English catalog labels")
    await press_node("SelectGenre0")
    var ready := false
    for frame in range(15000):
        for button in main.find_children("*", "Button", true, false):
            if button.text == tr("RELEASE_GAME"):
                ready = true
                break
        if ready:
            break
        await process_frame
    check(ready,"Development reaches completion")
    if not ready:
        quit(1)
        return
    await press("CONTINUE_DEBUGGING")
    var debugged := false
    for frame in range(15000):
        var report = main.find_child("DevelopmentReport",true,false)
        if report != null and report.text.contains(tr("BUGS") + ": 0"):
            debugged = true
            break
        await process_frame
    check(debugged,"Stage-three work removes bugs")
    if not debugged:
        quit(1)
        return
    await press("RELEASE_GAME")
    check(main.find_child("ReleasePrice",true,false).value == 5800,"Original default price")
    check(main.find_child("ReleaseCopies",true,false).value == 5000,"Original default production")
    await press_node("ConfirmRelease")
    check(main.find_child("ReviewScore3",true,false) != null,"Release opens four magazine reviews")
    await press("SAVE_GAME")
    var review_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(review_save.State.ReleasedGames[0].ReviewPending,"Pending reviews saved")
    check(review_save.State.ProductionPerformedThisWeek,"First shipment sets the production flag")
    # Isolate the budget boundary from variable development duration.
    review_save.State.CompanyActivityTicks = 0
    # Separate consumer investigations from this baseline sales/chart check.
    review_save.State.ReleasedGames[0].Recall = {"OriginalState":1,"WeeksRemaining":0,"PendingEvent":null,"OutcomeRoll":null,"RefundAmount":null}
    write_json("user://saves/slot1.json",review_save)
    Engine.time_scale = 1
    await press("LOAD_GAME")
    check(main.find_child("ReviewScore3",true,false) != null,"Load restores review ceremony")
    await press_node("AcknowledgeReviews")
    check(main.find_child("SalesProduct0",true,false) != null,"Review acknowledgement opens sales screen")
    check(main.find_child("ProduceProduct0",true,false).disabled,"Reprint waits for the next budget week")
    Engine.time_scale = 100
    for frame in range(100):
        await process_frame
    Engine.time_scale = 1
    await press("BACK_TO_OFFICE")
    await press("SAVE_GAME")
    var released = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(released.State.DevelopmentProjects.is_empty(),"Release frees room")
    check(released.State.ReleasedGames.size() == 1,"One product released")
    var product = released.State.ReleasedGames[0]
    check(product.UnitsSold > 0 and product.Stock < 5000,"Sales consume stock")
    check(product.NetSalesReceipts == product.UnitsSold * 2800,"Revenue uses original per-copy fees")
    await press("LOAD_GAME")
    await press("SALES")
    await press_node("SalesHistoryProduct0")
    check(main.find_child("WeeklySalesChart",true,false) != null,"Original thirteen-cell sales chart")
    check(main.find_child("CurrentWeeklySales",true,false) != null,"Current-week sales label")
    await press("BACK_TO_SALES")
    await press_node("ProduceProduct0")
    check(main.find_child("ProductionCopies",true,false) != null,"Reprint controls")
    main.find_child("ProductionCopies",true,false).value = 1000
    await press_node("ConfirmProduction")
    check(main.find_child("ProduceProduct0",true,false).disabled,"Reprint consumes the shared weekly allowance")
    await press("BACK_TO_OFFICE")
    await press("SAVE_GAME")
    var reprinted = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(reprinted.State.ReleasedGames[0].ProductionSpending == product.ProductionSpending + 500000,"Reprint costs original band plus packaging")
    check(reprinted.State.ProductionPerformedThisWeek,"Production allowance persists in saves")
    check(reprinted.State.ReleasedGames[0].WeeklySalesHistory.size() == 13,"Sales history persists all thirteen cells")
    await press("START_GAME_DEVELOPMENT")
    main.find_child("PlanningTitle",true,false).text = "Synthetic Second Game"
    await press_node("AcceptPlan")
    check(main.find_child("DevelopmentProgress",true,false) != null,"Room accepts another project")
    await press("BACK_TO_OFFICE")
    await press("SAVE_GAME")
    # Synthetic boundary fixture drives the real modal/event UI without adding
    # game cheats or waiting for hundreds of thousands of physical copies.
    var milestone_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    var milestone_product = milestone_save.State.ReleasedGames[0]
    milestone_product.UnitsSold = 1000000
    milestone_product.Stock = 1000
    milestone_product.Demand = 100
    milestone_product.Quality = {"Fun":100,"Novelty":100,"Graphics":100,"Music":100}
    milestone_product.Bugs = 0
    milestone_product.MarketFactor = 1
    milestone_product.SalesMilestoneStage = 0
    milestone_save.State.SalesTicks = 19
    write_json("user://saves/slot1.json",milestone_save)
    await press("LOAD_GAME")
    if not await await_node("AcknowledgeSalesMilestone"):
        quit(1)
        return
    await press("SAVE_GAME")
    var pending_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(normalize_numbers(pending_save.State.ReleasedGames[0].PendingSalesMilestones) == [1,2,3,4],"All crossed sales milestones queued")
    await press("LOAD_GAME")
    for stage in range(4):
        check(main.find_child("AcknowledgeSalesMilestone",true,false) != null,"Sequential sales milestone ceremony")
        await press_node("AcknowledgeSalesMilestone")
    check(main.find_child("SalesProduct0",true,false) != null,"Sales resume after all milestone acknowledgements")
    await press("BACK_TO_OFFICE")
    await press("SAVE_GAME")
    var completed_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(completed_save.State.ReleasedGames[0].SalesMilestoneStage == 4,"Acknowledged milestone progress persists")
    # Guaranteed investigation fixture drives the real sales and weekly clocks.
    var recall_save = completed_save
    var recall_product = recall_save.State.ReleasedGames[0]
    recall_product.UnitsSold = 120
    recall_product.Stock = 30
    recall_product.Price = 5800
    recall_product.Demand = 0
    recall_product.Bugs = 50
    recall_product.Recall = {"OriginalState":0,"WeeksRemaining":0,"PendingEvent":null,"OutcomeRoll":null,"RefundAmount":null}
    recall_save.State.Money = 100000000
    recall_save.State.CompanyActivityTicks = 0
    recall_save.State.SalesTicks = 19
    var recall_project = recall_save.State.DevelopmentProjects[0]
    recall_project.OriginalStage = 3
    recall_project.GameTypeSourceIndex = 0
    recall_project.GenreSourceIndex = 0
    recall_project.Progress = recall_project.RequiredPoints
    recall_project.PendingDecision = null
    recall_project.Bugs = 0
    write_json("user://saves/slot1.json",recall_save)
    await press("LOAD_GAME")
    if not await await_node("AcknowledgeRecall"):
        quit(1)
        return
    await press("SAVE_GAME")
    var investigation_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(investigation_save.State.ReleasedGames[0].Recall.WeeksRemaining == 5,"Consumer investigation starts a five-week countdown")
    check(investigation_save.State.ReleasedGames[0].Recall.PendingEvent == 0,"Investigation modal persists")
    var paused_budget = investigation_save.State.CompanyActivityTicks
    Engine.time_scale = 100
    for frame in range(20):
        await process_frame
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var paused_recall_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(paused_recall_save.State.CompanyActivityTicks == paused_budget,"Investigation modal freezes the budget clock")
    await press("LOAD_GAME")
    await press_node("AcknowledgeRecall")
    Engine.time_scale = 100
    if not await await_node("AcknowledgeRecall"):
        quit(1)
        return
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var decision_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    var decision_recall = decision_save.State.ReleasedGames[0].Recall
    check(decision_recall.PendingEvent == 1 and decision_recall.WeeksRemaining == 0,"Five live budget weeks produce the recall decision")
    var expected_refund = 0 if decision_recall.OutcomeRoll == 3 else (870000 if decision_recall.OutcomeRoll == 0 else 150000)
    check(decision_recall.RefundAmount == expected_refund,"Original recall refund formula")
    var investigation_receipts = decision_save.State.ReleasedGames[0].NetSalesReceipts-recall_save.State.ReleasedGames[0].NetSalesReceipts
    check(decision_save.State.Money == 100000000+investigation_receipts-expected_refund,"Recall charges once alongside ongoing sales")
    await press("LOAD_GAME")
    await press("SAVE_GAME")
    check(JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json")).State.Money == decision_save.State.Money,"Result reload does not charge a second refund")
    await press_node("AcknowledgeRecall")
    check(main.find_child("ProduceProduct0",true,false).disabled == (decision_recall.OriginalState == 2),"Recall result controls reprint eligibility")
    await press("BACK_TO_OFFICE")
    await press("TECHNOLOGIES")
    await press_node("PurchaseTechnology0")
    check(main.find_child("PurchaseTechnology0",true,false).disabled,"Owned technology cannot be bought again")
    await press("SAVE_GAME")
    var owned_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(owned_save.State.TechnologyOwnershipLevels["0"] == 3,"Purchase unlocks all training levels")
    var training_xp = owned_save.State.Employees[0].Experience
    await press("LOAD_GAME")
    await press("STAFF")
    await press("TAKE_BREAK")
    Engine.time_scale = 100
    for frame in range(20):
        await process_frame
    Engine.time_scale = 1
    await press("STAFF")
    await press("TRAINING")
    check(main.find_child("TrainTechnology0",true,false).disabled,"Training needs a study seat")
    await press_node("AssignStudySeat1")
    await press_node("TrainTechnology0")
    await press("SAVE_GAME")
    var training_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(training_save.State.Employees[0].Experience == training_xp-10,"Training spends experience upfront")
    check(training_save.State.Employees[0].Office.Training != null,"Active course is saved")
    await press("LOAD_GAME")
    Engine.time_scale = 100
    for frame in range(25):
        await process_frame
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var learned_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(learned_save.State.Employees[0].TechnologyLevels["0"] == 1,"Training completes in the live office")
    check(learned_save.State.Employees[0].Office.Training == null,"Completed course is cleared")
    await press("LOAD_GAME")
    await press("STAFF")
    await press("TRAINING")
    check(main.find_child("AssignStudySeat1",true,false).disabled,"Study seat survives reload")
    await press("BACK_TO_OFFICE")
    await press("STAFF")
    # Authored ready-project fixture isolates application from checkpoint modals.
    var use_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    var use_employee = use_save.State.Employees[0]
    use_employee.Experience = 100
    use_employee.Stress = 0
    use_employee.Office.Action = 6
    use_employee.Office.SeatedAction = 6
    use_employee.Office.WorkEngaged = true
    use_employee.Office.MovementState = 0
    use_employee.Office.Facing = 4
    use_employee.Office.X = 208
    use_employee.Office.Y = 104
    use_employee.Office.Frame = 22
    use_employee.Office.OffsetX = -4
    use_employee.Office.OffsetY = 4
    use_employee.Office.WorkTicks = 0
    use_employee.Office.WorkTier = 0
    use_employee.Office.WorkIdleTicks = 0
    use_employee.Office.IdleBreak = false
    var use_project = use_save.State.DevelopmentProjects[0]
    use_project.OriginalStage = 3
    use_project.GameTypeSourceIndex = 0
    use_project.GenreSourceIndex = 0
    use_project.Progress = use_project.RequiredPoints
    use_project.PendingDecision = null
    use_project.DataSize = 101.9
    use_project.Bugs = 100
    write_json("user://saves/slot1.json",use_save)
    await press("CANCEL")
    await press("LOAD_GAME")
    await press("STAFF")
    await press("USE_TECHNOLOGY")
    await press_node("UseTechnology0")
    await press("SAVE_GAME")
    var use_pending = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(use_pending.State.Employees[0].Experience == 99,"Technology use spends its imported experience cost")
    check(use_pending.State.Employees[0].Office.TechnologyApplication != null,"Active technology is saved")
    await press("LOAD_GAME")
    Engine.time_scale = 100
    for frame in range(5):
        await process_frame
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var applied = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(applied.State.DevelopmentProjects[0].DataSize >= 79 and applied.State.DevelopmentProjects[0].DataSize <= 81,"Compression changes the live project")
    check(applied.State.DevelopmentProjects[0].TechnologyApplicationCounts["0"] == 1,"Application history persists")
    check(applied.State.Employees[0].Office.TechnologyApplication == null,"Completed technology is cleared")
    await press("LOAD_GAME")
    await press("STAFF")
    await press_node("DismissEmployee0")
    check(main.find_child("ConfirmDismissal",true,false) != null,"Dismissal confirmation opens")
    await press("CANCEL")
    check(main.find_child("DismissEmployee0",true,false) != null,"Cancelled dismissal retains employee")
    await press_node("DismissEmployee0")
    await press_node("ConfirmDismissal")
    check(main.find_child("DismissEmployee0",true,false) == null,"Dismissal removes employee card")
    await press("CANCEL")
    await press("SAVE_GAME")
    var dismissal_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(dismissal_save.State.Employees.is_empty(),"Dismissal persists without employee workspace")
    check(dismissal_save.State.DevelopmentProjects.size() == 1,"Dismissal retains active development room")
    await press("COMPANY_ACTIVITIES")
    await press_node("CompanyActivity0")
    check(main.find_child("CompanyActivity1",true,false).disabled,"Business action budget shared across actions")
    await press("SAVE_GAME")
    var activity_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(activity_save.State.Company.Fame >= 13 and activity_save.State.Company.Fame <= 15,"Public relations raises fame")
    check(activity_save.State.Money == dismissal_save.State.Money - 5000000,"Public relations charges shared cost")
    await press("LOAD_GAME")
    await press("COMPANY_ACTIVITIES")
    check(main.find_child("CompanyActivity0",true,false).disabled,"Used budget survives save/load")
    await press("BACK_TO_OFFICE")
    Engine.time_scale = 100
    for frame in range(12):
        await process_frame
    Engine.time_scale = 1
    await press("COMPANY_ACTIVITIES")
    await press_node("CompanyActivity1")
    await press("SAVE_GAME")
    var research_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(research_save.State.Company.OriginalMember90 >= 25 and research_save.State.Company.OriginalMember90 <= 27,"Genre research raises unlock threshold")
    await press("BACK_TO_OFFICE")
    Engine.time_scale = 100
    for frame in range(12):
        await process_frame
    Engine.time_scale = 1
    await press("COMPANY_ACTIVITIES")
    await press_node("CompanyActivity2")
    await press("SAVE_GAME")
    var capacity_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(capacity_save.State.Company.ProductionCapacity >= 54,"Production expansion increases capacity")
    # Authored salary fixture: original staff total and shared-index debit differ.
    var payroll_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    payroll_save.State.MinimumSourceIndex = 0
    payroll_save.State.SourceUpperBound = 3
    payroll_save.State.OriginalPayrollScratchIndex = null
    payroll_save.State.EmployeeSources = []
    payroll_save.State.Employees = []
    for index in range(3):
        var salary_employee = {"Name":"Synthetic Salary "+str(index),"SourceIndex":index,"RawParameter40":[700,100,300][index]}
        payroll_save.State.EmployeeSources.append({"SourceIndex":index,"GateValue":0 if index == 0 else 1,"Employee":salary_employee})
        if index > 0:
            payroll_save.State.Employees.append(salary_employee.duplicate(true))
    var payroll_money = payroll_save.State.Money
    write_json("user://saves/slot1.json",payroll_save)
    await press("BACK_TO_OFFICE")
    await press("LOAD_GAME")
    await press("STAFF")
    check(main.find_child("StaffSalaryTotal",true,false).text.ends_with("400"),"Staff screen sums each employed salary")
    check(main.find_child("MonthlySalary1",true,false).text.ends_with("100"),"Employee monthly salary is displayed")
    check(main.find_child("OriginalPayrollPreview",true,false) == null,"Staff screen keeps the reconstruction preview in the inspector")
    await press("CANCEL")
    await press("CALENDAR_INSPECTOR")
    check(main.find_child("OriginalPayrollPreview",true,false).text.ends_with("600"),"Calendar inspector preserves the distinct shared-index payroll preview")
    await press("CANCEL")
    await press("SAVE_GAME")
    var payroll_captured = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(payroll_captured.State.OriginalPayrollScratchIndex == 2,"Staff scratch snapshot survives save")
    check(payroll_captured.State.Money == payroll_money,"Payroll preview does not charge money")
    await press("LOAD_GAME")
    await press("SAVE_GAME")
    check(JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json")).State.OriginalPayrollScratchIndex == 2,"Loading retains the shared salary index")
    await press_node("TogglePause")
    check(main.find_child("TogglePause",true,false).text == tr("RESUME"),"Paused office immediately offers Resume")
    check(main.find_child("OfficeStatus",true,false).text.contains(tr("PAUSED")),"Pause status appears immediately")
    await press("SAVE_GAME")
    var pause_saved = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(main.find_child("TogglePause",true,false).text == tr("RESUME"),"Rebuilding the office retains the correct pause label")
    Engine.time_scale = 100
    for frame in range(20):
        await process_frame
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var pause_after_wait = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(pause_after_wait.State.CompanyActivityTicks == pause_saved.State.CompanyActivityTicks,"Manual pause freezes the shared weekly clock")
    check(pause_after_wait.State.SalesTicks == pause_saved.State.SalesTicks,"Manual pause freezes sales")
    await press_node("TogglePause")
    check(main.find_child("TogglePause",true,false).text == tr("PAUSE"),"Resumed office immediately offers Pause")
    check(not main.find_child("OfficeStatus",true,false).text.contains(tr("PAUSED")),"Resume clears pause status immediately")
    Engine.time_scale = 100
    for frame in range(3):
        await process_frame
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var resumed_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(resumed_save.State.CompanyActivityTicks != pause_saved.State.CompanyActivityTicks,"Resume advances the clock again")
    var player = main.find_child("MusicPlayer",true,false)
    check(player != null and player.stream != null and abs(player.stream.get_length()-1.0) < 0.01,"Local music loads during gameplay")
    for index in range(8):
        var track = AudioStreamWAV.load_from_file("user://music/office-%02d.wav" % (index+1))
        check(track != null and abs(track.get_length()-(index+1)) < 0.01,"Local music duration %d" % (index+1))
    await press("SETTINGS")
    var enabled = main.find_child("MusicEnabled",true,false)
    var volume = main.find_child("MusicVolume",true,false)
    enabled.button_pressed = false
    volume.value = 42
    await process_frame
    check(player.stream_paused or not player.playing,"Music toggle prevents playback independently")
    var config = ConfigFile.new()
    check(config.load("user://audio.cfg") == OK,"Audio preferences saved")
    check(config.get_value("music","enabled") == false and config.get_value("music","volume") == 42,"Mute and volume survive configuration save")
    enabled.button_pressed = true
    var previous_stream = player.stream
    await press("NEXT_MUSIC_TRACK")
    check(player.stream != previous_stream and not player.stream_paused,"Next soundtrack track resumes playback")
    previous_stream = player.stream
    player.finished.emit()
    await process_frame
    check(player.stream != previous_stream,"Finished recording advances the playlist")
    # Eight advances must restore the same stream identity.
    previous_stream = player.stream
    for index in range(8):
        player.finished.emit()
        await process_frame
    check(player.stream == previous_stream,"Playlist wraps after all eight local tracks")
    await press("BACK_TO_OFFICE")
    check(main.find_child("DesktopEmployeeTable",true,false).get_root().get_child_count() == 2,"Desktop table refreshes after management")
    # Calendar fixtures exercise actual modal controls, persistence and the
    # resumed payroll branch. No original records or assets are published.
    var calendar_save = payroll_captured.duplicate(true)
    var cs = calendar_save.State
    cs.Year = 3
    cs.Month = 5
    cs.Week = 2
    cs.CompanyActivityTicks = 0
    cs.Employees = []
    cs.EmployeeSources = []
    cs.DevelopmentProjects = []
    cs.ReleasedGames = []
    cs.AvailableHardware = []
    cs.Calendar = {"HardwareStage":1,"ShipmentsSinceHardwareTrigger":0,
        "Hardware":[{"SourceIndex":0,"Availability":0,"LaunchWeeks":0,"RetirementWeeks":0,"Market":40,"GrowthWeeks":0,"GrowthBoost":0}],
        "Loans":[{"OriginalType":0},{"OriginalType":0},{"OriginalType":0}],
        "Tenants":[{"OriginalState":0,"Weeks":0},{"OriginalState":0,"Weeks":0}],
        "OriginalSlots0444":[-1,-1,-1],
        "Pending":{"ClosingDate":{"Year":3,"Month":5,"Week":1},"Phase":4,"Index":0,"PendingEvent":3}}
    write_json("user://saves/slot1.json",calendar_save)
    await press("LOAD_GAME")
    check(main.find_child("AcknowledgeCalendar",true,false) != null,"Hardware launch news opens after loading its transaction")
    # Main-window actions cannot replace an original modal weekly decision.
    for key in ["HIRE_EMPLOYEES","COMPANY_ACTIVITIES","STAFF","DEVELOPMENT_PLANNING"]:
        var calendar_toolbar = main.find_child("Toolbar"+key,true,false)
        check(calendar_toolbar.disabled,"Calendar modal disables toolbar action: "+key)
        calendar_toolbar.pressed.emit()
        await process_frame
        check(main.find_child("AcknowledgeCalendar",true,false) != null,"Calendar guards direct action callbacks: "+key)
    var staff_menu = main.find_child("MENU_STAFF",true,false).get_popup()
    staff_menu.about_to_popup.emit()
    check(staff_menu.is_item_disabled(0) and staff_menu.is_item_disabled(1),"Calendar modal disables employee menu actions")
    check(main.find_child("TogglePause",true,false).disabled,"Calendar modal owns the pause state")
    Engine.time_scale = 100
    for frame in range(20):
        await process_frame
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var launch_paused = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(launch_paused.State.Calendar.Pending.PendingEvent == 3 and launch_paused.State.SalesTicks == cs.SalesTicks,"Hardware dialog freezes the office and survives saving")
    check(launch_paused.State.AvailableHardware.is_empty(),"Platform stays unavailable until news acknowledgement")
    await press("LOAD_GAME")
    await press_node("AcknowledgeCalendar")
    Engine.time_scale = 100
    for frame in range(3):
        await process_frame
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var launched = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(launched.State.Calendar.Pending == null and launched.State.AvailableHardware.size() == 1 and launched.State.AvailableHardware[0] == 0,"Acknowledged launch resumes the calendar and unlocks its platform")
    # Renewal mutates the employed roster before the same week's salary debit.
    var renewal_save = payroll_captured.duplicate(true)
    var rs = renewal_save.State
    rs.Year = 3
    rs.Month = 3
    rs.Week = 4
    rs.CompanyActivityTicks = 0
    rs.DevelopmentProjects = []
    rs.ReleasedGames = []
    rs.CompanyActivityPerformed = true
    rs.Company.WeeklyNet = 0
    rs.Company.MonthlyNet = 0
    rs.Company.YearlyNet = 0
    rs.Employees[0].ContractFee = 10
    rs.Employees[1].ContractFee = 20
    rs.Calendar = launched.State.Calendar.duplicate(true)
    rs.Calendar.Pending = {"ClosingDate":{"Year":3,"Month":3,"Week":4},"Phase":1,"Index":0,"PendingEvent":0}
    write_json("user://saves/slot1.json",renewal_save)
    await press("LOAD_GAME")
    check(main.find_child("RenewEmployee1",true,false).button_pressed,"Renewal initially selects employed staff")
    main.find_child("RenewEmployee2",true,false).button_pressed = false
    await press_node("ConfirmContractRenewal")
    Engine.time_scale = 100
    if not await await_node("ExhibitionSelection"):
        quit(1)
        return
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var renewed = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(renewed.State.Employees.size() == 1 and renewed.State.EmployeeSources[2].GateValue == 0,"Unchecked annual contract dismisses its source before payroll")
    check(renewed.State.Money == rs.Money-10-300,"Annual fee and shared-index monthly salary are each charged once")
    check(renewed.State.Month == 4 and renewed.State.Week == 1 and renewed.State.Company.WeeklyNet == -300,"April rollover resets accounting before payroll")
    check(renewed.State.Calendar.Pending.PendingEvent == 7,"Spring exhibition pauses later calendar transactions")
    await press("LOAD_GAME")
    await press("SAVE_GAME")
    var renewed_reload = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(renewed_reload.State.Money == renewed.State.Money,"Reloading the pending exhibition never repeats annual fees or salary")
    # Participate in an exhibition using a wholly synthetic product.
    var exhibition_save = renewed_reload.duplicate(true)
    var es = exhibition_save.State
    es.Employees = []
    es.EmployeeSources = []
    es.Recruitment.CandidateDatabaseDecoded = false
    es.DevelopmentProjects = []
    es.ReleasedGames = [released.State.ReleasedGames[0].duplicate(true)]
    es.ReleasedGames[0].Recognition = 95
    es.ReleasedGames[0].Demand = 0
    es.ReleasedGames[0].Stock = 0
    es.ReleasedGames[0].ReleasedOn = {"Year":3,"Month":3,"Week":1}
    es.Calendar.Pending.Exhibition = {"EligibleProducts":[0],"SelectedProducts":[]}
    es.Calendar.ExhibitionVisitorRecord = 0
    write_json("user://saves/slot1.json",exhibition_save)
    await press("LOAD_GAME")
    check(main.find_child("ExhibitionSelection",true,false) != null,"Exhibition selection survives loading")
    main.find_child("ExhibitProduct0",true,false).button_pressed = true
    check(main.find_child("ExhibitionFee",true,false).text.ends_with("10,000,000"),"Exhibition checkbox displays the proven fee")
    await press_node("ParticipateExhibition")
    await press("SAVE_GAME")
    var exhibited = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    var attendance = exhibited.State.Calendar.Pending.Exhibition.Visitors
    check(attendance >= 9900 and attendance <= 17578,"Exhibition attendance stays within original timer bounds")
    check(exhibited.State.Money == es.Money-10000000 and exhibited.State.ReleasedGames[0].Recognition == 95+floor(attendance/500),"Exhibition fee and uncapped product awareness commit together")
    await press("LOAD_GAME")
    check(main.find_child("ExhibitionResult",true,false) != null,"Exhibition result survives loading without another fee")
    await press("SAVE_GAME")
    check(JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json")).State.Money == exhibited.State.Money,"Loading exhibition result never repeats payment")
    await press_node("CloseExhibition")
    # Award categories with no own entries require no invented rival names.
    var awards_save = exhibition_save.duplicate(true)
    var aws = awards_save.State
    aws.Month = 12
    aws.Week = 4
    aws.ReleasedGames = []
    aws.Calendar.Pending = {"ClosingDate":{"Year":3,"Month":12,"Week":3},"Phase":0,"Index":0,"PendingEvent":8,
        "Awards":{"Year":3,"Entries":[],"CategoryIndex":0}}
    write_json("user://saves/slot1.json",awards_save)
    await press("LOAD_GAME")
    for category in range(5):
        check(main.find_child("RevealAward",true,false) != null,"Award category awaits announcement")
        await press_node("RevealAward")
        await press("SAVE_GAME")
        var announced = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
        check(announced.State.Calendar.Pending.Awards.CategoryIndex == category and announced.State.Calendar.Pending.Awards.ResultReady,"Award result and cursor persist")
        check(announced.State.Money == aws.Money,"Competitor result never invents a company payout")
        await press("LOAD_GAME")
        check(main.find_child("AwardResult",true,false) != null,"Saved award result reopens without rerolling")
        await press_node("ContinueAwards")
    # Force only a proven sales override for a deterministic grand prize.
    var grand_save = awards_save.duplicate(true)
    grand_save.State.ReleasedGames = [exhibition_save.State.ReleasedGames[0].duplicate(true)]
    grand_save.State.ReleasedGames[0].UnitsSold = 1000000
    grand_save.State.ReleasedGames[0].Quality = {"Fun":0,"Novelty":0,"Graphics":0,"Music":0}
    grand_save.State.Calendar.Pending.Awards.Entries = [0]
    grand_save.State.Calendar.Pending.Awards.CategoryIndex = 4
    write_json("user://saves/slot1.json",grand_save)
    await press("LOAD_GAME")
    await press_node("RevealAward")
    await press("SAVE_GAME")
    var grand = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(grand.State.Money == grand_save.State.Money+500000000 and grand.State.ReleasedGames[0].OriginalAwardCode == 5,"Grand prize credits the proven reward and product marker")
    await press("LOAD_GAME")
    await press("SAVE_GAME")
    check(JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json")).State.Money == grand.State.Money,"Grand prize reload does not duplicate its payout")
    await press_node("ContinueAwards")
    # Graduate forms use source employees and one hire-or-decline per candidate.
    var graduates_save = payroll_captured.duplicate(true)
    var gs = graduates_save.State
    gs.Year = 3
    gs.Month = 4
    gs.Week = 2
    gs.CompanyActivityTicks = 0
    gs.DevelopmentProjects = []
    gs.ReleasedGames = []
    gs.Employees = []
    gs.MinimumSourceIndex = 0
    gs.MaximumSourceIndex = 2
    for row in gs.EmployeeSources:
        row.GateValue = 0
        row.Employee.ContractFee = 100
    gs.Calendar = launched.State.Calendar.duplicate(true)
    gs.Calendar.OriginalSlots0444 = [0,-1,-1]
    gs.Calendar.Pending = {"ClosingDate":{"Year":3,"Month":4,"Week":1},"Phase":9,"Index":0,"PendingEvent":9,
        "Graduates":{"Candidates":[0,2],"CandidateIndex":0}}
    write_json("user://saves/slot1.json",graduates_save)
    await press("LOAD_GAME")
    check(main.find_child("GraduatePosition",true,false) != null,"Graduate prompt opens from its saved source queue")
    await press_node("HireGraduate")
    await press("SAVE_GAME")
    var hired_graduate = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(hired_graduate.State.Money == gs.Money-100 and hired_graduate.State.Employees.size() == 1,"Graduate hiring charges the displayed source contract fee")
    check(hired_graduate.State.Calendar.Pending.Graduates.CandidateIndex == 1,"Hiring advances exactly one graduate")
    await press("LOAD_GAME")
    await press_node("DeclineGraduate")
    await press("SAVE_GAME")
    var graduates_done = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(graduates_done.State.Employees.size() == 1 and graduates_done.State.Money == hired_graduate.State.Money,"Declining another graduate preserves the first hire")
    # Beginner ending is terminal; Normal mode can load another save afterwards.
    var ending_save = awards_save.duplicate(true)
    ending_save.State.Mode = 0
    ending_save.State.Year = 2
    ending_save.State.Calendar.Pending = {"ClosingDate":{"Year":2,"Month":12,"Week":4},"Phase":0,"Index":0,"PendingEvent":10}
    write_json("user://saves/slot1.json",ending_save)
    await press("LOAD_GAME")
    check(main.find_child("BeginnerEnding",true,false) != null,"Beginner ending opens its title-return message")
    await press("BACK_TO_TITLE")
    check(main.find_child("BeginnerEnding",true,false) == null,"Ending returns to the title screen")
    # The uncertainty guard permits only the verified staff-list scratch writer.
    var unknown_payroll = payroll_captured.duplicate(true)
    var ps = unknown_payroll.State
    ps.Year = 3
    ps.Month = 5
    ps.Week = 4
    ps.CompanyActivityTicks = 0
    ps.OriginalPayrollScratchIndex = null
    ps.DevelopmentProjects = []
    ps.ReleasedGames = []
    ps.Calendar = launched.State.Calendar.duplicate(true)
    ps.Calendar.Pending = {"ClosingDate":{"Year":3,"Month":5,"Week":4},"Phase":1,"Index":0,"Block":0}
    write_json("user://saves/slot1.json",unknown_payroll)
    await press("LOAD_GAME")
    await press_node("OpenPayrollStaff")
    check(main.find_child("StaffSalaryTotal",true,false).text.ends_with("400"),"Uncertain payroll can open the verified staff-list projection")
    check(main.find_child("DismissEmployee1",true,false) == null and main.find_child("DismissEmployee2",true,false) == null,"Payroll resolution staff list cannot dismiss employees during the suspended transaction")
    await press("CANCEL")
    await press_node("RetryCalendar")
    await press("SAVE_GAME")
    var resolved_payroll = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(resolved_payroll.State.Calendar.Pending == null and resolved_payroll.State.Money == ps.Money-600,"Verified scratch resolution resumes the saved payroll once")
    check(resolved_payroll.State.Month == 6 and resolved_payroll.State.Week == 1,"Resolving payroll completes the calendar rollover")
    previous_stream = null
    player = null
    main.queue_free()
    main = null
    await process_frame
    await process_frame
    print("Core loop UI checks complete; failures=",failures)
    quit(0 if failures == 0 else 1)
