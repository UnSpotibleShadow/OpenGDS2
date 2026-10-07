extends SceneTree

func _initialize() -> void:
    var args := OS.get_cmdline_user_args()
    if args.size() != 1:
        push_error("Supply the absolute destination for project.binary")
        quit(1)
        return
    # The packager also preserves _custom_features from its trusted export.
    # save_custom exposes no custom export-feature argument in Godot 4.4.
    ProjectSettings.set_setting("dotnet/project/assembly_name", "OpenGDS2")
    var result := ProjectSettings.save_custom(args[0])
    if result != OK:
        push_error("Cannot write candidate project configuration: " + str(result))
    quit(0 if result == OK else 1)
