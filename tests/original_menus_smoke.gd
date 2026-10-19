extends "res://tests/original_settings_smoke.gd"

func menu_shape(name: String,keys: Array) -> void:
    var node=main.find_child(name,true,false)
    var popup=node.get_popup() if node is MenuButton else node
    check(popup.item_count==keys.size(),"Menu item count: "+name)
    for i in range(min(popup.item_count,keys.size())):
        check(popup.is_item_separator(i) if keys[i]=="" else popup.get_item_text(i)==tr(keys[i]),"Menu ordering: "+name+" / "+str(i))

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,720)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true);setup_catalog()
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);language.save("user://interface.cfg")
        var fixture=complete_value();await load_fixture(fixture);main.call("SetSimulationPaused",true)
        var roots=main.find_children("*","MenuButton",true,false).map(func(menu):return str(menu.name))
        check(roots==["MENU_GAME","MENU_ACTIONS","MENU_INFORMATION","MENU_WINDOWS","MENU_SETTINGS","MENU_HELP"],"Six recovered top-level menus in original order")
        menu_shape("MENU_GAME",["NEW_GAME","","SAVE_GAME","LOAD_GAME","","QUIT","","BACK_TO_TITLE"])
        menu_shape("MENU_ACTIONS",["COMPANY_DEVELOPMENT","","HIRE_EMPLOYEES","POACH_EMPLOYEE","","SECRETARY_POLICIES","HIRE_SECRETARY","","BANK","DEVELOPMENT_COMPUTERS","BUY_BACKUP_DEVICE","HARDWARE_LICENSES","TECHNOLOGIES","GO_OUT"])
        menu_shape("OriginalCompanyDevelopmentMenu",["PUBLIC_RELATIONS","GENRE_RESEARCH","EXPAND_PRODUCTION"])
        menu_shape("OriginalGoOutMenu",["CASINO_HORSE","CASINO_POKER","CASINO_BLACKJACK"])
        menu_shape("MENU_INFORMATION",["STAFF","NATIVE_PRODUCTS","NATIVE_TOP100","NATIVE_PLATFORMS","","NATIVE_OWN_CONSOLES","","NATIVE_TIME_RECORDS"])
        menu_shape("MENU_WINDOWS",["OFFICE_WINDOW","COMPANY_WINDOW","OTHER_WINDOWS"])
        menu_shape("MENU_HELP",["GAME_HELP","","ABOUT_GAME"])
        var info=main.find_child("MENU_INFORMATION",true,false).get_popup();info.about_to_popup.emit()
        check(info.is_item_disabled(5),"Own-hardware report retains its availability guard")
        main.find_child("MENU_ACTIONS",true,false).get_popup().id_pressed.emit(8);await process_frame
        check(main.find_child("BankWindow",true,false)!=null,"Moved Bank action opens bank")
        main.find_child("BankWindow",true,false).find_child("CloseWindow",true,false).pressed.emit();await process_frame
        var staff=main.find_child("EmployeeWindow",true,false);staff.find_child("CloseWindow",true,false).pressed.emit()
        var other=main.find_child("OtherWindowsMenu",true,false);other.about_to_popup.emit()
        var staff_index=-1
        for i in range(other.item_count):
            if other.get_item_metadata(i)==staff.get_instance_id():staff_index=i
        check(staff_index>=0,"Window list includes closed staff window")
        var checked=0
        for i in range(other.item_count):
            check(other.get_item_text(i).begins_with(str(i+1)+" "),"Native numbered window titles")
            if other.is_item_checked(i):checked+=1
        check(checked==1,"Window list marks only the front window")
        other.id_pressed.emit(staff_index);await process_frame
        check(staff.visible,"Other windows retains employee-window reopening")
        main.find_child("MENU_INFORMATION",true,false).get_popup().id_pressed.emit(2);await process_frame
        check(main.find_child("SalesRankingWindow",true,false)!=null,"Information owns the sales-ranking report")
        main.find_child("SalesRankingWindow",true,false).find_child("CloseWindow",true,false).pressed.emit();await process_frame
        main.find_child("MENU_GAME",true,false).get_popup().id_pressed.emit(0);await process_frame
        check(main.find_child("CompanySetupWindow",true,false)!=null,"New Game is first and keeps its existing route")
        await press("CANCEL");await press_node("ToolbarSAVE_GAME")
        check(saved().Money==fixture.State.Money,"Browsing regrouped menus preserves funds")
        await dispose()
    print("Original menus smoke failures: ",failures);quit(0 if failures==0 else 1)
