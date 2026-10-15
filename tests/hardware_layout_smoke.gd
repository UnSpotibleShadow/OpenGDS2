extends "res://tests/window_artwork_smoke.gd"

func hardware_value(ready: bool=false) -> Dictionary:
    var v=complete_value();v.State.CompanyActivityTicks=0
    v.State.DevelopmentProjects=[{"RoomIndex":0,"HardwareSourceIndex":0,"AudienceSourceIndex":0,"OriginalRoomState":2,"OriginalMode":1,"Title":"Authored hardware development","OriginalStage":0,"RequiredPoints":2500,"Progress":2500 if ready else 1234,
        "Quality":{"Fun":800 if ready else 140,"Novelty":800 if ready else 260,"Graphics":800 if ready else 390,"Music":800 if ready else 520},
        "PolishingTargets":{"Fun":800,"Novelty":800,"Graphics":800,"Music":800},"Focus":{"Fun":25,"Novelty":25,"Graphics":25,"Music":25},
        "HardwareDesign":{"Design":1,"Medium":1,"TechnologyFlags":{"1":1,"3":1,"6":1,"7":1 if ready else 0}}}]
    v.State.Calendar.Hardware=[]
    for id in range(11):v.State.Calendar.Hardware.append({"SourceIndex":id,"Availability":1 if id==0 else 0,"LaunchWeeks":0,"RetirementWeeks":0,"Market":40,"GrowthWeeks":0,"GrowthBoost":0})
    return v

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,800)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    setup_catalog()
    if not FileAccess.file_exists("user://original/Graphic/Embedded/HardwareLaunchScene.bmp"):blank_bmp("user://original/Graphic/Embedded/HardwareLaunchScene.bmp",256,160,false)
    var catalog=JSON.parse_string(FileAccess.get_file_as_string("user://original/catalog.json"))
    catalog.Audiences[0].Identifier=1;catalog.Hardware[0].RawMembers["1050"]=80;catalog.Hardware[0].RawMembers["0FA8"]=0
    for id in range(1,11):
        var hardware=catalog.Hardware[0].duplicate(true);hardware.SourceIndex=id;hardware.Name="Authored hardware %d"%id;hardware.RawMembers["0EC8"]=id;catalog.Hardware.append(hardware)
    write_json("user://original/catalog.json",integer_tokens(catalog))
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);check(language.save("user://interface.cfg")==OK,"Hardware layout locale")
        var plan=hardware_value();plan.State.DevelopmentProjects=[];plan.State.Calendar.OriginalSlots0444=[8,-1,-1]
        await load_fixture(plan);await press_node("TogglePause");await press_node("CompanyFacilities");await press_node("StartHardwarePlan0");await window("HardwarePlanWindow",378,280);check(not main.find_child("StartHardwareDevelopment",true,false).disabled,"Complete platform catalogue enables planning");await dispose()
        await load_fixture(hardware_value());await press_node("TogglePause")
        await press_node("OpenProject0");await window("HardwareProjectWindow",384,206)
        check(main.find_child("HardwareTechnologyFlag7",true,false).text=="off" and main.find_child("HardwareTechnologyFlag1",true,false).text=="on","Monitor technology flags")
        var monitor=main.find_child("HardwareProjectWindow",true,false);monitor.position=Vector2(720,180)
        await menu_choice("MENU_COMPANY",8);await window("HardwareMarketWindow",414,235)
        check(is_instance_valid(monitor) and monitor.position==Vector2(720,180),"Hardware monitor retained behind market view")
        var table=main.find_child("HardwareMarketTable",true,false)
        check(table.get_root().get_child_count()==1 and table.get_root().get_first_child().get_text(2)=="400,000","Market uses current available hardware and shipped units")
        table.get_root().get_first_child().select(0)
        await close_window("HardwareMarketWindow")
        check(monitor.position==Vector2(720,180),"Returning retains hardware monitor position")
        await close_window("HardwareProjectWindow")
        await press_node("OpenProject0");check(main.find_child("HardwareProjectWindow",true,false).position==Vector2(720,180),"Reopening hardware monitor retains position")
        await dispose()
        await load_fixture(hardware_value(true));await window("HardwareLaunchWindow",330,322)
        check(main.find_child("HardwareLaunchScene",true,false).texture!=null and main.find_child("HardwareLaunchDevice",true,false).texture!=null,"Launch has shared scene and design-specific device")
        var name=main.find_child("HardwareLaunchName",true,false);name.text="Open Station";name.text_changed.emit(name.text)
        await press_node("ToolbarSAVE_GAME");var money=saved().Money
        await press_node("ToolbarLOAD_GAME");check(main.find_child("HardwareLaunchName",true,false).text=="Open Station","Draft launch name survives reload")
        await press_node("LaunchOwnHardware");await press_node("ToolbarSAVE_GAME")
        check(saved().Money==money and saved().OwnHardware.Launched.Name=="Open Station","Launch keeps accounting and saved name")
        await menu_choice("MENU_PRODUCTS",3);await window("OwnHardwareReportWindow",320,115)
        check(main.find_child("OwnHardwareReportName",true,false).text=="Open Station" and main.find_child("OwnHardwareUnits",true,false).text=="100,000","Own report shows launched name and current market")
        await close_window("OwnHardwareReportWindow")
        await menu_choice("MENU_COMPANY",8);check(main.find_child("HardwareMarketTable",true,false).get_root().get_child_count()==2,"Launched own hardware joins market view")
        await dispose()
        var announced=hardware_value();announced.State.DevelopmentProjects=[];announced.State.Calendar.Pending={"ClosingDate":{"Year":3,"Month":4,"Week":4},"Phase":4,"Index":0,"PendingEvent":3}
        await load_fixture(announced);await window("HardwareAnnouncementWindow",330,356)
        check(main.find_child("HardwareAnnouncementScene",true,false).texture!=null,"Third-party launch shares recovered scene")
        await press_node("AcknowledgeCalendar");await press_node("ToolbarSAVE_GAME")
        check(saved().Calendar.Pending.HardwareLaunchPage==1,"Announcement advances its saved page once")
        await dispose()
    print("Hardware layout smoke failures: ",failures)
    quit(0 if failures==0 else 1)
