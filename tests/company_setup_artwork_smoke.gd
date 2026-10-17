extends "res://tests/company_setup_smoke.gd"

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,720)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    blank_bmp("user://original/Graphic/Title.bmp",558,385,false)
    var resource="user://original/Graphic/Recovered/resource-047345.bmp"
    blank_bmp(resource,256,160,true)
    var private_art=OS.get_environment("OPENGDS2_PRIVATE_REGISTRATION")
    if not private_art.is_empty():
        check(DirAccess.copy_absolute(private_art,ProjectSettings.globalize_path(resource))==OK,"Private registration fixture copied locally")
    var original=Image.load_from_file(ProjectSettings.globalize_path(resource));original.convert(Image.FORMAT_RGB8)
    setup_catalog();write_json("user://original/employees.json",authored_database())
    var marker=FileAccess.open("user://original/.imported",FileAccess.WRITE);marker.store_string("Authored registration fixture");marker.close()
    var english: PackedByteArray
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);language.save("user://interface.cfg")
        await load_fixture(value());await press_node("TogglePause")
        main.find_child("MENU_GAME",true,false).get_popup().id_pressed.emit(0);await check_setup()
        var actual=main.find_child("CompanySetupArtwork",true,false).texture.get_image();actual.convert(Image.FORMAT_RGB8)
        check(actual.get_size()==Vector2i(256,160),"Registration patch keeps native dimensions")
        if locale=="en" and not private_art.is_empty():
            english=actual.get_data();check(english!=original.get_data(),"English patches the verified original registration image")
            var outside=0
            for y in range(160):
                for x in range(256):
                    if not (x>=1 and x<63 and y>=76 and y<92) and not (x>=166 and x<251 and y>=132 and y<159):
                        if actual.get_pixel(x,y)!=original.get_pixel(x,y):outside+=1
            check(outside==0,"Registration scene outside both captions is pixel-identical")
            var output=OS.get_environment("OPENGDS2_REGISTRATION_CAPTURE")
            if not output.is_empty():
                DirAccess.make_dir_recursive_absolute(output)
                actual.save_png(output.path_join("english-registration.png"))
                actual.resize(1024,640,Image.INTERPOLATE_NEAREST)
                actual.save_png(output.path_join("english-registration-4x.png"))
        else:check(actual.get_data()==original.get_data(),"Japanese or unrecognized artwork stays unchanged")
        await press_node("CancelCompanySetup")
        main.call("ShowTitleScreen");await settle_setup();await press("NEW_GAME");await check_setup()
        var title_art=main.find_child("CompanySetupArtwork",true,false).texture.get_image();title_art.convert(Image.FORMAT_RGB8)
        check(title_art.get_data()==(english if locale=="en" and not private_art.is_empty() else original.get_data()),"Title and office routes use identical registration artwork")
        await dispose()
    # Both supported aliases use the same patch, and explicit overlays take priority.
    var alias="user://original/Graphic/Embedded/CompanySetup.bmp"
    DirAccess.copy_absolute(ProjectSettings.globalize_path(resource),ProjectSettings.globalize_path(alias))
    var language=ConfigFile.new();language.set_value("language","locale","en");language.save("user://interface.cfg")
    await load_fixture(value());await press_node("TogglePause")
    main.find_child("MENU_GAME",true,false).get_popup().id_pressed.emit(0);await settle_setup()
    var alias_art=main.find_child("CompanySetupArtwork",true,false).texture.get_image();alias_art.convert(Image.FORMAT_RGB8)
    check(alias_art.get_data()==(english if not private_art.is_empty() else original.get_data()),"Semantic alias matches recovered registration binding")
    await dispose()
    var overlay="user://asset-overrides/Graphic/Embedded/CompanySetup.bmp";blank_bmp(overlay,256,160,false)
    var expected=Image.load_from_file(ProjectSettings.globalize_path(overlay));expected.convert(Image.FORMAT_RGB8)
    await load_fixture(value());await press_node("TogglePause")
    main.find_child("MENU_GAME",true,false).get_popup().id_pressed.emit(0);await settle_setup()
    var shown=main.find_child("CompanySetupArtwork",true,false).texture.get_image();shown.convert(Image.FORMAT_RGB8)
    check(shown.get_data()==expected.get_data(),"Explicit registration artwork overrides are respected")
    await dispose()
    print("Registration artwork smoke failures: ",failures);quit(0 if failures==0 else 1)
