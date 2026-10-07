extends SceneTree

func _initialize() -> void:
    call_deferred("run")

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("beginner_mode_smoke requires disposable XDG_DATA_HOME")
        quit(1)
        return
    var main = load("res://scenes/Main.tscn").instantiate()
    root.add_child(main)
    await process_frame
    var failures = 0
    for locale in ["en", "ja"]:
        TranslationServer.set_locale(locale)
        main.call("ShowModeSelection", "Synthetic company", "Tester")
        await process_frame
        var beginner = null
        var normal = null
        for button in main.find_children("*", "Button", true, false):
            if button.text == tr("BEGINNER_MODE"):
                beginner = button
            if button.text == tr("NORMAL_MODE"):
                normal = button
        var expected = "Year 2, December, Week 4" if locale == "en" else "2年目12月第4週"
        if beginner == null or not beginner.tooltip_text.contains(expected):
            push_error("Beginner mode tooltip must state the verified ending in " + locale)
            failures += 1
        if normal == null or normal.tooltip_text != tr("NORMAL_RULES_VERIFIED"):
            push_error("Normal mode tooltip must state its unlimited calendar in " + locale)
            failures += 1
    main.queue_free()
    await process_frame
    print("Beginner mode UI checks complete; failures=", failures)
    quit(0 if failures == 0 else 1)
