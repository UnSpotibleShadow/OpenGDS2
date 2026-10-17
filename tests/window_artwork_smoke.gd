extends "res://tests/review_round_smoke.gd"

func menu_choice(menu: String,index: int) -> void:
    var target=main.find_child(menu,true,false)
    var popup=target.get_popup() if target is MenuButton else target
    popup.id_pressed.emit(index)
    for frame in range(4):await process_frame

func close_window(name: String) -> void:
    main.find_child(name,true,false).find_child("CloseWindow",true,false).pressed.emit()
    for frame in range(4):await process_frame

func select_employee(index: int) -> void:
    var table=main.find_child("DesktopEmployeeTable",true,false)
    var row=table.get_root().get_first_child()
    for i in range(index):row=row.get_next()
    row.select(0);table.item_activated.emit()
    for frame in range(4):await process_frame

func png(path: String,size: Vector2i,color: Color) -> void:
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(path.get_base_dir()))
    var art=Image.create(size.x,size.y,false,Image.FORMAT_RGBA8);art.fill(color)
    check(art.save_png(path)==OK,"Authored PNG fixture")

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,800)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    setup_catalog()
    var catalog=JSON.parse_string(FileAccess.get_file_as_string("user://original/catalog.json"))
    catalog.Audiences[0].Identifier=1;catalog.Hardware[0].RawMembers["1050"]=80
    write_json("user://original/catalog.json",integer_tokens(catalog))
    for form in ["fGMHLicense","fGMToutatuRank","fGMRank100","fGMSyain","fGMGameSave","fGMGameLoad"]:
        if not FileAccess.file_exists("user://original/Graphic/Embedded/WindowIcon-"+form+".png"):
            png("user://original/Graphic/Embedded/WindowIcon-"+form+".png",Vector2i(16,16),Color.BLUE)
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);check(language.save("user://interface.cfg")==OK,"Window/artwork locale")
        png("user://original/Graphic/Embedded/WindowIcon-fGMBank.png",Vector2i(16,16),Color.BLUE)
        png("user://asset-overrides/Graphic/Embedded/WindowIcon-fGMBank.png",Vector2i(16,16),Color.MAGENTA)
        var v=complete_value();var game=product();game.Recall={"OriginalState":1,"WeeksRemaining":0};game.ReviewPending=false;game.ReviewDisplayCallbacks=90;v.State.ReleasedGames=[game];v.State.ProductionPerformedThisWeek=false
        await load_fixture(v)
        await press_node("TogglePause")
        await menu_choice("MENU_ACTIONS",8)
        var bank=main.find_child("BankWindow",true,false)
        var icon=bank.find_child("WindowTitleIcon",true,false)
        check(icon.visible and icon.texture.get_image().get_pixel(0,0).is_equal_approx(Color.MAGENTA),"Native PNG override takes precedence")
        await close_window("BankWindow")
        png("user://asset-overrides/Graphic/Embedded/WindowIcon-fGMBank.png",Vector2i(32,32),Color.GREEN)
        await menu_choice("MENU_ACTIONS",8)
        icon=main.find_child("BankWindow",true,false).find_child("WindowTitleIcon",true,false)
        check(icon.texture.get_size()==Vector2(16,16) and icon.texture.get_image().get_pixel(0,0).is_equal_approx(Color.BLUE),"Wrong-sized PNG override falls back to original")
        await close_window("BankWindow")
        for screen in [["MENU_ACTIONS",11,"HardwareLicenceWindow"],["MENU_INFORMATION",7,"ReachTimeRecordsWindow"],["MENU_INFORMATION",2,"SalesRankingWindow"],["MENU_INFORMATION",0,"StaffRosterWindow"],["MENU_GAME",2,"SaveSlotWindow"],["MENU_GAME",3,"LoadSlotWindow"]]:
            await menu_choice(screen[0],screen[1]);var dialog=main.find_child(screen[2],true,false)
            check(dialog!=null,"Stable named screen: "+screen[2])
            check(dialog.find_child("WindowTitleIcon",true,false).texture!=null,"Bound native title icon: "+screen[2])
            dialog.position=Vector2(310,100);await close_window(screen[2])
            await menu_choice(screen[0],screen[1]);check(main.find_child(screen[2],true,false).position==Vector2(310,100),"Reopen retains position: "+screen[2]);await close_window(screen[2])
        await select_employee(0)
        var detail=main.find_child("EmployeeDetailsWindow",true,false);detail.position=Vector2(750,170)
        if not OS.get_environment("OPENGDS2_PRIVATE_NATIVE_ART").is_empty():
            check(main.find_child("EmployeeLevelUp",true,false).icon!=null,"Recovered small Level Up glyph")
            var abilities=main.find_child("EmployeeAbilities",true,false)
            check(abilities.get_theme_stylebox("normal") is StyleBoxTexture and abilities.get_theme_stylebox("normal").texture.get_size()==Vector2(39,16),"Recovered tab skin with localized caption")
            check(abilities.get_theme_stylebox("normal").get_content_margin(SIDE_LEFT)==abilities.get_theme_stylebox("hover").get_content_margin(SIDE_LEFT),"Tab hover preserves content position")
        await select_employee(1)
        check(main.find_child("EmployeeDetailsWindow",true,false).position==Vector2(750,170),"Switching employees retains detail position")
        await press_node("EmployeeLevelUp")
        await close_window("LevelUpResultWindow")
        await select_employee(1)
        check(main.find_child("EmployeeDetailsWindow",true,false).position==Vector2(750,170),"Returning from Level Up retains detail position")
        await close_window("EmployeeDetailsWindow")
        await menu_choice("MENU_INFORMATION",1)
        var table=main.find_child("DesktopProductTable",true,false);table.get_root().get_first_child().select(0);table.item_activated.emit();await process_frame
        await press_node("CompactProduceMore")
        check(main.find_child("AdditionalProductionWindow",true,false)!=null,"Reproduction has its own stable identity")
        var production=main.find_child("AdditionalProductionWindow",true,false);production.position=Vector2(300,110)
        main.find_child("ProductionCopies",true,false).value=2300
        await press("MEDIA_INFORMATION")
        await press_node("CloseMediaInformation")
        check(main.find_child("AdditionalProductionWindow",true,false).position==Vector2(300,110),"Media detour retains reproduction position")
        check(main.find_child("ProductionCopies",true,false).value==2300,"Media detour retains draft quantity")
        await dispose()
        DirAccess.remove_absolute(ProjectSettings.globalize_path("user://asset-overrides/Graphic/Embedded/WindowIcon-fGMBank.png"))
    print("Window/artwork smoke failures: ",failures)
    quit(0 if failures==0 else 1)
