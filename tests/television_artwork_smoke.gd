extends "res://tests/issue118_smoke.gd"

func television_scene() -> TextureRect:
    Engine.time_scale=0
    await pause_load(project_value())
    await press_node("OpenProject0");await press_node("ProjectPromotion")
    var media=main.find_child("PromotionMedia",true,false)
    media.get_root().get_child(2).select(0);media.item_selected.emit()
    await press_node("ConfirmPromotion")
    return main.find_child("PromotionMeetingPicture",true,false)

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    setup_issue118()
    var original=Image.load_from_file(ProjectSettings.globalize_path("user://original/Graphic/Embedded/Promotion2.bmp"))
    original.convert(Image.FORMAT_RGB8)
    var native=not OS.get_environment("OPENGDS2_PRIVATE_NATIVE_ART").is_empty()
    for locale in ["en","ja"]:
        var settings=ConfigFile.new();settings.set_value("language","locale",locale);settings.save("user://interface.cfg")
        var scene=await television_scene()
        var patched=scene.texture.get_image();patched.convert(Image.FORMAT_RGB8)
        check(patched.get_size()==Vector2i(256,152),"TV keeps native dimensions: "+locale)
        if native:
            if locale=="ja":check(patched.get_data()==original.get_data(),"Japanese TV remains byte-identical")
            else:
                check(patched.get_data()!=original.get_data(),"English TV receives translated lettering")
                var glass=PackedVector2Array([Vector2(84,31),Vector2(178,38),Vector2(174,94),Vector2(87,85)])
                var outside_changes=0
                for y in range(152):
                    for x in range(256):
                        if not Geometry2D.is_point_in_polygon(Vector2(x,y),glass) and patched.get_pixel(x,y)!=original.get_pixel(x,y):outside_changes+=1
                check(outside_changes==0,"Native bezel, stand and room have zero changed pixels")
                var cache="user://localized-artwork/en/television-screen-v2/7be71e872f1572b6ca34f51b713b14725987f6680c4eaef163dbf727a3c9eedf/Promotion2.png"
                check(FileAccess.file_exists(cache),"New TV cache supersedes old oversized patch")
                var cached=Image.load_from_file(ProjectSettings.globalize_path(cache));cached.convert(Image.FORMAT_RGB8)
                check(cached.get_data()==patched.get_data(),"Rendered TV uses the revised cached artwork")
        var output=OS.get_environment("OPENGDS2_PRIVATE_UI_CAPTURE")
        if not output.is_empty():
            DirAccess.make_dir_recursive_absolute(output)
            check(patched.save_png(output.path_join(locale+"-television-artwork.png"))==OK,"Private TV artwork preview")
        await capture_desktop("television")
        await dispose()
    var override_path="user://asset-overrides/Graphic/Embedded/Promotion2.bmp"
    check(not FileAccess.file_exists(override_path),"Test requires an unused TV override path")
    if not FileAccess.file_exists(override_path):
        blank_bmp(override_path,256,152,true)
        var explicit=Image.load_from_file(ProjectSettings.globalize_path(override_path));explicit.convert(Image.FORMAT_RGB8)
        var settings=ConfigFile.new();settings.set_value("language","locale","en");settings.save("user://interface.cfg")
        var scene=await television_scene();var actual=scene.texture.get_image();actual.convert(Image.FORMAT_RGB8)
        check(actual.get_data()==explicit.get_data(),"Explicit TV artwork overrides retain priority")
        await dispose()
        DirAccess.remove_absolute(ProjectSettings.globalize_path(override_path))
    Engine.time_scale=1
    print("Television artwork smoke failures: ",failures);quit(0 if failures==0 else 1)
