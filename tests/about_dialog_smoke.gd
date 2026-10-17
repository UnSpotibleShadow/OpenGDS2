extends "res://tests/native_layout_smoke.gd"

func settle_about() -> void:
    for frame in range(5):await process_frame

func open_about() -> void:
    var popup=main.find_child("MENU_HELP",true,false).get_popup()
    var target=-1
    for i in range(popup.item_count):
        if popup.get_item_text(i)==TranslationServer.translate("ABOUT_GAME"):target=popup.get_item_id(i)
    check(target>=0,"Help menu exposes localized About/version action")
    if target>=0:popup.id_pressed.emit(target)
    await settle_about()

func check_about(artwork: bool) -> void:
    await window("AboutWindow",308,328)
    check(main.find_child("AboutWindow",true,false).find_child("WindowTitleIcon",true,false).texture!=null,"About reuses its original window icon")
    var canvas=main.find_child("NativeDialogCanvas",true,false)
    check(canvas!=null and canvas.size==Vector2(300,296),"About keeps a compact client")
    if canvas==null:return
    for control in canvas.find_children("*","Control",true,false):
        if control.is_visible_in_tree():
            check(canvas.get_global_rect().grow(1).encloses(control.get_global_rect()),"About contains "+str(control.name))
    check(main.find_child("AboutPortName",true,false).text==ProjectSettings.get_setting("application/config/name"),"About identifies the port")
    check(main.find_child("AboutVersion",true,false).text.contains(ProjectSettings.get_setting("application/config/version")),"About reports current port version")
    check(main.find_child("AboutOriginalCredit",true,false).text.contains("2001–2008"),"Original copyright stays attributed")
    check((main.find_child("AboutArtwork",true,false).texture!=null)==artwork,"About uses local recovered artwork or fallback")
    check((main.find_child("AboutArtworkMissing",true,false)==null)==artwork,"Missing artwork has a bounded reimport hint")
    var link=main.find_child("AboutWebsite",true,false)
    var visit=main.find_child("VisitKairosoftWebsite",true,false)
    check(link is LinkButton and link.text=="https://kairosoft.net/" and not link.disabled,"Official website is a readable interactive link")
    check(not link.get_signal_connection_list("pressed").is_empty() and not visit.get_signal_connection_list("pressed").is_empty(),"Both original website controls have activation handlers")
    # Do not launch an external browser from an automated fixture.

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,720)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    blank_bmp("user://original/Graphic/Recovered/resource-31EDEC.bmp",250,171,false)
    var icon=Image.create(16,16,false,Image.FORMAT_RGBA8);icon.fill(Color.BLUE)
    icon.save_png("user://original/Graphic/Embedded/WindowIcon-fMainAbout.png")
    setup_catalog()
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);language.save("user://interface.cfg")
        var fixture=value()
        await load_fixture(fixture);await press_node("TogglePause")
        await open_about();await check_about(true)
        main.find_child("AboutWindow",true,false).find_child("CloseWindow",true,false).pressed.emit()
        await settle_about()
        check(main.find_child("AboutWindow",true,false)==null and main.find_child("OfficeWindow",true,false)!=null,"Closing About returns to the office")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Company.Name==fixture.State.Company.Name and saved().Money==fixture.State.Money,"About preserves company and funds")
        await open_about();await check_about(true)
        await dispose()
    DirAccess.remove_absolute(ProjectSettings.globalize_path("user://original/Graphic/Recovered/resource-31EDEC.bmp"))
    await load_fixture(value());await press_node("TogglePause")
    await open_about();await check_about(false);await dispose()
    print("About dialog smoke failures: ",failures)
    quit(0 if failures==0 else 1)
