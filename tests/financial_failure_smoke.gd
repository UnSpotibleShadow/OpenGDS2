extends "res://tests/mystery_lab_smoke.gd"

func failure_fixture(phase: int) -> Dictionary:
    var value := fixture()
    value.State.Money=-1
    value.State.OriginalPayrollScratchIndex=17
    value.State.FinancialFailure={"Phase":phase,"NextSource":1 if phase>=2 else 0,
        "Slots":[{"SourceIndex":0,"Employed":true,"Name":"Synthetic worker","PortraitId":1}],
        "Reaction":{"SourceIndex":0,"Variant":7} if phase==2 else null}
    return value

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("financial_failure_smoke requires disposable XDG_DATA_HOME")
        quit(1)
        return
    root.size=Vector2i(1280,720)
    write_json("user://original/catalog.json",{"SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6"})
    for locale in ["en","ja"]:
        var language := ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Failure locale " + locale)
        await load_fixture(failure_fixture(0))
        check(main.find_child("ConversationMessage",true,false)!=null,"First dissolution message " + locale)
        await press_node("ToolbarSAVE_GAME")
        check(saved().FinancialFailure.Phase==0 and saved().OriginalPayrollScratchIndex==17,"Pending save preserves introduction and payroll scratch")
        check(main.find_child("ToolbarSETTINGS",true,false).disabled,"Settings blocked during failure")
        check(main.find_child("ToolbarHIRE_EMPLOYEES",true,false).disabled,"Gameplay blocked during failure")
        check(main.find_child("TogglePause",true,false).disabled,"Clock control blocked during failure")
        var ticks=saved().CompanyActivityTicks
        await create_timer(0.16).timeout
        await press_node("ToolbarSAVE_GAME")
        check(saved().CompanyActivityTicks==ticks and saved().Money==-1,"Dissolution freezes simulation without clamping cash")
        await advance_page()
        await press_node("ToolbarSAVE_GAME")
        check(saved().FinancialFailure.Phase==1,"Second introduction saves separately")
        await dispose()
        await load_fixture(failure_fixture(2))
        var body=main.find_child("ConversationMessage",true,false)
        check(body!=null and body.text==tr("FINANCIAL_FAILURE_REACTION_7"),"Saved eighth reaction resumes " + locale)
        var portrait=main.find_child("ConversationPortrait",true,false)
        check(portrait!=null and portrait.visible,"Reaction uses explicit employee portrait")
        await advance_page()
        await press_node("ToolbarSAVE_GAME")
        check(saved().FinancialFailure.Phase==3 and saved().FinancialFailure.NextSource==1,"Closing page resumes without reroll")
        var preserved=FileAccess.get_file_as_string("user://saves/slot1.json")
        await advance_page()
        check(main.find_child("GameDesktop",true,false)==null,"Closure returns to title and removes gameplay controls")
        check(FileAccess.get_file_as_string("user://saves/slot1.json")==preserved,"Closure leaves existing save untouched")
        await press("LOAD_GAME")
        check(main.find_child("ConversationMessage",true,false)!=null,"Earlier pending closing save remains loadable")
        await advance_page()
        await dispose()
        var unknown := failure_fixture(0)
        unknown.State.SourceUpperBound=null
        unknown.State.MaximumSourceIndex=null
        unknown.State.FinancialFailure={"Phase":2,"NextSource":0,"Slots":null,"Reaction":null}
        await load_fixture(unknown)
        check(main.find_child("ConversationMessage",true,false)==null,"Unknown roster does not fabricate farewell")
        var found := false
        for label in main.find_children("*","Label",true,false):
            if label.text==tr("FINANCIAL_FAILURE_ROSTER_UNKNOWN"):
                found=true
        check(found,"Unknown older roster offers explicit recovery message " + locale)
        await dispose()
        var fresh := fixture()
        fresh.State.Money=-1
        await load_fixture(fresh)
        await create_timer(0.2).timeout
        check(main.find_child("ConversationMessage",true,false)!=null,"Negative cash triggers through normal office clock " + locale)
        await dispose()
        fresh=fixture()
        fresh.State.Money=0
        await load_fixture(fresh)
        await create_timer(0.2).timeout
        check(main.find_child("ConversationMessage",true,false)==null,"Exactly zero cash survives office loop " + locale)
        await dispose()
    print("Financial failure smoke failures: ",failures)
    await create_timer(1).timeout
    quit(0 if failures==0 else 1)
