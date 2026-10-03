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
        if button.text == tr(key):
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

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("core_loop_smoke requires disposable XDG_DATA_HOME")
        quit(1)
        return
    var raw_type := {"1378":5,"1308":10,"13B0":1,"12EC":3}
    var catalog := {
        "SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6",
        "Hardware":[{"SourceIndex":0,"Name":"Synthetic Hardware","RawMembers":{"0F00":8,"0FE0":20,"0FFC":10,"0EC8":0,"1050":40},
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
        "Company":{"Name":"Synthetic UI Company","PresidentName":"Tester","Fame":12,"Rank":0,"ProductionCapacity":50,"OriginalMember90":23,"ActivityCost":5000000},
        "Employees":[worker],"OriginalAccuracyCells":{"99":0},"OriginalTypeRosterMap":roster,"SalesFee1":1500,"SalesFee2":1500,"CompanyActivityTicks":0,"CompanyActivityPerformed":false}
    write_json("user://original/catalog.json",catalog)
    write_json("user://saves/slot1.json",{"Format":"OpenGDS2","SchemaVersion":1,"State":state})
    main = load("res://scenes/Main.tscn").instantiate()
    root.add_child(main)
    await process_frame
    await press("LOAD_GAME")
    await press("START_GAME_DEVELOPMENT")
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
    check(checkpoint.State.DevelopmentProjects[0].PendingDecision == 0,"Type checkpoint persisted")
    # Reload checkpoint through the actual office controls before selecting.
    await press("LOAD_GAME")
    await await_node("SelectType0")
    await press_node("SelectType0")
    if not await await_node("SelectGenre0"):
        quit(1)
        return
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
    await press("LOAD_GAME")
    check(main.find_child("ReviewScore3",true,false) != null,"Load restores review ceremony")
    await press_node("AcknowledgeReviews")
    check(main.find_child("SalesProduct0",true,false) != null,"Review acknowledgement opens sales screen")
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
    await press_node("ProduceProduct0")
    check(main.find_child("ProductionCopies",true,false) != null,"Reprint controls")
    main.find_child("ProductionCopies",true,false).value = 1000
    await press_node("ConfirmProduction")
    await press("BACK_TO_OFFICE")
    await press("SAVE_GAME")
    var reprinted = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(reprinted.State.ReleasedGames[0].ProductionSpending == product.ProductionSpending + 500000,"Reprint costs original band plus packaging")
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
    print("Core loop UI checks complete; failures=",failures)
    quit(0 if failures == 0 else 1)
