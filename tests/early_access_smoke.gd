extends SceneTree

var main: Node
var failures := 0

func check(ok: bool, message: String) -> void:
    print(("PASS " if ok else "FAIL ") + message)
    if not ok:
        failures += 1

func write_text(path: String, text: String) -> void:
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(path.get_base_dir()))
    var file := FileAccess.open(path, FileAccess.WRITE)
    file.store_string(text)
    file.close()

func _initialize() -> void:
    call_deferred("run")

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("early_access_smoke requires disposable XDG_DATA_HOME")
        quit(1)
        return
    var fixture := {"Format":"OpenGDS2","SchemaVersion":1,"State":{
        "Money":100,"Year":1,"Month":4,"Week":1,
        "Company":{"Name":"Recovery fixture","PresidentName":"Tester"}}}
    for locale in ["en", "ja"]:
        write_text("user://saves/slot1.json", "{")
        write_text("user://saves/slot1.json.bak", JSON.stringify(fixture))
        var language := ConfigFile.new()
        language.set_value("language", "locale", locale)
        language.save("user://interface.cfg")
        main = load("res://scenes/Main.tscn").instantiate()
        root.add_child(main)
        await process_frame
        check(main.find_child("BuildVersion",true,false).text==tr("EARLY_ACCESS_LABEL")+" — "+str(ProjectSettings.get_setting("application/config/version")),"Localized EA title and configured version "+locale)
        check(root.title.contains(str(ProjectSettings.get_setting("application/config/version"))),"Application and project versions agree "+locale)
        check(not main.find_child("BuildVersion",true,false).text.contains("Private"),"Title has no private-candidate label "+locale)
        check(main.find_child("EarlyAccessBoundary", true, false).text == tr("EARLY_ACCESS_BOUNDARY"), "Localized campaign boundary " + locale)
        main.call("ShowEarlyAccessScope")
        await process_frame
        var labels = main.find_children("*", "Label", true, false)
        check(labels.any(func(label): return label.text == tr("EARLY_ACCESS_DEFERRED")), "Unavailable mechanics are explained " + locale)
        # Exercise the actual form transition; ShowModeSelection now takes a
        # domain-state argument and is intentionally not a Godot-callable method.
        main.call("ShowCompanySetup")
        await process_frame
        var fields=main.find_children("*","LineEdit",true,false).filter(func(field):return field.is_visible_in_tree())
        check(fields.size()==2,"Company form has both name fields "+locale)
        fields[0].text="Fixture";fields[1].text="Tester"
        for button in main.find_children("*","Button",true,false):
            if button.is_visible_in_tree() and button.text==tr("CONFIRM"):button.pressed.emit();break
        await process_frame
        check(main.find_child("EarlyAccessModeBoundary", true, false) != null, "Boundary before starting a mode " + locale)
        main.call("ShowTitleScreen")
        await process_frame
        var previous = main.find_child("LoadPreviousSave", true, false)
        check(previous != null and not previous.disabled, "Previous-save action remains available with a damaged current slot")
        previous.pressed.emit()
        await process_frame
        check(main.find_child("DesktopFunds", true, false) != null, "Previous valid save restores a playable office")
        check(FileAccess.get_file_as_string("user://saves/slot1.json") == "{", "Recovery does not overwrite the current file")
        main.call("SaveDesktop")
        await process_frame
        check(JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json")).State.Money == 100, "Explicit save writes recovered progress")
        check(JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json.bak")).State.Money == 100, "Saving after corruption retains the valid backup")
        main.queue_free()
        await process_frame
        DirAccess.remove_absolute(ProjectSettings.globalize_path("user://saves/slot1.json.bak"))
        main = load("res://scenes/Main.tscn").instantiate()
        root.add_child(main)
        await process_frame
        check(main.find_child("LoadPreviousSave", true, false).disabled, "Recovery disabled when no previous save exists")
        main.queue_free()
        await process_frame
    print("Early Access scope/recovery checks complete; failures=", failures)
    quit(0 if failures == 0 else 1)
