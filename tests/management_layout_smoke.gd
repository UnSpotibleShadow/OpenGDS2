extends "res://tests/window_artwork_smoke.gd"

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,800)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    setup_catalog()
    var catalog=JSON.parse_string(FileAccess.get_file_as_string("user://original/catalog.json"))
    catalog.Audiences[0].Identifier=1;catalog.Hardware[0].RawMembers["1050"]=80
    catalog.Hardware[0].RawMembers["0FA8"]=0
    catalog.Technologies=[{"SourceIndex":0,"Identifier":0,"Name":"Authored technique","Description":"Authored description","RequiredTrainingWork":[40,60,80],"ExperienceCosts":[10,20,30],"RawTable1568":[1,2,3],"RawTable158C":[1,2,3]}]
    catalog.Secretaries=[]
    for i in range(5):catalog.Secretaries.append({"SourceIndex":i,"Name":"Authored secretary %d"%i,"RankCode":mini(i,3),"ContractFee":100+i,"AdvertisedSalary":10+i,"PortraitId":[1,2,4,5,6][i],"Description":"Authored biography %d"%i})
    write_json("user://original/catalog.json",integer_tokens(catalog))
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);check(language.save("user://interface.cfg")==OK,"Management locale")
        var v=complete_value();v.State.SecretarySourceIndex=1;v.State.SecretaryPolicies={"Production":2,"Break":2,"Return":0}
        v.State.HardwareLicenses=[];v.State.TechnologyOwnershipLevels={"0":3};v.State.ProductionPerformedThisWeek=false
        for employee in v.State.Employees:employee.TechnologyLevels={"0":0};employee.MaximumTechnologies=4
        var game=product();game.Recall={"OriginalState":1,"WeeksRemaining":0};game.ReviewPending=false;game.ReviewDisplayCallbacks=90;v.State.ReleasedGames=[game]
        await load_fixture(v);await press_node("TogglePause")
        await menu_choice("MENU_ACTIONS",8);await window("BankWindow",422,252)
        check(main.find_child("BankLoanDate2",true,false)!=null and main.find_child("BankLoanType4",true,false)!=null,"Three loan records and four borrowing choices")
        await close_window("BankWindow")
        await menu_choice("MENU_ACTIONS",11);await window("HardwareLicenceWindow",398,306)
        var table=main.find_child("HardwareLicenceTable",true,false)
        check(table.columns==4 and table.get_column_title(3)==tr("HARDWARE_MAKER"),"Licence company column")
        await press_node("ToolbarSAVE_GAME");var before_licence=saved().Money
        await press_node("BuySelectedLicence");await press_node("ToolbarSAVE_GAME")
        check(saved().HardwareLicenses.size()==1 and int(saved().HardwareLicenses[0])==0 and saved().Money==before_licence-20000000,"Licence charges once")
        check(main.find_child("BuySelectedLicence",true,false).disabled,"Owned licence cannot repeat purchase")
        await close_window("HardwareLicenceWindow")
        await menu_choice("MENU_INFORMATION",0);await window("StaffRosterWindow",450,394);await close_window("StaffRosterWindow")
        await select_employee(0);await press_node("EmployeeTechnologies")
        await office_input(MOUSE_BUTTON_RIGHT,Vector2(166,68));await press_node("EmployeeLearnTechnology");await window("TrainingWindow",281,270)
        check(main.find_child("TrainingCourses",true,false).columns==3,"Original learning columns")
        check(main.find_child("StudySeats",true,false)==null and main.find_child("ManagementSave",true,false)==null,"Learning dialog omits added seat and persistence controls")
        await press_node("TrainTechnology0");await press_node("ToolbarSAVE_GAME")
        check(saved().Employees[0].Office.Training!=null and saved().Employees[0].Experience==90,"Training saves cost and active course")
        await select_employee(0)
        check(main.find_child("EmployeeTechnologies",true,false).button_pressed,"Selected employee tab is pressed")
        await select_employee(1);check(main.find_child("EmployeeTechnologiesPage",true,false).visible,"Employee switch retains selected page")
        await close_window("EmployeeDetailsWindow")
        check(main.find_child("CompanyFacilities",true,false)==null and main.find_child("CompanyInformation",true,false)==null,"Company overview has no port-only navigation buttons")
        await menu_choice("MENU_ACTIONS",6);await window("SecretaryHiringWindow",398,291)
        table=main.find_child("SecretaryCandidates",true,false)
        check(table.columns==3 and table.get_column_title(1)==tr("CONTRACT_FEE"),"Secretary native three-column order")
        await close_window("SecretaryHiringWindow")
        await menu_choice("MENU_ACTIONS",5);await window("SecretaryPolicyWindow",428,332)
        check(main.find_child("SecretaryProductionFrame",true,false)!=null and main.find_child("SecretaryReturnFrame",true,false)!=null,"Boxed policy groups")
        check(main.find_child("SecretaryProduction2",true,false).button_pressed,"Saved policy selected")
        await close_window("SecretaryPolicyWindow")
        await menu_choice("MENU_INFORMATION",1)
        table=main.find_child("DesktopProductTable",true,false);table.get_root().get_first_child().select(0);table.item_activated.emit();await process_frame
        await press_node("CompactProduceMore");await window("AdditionalProductionWindow",414,358)
        check(main.find_child("ProductionRetailer",true,false)!=null and main.find_child("ProductionReadonlyPrice",true,false)!=null,"Reproduction includes distribution and readonly price")
        check(main.find_children("ProductionTotalCost","Label",true,false).size()==1,"Single reproduction total")
        await dispose()
    print("Management layout smoke failures: ",failures)
    quit(0 if failures==0 else 1)
