extends "res://tests/casino_layout_smoke.gd"

func resize_list(name: String) -> void:
    var dialog=main.find_child(name,true,false)
    var grip=dialog.find_child("VerticalResizeHandle",true,false)
    check(grip!=null,"Vertical resize handle: "+name)
    var before=dialog.size
    Input.warp_mouse(grip.global_position+Vector2(30,2));await process_frame
    var down=InputEventMouseButton.new();down.button_index=MOUSE_BUTTON_LEFT;down.pressed=true
    grip.gui_input.emit(down)
    Input.warp_mouse(grip.global_position+Vector2(30,102));await process_frame
    var motion=InputEventMouseMotion.new();Input.parse_input_event(motion);await process_frame
    var up=InputEventMouseButton.new();up.button_index=MOUSE_BUTTON_LEFT;up.pressed=false;Input.parse_input_event(up)
    for i in range(4):await process_frame
    check(dialog.size.y>before.y+50 and dialog.size.x==before.x,"Drag increases list height only: "+name)

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,800);blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true);setup_catalog()
    var paragraphs=[]
    for topic in range(5):paragraphs.append({"Topic":topic,"Top":0,"Text":"Authored native Help page "+str(topic)})
    write_json("user://original/help.json",{"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6","Paragraphs":paragraphs})
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);check(language.save("user://interface.cfg")==OK,"Review locale")
        var v=complete_value()
        v.State.DevelopmentProjects=stress_fixture().State.DevelopmentProjects
        v.State.DevelopmentProjects[0].RequiredPoints=100000
        v.State.DevelopmentProjects[0].Progress=0
        v.State.DevelopmentProjects[0].Bugs=6
        var older=product();older.Title="Earlier release";older.ProductIndex=0;older.ReviewPending=false;older.ReviewDisplayCallbacks=90;older.Recall={"OriginalState":1,"WeeksRemaining":0}
        var newer=older.duplicate(true);newer.Title="Latest release";newer.ProductIndex=1
        v.State.ReleasedGames=[older,newer]
        await load_fixture(v);await press_node("TogglePause")
        var products=main.find_child("DesktopProductTable",true,false)
        check(products.get_root().get_first_child().get_text(0)=="Latest release","Products newest first")
        check(not products.scroll_horizontal_enabled,"Products has no horizontal scroll")
        await resize_list("ProductWindow");await resize_list("EmployeeWindow")
        await press_node("OpenProject0")
        await select_employee(0)
        check(main.find_child("EmployeeDetailsWindow",true,false)!=null,"Roster opens employee while live project report is open")
        await close_window("EmployeeDetailsWindow")
        await menu_choice("MENU_HELP",0)
        for topic in range(5):
            var selector=main.find_child("HelpTopicSelector",true,false);selector.select(topic);selector.item_selected.emit(topic)
            await window("HelpTopicWindow",425,405)
            var body=main.find_child("HelpTopicBody",true,false)
            check(body.text.length()>20,"Help page has content")
            if locale=="en":check(main.find_child("HelpPortNotes",true,false)!=null,"Port notes separate from original Help")
            if topic==3:check(main.find_child("OriginalHelpWebsite",true,false).text=="http://www.bj.wakwak.com/~gamedev2/","Original historical reference retained")
        await close_window("HelpTopicWindow")
        await menu_choice("MENU_COMPANY",0);await window("CompanyActivitiesWindow",410,360)
        await press_node("CompanyActivity0");await window("ActivityConfirmationWindow",310,320)
        check(main.find_child("CompanyActivityArtwork",true,false).texture!=null,"Original company activity illustration")
        var original_art=Image.load_from_file(ProjectSettings.globalize_path("user://original/Graphic/Embedded/CompanyActivity.bmp"));original_art.convert(Image.FORMAT_RGB8)
        var shown_art=main.find_child("CompanyActivityArtwork",true,false).texture.get_image();shown_art.convert(Image.FORMAT_RGB8)
        check((shown_art.get_data()!=original_art.get_data())==(locale=="en"),"English captions patched; Japanese artwork preserved")
        # Roster must still respect a pending decision.
        await select_employee(0);check(main.find_child("EmployeeDetailsWindow",true,false)==null,"Pending confirmation blocks roster")
        await press_node("CancelCompanyActivity");await close_window("CompanyActivitiesWindow");await dispose()
        await load_fixture(casino_value());await press_node("TogglePause")
        await press_node("CompanyFacilities");await press_node("EnterCasino0")
        await window("CasinoSelector",235,310)
        await press_node("CasinoBuyCoins");await window("CasinoCoinPurchase",310,190)
        await press_node("CancelCasinoCoins");await press_node("CasinoPoker")
        check(main.find_child("CasinoPokerWindow",true,false)!=null,"Compact selector retains game route")
        await dispose()
    print("Review112 smoke failures: ",failures);quit(0 if failures==0 else 1)
