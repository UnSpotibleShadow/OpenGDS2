extends SceneTree

# Optional private archive integration. No source assets or text live in this test.
var failures=0
func check(ok: bool,message: String) -> void:
    print(("PASS " if ok else "FAIL ")+message)
    if not ok:failures+=1
func _initialize() -> void:call_deferred("run")
func run() -> void:
    var archive=OS.get_environment("OPENGDS2_PRIVATE_ARCHIVE")
    if archive.is_empty() or OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    var main=load("res://scenes/Main.tscn").instantiate();root.add_child(main);await process_frame
    for button in main.find_children("*","Button",true,false):
        if button.text==tr("IMPORT_BUTTON"):button.pressed.emit();break
    await process_frame
    main.find_children("*","FileDialog",false,false)[0].file_selected.emit(archive)
    await process_frame
    check(FileAccess.file_exists("user://original/.imported"),"Supported original archive imported atomically")
    for filename in ["Graphic/KeibaC.bmp","Graphic/KeibaM.bmp","Sound/Bakuhatu.wav","Sound/FIRE.wav","Sound/GOLD.WAV","Sound/Open.wav","Sound/Up.wav","Graphic/Embedded/PoachingRival.bmp","Graphic/Embedded/PoachingPresident.bmp","Graphic/Embedded/Promotion1.bmp","Graphic/Embedded/Promotion2.bmp","Graphic/Embedded/PublisherMeeting.bmp","Graphic/Embedded/BusinessMeeting.png"]:
        check(FileAccess.file_exists("user://original/"+filename),"Imported original resource: "+filename)
    var dialogue=JSON.parse_string(FileAccess.get_file_as_string("user://original/dialogue.json"))
    check(dialogue.SchemaVersion==17 and dialogue.Entries.size()==334,"All current native dialogue entries imported")
    for key in ["BACKUP_DEVICE_EXPLANATION","VIRUS_INTRO","POACHING_RIVAL_MESSAGE_3","POACHING_RESULT_WON_OFFENSIVE","INTERVIEW_INTRO","INTERVIEW_PAGE_3"]:
        check(dialogue.Entries.has(key) and not dialogue.Entries[key].is_empty(),"Source-bound dialogue key: "+key)
    main.queue_free();await process_frame
    print("Original archive smoke failures: ",failures);quit(0 if failures==0 else 1)
