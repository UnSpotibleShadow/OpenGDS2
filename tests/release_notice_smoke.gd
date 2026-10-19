extends SceneTree
var failures:=0
var main: Node
func check(ok: bool,message: String) -> void:
    print(("PASS " if ok else "FAIL ")+message)
    if not ok:failures+=1
func _initialize() -> void:call_deferred("run")
func start() -> void:
    main=load("res://scenes/Main.tscn").instantiate();root.add_child(main)
    for frame in range(3):await process_frame
func stop() -> void:
    main.queue_free();await process_frame
func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    var config=ConfigFile.new();config.set_value("updates","enabled",true)
    config.set_value("updates","checked",int(Time.get_unix_time_from_system()))
    config.set_value("updates","cache",JSON.stringify([{"tag_name":"v0.1.0-ea.10","draft":false,"html_url":"https://github.com/UnSpotibleShadow/OpenGDS2/releases/tag/v0.1.0-ea.10"}]))
    check(config.save("user://updates.cfg")==OK,"Disposable release metadata cache")
    await start()
    check(main.find_child("ReleaseNotice",true,false)!=null,"New cached EA release has a startup notice")
    check(main.find_child("ReleaseNoticeText",true,false).text.contains("v0.1.0-ea.10"),"Notice identifies release version")
    check(main.find_child("ReleaseUpdateRequest",true,false)==null,"Cached and headless checks never contact GitHub")
    var notice=main.find_child("ReleaseNotice",true,false)
    var close=notice.find_children("*","Button",true,false)[-1];close.pressed.emit();await process_frame
    check(main.find_child("ReleaseNotice",true,false)==null,"Notice can be dismissed")
    await stop();await start()
    check(main.find_child("ReleaseNotice",true,false)==null,"Same release is not repeatedly announced")
    main.find_child("TitleSETTINGS",true,false).pressed.emit();await process_frame
    var toggle=main.find_child("UpdateChecksEnabled",true,false);toggle.button_pressed=false;await process_frame
    config.load("user://updates.cfg");check(not config.get_value("updates","enabled",true),"Update checks can be disabled durably")
    await stop();await start();check(main.find_child("ReleaseNotice",true,false)==null,"Disabled check keeps startup quiet");await stop()
    print("Release notice smoke failures: ",failures);quit(0 if failures==0 else 1)
