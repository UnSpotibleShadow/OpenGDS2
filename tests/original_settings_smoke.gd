extends "res://tests/review_round_smoke.gd"

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,800);blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true);setup_catalog()
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);check(language.save("user://interface.cfg")==OK,"Original settings locale")
        var v=complete_value();v.State.Money=123456789
        await load_fixture(v);check(main.call("SetSimulationPaused",true),"Freeze settings fixture")
        var settings=main.find_child("MENU_SETTINGS",true,false)
        check(settings!=null and settings.get_popup().item_count==3,"Original settings has Sound, Graphics and Numbers submenus")
        var sound=main.find_child("OriginalSoundMenu",true,false)
        sound.id_pressed.emit(1);sound.about_to_popup.emit()
        check(sound.is_item_checked(1),"Original Sound Off updates effects setting")
        var audio=ConfigFile.new();audio.load("user://audio.cfg");check(not audio.get_value("effects","enabled",true),"Sound setting is durable")
        sound.id_pressed.emit(0)
        var graphics=main.find_child("OriginalGraphicsMenu",true,false)
        graphics.id_pressed.emit(2);await process_frame
        check(main.find_child("OriginalOfficeBackground",true,false).custom_minimum_size==Vector2(932,580),"Enlarged office uses two native pixels per pixel")
        check(main.find_child("OfficeViewport",true,false)!=null,"Enlarged office has a bounded scroll viewport")
        var numbers=main.find_child("OriginalNumberMenu",true,false)
        numbers.id_pressed.emit(0);await process_frame
        check(main.find_child("DesktopFunds",true,false).text.ends_with("¥1億2345万6789"),"Japanese grouping reaches actual funds display")
        await press_node("ToolbarSAVE_GAME");check(saved().Money==123456789,"Display changes do not alter saved funds")
        check(saved().NumberDisplayGrouping==0 and saved().OfficeGraphicsEnlarged,"Native display choices are part of company save state")
        await press_node("ToolbarLOAD_GAME")
        check(main.find_child("OriginalOfficeBackground",true,false).custom_minimum_size==Vector2(932,580),"Reload keeps graphic preference")
        check(main.find_child("DesktopFunds",true,false).text.ends_with("¥1億2345万6789"),"Reload keeps grouping preference")
        numbers=main.find_child("OriginalNumberMenu",true,false);numbers.id_pressed.emit(1)
        check(main.find_child("DesktopFunds",true,false).text.ends_with("¥123,456,789"),"Thousands setting uses original comma grouping")
        graphics=main.find_child("OriginalGraphicsMenu",true,false);graphics.id_pressed.emit(1);await process_frame
        check(main.find_child("OriginalOfficeBackground",true,false).custom_minimum_size==Vector2(466,290),"Normal restores native office resolution")
        var go=main.find_child("OriginalGoOutMenu",true,false)
        check(go!=null and go.item_count==3,"Go out exposes the three original direct games")
        var game=main.find_child("MENU_GAME",true,false).get_popup()
        check(game.get_item_text(0)==tr("NEW_GAME") and game.get_item_text(5)==tr("QUIT"),"In-game New Game and Exit routes are present")
        var office_position=main.find_child("OfficeWindow",true,false).position
        game.id_pressed.emit(0);await process_frame;await press("CANCEL");await press_node("ToolbarSAVE_GAME")
        check(saved().Money==123456789 and main.find_child("OfficeWindow",true,false).position==office_position,"Cancelling in-game New Game retains the company and window position")
        await dispose()
    print("Original settings smoke failures: ",failures);quit(0 if failures==0 else 1)
