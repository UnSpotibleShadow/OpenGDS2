extends "res://tests/facility_management_smoke.gd"

func settle() -> void:
    for frame in range(8):
        await process_frame

func metric(key: String) -> Label:
    return main.find_child("CompanyMetric"+key,true,false)

func load_summary(value: Dictionary) -> void:
    write_json("user://saves/slot1.json",value)
    main.call("LoadSession")
    await process_frame
    main.call("SetSimulationPaused",true)
    await settle()

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,720)
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Summary locale " + locale)
        main=load("res://scenes/Main.tscn").instantiate();root.add_child(main)
        await process_frame
        var value=fixture(7)
        value.State.Company.Name="Review Company"
        value.State.Company.Fame=12
        value.State.Company.OriginalMember90=23
        value.State.Company.HallOfFameCount=0
        value.State.Company.WeeklyNet=-12345
        value.State.Company.MonthlyNet=20000
        value.State.Company.YearlyNet=987654321
        value.State.Calendar.OriginalSlots0444=[-1,-1,-1]
        value.State.ReleasedGames=[]
        await load_summary(value)
        var window=main.find_child("CompanyWindow",true,false)
        check(window != null,"Company summary loads " + locale)
        var names=["Identity","Capacity","Games","Profit","Tenants"]
        var colors=[Color("c0c0ff"),Color("ffe0c0"),Color("ffffc0"),Color("c0ffc0"),Color("c0ffff")]
        var prior=-1.0
        for i in range(names.size()):
            var section=main.find_child("CompanySection"+names[i],true,false)
            check(section.get_theme_stylebox("panel").bg_color.is_equal_approx(colors[i]),"Original section color " + names[i])
            check(section.global_position.y > prior,"Original section order " + names[i]);prior=section.global_position.y
            check(window.get_global_rect().encloses(section.get_global_rect()),"Section stays inside Company window " + names[i] + " " + locale)
        check(metric("COMPANY_RANK")==null,"Off-window rank excluded")
        check(metric("DATE")==null and metric("STAFF")==null,"Original summary contents")
        check(metric("GENRE_RESEARCH").text=="23%","Library percentage")
        check(metric("PRODUCTION_CAPACITY").text==tr("COMPANY_SUMMARY_CAPACITY").format(["50","5,000"],"{_}"),"Capacity includes copy equivalent")
        check(metric("HIGHEST_SALES").text=="0" and metric("HIGHEST_SALES_TITLE").text=="—","No releases has no invented record")
        for slot in range(1,4):check(metric("TENANT_"+str(slot)).text==tr("COMPANY_SUMMARY_NO_TENANT"),"Empty tenant " + str(slot))
        check(metric("WEEKLY_NET").text.contains("-12,345"),"Signed weekly profit")
        check(metric("MONTHLY_NET").text.contains("20,000"),"Monthly profit")
        check(metric("YEARLY_NET").text.contains("987,654,321"),"Yearly profit")
        var first=value.State.ReleasedGames
        first.append({"ProductIndex":0,"Title":"Record title","Stock":7,"UnitsSold":12000,"Price":1000,"ReviewPending":false,"Demand":0})
        first.append({"ProductIndex":1,"Title":"Second release","Stock":7,"UnitsSold":9000,"Price":1000,"ReviewPending":false,"Demand":0})
        value.State.Company.HallOfFameCount=1
        value.State.Calendar.OriginalSlots0444=[7,-1,5]
        await load_summary(value)
        check(metric("RELEASED_COUNT").text=="2" and metric("HALL_OF_FAME_COUNT").text=="1","Game counts use saved company totals")
        check(metric("HIGHEST_SALES_TITLE").text=="Record title" and metric("HIGHEST_SALES").text=="12,000","Highest sales uses units, not revenue")
        check(metric("TENANT_1").text==tr("FACILITY_NAME_ProductionLab") and metric("TENANT_3").text==tr("FACILITY_NAME_Lawyer"),"Occupied tenant names")
        main.find_child("CompanyWindow",true,false).move_to_front()
        await settle()
        var captures=OS.get_environment("OPENGDS2_SUMMARY_CAPTURE")
        if not captures.is_empty():
            await RenderingServer.frame_post_draw
            root.get_texture().get_image().save_png(captures+"/company-"+locale+".png")
        value.State.ReleasedGames[1].UnitsSold=20000
        await load_summary(value)
        check(metric("HIGHEST_SALES_TITLE").text=="Second release" and metric("HIGHEST_SALES").text=="20,000","Record refreshes when a different game leads")
        value.State.Company.ProductionCapacity=null
        value.State.Company.OriginalMember90=null
        value.State.Company.WeeklyNet=null
        value.State.Company.HallOfFameCount=null
        await load_summary(value)
        for key in ["PRODUCTION_CAPACITY","GENRE_RESEARCH","WEEKLY_NET","HALL_OF_FAME_COUNT"]:
            check(metric(key).text=="?","Unknown legacy value preserved " + key)
        main.queue_free();await process_frame;await process_frame
    print("Company summary checks complete; failures=",failures)
    quit(0 if failures==0 else 1)
