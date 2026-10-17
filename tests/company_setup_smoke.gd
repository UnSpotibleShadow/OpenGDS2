extends "res://tests/native_layout_smoke.gd"

func settle_setup() -> void:
    for frame in range(5):await process_frame

func check_setup() -> void:
    await settle_setup()
    var canvas=main.find_child("NativeDialogCanvas",true,false)
    check(canvas!=null and canvas.size==Vector2(340,292),"Setup keeps its compact client size")
    if canvas==null:return
    check(Rect2(Vector2.ZERO,Vector2(root.size)).encloses(canvas.get_global_rect()),"Setup fits the application window")
    for control in canvas.find_children("*","Control",true,false):
        if control.is_visible_in_tree() and not control is PopupMenu:
            check(canvas.get_global_rect().grow(1).encloses(control.get_global_rect()),"Setup contains "+str(control.name))
    var company=main.find_child("CompanyNameInput",true,false)
    var president=main.find_child("PresidentNameInput",true,false)
    if company.size.y>25 or president.size.y>25 or company.position.x!=president.position.x:
        print("Setup input sizes ",company.size," / ",president.size," minima ",company.get_combined_minimum_size()," / ",president.get_combined_minimum_size())
    check(company.size.y<=25 and president.size.y<=25 and company.position.x==president.position.x,"Compact name fields align")
    check(company.max_length==32 and president.max_length==32,"Existing name limits remain")
    var confirm=main.find_child("ConfirmCompanySetup",true,false)
    var cancel=main.find_child("CancelCompanySetup",true,false)
    check(confirm.size.y<=25 and cancel.size.y<=25 and confirm.position.y==cancel.position.y,"Compact actions share one row")

func authored_database() -> Dictionary:
    var database={"SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6","MinimumSourceIndex":35,"SourceUpperBound":60,"NameComponents":[],"Employees":[]}
    for i in range(8):database.NameComponents.append({"Given":"Authored","Family":"Name"})
    for i in range(60):database.Employees.append({"SourceIndex":i,"Name":"Authored employee %d"%i,"RawAttributes":[1,1,1,1,1,1,1],"ContractFee":10})
    return database

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,720)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    blank_bmp("user://original/Graphic/Title.bmp",558,385,false)
    blank_bmp("user://original/Graphic/Recovered/resource-047345.bmp",256,160,true)
    setup_catalog()
    write_json("user://original/employees.json",authored_database())
    var marker=FileAccess.open("user://original/.imported",FileAccess.WRITE);marker.store_string("Authored setup fixture");marker.close()
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);language.save("user://interface.cfg")
        var fixture_value=value()
        await load_fixture(fixture_value);await press_node("TogglePause")
        main.find_child("MENU_GAME",true,false).get_popup().id_pressed.emit(2)
        await check_setup();await window("CompanySetupWindow",348,324)
        check(main.find_child("CompanySetupArtwork",true,false).texture!=null,"Setup reuses recovered registration artwork")
        await press_node("ConfirmCompanySetup")
        check(not main.find_child("CompanySetupValidation",true,false).text.is_empty(),"Empty names show an inline message")
        await check_setup()
        var company="Community "+"X".repeat(22)
        var president="Chair "+"Y".repeat(26)
        main.find_child("CompanyNameInput",true,false).text=company
        main.find_child("PresidentNameInput",true,false).text=president
        await press_node("ConfirmCompanySetup");await press("CANCEL")
        await check_setup()
        check(main.find_child("CompanyNameInput",true,false).text==company and main.find_child("PresidentNameInput",true,false).text==president,"Returning from mode selection preserves both drafts")
        await press_node("CancelCompanySetup");await press_node("ToolbarSAVE_GAME")
        check(saved().Company.Name==fixture_value.State.Company.Name and saved().Money==fixture_value.State.Money,"Cancel preserves the active company and funds")
        main.find_child("MENU_GAME",true,false).get_popup().id_pressed.emit(2);await settle_setup()
        main.find_child("CompanySetupWindow",true,false).find_child("CloseWindow",true,false).pressed.emit()
        await settle_setup()
        check(main.find_child("CompanySetupWindow",true,false)==null and main.find_child("OfficeWindow",true,false)!=null,"Window close cancels the in-game draft")
        main.call("ShowTitleScreen");await settle_setup()
        await press("NEW_GAME");await check_setup()
        await press_node("CancelCompanySetup")
        check(main.find_child("TitlePrimaryActions",true,false)!=null,"Title setup cancels back to the title")
        await dispose()
    DirAccess.remove_absolute(ProjectSettings.globalize_path("user://original/Graphic/Recovered/resource-047345.bmp"))
    root.size=Vector2i(1280,1016)
    main=load("res://scenes/Main.tscn").instantiate();root.add_child(main);await settle_setup()
    await press("NEW_GAME");await check_setup()
    check(main.find_child("CompanySetupArtworkMissing",true,false)!=null,"Missing registration art keeps a compact usable form")
    await press_node("CancelCompanySetup");await dispose()
    print("Company setup smoke failures: ",failures)
    quit(0 if failures==0 else 1)
