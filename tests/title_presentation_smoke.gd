extends SceneTree

var main: Node
var failures := 0

func check(ok: bool, message: String) -> void:
    print(("PASS " if ok else "FAIL ") + message)
    if not ok:
        failures += 1

func settle() -> void:
    for frame in range(5):
        await process_frame

func menu() -> void:
    main.call("ShowTitleScreen")
    await settle()

func _initialize() -> void:
    call_deferred("run")

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("Use a disposable XDG_DATA_HOME")
        quit(1)
        return
    root.size = Vector2i(1280,720)
    TranslationServer.set_locale("en")
    var config := ConfigFile.new()
    config.set_value("language","locale","en")
    config.save("user://interface.cfg")
    main = load("res://scenes/Main.tscn").instantiate()
    root.add_child(main)
    await settle()
    check(main.find_child("TitleArtwork",true,false) == null,"Fresh title works without imported artwork")
    var source := Image.load_from_file(OS.get_environment("OPENGDS2_PRIVATE_TITLE"))
    check(source != null,"Local title fixture available")
    if source == null:
        quit(1)
        return
    source.convert(Image.FORMAT_RGB8)
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("user://original/Graphic"))
    var file := FileAccess.open("user://original/Graphic/Title.bmp",FileAccess.WRITE)
    file.store_buffer(FileAccess.get_file_as_bytes(OS.get_environment("OPENGDS2_PRIVATE_TITLE")))
    file.close()
    var original := source.get_data()
    var captures := OS.get_environment("OPENGDS2_TITLE_CAPTURE")
    for locale in ["en","ja"]:
        TranslationServer.set_locale(locale)
        await menu()
        var artwork = main.find_child("TitleArtwork",true,false)
        var localized: Image = artwork.texture.get_image()
        localized.convert(Image.FORMAT_RGB8)
        var pixels: PackedByteArray = localized.get_data()
        check(artwork.texture.get_size() == Vector2(558,385),"Title dimensions preserved " + locale)
        check(pixels != original if locale == "en" else pixels == original,"English logo patch / unchanged Japanese " + locale)
        check(pixels.slice(558*165*3) == original.slice(558*165*3),"City and mascot pixels unchanged " + locale)
        for row in range(161,165):
            var first := (row*558+269)*3
            check(pixels.slice(first,first+22*3) == original.slice(first,first+22*3),"Tower cap preserved " + locale + " " + str(row))
        var buttons = main.find_children("*","Button",true,false).filter(func(b): return b.is_visible_in_tree())
        check(buttons.size() == 10,"All ten title actions retained " + locale)
        for button in buttons:
            check(button.size.y <= 30 and button.size.x <= 310,"Compact action " + str(button.name) + " " + locale)
            check(Rect2(Vector2.ZERO,Vector2(root.size)).encloses(button.get_global_rect()),"Action fits 720p " + str(button.name) + " " + locale)
        var boundary = main.find_child("EarlyAccessBoundary",true,false)
        check(Rect2(Vector2.ZERO,Vector2(root.size)).encloses(boundary.get_global_rect()),"Campaign note fits 720p " + locale)
        check(main.find_child("LoadPreviousSave",true,false).disabled,"Missing backup remains disabled " + locale)
        if not captures.is_empty():
            await RenderingServer.frame_post_draw
            root.get_texture().get_image().save_png(captures + "/title-" + locale + "-720.png")
        main.call("ShowSettings")
        await settle()
        await menu()
        check(main.find_child("TitleSecondaryActions",true,false) != null,"Return from settings keeps compact menu " + locale)
    TranslationServer.set_locale("en")
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("user://asset-overrides/Graphic"))
    source.set_pixel(557,384,Color.MAGENTA)
    source.save_png("user://asset-overrides/Graphic/Title.png")
    await menu()
    check(main.find_child("TitleArtwork",true,false).texture.get_image().get_data() == source.get_data(),"Custom title override has priority over automatic patch")
    DirAccess.remove_absolute(ProjectSettings.globalize_path("user://asset-overrides/Graphic/Title.png"))
    root.size = Vector2i(1280,1016)
    await menu()
    check(main.find_child("TitlePrimaryActions",true,false).size.x <= 600,"Tall window keeps menu compact")
    if not captures.is_empty():
        await RenderingServer.frame_post_draw
        root.get_texture().get_image().save_png(captures + "/title-en-tall.png")
        main.find_child("TitleArtwork",true,false).texture.get_image().save_png(captures + "/title-patched.png")
    print("Title presentation checks complete; failures=",failures)
    quit(0 if failures == 0 else 1)
