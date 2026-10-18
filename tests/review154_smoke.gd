extends "res://tests/window_artwork_smoke.gd"

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,800)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true);setup_catalog()
    var catalog=JSON.parse_string(FileAccess.get_file_as_string("user://original/catalog.json"))
    catalog.Technologies=[{"SourceIndex":0,"Identifier":0,"Name":"Authored technique","Description":"Authored description","RequiredTrainingWork":[40,60,80],"ExperienceCosts":[10,20,30],"RawTable1568":[1,2,3],"RawTable158C":[1,2,3]}]
    write_json("user://original/catalog.json",integer_tokens(catalog))
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);language.save("user://interface.cfg")
        var v=complete_value();v.State.TechnologyOwnershipLevels={"0":3}
        for e in v.State.Employees:e.TechnologyLevels={"0":0};e.MaximumTechnologies=4
        await load_fixture(v);await press_node("TogglePause");await select_employee(0)
        check(main.find_child("EmployeeActions",true,false)==null,"Employee popup omits unsupported action dropdown")
        await office_input(MOUSE_BUTTON_RIGHT,Vector2(166,68));await press_node("ToolbarSAVE_GAME")
        check(saved().Employees[0].Office.Action==1,"Right-click study desk assigns selected employee")
        await office_input(MOUSE_BUTTON_RIGHT,Vector2(350,155));await press_node("ToolbarSAVE_GAME")
        check(saved().Employees[0].Office.Action>=18,"Right-click vending area sends selected employee to rest")
        await office_input(MOUSE_BUTTON_RIGHT,Vector2(166,68))
        await office_input(MOUSE_BUTTON_LEFT,Vector2(165,58));await window("StudyComputerWindow",205,315)
        var courses=main.find_child("StudyAvailableTechnologies",true,false)
        check(courses.get_root().get_first_child()!=null,"Study PC offers company-owned training")
        courses.get_root().get_first_child().select(0);courses.item_activated.emit();await process_frame
        await press_node("TrainTechnology0");await press_node("ToolbarSAVE_GAME")
        check(saved().Employees[0].Office.Training.TechnologySourceIndex==0 and saved().Employees[0].Experience==90,"Study PC starts one course and charges experience once")
        check(main.find_child("CompanyFacilities",true,false)==null and main.find_child("CompanyInformation",true,false)==null,"Company overview omits extra buttons")
        await dispose()
        v=complete_value();var p=stress_fixture().State.DevelopmentProjects[0].duplicate(true)
        p.PendingDecision=2;p.DecisionIntroductionAcknowledged=2;p.OriginalStage=3;p.Progress=p.RequiredPoints;p.GameTypeSourceIndex=0;p.GenreSourceIndex=0;p.Bugs=3
        v.State.DevelopmentProjects=[p]
        await load_fixture(v)
        check(main.find_child("CompletionQualityFields",true,false)!=null,"Completion has shared quality region")
        var release=main.find_child("CompletionRelease",true,false);release.pressed.emit();await process_frame
        check(main.find_child("ConversationWindow",true,false)!=null and main.find_child("ShipmentWindow",true,false)==null,"Release begins with secretary dialogue")
        for page in range(8):
            if main.find_child("ConversationWindow",true,false)==null:break
            await award_page()
        await window("MediaInformationWindow",305,185)
        check(main.find_child("ShipmentWindow",true,false)==null,"Media information precedes shipment finalization")
        await press_node("CloseMediaInformation");await window("ShipmentWindow",445,380)
        await press_node("ToolbarSAVE_GAME")
        check(saved().ReleasedGames.size()==0 and saved().Money==v.State.Money,"Opening release flow never produces copies or spends funds")
        await dispose()
    print("Review 154 smoke failures: ",failures);quit(0 if failures==0 else 1)
