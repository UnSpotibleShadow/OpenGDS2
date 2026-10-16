extends SceneTree

var main: Node
var failures:=0
var legacy: String

func check(ok: bool,message: String) -> void:
    print(("PASS " if ok else "FAIL ")+message)
    if not ok:failures+=1

func write(path: String,text: String) -> void:
    DirAccess.make_dir_recursive_absolute(path.get_base_dir())
    var file=FileAccess.open(path,FileAccess.WRITE);file.store_string(text);file.close()

func start() -> void:
    main=load("res://scenes/Main.tscn").instantiate();root.add_child(main);current_scene=main
    await process_frame

func dispose() -> void:
    current_scene=null;main.queue_free();main=null;await process_frame

func _initialize() -> void:call_deferred("run")

func run() -> void:
    if not OS.get_environment("XDG_DATA_HOME").contains("stable-user-data-ui-state"):
        push_error("Migration smoke requires its own disposable SDK state");quit(1);return
    var folder=ProjectSettings.globalize_path("user://").trim_suffix("/")
    check(folder.get_file()=="OpenGDS2","Actual application root uses the permanent name")
    legacy=folder.get_base_dir().path_join("OpenGDS2-EarlyAccessTesting")
    check(not FileAccess.file_exists(folder.path_join(".legacy-data-migration-v1.complete")),"Fresh test profile has not migrated yet")
    var save={"Format":"OpenGDS2","SchemaVersion":1,"State":{"Money":123456,"Year":1,"Month":4,"Week":1,"Company":{"Name":"Legacy company","PresidentName":"Tester"}}}
    var settings=ConfigFile.new();settings.set_value("language","locale","ja")
    DirAccess.make_dir_recursive_absolute(legacy);settings.save(legacy.path_join("interface.cfg"))
    settings=ConfigFile.new();settings.set_value("music","enabled",false);settings.set_value("music","volume",17);settings.set_value("effects","enabled",false);settings.save(legacy.path_join("audio.cfg"))
    write(legacy.path_join("saves/slot1.json"),JSON.stringify(save))
    write(legacy.path_join("saves/slot1.json.bak"),JSON.stringify(save))
    write(legacy.path_join("original/fixture.txt"),"Authored import fixture")
    await start()
    check(TranslationServer.get_locale()=="ja","Legacy language is applied on first startup")
    check(not main.find_child("BackgroundMusic",true,false).get("Enabled"),"Legacy mute setting is applied on first startup")
    check(main.find_child("OriginalSoundEffects",true,false).get("Enabled")==false,"Legacy effect setting is applied on first startup")
    check(FileAccess.get_file_as_string("user://original/fixture.txt")=="Authored import fixture","Original cache copied without a new import")
    check(FileAccess.get_file_as_string("user://saves/slot1.json")==JSON.stringify(save),"Current save copied exactly")
    check(FileAccess.file_exists("user://saves/slot1.json.bak"),"Previous save copied alongside current save")
    for button in main.find_children("*","Button",true,false):
        if button.text==tr("LOAD_GAME"):button.pressed.emit();break
    await process_frame
    check(main.find_child("DesktopFunds",true,false)!=null,"Migrated save loads into the office")
    main.call("SaveDesktop");await process_frame
    check(JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json")).State.Money==123456,"Migrated company funds retained")
    check(FileAccess.get_file_as_string(legacy.path_join("saves/slot1.json"))==JSON.stringify(save),"Legacy save remains intact")
    await dispose()
    write("user://saves/slot1.json","New destination bytes")
    await start();check(FileAccess.get_file_as_string("user://saves/slot1.json")=="New destination bytes","Restart never reimports over destination progress")
    await dispose()
    # A failed copy must expose a retry screen before audio or gameplay starts.
    DirAccess.remove_absolute(folder.path_join(".legacy-data-migration-v1.complete"))
    DirAccess.remove_absolute(folder.path_join("original/fixture.txt"));DirAccess.remove_absolute(folder.path_join("original"))
    write(folder.path_join("original"),"Destination obstacle")
    await start();await create_timer(.15).timeout
    check(main.find_child("DataMigrationError",true,false)!=null,"Partial migration presents recovery guidance")
    check(main.find_child("BackgroundMusic",true,false)==null,"Copy failure does not start ordinary game services")
    check(not FileAccess.file_exists(folder.path_join(".legacy-data-migration-v1.complete")),"Failed copy is not marked complete")
    DirAccess.remove_absolute(folder.path_join("original"))
    for button in main.find_children("*","Button",true,false):
        if button.text==tr("DATA_MIGRATION_RETRY"):button.pressed.emit();break
    await process_frame;await process_frame
    main=current_scene
    check(main!=null and main.find_child("BuildVersion",true,false)!=null,"Retry finishes migration and returns to the title")
    check(FileAccess.get_file_as_string("user://original/fixture.txt")=="Authored import fixture","Retry resumes missing import files")
    check(FileAccess.get_file_as_string("user://saves/slot1.json")=="New destination bytes","Retry preserves conflicting destination save")
    check(FileAccess.get_file_as_string(legacy.path_join("saves/slot1.json"))==JSON.stringify(save),"Retry leaves legacy save intact")
    print("User data migration smoke failures: ",failures);quit(0 if failures==0 else 1)
