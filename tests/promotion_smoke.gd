extends "res://tests/issue118_smoke.gd"

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    setup_issue118()
    for locale in ["en","ja"]:
        var settings=ConfigFile.new();settings.set_value("language","locale",locale);settings.set_value("display","number_grouping",1);settings.save("user://interface.cfg")
        await promotion_cases()
    print("Promotion smoke failures: ",failures);quit(0 if failures==0 else 1)
