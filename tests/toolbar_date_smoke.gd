extends "res://tests/window_artwork_smoke.gd"

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,800);setup_catalog()
    var private_art=OS.get_environment("OPENGDS2_PRIVATE_NATIVE_ART")
    if not private_art.is_empty():
        check(DirAccess.copy_absolute(private_art.path_join("office.json"),ProjectSettings.globalize_path("user://original/office.json"))==OK,"Original office scene fixture")
    var english_plate: Image
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);check(language.save("user://interface.cfg")==OK,"Date locale")
        var v=complete_value();v.State.Year=3 if locale=="en" else 4;v.State.Month=12;v.State.Week=4
        await load_fixture(v);check(main.call("SetSimulationPaused",true),"Freeze date fixture")
        var year=main.find_child("DesktopFunds",true,false)
        check(year.text.begins_with(tr("TOOLBAR_YEAR").format([v.State.Year],"{_}")),"Toolbar shows current year before funds")
        await press_node("ToolbarSAVE_GAME");await press_node("ToolbarLOAD_GAME")
        check(main.find_child("DesktopFunds",true,false).text.begins_with(tr("TOOLBAR_YEAR").format([v.State.Year],"{_}")),"Year survives reload")
        if not private_art.is_empty():
            var office=main.find_child("OriginalOfficeBackground",true,false).texture.get_image();office.convert(Image.FORMAT_RGB8)
            var plate=office.get_region(Rect2i(336,16,122,36))
            if locale=="en":english_plate=plate
            else:
                check(english_plate.get_data()!=plate.get_data(),"English office date labels differ from original Japanese")
                var outside_preserved=true
                for y in range(36):
                    for x in range(122):
                        var px=x+336;var py=y+16
                        var label_area=(px>=378 and px<400 and py>=22 and py<47) or (px>=422 and px<447 and py>=22 and py<47)
                        if not label_area and english_plate.get_pixel(x,y)!=plate.get_pixel(x,y):outside_preserved=false
                check(outside_preserved,"Date frame and numeric pixels preserved")
            var captures=OS.get_environment("OPENGDS2_PRIVATE_UI_CAPTURE")
            if not captures.is_empty():
                DirAccess.make_dir_recursive_absolute(captures)
                check(plate.save_png(captures.path_join(locale+"-office-date.png"))==OK,"Office date preview")
                await process_frame;await process_frame
                check(root.get_texture().get_image().get_region(Rect2i(0,24,1280,30)).save_png(captures.path_join(locale+"-toolbar-year.png"))==OK,"Year toolbar preview")
        await dispose()
    print("Toolbar/date smoke failures: ",failures);quit(0 if failures==0 else 1)
