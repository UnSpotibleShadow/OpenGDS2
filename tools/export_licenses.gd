extends SceneTree
# Generate notices from the actual Godot binary used for the desktop export.
func _initialize() -> void:
    var args := OS.get_cmdline_user_args()
    if args.size() != 1:
        push_error("Usage: --script tools/export_licenses.gd -- OUTPUT_DIRECTORY")
        quit(1)
        return
    var output := args[0]
    if DirAccess.make_dir_recursive_absolute(output) != OK:
        push_error("Cannot create notice directory")
        quit(1)
        return
    var engine_file := FileAccess.open(output.path_join("GODOT_LICENSE.txt"),FileAccess.WRITE)
    var third_party := FileAccess.open(output.path_join("GODOT_THIRD_PARTY.txt"),FileAccess.WRITE)
    if engine_file == null or third_party == null:
        push_error("Cannot write engine notices")
        quit(1)
        return
    engine_file.store_string(Engine.get_license_text()+"\n")
    third_party.store_string("Godot Engine "+Engine.get_version_info().string+"\n\n")
    for component in Engine.get_copyright_info():
        third_party.store_string(component.name+"\n")
        for part in component.parts:
            third_party.store_string("Files: "+", ".join(part.files)+"\n")
            third_party.store_string("\n".join(part.copyright)+"\n")
            third_party.store_string("License: "+part.license+"\n\n")
    third_party.store_string("Full license texts\n\n")
    var licenses := Engine.get_license_info()
    for name in licenses:
        third_party.store_string(str(name)+"\n"+str(licenses[name])+"\n\n")
    print("Godot license notices written")
    quit(0)
