extends "res://tests/office_presentation_smoke.gd"

# Community-created fixtures; checks presentation without introducing outcomes.
func panel_catalog() -> void:
    write_json("user://original/catalog.json",{"SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6",
        "Hardware":[{"SourceIndex":0,"Name":"Synthetic console","BitWidth":16,"RawMembers":{"10A4":0,"10C0":0},
            "ProductionBands":[{"SizeThreshold":100,"UnitCost":300},{"SizeThreshold":200,"UnitCost":300},{"SizeThreshold":300,"UnitCost":300},{"SizeThreshold":400,"UnitCost":300}]}],
        "Audiences":[{"SourceIndex":0,"Name":"Synthetic audience"}],
        "GameTypes":[{"SourceIndex":0,"Name":"Synthetic type"}],"Genres":[{"SourceIndex":0,"Name":"Synthetic genre"}]})
    blank_bmp("user://original/Graphic/Embedded/Hardware0.bmp",64,64,false)
    blank_bmp("user://original/Graphic/Embedded/PlanningMeeting.bmp",256,160,false)

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("project_panels_smoke requires disposable data")
        quit(1)
        return
    root.size=Vector2i(1280,720)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    panel_catalog()
    for locale in ["en","ja"]:
        var language := ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Panel locale " + locale)
        var value := stress_fixture()
        value.State.Money=10000000
        value.State.SalesFee1=1500
        value.State.SalesFee2=1500
        value.State.Company.ProductionCapacity=100
        var p: Dictionary=value.State.DevelopmentProjects[0]
        p.PendingDecision=2
        p.DecisionIntroductionAcknowledged=2
        p.Recognition=81
        p.Bugs=22
        await load_fixture(value)
        check(main.find_child("CompletionIllustration",true,false).texture != null,"Completion loads the meeting illustration")
        var summary = main.find_child("CompletionMetadataCaption0",true,false)
        check(summary.text.contains(tr("TARGET_HARDWARE")) and not summary.text.contains("TARGET_HARDWARE"),"Completion metadata captions are localized")
        check(main.find_child("ProjectBugCount",true,false).text == "22" and main.find_child("ProjectRecognition",true,false).text.contains("81%"),"Completion shows saved bug count and recognition")
        await press("RELEASE_GAME")
        check(main.find_child("ShipmentHardwarePicture",true,false).texture != null,"Shipment has its actual hardware picture")
        check(main.find_child("ShipmentNetCopy",true,false).text == "¥2,800","Shipment keeps native distribution receipts")
        main.find_child("ReleaseCopies",true,false).value=1000
        main.find_child("ReleasePrice",true,false).value=6400
        check(main.find_child("ShipmentTotalCost",true,false).text == "¥500,000" and main.find_child("ShipmentAllSold",true,false).text == "¥3,400,000","Shipment previews recalculate independent tables")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Money == 10000000 and saved().DevelopmentProjects.size() == 1,"Editing shipment does not charge or release")
        await press("CANCEL")
        check(main.find_child("CompletionWindow",true,false) != null,"Shipment cancellation returns to the completion checkpoint")
        await press_node("ToolbarLOAD_GAME")
        check(main.find_child("ProjectBugCount",true,false).text == "22","Reload preserves completion results")
        await dispose()
        var renewal := stress_fixture()
        renewal.State.DevelopmentProjects=[]
        renewal.State.Year=3
        renewal.State.Month=3
        renewal.State.Week=4
        renewal.State.Calendar.Pending={"ClosingDate":{"Year":3,"Month":3,"Week":4},"Phase":1,"Index":0,"PendingEvent":0}
        renewal.State.Employees[0].ContractFee=10
        renewal.State.Employees[1].ContractFee=20
        await load_fixture(renewal)
        check(main.find_child("RenewalSelectedTotal",true,false).text.ends_with("¥30"),"Renewal sums all initially selected fees")
        await press_node("SelectRenewalEmployee1")
        check(main.find_child("RenewalSelectedName",true,false).text == "Synthetic selected" and main.find_child("RenewalAbilityChart",true,false)!=null,"Renewal details follow the chosen employee")
        main.find_child("RenewEmployee1",true,false).button_pressed=false
        check(main.find_child("RenewalSelectedTotal",true,false).text.ends_with("¥10"),"Renewal total follows checked employees")
        await dispose()
    print("Project panels smoke failures: ",failures)
    quit(0 if failures==0 else 1)
