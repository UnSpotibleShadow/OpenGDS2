extends SceneTree

# Run with an exported .NET engine and its actual PCK, not the editor.
# This catches missing runtime features and editor-only managed initialization.
func _initialize() -> void:
    call_deferred("run")

func require(condition: bool, message: String) -> bool:
    if not condition:
        push_error("FAIL EXPORT STARTUP: " + message)
        quit(1)
    return condition

func run() -> void:
    if not require(not OS.has_feature("editor"), "Use the exported engine"):
        return
    if not require(OS.has_feature("dotnet"), "Export metadata enables the managed runtime"):
        return
    if not require(ProjectSettings.get_setting("dotnet/project/assembly_name", "") == "OpenGDS2", "Assembly identity is preserved"):
        return
    var scene = load("res://scenes/Main.tscn")
    if not require(scene is PackedScene, "Main scene loads"):
        return
    var main = scene.instantiate()
    root.add_child(main)
    await process_frame
    await process_frame
    var version = ProjectSettings.get_setting("application/config/version", "")
    if not require(root.title == "Open Game Dev Story II", "Managed Main._Ready applies the friendly game title"):
        return
    if not require(main.find_child("BuildVersion", true, false) != null, "Managed title interface was created"):
        return
    if not require(main.find_child("BuildVersion",true,false).text.contains(str(version)), "Exported version remains visible in-game"):
        return
    TranslationServer.set_locale("en")
    if not require(tr("CHECKPOINT_FEE") == "Fee" and tr("CHECKPOINT_EXAMPLES") == "Example titles", "Current translations load in the export"):
        return
    for phase in ["planning", "alpha", "beta", "final"]:
        var stream = load("res://audio/community/meetings/" + phase + ".ogg")
        if not require(stream is AudioStreamOggVorbis and stream.get_length() > 0, "Exported meeting stream loads: " + phase):
            return
    main.queue_free()
    await process_frame
    print("Export startup checks complete; managed UI, metadata, translations and meeting resources passed")
    quit(0)
