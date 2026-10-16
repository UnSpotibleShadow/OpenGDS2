extends SceneTree

var failures:=0
var main: Node

func check(ok: bool,message: String) -> void:
    print(("PASS " if ok else "FAIL ")+message)
    if not ok:failures+=1

func start() -> void:
    main=load("res://scenes/Main.tscn").instantiate();root.add_child(main);await process_frame
    check(root.title=="Open Game Dev Story II","Exact friendly game-window title")
    check(ProjectSettings.get_setting("dotnet/project/assembly_name")=="OpenGDS2","Managed assembly identity unchanged")
    check(ProjectSettings.get_setting("application/config/custom_user_dir_name")=="OpenGDS2","Permanent save/import folder unchanged")
    check(main.find_child("BuildVersion",true,false).text.contains(str(ProjectSettings.get_setting("application/config/version"))),"Version remains visible in-game")

func desktop_phase(index: int,expected: Image) -> void:
    var folder=OS.get_environment("OPENGDS2_IDENTITY_CAPTURE")
    if folder.is_empty():return
    DirAccess.make_dir_recursive_absolute(folder)
    await process_frame;await RenderingServer.frame_post_draw
    check(expected.save_png(folder.path_join("expected-%d.png"%index))==OK,"Private icon reference")
    var metadata={"phase":index,"window":DisplayServer.window_get_native_handle(DisplayServer.WINDOW_HANDLE,root.get_window_id())}
    var file=FileAccess.open(folder.path_join("phase-%d.json"%index),FileAccess.WRITE);file.store_string(JSON.stringify(metadata));file.close()
    var deadline=Time.get_ticks_msec()+30000
    while not FileAccess.file_exists(folder.path_join("ack-%d.json"%index)) and Time.get_ticks_msec()<deadline:await process_frame
    check(FileAccess.file_exists(folder.path_join("ack-%d.json"%index)),"Desktop property check completed")
    if FileAccess.file_exists(folder.path_join("ack-%d.json"%index)):
        var ack=JSON.parse_string(FileAccess.get_file_as_string(folder.path_join("ack-%d.json"%index)))
        check(ack.ok,"Actual desktop window has the requested title and icon, phase "+str(index))

func _initialize() -> void:call_deferred("run")

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    var alias="user://original/Graphic/Embedded/WindowIcon-fGMGameKaishi.png"
    check(not FileAccess.file_exists(alias),"First phase starts without an original import")
    await start()
    var fallback=load("res://assets/application-icon.svg").get_image()
    await desktop_phase(0,fallback)
    var private_icon=OS.get_environment("OPENGDS2_PRIVATE_APPLICATION_ICON")
    if not private_icon.is_empty():
        DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(alias.get_base_dir()))
        check(DirAccess.copy_absolute(private_icon,ProjectSettings.globalize_path(alias))==OK,"Copy private original application icon fixture")
        main.call("ShowTitleScreen");await process_frame
        var original=Image.load_from_file(private_icon)
        await desktop_phase(1,original)
        main.queue_free();await process_frame
        TranslationServer.set_locale("ja")
        await start();await desktop_phase(2,original)
    main.queue_free();await process_frame
    print("Application identity smoke failures: ",failures);quit(0 if failures==0 else 1)
