extends "res://tests/stress_relief_smoke.gd"

func checkpoint_fixture() -> Dictionary:
    var value := stress_fixture()
    var project: Dictionary=value.State.DevelopmentProjects[0]
    project.OriginalStage=1
    project.RequiredPoints=100
    project.Progress=81
    project.PendingDecision=1
    project.DecisionIntroductionAcknowledged=1
    project.GenreSourceIndex=null
    return value

func check_speaker_page() -> void:
    await press_node("ToolbarSAVE_GAME")
    if not saved().DevelopmentProjects[0].has("CheckpointMeeting") or saved().DevelopmentProjects[0].CheckpointMeeting==null:
        check(false,"Checkpoint meeting was saved")
        for label in main.find_children("*","Label",true,false):
            if label.visible and (label.text.contains("failed") or label.text.contains("Invalid") or label.name.contains("Status")):
                print("Fixture diagnostic: ",label.name," ",label.text)
        quit(1)
        return
    var meeting: Dictionary=saved().DevelopmentProjects[0].CheckpointMeeting
    var line: Dictionary=meeting.Lines[meeting.MessageIndex]
    check(int(line.EmployeeSourceIndex) in [0,1] and line.EmployeePortraitId==1,"Saved source identity and portrait")
    var person: String="Synthetic first slot" if line.EmployeeSourceIndex==0 else "Synthetic selected"
    check(main.find_child("ConversationSpeaker",true,false).text==tr("CHECKPOINT_EMPLOYEE_NAMED").format([person],"{_}"),"Localized selected employee speaker")
    check(main.find_child("ConversationPortrait",true,false).visible,"Selected employee portrait pane is visible")
    check(saved().OriginalPayrollScratchIndex==2,"Dialogue entry rebuilds shared payroll scratch")

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("checkpoint_speaker_smoke requires disposable data")
        quit(1)
        return
    write_json("user://original/catalog.json",{"SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6",
        "Audiences":[{"SourceIndex":0,"Name":"Synthetic audience","RawMembers":{"14A8":0}}],
        "GameTypes":[{"SourceIndex":0,"Name":"Synthetic type"}]})
    for locale in ["en","ja"]:
        var language := ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Checkpoint speaker locale " + locale)
        await load_fixture(checkpoint_fixture())
        await check_speaker_page()
        var first: Dictionary=saved().DevelopmentProjects[0].CheckpointMeeting.Lines[0].duplicate(true)
        await press_node("ToolbarLOAD_GAME")
        await check_speaker_page()
        check(saved().DevelopmentProjects[0].CheckpointMeeting.Lines[0]==first,"Loading preserves the selected speaker")
        await advance_page()
        await check_speaker_page()
        check(saved().DevelopmentProjects[0].CheckpointMeeting.MessageIndex==1,"Acknowledgement advances the saved speaker page")
        await dispose()
    await process_frame
    await create_timer(1.0).timeout
    print("Checkpoint speaker smoke failures: ",failures)
    quit(0 if failures==0 else 1)
