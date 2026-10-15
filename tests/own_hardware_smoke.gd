extends "res://tests/mystery_lab_smoke.gd"

# Shared input/save helpers; every gameplay fixture below is authored.
func fixture() -> Dictionary:
    var value := super.fixture()
    value.State.Money=300000000
    value.State.Calendar.OriginalSlots0444=[8,-1,-1]
    value.State.Calendar.Hardware=[]
    for id in range(11):
        value.State.Calendar.Hardware.append({"SourceIndex":id,"Availability":1 if id==0 else 0,"LaunchWeeks":0,"RetirementWeeks":0,"Market":40,"GrowthWeeks":0,"GrowthBoost":0})
    value.State.Employees[0].RawParameter34=4
    value.State.Employees[0].TechnologyLevels={"1":1,"3":1,"6":1,"7":1}
    value.State.DevelopmentComputerIndex=0
    return value

func catalog() -> Dictionary:
    var value := {"SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6",
        "Hardware":[],"Technologies":[],"DevelopmentComputers":[{"SourceIndex":0,"Name":"Synthetic PC","BitWidth":64}],
        "Audiences":[{"SourceIndex":0,"Name":"Synthetic audience","RawMembers":{"1454":300,"148C":20,"14A8":2}}]}
    for id in range(11):
        value.Hardware.append({"SourceIndex":id,"Name":"Synthetic platform " + str(id),
            "RawMembers":{"0EC8":id,"0F00":8,"0FFC":10,"0FE0":100,"0FC4":0,"0F1C":1 if id==0 else 0,"1050":40,"0F54":15,"10A4":0,"10C0":0},
            "ProductionBands":[{"SizeThreshold":16,"UnitCost":300},{"SizeThreshold":32,"UnitCost":700},{"SizeThreshold":48,"UnitCost":1200},{"SizeThreshold":64,"UnitCost":1800}]})
    for id in [1,3,6,7]:
        value.Technologies.append({"SourceIndex":id,"Identifier":id,"Name":"Synthetic technology " + str(id),"RawTable1568":[5,10,15],"RawTable158C":[1,2,3]})
    return value

func pending_fixture(medium: int) -> Dictionary:
    var value := fixture()
    value.State.Money=9876543
    value.State.DevelopmentProjects=[{"RoomIndex":0,"HardwareSourceIndex":0,"AudienceSourceIndex":0,"OriginalRoomState":2,"OriginalMode":1,
        "Title":"Synthetic hardware development","OriginalStage":0,"RequiredPoints":2500,"Progress":2500,"Quality":{"Fun":800,"Novelty":800,"Graphics":800,"Music":800},
        "PolishingTargets":{"Fun":800,"Novelty":800,"Graphics":800,"Music":800},"Focus":{"Fun":25,"Novelty":25,"Graphics":25,"Music":25},
        "TechnologyApplicationCounts":{"0":0,"1":0,"2":0,"3":0,"6":0,"7":0},
        "HardwareDesign":{"Design":1,"Medium":medium,"TechnologyFlags":{"1":0,"3":1,"6":1,"7":1},"PendingTechnology":1}}]
    return value

func select_option(name: String,id: int) -> void:
    var option=main.find_child(name,true,false)
    check(option!=null,"Hardware option " + name)
    if option!=null:
        option.select(id)
        option.item_selected.emit(id)
    await process_frame

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("own_hardware_smoke requires disposable XDG_DATA_HOME")
        quit(1)
        return
    root.size=Vector2i(1280,720)
    write_json("user://original/catalog.json",catalog())
    for locale in ["en","ja"]:
        var language := ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Hardware locale " + locale)
        await load_fixture(fixture())
        await press_node("TogglePause")
        await press_node("CompanyFacilities")
        await press_node("StartHardwarePlan0")
        check(main.find_child("StartHardwareDevelopment",true,false).disabled,"Expensive default design requires enough funds")
        await select_option("HardwareDesign",1)
        await select_option("HardwareMedium",1)
        check(main.find_child("HardwareDesignPicture",true,false).get_meta("hardware_picture_index")==14,"Design B CD uses original picture 14")
        await press("CANCEL")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Money==300000000 and saved().OwnHardware.SelectedDesign==1 and saved().OwnHardware.SelectedMedium==1,"Declining preserves cash and shared design choices")
        await press_node("StartHardwarePlan0")
        await press_node("StartHardwareDevelopment")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Money==100000000 and saved().Company.WeeklyNet==11-200000000,"Hardware acceptance charges once with accounting")
        check(saved().DevelopmentProjects[0].OriginalRoomState==2 and saved().DevelopmentProjects[0].HardwareDesign.Medium==1,"Hardware project created")
        await press_node("CompanyFacilities")
        check(main.find_child("StartHardwarePlan0",true,false).disabled,"A second hardware project is disabled")
        await press_node("CloseFacility0")
        await press_node("ConfirmFacilityClosure")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Calendar.OriginalSlots0444[0]==-1 and saved().DevelopmentProjects.size()==1 and saved().Money==100000000,"Demolition keeps hardware project without refund")
        await press_node("ToolbarLOAD_GAME")
        await press_node("OpenProject0")
        check(main.find_child("HardwareReport",true,false)!=null,"Hardware monitor survives reload after demolition")
        var cancellation := false
        for button in main.find_children("*","Button",true,false):
            if button.is_visible_in_tree() and button.text==tr("CANCEL_PROJECT"):
                cancellation=true
        check(not cancellation,"Hardware monitor exposes no unsupported cancellation")
        await press_node("TogglePause")
        await press_node("ToolbarSAVE_GAME")
        var before_ticks: float = saved().CompanyActivityTicks
        await create_timer(0.3).timeout
        await press_node("ToolbarSAVE_GAME")
        check(saved().CompanyActivityTicks>before_ticks,"Simulation continues while the hardware monitor is open")
        await dispose()
        for medium in [0,1]:
            await load_fixture(pending_fixture(medium))
            var message=main.find_child("ConversationMessage",true,false)
            check(message!=null and not message.text.begins_with("OWN_HARDWARE_"),"Translated technology completion")
            await press_node("ToolbarSAVE_GAME")
            check(saved().DevelopmentProjects[0].HardwareDesign.TechnologyFlags["1"]==0,"Flag waits for the source modal acknowledgment")
            await press_node("ToolbarLOAD_GAME")
            main.find_child("CompanyFacilities",true,false).pressed.emit()
            await process_frame
            check(main.find_child("ConversationMessage",true,false)!=null,"Hardware completion blocks other facilities")
            await advance_page()
            check(main.find_child("HardwareLaunchWindow",true,false)!=null,"All requirements open the launch form")
            var input=main.find_child("HardwareLaunchName",true,false)
            var console_name: String = "Synthetic Console " + locale + str(medium)
            input.text=console_name
            input.text_changed.emit(console_name)
            await press_node("ToolbarSAVE_GAME")
            await press_node("ToolbarLOAD_GAME")
            check(main.find_child("HardwareLaunchName",true,false).text==console_name,"Launch name draft survives reload")
            check(saved().OwnHardware.Launched==null and saved().DevelopmentProjects[0].HardwareDesign.TechnologyFlags["1"]==1,"Acknowledgment sets flag without early launch")
            await press_node("LaunchOwnHardware")
            await press_node("ToolbarSAVE_GAME")
            var state := saved()
            check(state.Money==9876543 and state.Company.WeeklyNet==11,"Launch adds no extra charge")
            check(state.OwnHardware.Launched.Name==console_name and state.HardwareLicenses.has(11.0) and state.AvailableHardware.has(11.0),"Named console is available and licensed")
            check(state.Calendar.Hardware.size()==12 and state.DevelopmentProjects.size()==0,"Launch appends market record and releases room")
            check(state.OwnHardware.Launched.RawMembers["0F54"]==(50 if medium==0 else 36),"Correct medium growth scale")
            await press_node("ToolbarLOAD_GAME")
            await press_node("ToolbarSTART_GAME_DEVELOPMENT")
            var table=main.find_child("PlanningHardware",true,false)
            check(table!=null,"Software planning hardware table exists")
            if table!=null:
                var row=table.get_root().get_first_child()
                var found := false
                while row!=null:
                    if row.get_metadata(0)==11:
                        found=row.get_text(0)==console_name and row.is_selectable(0)
                        row.select(0)
                        table.item_selected.emit()
                    row=row.get_next()
                check(found,"Software planning lists and licenses the saved own console")
                check(main.find_child("PlanningHardwarePicture",true,false).get_meta("hardware_picture_index")==13+medium,"Planning uses saved design and medium artwork")
            await dispose()
    await create_timer(1.0).timeout
    print("own hardware UI failures: ",failures)
    quit(0 if failures==0 else 1)
