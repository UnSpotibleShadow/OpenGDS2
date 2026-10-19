extends "res://tests/casino_poker_smoke.gd"

func write_json(path: String, value: Dictionary) -> void:
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(path.get_base_dir()))
    var file := FileAccess.open(path, FileAccess.WRITE)
    # Preserve C# round-trip doubles in reconstructed saved-race fixtures.
    file.store_string(JSON.stringify(value,"",true,true))

func race_catalog() -> Dictionary:
    var value := {"SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6","Horses":[],"Jockeys":[]}
    for i in range(44):
        value.Horses.append({"SourceIndex":i,"Name":"Synthetic runner %d" % i,"RawWeight":450,"RawKind":3,"RawParameter":13,"Strength":i+0.13})
    for i in range(10):
        value.Jockeys.append({"SourceIndex":i,"Name":"Synthetic rider %d" % i,"RawValue":5})
    return value

func replace_number(text: String, property: String, value: float) -> String:
    var key_pos := text.find('"'+property+'":')
    var begin := text.find(":",key_pos)+1
    var finish := text.find(",",begin)
    return text.substr(0,begin)+str(roundi(value))+text.substr(finish)

func payout_raw(raw: String, award: float) -> String:
    var begin := raw.find("{",raw.find('"Horse":'))
    var depth := 1
    var finish := begin+1
    # Fixture names are synthetic and contain no braces. Keep all source doubles untouched.
    while depth>0:
        if raw[finish]=="{":
            depth+=1
        elif raw[finish]=="}":
            depth-=1
        finish+=1
    var horse := raw.substr(begin,finish-begin)
    for property in ["Stage","ResultTick","DraftCash","TemporaryBalance","DisplayedAward"]:
        var value := 3.0 if property=="Stage" else 100.0 if property=="ResultTick" else award if property=="DisplayedAward" else 9900.0
        horse=replace_number(horse,property,value)
    var arrivals := RegEx.new()
    arrivals.compile('"Arrival"\\s*:\\s*\\[[^\\]]*\\]')
    horse=arrivals.sub(horse,'"Arrival":[0,1,2,3,4,5,6,7]')
    var result := raw.substr(0,begin)+horse+raw.substr(finish)
    result=replace_number(result,"Money",9900)
    return replace_number(result,"OriginalPayrollScratchIndex",17)

func load_raw_fixture(text: String) -> void:
    var file := FileAccess.open("user://saves/slot1.json",FileAccess.WRITE)
    file.store_string(text)
    file.close()
    main=load("res://scenes/Main.tscn").instantiate()
    root.add_child(main)
    await process_frame
    await press("LOAD_GAME")

func run() -> void:
    blank_office_fixture()
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("casino_horse_smoke requires disposable XDG_DATA_HOME")
        quit(1)
        return
    root.size=Vector2i(1280,720)
    for locale in ["en","ja"]:
        write_json("user://original/catalog.json",race_catalog())
        var language := ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Horse locale " + locale)
        var value := casino_fixture()
        value.State.Money=10000
        await load_fixture(value)
        await press_node("TogglePause")
        await press_node("CompanyFacilities")
        await press_node("EnterCasino0")
        check(main.find_child("CasinoHorse",true,false).text==tr("CASINO_HORSE"),"Localized cash-race selector")
        await press_node("CasinoHorse")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Casino.Coins==0 and saved().Casino.Horse.Runners.size()==8,"Cash race opens without coins with eight runners")
        var runner=saved().Casino.Horse.Runners[0]
        var row=main.find_child("HorseRoster",true,false).get_root().get_first_child()
        check(row.get_text(1)==(tr("CATALOG_HORSE_"+str(int(runner.SourceIndex))) if locale=="en" else runner.Name),"Horse names use English display aliases and retain Japanese-mode catalog text")
        check(row.get_text(2)==(tr("CATALOG_JOCKEY_"+str(int(runner.JockeySourceIndex))) if locale=="en" else runner.JockeyName),"Jockey aliases follow imported identity")
        check(runner.Name.begins_with("Synthetic runner"),"Display aliases never rewrite saved runner names")
        var add=main.find_child("HorseAddWager",true,false)
        add.button_down.emit()
        await create_timer(0.13).timeout
        add.button_up.emit()
        await press_node("ToolbarSAVE_GAME")
        var draft: int=saved().Casino.Horse.DraftCash
        check(draft<10000 and saved().Money==10000,"Held timer changes draft without cash commit")
        var raw_snapshot := FileAccess.get_file_as_string("user://saves/slot1.json")
        var award: float=saved().Casino.Horse.SingleStakes[0]*saved().Casino.Horse.WinOdds[0]
        await press_node("ToolbarLOAD_GAME")
        await create_timer(0.15).timeout
        await press_node("ToolbarSAVE_GAME")
        check(saved().Casino.Horse.DraftCash==draft,"Reload does not resume physical held input")
        var ticks=saved().CompanyActivityTicks
        await press_node("HorseStart")
        await create_timer(0.2).timeout
        await press_node("ToolbarSAVE_GAME")
        check(saved().Money==draft and saved().Casino.Horse.Stage==1,"Start commits draft once")
        check(saved().CompanyActivityTicks==ticks,"Race freezes office")
        check(main.find_child("ToolbarSETTINGS",true,false)==null,"No port settings shortcut during race")
        await press_node("CloseHorse")
        await dispose()
        await load_raw_fixture(payout_raw(raw_snapshot,award))
        var phase=main.find_child("HorsePhase",true,false)
        if phase==null:
            print("Payout fixture rejected or routed elsewhere; HorseOpen=",true)
            for label in main.find_children("*","Label",true,false):
                if "Status" in label.name or "status" in label.name:
                    print("Diagnostic ",label.name,": ",label.text)
            check(false,"Payout fixture loaded")
            await dispose()
            await create_timer(1.0).timeout
            quit(1)
            return
        check(phase.text.contains(tr("HORSE_PAYOUT")),"Localized payout state")
        await press_node("ToolbarSAVE_GAME")
        check(saved().OriginalPayrollScratchIndex==17,"Saved race preserves payroll scratch")
        await press_node("ToolbarLOAD_GAME")
        await press_node("HorseAcceptResult")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Money==9900 and saved().Casino.Horse.DraftCash==9900 and saved().Casino.Horse.Stage==0,"Original next-card path discards temporary winnings")
        await press_node("CloseHorse")
        await dispose()
        write_json("user://original/catalog.json",{"SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6"})
        await load_fixture(value)
        await press_node("CompanyFacilities")
        await press_node("EnterCasino0")
        check(main.find_child("CasinoHorse",true,false).disabled,"Older local catalog asks for reimport")
        await dispose()
    await process_frame
    await create_timer(1.0).timeout
    print("Horse smoke failures: ",failures)
    quit(0 if failures==0 else 1)
