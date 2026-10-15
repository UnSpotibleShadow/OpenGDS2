extends "res://tests/window_artwork_smoke.gd"

func finish_up_result() -> void:
    for page in range(20):
        if main.find_child("ConversationWindow",true,false)==null:return
        await advance_page()
    check(false,"UP result conversation completes")

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,800);blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true);setup_catalog()
    var keys=["SAVE_GAME","LOAD_GAME","PUBLIC_RELATIONS","GENRE_RESEARCH","EXPAND_PRODUCTION","HIRE_EMPLOYEES","TECHNOLOGIES","BANK","DEVELOPMENT_COMPUTERS","HARDWARE_LICENSES","HIRE_SECRETARY","STAFF","SALES","SALES_CHART","GAME_HELP"]
    var icons=[15,14,2,1,0,13,12,11,10,9,8,7,6,5,3]
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);check(language.save("user://interface.cfg")==OK,"Toolbar locale")
        var v=complete_value();await load_fixture(v);check(main.call("SetSimulationPaused",true),"Freeze authored toolbar fixture")
        var bar=main.find_child("DesktopToolbar",true,false)
        var buttons=[]
        for child in bar.get_children():
            if child is Button:buttons.append(child)
        check(buttons.size()==keys.size(),"Native toolbar command count")
        for i in range(min(buttons.size(),keys.size())):
            var button=buttons[i]
            check(button.name=="Toolbar"+keys[i],"Native command order: "+keys[i])
            check(button.size.x==23 and button.size.y==22,"Native button dimensions: "+keys[i])
            if not OS.get_environment("OPENGDS2_PRIVATE_NATIVE_ART").is_empty():
                var original=Image.load_from_file(ProjectSettings.globalize_path("user://original/Graphic/Embedded/Toolbar%d.bmp"%icons[i]));original.convert(Image.FORMAT_RGB8)
                var shown=button.icon.get_image();shown.convert(Image.FORMAT_RGB8)
                check(original.get_data()==shown.get_data(),"Recovered bitmap association: "+keys[i])
            var before=button.size;button.mouse_entered.emit();await process_frame
            check(button.size==before,"Hover preserves toolbar geometry")
            button.mouse_exited.emit()
        for key in ["IMPORT_BUTTON","SETTINGS","CALENDAR_INSPECTOR","COMPANY_ACTIVITIES","DEVELOPMENT_PLANNING","PAUSE","RESUME"]:
            for menu in main.find_children("*","MenuButton",true,false):
                var popup=menu.get_popup()
                for item in range(popup.item_count):check(popup.get_item_text(item)!=tr(key),"No port-only menu command: "+key)
        check(main.find_child("TogglePause",true,false)==null and main.find_child("ToolbarBACK_TO_TITLE",true,false)==null,"No authored extras on toolbar")
        var funds=saved().Money
        for key in ["PUBLIC_RELATIONS","GENRE_RESEARCH","EXPAND_PRODUCTION"]:
            await press_node("Toolbar"+key)
            check(main.find_child("ActivityConfirmationWindow",true,false)!=null,"UP opens direct original confirmation: "+key)
            check(main.find_child("CompanyActivitiesWindow",true,false)==null,"No port overview detour")
            await press_node("CancelCompanyActivity")
            await press_node("ToolbarSAVE_GAME");check(saved().Money==funds and saved().PendingCompanyActivity==null,"Cancelling UP action preserves funds")
        await press_node("ToolbarBANK");check(main.find_child("BankWindow",true,false)!=null,"Native Bank shortcut")
        await close_window("BankWindow")
        await press_node("ToolbarGAME_HELP");check(main.find_child("HelpTopicWindow",true,false)!=null,"Native Help shortcut")
        await close_window("HelpTopicWindow")
        var captures=OS.get_environment("OPENGDS2_PRIVATE_UI_CAPTURE")
        if not captures.is_empty():
            DirAccess.make_dir_recursive_absolute(captures)
            await process_frame;await process_frame
            check(root.get_texture().get_image().get_region(Rect2i(0,24,1280,30)).save_png(captures.path_join(locale+"-native-toolbar.png"))==OK,"Native toolbar preview")
        await press_node("ToolbarPUBLIC_RELATIONS");await press_node("ConfirmCompanyActivity")
        await finish_up_result();await press_node("ToolbarSAVE_GAME")
        check(saved().Money==funds-v.State.Company.ActivityCost and saved().PendingCompanyActivity==null,"Direct UP confirmation charges once and returns to office")
        for key in ["PUBLIC_RELATIONS","GENRE_RESEARCH","EXPAND_PRODUCTION"]:check(main.find_child("Toolbar"+key,true,false).disabled,"Shared weekly budget disables all three UP actions")
        await press_node("ToolbarLOAD_GAME");await press_node("ToolbarSAVE_GAME")
        check(saved().Money==funds-v.State.Company.ActivityCost and main.find_child("ToolbarPUBLIC_RELATIONS",true,false).disabled,"Direct UP action survives reload without repeat charge")
        await dispose()
    print("Native toolbar smoke failures: ",failures);quit(0 if failures==0 else 1)
